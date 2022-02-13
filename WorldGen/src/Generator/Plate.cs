/*
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
using OpenTK.Mathematics;

namespace WorldGen
{
    class PlatePhysicsTraits
    {
        public Vector3 Pivot { set; get;  }
        public Vector3 Center { set; get; }
        public float CenterRotation { set; get; }
        public float PivotRotation { set; get; }
        public float Elevation { set; get; }
        public float Thickness { set; get; }
    }

    class Plate
    {
        private IGeometry geometry;
        private int startIndex;
        List<int> allIndices; // Tiles
        List<int> outerIndices; // BorderTiles. During growth, it's the last set of grown tiles
        int cycleNum = 0;
        int plateIndex = -1;
        float hue;
        int[] vertexToPlate;

        public PlatePhysicsTraits Traits { get; set; }
        public List<int> BorderTiles { get { return outerIndices; } }
        public List<int> Tiles { get { return allIndices; } }
        public List<int> Corners { get; } // centroid indices, sorted by distance from center

        public Plate(int[] vertexToPlate, IGeometry geometry, PlatePhysicsTraits traits, int plateIndex, int startIndex, ref Random rand)
        {
            this.geometry = geometry;
            this.startIndex = startIndex;
            this.vertexToPlate = vertexToPlate;
            this.plateIndex = plateIndex;
            this.Traits = traits;
            Corners = new List<int>();

            allIndices = new List<int>(6);
            allIndices.Add(startIndex);
            outerIndices = new List<int>(6);
            outerIndices.Add(startIndex);
            hue = rand.Next(500) / 500.0f;
            Vector4 color = NextColor(0);
            geometry.Mesh.SetColor(startIndex, ref color);
            vertexToPlate[startIndex] = plateIndex;
        }
        
        public int Grow(int numCycles)
        {
            int growth = 0;
            for (int i = 0; i < numCycles; ++i)
            {
                Vector4 cycleColor = NextColor(cycleNum++);//debug
                var newOuterIndices = new List<int>();
                foreach (int index in outerIndices)
                {
                    var vertexNeighbours = geometry.Topology.VertexNeighbours.GetNeighbours(index);

                    foreach (int neighbourIndex in vertexNeighbours)
                    {
                        int neighbourPlateIdx = vertexToPlate[neighbourIndex];
                        if ( neighbourPlateIdx == -1 )
                        {
                            // It's not been claimed yet.
                            newOuterIndices.Add(neighbourIndex);
                            vertexToPlate[neighbourIndex] = plateIndex;
                            geometry.Mesh.SetColor(neighbourIndex, ref cycleColor);//debug
                        }
                    }
                }

                foreach (int index in newOuterIndices)
                {
                    allIndices.Add(index);
                }

                outerIndices = newOuterIndices;
                growth += newOuterIndices.Count;
            }

            return growth;
        }

        public void CalculateBorderTiles( bool recolor)
        {
            // Color for debug
            Vector3 HSV = new Vector3(hue, 0.9f, 0.1f);
            Vector3 RGB = Math2.HSV2RGB(HSV);
            Vector4 color = new Vector4(RGB.X, RGB.Y, RGB.Z, 1.0f);

            outerIndices.Clear();
            int plateIdx = vertexToPlate[allIndices[0]];
            foreach (int index in allIndices)
            {
                var vertexNeighbours = geometry.Topology.VertexNeighbours.GetNeighbours(index);
                foreach (int neighbourIndex in vertexNeighbours)
                {
                    if (plateIdx != vertexToPlate[neighbourIndex] && !outerIndices.Contains(index))
                    {
                        outerIndices.Add(index);
                        if(recolor)
                            geometry.Mesh.SetColor(index, ref color);
                    }
                }
            }
        }

        public Vector3 CalculateSpin(Vector3 position)
        {
            // Combine transverse movement about center and pivot.
            // First project center onto position vector to get an orthogonal spin vector.
            Vector3 spin = Math2.ProjectOnVector(Traits.Center, position) - Traits.Center;
            if (spin.Length > 0.0001f)
            {
                Vector3 tangent = Vector3.Cross(spin, position);
                tangent.Normalize();
                tangent *= spin.Length * (float)Math.Tan(Traits.CenterRotation);
                return tangent;
            }
            return Vector3.Zero;
        }

        public Vector3 CalculateDrift(Vector3 position)
        {
            Vector3 drift = Math2.ProjectOnVector(Traits.Pivot, position ) - Traits.Pivot;
            if (drift.Length > 0.0001f)
            {
                Vector3 tangent = Vector3.Cross(drift, position);
                tangent.Normalize();
                float transverseDrift = Math2.Clamp(drift.Length * (float)Math.Tan(Traits.PivotRotation), -0.01f, .01f);
                tangent *= transverseDrift;
                return tangent;
            }
            return Vector3.Zero;
        }

        public void Recolor(Vector4 color)
        {
            foreach (int index in allIndices)
            {
                geometry.Mesh.SetColor(index, ref color);
            }
        }

        private Vector4 NextColor(int cycleNum)
        {
            float value = Math2.Clamp(0.2f + cycleNum / 20.0f, 0.0f, 1.0f);
            Vector3 HSV = new Vector3(hue, 0.5f, value);
            Vector3 RGB = Math2.HSV2RGB(HSV);
            Vector4 color = new Vector4(RGB.X, RGB.Y, RGB.Z, 1.0f);
            return color;
        }
    }
}
