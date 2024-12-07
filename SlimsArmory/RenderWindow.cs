using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlimsArmory.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Input;
using ImGuiNET;
using SlimsArmory.ImGuiHelpers;
using RaCLib.IO;

namespace SlimsArmory
{
    public class RenderWindow : GameWindow
    {
        public Renderer? Renderer;
        private ImGuiController mController;

        protected override void OnLoad()
        {
            base.OnLoad();

            mController = new ImGuiController(ClientSize.X, ClientSize.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            // draw on top
            ImGui.DockSpaceOverViewport();

            ImGui.ShowDemoWindow();

            mController.Update(this, (float)args.Time);

            ImGuiController.CheckGLError("End of frame"); 

            if (Renderer != null)
            {
                Renderer.DrawAll();
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }

            if (Renderer != null)
            {
                KeyboardState input = KeyboardState;

                if (input.IsKeyDown(Keys.W))
                {
                    Vector3 dir = (Renderer.Camera.Target - Renderer.Camera.Position).Normalized();
                    Renderer.Camera.Position += dir * Renderer.Camera.Speed * (float)args.Time; //Forward 
                    Renderer.Camera.Target += dir * Renderer.Camera.Speed * (float)args.Time; //Forward 
                }

                if (input.IsKeyDown(Keys.S))
                {
                    Vector3 dir = (Renderer.Camera.Target - Renderer.Camera.Position).Normalized();
                    Renderer.Camera.Position -= dir * Renderer.Camera.Speed * (float)args.Time; //Backwards
                    Renderer.Camera.Target -= dir * Renderer.Camera.Speed * (float)args.Time; //Backwards
                }

                if (input.IsKeyDown(Keys.A))
                {
                    Vector3 dir = (Renderer.Camera.Target - Renderer.Camera.Position).Normalized();
                    Renderer.Camera.Position -= Vector3.Normalize(Vector3.Cross(dir, Renderer.Camera.Up)) * Renderer.Camera.Speed * (float)args.Time; //Left
                    Renderer.Camera.Target -= Vector3.Normalize(Vector3.Cross(dir, Renderer.Camera.Up)) * Renderer.Camera.Speed * (float)args.Time; //Left
                }

                if (input.IsKeyDown(Keys.D))
                {
                    Vector3 dir = (Renderer.Camera.Target - Renderer.Camera.Position).Normalized();
                    Renderer.Camera.Position += Vector3.Normalize(Vector3.Cross(dir, Renderer.Camera.Up)) * Renderer.Camera.Speed * (float)args.Time; //Right
                    Renderer.Camera.Target += Vector3.Normalize(Vector3.Cross(dir, Renderer.Camera.Up)) * Renderer.Camera.Speed * (float)args.Time;
                }

                if (input.IsKeyDown(Keys.Space))
                {
                    Renderer.Camera.Position += Renderer.Camera.Up * Renderer.Camera.Speed * (float)args.Time; //Up 
                    Renderer.Camera.Target += Renderer.Camera.Up * Renderer.Camera.Speed * (float)args.Time;
                }

                if (input.IsKeyDown(Keys.LeftShift))
                {
                    Renderer.Camera.Position -= Renderer.Camera.Up * Renderer.Camera.Speed * (float)args.Time; //Down
                    Renderer.Camera.Target -= Renderer.Camera.Up * Renderer.Camera.Speed * (float)args.Time; //Down;
                }

                if (input.IsKeyDown(Keys.Left))
                {
                    Renderer.Camera.Rotate(-Renderer.Camera.Speed * 50 * (float)args.Time, 0);
                }
                if (input.IsKeyDown(Keys.Right))
                {
                    Renderer.Camera.Rotate(Renderer.Camera.Speed * 50 * (float)args.Time, 0);
                }

                if (input.IsKeyDown(Keys.Up))
                {
                    Renderer.Camera.Rotate(0, -Renderer.Camera.Speed * 50 * (float)args.Time);
                }

                if (input.IsKeyDown(Keys.Down))
                {
                    Renderer.Camera.Rotate(0, Renderer.Camera.Speed * 50 * (float)args.Time);
                }

                if (input.IsKeyPressed(Keys.E))
                {
                    Renderer.DebugLodLevel++;
                }
                if (input.IsKeyPressed(Keys.Q))
                {
                    Renderer.DebugLodLevel--;
                }

                if (input.IsKeyDown(Keys.J))
                {
                    Renderer.LightPosition.X -= 50 * (float)args.Time;
                }
                if (input.IsKeyDown(Keys.L))
                {
                    Renderer.LightPosition.X += 50 * (float)args.Time;
                }
                if (input.IsKeyDown(Keys.I))
                {
                    Renderer.LightPosition.Y -= 50 * (float)args.Time;
                }
                if (input.IsKeyDown(Keys.K))
                {
                    Renderer.LightPosition.Y += 50 * (float)args.Time;
                }
                if (input.IsKeyDown(Keys.U))
                {
                    Renderer.LightPosition.Z -= 50 * (float)args.Time;
                }
                if (input.IsKeyDown(Keys.O))
                {
                    Renderer.LightPosition.Z += 50 * (float)args.Time;
                }
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            Renderer.Projection = Matrix4.CreatePerspectiveFieldOfView(80.0f * (float)Math.PI / 180, e.Width / e.Height, 0.1f, 100.0f);
            Renderer.Camera.SetFieldOfView(MathHelper.DegreesToRadians(80.0f));
            Renderer.Camera.SetAspectRatio(e.Width / e.Height);
            GL.Viewport(0, 0, e.Width, e.Height);
            mController.WindowResized(ClientSize.X, ClientSize.Y);
        }

        public RenderWindow(int width, int height, Armor armor) : base(new GameWindowSettings(), new NativeWindowSettings())
        {
            this.Title = "Slim's Armory (GLTest)";
            Renderer = new Renderer();
            Renderer.AddObject(armor);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
