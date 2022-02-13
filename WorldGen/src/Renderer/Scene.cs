using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace WorldGen
{
    /// <summary>
    /// A scene holds a list of nodes (note, not a tree) and a camera. It has methods for updating and rendering the scene.
    /// </summary>
    class Scene
    {
        List<Node> nodes = new List<Node>();

        public Camera camera { get; set; }


        public event EventHandler<EventArgs> SceneUpdatedEvent;
        private void RaiseSceneUpdatedEvent()
        {
            SceneUpdatedEvent?.Invoke(this, new EventArgs());
        }

        public Scene(float width, float height)
        {
            camera = new Camera
            {
                Position = Vector3.UnitZ * 2.0f,
                Width = width,
                Height = height
            };
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
            camera.Update();
            RaiseSceneUpdatedEvent();
        }

        public void Render()
        {
            Matrix4 view = camera.View;
            Matrix4 projection = camera.Projection;

            foreach (var node in nodes)
            {
                node.Draw(ref view, ref projection);
            }
        }
    }
}
