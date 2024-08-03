/*
 * Copyright 2019 David Ian Steele
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

using OpenTK.Mathematics;

namespace WorldGen
{
    /// <summary>
    /// A number of static methods for creating basic primitive renderers
    /// </summary>
    class RendererFactory
    {
        /// <summary>
        /// Hello Triangle!
        /// </summary>
        /// <param name="shader">The shader to use for rendering the triangle. It should handle 3D position, UV and 4 channel color</param>
        /// <returns></returns>
        static public Renderer CreateTriangle(Shader shader)
        {
            Vertex[] verts = new Vertex[3]
            {
                new Vertex(new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0,0), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector2(0,1), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Vertex(new Vector3( 0.0f, -1.0f, 0.0f), new Vector2(1,1), new Vector4(1.0f, 0.0f, 1.0f, 1.0f)),
            };
            var vertexBuffer = new VertexBuffer<Vertex>(verts);
            Renderer renderer = new Renderer(vertexBuffer, null, shader);
            return renderer;
        }

        /// <summary>
        /// Basic quad. Doesn't try to re-use VBO, but it should. It also has an index buffer, but shouldn't.
        /// </summary>
        /// <param name="shader">The shader to use for rendering the quad. It should handle 3D position, UV and 4 channel color</param>
        /// <returns></returns>
        static public Renderer CreateQuad(Shader shader)
        {
            Vertex[] quad = new Vertex[4]
            {
                // Texture coords - top is 0, bottom is 1
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0,1), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector2(1,1), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector2(0,0), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector2(1,0), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
            };
            uint[] indices = new uint[6]
            {
                0, 1, 2, 2, 1, 3
            };
            var vertexBuffer = new VertexBuffer<Vertex>(quad);
            var indexBuffer = new IndexBuffer(indices);
            Renderer quadRenderer = new Renderer(vertexBuffer, indexBuffer, shader);
            return quadRenderer;
        }

        /// <summary>
        /// Create a renderer with an arbitrary set of vertices and indices for a generic vertex type.
        /// </summary>
        /// <typeparam name="TVertex">The type of vertex.</typeparam>
        /// <param name="shader">The shader that will be used</param>
        /// <param name="vertices">The initial vertices for this renderer</param>
        /// <param name="indices">The initial indices for this renderer</param>
        /// <returns></returns>
        static public Renderer CreateRenderer<TVertex>(Shader shader, TVertex[] vertices, uint[] indices) where TVertex : struct, IVertex
        {
            var vertexBuffer = new VertexBuffer<TVertex>(vertices);
            var indexBuffer = new IndexBuffer(indices);
            var renderer = new Renderer(vertexBuffer, indexBuffer, shader);
            return renderer;
        }
    }
}
