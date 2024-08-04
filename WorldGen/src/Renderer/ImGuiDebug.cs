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


using ImGuiNET;
using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Mathematics;

namespace WorldGen
{
    class ImGuiDebug
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct VTest
        {
            public float PX;
            public float PY;
            public float U;
            public float V;
            public byte r;
            public byte g;
            public byte b;
            public byte a;
        }
        private unsafe void WriteVertexData(byte* buffer, int elements)
        {
            Console.WriteLine("...");
            byte* vtx_buffer = buffer;

            for (int i = 0; i < elements; ++i)
            {
                Vertex2DColorUV* vertPtr = (Vertex2DColorUV*)vtx_buffer;
                Console.WriteLine("((({0:f}, {1:f}), ({2:f},{3:f}), (0x{4:x2},{5:x2},{6:x2},{7:x2}))",
                    vertPtr->position.X, vertPtr->position.Y, vertPtr->uv.X, vertPtr->uv.Y,
                    vertPtr->color.r, vertPtr->color.g, vertPtr->color.b, vertPtr->color.a);
                vtx_buffer += sizeof(Vertex2DColorUV);
            }
        }

        private unsafe void WriteIndexData(ushort[] buffer, uint elementCount)
        {
            for (int i = 0; i < elementCount; i++)
            {
                if (i > 0)
                    Console.Write(", ");
                Console.Write("{0:d}", buffer[i]);
            }
            Console.WriteLine();
        }
        private unsafe void WriteVertexData3(byte* buffer, int elements)
        {
            Console.WriteLine("...");
            byte* vtx_buffer = buffer;


            for (int i = 0; i < elements; ++i)
            {
                VTest* vertPtr = (VTest*)vtx_buffer;
                Console.WriteLine("({0:f}, {1:f}), ({2:f},{3:f}), 0x{4:x2}{5:x2}{6:x2}{7:x2}h",
                    vertPtr->PX, vertPtr->PY, vertPtr->U, vertPtr->V,
                    vertPtr->r, vertPtr->g, vertPtr->b, vertPtr->a);
                vtx_buffer += sizeof(VTest);
            }
        }
        private unsafe void WriteVertexData2(byte* buffer, int elements)
        {
            Console.WriteLine("...");
            byte* vtx_buffer = buffer;

            for (int i = 0; i < elements; ++i)
            {
                Console.WriteLine("({0:f}, {1:f}), ({2:f},{3:f}), 0x{4:x2}{5:x2}{6:x2}{7:x2}h",
                    *(float*)vtx_buffer, *(float*)(vtx_buffer + sizeof(float)),
                    *(float*)(vtx_buffer + 2 * sizeof(float)), *(float*)(vtx_buffer + 3 * sizeof(float)),
                    (int)*(vtx_buffer + 4 * sizeof(float)), (int)*(vtx_buffer + 1 + 4 * sizeof(float)),
                    (int)*(vtx_buffer + 2 + 4 * sizeof(float)), (int)*(vtx_buffer + 3 + 4 * sizeof(float)));
                vtx_buffer += 4 * sizeof(float) + 4;
            }
        }

        private unsafe void AddTestNode1(Scene scene)
        {
            Shader shader = new Shader(GameWindow.SHADER_PATH + "Vert2DColorUVShader.glsl", GameWindow.SHADER_PATH + "4channelFragShader.glsl");
            var texture = new Texture("CellCorner.png");
            var node = new Node { Position = new Vector3(0, 0, -3) };
            node.Update();

            var renderer = GeometryRenderer<Vertex2DColorUV>.NewQuad(shader);
            renderer.Renderer.AddTexture(texture);
            renderer.Renderer.BlendingFlag = true;
            node.Add(renderer);
            scene.Add(node);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct VTest2
        {
            public float PX1;
            public float PY1;
            public float U1;
            public float V1;
            public byte r1;
            public byte g1;
            public byte b1;
            public byte a1;
            public float PX2;
            public float PY2;
            public float U2;
            public float V2;
            public byte r2;
            public byte g2;
            public byte b2;
            public byte a2;
            public float PX3;
            public float PY3;
            public float U3;
            public float V3;
            public byte r3;
            public byte g3;
            public byte b3;
            public byte a3;
            public float PX4;
            public float PY4;
            public float U4;
            public float V4;
            public byte r4;
            public byte g4;
            public byte b4;
            public byte a4;
        }

        private static unsafe byte* Serialize(VTest2 data)
        {
            int len = Marshal.SizeOf(data);

            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(data, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);
            fixed (byte* d = &arr[0])
            {
                return d;
            }
        }

        private static unsafe byte* Serialize(Vertex2DColorUV[] data)
        {
            int len = sizeof(Vertex2DColorUV) * data.Length;

            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(data, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);
            fixed (byte* d = &arr[0])
            {
                return d;
            }
        }

        private unsafe void AddTestNode(Scene scene)
        {
            Shader shader = new Shader(GameWindow.SHADER_PATH + "Vert2DColorUVShader.glsl", GameWindow.SHADER_PATH + "pointTexFragShader.glsl");
            var texture = new Texture("Arrow.png");
            var node = new Node { Position = new Vector3(0, 0, -3) };
            node.Update();

            VTest2 quad = new VTest2();
            quad.PX1 = -0.5f; quad.PY1 = -0.5f; quad.U1 = 0f; quad.V1 = 1f; quad.r1 = 0xff; quad.g1 = 0xff; quad.b1 = 0xff; quad.a1 = 0xff;
            quad.PX2 = 0.5f; quad.PY2 = -0.5f; quad.U2 = 1f; quad.V2 = 1f; quad.r2 = 0x00; quad.g2 = 0x00; quad.b2 = 0xff; quad.a2 = 0xff;
            quad.PX3 = -0.5f; quad.PY3 = 0.5f; quad.U3 = 0f; quad.V3 = 0f; quad.r3 = 0x00; quad.g3 = 0xff; quad.b3 = 0x00; quad.a3 = 0xff;
            quad.PX4 = 0.5f; quad.PY4 = 0.5f; quad.U4 = 1f; quad.V4 = 0f; quad.r4 = 0xff; quad.g4 = 0x00; quad.b4 = 0x00; quad.a4 = 0xff;
            Vertex2DColorUV vertex = new Vertex2DColorUV();
            var buffer = Serialize(quad);
            var vertexBuffer = new VertexBuffer<Vertex2DColorUV>(buffer, sizeof(VTest2), vertex.GetVertexFormat());
            uint[] indices = new uint[6] { 0, 1, 2, 2, 1, 3 };
            var indexBuffer = new IndexBuffer(indices);
            var renderer = new Renderer(vertexBuffer, indexBuffer, shader);
            renderer.AddTexture(texture);
            renderer.BlendingFlag = true;
            renderer.CullFaceFlag = false;
            var geometryRenderer = new GeometryRenderer<Vertex2DColorUV>(renderer);
            node.Add(geometryRenderer);
            scene.Add(node);
        }
        
    }
}
