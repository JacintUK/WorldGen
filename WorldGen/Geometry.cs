using OpenTK;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace WorldGenerator
{
    class Geometry<TVertex> : IGeometry where TVertex : struct, IVertex
    {
        protected Mesh<TVertex> mesh;
        public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;
        protected Indices indices;
        public bool NeedsUpdate { set; get; }
        public IMesh Mesh { get { return mesh; } }
        public int NumVertices { get { return mesh.Length; } }
        public int NumIndices { get { return indices.Length; } }
        public IIndices Indices { get { return indices; } }

        public Geometry(Mesh<TVertex> mesh, uint[] indices)
        {
            this.mesh = mesh;
            this.indices = new Indices(indices);
        }

        public Geometry(Mesh<TVertex> mesh, Indices indices)
        {
            this.mesh = mesh;
            this.indices = indices;
        }

        public Geometry(Mesh<TVertex> mesh)
        {
            this.mesh = mesh;
            this.indices = null;
        }

        public IVertexBuffer CreateVertexBuffer()
        {
            return new VertexBuffer<TVertex>(mesh);
        }

        public IndexBuffer CreateIndexBuffer()
        {
            if (indices != null)
            {
                return indices.NewIndexBuffer();
            }
            else
            {
                return null;
            }
        }

        public void Upload( IVertexBuffer vbo, IndexBuffer ibo)
        {
            vbo.Upload(mesh);
            if (indices != null && ibo != null)
            {
                indices.Upload(ibo);
            }
            NeedsUpdate = false;
        }
    }
}
