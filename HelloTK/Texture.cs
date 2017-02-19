using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace HelloTK
{
    internal class Texture
    {
        private const string IMAGE_PATH = "Resources/Images/";
        private readonly int handle;

        public Texture(string filename)
        {
            if( !File.Exists(filename) && !File.Exists(IMAGE_PATH+filename) )
            {
                Console.WriteLine("Texture(" + filename + ") Filename doesn't exist");
                throw new FileNotFoundException("Texture(" + filename + ") Filename doesn't exist");
            }

            handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            Bitmap bmp = new Bitmap(IMAGE_PATH + filename);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Bind()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }
    }
}
