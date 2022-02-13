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
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace WorldGen
{
    internal class Texture : IDisposable
    {
        private const string IMAGE_PATH = "Resources/Images/";
        private readonly int handle;
        public string name;
        public int Handle { get { return handle; } }

        public Texture(string filename) 
        {
            if( !File.Exists(filename) && !File.Exists(IMAGE_PATH+filename) )
            {
                Console.WriteLine("Texture(" + filename + ") Filename doesn't exist");
                throw new FileNotFoundException("Texture(" + filename + ") Filename doesn't exist");
            }
            name = filename;
            handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            var bmp = new System.Drawing.Bitmap(IMAGE_PATH + filename);
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        // 1 channel texture
        public Texture(string name, int width, int height, int bytesPerPixel, IntPtr pixelData)
        {
            this.name = name;
            handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, pixelData);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            GL.DeleteTexture(handle);
        }

        public void Bind()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        private TextureMagFilter magFilter=TextureMagFilter.Linear;
        private TextureMinFilter minFilter=TextureMinFilter.Linear;
        public void SetMagFilter(TextureMagFilter filter)
        {
            magFilter = filter;
            if (handle != 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, handle);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }
        public void SetMinFilter(TextureMinFilter filter)
        {
            minFilter = filter;
            if (handle != 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, handle);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }
    }
}
