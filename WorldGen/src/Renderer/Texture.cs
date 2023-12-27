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
using System.Drawing;

namespace WorldGen
{
    /// <summary>
    /// Class to upload pixel data to a gpu texture, providing methods to bind the texture
    /// and set the wrap parameters.
    /// </summary>
    internal class Texture : IDisposable
    {
        private const string IMAGE_PATH = "Resources/Images/";
        private readonly int handle;
        public string name;
        public int Handle { get { return handle; } }

        /// <summary>
        /// Load the given image from the resources folder and create a gpu texture with the same dimensions
        /// </summary>
        /// <param name="filename">The resource filename</param>
        /// <exception cref="FileNotFoundException"></exception>
        public Texture(string filename)
        {
            if (!File.Exists(filename) && !File.Exists(IMAGE_PATH + filename))
            {
                Console.WriteLine("Texture(" + filename + ") Filename doesn't exist");
                throw new FileNotFoundException("Texture(" + filename + ") Filename doesn't exist");
            }
            name = filename;

            var bmp = new Bitmap(IMAGE_PATH + filename);

            handle = GL.GenTexture();
            Upload(bmp);
        }

        /// <summary>
        /// Load the given image from the resources folder, scaling to the given dimensions, 
        /// create a gpu texture and upload the scaled image
        /// </summary>
        /// <param name="filename">The resource filename</param>
        /// <param name="width">Desired width</param>
        /// <param name="height">Desired Height</param>
        /// <exception cref="FileNotFoundException"></exception>
        public Texture(string filename, int width, int height)
        {
            if (!File.Exists(filename) && !File.Exists(IMAGE_PATH + filename))
            {
                Console.WriteLine("Texture(" + filename + ") Filename doesn't exist");
                throw new FileNotFoundException("Texture(" + filename + ") Filename doesn't exist");
            }
            name = filename;

            var img = (Bitmap)Image.FromFile(IMAGE_PATH + filename);
            var bmp = new Bitmap(img, new Size(width, height));

            handle = GL.GenTexture();
            Upload(bmp);
        }

        /// <summary>
        /// Create an RGBA gpu texture with the given size, and upload the given pixel data to it.
        /// </summary>
        /// <param name="name">Name of the texture</param>
        /// <param name="width">Width of the pixel data</param>
        /// <param name="height">Height of the pixel data</param>
        /// <param name="bytesPerPixel">Bytes per pixel</param>
        /// <param name="pixelData">The pixel data</param>
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

        void Upload(System.Drawing.Bitmap bmp)
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);

            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
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
