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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace WorldGenerator
{
    class IndexBuffer
    {
        uint[] indices;
        int bufferHandle;
        int numIndices;
        bool uploaded=false;

        public IndexBuffer(uint[] indices)
        {
            numIndices = indices.Length;
            this.indices = indices;
            bufferHandle = GL.GenBuffer();
        }

        public void Upload( uint[] newIndices )
        {
            indices = newIndices;
            numIndices = newIndices.Length;
            uploaded = false;
        }
        public void Bind()
        {
            GL.EnableClientState(ArrayCap.IndexArray);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferHandle);
            if( !uploaded )
            {
                uploaded = true;
                GL.BufferData<uint>(BufferTarget.ElementArrayBuffer,
                       (IntPtr)(sizeof(uint) * indices.Length),
                       indices, BufferUsageHint.StaticDraw);

            }
        }

        public int Size()
        {
            return numIndices;
        }
    }
}
