using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;

namespace WorldGenerator
{
    class Shader
    {
        int programId;
        int vsId;
        int fsId;
        string vertexShaderSource;
        string fragmentShaderSource;

        List<Tuple<string, int>> attrLocs = new List<Tuple<string, int>>();
        List<Tuple<string, int>> uniformLocs = new List<Tuple<string, int>>(); 
        List<Tuple<string, int>> samplerLocs = new List<Tuple<string, int>>();

        public Shader(string vertexShader, string fragmentShader)
        {
            programId = GL.CreateProgram();

            // Are these paths or shader strings?
            // Let's assume they're filenames:

            using (StreamReader sr = new StreamReader(vertexShader))
            {
                vertexShaderSource = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader(fragmentShader))
            {
                fragmentShaderSource = sr.ReadToEnd();
            }
            LoadShader(vertexShaderSource, ShaderType.VertexShader, programId, out vsId);
            LoadShader(fragmentShaderSource, ShaderType.FragmentShader, programId, out fsId);
            GL.LinkProgram(programId);
            Console.WriteLine(GL.GetProgramInfoLog(programId));
            GL.DetachShader(programId, fsId);
            GL.DetachShader(programId, vsId);
        }

        void GetActiveSamplerUniforms()
        {
            int numActiveUniforms;
            int uniformMaxNameLength;
            GL.GetProgram(programId, GetProgramParameterName.ActiveUniforms, out numActiveUniforms);
            GL.GetProgram(programId, GetProgramParameterName.ActiveUniformMaxLength, out uniformMaxNameLength);

            for (int i = 0; i < numActiveUniforms; ++i)
            {
                int size;
                ActiveUniformType type;
                GL.GetActiveUniform(programId, i, out size, out type);
                if(type == ActiveUniformType.Sampler2D || type == ActiveUniformType.SamplerCube)
                {
                    string name = GL.GetActiveUniformName(programId, i);
                    int location = GL.GetUniformLocation(programId, name);
                    samplerLocs.Add(new Tuple<string, int>(name, location));
                }
            }
        }

        public bool GetSamplerUniformLocation(int index, out int location)
        {
            bool result = false;
            location = -1;
            if( index < samplerLocs.Count )
            {
                location = samplerLocs[index].Item2;
                result = true;
            }
            return result;
        }

        public void SetSamplerUniform(int index, int textureUnit)
        {
            int location;
            if (GetSamplerUniformLocation(index, out location))
            {
                GL.Uniform1(location, textureUnit);
            }
        }

        public void SetUniformMatrix4(string uniform, Matrix4 matrix )
        {
            int loc = GetUniformLoc(uniform);
            if( loc != -1)
            {
                GL.UniformMatrix4(loc, false, ref matrix);
            }
        }
        public void SetUniformMatrix3(string uniform, Matrix3 matrix)
        {
            int loc = GetUniformLoc(uniform);
            if (loc != -1)
            {
                GL.UniformMatrix3(loc, false, ref matrix);
            }
        }
        public void SetUniform(string uniform, int value)
        {
            int loc = GetUniformLoc(uniform);
            if( loc != -1 )
            {
                GL.Uniform1(loc, value);
            }
        }
        public void SetUniform(string uniform, float value)
        {
            int loc = GetUniformLoc(uniform);
            if (loc != -1)
            {
                GL.Uniform1(loc, value);
            }
        }
        public void SetUniformVector2(string uniform, Vector2 vector)
        {
            int loc = GetUniformLoc(uniform);
            if (loc != -1)
            {
                GL.Uniform2(loc, ref vector);
            }
        }
        public void SetUniformVector3(string uniform, Vector3 vector)
        {
            int loc = GetUniformLoc(uniform);
            if (loc != -1)
            {
                GL.Uniform3(loc, ref vector);
            }
        }
        public void SetUniformVector4(string uniform, Vector4 vector)
        {
            int loc = GetUniformLoc(uniform);
            if (loc != -1)
            {
                GL.Uniform4(loc, ref vector);
            }
        }
        public void SetUniformVector4(string uniform, Color4 vector)
        {
            int loc = GetUniformLoc(uniform);
            if (loc != -1)
            {
                GL.Uniform4(loc, vector);
            }
        }
        public void EnableAttr(string attr)
        {
            int loc = GetAttrLoc(attr);
            if( loc != -1)
            {
                GL.EnableVertexAttribArray(loc);
            }
        }
        public void DisableAttr(string attr)
        {
            int loc = GetAttrLoc(attr);
            if (loc != -1)
            {
                GL.DisableVertexAttribArray(loc);
            }
        }

        public void Use()
        {
            GL.UseProgram(programId);
        }

        private void LoadShader(string shaderSource, ShaderType type, int program, out int id)
        {
            id = GL.CreateShader(type);

            GL.ShaderSource(id, shaderSource);
            GL.CompileShader(id);
            int errorCode=-1;
            GL.GetShader(id, ShaderParameter.CompileStatus, out errorCode);
            if( errorCode != 1)
            {
                Console.WriteLine("LoadShader()\n" + shaderSource + "\nError code:" + errorCode + "\n:"+ GL.GetShaderInfoLog(id));
            }
            GL.AttachShader(program, id);
        }

        private int AddAttr( string name )
        {
            int loc = GL.GetAttribLocation(programId, name);
            if (loc == -1)
            {
                Console.WriteLine("Shader::AddAttr(" + name + ") failed to get location");
            }
            else
            {
                attrLocs.Add(new Tuple<string, int>(name, loc));
            }
            return loc;
        }

        private int GetUniformLoc(string name)
        {
            foreach ( Tuple<string, int> tuple in uniformLocs )
            {
                if(tuple.Item1 == name)
                {
                    return tuple.Item2;
                }
            }
            int loc = GL.GetUniformLocation(programId, name);
            uniformLocs.Add(new Tuple<string, int>(name, loc));
            return loc;
        }
        public int GetAttrLoc(string name)
        {
            foreach (Tuple<string, int> tuple in attrLocs)
            {
                if (tuple.Item1 == name)
                {
                    return tuple.Item2;
                }
            }
            return AddAttr(name);
        }

        private void AddSampler(string name)
        {
            // todo: Add sampler introspection
        }
    }
}
