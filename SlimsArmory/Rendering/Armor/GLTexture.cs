using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using RaCLib.Armor;

namespace SlimsArmory.Rendering.Armor
{
    public class GLTexture
    {
        private int mTexture;


        public GLTexture(ArmorTexture texture)
        {
            mTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, mTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);

            switch (texture.Format)
            {
                case ArmorTextureFormat.BC1:
                    for (int i = 0; i < texture.MipMapCount; i++)
                    {
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, i, InternalFormat.CompressedRgbaS3tcDxt1Ext, texture.MipMaps[i].Width, texture.MipMaps[i].Height, 0, texture.MipMaps[i].MipData.Length, texture.MipMaps[i].MipData);
                    }
                    break;
                case ArmorTextureFormat.BC2:
                    for (int i = 0; i < texture.MipMapCount; i++)
                    {
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, i, InternalFormat.CompressedRgbaS3tcDxt3Ext, texture.MipMaps[i].Width, texture.MipMaps[i].Height, 0, texture.MipMaps[i].MipData.Length, texture.MipMaps[i].MipData);
                    }
                    break;
                case ArmorTextureFormat.BC3:
                    for (int i = 0; i < texture.MipMapCount; i++)
                    {
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, i, InternalFormat.CompressedRgbaS3tcDxt5Ext, texture.MipMaps[i].Width, texture.MipMaps[i].Height, 0, texture.MipMaps[i].MipData.Length, texture.MipMaps[i].MipData);
                    }
                    break;
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, mTexture);
        }
    }
}
