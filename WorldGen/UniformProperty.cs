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
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;

namespace WorldGenerator
{
    class UniformProperty
    {
        private string name;
        private object value;

        public string Name { set { name = value; } get { return name; } }
        public object Value {  set { this.value = value; } }

        public UniformProperty(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetUniform(Shader shader)
        {
            Type t = value.GetType();
            if(t == typeof(int))
            {
                shader.SetUniform(name, (int)value);
            }
            else if( t== typeof(float))
            {
                shader.SetUniform(name, (float)value);
            }
            else if (t == typeof(Vector2))
            {
                shader.SetUniformVector2(name, (Vector2)value);
            }
            else if (t == typeof(Vector3))
            {
                shader.SetUniformVector3(name, (Vector3)value);
            }
            else if (t == typeof(Vector4))
            {
                shader.SetUniformVector4(name, (Vector4)value);
            }
            else if (t == typeof(Matrix3))
            {
                shader.SetUniformMatrix3(name, (Matrix3)value);
            }
            else if (t == typeof(Matrix4))
            {
                shader.SetUniformMatrix4(name, (Matrix4)value);
            }
            else if (t == typeof(Color4))
            {
                shader.SetUniformVector4(name, (Color4)value);
            }
            else if (t == typeof(Color))
            {
                Color c = (Color)value;
                shader.SetUniformVector4(name, new Color4(c.R, c.G, c.B, c.A));
            }
        }
    }
}
