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
using OpenTK.Graphics.OpenGL;

namespace WorldGenerator
{
    class VertexArray<TVertex> where TVertex : struct, IVertex
    {
        private int handle;

        public VertexArray(VertexBuffer<TVertex> vertexBuffer, Shader shader, VertexFormat vertexFormat)
        {
            // Construct array of VertexAttribute from vertex format:
            List < VertexAttribute > attrs = new List<VertexAttribute>();
            foreach (var attr in vertexFormat.Attributes)
            {
                attrs.Add(new VertexAttribute(attr.Name, 
                    VertexFormat.NumberOfFloatsInType(attr.Type),
                    VertexAttribPointerType.Float, vertexFormat.size, attr.Offset));
            }
            Initialize(vertexBuffer, shader, attrs.ToArray());
        }

        public VertexArray(VertexBuffer<TVertex> vertexBuffer, Shader shader,
            params VertexAttribute[] attributes)
        {
            Initialize(vertexBuffer, shader, attributes);
        }

        private void Initialize(VertexBuffer<TVertex> vertexBuffer, Shader shader,
            params VertexAttribute[] attributes)
        {
            // create new vertex array object
            GL.GenVertexArrays(1, out this.handle);

            // bind the object so we can modify it
            this.Bind();

            // bind the vertex buffer object
            vertexBuffer.Bind(shader);

            // set all attributes
            foreach (var attribute in attributes)
                attribute.Set(shader);

            // unbind objects to reset state
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Bind()
        {
            // bind for usage (modification or rendering)
            GL.BindVertexArray(this.handle);
        }
    }
}
