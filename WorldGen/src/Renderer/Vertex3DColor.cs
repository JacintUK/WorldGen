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
using OpenTK;
using OpenTK.Mathematics;

namespace WorldGen
{
    struct Vertex3DColor : IVertex, IPositionVertex, IColorVertex
    {
        public Vector3 position;
        public Color4 color;

        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3 },
                new Attribute() { Name = "aColor",    Type = Attribute.AType.VECTOR4 }
                });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }

        public Color4 Color 
        {
            get { return color; }
            set { color = value; }
        }

        public Vertex3DColor(Vector3 position, Vector4 color)
        {
            this.position = position;
            this.color = new Color4(color.X, color.Y, color.Z, color.W);
        }
        public Vertex3DColor(Vector3 position, Color4 color)
        {
            this.position = position;
            this.color = color; 
        }
        public Vector3 GetPosition()
        {
            return this.position;
        }
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
        public Vector4 GetColor()
        {
            return (Vector4)this.color;
        }
        public void SetColor(Vector4 color)
        {
            this.color = new Color4(color.X, color.Y, color.Z, color.W);
        }
    }
}
