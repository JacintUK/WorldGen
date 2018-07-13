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
        private DelimIntArrayEnum faces; // vertex index of this triangle's corners === adjacent face index in dual
        private DelimIntArrayEnum neighbours; // neighbouring centroids
        public DelimIntArrayEnum Faces { get { return faces; } }
        public DelimIntArrayEnum Neighbours { get { return neighbours; } }
        public Dictionary<int, float> PlateDistances { get; }

        public Centroid(Vector3 position)
        {
            this.position = position;
            faces = new DelimIntArrayEnum(3); // face index in dual is equ. to vertex index in this mesh.
            neighbours = new DelimIntArrayEnum(3); // neighbouring centroids
            PlateDistances = new Dictionary<int, float>(3); // distances to unique plates in faces
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
