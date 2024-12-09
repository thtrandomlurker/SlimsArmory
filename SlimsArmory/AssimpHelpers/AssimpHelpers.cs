using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SlimsArmory.AssimpHelpers
{
    public static class AssimpHelpers
    {
        public static Matrix4x4 CalculateNodeMatrixWS(Node node)
        {
            // get the node transform matrix
            Matrix4x4 val = node.Transform;
            if (node.Parent != null)
            {
                // if the node has a parent run this block of code until there is no parent
                val *= CalculateNodeMatrixWS(node.Parent);
            }
            return val;
        }

        public static Vector4 MultiplyVec4(this Matrix4x4 matrix, Vector4 value)
        {
            Vector4 ret = new Vector4();

            ret.X = value.X * matrix.M11 + value.Y * matrix.M12 + value.Z * matrix.M13 + value.W * matrix.M14;
            ret.Y = value.X * matrix.M21 + value.Y * matrix.M22 + value.Z * matrix.M23 + value.W * matrix.M24;
            ret.Z = value.X * matrix.M31 + value.Y * matrix.M32 + value.Z * matrix.M33 + value.W * matrix.M34;
            ret.W = value.X * matrix.M41 + value.Y * matrix.M42 + value.Z * matrix.M43 + value.W * matrix.M44;

            return ret;
        }

        public static Vector3 MultiplyVec3(this Matrix4x4 matrix, Vector3 valueV3)
        {
            Vector3 ret = new Vector3();

            Vector4 value = new Vector4(valueV3, 1.0f);

            ret.X = value.X * matrix.M11 + value.Y * matrix.M12 + value.Z * matrix.M13 + value.W * matrix.M14;
            ret.Y = value.X * matrix.M21 + value.Y * matrix.M22 + value.Z * matrix.M23 + value.W * matrix.M24;
            ret.Z = value.X * matrix.M31 + value.Y * matrix.M32 + value.Z * matrix.M33 + value.W * matrix.M34;
            //ret.W = value.X * matrix.M41 + value.Y * matrix.M42 + value.Z * matrix.M43 + value.W * matrix.M44;

            return ret;
        }

        public static Vector3 MultiplyByTransposedInverseMat3(this Matrix4x4 matrix, Vector3 value)
        {
            Vector3 ret = new Vector3();

            // "cast" the matrix to 3x3
            float mat3M11 = matrix.M11, mat3M12 = matrix.M12, mat3M13 = matrix.M13;
            float mat3M21 = matrix.M21, mat3M22 = matrix.M22, mat3M23 = matrix.M23;
            float mat3M31 = matrix.M31, mat3M32 = matrix.M32, mat3M33 = matrix.M33;

            // calculate the determinant
            float det = mat3M11 * (mat3M22 * mat3M33 - mat3M23 * mat3M32) - mat3M12 * (mat3M21 * mat3M33 - mat3M23 * mat3M31) + mat3M13 * (mat3M21 * mat3M32 - mat3M22 * mat3M31);
            if (det == 0)
            {
                throw new InvalidDataException("Matrix is unable to be inverted");
            }

            // Calculate the Cofactor Matrix
            float mat3CoM11 = mat3M22 * mat3M33 - mat3M23 * mat3M32;
            float mat3CoM12 = -(mat3M12 * mat3M33 - mat3M13 * mat3M32);
            float mat3CoM13 = mat3M12 * mat3M23 - mat3M13 * mat3M22;

            float mat3CoM21 = -(mat3M21 * mat3M33 - mat3M23 * mat3M31);
            float mat3CoM22 = mat3M11 * mat3M33 - mat3M13 * mat3M31;
            float mat3CoM23 = -(mat3M11 * mat3M23 - mat3M13 * mat3M21);

            float mat3CoM31 = mat3M21 * mat3M32 - mat3M22 * mat3M31;
            float mat3CoM32 = -(mat3M11 * mat3M32 - mat3M12 * mat3M31);
            float mat3CoM33 = mat3M11 * mat3M22 - mat3M12 * mat3M21;

            // transpose to make the adjugate matrix
            float mat3AdjM11 = mat3CoM11;
            float mat3AdjM12 = mat3CoM21;
            float mat3AdjM13 = mat3CoM31;

            float mat3AdjM21 = mat3CoM12;
            float mat3AdjM22 = mat3CoM22;
            float mat3AdjM23 = mat3CoM32;

            float mat3AdjM31 = mat3CoM13;
            float mat3AdjM32 = mat3CoM23;
            float mat3AdjM33 = mat3CoM33;

            // then calculate the inverse matrix from the determinant
            float invMat3M11 = mat3AdjM11 * (1 / det);
            float invMat3M12 = mat3AdjM12 * (1 / det);
            float invMat3M13 = mat3AdjM13 * (1 / det);

            float invMat3M21 = mat3AdjM21 * (1 / det);
            float invMat3M22 = mat3AdjM22 * (1 / det);
            float invMat3M23 = mat3AdjM23 * (1 / det);

            float invMat3M31 = mat3AdjM31 * (1 / det);
            float invMat3M32 = mat3AdjM32 * (1 / det);
            float invMat3M33 = mat3AdjM33 * (1 / det);

            // next transpose the matrix again
            float transInvMat3M11 = invMat3M11;
            float transInvMat3M12 = invMat3M21;
            float transInvMat3M13 = invMat3M31;

            float transInvMat3M21 = invMat3M12;
            float transInvMat3M22 = invMat3M22;
            float transInvMat3M23 = invMat3M32;

            float transInvMat3M31 = invMat3M13;
            float transInvMat3M32 = invMat3M23;
            float transInvMat3M33 = invMat3M33;

            // finally multiply the matrix by the vector to get the updated transform
            ret.X = value.X * transInvMat3M11 + value.Y * transInvMat3M12 + value.Z * transInvMat3M13;
            ret.Y = value.X * transInvMat3M21 + value.Y * transInvMat3M22 + value.Z * transInvMat3M23;
            ret.Z = value.X * transInvMat3M31 + value.Y * transInvMat3M32 + value.Z * transInvMat3M33;

            return ret;

        }
    }
}
