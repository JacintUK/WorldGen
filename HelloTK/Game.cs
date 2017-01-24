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
           : base(w, h,       // set window resolution, title, and default behaviour
                  new OpenTK.Graphics.GraphicsMode(32, 8, 0, 0), "Icosahedron",
                  GameWindowFlags.Default, DisplayDevice.Default,
                  // ask for an OpenGL 3.0 forward compatible context
                   3, 0, GraphicsContextFlags.ForwardCompatible)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //Title = "Icosahedron";
            GL.ClearColor(System.Drawing.Color.Aquamarine);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            SetCameraProjection();

            Vertex[] verts = new Vertex[3]
            {
                new Vertex(new Vector2(100.0f, 0.0f), new Vector2(0,0), Color.Red),
                new Vertex(new Vector2(300.0f, 300.0f), new Vector2(0,1), Color.Blue),
                new Vertex(new Vector2(100.0f, 300.0f), new Vector2(1,1), Color.Purple),
            };
            VBO vbo = new VBO(verts);
            Renderer renderer = new Renderer();
            renderer.AddVBO(vbo);
            renderers.Add(renderer);

            Vertex[] quad = new Vertex[4]
            {
                new Vertex(new Vector2(300.0f, 0.0f), new Vector2(0,0), Color.Green),
                new Vertex(new Vector2(500.0f, 0.0f), new Vector2(1,1), Color.Orange),
                new Vertex(new Vector2(300.0f, 500.0f), new Vector2(1,1), Color.White),
                new Vertex(new Vector2(500.0f, 500.0f), new Vector2(0,1), Color.Yellow),
            };

            uint[] indices = new uint[6]
            {
                0, 1, 2, 1, 2, 3
            };
            VBO quadVbo = new VBO(quad);
            IBO ibo = new IBO(indices);
            Renderer quadRenderer = new Renderer();
            quadRenderer.AddVBO(quadVbo);
            quadRenderer.AddIBO(ibo);
            renderers.Add(quadRenderer);
        }

        private void SetCameraProjection()
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            projection =
                Matrix4.CreateOrthographicOffCenter(0, ClientSize.Width, ClientSize.Height, 0, 0, 1);
                //Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(backgroundColor);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            Matrix4 modelView = Matrix4.Identity; //Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            foreach (Renderer renderer in renderers)
            {
                renderer.Draw();
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
