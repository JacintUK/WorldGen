using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace HelloTK
{
    class Plate<TVertex> where TVertex : struct, IVertex
    {
        private Mesh<TVertex> mesh;
        private int startIndex;
        List<int> allIndices;
        List<int> outerIndices;
        Neighbours neighbours;
        int cycleNum = 0;
        float hue;

        public Plate(Mesh<TVertex> mesh, int startIndex, Neighbours neighbours, ref Random rand)
        {
            this.mesh = mesh;
            this.startIndex = startIndex;
            this.neighbours = neighbours;

            allIndices = new List<int>(6);
            allIndices.Add(startIndex);
            outerIndices = new List<int>(6);
            outerIndices.Add(startIndex);
            hue = rand.Next(500) / 500.0f;
            Vector4 color = NextColor(0);
            MeshAttr.SetColor(ref mesh.vertices[startIndex], color);
        }

        private Vector4 NextColor(int numCycles)
        {
            float value = Math2.Clamp(0.2f + numCycles / 20.0f, 0.0f, 1.0f);
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
                        Vector4 vertexColor = MeshAttr.GetColor(ref mesh.vertices[neighbourIndex]);
                        if (vertexColor.W == 0.0f )
                        {
                            // It's not been claimed yet.
                            newOuterIndices.Add(neighbourIndex);
                            MeshAttr.SetColor(ref mesh.vertices[neighbourIndex], cycleColor);
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
    }
}
