using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace WorldGenerator
{
    class Plates
    {
        private int[] vertexToPlate;
        private Plate[] plates;
        private int numPlates = 20;
        public int Length { get { return plates.Length; } }
        public int[] VertexToPlates {  get { return vertexToPlate; } }

        public struct Border
        {
            public int plate0;
            public int plate1;
            public int c1Index;
            public int c2Index;

            // c1 and c2 are indexes into the geometry's centroid array, and 
            // represent the corners of the border.
            public Border(int plate0, int plate1, int c1Index, int c2Index)
            {
                this.plate0 = plate0;
                this.plate1 = plate1;
                this.c1Index = c1Index;
                this.c2Index = c2Index;
            }

            public int OppositeCorner( int corner )
            {
                if (c1Index == corner)
                    return c2Index;
                else
                    return c1Index;
            }
        }

        struct Stress
        {
            public float pressure;
            public float shear;
            public Stress(float p, float s)
            {
                pressure = p;
                shear = s;
            }
        }

        class BorderCorner
        {
            public List<Int64> borderIndices = new List<Int64>();
            public Stress stress = new Stress();
        }
        Dictionary<int, BorderCorner> borderCorners;

        private Dictionary<Int64, Border> borders;
        Random rand;
        IGeometry geometry;

        public Plates(ref Random rand, int numPlates, IGeometry geometry )
        {
            this.plates = new Plate[numPlates];
            this.borders = new Dictionary<long, Border>();
            this.rand = rand;
            this.geometry = geometry;
            CreatePlates();
        }

        private void CreatePlates()
        {
            InitPlates();

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

            // Determine elevations of plates
            for (int i = 0; i < numPlates; ++i)
            {
                // Earth: crust is 5-70km thick, radius of planet is 6,371km, i.e. 0.1% -> 1.0%
                // deepest ocean is ~8km; tallest mountain is ~9km; i.e. +/- 10km

                bool oceanic = rand.Next(100) / 100.0f < 0.65f;
                if (oceanic)
                {
                    // Oceanic plates range from -8km to -3km
                    plates[i].Traits.Elevation = rand.Next(100) / 200.0f - 0.8f;
                    //plates[i].Recolor(new Vector4(0, 0, 0.5f, 1));
                }
                else
                {
                    // Continental plates range from 1km to 9km
                    plates[i].Traits.Elevation = rand.Next(800) / 1000.0f + 0.1f;
                    //plates[i].Recolor(new Vector4(0, 0.5f, 0, 1));
                }
            }
        }

        // Debug method
        public void GrowPlates()
        {
            for (int i = 0; i < plates.Length; ++i)
            {
                plates[i].Grow(1);
            }
        }

        public void InitPlates()
        {
            // Initialize vertexToPlate array to a value that isn't a valid plate index
            vertexToPlate = new int[geometry.Mesh.Length];
            for (int i = 0; i < geometry.Mesh.Length; ++i)
                vertexToPlate[i] = -1;
            borders.Clear();

            Neighbours neighbours = geometry.Topology.Neighbours;

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
                        var traits = new PlatePhysicsTraits();
                        traits.Pivot = new Vector3(rand.Next(100) - 50, rand.Next(100) - 50, rand.Next(100) - 50);
                        traits.Pivot.Normalize();
                        traits.Center = geometry.Mesh.GetPosition(vertexIndex);
                        if (Vector3.Dot(traits.Center, traits.Pivot) < 0)
                        {
                            traits.Pivot *= -1.0f; // Ensure pivot is at least in the same hemisphere as the plate center.
                        }
                        traits.CenterRotation = (rand.Next(200) - 100) * (float)Math.PI / 6000.0f; // -3 -> 3 degrees
                        traits.PivotRotation = (rand.Next(200) - 100) * (float)Math.PI / 6000.0f;
                        traits.Elevation = 0.0f;

                        plates[plateIndex] = new Plate(vertexToPlate, geometry.Mesh, traits, plateIndex, vertexIndex, neighbours, ref rand);
                        break;
                    }
                } while (plate != -1);
            }
        }

        public void CalculatePlateBoundaries(bool recolor)
        {
            borders.Clear();
            borderCorners = new Dictionary<int, BorderCorner>();

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
                for (int borderTile = 0; borderTile < borderTiles.Count; ++borderTile)
                {
                    var outerNeighbours = geometry.Topology.Neighbours.GetNeighbours(borderTiles[borderTile]);
                    for (int neighbourIdx = 0; neighbourIdx < outerNeighbours.Count; ++neighbourIdx)
                    {
                        int neighbourVertexIdx = outerNeighbours.Neighbours[neighbourIdx];
                        int neighbourPlateIdx = vertexToPlate[neighbourVertexIdx];
                        if (neighbourPlateIdx != plateIdx)
                        {
                            // It's not in the plate, so must be a bordering plate.
                            // Index by key from verts, stores plate ids, corners
                            Int64 borderKey = Topology.CreateBorderKey((uint)neighbourVertexIdx, (uint)borderTiles[borderTile]);

                            if (!borders.ContainsKey(borderKey))
                            {
                                int c1Index;
                                int c2Index;
                                geometry.Topology.GetCorners(borderTiles[borderTile], neighbourVertexIdx, out c1Index, out c2Index);
                                borders.Add(borderKey, new Border(plateIdx, neighbourPlateIdx, c1Index, c2Index));
                                AddBorderCorner(c1Index, borderKey);
                                AddBorderCorner(c2Index, borderKey);
                            }
                        }
                    }
                }
            }

            if (borders.Count < totalBorderTiles / 2.0f)
            {
                throw new Exception();
            }
        }

        public void AddBorderCorner(int cornerIndex, Int64 borderKey)
        {
            BorderCorner bc;
            if( ! borderCorners.TryGetValue(cornerIndex, out bc) )
            {
                borderCorners[cornerIndex] = new BorderCorner();
            }
            borderCorners[cornerIndex].borderIndices.Add(borderKey);
        }

        public void CalculateStresses()
        {
            // for each vertex in plate boundaries,
            //   calculate relative motion between tiles
            //     both parallel to (shear) and perpendicular to (pressure) edge.
            //     if perpendicular motion is +ve, it's a collision.
            //                                -ve, it's a rift (which would form basaltic volcanoes)
            //     If collision, then
            //        if heights sufficiently close & same sign, treat as height increase
            //           mountain formation; folding;
            //        if heights opposite sign; subduct the lower under the higher.
            //           tilt upper plate (calc for all boundaries)

            int index = 0;
            foreach (int borderCornerKey in borderCorners.Keys)
            {
                List<Int64> borderIndices = borderCorners[borderCornerKey].borderIndices;
                var centroid = geometry.Topology.Centroids[borderCornerKey];
                var pos = centroid.position;

                Dictionary<int, Vector3> plateMovement = new Dictionary<int, Vector3>();
                foreach (Int64 borderKey in borderIndices)
                {
                    Border border = borders[borderKey];

                    // Calculate movement only once for each plate 
                    for (int i = 0; i < 2; ++i)
                    {
                        int plateIndex = (i == 0) ? border.plate0 : border.plate1;
                        Vector3 movement;
                        if (!plateMovement.TryGetValue(plateIndex, out movement))
                        {
                            Plate plate = plates[plateIndex];
                            movement = plate.CalculateSpin(pos) + plate.CalculateDrift(pos);
                            plateMovement[plateIndex] = movement;
                        }
                    }
                }

                if (borderIndices.Count == 3)
                {
                    // 3 separate plates. Find movement from each plate at this corner and average it
                    Stress[] stresses = new Stress[3];
                    int stressIdx = 0;
                    foreach (Int64 borderKey in borderIndices)
                    {
                        Border border = borders[borderKey];
                        int oppositeCornerIndex = border.OppositeCorner(borderCornerKey);
                        var oppositeCornerPosition = geometry.Topology.Centroids[oppositeCornerIndex].position;
                        Vector3 boundary = oppositeCornerPosition - pos;
                        Vector3 boundaryNormal = Vector3.Cross(boundary, pos);
                        stresses[stressIdx++] = calculateStress(plateMovement[border.plate0], plateMovement[border.plate1], boundary, boundaryNormal);
                    }
                    borderCorners[borderCornerKey].stress.pressure = (stresses[0].pressure + stresses[1].pressure + stresses[2].pressure) / 3.0f;
                    borderCorners[borderCornerKey].stress.shear = (stresses[0].shear + stresses[1].shear + stresses[2].shear) / 3.0f;
                }
                else // Border between only 2 plates.
                {
                    // generate average vector, calculate stress once.
                    Border border0 = borders[borderIndices[0]];
                    Border border1 = borders[borderIndices[1]];
                    int plate0 = border0.plate0;
                    int plate1 = border1.plate0 == plate0 ? border1.plate1 : border0.plate1;
                    int oppositeCornerIndex0 = border0.OppositeCorner(borderCornerKey);
                    int oppositeCornerIndex1 = border0.OppositeCorner(borderCornerKey);
                    var oppositeCornerPosition0 = geometry.Topology.Centroids[oppositeCornerIndex0].position;
                    var oppositeCornerPosition1 = geometry.Topology.Centroids[oppositeCornerIndex1].position;
                    Vector3 boundary = oppositeCornerPosition1 = oppositeCornerPosition0;
                    Vector3 boundaryNormal = Vector3.Cross(boundary, pos);
                    borderCorners[borderCornerKey].stress = calculateStress(plateMovement[plate0], plateMovement[plate1], boundary, boundaryNormal);
                }
            }
            index++;
        }

        static Stress calculateStress(Vector3 movement0, Vector3 movement1, Vector3 boundaryVector, Vector3 boundaryNormal)
        {
            var relativeMovement = movement0 - movement1;
            var pressureVector = Math2.ProjectOnVector(relativeMovement, boundaryNormal);
            var pressure = pressureVector.Length;
            if (Vector3.Dot(pressureVector, boundaryNormal) > 0) pressure = -pressure;
            var shear = Math2.ProjectOnVector(relativeMovement, boundaryVector).Length;
            return new Stress(2.0f / (1.0f + (float)Math.Exp(-pressure / 30.0f)) - 1.0f,
                                2.0f / (1.0f + (float)Math.Exp(-shear / 30.0f)) - 1.0f);
        }

        public Geometry<AltVertex> GenerateBorderGeometry<AltVertex>()
            where AltVertex : struct, IVertex
        {
            if (borders == null || borders.Count == 0)
            {
                return null;
            }
            else
            {
                List<AltVertex> vertices = new List<AltVertex>();
                List<uint> newIndices = new List<uint>();

                const float h = 0.1f;

                foreach (var iter in borders)
                {
                    Int64 borderKey = iter.Key;
                    int v1index = (int)(borderKey & 0xffffffff);
                    int v2index = (int)((borderKey >> 32) & 0xffffffff);
                    Border border = iter.Value;

                    // The border lies along an edge in the dual geometry.
                    int plateIndex1 = border.plate0;
                    int plateIndex2 = border.plate1;
                    Vector3 v1Pos = geometry.Mesh.GetPosition(v1index);
                    Vector3 v2Pos = geometry.Mesh.GetPosition(v2index);
                    Vector3 centroid1 = geometry.Topology.Centroids[border.c1Index].position;
                    Vector3 centroid2 = geometry.Topology.Centroids[border.c2Index].position;

                    Vector3 v1g1prime = Math2.BaseProjection(v1Pos, centroid1, centroid2, h);
                    Vector3 v1g2prime = Math2.BaseProjection(v1Pos, centroid2, centroid1, h);
                    Vector3 v2g1prime = Math2.BaseProjection(v2Pos, centroid1, centroid2, h);
                    Vector3 v2g2prime = Math2.BaseProjection(v2Pos, centroid2, centroid1, h);

                    centroid1 *= 1.01f; // Project the centroids out of the sphere slightly
                    centroid2 *= 1.01f;

                    bool edgeOrderIsAnticlockwise = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (geometry.Indices[border.c1Index * 3 + i] == v1index)
                        {
                            if (geometry.Indices[border.c1Index * 3 + (i + 1) % 3] == v2index)
                            {
                                edgeOrderIsAnticlockwise = true;
                            }
                            break;
                        }
                    }

                    int index = vertices.Count;
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v1g2prime, ref centroid2, ref centroid1, edgeOrderIsAnticlockwise);
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v1g2prime, ref centroid1, ref v1g1prime, edgeOrderIsAnticlockwise);
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v2g1prime, ref centroid1, ref centroid2, edgeOrderIsAnticlockwise);
                    geometry.AddTriangle(ref vertices, ref newIndices, ref v2g1prime, ref centroid2, ref v2g2prime, edgeOrderIsAnticlockwise);

                    float p1 = Math2.Clamp(Math.Abs(borderCorners[border.c1Index].stress.pressure) * 1000.0f, 0, 1.0f);
                    float p2 = Math2.Clamp(Math.Abs(borderCorners[border.c2Index].stress.pressure) * 1000.0f, 0, 1.0f);
                    float hue1 = borderCorners[border.c1Index].stress.pressure > 0 ? 0 : 0.5f;
                    float hue2 = borderCorners[border.c2Index].stress.pressure > 0 ? 0 : 0.5f;
                    Vector3 rgb1 = Math2.HSV2RGB(new Vector3(hue1, p1, 1.0f - p1));
                    Vector3 rgb2 = Math2.HSV2RGB(new Vector3(hue2, p2, 1.0f - p2));
                    Vector4 c1Color = new Vector4(rgb1.X, rgb1.Y, rgb1.Z, 1);
                    Vector4 c2Color = new Vector4(rgb2.X, rgb2.Y, rgb2.Z, 1);
                    AltVertex v;
                    for (int i = 0; i < 12; ++i)
                    {
                        Vector4 color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        if (i == 0 || i == 1 || i == 3 || i == 8 || i == 10 || i== 11)
                        {
                            color = c2Color;
                        }
                        else if (i == 2 || i == 4 || i==5 || i==6 || i == 7 || i== 9)
                        {
                            color = c1Color;
                        }
                        v = vertices[index +i]; MeshAttr.SetColor(ref v, ref color); vertices[index + i] = v;
                    }
                }
                Mesh<AltVertex> newMesh = new Mesh<AltVertex>(vertices.ToArray());
                return new Geometry<AltVertex>(newMesh, newIndices.ToArray());
            }
        }

        public Geometry<Vertex3DColorUV> GenerateSpinDriftDebugGeom(bool spin)
        {
            List<Vertex3DColorUV> vertices = new List<Vertex3DColorUV>(geometry.Mesh.Length * 4);
            List<uint> indices = new List<uint>();
            const float epsilon = 1e-8f;
            float sqrt2by2 = (float)Math.Sqrt(2.0) * 0.5f;
            foreach (var plate in plates)
            {
                foreach (int tileIndex in plate.Tiles)
                {
                    Vector3 pos = geometry.Mesh.GetPosition(tileIndex);
                    Vector3 tangent;
                    if (spin)
                        tangent = plate.CalculateSpin(pos);
                    else
                        tangent = plate.CalculateDrift(pos);

                    pos *= 1.01f;
                    float side = tangent.Length;
                    if (side > epsilon)
                    {
                        tangent.Normalize();
                        Vector3 w = Vector3.Cross(tangent, pos);
                        tangent *= side;
                        w.Normalize();
                        w *= side;

                        // Construct 4 verts around this position:
                        Vector3 A = pos + tangent + w;
                        Vector3 B = pos - tangent + w;
                        Vector3 C = pos - tangent - w;
                        Vector3 D = pos + tangent - w;

                        int index = vertices.Count;
                        var vertex = new Vertex3DColorUV();
                        vertex.SetPosition(A);
                        vertex.SetTextureCoordinates(new Vector2(1, 1));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertices.Add(vertex);
                        vertex = new Vertex3DColorUV();
                        vertex.SetPosition(B);
                        vertex.SetTextureCoordinates(new Vector2(0, 1));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertices.Add(vertex);
                        vertex = new Vertex3DColorUV();
                        vertex.SetPosition(C);
                        vertex.SetTextureCoordinates(new Vector2(0, 0));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertices.Add(vertex);
                        vertex = new Vertex3DColorUV();
                        vertex.SetPosition(D);
                        vertex.SetTextureCoordinates(new Vector2(1, 0));
                        vertex.SetColor(new Vector4(1, 1, 1, 1));
                        vertices.Add(vertex);
                        indices.Add((uint)index);
                        indices.Add((uint)index + 2);
                        indices.Add((uint)index + 1);
                        indices.Add((uint)index + 3);
                        indices.Add((uint)index + 2);
                        indices.Add((uint)index);
                    }
                }
            }
            var mesh = new Mesh<Vertex3DColorUV>(vertices.ToArray());
            var spinGeometry = new Geometry<Vertex3DColorUV>(mesh, indices.ToArray());
            return spinGeometry;
        }
    }
}
