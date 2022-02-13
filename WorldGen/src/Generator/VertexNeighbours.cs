/*
 * Copyright 2018 David Ian Steele
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGen
{
    class VertexNeighbours
    {
        public class PerVertexNeighbours : IEnumerable
        {
            private List<int> neighbours;
            public List<int> Neighbours { get { return neighbours; } }
            public int Count { get { return neighbours.Count; } }

            public PerVertexNeighbours()
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
        private PerVertexNeighbours[] neighbours;

        public int Count { get { return neighbours.Length; } }
        public PerVertexNeighbours GetNeighbours(int vertex)
        {
            return neighbours[vertex];
        }

        public VertexNeighbours(int size)
        {
            neighbours = new PerVertexNeighbours[size];
            for (int i = 0; i < size; ++i)
                neighbours[i] = new PerVertexNeighbours();
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
