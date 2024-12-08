using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL.GL;
using RaCLib.IO;
using RaCLib.Armor;

namespace SlimsArmory.Rendering.Armor
{
    /// <summary>
    /// Binds a PXBI Submesh to something usable with OpenGL
    /// </summary>
    public class GLMesh
    {
        private RaCLib.Armor.Armor mBaseMesh;
        private int mTexturedVertexArrayObject;
        private int mTexturedVertexBufferObject;

        public List<GLTexturedSubMesh> TexturedSubMeshes;
        public List<GLReflectiveSubMesh> ReflectiveSubMeshes;
        public List<GLTexture> Textures;

        public int TextureIndex;
        public int NumLitVertices;

        public GLMesh(RaCLib.Armor.Armor mesh)
        {
            mBaseMesh = mesh;
            if (mBaseMesh.Vertices == null)
            {
                throw new InvalidDataException("Armor mesh contains no vertices.");
            }
            else
            {
                if (mBaseMesh.Vertices != null)
                {
                    mTexturedVertexArrayObject = GL.GenVertexArray();
                    GL.BindVertexArray(mTexturedVertexArrayObject);
                    mTexturedVertexBufferObject = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, mTexturedVertexBufferObject);
                    GL.NamedBufferData(mTexturedVertexBufferObject, mBaseMesh.Vertices.Length * (12 + 12 + 8 + 16 + 16 + 8), mBaseMesh.Vertices, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (12 + 12 + 8 + 16 + 16 + 8), 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (12 + 12 + 8 + 16 + 16 + 8), 12);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (12 + 12 + 8 + 16 + 16 + 8), 24);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, (12 + 12 + 8 + 16 + 16 + 8), 40);
                    GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, (12 + 12 + 8 + 16 + 16 + 8), 56);
                    GL.VertexAttribPointer(5, 2, VertexAttribPointerType.Float, false, (12 + 12 + 8 + 16 + 16 + 8), 72);
                    GL.EnableVertexAttribArray(0);
                    GL.EnableVertexAttribArray(1);
                    GL.EnableVertexAttribArray(2);
                    GL.EnableVertexAttribArray(3);
                    GL.EnableVertexAttribArray(4);
                    GL.EnableVertexAttribArray(5);

                    TexturedSubMeshes = new List<GLTexturedSubMesh>();

                    foreach (var texMesh in mBaseMesh.TexturedMeshes)
                    {
                        TexturedSubMeshes.Add(new GLTexturedSubMesh(texMesh));
                    }

                    ReflectiveSubMeshes = new List<GLReflectiveSubMesh>();

                    foreach (var refMesh in mBaseMesh.ReflectiveMeshes)
                    {
                        ReflectiveSubMeshes.Add(new GLReflectiveSubMesh(refMesh));
                    }

                    Textures = new List<GLTexture>();

                    foreach (var tex in mBaseMesh.Textures)
                    {
                        Textures.Add(new GLTexture(tex));
                    }

                    NumLitVertices = mesh.NumLitVertices;
                }
            }
        }

        public void DrawTextured(Shader shader)
        {
            GL.BindVertexArray(mTexturedVertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mTexturedVertexBufferObject);

            shader.SetUniform("uTexture", 0);
            
            foreach (var msh in TexturedSubMeshes)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                Textures[msh.TextureIndex].Bind();
                
                msh.Draw();
            }
        }

        public void DrawReflective(Shader shader)
        {
            GL.BindVertexArray(mTexturedVertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mTexturedVertexBufferObject);

            foreach (var msh in ReflectiveSubMeshes)
            {
                msh.Draw();
            }
        }
    }
}
