using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SlimsArmory.Rendering
{
    public class Shader : IDisposable
    {
        private int mShaderHandle;
        private bool mDisposed;

        public static Shader Create(string shaderName)
        {
            Shader shd = new Shader();
            shd.mShaderHandle = GL.CreateProgram();

            // grab shader source. target directory is Resources\\Shaders\\Basic
            int vertShader = GL.CreateShader(ShaderType.VertexShader);
            string vertSource = File.ReadAllText($"Resources\\Shaders\\{shaderName}\\{shaderName}.vert");
            GL.ShaderSource(vertShader, vertSource);
            GL.CompileShader(vertShader);
            GL.GetShader(vertShader, ShaderParameter.CompileStatus, out int vsuccess);
            if (vsuccess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(vertShader);
                Console.WriteLine(infoLog);
            }

            int fragShader = GL.CreateShader(ShaderType.FragmentShader);
            string fragSource = File.ReadAllText($"Resources\\Shaders\\{shaderName}\\{shaderName}.frag");
            GL.ShaderSource(fragShader, fragSource);
            GL.CompileShader(fragShader);
            GL.GetShader(fragShader, ShaderParameter.CompileStatus, out int fsuccess);
            if (fsuccess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(fragShader);
                Console.WriteLine(infoLog);
            }

            GL.AttachShader(shd.mShaderHandle, vertShader);
            GL.AttachShader(shd.mShaderHandle, fragShader);

            GL.LinkProgram(shd.mShaderHandle);

            GL.DetachShader(shd.mShaderHandle, vertShader);
            GL.DetachShader(shd.mShaderHandle, fragShader);

            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);
            return shd;
        }

        public void Bind()
        {
            GL.UseProgram(mShaderHandle);
        }

        public void SetUniform(string uniformName, int value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.Uniform1(loc, value);
        }
        public void SetUniform(string uniformName, float value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.Uniform1(loc, value);
        }
        public void SetUniform(string uniformName, double value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.Uniform1(loc, value);
        }
        public void SetUniform(string uniformName, Vector2 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.Uniform2(loc, value);
        }
        public void SetUniform(string uniformName, Vector3 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.Uniform3(loc, value);
        }
        public void SetUniform(string uniformName, Vector4 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.Uniform4(loc, value);
        }

        public void SetUniform(string uniformName, Matrix2 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix2(loc, false, ref value);
        }
        public void SetUniform(string uniformName, Matrix2x3 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix2x3(loc, false, ref value);
        }
        public void SetUniform(string uniformName, Matrix2x4 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix2x4(loc, false, ref value);
        }

        public void SetUniform(string uniformName, Matrix3x2 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix3x2(loc, false, ref value);
        }
        public void SetUniform(string uniformName, Matrix3 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix3(loc, false, ref value);
        }
        public void SetUniform(string uniformName, Matrix3x4 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix3x4(loc, false, ref value);
        }

        public void SetUniform(string uniformName, Matrix4x2 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix4x2(loc, false, ref value);
        }
        public void SetUniform(string uniformName, Matrix4x3 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix4x3(loc, false, ref value);
        }
        public void SetUniform(string uniformName, Matrix4 value)
        {
            int loc = GL.GetUniformLocation(mShaderHandle, uniformName);
            GL.UniformMatrix4(loc, false, ref value);
        }

        public void Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                GL.DeleteProgram(mShaderHandle);

                mDisposed = true;
            }
        }
        ~Shader()
        {
            GL.DeleteProgram(mShaderHandle);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
