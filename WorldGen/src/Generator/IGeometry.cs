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
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace WorldGen
{
    public interface IColorProvider
    {
        Vector4 GetColor(int index);
    }

    internal interface IGeometry
    {
        /// <summary>
        /// The primitive used for rendering this geometry's mesh
        /// </summary>
        PrimitiveType PrimitiveType { get; set; }

        /// <summary>
        /// Set to true if this needs re-uploading
        /// </summary>
        bool NeedsUpdate { set; get; }

        IMesh Mesh { get; }
        int NumVertices { get; }
        int NumIndices { get; }
        IIndices Indices { get; }
        ITopology Topology { get; }

        void TweakTriangles(float ratio, Random rand);
        float RelaxTriangles(float multiplier);

        IGeometry Clone();
        IGeometry ClonePosition<TVertex2>() where TVertex2 : struct, IVertex;

        void ConvertToVertexPerIndex();
        void AddNormals();
        void AddUVs();

        void SetColor(Vector4 color);

        Mesh<Vertex3D> GenerateCentroidPointMesh();
        Geometry<AltVertex> GenerateDual<AltVertex>(IColorProvider colorProvider) where AltVertex : struct, IVertex;
        public IGeometry GenerateTile(uint vertexIndex);

        /// <summary>
        /// Add the triangle specified by the 3 points to the given vertex and index lists.
        /// Doesn't modify this geometry.
        /// </summary>
        /// <typeparam name="AltVertex">Vertex type</typeparam>
        /// <param name="newVerts">Add the vertices v1, v2 and v3 to this list</param>
        /// <param name="newIndices">Add the new indices for the given triangle to this list</param>
        /// <param name="v1">new vertex of the triangle</param>
        /// <param name="v2">new vertex of the triangle</param>
        /// <param name="v3">new vertex of the triangle</param>
        /// <param name="isAnticlockwise">Whether the triangle is clocwise (false) or anticlocwise (true)</param>
        void AddTriangle<AltVertex>(ref List<AltVertex> newVerts, ref List<uint> newIndices, ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, bool isAnticlockwise)
            where AltVertex : struct, IVertex;

        /// <summary>
        /// Add the triangle specified by the vertex at the existing index & the 2 new points 
        /// to the given vertex and index lists. Expects that the existing index is already
        /// in newVerts.
        /// </summary>
        /// <typeparam name="AltVertex">Vertex type</typeparam>
        /// <param name="newVerts">Add the vertices v2 and v3 to this list</param>
        /// <param name="newIndices">Add the new indices for the given triangle to this list</param>
        /// <param name="v1index">index of the existing vertex</param>
        /// <param name="v2">new vertex of the triangle</param>
        /// <param name="v3">new vertex of the triangle</param>
        void AddTriangle2<AltVertex>(ref List<AltVertex> newVerts, ref List<uint> newIndices, int v1index, ref Vector3 v2, ref Vector3 v3, Vector4 color)
            where AltVertex : struct, IVertex;
    }
}