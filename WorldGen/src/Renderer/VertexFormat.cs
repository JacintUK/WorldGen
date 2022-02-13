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

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace WorldGen
{
    internal struct Attribute
    {
        public enum AType { FLOAT, VECTOR2, VECTOR3, VECTOR4, UBYTE4 };

        string name;
        AType type;
        int offset;
        public string Name { set { name = value; } get { return name; } }
        public AType Type { set { type = value; } get { return type; } }
        public int Offset { set { offset = value; } get { return offset; } }
    }
    internal class VertexFormat
    {
        List<Attribute> attributes;
        public readonly int size;
        public List<Attribute> Attributes { get { return attributes; } }

        public VertexFormat( List<Attribute> attributes )
        {
            this.attributes = attributes;
            int offset = 0;
            for(int i=0; i<this.attributes.Count;++i)
            {
                Attribute attr = this.attributes[i];
                attr.Offset = offset;
                this.attributes[i] = attr;
                offset += TypeSizeInBytes(attr.Type);
            }
            size = offset;
        }

        public int FindOffset(string name)
        {
            for (int i = 0; i < this.attributes.Count; ++i)
            {
                if (attributes[i].Name == name)
                    return attributes[i].Offset;
            }
            return -1;
        }
        public static VertexAttribPointerType BaseType(Attribute.AType type)
        {
            return (type == Attribute.AType.UBYTE4) ? VertexAttribPointerType.UnsignedByte : VertexAttribPointerType.Float;
        }

        public int TypeSizeInBytes(Attribute.AType type)
        {
            int size = 0;
            switch(type)
            {
                case Attribute.AType.FLOAT:
                {
                    size = sizeof(float);
                    break;
                }
                case Attribute.AType.VECTOR2:
                {
                    size = Vector2.SizeInBytes;
                    break;
                }
                case Attribute.AType.VECTOR3:
                {
                    size = Vector3.SizeInBytes;
                    break;
                }
                case Attribute.AType.VECTOR4:
                {
                    size = Vector4.SizeInBytes;
                    break;
                }
                case Attribute.AType.UBYTE4:
                {
                    size = 4;
                    break;
                }
            }
            return size;
        }
        public static int NumberOfElementsInType(Attribute.AType type)
        {
            int size = 0;
            switch (type)
            {
                case Attribute.AType.FLOAT:
                    {
                        size = 1;
                        break;
                    }
                case Attribute.AType.VECTOR2:
                    {
                        size = 2;
                        break;
                    }
                case Attribute.AType.VECTOR3:
                    {
                        size = 3;
                        break;
                    }
                case Attribute.AType.VECTOR4:
                    {
                        size = 4;
                        break;
                    }
                case Attribute.AType.UBYTE4:
                    {
                        size = 4;
                        break;
                    }
            }
            return size;
        }
    }
}