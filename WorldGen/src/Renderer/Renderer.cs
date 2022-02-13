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
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace WorldGen
{
    /// <summary>
    /// A renderer holds all the data necessary for drawing: VBO, optional index buffer, shader & texture.
    /// It also has a number of properties for drawing, e.g. blend options, cull face, etc.
    /// On top of that, it holds an arbitrary list of uniforms, which are written to the shader before drawing.
    /// </summary>
    class Renderer
    {
        private IVertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private Shader shader;
        private Texture texture;
        
        public bool DepthTestFlag { set; get; }
        public CullFaceMode CullFaceMode { set; get; }
        public bool CullFaceFlag { set; get; }
        public bool BlendingFlag { set; get; }
        public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;
        public bool Visible { get; set; } = true;

        private List<UniformProperty> uniforms;
        
        public Renderer(IVertexBuffer vertexBuffer, IndexBuffer indexBuffer, Shader shader)
        {
            this.DepthTestFlag = true;
            this.CullFaceMode = CullFaceMode.Back;
            this.CullFaceFlag = true;
            this.BlendingFlag = false;
            this.shader = shader;
            this.vertexBuffer = vertexBuffer;
            this.indexBuffer = indexBuffer; // Can be null
            this.uniforms = new List<UniformProperty>();
        }

        public void AddIndexBuffer(IndexBuffer indexBuffer)
        {
            this.indexBuffer = indexBuffer;
        }

        public void AddTexture(Texture texture)
        {
            this.texture = texture;
        }

        public void AddShader(Shader shader)
        {
            this.shader = shader;
        }

        public void AddUniform(UniformProperty uniform)
        {
            uniforms.Add(uniform);
        }

        public void Draw(Matrix4 model, Matrix4 view, Matrix4 projection)
        {
            if (!Visible)
                return;

            if (DepthTestFlag)
            {
                GL.Enable(EnableCap.DepthTest);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }
            if (CullFaceFlag)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }
            if( BlendingFlag )
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }
            if (shader != null)
            {
                shader.Use();

                if (texture != null)
                {
                    texture.Bind();
                    shader.SetSamplerUniform(0, 0);
                }
                else
                {
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }

                // Set up uniforms:
                Matrix4 mv = model * view;
                Matrix3 mvIT = new Matrix3(mv);
                mvIT.Invert();
                mvIT.Transpose();
                shader.SetUniformMatrix3("mvIT", mvIT);
                shader.SetUniformMatrix4("modelView", mv);
                shader.SetUniformMatrix4("projection", projection);
                shader.SetUniformMatrix4("model", model);
                shader.SetUniformMatrix4("view", view);

                foreach (var uniform in uniforms)
                {
                    uniform.SetUniform(shader);
                }
            }
            vertexBuffer.Bind(shader);

            if (indexBuffer != null)
            {
                indexBuffer.Bind();
                GL.DrawElements(PrimitiveType, indexBuffer.Size(), DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(PrimitiveType, 0, vertexBuffer.Size);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
