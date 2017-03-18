using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WorldGenerator
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
        private IMesh mesh;
        private int startIndex;
        List<int> allIndices; // Tiles
        List<int> outerIndices; // BorderTiles. During growth, it's the last set of grown tiles
        Neighbours neighbours;
        int cycleNum = 0;
        int plateIndex = -1;
        float hue;
        int[] vertexToPlate;

        public PlatePhysicsTraits Traits { get; set; }
        public List<int> BorderTiles { get { return outerIndices; } }
        public List<int> Tiles { get { return allIndices; } }

        public Plate(int[] vertexToPlate, IMesh mesh, PlatePhysicsTraits traits, int plateIndex, int startIndex, Neighbours neighbours, ref Random rand)
        {
            this.mesh = mesh;
            this.startIndex = startIndex;
            this.neighbours = neighbours;
            this.vertexToPlate = vertexToPlate;
            this.plateIndex = plateIndex;
            this.Traits = traits;

            allIndices = new List<int>(6);
            allIndices.Add(startIndex);
            outerIndices = new List<int>(6);
            outerIndices.Add(startIndex);
            hue = rand.Next(500) / 500.0f;
            Vector4 color = NextColor(0);
            mesh.SetColor(startIndex, ref color);
            vertexToPlate[startIndex] = plateIndex;
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
                Vector4 cycleColor = NextColor(cycleNum++);//debug
                var newOuterIndices = new List<int>();
                foreach (int index in outerIndices)
                {
                    var neighbour = neighbours.GetNeighbours(index);

                    foreach (int neighbourIndex in neighbour)
                    {
                        int neighbourPlateIdx = vertexToPlate[neighbourIndex];
                        if ( neighbourPlateIdx == -1 )
                        {
                            // It's not been claimed yet.
                            newOuterIndices.Add(neighbourIndex);
                            vertexToPlate[neighbourIndex] = plateIndex;
                            mesh.SetColor(neighbourIndex, ref cycleColor);//debug
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
            // For each vertex on boundary, 
            //   motion due to rotation about pivot (drift)
            //   + motion due to rotation about plate center (spin)
        }

        public void Recolor(Vector4 color)
        {
            foreach (int index in allIndices)
            {
                mesh.SetColor(index, ref color);
            }
        }
    }
}
