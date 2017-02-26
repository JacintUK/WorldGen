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

namespace WorldGenerator
{
    class Game : OpenTK.GameWindow
    {
        List<MouseButton> pressedButtons = new List<MouseButton>();
        List<KeyHandler> keyHandlers = new List<KeyHandler>();
        Node node;
        Color backgroundColor = Color.Aquamarine;
        Matrix4 projection;

        IGeometry worldRenderGeometry;
        IGeometry borderGeometry;
        Renderer worldRenderer;
        Renderer worldCentroidDebugRenderer;
        Renderer worldVertsDebugRenderer;
        Renderer borderRenderer;
        Quaternion rotation = new Quaternion(Vector3.UnitY, 0.0f);
        Vector3 worldPosition;
        Vector3 lightPosition = new Vector3(-2, 2, 2);
        Vector3 cameraPosition = Vector3.UnitZ *2.0f;
        Vector3 ambientColor;

        float fieldOfView = (float)Math.PI / 4.0f;
        float longitude, attitude;
        World world;

        const string SHADER_PATH = "Resources/Shaders/";

        public Game(int w, int h)
           : base(w, h, new OpenTK.Graphics.GraphicsMode(32, 8, 0, 0), 
                 "WorldGen",
                  GameWindowFlags.Default, DisplayDevice.Default,
                  // ask for an OpenGL 3.0 forward compatible context
                   3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            ambientColor = Math2.ToVec3(backgroundColor)*0.25f;
            Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Left, UpdateIco));
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Middle, UpdateZoom));
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Right, UpdateCameraPos));

            keyHandlers.Add(new KeyHandler(Key.Space, RelaxTriangles));
            keyHandlers.Add(new KeyHandler(Key.D, DistortTriangles));
            keyHandlers.Add(new KeyHandler(Key.R, ResetSphere));
            keyHandlers.Add(new KeyHandler(Key.Number1, DR1));
            keyHandlers.Add(new KeyHandler(Key.C, Recolor));
            keyHandlers.Add(new KeyHandler(Key.I, InitPlates));
            keyHandlers.Add(new KeyHandler(Key.P, GrowPlates));

            InitializeGL();
            SetCameraProjection();

            Shader texShader = new Shader(SHADER_PATH+"quadVertShader.glsl", SHADER_PATH + "texFragShader.glsl");
            Shader shader = new Shader(SHADER_PATH + "Vert3DColorUVShader.glsl", SHADER_PATH + "shadedFragShader.glsl");
            Shader pointShader = new Shader(SHADER_PATH + "pointVertShader.glsl", SHADER_PATH + "pointFragShader.glsl");
            Shader borderShader = new Shader(SHADER_PATH + "Vert3DColorShader.glsl", SHADER_PATH + "pointFragShader.glsl");
            Texture cellTexture = new Texture("Edge.png");
            world = new World();
            worldPosition = new Vector3(0, 0, -3);

            node = new Node();
            node.Model = Matrix4.CreateTranslation(worldPosition);

            worldRenderGeometry = world.geometry.GenerateDualMesh<Vertex3DColorUV>();

            worldRenderer = new Renderer(worldRenderGeometry, shader);
            worldRenderer.AddUniform(new UniformProperty("lightPosition", lightPosition));
            worldRenderer.AddUniform(new UniformProperty("ambientColor", ambientColor));
            worldRenderer.AddTexture(cellTexture);
            worldRenderer.CullFaceFlag = true;
            node.Add(worldRenderer);

            borderGeometry = world.geometry.GenerateBorderGeometry<Vertex3DColor>(world.Borders);
            borderGeometry.PrimitiveType = PrimitiveType.Triangles;
            borderRenderer = new Renderer(borderGeometry, borderShader);
            borderRenderer.DepthTestFlag = true;
            borderRenderer.CullFaceFlag = false;
            node.Add(borderRenderer);

            worldVertsDebugRenderer = new Renderer(world.geometry, pointShader);
            worldVertsDebugRenderer.AddUniform(new UniformProperty("color", new Vector4(0, 0.2f, 0.7f, 1)));
            worldVertsDebugRenderer.AddUniform(new UniformProperty("pointSize", 3f));
            node.Add(worldVertsDebugRenderer);

            Geometry<Vertex3D> centroidGeom = new Geometry<Vertex3D>(world.geometry.GenerateCentroidMesh());
            centroidGeom.PrimitiveType = PrimitiveType.Points;
            worldCentroidDebugRenderer = new Renderer(centroidGeom, pointShader);
            worldCentroidDebugRenderer.DepthTestFlag = false;
            worldCentroidDebugRenderer.CullFaceFlag = false;
            worldCentroidDebugRenderer.AddUniform(new UniformProperty("color", new Vector4(0.5f, 0, 0.5f, 1)));
            worldCentroidDebugRenderer.AddUniform(new UniformProperty("pointSize", 3f));
            node.Add(worldCentroidDebugRenderer);
        }


        void UpdateRenderers()
        {
            worldRenderGeometry = world.geometry.GenerateDualMesh<Vertex3DColorUV>();
            borderGeometry = world.geometry.GenerateBorderGeometry<Vertex3DColor>(world.Borders);
            borderRenderer.Update(borderGeometry);
            worldRenderer.Update(worldRenderGeometry);
            worldVertsDebugRenderer.Update(world.geometry);

            Geometry<Vertex3D> centroidGeom = new Geometry<Vertex3D>(world.geometry.GenerateCentroidMesh());
            centroidGeom.PrimitiveType = PrimitiveType.Points;
            worldCentroidDebugRenderer.Update(centroidGeom);
        }

        private void ResetSphere(bool down)
        {
            if(down)
            {
                world.ResetSeed();
 
                world.Initialize();
                UpdateRenderers();
            }
        }

        private void RelaxTriangles(bool down)
        {
            if (down)
            {
                world.RelaxTriangles();   
                UpdateRenderers();
            }
        }

        private void DistortTriangles(bool down)
        {
            if (down)
            {
                world.TweakTriangles();
                UpdateRenderers();
            }
        }
 
        private void DR1(bool down)
        {
            if(down)
            {
                world.Distort();
                UpdateRenderers();
            }
        }

        private void Recolor(bool down)
        {
            if(down)
            {
                world.CreatePlates();
                UpdateRenderers();
            }
        }

        private void InitPlates(bool down)
        {
            if (down)
            {
                world.InitPlates();
                UpdateRenderers();
            }
        }

        private void GrowPlates(bool down)
        {
            if (down)
            {
                world.GrowPlates();
                UpdateRenderers();
            }
        }

        void UpdateZoom(MouseButton button)
        {
            if (button.IsDown)
            {
                // drag up and down to change zoom level.
                fieldOfView += button.YDelta / 100.0f;
                fieldOfView = Clamp(fieldOfView, 0.1f, (float)Math.PI - 0.1f);
            }
        }

        void UpdateCameraPos(MouseButton button)
        {
            if (button.IsDown)
            {
                // drag up and down to change camera Z.
                cameraPosition.Z += button.YDelta / 100.0f;
                cameraPosition.Z = Clamp(cameraPosition.Z, -1, 2);
            }
        }

        void UpdateIco(MouseButton button)
        {
            if (button.IsDown)
            {
                // Actually, what you really want to do, is to unproject the mouse pointer onto the sphere, 
                // before and after movement, then slerp to new rotation.

                // Rotate around Y axis
                longitude += button.XDelta / 4.0f;
                longitude %= 360;

                attitude += button.YDelta / 2.0f;
                attitude = Math.Max(Math.Min(90, attitude), -90);

                Quaternion equatorRot = Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI * longitude / 180.0f);
                Quaternion polarAxisRot = Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI * attitude / 180.0f);
                Quaternion rotation = equatorRot * polarAxisRot;
                Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);
                Matrix4 tr = Matrix4.CreateTranslation(worldPosition);
                node.Model = rotationMatrix * tr;
            }
        }

        private void InitializeGL()
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            GL.ClearColor(System.Drawing.Color.Aquamarine);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.VertexProgramPointSize);
        }

        private void SetCameraProjection()
        {
            projection = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, Width / (float)Height, 0.1f, 100.0f);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Coords relative to drawing frame
            // var mousePos = base.PointToClient(new Point(Mouse.X, Mouse.Y));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(backgroundColor);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 view = Matrix4.LookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
            SetCameraProjection();

            node.Draw( ref view, ref projection);
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            SetCameraProjection();
        }


        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                base.Exit();
            }
            foreach (var h in keyHandlers)
            {
                h.OnKeyDown(e.Key);
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            foreach (var h in keyHandlers)
            {
                h.OnKeyUp(e.Key);
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
        }
    }
}
