using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WorldGenerator
{
    using Borders = Dictionary<Int64, Tuple<int, int>>;
    class World
    {
        private float tweakRatio = 0.25f; // Percentage of total triangles to attempt to tweak
        private int worldSeed = 0;
        private int[] vertexToPlate;
        private Neighbours neighbours;
        private Plate[] plates;
        private int numPlates = 20;
        private Random rand;
        private Borders borders;

        public IGeometry geometry;
        public Borders Borders { get { return borders; } }

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
            borders = null;
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
            CreatePlates(ref rand, numPlates);
            CalculatePlateBoundaries(true);
        }

        public void InitPlates()
        {
            InitPlates(ref rand, numPlates);
            if (borders != null)
                borders.Clear();
        }

        public void GrowPlates()
        {
            for (int i = 0; i < plates.Length; ++i)
            {
                plates[i].Grow(1);
            }
            CalculatePlateBoundaries(false);
        }

        private void CreatePlates(ref Random rand, int numPlates)
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

        private void InitPlates(ref Random rand, int numPlates)
        {
            Vector4 blankColor = new Vector4(0.2f, 0.3f, 0.8f, 0.0f);
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                geometry.Mesh.SetColor(i, ref blankColor);

            neighbours = geometry.GetNeighbours();

            plates = new Plate[numPlates];
            int total = numPlates;
            for (int i = 0; i < numPlates; ++i)
            {
                Vector4 color = Vector4.Zero;
                do
                {
                    int vertexIndex = rand.Next(geometry.Mesh.Length);
                    // prevent 2 plates spawning at the same vertex.
                    color = geometry.Mesh.GetColor(vertexIndex);
                    if (color.W == 0.0f)
                    {
                        plates[i] = new Plate(geometry.Mesh, vertexIndex, neighbours, ref rand);
                        break;
                    }
                } while (color.W != 0.0f);
            }
        }

        private void PlateCalculations()
        {
            for (int i = 0; i < numPlates; ++i)
            {
                int vertexIndex = rand.Next(geometry.Mesh.Length);
                plates[i].CalculateMovement();
            }
            // for each edge in plate boundaries,
            //   calculate relative motion between tiles
            //     both parallel to (shear) and perpendicular to (pressure) edge.
            //     if perpendicular motion is +ve, it's a collision.
            //                                -ve, it's a rift (which would form basaltic volcanoes)
            //     If collision, then
            //        if heights sufficiently close & same sign, treat as height increase
            //           mountain formation; folding;
            //        if heights opposite sign; subduct the lower under the higher.
            //           tilt upper plate (calc for all boundaries)

        }


        private void CalculatePlateBoundaries(bool recolor)
        {
            // Find the boundaries between each plate.
            // Each plate keeps it's outerIndices.
            //   Can use this, plus world geometry's neighbours to work out
            //   the neighbouring tile & plate.
            // For each edge in boundary, 
            //   store plate index and tile index 
            if(borders != null)
                borders.Clear();
            borders = new Dictionary<long, Tuple<int, int>>();

            vertexToPlate = new int[geometry.Mesh.Length];
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                vertexToPlate[i] = -1;
            for (int i = 0; i < numPlates; ++i)
            {
                List<int> indices = plates[i].AllIndices;
                for( int j = 0; j < indices.Count; ++j )
                {
                    vertexToPlate[indices[j]] = i;
                }
            }

            if( neighbours == null )
            {
                neighbours = geometry.GetNeighbours();
            }

            int totalBorderTiles = 0;
            for (int plateIdx = 0; plateIdx < numPlates; ++plateIdx)
            {
                plates[plateIdx].CalculateBorderTiles(vertexToPlate, recolor);
                totalBorderTiles += plates[plateIdx].OuterIndices.Count;
            }

            for (int plateIdx = 0; plateIdx < numPlates; ++plateIdx)
            {
                List<int> outerIndices = plates[plateIdx].OuterIndices;
                for( int outerVertexIndex = 0; outerVertexIndex< outerIndices.Count; ++outerVertexIndex )
                {
                    var outerNeighbours = neighbours.GetNeighbours(outerIndices[outerVertexIndex]);
                    for( int neighbourIdx=0; neighbourIdx < outerNeighbours.Count; ++neighbourIdx)
                    {
                        int neighbourVertexIdx = outerNeighbours.Neighbours[neighbourIdx];
                        int neighbourPlateIdx = vertexToPlate[neighbourVertexIdx];
                        if ( neighbourPlateIdx != plateIdx)
                        {
                            // It's not in the plate, so must be a bordering plate.
                            // Index by key from plates; stores verts.
                            Int64 borderKey = CreateBorderKey((uint)neighbourVertexIdx, (uint)outerIndices[outerVertexIndex]);
                            if (!borders.ContainsKey(borderKey ))
                            {
                                borders.Add(borderKey, new Tuple<int, int>(plateIdx, neighbourPlateIdx));
                            }
                        }
                    }
                }
            }
            if( borders.Count < totalBorderTiles/2.0f )
            {
                throw new Exception();
            }
        }

        private static Int64 CreateBorderKey( uint a, uint b )
        {
            Int64 min = a < b ? a : b;
            Int64 max = a >= b ? a : b;
            Int64 key = min << 32 | max;
            return key;
        }
    }
}
