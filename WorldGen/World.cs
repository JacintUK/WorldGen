using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WorldGenerator
{
    using Borders = Dictionary<Int64, Border>;

    struct Border
    {
        public int plate0;
        public int plate1;
        public int c1Index;
        public int c2Index;

        // c1 and c2 are indexes into the geometry's centroid array, and 
        // represent the corners of the border.
        public Border( int plate0, int plate1, int c1Index, int c2Index )
        {
            this.plate0 = plate0;
            this.plate1 = plate1;
            this.c1Index = c1Index;
            this.c2Index = c2Index;
        }
    }

    class World
    {
        private float tweakRatio = 0.25f; // Percentage of total triangles to attempt to tweak
        private int worldSeed = 0;
        private int[] vertexToPlate;
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
            // Debugging plate growth
            Vector4 blankColor = new Vector4(0.2f, 0.3f, 0.8f, 0.0f);
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                geometry.Mesh.SetColor(i, ref blankColor);

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
                    total += plates[i].Grow(1); // todo: Consider interleaving growth loops.
                }
            }

            // Double check we have all tiles covered
            for (int i = 0; i < vertexToPlate.Length; ++i)
            {
                if (vertexToPlate[i] == -1)
                {
                    throw new Exception();
                }
            }
        }

        private void InitPlates(ref Random rand, int numPlates)
        {
            // Initialize vertexToPlate array to a value that isn't a valid plate index
            vertexToPlate = new int[geometry.Mesh.Length];
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                vertexToPlate[i] = -1;

            plates = new Plate[numPlates];
            int total = numPlates;
            for (int plateIndex = 0; plateIndex < numPlates; ++plateIndex)
            {
                int plate = -1;
                do
                {
                    int vertexIndex = rand.Next(geometry.Mesh.Length);
                    // prevent 2 plates spawning at the same vertex.
                    plate = vertexToPlate[vertexIndex];
                    if (plate == -1)
                    {
                        plates[plateIndex] = new Plate(vertexToPlate, geometry.Mesh, plateIndex, vertexIndex, geometry.Neighbours, ref rand);
                        break;
                    }
                } while (plate != -1);
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
            if(borders != null)
                borders.Clear();
            borders = new Borders();

            Neighbours neighbours = geometry.Neighbours;
            // Recalculate border tiles for each plate
            int totalBorderTiles = 0;
            for (int plateIdx = 0; plateIdx < numPlates; ++plateIdx)
            {
                plates[plateIdx].CalculateBorderTiles(recolor);
                totalBorderTiles += plates[plateIdx].BorderTiles.Count;
            }

            // For each plate, check each border tile's neighbours
            //   If a neighbour belongs to a different plate, create a border edge.
            for (int plateIdx = 0; plateIdx < numPlates; ++plateIdx)
            {
                List<int> borderTiles = plates[plateIdx].BorderTiles;
                for( int borderTile = 0; borderTile < borderTiles.Count; ++borderTile )
                {
                    var outerNeighbours = neighbours.GetNeighbours( borderTiles[borderTile] );
                    for( int neighbourIdx=0; neighbourIdx < outerNeighbours.Count; ++neighbourIdx )
                    {
                        int neighbourVertexIdx = outerNeighbours.Neighbours[neighbourIdx];
                        int neighbourPlateIdx = vertexToPlate[neighbourVertexIdx];
                        if ( neighbourPlateIdx != plateIdx)
                        {
                            // It's not in the plate, so must be a bordering plate.
                            // Index by key from verts, stores plate ids, corners
                            Int64 borderKey = CreateBorderKey((uint)neighbourVertexIdx, (uint)borderTiles[borderTile]);

                            if (!borders.ContainsKey(borderKey ))
                            {
                                int c1Index;
                                int c2Index;
                                //geometry.GetCorners(borderTiles[borderTile], neighbourVertexIdx, out c1Index, out c2Index);
                                borders.Add(borderKey, new Border(plateIdx, neighbourPlateIdx, -1, -1));//c1Index, c2Index));
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

        /// <summary>
        /// Key is generated from the vertex index of adjacent tiles
        /// </summary>
        /// <param name="a">vertex index of first tile</param>
        /// <param name="b">vertex index of second tile</param>
        /// <returns></returns>
        private static Int64 CreateBorderKey( uint a, uint b )
        {
            Int64 min = a < b ? a : b;
            Int64 max = a >= b ? a : b;
            Int64 key = min << 32 | max;
            return key;
        }
    }
}
