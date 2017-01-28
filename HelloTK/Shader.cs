using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;

namespace HelloTK
{
    class Shader
    {
        int programId;
        int vsId;
        int fsId;
        List<Tuple<string, int>> attrLocs = new List<Tuple<string, int>>();
        List<Tuple<string, int>> uniformLocs = new List<Tuple<string, int>>(); //Todo add type system to uniform

        public Shader(string vertexShader, string fragmentShader)
        {
            programId = GL.CreateProgram();
            
            // Are these paths or shader strings?
            // Let's assume they're filenames:

            LoadShader(vertexShader, ShaderType.VertexShader, programId, out vsId);
            LoadShader(fragmentShader, ShaderType.FragmentShader, programId, out fsId);
            GL.LinkProgram(programId);
            Console.WriteLine(GL.GetProgramInfoLog(programId));
            GL.DetachShader(programId, fsId);
            GL.DetachShader(programId, vsId);

            // Could do with introspecting the attrs, uniforms and samplers
 
            AddUniform("modelView");
            AddUniform("projection");
        }

        // Todo Remember how when we were younger, we did templating for this in our sleep?
        public void SetUniformMatrix4(string uniform, Matrix4 matrix )
        {
            int loc = GetUniformLoc(uniform);
            if( loc != -1)
            {
                GL.UniformMatrix4(loc, false, ref matrix);
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

        private void LoadShader(string filename, ShaderType type, int program, out int id)
        {
            id = GL.CreateShader(type);
            string shaderSource;
            using (StreamReader sr = new StreamReader(filename))
            {
                shaderSource = sr.ReadToEnd();
            }
            GL.ShaderSource(id, shaderSource);
            GL.CompileShader(id);
            int errorCode=-1;
            GL.GetShader(id, ShaderParameter.CompileStatus, out errorCode);
            if( errorCode != 1)
            {
                Console.WriteLine("LoadShader(" + filename + ")\n" + shaderSource + "\nError code:" + errorCode + "\n:"+ GL.GetShaderInfoLog(id));
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
        private void AddUniform(string name)
        {
            uniformLocs.Add(new Tuple<string, int>(name, GL.GetUniformLocation(programId, name)));
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
            return -1;
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
