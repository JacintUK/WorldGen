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
using System.Collections;
using OpenTK.Mathematics;

namespace WorldGen
{
    /// <summary>
    /// A centroid is the center of a triangle in the primary mesh. This class
    /// holds that position, plus information about the associated faces and plates in the dual mesh
    /// </summary>
    class Centroid
    {
        /// <summary>
        /// Delimited integer array. Delimiter is -1; enumerator (foreach) works up to this delimiter.
        /// </summary>
        public class DelimIntArrayEnum : IEnumerable
        {
            public int[] array;
            public static int DELIMITER = -1;
            public DelimIntArrayEnum(int size)
            {
                array = new int[size];
                Array.Fill(array, DELIMITER);
            }
            public IEnumerator GetEnumerator()
            {
                for (int index = 0; index < array.Length && array[index] != DELIMITER; ++index)
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

        /// <summary>
        /// Position of Centroid
        /// </summary>
        public Vector3 position;

        /// <summary>
        ///  A Face in the geometry's Dual is a vertex in the original geometry.
        /// Each centroid therefore has 3 faces in the dual - the vertices of the triangle that generates the centroid.
        /// </summary>
        public DelimIntArrayEnum Faces { get; }
 
        /// <summary>
        /// Neighbouring centroids in the parent container
        /// </summary>
        public DelimIntArrayEnum Neighbours { get; }

        /// <summary>
        /// Map of Distance from centroid to center vertex of indexed plate, for each adjacent plate
        /// </summary>
        public Dictionary<int, float> PlateDistances { get; }

        /// <summary>
        /// Constructor which takes the position of the centroid in the primary mesh.
        /// </summary>
        /// <param name="position"></param>
        public Centroid(Vector3 position)
        {
            this.position = position;
            Faces = new DelimIntArrayEnum(3); // face index in dual is equ. to vertex index in this mesh.
            Neighbours = new DelimIntArrayEnum(3); // neighbouring centroids
            PlateDistances = new Dictionary<int, float>(3); // distances to unique plates in faces
        }

        /// <summary>
        /// Add a face index 
        /// </summary>
        /// <param name="face">Index of the face in the Dual.</param>
        public void AddFace(int face)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (Faces[i] == DelimIntArrayEnum.DELIMITER)
                {
                    Faces[i] = face;
                    break;
                }
            }
        }

        /// <summary>
        /// Add a neighbouring centroid
        /// </summary>
        /// <param name="neighbour">The index of the neighbouring centroid in the parent container</param>
        public void AddNeighbour(int neighbour)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (Neighbours[i] == DelimIntArrayEnum.DELIMITER)
                {
                    Neighbours[i] = neighbour;
                    break;
                }
            }
        }
    }
}
