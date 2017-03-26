using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;

namespace WorldGenerator
{
    internal interface IGeometry
    {
        PrimitiveType PrimitiveType { get; set; }
        bool NeedsUpdate { set; get; }

        IVertexBuffer CreateVertexBuffer();
        IndexBuffer CreateIndexBuffer();
        void Upload(IVertexBuffer vbo, IndexBuffer ibo);
    }
}