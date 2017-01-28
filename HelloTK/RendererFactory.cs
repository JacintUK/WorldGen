using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace HelloTK
{
    class RendererFactory
    {
        static public Renderer CreateTriangle(Shader shader)
        {
            VertexFormat format = new VertexFormat( new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3},
                new Attribute() { Name = "aTexCoords", Type = Attribute.AType.VECTOR2},
                new Attribute() { Name = "aColor", Type = Attribute.AType.VECTOR4} } );

            Vertex[] verts = new Vertex[3]
            {
                new Vertex(new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0,0), Color.Red),
                new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector2(0,1), Color.Blue),
                new Vertex(new Vector3( 0.0f, -1.0f, 0.0f), new Vector2(1,1), Color.Purple),
            };
            var mesh = new Mesh<Vertex>(verts, format);

            var vertexBuffer = new VertexBuffer<Vertex>(mesh, shader);
            Renderer renderer = new Renderer();
            renderer.AddShader(shader);
            renderer.AddVertexBuffer(vertexBuffer);
            return renderer;
        }

        static public Renderer CreateQuad(Shader shader)
        {
            VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3},
                new Attribute() { Name = "aTexCoords", Type = Attribute.AType.VECTOR2},
                new Attribute() { Name = "aColor", Type = Attribute.AType.VECTOR4} });

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
            
            VertexBuffer<Vertex> quadVertexBuffer = new VertexBuffer<Vertex>(new Mesh<Vertex>(quad, format), shader);
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
