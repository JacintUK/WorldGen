﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace HelloTK
{
    class VertexBuffer<TVertex> : IVertexBuffer where TVertex : struct 
    {
        private TVertex[] vertices;
        private VertexArray<TVertex> vertexArray; // TODO Move to a new container parent
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
            this.vertices = mesh.Vertices;
            this.vertexFormat = mesh.VertexFormat;
            bufferHandle = GL.GenBuffer();
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
