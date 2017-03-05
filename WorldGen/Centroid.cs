using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WorldGenerator
{
    class Centroid
    {
        public Vector3 position;
        public int[] faces;
        public int[] neighbours;

        public Centroid(Vector3 position)
        {
            this.position = position;
            faces = new int[3]; // face index in dual is equ. to vertex index in this mesh.
            neighbours = new int[3]; // neighbouring centroids
            faces[0] = faces[1] = faces[2] = -1;
            neighbours[0] = neighbours[1] = neighbours[2] = -1;
        }
        public void AddFace(int face)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (faces[i] == -1)
                {
                    faces[i] = face;
                    break;
                }
            }
        }
        public void AddNeighbour(int neighbour)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (neighbours[i] == -1)
                {
                    neighbours[i] = neighbour;
                    break;
                }
            }
        }
    }
}
