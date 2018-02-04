using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Microsoft.Xna.Framework {
    // ReSharper disable once InconsistentNaming
    public struct Matrix3x2 : IEquatable<Matrix3x2> {

        public Matrix3x2(float m11, float m12, float m21, float m22, float m31, float m32) {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            M31 = m31;
            M32 = m32;
        }

        private Matrix3x2(float elem) {
            M11 = elem;
            M12 = elem;
            M21 = elem;
            M22 = elem;
            M31 = elem;
            M32 = elem;
        }

        public float M11, M12, M21, M22, M31, M32;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(Matrix3x2 matrix, Vector2 vector) {
            var x = vector.X * matrix.M11 + vector.Y * matrix.M21 + matrix.M31;
            var y = vector.X * matrix.M12 + vector.Y * matrix.M22 + matrix.M32;

            return new Vector2(x, y);
        }

        public bool IsIdentity => this == IdentityMatrix;

        public static Matrix3x2 Identity => IdentityMatrix;

        public static Matrix3x2 Add(Matrix3x2 left, Matrix3x2 right) {
            return new Matrix3x2(left.M11 + right.M11, left.M12 + right.M12, left.M21 + right.M21, left.M22 + right.M22, left.M31 + right.M31, left.M32 + right.M32);
        }

        public static Matrix3x2 Subtract(Matrix3x2 left, Matrix3x2 right) {
            return new Matrix3x2(left.M11 - right.M11, left.M12 - right.M12, left.M21 - right.M21, left.M22 - right.M22, left.M31 - right.M31, left.M32 - right.M32);
        }

        public static Matrix3x2 Multiply(Matrix3x2 left, Matrix3x2 right) {
            var m11 = left.M11 * right.M11 + left.M12 + right.M21;
            var m12 = left.M11 * right.M12 + left.M12 * right.M22;
            var m21 = left.M21 * right.M11 + left.M22 * right.M21;
            var m22 = left.M21 * right.M12 + left.M22 * right.M22;
            var m31 = left.M31 * right.M11 + left.M32 * right.M21 + right.M31;
            var m32 = left.M31 * right.M12 + left.M32 * right.M22 + right.M32;

            return new Matrix3x2(m11, m12, m21, m22, m31, m32);
        }

        public static Matrix3x2 Multiply(Matrix3x2 matrix, float factor) {
            return new Matrix3x2(matrix.M11 * factor, matrix.M12 * factor, matrix.M21 * factor, matrix.M22 * factor, matrix.M31 * factor, matrix.M32 * factor);
        }

        public static Matrix3x2 operator +(Matrix3x2 left, Matrix3x2 right) {
            return Add(left, right);
        }

        public static Matrix3x2 operator -(Matrix3x2 left, Matrix3x2 right) {
            return Subtract(left, right);
        }

        public static Matrix3x2 operator *(Matrix3x2 left, Matrix3x2 right) {
            return Multiply(left, right);
        }

        public static Matrix3x2 operator *(Matrix3x2 left, float right) {
            return Multiply(left, right);
        }

        public static Matrix3x2 operator *(float left, Matrix3x2 right) {
            return Multiply(right, left);
        }

        public float GetDeterminant() {
            return M11 * M22 - M21 * M12;
        }

        // ReSharper disable once InconsistentNaming
        public Matrix ToMatrix4x4() {
            return new Matrix(M11, M12, 0, 0, M21, M22, 0, 0, 0, 0, 1, 0, M31, M32, 0, 1);
        }

        public static Matrix3x2 Invert(Matrix3x2 matrix, out bool successful) {
            successful = false;

            var det = matrix.GetDeterminant();

            if (det.Equals(0)) {
                return new Matrix3x2(float.NaN);
            }

            var invDet = 1 / det;

            var m11 = matrix.M22 * invDet;
            var m12 = -matrix.M12 * invDet;
            var m21 = -matrix.M21 * invDet;
            var m22 = matrix.M11 * invDet;
            var m31 = (matrix.M21 * matrix.M32 - matrix.M31 * matrix.M22) * invDet;
            var m32 = (matrix.M31 * matrix.M12 - matrix.M11 * matrix.M32) * invDet;

            return new Matrix3x2(m11, m12, m21, m22, m31, m32);
        }

        public static Matrix3x2 Lerp(Matrix3x2 from, Matrix3x2 to, float amount) {
            var m11 = MathHelper.Lerp(from.M11, to.M11, amount);
            var m12 = MathHelper.Lerp(from.M12, to.M12, amount);
            var m21 = MathHelper.Lerp(from.M21, to.M21, amount);
            var m22 = MathHelper.Lerp(from.M22, to.M22, amount);
            var m31 = MathHelper.Lerp(from.M31, to.M31, amount);
            var m32 = MathHelper.Lerp(from.M32, to.M32, amount);

            return new Matrix3x2(m11, m12, m21, m22, m31, m32);
        }

        public static Matrix3x2 Negate(Matrix3x2 matrix) {
            return new Matrix3x2(-matrix.M11, -matrix.M12, -matrix.M21, -matrix.M22, -matrix.M31, -matrix.M32);
        }

        public static Matrix3x2 CreateTranslation(float x, float y) {
            return new Matrix3x2(1, 0, 0, 1, x, y);
        }

        public static Matrix3x2 CreateTranslation(Vector2 translation) {
            return CreateTranslation(translation.X, translation.Y);
        }

        public static Matrix3x2 CreateScale(float scaleX, float scaleY) {
            return new Matrix3x2(scaleX, 0, 0, scaleY, 0, 0);
        }

        public static Matrix3x2 CreateScale(Vector2 scale) {
            return CreateScale(scale.X, scale.Y);
        }

        public static Matrix3x2 CreateScale(float scaleX, float scaleY, Vector2 origin) {
            var tx = origin.X * (1 - scaleX);
            var ty = origin.Y * (1 - scaleY);

            return new Matrix3x2(scaleX, 0, 0, scaleY, tx, ty);
        }

        public static Matrix3x2 CreateScale(Vector2 scale, Vector2 origin) {
            return CreateScale(scale.X, scale.Y, origin);
        }

        public static Matrix3x2 CreateRotation(float rotation) {
            var cos = (float)Math.Cos(rotation);
            var sin = (float)Math.Sqrt(1 - cos * cos);

            return new Matrix3x2(cos, sin, -sin, cos, 0, 0);
        }

        public static Matrix3x2 CreateRotation(float rotation, Vector2 origin) {
            var cos = (float)Math.Cos(rotation);
            var sin = (float)Math.Sqrt(1 - cos * cos);

            var tx = origin.X * (1 - cos) + origin.Y * sin;
            var ty = origin.Y * (1 - cos) + origin.X * sin;

            return new Matrix3x2(cos, sin, -sin, cos, tx, ty);
        }

        public static Matrix3x2 CreateSkew(float skewX, float skewY) {
            var sx = (float)Math.Tan(skewX);
            var sy = (float)Math.Tan(skewY);

            return new Matrix3x2(1, sy, sx, 1, 0, 0);
        }

        public static Matrix3x2 CreateSkew(Vector2 skew) {
            return CreateSkew(skew.X, skew.Y);
        }

        public static Matrix3x2 CreateSkew(float skewX, float skewY, Vector2 origin) {
            var sx = (float)Math.Tan(skewX);
            var sy = (float)Math.Tan(skewY);

            var tx = -origin.Y * sx;
            var ty = -origin.X * sy;

            return new Matrix3x2(1, sy, sx, 1, tx, ty);
        }

        public static Matrix3x2 CreateSkew(Vector2 skew, Vector2 origin) {
            return CreateSkew(skew.X, skew.Y, origin);
        }

        public override string ToString() {
            return $"({M11} {M12} 0) ({M21} {M22} 0) ({M31} {M32} 1)";
        }

        #region IEquatable<Matrix3x2>
        public bool Equals(Matrix3x2 other) {
            return M11.Equals(other.M11) && M12.Equals(other.M12) && M21.Equals(other.M21) && M22.Equals(other.M22) && M31.Equals(other.M31) && M32.Equals(other.M32);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is Matrix3x2 matrix && Equals(matrix);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            unchecked {
                var hashCode = M11.GetHashCode();
                hashCode = (hashCode * 397) ^ M12.GetHashCode();
                hashCode = (hashCode * 397) ^ M21.GetHashCode();
                hashCode = (hashCode * 397) ^ M22.GetHashCode();
                hashCode = (hashCode * 397) ^ M31.GetHashCode();
                hashCode = (hashCode * 397) ^ M32.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Matrix3x2 left, Matrix3x2 right) {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix3x2 left, Matrix3x2 right) {
            return !left.Equals(right);
        }
        #endregion

        private static readonly Matrix3x2 IdentityMatrix = new Matrix3x2(1, 0, 0, 1, 0, 0);

    }
}