using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace HelloTK
{
    class Game : OpenTK.GameWindow
    {
        List<Renderer> renderers = new List<Renderer>();
        Color backgroundColor = Color.Aquamarine;
        Matrix4 projection;

        public Game(int w, int h)
           : base(w, h, new OpenTK.Graphics.GraphicsMode(32, 8, 0, 0), 
                 "Icosahedron",
                  GameWindowFlags.Default, DisplayDevice.Default,
                  // ask for an OpenGL 3.0 forward compatible context
                   3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
        }

        private Renderer CreateTriangle(Shader shader)
        {
            Vertex[] verts = new Vertex[3]
            {
                new Vertex(new Vector2(-1.0f, 0.0f), new Vector2(0,0), Color.Red),
                new Vertex(new Vector2(-1.0f, -1.0f), new Vector2(0,1), Color.Blue),
                new Vertex(new Vector2( 0.0f, -1.0f), new Vector2(1,1), Color.Purple),
            };

            VertexBuffer vbo = new VertexBuffer(verts);
            VertexArray vertexArray = new VertexArray(vbo, shader,
                new VertexAttribute("aPosition", 2, VertexAttribPointerType.Float, Vertex.SizeInBytes, 0),
                new VertexAttribute("aTexCoords", 2, VertexAttribPointerType.Float, Vertex.SizeInBytes, 8),
                new VertexAttribute("aColor", 4, VertexAttribPointerType.Float, Vertex.SizeInBytes, 16)
            );
            vbo.AddVertexArray(vertexArray);
            Renderer renderer = new Renderer();
            renderer.AddShader(shader);
            renderer.AddVBO(vbo);
            return renderer;
        }

        private Renderer CreateQuad(Shader shader)
        {
            Vertex[] quad = new Vertex[4]
            {
                new Vertex(new Vector2(0.0f, 0.0f), new Vector2(0,0), Color.Green),
                new Vertex(new Vector2(1.0f, 0.0f), new Vector2(1,1), Color.Orange),
                new Vertex(new Vector2(0.0f, 1.0f), new Vector2(1,1), Color.White),
                new Vertex(new Vector2(1.0f, 1.0f), new Vector2(0,1), Color.Yellow),
            };
            uint[] indices = new uint[6]
            {
                0, 1, 2, 1, 2, 3
            };
            VertexBuffer quadVbo = new VertexBuffer(quad);
            // TODO Define VertexFormat alongside Vertex, and autogen VertexArray from it.
            VertexArray qvertexArray = new VertexArray(quadVbo, shader,
                new VertexAttribute("aPosition", 2, VertexAttribPointerType.Float, Vertex.SizeInBytes, 0),
                new VertexAttribute("aTexCoords", 2, VertexAttribPointerType.Float, Vertex.SizeInBytes, Vector2.SizeInBytes),
                new VertexAttribute("aColor", 4, VertexAttribPointerType.Float, Vertex.SizeInBytes, Vector2.SizeInBytes * 2)
            );

            quadVbo.AddVertexArray(qvertexArray);
            IBO ibo = new IBO(indices);
            Renderer quadRenderer = new Renderer();
            quadRenderer.AddVBO(quadVbo);
            quadRenderer.AddIBO(ibo);
            quadRenderer.AddShader(shader);
            quadRenderer.ModelView = Matrix4.CreateScale(0.7f, 0.7f, 0.7f);//CreateTranslation(0, 0, 5);
            return quadRenderer;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(System.Drawing.Color.Aquamarine);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            SetCameraProjection();

            Shader texShader = new Shader("quadVertShader.glsl", "texFragShader.glsl");
            renderers.Add(CreateTriangle(texShader));
            renderers.Add(CreateQuad(texShader));
        }

        private void SetCameraProjection()
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            projection =
                //Matrix4.CreateOrthographicOffCenter(0, ClientSize.Width, ClientSize.Height, 0, 0, 1);
                //Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
                //Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, Width / (float)Height, 0.1f, 100.0f);
                Matrix4.Identity;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(backgroundColor);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelView = Matrix4.Identity;
            foreach (var renderer in renderers)
            {
                renderer.Draw(modelView, projection);
            }

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetCameraProjection();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Console.WriteLine("Escape Down");
                base.Exit();
            }
        }

        Vector2 mousePos;
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            // Coords relative to top left corner of screen.
            mousePos = new Vector2(e.X, e.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Coords relative to drawing frame
            Point mousePos = base.PointToClient(new Point(Mouse.X, Mouse.Y));
        }
    }
}
