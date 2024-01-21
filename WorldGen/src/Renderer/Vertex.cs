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
    struct Vertex : IVertex, IPositionVertex, ITextureCoordinateVertex, IColorVertex
    {
        public Vector3 position;
        public Vector2 texCoords;
        public Vector4 color;

        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3},
                new Attribute() { Name = "aTexCoords", Type = Attribute.AType.VECTOR2},
                new Attribute() { Name = "aColor", Type = Attribute.AType.VECTOR4} });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }

        public Vector4 Color 
        {
            get { return this.color; }
            set { this.color = value; }
        }

        public Vertex(Vector3 position, Vector2 texCoords, Vector4 color)
        {
            this.position = position;
            this.texCoords = texCoords;
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
 
        public void SetTextureCoordinates(Vector2 texCoords)
        {
            this.texCoords = texCoords;
        }
        public void SetColor(Vector4 color)
        {
            this.color = color;
        }
        public Vector4 GetColor()
        {
            return color;
        }
        public Vector2 GetTextureCoordinates()
        {
            return this.texCoords;
        }
    }
}
