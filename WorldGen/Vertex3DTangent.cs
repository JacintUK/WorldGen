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
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WorldGenerator
{
    struct Vertex3DTangent : IVertex, IPositionVertex, ITangentVertex
    {
        public Vector3 position;
        public Vector3 tangent;
        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3 },
                new Attribute() { Name = "aTangent", Type = Attribute.AType.VECTOR3 },
                });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }
        public static int SizeInBytes { get { return Vector3.SizeInBytes; } }

        public Vertex3DTangent(Vector3 position )
        {
            this.position = position;
            this.tangent = Vector3.UnitZ;
        }
        public Vector3 GetPosition()
        {
            return this.position;
        }
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
        public Vector3 GetTangent()
        {
            return tangent;
        }
        public void SetTangent(Vector3 tangent)
        {
            this.tangent = tangent;
        }
    }
}
