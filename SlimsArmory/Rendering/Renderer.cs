using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using RaCLib.Armor;
using SlimsArmory.Rendering.Armor;
using System.Reflection;

namespace SlimsArmory.Rendering
{
    public class Renderer
    {
        public List<GLMesh> Meshes;
        public Camera Camera;
        public Shader TexturedShader;
        public Shader ReflectiveShader;
        public Shader DebugShader;

        public Matrix4 Projection = Matrix4.CreatePerspectiveFieldOfView(80.0f * (float)Math.PI / 180, 16 / 9, 0.1f, 100.0f);
        public Matrix4 View = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
        public Matrix4 Model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f)) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90.0f));

        public Vector3 LightPosition;

        public int DebugLodLevel = 0;

        private float[] mDebugVerts = new float[9]
        {
            -0.5f, 0.0f, 0.0f,
             0.5f, 0.0f, 0.0f,
             0.0f, 0.5f, 0.0f
        };
        private int mDebugVertexBuffer;
        private int mDebugVertexArray;


        public Renderer()
        {
            Camera = new Camera();
            Camera.FieldOfView = 80.0f * (float)Math.PI / 180;
            Camera.SetPosition(0.0f, 0.0f, -3.0f);
            Meshes = new List<GLMesh>();
            TexturedShader = Shader.Create("Textured");
            ReflectiveShader = Shader.Create("Reflective");
            DebugShader = Shader.Create("Debug");
            LightPosition = new Vector3(0.0f, 0.0f, 0.0f);

            mDebugVertexArray = GL.GenVertexArray();
            GL.BindVertexArray(mDebugVertexArray);

            mDebugVertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, mDebugVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 9, mDebugVerts, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12, 0);
            GL.EnableVertexAttribArray(0);
        }

        public void DebugDraw()
        {
            DebugShader.Bind();
            GL.BindVertexArray(mDebugVertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mDebugVertexBuffer);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        public void AddObject(RaCLib.Armor.Armor armor)
        {
            Meshes.Add(new GLMesh(armor));
        }

        public void DrawAll()
        {
            Vector3 viewDirection = Camera.Position - Camera.Target;
            viewDirection.Normalize();
            GL.Enable(EnableCap.DepthTest);
            foreach (var mesh in Meshes)
            {
                TexturedShader.Bind();
                TexturedShader.SetUniform("matView", Camera.ViewMatrix);
                TexturedShader.SetUniform("matModel", Model);
                TexturedShader.SetUniform("matProj", Camera.ProjectionMatrix);// Calculate the view direction (normalized)
                TexturedShader.SetUniform("uEye", viewDirection);
                TexturedShader.SetUniform("uLightPos", LightPosition);

                mesh.DrawTextured(TexturedShader);

                ReflectiveShader.Bind();
                ReflectiveShader.SetUniform("matView", Camera.ViewMatrix);
                ReflectiveShader.SetUniform("matModel", Model);
                ReflectiveShader.SetUniform("matProj", Camera.ProjectionMatrix);// Calculate the view direction (normalized)
                ReflectiveShader.SetUniform("uEye", viewDirection);
                ReflectiveShader.SetUniform("uLightPos", LightPosition);
                mesh.DrawReflective(ReflectiveShader);
            }
        }
    }
}
