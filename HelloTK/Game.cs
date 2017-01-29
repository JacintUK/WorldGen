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
        Vector3 lightPosition = new Vector3(-2, 2, 2);
        Vector3 ambientColor;

        float longitude, attitude;

        const string SHADER_PATH = "Resources/Shaders/";

        public Game(int w, int h)
           : base(w, h, new OpenTK.Graphics.GraphicsMode(32, 8, 0, 0), 
                 "Icosahedron",
                  GameWindowFlags.Default, DisplayDevice.Default,
                  // ask for an OpenGL 3.0 forward compatible context
                   3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            ambientColor = ToVec3(backgroundColor)*0.25f;
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

            GL.Enable(EnableCap.Texture2D);

            SetCameraProjection();

            Shader texShader = new Shader(SHADER_PATH+"quadVertShader.glsl", SHADER_PATH + "texFragShader.glsl");
            Shader shader = new Shader(SHADER_PATH + "Vert3DColorUVShader.glsl", SHADER_PATH + "shadedFragShader.glsl");
            //Texture edgeTexture = new Texture("edge.png");
            Texture cellTexture = new Texture("CellCorner.png");
            //renderers.Add(RendererFactory.CreateTriangle(texShader));
            quad = RendererFactory.CreateQuad(texShader);
            quad.AddTexture(cellTexture);
            //renderers.Add(quad);

            ico = RendererFactory.CreateIcosphere(shader);
            icoPos = new Vector3(0, 0, -3);
            ico.Model = Matrix4.CreateTranslation(icoPos);
            ico.AddTexture(cellTexture);
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
                renderer.Draw(view, projection, lightPosition, ambientColor);
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

        bool mouseButton1Down = false;
        bool mouseButton1DownTrigger = false;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                if (!mouseButton1Down)
                    mouseButton1DownTrigger = true;
                mouseButton1Down = true;
            }
        }
 
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                mouseButton1Down = false;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (mouseButton1Down)
            {
                // Coords relative to top left corner of screen.
                UpdateIco(new Point(e.X, e.Y));
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Coords relative to drawing frame
            var mousePos = base.PointToClient(new Point(Mouse.X, Mouse.Y));
        }

        Point oldPoint;
        void UpdateIco(Point mousePos)
        {
            if(mouseButton1DownTrigger)
            {
                oldPoint = mousePos;
                mouseButton1DownTrigger = false;
            }
            int xDelta = mousePos.X - oldPoint.X;
            int yDelta = mousePos.Y - oldPoint.Y;
            oldPoint = mousePos;

            if ( Math.Abs(xDelta) > Math.Abs(yDelta))
            {
                // Rotate around Y axis
                longitude += xDelta/4.0f;
                longitude %= 360;
            }
            else
            {
                attitude += yDelta/2.0f;
                attitude = Math.Max(Math.Min(90, attitude), -90);
            }
            Quaternion equatorRot = Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI * longitude / 180.0f);
            Quaternion polarAxisRot = Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI*attitude/180.0f);
            Quaternion rotation = equatorRot * polarAxisRot;
            //rotation = FromEuler(attitude, longitude, 0);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 tr = Matrix4.CreateTranslation(icoPos);
            ico.Model = rotationMatrix * tr;
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
        public Vector3 ToVec3(Color value)
        {
            return new Vector3(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
        }
    }
}
