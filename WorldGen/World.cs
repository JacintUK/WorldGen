using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WorldGenerator
{
    class World
    {
        private float tweakRatio = 0.25f; // Percentage of total triangles to attempt to tweak
        private int worldSeed = 0;
        private Random rand;
        private int numPlates = 20;
        public IGeometry geometry;
        public Plates plates;

        public World()
        {
            Initialize();
            Distort();
            CreatePlates();
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

        public void CreatePlates()
        {
            plates = new Plates (ref rand, numPlates, geometry);
            plates.CalculatePlateBoundaries(false);
            plates.CalculateStresses();
        }

        // Debug method
        public void InitPlates()
        {
            // Debugging plate growth
            Vector4 blankColor = new Vector4(0.2f, 0.3f, 0.8f, 0.0f);
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                geometry.Mesh.SetColor(i, ref blankColor);

            plates.InitPlates();
        }

        // Debug method
        public void GrowPlates()
        {
            plates.GrowPlates();
            plates.CalculatePlateBoundaries(false);
        }
    }
}
