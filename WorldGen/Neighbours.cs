using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGenerator
{
    class Neighbours
    {
        public class VertexNeighbours : IEnumerable
        {
            private List<int> neighbours;
            public List<int> Neighbours { get { return neighbours; } }
            public int Count { get { return neighbours.Count; } }

            public VertexNeighbours()
            {
                this.neighbours = new List<int>();
            }
            public IEnumerator GetEnumerator()
            {
                for(int index=0; index < neighbours.Count; ++index)
                {
                    yield return neighbours[index];
                }
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
