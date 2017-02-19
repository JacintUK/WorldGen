using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace HelloTK
{
    class World
    {
        public IGeometry geometry;
        float tweakRatio = 0.25f; // Percentage of total triangles to attempt to tweak
        int worldSeed = 0;
        int numPlates = 20;
        Random rand;

        public World()
        {
            Initialize();
            Distort();
        }

        public void Initialize()
        {
            rand = new Random(0);
            geometry = RendererFactory.CreateIcosphere(4);
            geometry.PrimitiveType = PrimitiveType.Points;
        }

        public void Distort()
        {
            for (int i = 0; i < 6; i++)
            {
                geometry.TweakTriangles(tweakRatio, rand);
                geometry.RelaxTriangles(0.5f);
            }
            for (int i = 0; i < 6; i++)
            {
                geometry.RelaxTriangles(0.5f);
            }
            Colorise(ref rand, numPlates);
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

        public void Colorise()
        {
            Colorise(ref rand, numPlates);
        }

        public void InitPlates()
        {
            InitPlates(ref rand, numPlates);
        }
        public void GrowPlates()
        {
            for (int i = 0; i < plates.Length; ++i)
            {
                plates[i].Grow(1);
            }
        }


        Plate[] plates;
        private void InitPlates(ref Random rand, int numPlates)
        {
            Vector4 blankColor = new Vector4(0.2f, 0.3f, 0.8f, 0.0f);
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                geometry.Mesh.SetColor(i, ref blankColor);

            Neighbours neighbours = geometry.GetNeighbours();

            plates = new Plate[numPlates];
            int total = numPlates;
            for (int i = 0; i < numPlates; ++i)
            {
                int vertexIndex = rand.Next(geometry.Mesh.Length);
                plates[i] = new Plate(geometry.Mesh, vertexIndex, neighbours, ref rand);
            }
        }

        private void Colorise(ref Random rand, int numPlates)
        {
            InitPlates(ref rand, numPlates);
            int total = numPlates;
            while (total < geometry.Mesh.Length)
            {
                for (int i = 0; i < numPlates; ++i)
                {
                    total += plates[i].Grow(1);
                }
            }
        }
    }
}
