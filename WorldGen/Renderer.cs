using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace WorldGenerator
{
    class Renderer
    {
        private IVertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private IGeometry geometry;
        private Shader shader;
        private Texture texture;
        
        public bool DepthTestFlag { set; get; }
        public CullFaceMode CullFaceMode { set; get; }
        public bool CullFaceFlag { set; get; }
        public bool BlendingFlag { set; get; }

        private List<UniformProperty> uniforms;
        
        public Renderer(IGeometry geometry, Shader shader)
        {
            this.DepthTestFlag = true;
            this.CullFaceMode = CullFaceMode.Back;
            this.CullFaceFlag = true;
            this.BlendingFlag = false;
            this.shader = shader;
            this.geometry = geometry;
            this.vertexBuffer = geometry.CreateVertexBuffer();
            this.indexBuffer = geometry.CreateIndexBuffer(); // returns null if there are no indices
            this.uniforms = new List<UniformProperty>();
        }

        public void Update( IGeometry geometry )
        {
            this.geometry = geometry;
            if (geometry != null)
            {
                geometry.Upload(this.vertexBuffer, this.indexBuffer);
            }
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
            if (geometry != null)
            {
                if (geometry.NeedsUpdate)
                {
                    geometry.Upload(vertexBuffer, indexBuffer);
                }

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
                    GL.DrawElements(geometry.PrimitiveType, indexBuffer.Size(), DrawElementsType.UnsignedInt, 0);
                }
                else
                {
                    GL.DrawArrays(geometry.PrimitiveType, 0, vertexBuffer.Size);
                }

                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }
    }
}
