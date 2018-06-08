using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAPICodePack.DirectX.Direct3D;
using System.Windows.Media.Media3D;

namespace Microsoft.Research.DynamicDataDisplay.DirectX11.DirectXInternalHelpers
{
    internal static class DXMatrixHelper
    {
        public static Matrix4x4F Transpose(Matrix4x4F matrix)
        {
            Matrix4x4F matrix2 = new Matrix4x4F();
            matrix2.M11 = matrix.M11;
            matrix2.M12 = matrix.M21;
            matrix2.M13 = matrix.M31;
            matrix2.M14 = matrix.M41;
            matrix2.M21 = matrix.M12;
            matrix2.M22 = matrix.M22;
            matrix2.M23 = matrix.M32;
            matrix2.M24 = matrix.M42;
            matrix2.M31 = matrix.M13;
            matrix2.M32 = matrix.M23;
            matrix2.M33 = matrix.M33;
            matrix2.M34 = matrix.M43;
            matrix2.M41 = matrix.M14;
            matrix2.M42 = matrix.M24;
            matrix2.M43 = matrix.M34;
            matrix2.M44 = matrix.M44;
            return matrix2;
        }

        public static Matrix4x4F ToMatrix4x4F(Matrix3D matrix)
        {
            Matrix4x4F matrix2 = new Matrix4x4F();

            matrix2.M11 = (float)matrix.M11;
            matrix2.M12 = (float)matrix.M12;
            matrix2.M13 = (float)matrix.M13;
            matrix2.M14 = (float)matrix.M14;

            matrix2.M21 = (float)matrix.M21;
            matrix2.M22 = (float)matrix.M22;
            matrix2.M23 = (float)matrix.M23;
            matrix2.M24 = (float)matrix.M24;

            matrix2.M31 = (float)matrix.M31;
            matrix2.M32 = (float)matrix.M32;
            matrix2.M33 = (float)matrix.M33;
            matrix2.M34 = (float)matrix.M34;

            matrix2.M41 = (float)matrix.OffsetX;
            matrix2.M42 = (float)matrix.OffsetY;
            matrix2.M43 = (float)matrix.OffsetZ;
            matrix2.M44 = (float)matrix.M44;

            return matrix2;
        }
    }
}
