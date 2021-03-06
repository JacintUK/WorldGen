﻿/*
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
    struct Vertex3DColor : IVertex, IPositionVertex, IColorVertex
    {
        public Vector3 position;
        public Vector4 color;

        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3 },
                new Attribute() { Name = "aColor",    Type = Attribute.AType.VECTOR4 }
                });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }

        public Color Color 
        {
            get { return Color.FromArgb((int)(color.W*255), (int)(color.X*255), (int)(color.Y*255), (int)(color.Z*255) ); }
            set { this.color = new Vector4(value.R/255.0f, value.G/255.0f, value.B/255.0f, value.A/255.0f); }
        }

        public Vertex3DColor(Vector3 position, Vector4 color)
        {
            this.position = position;
            this.color = color;
        }
        public Vertex3DColor(Vector3 position, Color color)
        {
            this.position = position;
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
        public Vector4 GetColor()
        {
            return this.color;
        }
        public void SetColor(Vector4 color)
        {
            this.color = color;
        }
    }
}
