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
        private Renderer background;

        public event EventHandler<EventArgs> SceneUpdatedEvent;
        private void RaiseSceneUpdatedEvent()
        {
            SceneUpdatedEvent?.Invoke(this, new EventArgs());
        }

        public Scene(float width, float height)
        {
            camera = new Camera
            {
                Position = new Vector3(0.0f, 0.0f, 3.0f),
                Width = width,
                Height = height
            };
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
            Matrix4 projection = camera.Projection;

            background?.Draw(Matrix4.Identity, view, projection);
            foreach (var node in nodes)
            {
                node.Draw(ref view, ref projection);
            }
        }
    }
}
