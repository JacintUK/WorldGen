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
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace WorldGen
{
    class MeshColorProvider : IColorProvider 
    {
        IMesh mesh;
        public MeshColorProvider(IMesh mesh)
        {
            this.mesh = mesh;
        }
        public Vector4 GetColor(int index)
        {
            return mesh.GetColor(index);
        }
    }

    class HeightColorProvider : IColorProvider
    {
        Plates plates;
        public HeightColorProvider(Plates p)
        {
            plates = p;
        }
        public Vector4 GetColor(int index)
        {
            return (Vector4)(plates.GetPlates()[plates.VertexToPlates[index]].Traits.Elevation > 0 ? Color4.ForestGreen : Color4.Blue);
        }
    }

    /// <summary>
    /// The world class contains the geometry for the world. This is NOT the rendered geometry. It is a 
    /// triangle mesh, based on an subdivided icosahedron that is tweaked and relaxed repeatedly to remove uniformity.
    /// The dual of this mesh (vertices -> faces) is what is rendered. 
    /// 
    /// Each vertex in this geometry represents the center of a tile (face) of side 5, 6 or 7.
    /// Each centroid of a triangle in this geometry represents a vertex of the dual.
    /// 
    /// Each tile is assigned to a plate, by using a simultaneous flood fill for the given number of plates.
    /// </summary>
    class World
    {
        private float tweakRatio = 0.25f; // Percentage of total triangles to attempt to tweak
        private int worldSeed = 0;
        private Random rand;
        public IGeometry geometry;
        public Plates plates;
        MeshColorProvider meshColorProvider;
        HeightColorProvider heightColorProvider;

        public enum WorldColorE{ PlateColor, Height };
        public WorldColorE WorldColor { get; set; } = World.WorldColorE.Height;
        public int NumPlates { get; set; } = 20;
        public int NumSubDivisions { get; set; } = 4;
        public int NumDistortions { get; set; } = 6;

        public World()
        {
            Initialize();
        }

        public void Initialize()
        {
            InitializeSphere(NumSubDivisions); // Generate sphere with unit radius.
            Distort(NumDistortions);
            geometry.Topology.Regenerate(true); 
            CreatePlates(NumPlates);
        }

        public void InitializeSphere(int subDivisions)
        {
            rand = new Random(worldSeed);
            geometry = GeometryFactory.CreateIcosphere(subDivisions);
            geometry.PrimitiveType = PrimitiveType.Points;
            meshColorProvider = new MeshColorProvider(geometry.Mesh);
        }

        public void Distort(int distortions)
        {
            for (int i = 0; i < distortions; i++)
            {
                geometry.TweakTriangles(tweakRatio, rand);
                geometry.RelaxTriangles(0.5f);
            }
            for (int i = 0; i < distortions; i++)
            {
                geometry.RelaxTriangles(0.5f);
            }
        }

        public void ResetSeed()
        {
            rand = new Random(worldSeed);
        }

        public void RelaxTriangles()
        {
            geometry.RelaxTriangles(0.5f);
        }

        public void TweakTriangles()
        {
            geometry.TweakTriangles(tweakRatio, rand);
        }

        public void CreatePlates(int numPlates)
        {
            plates = new Plates (ref rand, numPlates, geometry);
            heightColorProvider = new HeightColorProvider(plates);

            plates.CalculatePlateBoundaries(WorldColor == WorldColorE.PlateColor);
            plates.CalculateStresses();
            plates.CalculateBorderTileHeights();
        }

        public IGeometry RegenerateMesh()
        {
            IColorProvider provider = meshColorProvider;
            switch ( WorldColor )
            {
                case WorldColorE.PlateColor:
                    provider = meshColorProvider;
                    break;
                case WorldColorE.Height:
                    provider = heightColorProvider;
                    break;
            }
            return geometry.GenerateDual<Vertex3DColorUV>(provider);
        }

        // Debug method
        public void GrowPlates()
        {
            plates.GrowPlates();
            plates.CalculatePlateBoundaries(false);
        }
        public void CalculatePlateBoundaries()
        {
            plates.CalculatePlateBoundaries(false);
        }

    }
}
