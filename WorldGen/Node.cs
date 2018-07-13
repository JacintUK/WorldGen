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
