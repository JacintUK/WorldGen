﻿/*
 * Copyright 2018 David Ian Steele
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;
using static WorldGen.Geometry;

namespace WorldGen
{
    class Plates
    {
        /// <summary>
        /// Translation table to lookup plate index from Geometry vertexIndex.
        /// </summary>
        public int[] VertexToPlates { get; private set; }

        private readonly Plate[] plates;

        public Plate[] GetPlates()
        {
            return plates;
        }
        public Plate GetPlate(int plateIndex)
        {
            return plates[plateIndex];
        }

        public struct Border
        {
            public int plate0;
            public int plate1;
            public int c1Index;
            public int c2Index;

            // c1 and c2 are indexes into the geometry's centroid array, and 
            // represent the corners of the border.
            public Border(int plate0, int plate1, int c1Index, int c2Index)
            {
                this.plate0 = plate0;
                this.plate1 = plate1;
                this.c1Index = c1Index;
                this.c2Index = c2Index;
            }

            public int OppositeCorner(int corner)
            {
                if (c1Index == corner)
                    return c2Index;
                else
                    return c1Index;
            }
        }

        public struct Stress
        {
            public float pressure;
            public float shear;
            public Stress(float p, float s)
            {
                pressure = p;
                shear = s;
            }
        }

        public class BorderCorner
        {
            public List<Int64> borderIndices = new();
            public Stress stress = new();
            public float elevation = 0;
            public ElevationCalculation calculation = ElevationCalculation.DORMANT;
        }
        Dictionary<int, BorderCorner> borderCorners;

        /// <summary>
        /// borders map to edges between tiles 
        /// </summary>
        private readonly Dictionary<Int64, Border> borders;

        Random rand;
        public readonly IGeometry geometry;

        public Plates(ref Random rand, int numPlates, IGeometry geometry)
        {
            this.plates = new Plate[numPlates];
            this.borders = new Dictionary<long, Border>();
            this.rand = rand;
            this.geometry = geometry;
            CreatePlates();
        }

        /// <summary>
        /// Debug method
        /// </summary>
        public void GrowPlates()
        {
            for (int i = 0; i < plates.Length; ++i)
            {
                plates[i].Grow(1);
            }
        }

        private void CreatePlates()
        {
            InitializePlates();
            GrowAllPlates();
            GenerateCornerPlateRelationships();
            GenerateInitialPlateElevations();
        }

        /// <summary>
        /// Initialize plates.
        /// Randomly choose a vertex on the icosphere as the center of the plate.
        /// For each plate, choose a random pivot that's in the same hemisphere as the mesh,
        /// and set pivot/center rotations to +/- 3 degrees.
        /// </summary>
        private void InitializePlates()
        {
            // Initialize vertexToPlate array to a value that isn't a valid plate index
            VertexToPlates = new int[geometry.Mesh.Length];
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                VertexToPlates[i] = -1;
            borders.Clear();

            for (int plateIndex = 0; plateIndex < plates.Length; ++plateIndex)
            {
                int plate = -1;
                do
                {
                    int vertexIndex = rand.Next(geometry.Mesh.Length);
                    // prevent 2 plates spawning at the same vertex.
                    plate = VertexToPlates[vertexIndex];
                    if (plate == -1)
                    {
                        var traits = new PlatePhysicsTraits();
                        traits.Pivot = new Vector3(rand.Next(100) - 50, rand.Next(100) - 50, rand.Next(100) - 50);
                        traits.Pivot.Normalize();
                        traits.Center = geometry.Mesh.GetPosition(vertexIndex);
                        if (Vector3.Dot(traits.Center, traits.Pivot) < 0)
                        {
                            traits.Pivot *= -1.0f; // Ensure pivot is at least in the same hemisphere as the plate center.
                        }
                        traits.CenterRotation = (rand.Next(200) - 100) * (float)Math.PI / 6000.0f; // -3 -> 3 degrees
                        traits.PivotRotation = (rand.Next(200) - 100) * (float)Math.PI / 6000.0f;
                        traits.Elevation = 0.0f;

                        plates[plateIndex] = new Plate(VertexToPlates, geometry, traits, plateIndex, vertexIndex, ref rand);
                        break;
                    }
                } while (plate != -1);
            }
        }

        /// <summary>
        /// Perform a flood fill for each plate, flooding by 1 face each until all verts are covered.
        /// </summary>
        /// <exception cref="Exception">Remove this check.</exception>
        private void GrowAllPlates()
        {
            int total = plates.Length;
            while (total < geometry.Mesh.Length)
            {
                for (int i = 0; i < plates.Length; ++i)
                {
                    total += plates[i].Grow(1); // todo: Consider interleaving growth loops.
                }
            }

            // Double check we have all tiles covered
            for (int i = 0; i < VertexToPlates.Length; ++i)
            {
                if (VertexToPlates[i] == -1)
                {
                    throw new Exception();
                }
            }
        }

        /// <summary>
        /// Each centroid is a corner between tiles of a plate or plates.
        /// 
        /// </summary>
        private void GenerateCornerPlateRelationships()
        {
            // Create corner / plate relationships
            int centroidIndex = 0;
            foreach (var centroid in geometry.Topology.Centroids)
            {
                foreach (int faceIndex in centroid.Faces)
                {
                    int plateIndex = VertexToPlates[faceIndex];
                    float plateDistance = 0;
                    // ensure we only add corner<->plate once.
                    if (!centroid.PlateDistances.TryGetValue(plateIndex, out plateDistance))
                    {
                        // Great circle distance to plate center:
                        Vector3 center = plates[plateIndex].Traits.Center;
                        // Both centroid.pos and plate center are unit length; so dot product = cos theta.
                        // unit sphere, so great cicle distance is theta
                        float distance = (float)Math.Acos(Vector3.Dot(center, centroid.position));
                        centroid.PlateDistances.Add(plateIndex, distance);
                        plates[plateIndex].Corners.Add(centroidIndex);
                    }
                }
                ++centroidIndex;
            }

            // Sort corners in plates by distance from center of plate.
            {
                for (int plateIndex = 0; plateIndex < plates.Length; ++plateIndex)
                {
                    plates[plateIndex].Corners.Sort(delegate (int a, int b)
                    {
                        float diff = geometry.Topology.Centroids[a].PlateDistances[plateIndex] -
                                        geometry.Topology.Centroids[b].PlateDistances[plateIndex];
                        if (diff < 0)
                            return -1;
                        else if (diff > 0)
                            return 1;
                        return 0;
                    });
                }
            }
        }

        /// <summary>
        /// Generate initial plate elevations. Units are 10km. (Different to mesh units)
        /// </summary>
        private void GenerateInitialPlateElevations()
        {
            // Determine elevations of plates
            for (int i = 0; i < plates.Length; ++i)
            {
                // Earth: crust is 5-70km thick, radius of planet is 6,371km, i.e. 0.1% -> 1.0%
                // deepest ocean is ~8km; tallest mountain is ~9km; i.e. +/- 10km

                bool oceanic = rand.Next(100) < 65; // percentage of world covered by water
                if (oceanic)
                {
                    // Oceanic plates range from -8km to -3km
                    float height = rand.Next(100) / 200.0f;
                    plates[i].Traits.Elevation = height - 0.8f;
                    plates[i].Traits.Thickness = 0.5f + height;
                }
                else
                {
                    // Continental plates range from 1km to 5km above sea level; from 20-70km thick
                    plates[i].Traits.Elevation = rand.Next(400) / 1000.0f + 0.1f;
                    plates[i].Traits.Thickness = 2.0f + (plates[i].Traits.Elevation - 0.1f) * 12.5f;
                }
            }
        }

        /// <summary>
        /// Generates borders and borderCorners attributes
        /// </summary>
        /// <param name="recolor"></param>
        /// <exception cref="Exception"></exception>
        public void CalculatePlateBoundaries(bool recolor)
        {
            borders.Clear();
            borderCorners = new Dictionary<int, BorderCorner>();

            // Recalculate border tiles for each plate
            int totalBorderTiles = 0;
            for (int plateIdx = 0; plateIdx < plates.Length; ++plateIdx)
            {
                plates[plateIdx].CalculateBorderTiles(recolor);
                totalBorderTiles += plates[plateIdx].BorderTiles.Count;
            }

            // For each plate, check each border tile's neighbours
            //   If a neighbour belongs to a different plate, create a border edge.
            for (int plateIdx = 0; plateIdx < plates.Length; ++plateIdx)
            {
                List<int> borderTiles = plates[plateIdx].BorderTiles;
                foreach(var borderTile in borderTiles)
                {
                    // borderTile is a Vertex index in the world mesh.
                    var outerNeighbours = geometry.Topology.VertexNeighbours.GetNeighbours(borderTile);
                    for (int neighbourIdx = 0; neighbourIdx < outerNeighbours.Count; ++neighbourIdx)
                    {
                        // neighbour vertexIdx is the vertex index in this world mesh of this tile's neighbour.
                        int neighbourVertexIdx = outerNeighbours.Neighbours[neighbourIdx];
                        int neighbourPlateIdx = VertexToPlates[neighbourVertexIdx];
                        if (neighbourPlateIdx != plateIdx)
                        {
                            // It's not in the plate, so must be a bordering plate.
                            // Index by key from verts, stores plate ids, corners
                            Int64 borderKey = Geometry.Topology.CreateEdgeKey((uint)neighbourVertexIdx, (uint)borderTile);

                            if (!borders.ContainsKey(borderKey))
                            {
                                int c1Index;
                                int c2Index;
                                geometry.Topology.GetCorners(borderTile, neighbourVertexIdx, out c1Index, out c2Index);
                                borders.Add(borderKey, new Border(plateIdx, neighbourPlateIdx, c1Index, c2Index));
                                AddBorderCorner(c1Index, borderKey);
                                AddBorderCorner(c2Index, borderKey);
                            }
                        }
                    }
                }
            }

            if (borders.Count < totalBorderTiles / 2.0f)
            {
                throw new Exception();
            }
        }

        private void AddBorderCorner(int cornerIndex, Int64 borderKey)
        {
            BorderCorner bc;
            if (!borderCorners.TryGetValue(cornerIndex, out bc))
            {
                borderCorners[cornerIndex] = new BorderCorner();
            }
            borderCorners[cornerIndex].borderIndices.Add(borderKey);
        }

        /// <summary>
        /// Compute stresses at each border and update "borderCorners" attribute
        /// </summary>
        public void CalculateStresses()
        {
            // for each vertex in plate boundaries,
            //   calculate relative motion between tiles
            //     both parallel to (shear) and perpendicular to (pressure) edge.

            foreach (int borderCornerKey in borderCorners.Keys)
            {
                List<Int64> borderIndices = borderCorners[borderCornerKey].borderIndices;
                var centroid = geometry.Topology.Centroids[borderCornerKey];
                var pos = centroid.position;

                Dictionary<int, Vector3> plateMovement = new Dictionary<int, Vector3>();
                foreach (Int64 borderKey in borderIndices)
                {
                    Border border = borders[borderKey];

                    // Calculate movement only once for each plate 
                    for (int i = 0; i < 2; ++i)
                    {
                        int plateIndex = (i == 0) ? border.plate0 : border.plate1;
                        Vector3 movement;
                        if (!plateMovement.TryGetValue(plateIndex, out movement))
                        {
                            Plate plate = plates[plateIndex];
                            movement = plate.CalculateSpin(pos) + plate.CalculateDrift(pos);
                            plateMovement[plateIndex] = movement;
                        }
                    }
                }

                if (borderIndices.Count == 3)
                {
                    // 3 separate plates. Find movement from each plate at this corner and average it
                    Stress[] stresses = new Stress[3];
                    int stressIdx = 0;
                    foreach (Int64 borderKey in borderIndices)
                    {
                        Border border = borders[borderKey];
                        int oppositeCornerIndex = border.OppositeCorner(borderCornerKey);
                        var oppositeCornerPosition = geometry.Topology.Centroids[oppositeCornerIndex].position;
                        Vector3 boundary = oppositeCornerPosition - pos;
                        Vector3 boundaryNormal = Vector3.Cross(boundary, pos);
                        stresses[stressIdx++] = calculateStress(plateMovement[border.plate0], plateMovement[border.plate1], boundary, boundaryNormal);
                    }
                    borderCorners[borderCornerKey].stress.pressure = (stresses[0].pressure + stresses[1].pressure + stresses[2].pressure) / 3.0f;
                    borderCorners[borderCornerKey].stress.shear = (stresses[0].shear + stresses[1].shear + stresses[2].shear) / 3.0f;
                }
                else // Border between only 2 plates.
                {
                    // generate average vector, calculate stress once.
                    Border border0 = borders[borderIndices[0]];
                    Border border1 = borders[borderIndices[1]];
                    int plate0 = border0.plate0;
                    int plate1 = border1.plate0 == plate0 ? border1.plate1 : border0.plate1;
                    int oppositeCornerIndex0 = border0.OppositeCorner(borderCornerKey);
                    int oppositeCornerIndex1 = border1.OppositeCorner(borderCornerKey);
                    var oppositeCornerPosition0 = geometry.Topology.Centroids[oppositeCornerIndex0].position;
                    var oppositeCornerPosition1 = geometry.Topology.Centroids[oppositeCornerIndex1].position;
                    Vector3 boundary = oppositeCornerPosition1 - oppositeCornerPosition0;
                    Vector3 boundaryNormal = Vector3.Cross(boundary, pos);
                    borderCorners[borderCornerKey].stress = calculateStress(plateMovement[plate0], plateMovement[plate1], boundary, boundaryNormal);
                }
            }
        }

        static Stress calculateStress(Vector3 movement0, Vector3 movement1, Vector3 boundaryVector, Vector3 boundaryNormal)
        {
            var relativeMovement = movement0 - movement1;
            // "pressure" is the stress normal to the tiles. In theory, negative pressure means they are moving apart, +ve means colliding
            var pressureVector = Math2.ProjectOnVector(relativeMovement, boundaryNormal);
            var pressure = pressureVector.Length;
            if (Vector3.Dot(pressureVector, boundaryNormal) > 0) pressure = -pressure;
            // Shear is the stress along the boundary between the tiles.
            var shear = Math2.ProjectOnVector(relativeMovement, boundaryVector).Length;

            // Values are in the range +- 0.001 - 0.05 - change up to +- 0.02 - 1.0
            return new Stress(20.0f * pressure, 20.0f * shear);
        }

        public enum ElevationCalculation
        {
            COLLIDING,
            SUBDUCTING,
            SUPERDUCTING,
            DIVERGING,
            SHEARING,
            DORMANT
        };

        public void CalculateBorderTileHeights()
        {
            List<ElevationElement> elevationBorderQueue = new List<ElevationElement>();

            //     if perpendicular motion is +ve, it's a collision.
            //                                -ve, it's a rift (which would form basaltic volcanoes)
            //     If collision, then
            //        if heights sufficiently close & same sign, treat as height increase
            //           mountain formation; folding;
            //        if heights opposite sign; subduct the lower under the higher.
            //           tilt upper plate (calc for all boundaries)
            foreach (int borderCornerKey in borderCorners.Keys)
            {
                List<Int64> borderIndices = borderCorners[borderCornerKey].borderIndices;
                if (borderIndices.Count == 3)
                {
                    // At 3 plate boundaries, just take some maxima / averages:
                    Plate plate0 = plates[VertexToPlates[geometry.Topology.Centroids[borderCornerKey].Faces[0]]];
                    Plate plate1 = plates[VertexToPlates[geometry.Topology.Centroids[borderCornerKey].Faces[1]]];
                    Plate plate2 = plates[VertexToPlates[geometry.Topology.Centroids[borderCornerKey].Faces[2]]];
                    Stress stress = borderCorners[borderCornerKey].stress;
                    if (stress.pressure > 0.3)
                    {
                        borderCorners[borderCornerKey].elevation = Math.Max(plate0.Traits.Elevation, Math.Max(plate1.Traits.Elevation, plate2.Traits.Elevation)) + stress.pressure/3;
                    }
                    else if (stress.pressure < -0.01)
                    {
                        borderCorners[borderCornerKey].elevation = Math.Max(plate0.Traits.Elevation, Math.Max(plate1.Traits.Elevation, plate2.Traits.Elevation)) + stress.pressure / 4;
                    }
                    else if (stress.shear > 0.3)
                    {
                        borderCorners[borderCornerKey].elevation = Math.Max(plate0.Traits.Elevation, Math.Max(plate1.Traits.Elevation, plate2.Traits.Elevation)) + stress.shear / 8;
                    }
                    else
                    {
                        borderCorners[borderCornerKey].elevation = (plate0.Traits.Elevation + plate1.Traits.Elevation + plate2.Traits.Elevation) / 3.0f;
                    }
                }
                else
                {
                    Border border0 = borders[borderIndices[0]];
                    Border border1 = borders[borderIndices[1]];
                    int plateIndex0 = border0.plate0;
                    int plateIndex1 = border1.plate0 == plateIndex0 ? border1.plate1 : border0.plate1;
                    Plate plate0 = plates[plateIndex0];
                    Plate plate1 = plates[plateIndex1];

                    ElevationCalculation elevationCalculation = ElevationCalculation.DORMANT;
                    Stress stress = borderCorners[borderCornerKey].stress;
                    if (stress.pressure > 0.2)
                    {
                        borderCorners[borderCornerKey].elevation = Math.Max(plate0.Traits.Elevation, plate1.Traits.Elevation) + stress.pressure;

                        if (plate0.Traits.Elevation < 0 && plate0.Traits.Elevation < 0)
                        {
                            elevationCalculation = ElevationCalculation.COLLIDING;
                        }
                        else if (plate0.Traits.Elevation < 0)
                        {
                            elevationCalculation = ElevationCalculation.SUBDUCTING;
                        }
                        else if (plate1.Traits.Elevation < 0)
                        {
                            elevationCalculation = ElevationCalculation.SUPERDUCTING;
                        }
                        else
                        {
                            elevationCalculation = ElevationCalculation.COLLIDING;
                        }
                    }
                    else if (stress.pressure < -0.3)
                    {
                        borderCorners[borderCornerKey].elevation = Math.Max(plate0.Traits.Elevation, plate1.Traits.Elevation) + stress.pressure / 4;
                        elevationCalculation = ElevationCalculation.DIVERGING;
                    }
                    else if (stress.shear > 0.3)
                    {
                        borderCorners[borderCornerKey].elevation = Math.Max(plate0.Traits.Elevation, plate1.Traits.Elevation) + stress.shear / 8;
                        elevationCalculation = ElevationCalculation.SHEARING;
                    }
                    else
                    {
                        borderCorners[borderCornerKey].elevation = (plate0.Traits.Elevation + plate1.Traits.Elevation) / 2.0f;
                        elevationCalculation = ElevationCalculation.DORMANT;
                    }
                    borderCorners[borderCornerKey].calculation = elevationCalculation;
                    /*
                    // Queue up:
                    //   next corner: Inner corner: the corner that isn't the opposite corner of border0 and border1
                    //     (i.e. remove the opposite corners from Centroid neighbours, and it's the remaining one).
                    //   origin: { this corner, stress, plate, elevationType }
                    //   border: inner border 
                    //   corner: this corner
                    //   distanceToPlateBoundary: inner border length, i.e. of next corner.
                    ElevationElement e = new ElevationElement
                    {
                        CornerIndex = borderCornerKey,
                        NextCornerIndex = 0, // TODO
                        DistanceToBoundary = 0, // TODO
                        Edge = border0,
                        Origin1 = new ElevationElement.Origin
                        {
                            CornerIndex = borderCornerKey,
                            Stress = stress,
                            PlateIndex = plateIndex0,
                            ElevationCalculation = elevationCalculation
                        }
                    };
                    elevationElements.Add(e);*/
                }
            }
        }


        float CalculateCollidingElevation(float distanceToPlateBoundary, float distanceToPlateRoot, float boundaryElevation, 
            float plateElevation, float pressure, float shear)
        {
            var t = distanceToPlateBoundary / (distanceToPlateBoundary + distanceToPlateRoot);
            if (t < 0.5)
            {
                t = t / 0.5f;
                return plateElevation + (float)Math.Pow(t - 1, 2) * (boundaryElevation - plateElevation);
            }
            else
            {
                return plateElevation;
            }
        }

        float CalculateSuperductingElevation(float distanceToPlateBoundary, float distanceToPlateRoot, float boundaryElevation,
            float plateElevation, float pressure, float shear)
        {
            var t = distanceToPlateBoundary / (distanceToPlateBoundary + distanceToPlateRoot);
            if (t < 0.2f)
            {
                t = t / 0.2f;
                return boundaryElevation + t * (plateElevation - boundaryElevation + pressure / 2);
            }
            else if (t < 0.5)
            {
                t = (t - 0.2f) / 0.3f;
                return plateElevation + (float)Math.Pow(t - 1, 2.0f) * pressure / 2.0f;
            }
            else
            {
                return plateElevation;
            }
        }

        float CalculateSubductingElevation(float distanceToPlateBoundary, float distanceToPlateRoot, float boundaryElevation,
            float plateElevation, float pressure, float shear)
        {
            var t = distanceToPlateBoundary / (distanceToPlateBoundary + distanceToPlateRoot);
            return plateElevation + (float)Math.Pow(t - 1, 2) * (boundaryElevation - plateElevation);
        }

        float CalculateDivergingElevation(float distanceToPlateBoundary, float distanceToPlateRoot, float boundaryElevation,
            float plateElevation, float pressure, float shear)
        {
            var t = distanceToPlateBoundary / (distanceToPlateBoundary + distanceToPlateRoot);
            if (t < 0.3)
            {
                t = t / 0.3f;
                return plateElevation + (float)Math.Pow(t - 1, 2) * (boundaryElevation - plateElevation);
            }
            else
            {
                return plateElevation;
            }
        }

        float CalculateShearingElevation(float distanceToPlateBoundary, float distanceToPlateRoot, float boundaryElevation,
            float plateElevation, float pressure, float shear)
        {
            var t = distanceToPlateBoundary / (distanceToPlateBoundary + distanceToPlateRoot);
            if (t < 0.2)
            {
                t = t / 0.2f;
                return plateElevation + (float)Math.Pow(t - 1, 2) * (boundaryElevation - plateElevation);
            }
            else
            {
                return plateElevation;
            }
        }

        float CalculateDormantElevation(float distanceToPlateBoundary, float distanceToPlateRoot, float boundaryElevation,
            float plateElevation, float pressure, float shear)
        {
            var t = distanceToPlateBoundary / (distanceToPlateBoundary + distanceToPlateRoot);
            var elevationDifference = boundaryElevation - plateElevation;
            var a = 2 * elevationDifference;
            var b = -3 * elevationDifference;
            return t * t * elevationDifference * (2 * t - 3) + boundaryElevation;
        }

        struct ElevationElement
        {
            public struct Origin
            {
                public int CornerIndex { get; set; }
                public Stress Stress { get; set; }
                public int PlateIndex { get; set; }
                public ElevationCalculation ElevationCalculation { get; set; }
            };

            public int CornerIndex { get; set; }
            public int NextCornerIndex { get; set; }
            public Border Edge { get; set; }
            public float DistanceToBoundary { get; set; }
            public Origin Origin1 { get; set; }
        }

        List<ElevationElement> elevationElements = new List<ElevationElement>();

        void ProcessElevationElements()
        {
            foreach( ElevationElement elevationElement in elevationElements )
            {
                switch(elevationElement.Origin1.ElevationCalculation)
                {
                    case ElevationCalculation.COLLIDING:
                    case ElevationCalculation.SUBDUCTING:
                    case ElevationCalculation.SUPERDUCTING:
                    case ElevationCalculation.DIVERGING:
                    case ElevationCalculation.SHEARING:
                    case ElevationCalculation.DORMANT:
                        break;
                }
            }
        }


        #region Debug methods
        public List<BorderCorner> GetBorderCorners(int vertex)
        {
            var plateIdx = VertexToPlates[vertex];
            var plate = plates[VertexToPlates[vertex]];
            List<BorderCorner> theBorders = new();
            try
            {
                int borderTileIndex = plate.BorderTiles.Find(tile => tile == vertex);
                if(borderTileIndex == 0 && plate.BorderTiles[0] != vertex)
                {
                    return theBorders;
                }
                var outerNeighbours = geometry.Topology.VertexNeighbours.GetNeighbours((int)borderTileIndex);
                for (int neighbourIdx = 0; neighbourIdx < outerNeighbours.Count; ++neighbourIdx)
                {
                    // neighbour vertexIdx is the vertex index in this world mesh of this tile's neighbour.
                    int neighbourVertexIdx = outerNeighbours.Neighbours[neighbourIdx];
                    int neighbourPlateIdx = VertexToPlates[neighbourVertexIdx];
                    if (neighbourPlateIdx != plateIdx)
                    {
                        var key = Geometry.Topology.CreateEdgeKey((uint)neighbourVertexIdx, (uint)borderTileIndex);
                        var border = borders[key];
                        theBorders.Add(borderCorners[border.c1Index]);
                        theBorders.Add(borderCorners[border.c2Index]);
                    }
                }
            }
            catch { }
            return theBorders;
        }
        #endregion
        #region Debug renderers
        public Geometry<AltVertex> GenerateBorderGeometry<AltVertex>()
            where AltVertex : struct, IVertex
        {
            if (borders == null || borders.Count == 0)
            {
                return null;
            }
            else
            {
                List<AltVertex> vertices = new List<AltVertex>();
                List<uint> newIndices = new List<uint>();

                const float h = 0.1f;

                foreach (var iter in borders)
                {
                    Int64 borderKey = iter.Key;
                    int v1index = (int)(borderKey & 0xffffffff);
                    int v2index = (int)((borderKey >> 32) & 0xffffffff);
                    Border border = iter.Value;

                    // The border lies along an edge in the dual geometry.
                    int plateIndex1 = border.plate0;
                    int plateIndex2 = border.plate1;
                    Vector3 v1Pos = geometry.Mesh.GetPosition(v1index);
                    Vector3 v2Pos = geometry.Mesh.GetPosition(v2index);
                    Vector3 centroid1 = geometry.Topology.Centroids[border.c1Index].position;
                    Vector3 centroid2 = geometry.Topology.Centroids[border.c2Index].position;

                    Vector3 v1g1prime = Math2.BaseProjection(v1Pos, centroid1, centroid2, h);
                    Vector3 v1g2prime = Math2.BaseProjection(v1Pos, centroid2, centroid1, h);
                    Vector3 v2g1prime = Math2.BaseProjection(v2Pos, centroid1, centroid2, h);
                    Vector3 v2g2prime = Math2.BaseProjection(v2Pos, centroid2, centroid1, h);

                    centroid1 *= 1.01f; // Project the centroids out of the sphere slightly
                    centroid2 *= 1.01f;

                    bool edgeOrderIsAnticlockwise = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (geometry.Indices[border.c1Index * 3 + i] == v1index)
                        {
                            if (geometry.Indices[border.c1Index * 3 + (i + 1) % 3] == v2index)
                            {
                                edgeOrderIsAnticlockwise = true;
                            }
                            break;
                        }
                    }

                    int index = vertices.Count;
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v1g2prime, ref centroid2, ref centroid1, edgeOrderIsAnticlockwise);
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v1g2prime, ref centroid1, ref v1g1prime, edgeOrderIsAnticlockwise);
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v2g1prime, ref centroid1, ref centroid2, edgeOrderIsAnticlockwise);
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v2g1prime, ref centroid2, ref v2g2prime, edgeOrderIsAnticlockwise);

                    float p1 = Math2.Clamp(Math.Abs(borderCorners[border.c1Index].stress.pressure), 0, 1.0f);
                    float p2 = Math2.Clamp(Math.Abs(borderCorners[border.c2Index].stress.pressure), 0, 1.0f);
                    float hue1 = borderCorners[border.c1Index].stress.pressure > 0 ? 0 : 0.5f;
                    float hue2 = borderCorners[border.c2Index].stress.pressure > 0 ? 0 : 0.5f;
                    Vector3 rgb1 = Math2.HSV2RGB(new Vector3(hue1, p1, 1.0f - p1));
                    Vector3 rgb2 = Math2.HSV2RGB(new Vector3(hue2, p2, 1.0f - p2));
                    Vector4 c1Color = new Vector4(rgb1.X, rgb1.Y, rgb1.Z, 1);
                    Vector4 c2Color = new Vector4(rgb2.X, rgb2.Y, rgb2.Z, 1);
                    AltVertex v;
                    for (int i = 0; i < 12; ++i)
                    {
                        Vector4 color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        if (i == 0 || i == 1 || i == 3 || i == 8 || i == 10 || i== 11)
                        {
                            color = c2Color;
                        }
                        else if (i == 2 || i == 4 || i==5 || i==6 || i == 7 || i== 9)
                        {
                            color = c1Color;
                        }
                        v = vertices[index +i]; MeshAttr.SetColor(ref v, ref color); vertices[index + i] = v;
                    }
                }
                Mesh<AltVertex> newMesh = new Mesh<AltVertex>(vertices.ToArray());
                return new Geometry<AltVertex>(newMesh, newIndices.ToArray());
            }
        }

        public Geometry<Vertex3DColorUV> GenerateSpinDriftDebugGeom(bool spin)
        {
            List<Vertex3DColorUV> vertices = new List<Vertex3DColorUV>(geometry.Mesh.Length * 4);
            List<uint> indices = new List<uint>();
            const float epsilon = 1e-8f;
            float sqrt2by2 = (float)Math.Sqrt(2.0) * 0.5f;
            foreach (var plate in plates)
            {
                foreach (int tileIndex in plate.Tiles)
                {
                    Vector3 pos = geometry.Mesh.GetPosition(tileIndex);
                    Vector3 tangent;
                    if (spin)
                        tangent = plate.CalculateSpin(pos);
                    else
                        tangent = plate.CalculateDrift(pos);

                    pos *= 1.01f;
                    float side = tangent.Length;
                    if (side > epsilon)
                    {
                        tangent.Normalize();
                        Vector3 w = Vector3.Cross(tangent, pos);
                        w.Normalize();
                        Vector3 n = pos;
                        n.Normalize();
                        tangent *= side;
                        w *= side;

                        // Construct 4 verts around this position:
                        Vector3 A = pos + tangent + w;
                        Vector3 B = pos - tangent + w;
                        Vector3 C = pos - tangent - w;
                        Vector3 D = pos + tangent - w;

                        int index = vertices.Count;
                        var vertex = new Vertex3DColorUV();
                        vertex.SetPosition(A);
                        vertex.SetTextureCoordinates(new Vector2(1, 1));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertex.SetNormal(n);
                        vertices.Add(vertex);
                        vertex = new Vertex3DColorUV();
                        vertex.SetPosition(B);
                        vertex.SetTextureCoordinates(new Vector2(0, 1));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertex.SetNormal(n);
                        vertices.Add(vertex);
                        vertex = new Vertex3DColorUV();
                        vertex.SetPosition(C);
                        vertex.SetTextureCoordinates(new Vector2(0, 0));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertex.SetNormal(n);
                        vertices.Add(vertex);
                        vertex = new Vertex3DColorUV();
                        vertex.SetPosition(D);
                        vertex.SetTextureCoordinates(new Vector2(1, 0));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertex.SetNormal(n);
                        vertices.Add(vertex);
                        indices.Add((uint)index);
                        indices.Add((uint)index + 2);
                        indices.Add((uint)index + 1);
                        indices.Add((uint)index + 3);
                        indices.Add((uint)index + 2);
                        indices.Add((uint)index);
                    }
                }
            }
            var mesh = new Mesh<Vertex3DColorUV>(vertices.ToArray());
            var spinGeometry = new Geometry<Vertex3DColorUV>(mesh, indices.ToArray());
            return spinGeometry;
        }

        public Geometry<Vertex3DColor> GenerateDistanceDebugGeom()
        {
            List<Vertex3DColor> verts = new List<Vertex3DColor>();
            List<uint> indices = new List<uint>();
            for( int plateIndex=0; plateIndex < plates.Length; ++plateIndex)
            {
                Vector3 center = plates[plateIndex].Traits.Center;
                int cornerIndex = 0;
                foreach( int corner in plates[plateIndex].Corners )
                {
                    float hue = (float)cornerIndex / (float)plates[plateIndex].Corners.Count;
                    Vector3 color3 = Math2.HSV2RGB(new Vector3(hue, 0.7f, 0.5f));
                    Vector4 color = new Vector4(color3.X, color3.Y, color3.Z, 1.0f);
                    Centroid centroid = geometry.Topology.Centroids[corner];
                    Vector3 cornerPos = centroid.position;
                    Vector3 direction = cornerPos - center;
                    direction.Normalize();
                    float theta = centroid.PlateDistances[plateIndex];
                    float distance = 2.0f * (float)Math.Sin(theta * 0.5f);
                    direction *= distance;
                    cornerPos = center + direction;
                    cornerPos.Normalize();
                    int vertexStart = verts.Count;
                    GeometryFactory.AddArcVerts(verts, center, cornerPos, 1.001f, color);
                    for(int vertexIndex = vertexStart; vertexIndex < verts.Count-1; ++vertexIndex )
                    {
                        indices.Add((uint)vertexIndex);
                        indices.Add((uint)vertexIndex+1);
                    }
                    ++cornerIndex;
                }
            }
            Mesh<Vertex3DColor> mesh = new Mesh<Vertex3DColor>(verts.ToArray());
            Geometry<Vertex3DColor> geom = new Geometry<Vertex3DColor>(mesh, indices.ToArray());
            geom.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Lines;
            return geom;
        }
        #endregion
    }
}
