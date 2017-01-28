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
        Renderer ico;
        Renderer quad;
        Vector3 icoPos;
        float longitude, latitude;

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
            Shader shader = new Shader("Vert3DColorShader.glsl", "texFragShader.glsl");
            //renderers.Add(RendererFactory.CreateTriangle(texShader));
            quad = RendererFactory.CreateQuad(texShader);
            //renderers.Add(quad);

            ico = RendererFactory.CreateIcosahedron(shader);
            icoPos = new Vector3(0, 0, -3);
            ico.Model = Matrix4.CreateTranslation(icoPos);
            renderers.Add(ico);
        }

        private void SetCameraProjection()
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            projection =
                Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2.0f, Width / (float)Height, 0.1f, 100.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(backgroundColor);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 view = Matrix4.LookAt(Vector3.UnitZ*2, Vector3.Zero, Vector3.UnitY);
            GL.CullFace(CullFaceMode.Back);
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
            UpdateIco(new Point(e.X, e.Y));
        }

        Point oldPoint;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Coords relative to drawing frame
            var mousePos = base.PointToClient(new Point(Mouse.X, Mouse.Y));
            //UpdateIco(mousePos);
        }
        void UpdateIco(Point mousePos)
        {
            int xDelta = mousePos.X - oldPoint.X;
            int yDelta = mousePos.Y - oldPoint.Y;
            if ( Math.Abs(xDelta) > Math.Abs(yDelta))
            {
                // Rotate around Y axis
                longitude += xDelta/100.0f;
                longitude %= 360;
            }
            else
            {
                latitude += yDelta/100.0f;
                latitude = Math.Max(Math.Min(180, latitude), -180);
            }
            Quaternion rot = 
                Quaternion.FromAxisAngle(Vector3.UnitY, longitude) * Quaternion.FromAxisAngle(Vector3.UnitX, latitude);
            Matrix4 rot2 = Matrix4.CreateFromQuaternion(rot);
            Matrix4 tr = Matrix4.CreateTranslation(icoPos);
            ico.Model = rot2 * tr;
            //quad.Model = rot2;
            oldPoint = mousePos;
        }

        Quaternion FromEuler(float pitch, float yaw, float roll)
        {
            yaw *= 0.5f * (float)Math.PI/180.0f;
            pitch *= 0.5f * (float)Math.PI / 180.0f;
            roll *= 0.5f * (float)Math.PI / 180.0f;

            float c1 = (float)Math.Cos(yaw);
            float c2 = (float)Math.Cos(pitch);
            float c3 = (float)Math.Cos(roll);
            float s1 = (float)Math.Sin(yaw);
            float s2 = (float)Math.Sin(pitch);
            float s3 = (float)Math.Sin(roll);

            float w = c1 * c2 * c3 - s1 * s2 * s3;
            Vector3 xyz = new Vector3();
            xyz.X = s1 * s2 * c3 + c1 * c2 * s3;
            xyz.Y = s1 * c2 * c3 + c1 * s2 * s3;
            xyz.Z = c1 * s2 * c3 - s1 * c2 * s3;
            return new Quaternion(xyz, w);
        }
    }
}
