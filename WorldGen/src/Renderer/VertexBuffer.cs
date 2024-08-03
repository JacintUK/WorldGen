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

using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace WorldGen
{
    class VertexBuffer<TVertex> : IVertexBuffer where TVertex : struct, IVertex
    {
        private TVertex[] vertices;
        private unsafe byte* data=null;

        private VertexFormat vertexFormat;
        private int bufferHandle;
        private int vertexArrayHandle;
        private bool uploaded = false;
        private bool createdArrays = false;
        private bool safe = true;

        public int Size { get; private set; }

        public int Stride
        {
            get { return vertexFormat.size; }
        }

        public VertexBuffer()
        {
            this.Size = 0;
            this.vertices = null;
            this.vertexFormat = null;
            bufferHandle = -1;
        }
        public VertexBuffer(TVertex[] vertices)
        {
            this.Size = vertices.Length;
            this.vertices = vertices;
            this.vertexFormat = vertices[0].GetVertexFormat();
            bufferHandle = GL.GenBuffer();
        }

        public unsafe VertexBuffer(byte* data, int size, VertexFormat vertexFormat)
        {
            this.Size = size;
            this.data = data;
            this.vertexFormat = vertexFormat;
            bufferHandle = GL.GenBuffer();
            safe = false;
        }

        public void Upload(TVertex[] newVertices)
        {
            if( bufferHandle == -1)
            {
                bufferHandle = GL.GenBuffer();
            }
            vertices = newVertices;
            Size = vertices.Length;
            vertexFormat = vertices[0].GetVertexFormat();
            uploaded = false;
        }

        public unsafe void Bind(Shader shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);
            if ( !uploaded )
            {
                uploaded = true;
                if (safe)
                {
                    GL.BufferData<TVertex>(BufferTarget.ArrayBuffer,
                                          (IntPtr)(vertexFormat.size * vertices.Length),
                                          vertices, BufferUsageHint.StaticDraw);
                }
                else
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, Size, (IntPtr)data, BufferUsageHint.StaticDraw);
                }
            }
            if (!createdArrays)
            {
                // create new vertex array object
                GL.GenVertexArrays(1, out vertexArrayHandle);
                GL.BindVertexArray(vertexArrayHandle);
                
                // set all attributes
                List<VertexAttribute> attrs = new List<VertexAttribute>();
                
                foreach (var attribute in vertexFormat.Attributes)
                {
                    var attrib = new VertexAttribute(attribute.Name, VertexFormat.NumberOfElementsInType(attribute.Type),
                        VertexFormat.BaseType(attribute.Type), vertexFormat.size, attribute.Offset, 
                        (attribute.Type==Attribute.AType.UBYTE4) ?true:false); // Normalize if not float.
                    attrib.Set(shader);
                }

                createdArrays = true;
            }
            else
            {
                GL.BindVertexArray(vertexArrayHandle);
            }
        }
    }
}
