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
using OpenTK.Mathematics;

namespace WorldGen
{
    struct Vertex3DColorUV : IVertex, INormalVertex, IPositionVertex, ITextureCoordinateVertex, IColorVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
        public Vector4 color;

        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3},
                new Attribute() { Name = "aNormal", Type = Attribute.AType.VECTOR3},
                new Attribute() { Name = "aTexCoords", Type = Attribute.AType.VECTOR2},
                new Attribute() { Name = "aColor", Type = Attribute.AType.VECTOR4} });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }

        public Color4 Color
        {
            get { return new Color4(color.X, color.Y, color.Z, color.W); }
            set { this.color = (Vector4)value; }
        }

        public Vertex3DColorUV(Vector3 position, Vector3 normal, Vector2 uv, Vector4 color)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
            this.color = color;
        }
        public Vertex3DColorUV(Vector3 position, Vector3 normal, Vector2 uv, Color4 color)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
            this.color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f); 
        }
        public Vector3 GetPosition()
        {
            return this.position;
        }
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
        public Vector3 GetNormal()
        {
            return this.normal;
        }
        public void SetNormal(Vector3 normal)
        {
            this.normal = normal;
        }
        public void SetTextureCoordinates(Vector2 texCoords)
        {
            this.uv = texCoords;
        }
        public void SetColor(Vector4 color)
        {
            this.color = color;
        }
        public Vector4 GetColor()
        {
            return this.color;
        }
        public Vector2 GetTextureCoordinates()
        {
            return this.uv;
        }
    }
}
