using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WorldGenerator
{
    class Node
    {
        private List<Renderer> renderers = new List<Renderer>();
        public Matrix4 Model { get; set; }

        public void Add(Renderer renderer)
        {
            renderers.Add(renderer);
        }

        public void Draw(ref Matrix4 view,  ref Matrix4 projection)
        {
            foreach (var renderer in renderers)
            {
                renderer.Draw(Model, view, projection);
            }
        }
    }
}
