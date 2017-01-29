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
using static OpenTK.MathHelper;

namespace HelloTK
{
    class Game : OpenTK.GameWindow
    {
        List<MouseButton> pressedButtons = new List<MouseButton>();
        List<Renderer> renderers = new List<Renderer>();
        Color backgroundColor = Color.Aquamarine;
        Matrix4 projection;
        Renderer ico;
        Renderer quad;
        Vector3 icoPos;
        Vector3 lightPosition = new Vector3(-2, 2, 2);
        Vector3 cameraPosition = Vector3.UnitZ * 2;
        float fieldOfView = (float)Math.PI / 2.0f;
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
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Left));
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Middle));
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Right));

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
            Matrix4 view = Matrix4.LookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
            projection =
                Matrix4.CreatePerspectiveFieldOfView(fieldOfView, Width / (float)Height, 0.1f, 100.0f);

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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            foreach(var button in pressedButtons)
            {
                button.Down(e.Button);
            }
        }
 
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Up(e.Button);
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Update(new Point(e.X, e.Y));
            }

            UpdateIco(pressedButtons[0]);
            UpdateZoom(pressedButtons[1]);
            UpdateCameraPos(pressedButtons[2]);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Coords relative to drawing frame
            var mousePos = base.PointToClient(new Point(Mouse.X, Mouse.Y));
        }

        void UpdateZoom(MouseButton button)
        {
            if (button.IsDown)
            {
                // drag up and down to change zoom level.
                fieldOfView += button.YDelta / 100.0f;
                fieldOfView = Clamp(fieldOfView, 0.1f, (float)Math.PI-0.1f);
            }
        }
        void UpdateCameraPos(MouseButton button)
        {
            if (button.IsDown)
            {
                // drag up and down to change zoom level.
                cameraPosition.Z += button.YDelta / 100.0f;
                cameraPosition.Z = Clamp(cameraPosition.Z, -1, 2);
            }
        }
        void UpdateIco(MouseButton button)
        {
            if (button.IsDown)
            {
                // Rotate around Y axis
                longitude += button.XDelta / 4.0f;
                longitude %= 360;

                attitude += button.YDelta / 2.0f;
                attitude = Math.Max(Math.Min(90, attitude), -90);

                Quaternion equatorRot = Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI * longitude / 180.0f);
                Quaternion polarAxisRot = Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI * attitude / 180.0f);
                Quaternion rotation = equatorRot * polarAxisRot;
                //rotation = FromEuler(attitude, longitude, 0);
                Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);
                Matrix4 tr = Matrix4.CreateTranslation(icoPos);
                ico.Model = rotationMatrix * tr;
            }
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
