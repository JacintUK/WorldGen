using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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

            VertexBuffer<Vertex> vertexBuffer = new VertexBuffer<Vertex>(verts, Vertex.SizeInBytes);
            VertexArray<Vertex> vertexArray = new VertexArray<Vertex>(vertexBuffer, shader,
                new VertexAttribute("aPosition", 3, VertexAttribPointerType.Float, Vertex.SizeInBytes, 0),
                new VertexAttribute("aTexCoords", 2, VertexAttribPointerType.Float, Vertex.SizeInBytes, 12),
                new VertexAttribute("aColor", 4, VertexAttribPointerType.Float, Vertex.SizeInBytes, 20)
            );
            vertexBuffer.AddVertexArray(vertexArray);
            Renderer renderer = new Renderer();
            renderer.AddShader(shader);
            renderer.AddVertexBuffer(vertexBuffer);
            return renderer;
        }

        static public Renderer CreateQuad(Shader shader)
        {
            Vertex[] quad = new Vertex[4]
            {
                new Vertex(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0,0), Color.Green),
                new Vertex(new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1,1), Color.Orange),
                new Vertex(new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1,1), Color.White),
                new Vertex(new Vector3(1.0f, 1.0f, 0.0f), new Vector2(0,1), Color.Yellow),
            };
            uint[] indices = new uint[6]
            {
                0, 1, 2, 1, 2, 3
            };
            VertexBuffer<Vertex> quadVertexBuffer = new VertexBuffer<Vertex>(quad, Vertex.SizeInBytes);
            // TODO Define VertexFormat alongside Vertex, and autogen VertexArray from it.
            VertexArray<Vertex> qvertexArray = new VertexArray<Vertex>(quadVertexBuffer, shader,
                new VertexAttribute("aPosition", 3, VertexAttribPointerType.Float, Vertex.SizeInBytes, 0),
                new VertexAttribute("aTexCoords", 2, VertexAttribPointerType.Float, Vertex.SizeInBytes, Vector3.SizeInBytes),
                new VertexAttribute("aColor", 4, VertexAttribPointerType.Float, Vertex.SizeInBytes, Vector2.SizeInBytes + Vector3.SizeInBytes)
            );

            quadVertexBuffer.AddVertexArray(qvertexArray);
            IndexBuffer indexBuffer = new IndexBuffer(indices);
            Renderer quadRenderer = new Renderer();
            quadRenderer.AddVertexBuffer(quadVertexBuffer);
            quadRenderer.AddIndexBuffer(indexBuffer);
            quadRenderer.AddShader(shader);
            quadRenderer.Model = Matrix4.CreateTranslation(0, 0, -1);
            return quadRenderer;
        }
    }
}
