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

using OpenTK.Mathematics;
using System.Collections.Generic;

namespace WorldGen
{
    struct Vertex : IVertex
    {
        public Vector3 position;
        public Vector2 texCoords;
        public Color4 color;

        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3},
                new Attribute() { Name = "aTexCoords", Type = Attribute.AType.VECTOR2},
                new Attribute() { Name = "aColor", Type = Attribute.AType.VECTOR4} });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }

        public Color4 Color 
        {
            get { return this.color; }
            set { this.color = value; }
        }

        public Vertex(Vector3 position, Vector2 texCoords, Vector4 color)
        {
            this.position = position;
            this.texCoords = texCoords;
            this.color = new Color4(color.X, color.Y, color.Z, color.W);
        }
        public Vertex(Vector3 position, Vector2 texCoords, Color4 color)
        {
            this.position = position;
            this.texCoords = texCoords;
            this.color = color;
        }
    }
}
