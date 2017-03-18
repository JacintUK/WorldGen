using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Collections;

namespace WorldGenerator
{
    class Centroid
    {
        public class DelimIntArrayEnum : IEnumerable
        {
            public int[] array;
            public DelimIntArrayEnum(int size)
            {
                array = new int[size];
                for (int i = 0; i < size; ++i)
                {
                    array[i] = -1;
                }
            }
            public IEnumerator GetEnumerator()
            {
                for (int index = 0; index < array.Length && array[index] != -1; ++index)
                {
                    yield return array[index];
                }
            }
            public int this[int index]
            {
                get { return array[index]; }
                set { array[index] = value; }
            }
        }

        public Vector3 position;
        private DelimIntArrayEnum faces;
        private DelimIntArrayEnum neighbours;
        public DelimIntArrayEnum Faces {  get { return faces; } }
        public DelimIntArrayEnum Neighbours { get { return neighbours; } }

        public Centroid(Vector3 position)
        {
            this.position = position;
            faces = new DelimIntArrayEnum(3); // face index in dual is equ. to vertex index in this mesh.
            neighbours = new DelimIntArrayEnum(3); // neighbouring centroids
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
