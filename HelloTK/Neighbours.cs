using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    class Neighbours
    {
        public class VertexEnum : IEnumerator
        {
            List<int> neighbours;
            int currentIdx;
            public VertexEnum( List<int> neighbours)
            {
                this.neighbours = neighbours;
                currentIdx = -1;
            }
            public object Current
            {
                get
                {
                    try
                    {
                        return neighbours[currentIdx];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            public bool MoveNext()
            {
                currentIdx++;
                return currentIdx < neighbours.Count;
            }

            public void Reset()
            {
                currentIdx = -1;
            }
        }

        public class VertexNeighbours : IEnumerable
        {
            List<int> neighbours;
            public List<int> Neighbours { get { return neighbours; } }
            public int Count { get { return neighbours.Count; } }

            public VertexNeighbours()
            {
                this.neighbours = new List<int>();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)GetEnumerator();
            }

            public VertexEnum GetEnumerator()
            {
                return new VertexEnum(neighbours);
            }
        }
        private VertexNeighbours[] neighbours;

        public int Count { get { return neighbours.Length; } }
        public VertexNeighbours GetNeighbours(int vertex)
        {
            return neighbours[vertex];
        }

        public Neighbours(int size)
        {
            neighbours = new VertexNeighbours[size];
            for (int i = 0; i < size; ++i)
                neighbours[i] = new VertexNeighbours();
        }
        public void AddTriangle(int v0, int v1, int v2)
        {
            if (!neighbours[v0].Neighbours.Contains(v1))
                neighbours[v0].Neighbours.Add(v1);
            if (!neighbours[v0].Neighbours.Contains(v2))
                neighbours[v0].Neighbours.Add(v2);

            if (!neighbours[v1].Neighbours.Contains(v0))
                neighbours[v1].Neighbours.Add(v0);
            if (!neighbours[v1].Neighbours.Contains(v2))
                neighbours[v1].Neighbours.Add(v2);

            if (!neighbours[v2].Neighbours.Contains(v1))
                neighbours[v2].Neighbours.Add(v1);
            if (!neighbours[v2].Neighbours.Contains(v0))
                neighbours[v2].Neighbours.Add(v0);
        }
    }
}
