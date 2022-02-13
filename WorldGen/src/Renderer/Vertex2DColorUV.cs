/*
 * Copyright 2019 David Ian Steele
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
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Mathematics;

namespace WorldGen
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct LowColor
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public LowColor(byte _r, byte _g, byte _b, byte _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }

        public LowColor(float r1, float g1, float b1, float a1) : this()
        {
            r = (byte)(r1*255.0f);
            g = (byte)(g1*255.0f);
            b = (byte)(b1*255.0f);
            a = (byte)(a1*255.0f);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Vertex2DColorUV : IVertex, IPosition2DVertex, ITextureCoordinateVertex, ILowColorVertex
    {
        public static LowColor WHITE = new LowColor(0xff, 0xff, 0xff, 0xff);
        public static LowColor BLACK = new LowColor(0x00, 0x00, 0x00, 0xff);
        public static LowColor RED   = new LowColor(0xff, 0x00, 0x00, 0xff);
        public static LowColor GREEN = new LowColor(0x00, 0xff, 0x00, 0xff);
        public static LowColor BLUE  = new LowColor(0x00, 0x00, 0xff, 0xff);

        public Vector2 position;
        public Vector2 uv;
        public LowColor color;

        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR2},
                new Attribute() { Name = "aTexCoords", Type = Attribute.AType.VECTOR2},
                new Attribute() { Name = "aColor", Type = Attribute.AType.UBYTE4} });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }

        public Color4 Color 
        {
            get { return new Color4(color.r, color.g, color.b, color.a); }
            set { this.color = new LowColor(value.R, value.G, value.B, value.A); }
        }

        public Vertex2DColorUV(Vector2 position, Vector2 uv, LowColor color)
        {
            this.position = position;
            this.uv = uv;
            this.color = color;
        }

        public Vector2 GetPosition()
        {
            return this.position;
        }
        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }

        public void SetTextureCoordinates(Vector2 texCoords)
        {
            this.uv = texCoords;
        }
        public Vector2 GetTextureCoordinates()
        {
            return this.uv;
        }
        public void SetColor(LowColor color)
        {
            this.color = color;
        }
        public LowColor GetColor()
        {
            return this.color;
        }
    }
}
