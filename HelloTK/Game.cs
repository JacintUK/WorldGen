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
            renderers.Add(RendererFactory.CreateTriangle(texShader));
            renderers.Add(RendererFactory.CreateQuad(texShader));
        }

        private void SetCameraProjection()
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            projection =
                Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, Width / (float)Height, 0.1f, 100.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(backgroundColor);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 view = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

            foreach (var renderer in renderers)
            {
                renderer.Draw(view, projection);
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
