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
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace WorldGenerator
{
    class VertexBuffer<TVertex> : IVertexBuffer where TVertex : struct, IVertex
    {
        private TVertex[] vertices;
        private VertexArray<TVertex> vertexArray; 
        private VertexFormat vertexFormat;
        private int numVertices;
        private int bufferHandle;
        private int vertexArrayHandle;
        private bool uploaded = false;
        private bool createdArrays = false;

        public int Size
        {
            get { return numVertices; }
        }

        public int Stride
        {
            get { return vertexFormat.size; }
        }

        public VertexBuffer( Mesh<TVertex> mesh )
        {
            this.numVertices = mesh.Length;
            this.vertices = mesh.vertices;
            this.vertexFormat = mesh.VertexFormat;
            bufferHandle = GL.GenBuffer();
        }

        public void Upload( IMesh newMesh)
        {
            if (newMesh is Mesh<TVertex>)
            {
                Mesh<TVertex> theMesh = newMesh as Mesh<TVertex>;
                vertices = theMesh.vertices;
                numVertices = theMesh.Length;
                vertexFormat = theMesh.VertexFormat;
                uploaded = false;
                // Vertex format has to stay the same, no need to change attrib array
            }
        }
        public void Bind(Shader shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);
            if ( !uploaded )
            {
                uploaded = true;
                GL.BufferData<TVertex>(BufferTarget.ArrayBuffer,
                                      (IntPtr)(vertexFormat.size * vertices.Length),
                                      vertices, BufferUsageHint.StaticDraw);
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
                    var attrib = new VertexAttribute(attribute.Name,
                        VertexFormat.NumberOfFloatsInType(attribute.Type),
                        VertexAttribPointerType.Float, vertexFormat.size, attribute.Offset);
                    attrib.Set(shader);
                }

                createdArrays = true;
            }
            else
            {
                GL.BindVertexArray(vertexArrayHandle);
            }
        }

        public void AddVertexArray(VertexArray<TVertex> vertexArray)
        {
            this.vertexArray = vertexArray;
        }
    }
}
