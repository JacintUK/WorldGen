using System;
using System.Collections.Generic;
using System.Text;

namespace WorldGen
{ 
    interface IIndices
    {
        uint this[int i] { get; }
    }

    class Indices : IIndices
    {
        private uint[] indices;
        public uint this[int i] { get { return indices[i]; } set { indices[i] = value; } }
        public uint[] IndexArray { get { return indices; } }
        public int Length { get { return indices.Length; } }

        public Indices(uint[] indices)
        {
            this.indices = indices;
        }

        public Indices Clone()
        {
            uint[] newIndices = null;
            if (indices != null)
            {
                newIndices = (uint[])indices.Clone();
            }
            return new Indices(newIndices);
        }

        public IndexBuffer NewIndexBuffer()
        {
            return new IndexBuffer(indices);
        }

        public void Upload(IndexBuffer ibo)
        {
            ibo.Upload(indices);
        }
    }

}
