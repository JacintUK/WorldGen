﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WorldGenerator
{
    class Plate
    {
        private IMesh mesh;
        private int startIndex;
        List<int> allIndices;
        List<int> outerIndices;
        Neighbours neighbours;
        int cycleNum = 0;
        float hue;

        Vector3 center;
        Vector3 pivot;
        float centerRotation;
        float pivotRotation;
        float elevation;

        public List<int> OuterIndices { get { return outerIndices; } }
        public List<int> AllIndices { get { return allIndices; } }

        public Plate(IMesh mesh, int startIndex, Neighbours neighbours, ref Random rand)
        {
            this.mesh = mesh;
            this.startIndex = startIndex;
            this.neighbours = neighbours;

            center = mesh.GetPosition(startIndex);
            pivot = new Vector3(rand.Next(100), rand.Next(100), rand.Next(100));
            pivot.Normalize();
            centerRotation = (rand.Next(100)-50)*(float)Math.PI/3600.0f; // -2.5 -> 2.5 degrees
            pivotRotation = (rand.Next(100)-50)*(float)Math.PI/3600.0f;
            // Earth: crust is 5-70km thick, radius of planet is 6,371km, i.e. 0.1% -> 1.0%
            // deepest ocean is ~8km; tallest mountain is ~9km; i.e. +/- 10km
            elevation = rand.Next(200)/100.0f - 0.5f; // -10 -> +10   
            //thickness = 0.001f+(rand.Next(100))/100000.0f ; //0.001 -> 0.01

            allIndices = new List<int>(6);
            allIndices.Add(startIndex);
            outerIndices = new List<int>(6);
            outerIndices.Add(startIndex);
            hue = rand.Next(500) / 500.0f;
            Vector4 color = NextColor(0);
            mesh.SetColor(startIndex, ref color);
        }

        private Vector4 NextColor(int cycleNum)
        {
            float value = Math2.Clamp(0.2f + cycleNum / 20.0f, 0.0f, 1.0f);
            Vector3 HSV = new Vector3(hue, 0.5f, value);
            Vector3 RGB = Math2.HSV2RGB(HSV);
            Vector4 color = new Vector4(RGB.X, RGB.Y, RGB.Z, 1.0f);
            return color;
        }

        public int Grow(int numCycles)
        {
            int growth = 0;
            for (int i = 0; i < numCycles; ++i)
            {
                Vector4 cycleColor = NextColor(cycleNum++);
                var newOuterIndices = new List<int>();
                foreach (int index in outerIndices)
                {
                    var neighbour = neighbours.GetNeighbours(index);

                    foreach (int neighbourIndex in neighbour)
                    {
                        Vector4 vertexColor = mesh.GetColor(neighbourIndex);
                        if (vertexColor.W == 0.0f )
                        {
                            // It's not been claimed yet.
                            newOuterIndices.Add(neighbourIndex);
                            mesh.SetColor(neighbourIndex, ref cycleColor);
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

        public void CalculateBorderTiles(int[] vertexToPlate, bool recolor)
        {
            Vector3 HSV = new Vector3(hue, 0.7f, 0.3f);
            Vector3 RGB = Math2.HSV2RGB(HSV);
            Vector4 color = new Vector4(RGB.X, RGB.Y, RGB.Z, 1.0f);

            outerIndices.Clear();
            int plateIdx = vertexToPlate[allIndices[0]];
            foreach (int index in allIndices)
            {
                var neighbourTiles = neighbours.GetNeighbours(index);

                foreach (int neighbourIndex in neighbourTiles)
                {
                    if (plateIdx != vertexToPlate[neighbourIndex] && !outerIndices.Contains(index))
                    {
                        outerIndices.Add(index);
                        if(recolor)
                            mesh.SetColor(index, ref color);
                    }
                }
            }
        }

        public void CalculateMovement()
        {
            // For each vertex, 
            //   motion due to rotation about pivot (drift)
            //   + motion due to rotation about plate center (spin)
        }
    }
}