using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;

namespace WorldGen
{
    /// <summary>
    /// A scene holds a list of nodes (note, not a tree) and a camera. It has methods for updating and rendering the scene.
    /// </summary>
    class Scene
    {
        List<Node> nodes = new List<Node>();

        public Camera camera { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Yaw { get; set; } = 90.0f;
        public float Pitch { get; set; } = 0.0f;

        private Renderer background;

        public event EventHandler<EventArgs> SceneUpdatedEvent;
        private void RaiseSceneUpdatedEvent()
        {
            SceneUpdatedEvent?.Invoke(this, new EventArgs());
        }

        public bool HitTestFailed { get; set; }

        private Vector3 _rayDirection=new Vector3();
        public Vector3 RayDirection { get { return _rayDirection; } }

        public Scene(float width, float height)
        {
            camera = new Camera
            {
                Width = width,
                Height = height,
                DistanceFromObject = 5,
                Projection = Camera.ProjectionType.Perspective,
                Mode = Camera.ModeType.Target,
                TargetPosition = new Vector3(0.0f, 0.0f, 0.0f)
            };
            UpdateCameraPos();
            Width = (int)width;
            Height = (int)height;
        }

        public void SetBackground(string filename)
        {
            var tx = new Texture(filename, Width, Height);
            var shader = new Shader(GameWindow.SHADER_PATH + "quadBlitVertShader.glsl", GameWindow.SHADER_PATH + "4ChannelFragShader.glsl");
            background = RendererFactory.CreateQuad(shader);
            background.AddTexture(tx);
            background.DepthTestFlag = false;
        }

        public void Add(Node node)
        {
            nodes.Add(node);
        }

        public Node GetRootNode()
        {
            return nodes[0];
        }

        public void UpdateCameraPos()
        {
            Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);
            var yawRad = Math.PI * Yaw / 180.0;
            var pitchRad = Math.PI * Pitch / 180.0;

            var x = camera.DistanceFromObject * (float)Math.Cos(yawRad) * (float)Math.Cos(pitchRad);
            var y = camera.DistanceFromObject * (float)Math.Sin(pitchRad);
            var z = camera.DistanceFromObject * (float)Math.Sin(yawRad) * (float)Math.Cos(pitchRad);
            camera.Position = new Vector3(x, y, z);
            camera.Update();
        }
        
        /// <summary>
        /// Select a tile on the world.
        /// </summary>
        /// <param name="screenCoords">coordinates of the mouse in the window</param>
        public void HitTest(Vector2i screenCoords)
        {
            Vector3 rayOrigin = new Vector3();
            camera.BuildPickingRay(screenCoords, ref rayOrigin, ref _rayDirection);

            foreach( var node in nodes)
            {
                foreach(var render in node.Renderers)
                {
                    if (render.HitTest(node.Model, rayOrigin, _rayDirection))
                    {
                        // Event handler invoked in callee.
                        HitTestFailed = false;
                        return; // don't do any more hit testing
                    }
                }
            }
            HitTestFailed = true;
        }

        public void Update(float width, float height)
        {
            camera.Width = width;
            camera.Height = height;
            Update();
        }

        public void Update()
        {
            foreach(Node node in nodes)
            {
                node.Update();
            }
            camera.Update();
            RaiseSceneUpdatedEvent();
        }

        public void Render()
        {
            Matrix4 view = camera.View;
            Matrix4 projection = camera.ProjectionMatrix;

            background?.Draw(Matrix4.Identity, view, projection);
            foreach (var node in nodes)
            {
                node.Draw(ref view, ref projection);
            }
        }
    }
}
