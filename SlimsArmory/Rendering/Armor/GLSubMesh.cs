using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using RaCLib.IO;

namespace SlimsArmory.Rendering.Armor
{
    public class GLTexturedSubMesh : IDisposable
    {
        private ArmorTexturedSubmesh mBaseMesh;
        private int mElementBuffer;
        public int TextureIndex { get; private set; }
        public int IndexCount { get; private set; }

        public GLTexturedSubMesh(ArmorTexturedSubmesh submesh)
        {
            mBaseMesh = submesh;
            TextureIndex = submesh.TextureIndex;
            mElementBuffer = GL.GenBuffer();
            IndexCount = submesh.IndexCount;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, submesh.IndexCount * sizeof(ushort), submesh.Indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Draw()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mElementBuffer);

            GL.DrawElements(BeginMode.Triangles, IndexCount, DrawElementsType.UnsignedShort, 0);
        }

        public void Dispose()
        {
            if (mElementBuffer != 0)
            {
                GL.DeleteBuffer(mElementBuffer);
                mElementBuffer = 0;
            }
        }
    }
    public class GLReflectiveSubMesh : IDisposable
    {
        private ArmorReflectiveSubmesh mBaseMesh;
        private int mElementBuffer;
        public int ReflectionMode { get; private set; }
        public int IndexCount { get; private set; }

        public GLReflectiveSubMesh(ArmorReflectiveSubmesh submesh)
        {
            mBaseMesh = submesh;
            ReflectionMode = submesh.ReflectionMode;
            mElementBuffer = GL.GenBuffer();
            IndexCount = submesh.IndexCount;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, submesh.IndexCount * sizeof(ushort), submesh.Indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Draw()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mElementBuffer);

            GL.DrawElements(BeginMode.Triangles, IndexCount, DrawElementsType.UnsignedShort, 0);
        }

        public void Dispose()
        {
            if (mElementBuffer != 0)
            {
                GL.DeleteBuffer(mElementBuffer);
                mElementBuffer = 0;
            }
        }
    }

}
