using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace SlimsArmory.Rendering
{
    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Target = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 CameraDirection => Vector3.Normalize(Position - Target);
        public Vector3 CameraRight => Vector3.Normalize(Vector3.Cross(Up, CameraDirection));
        public Vector3 CameraUp => Vector3.Cross(CameraDirection, CameraRight);
        public Vector3 Up = Vector3.UnitY;
        public Vector3 Front = Vector3.UnitZ * -1;
        public float FieldOfView;
        public float AspectRatio = 16 / 9;
        public Matrix4 ViewMatrix => Matrix4.LookAt(Position, Target, Up);
        public Matrix4 ProjectionMatrix => Matrix4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, 0.001f, 10000.0f);

        public float Speed = 1.0f;

        public void SetAspectRatio(float val) => AspectRatio = val;

        public void SetFieldOfView(float val) => FieldOfView = val;
        public void SetPosition(float x, float y, float z)
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
        }
        public void SetPosition(Vector3 pos) => Position = pos;
        public void SetTarget(float x, float y, float z)
        {
            Target.X = x;
            Target.Y = y;
            Target.Z = z;
        }
        public void SetTarget(Vector3 pos) => Target = pos;

        public void Rotate(float yaw, float pitch)
        {
            // Calculate the direction vector
            Vector3 direction = Target - Position;

            // Convert yaw and pitch from degrees to radians
            float yawRad = MathHelper.DegreesToRadians(yaw);
            float pitchRad = MathHelper.DegreesToRadians(pitch);

            // Apply yaw (rotation around Y-axis)
            float cosYaw = MathF.Cos(yawRad);
            float sinYaw = MathF.Sin(yawRad);
            float newX = direction.X * cosYaw - direction.Z * sinYaw;
            float newZ = direction.X * sinYaw + direction.Z * cosYaw;

            // Update direction after yaw
            direction.X = newX;
            direction.Z = newZ;

            // Apply pitch (rotation around X-axis)
            float cosPitch = MathF.Cos(pitchRad);
            float sinPitch = MathF.Sin(pitchRad);
            float newY = direction.Y * cosPitch - direction.Z * sinPitch;
            newZ = direction.Y * sinPitch + direction.Z * cosPitch;

            // Update direction after pitch
            direction.Y = newY;
            direction.Z = newZ;

            // Recalculate target based on new direction
            Target = Position + direction.Normalized();
        }
    }
}
