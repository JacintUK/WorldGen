
using OpenTK;
using System.Collections;
using System.Collections.Generic;

namespace WorldGenerator
{
    internal struct Attribute
    {
        public enum AType { FLOAT, VECTOR2, VECTOR3, VECTOR4 };

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

            }
            return size;
        }
        public static int NumberOfFloatsInType(Attribute.AType type)
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

            }
            return size;
        }
    }
}