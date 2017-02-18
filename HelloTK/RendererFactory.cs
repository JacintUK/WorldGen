using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System;

namespace HelloTK
{
    class RendererFactory
    {
        static public Renderer CreateTriangle(Shader shader)
        {
            Vertex[] verts = new Vertex[3]
            {
                new Vertex(new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0,0), Color.Red),
                new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector2(0,1), Color.Blue),
                new Vertex(new Vector3( 0.0f, -1.0f, 0.0f), new Vector2(1,1), Color.Purple),
            };
            var mesh = new Mesh<Vertex>(verts);
            Renderer renderer = new Renderer(new Geometry<Vertex>(mesh), shader);
            return renderer;
        }

        static public Renderer CreateQuad(Shader shader)
        {
            Vertex[] quad = new Vertex[4]
            {
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0,1), Color.White),
                new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector2(1,1), Color.White),
                new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector2(0,0), Color.White),
                new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector2(1,0), Color.White),
            };
            uint[] indices = new uint[6]
            {
                0, 1, 2, 1, 2, 3
            };

            Renderer quadRenderer = new Renderer(new Geometry<Vertex>(new Mesh<Vertex>(quad), indices), shader);
            quadRenderer.Model = Matrix4.CreateTranslation(0, 0, 0);
            return quadRenderer;
        }

        static public Renderer CreateRenderer<TVertex>(Shader shader, Mesh<TVertex> mesh, uint[] indices) where TVertex : struct, IVertex
        {
            var vertexBuffer = new VertexBuffer<TVertex>(mesh);
            var indexBuffer = new IndexBuffer(indices);
            var renderer = new Renderer(new Geometry<TVertex>(mesh, indices), shader);
            renderer.Model = Matrix4.CreateTranslation(0, 0, -1);
            return renderer;
        }

        static public IGeometry CreateIcosphere(Random rand, int subDivisions)
        {
            Vertex3DColor[] verts = new Vertex3DColor[12];
            Vector4 color = new Vector4(0.2f, 0.2f, 1.0f, 1.0f);
            Vector4 color2 = new Vector4(1.0f, 0.2f, 1.0f, 1.0f);
            Vector4 color3 = new Vector4(0.2f, 1.0f, 1.0f, 1.0f);

            float t = 1.61803398875f;// approximation of golden ratio

            verts[0] = new Vertex3DColor(Vector3.Normalize(new Vector3(-1, t, 0)),color2);
            verts[1] = new Vertex3DColor(Vector3.Normalize(new Vector3(1, t, 0)), color);
            verts[2] = new Vertex3DColor(Vector3.Normalize(new Vector3(-1, -t, 0)), color);
            verts[3] = new Vertex3DColor(Vector3.Normalize(new Vector3(1, -t, 0)), color3);

            verts[4] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, -1, t)), color);
            verts[5] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, 1, t)), color);
            verts[6] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, -1, -t)), color);
            verts[7] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, 1, -t)), color);

            verts[8] = new Vertex3DColor(Vector3.Normalize(new Vector3(t, 0, -1)), color);
            verts[9] = new Vertex3DColor(Vector3.Normalize(new Vector3(t, 0, 1)), color);
            verts[10] = new Vertex3DColor(Vector3.Normalize(new Vector3(-t, 0, -1)), color);
            verts[11] = new Vertex3DColor(Vector3.Normalize(new Vector3(-t, 0, 1)), color);
            var mesh = new Mesh<Vertex3DColor>(verts);
            var indices = new List<uint>();

            AddIndices(ref indices, 0, 1, 7);
            AddIndices(ref indices, 0, 5, 1);
            AddIndices(ref indices, 0, 11, 5);
            AddIndices(ref indices, 0, 10, 11);
            AddIndices(ref indices, 0, 7, 10);

            AddIndices(ref indices, 1, 5, 9);
            AddIndices(ref indices, 5, 4, 9);
            AddIndices(ref indices, 5, 11, 4);
            AddIndices(ref indices, 11, 2, 4);
            AddIndices(ref indices, 11, 10, 2);

            AddIndices(ref indices, 10, 6, 2);
            AddIndices(ref indices, 10, 7, 6);
            AddIndices(ref indices, 7, 8, 6);
            AddIndices(ref indices, 7, 1, 8);
            AddIndices(ref indices, 1, 9, 8);

            AddIndices(ref indices, 3, 4, 2);
            AddIndices(ref indices, 3, 2, 6);
            AddIndices(ref indices, 3, 6, 8);
            AddIndices(ref indices, 3, 8, 9);
            AddIndices(ref indices, 3, 9, 4);

            var geometry = new Geometry<Vertex3DColor>(mesh, indices.ToArray());
            int vertCount = geometry.SubDivide(subDivisions);

            return geometry;
        }


        private static void AddIndices( ref List<uint> list, uint one, uint two, uint three )
        {
            list.Add(one); list.Add(two); list.Add(three);
        }
    }
}
