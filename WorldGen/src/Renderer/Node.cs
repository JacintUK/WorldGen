/*
 * Copyright 2018 David Ian Steele
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Data;
using OpenTK.Mathematics;

namespace WorldGen
{
    /// <summary>
    /// A node is a class which has a world position and a number of renderers.
    /// </summary>
    class Node
    {
        private List<IGeometryRenderer> renderers = new();

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Matrix4 Model { get; private set; } // TODO Make passing this performant.
        
        public Node()
        {
        }

        /// <summary>
        /// Update the model matrix from the current position/rotation/scale.
        /// </summary>
        public void Update()
        {
            Matrix4 tr = Matrix4.CreateTranslation(Position);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Rotation);
            Model = rotationMatrix * tr;
        }
        public List<IGeometryRenderer> Renderers { get { return renderers; } }

        public void Add(IGeometryRenderer renderer) 
        {
            renderers.Add(renderer);
        }

        public void Draw(ref Matrix4 view, ref Matrix4 projection)
        {
            foreach (var renderer in renderers)
            {
                renderer.Renderer.Draw(Model, view, projection);
            }
        }
    }
}
