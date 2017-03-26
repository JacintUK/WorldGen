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
        private IComplexGeometry geometry;
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

        public Plate(int[] vertexToPlate, IComplexGeometry geometry, PlatePhysicsTraits traits, int plateIndex, int startIndex, ref Random rand)
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
            Vector3 HSV = new Vector3(hue, 0.7f, 0.3f);
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
