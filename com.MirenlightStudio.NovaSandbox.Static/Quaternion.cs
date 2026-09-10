using System.Globalization;
using System.Runtime.CompilerServices;

namespace com.MirenlightStudio.NovaSandbox.Static
{
    /// <summary>
    /// Standalone quaternion rotation, ported for use outside the Unity engine
    /// runtime (no UnityEngine dependency). Depends only on GameServer.Math.Vector3.
    /// API surface mirrors UnityEngine.Quaternion so client/server math stays consistent.
    /// </summary>
    [Serializable]
    public struct Quaternion : IEquatable<Quaternion>, IFormattable
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public const float kEpsilon = 0.000001f;

        const float Rad2Deg = 57.29578f;
        const float Deg2Rad = 0.0174533f;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    case 3: w = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float newX, float newY, float newZ, float newW)
        {
            x = newX;
            y = newY;
            z = newZ;
            w = newW;
        }

        static readonly Quaternion identityQuaternion = new Quaternion(0f, 0f, 0f, 1f);

        public static Quaternion identity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => identityQuaternion;
        }

        // ---------- Operators ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs) => new Quaternion(
            lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
            lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);

        // Rotates a point by this rotation.
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float x = rotation.x * 2f;
            float y = rotation.y * 2f;
            float z = rotation.z * 2f;
            float xx = rotation.x * x;
            float yy = rotation.y * y;
            float zz = rotation.z * z;
            float xy = rotation.x * y;
            float xz = rotation.x * z;
            float yz = rotation.y * z;
            float wx = rotation.w * x;
            float wy = rotation.w * y;
            float wz = rotation.w * z;

            return new Vector3(
                (1f - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z,
                (xy + wz) * point.x + (1f - (xx + zz)) * point.y + (yz - wx) * point.z,
                (xz - wy) * point.x + (yz + wx) * point.y + (1f - (xx + yy)) * point.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Quaternion lhs, Quaternion rhs) => IsEqualUsingDot(Dot(lhs, rhs));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Quaternion lhs, Quaternion rhs) => !IsEqualUsingDot(Dot(lhs, rhs));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsEqualUsingDot(float dot) => dot > 1.0f - kEpsilon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Quaternion a, Quaternion b) => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;

        // ---------- Look rotation ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLookRotation(Vector3 view, Vector3 up) => this = LookRotation(view, up);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLookRotation(Vector3 view) => SetLookRotation(view, Vector3.up);

        /// <summary>
        /// Creates a rotation with the specified forward and upwards directions.
        /// (Implemented directly, without the native LookRotation call Unity uses internally.)
        /// </summary>
        public static Quaternion LookRotation(Vector3 forward, Vector3 up)
        {
            forward = Vector3.Normalize(forward);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            Vector3 newUp = Vector3.Cross(forward, right);

            float m00 = right.x, m01 = right.y, m02 = right.z;
            float m10 = newUp.x, m11 = newUp.y, m12 = newUp.z;
            float m20 = forward.x, m21 = forward.y, m22 = forward.z;

            float trace = m00 + m11 + m22;
            Quaternion q = default;

            if (trace > 0f)
            {
                float s = (float)System.Math.Sqrt(trace + 1.0f) * 2f;
                q.w = 0.25f * s;
                q.x = (m12 - m21) / s;
                q.y = (m20 - m02) / s;
                q.z = (m01 - m10) / s;
            }
            else if (m00 > m11 && m00 > m22)
            {
                float s = (float)System.Math.Sqrt(1.0f + m00 - m11 - m22) * 2f;
                q.w = (m12 - m21) / s;
                q.x = 0.25f * s;
                q.y = (m10 + m01) / s;
                q.z = (m20 + m02) / s;
            }
            else if (m11 > m22)
            {
                float s = (float)System.Math.Sqrt(1.0f + m11 - m00 - m22) * 2f;
                q.w = (m20 - m02) / s;
                q.x = (m10 + m01) / s;
                q.y = 0.25f * s;
                q.z = (m21 + m12) / s;
            }
            else
            {
                float s = (float)System.Math.Sqrt(1.0f + m22 - m00 - m11) * 2f;
                q.w = (m01 - m10) / s;
                q.x = (m20 + m02) / s;
                q.y = (m21 + m12) / s;
                q.z = 0.25f * s;
            }

            return Normalize(q);
        }

        public static Quaternion LookRotation(Vector3 forward) => LookRotation(forward, Vector3.up);

        /// <summary>
        /// Creates a rotation that rotates fromDirection to toDirection.
        /// </summary>
        public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            Vector3 from = Vector3.Normalize(fromDirection);
            Vector3 to = Vector3.Normalize(toDirection);
            float dot = Vector3.Dot(from, to);

            if (dot >= 1f - kEpsilon)
                return identity;

            if (dot <= -1f + kEpsilon)
            {
                // Vectors point in opposite directions: pick any orthogonal axis.
                Vector3 axis = Vector3.Cross(Vector3.right, from);
                if (axis.sqrMagnitude < kEpsilon)
                    axis = Vector3.Cross(Vector3.up, from);
                return AngleAxis(180f, Vector3.Normalize(axis));
            }

            Vector3 cross = Vector3.Cross(from, to);
            float s = (float)System.Math.Sqrt((1f + dot) * 2f);
            float invS = 1f / s;

            return Normalize(new Quaternion(cross.x * invS, cross.y * invS, cross.z * invS, s * 0.5f));
        }

        public void SetFromToRotation(Vector3 fromDirection, Vector3 toDirection) => this = FromToRotation(fromDirection, toDirection);

        // ---------- Angle-axis ----------

        /// <summary>
        /// Creates a rotation of "angle" degrees around "axis".
        /// </summary>
        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            axis = Vector3.Normalize(axis);
            float radians = angle * Deg2Rad * 0.5f;
            float s = (float)System.Math.Sin(radians);

            return new Quaternion(axis.x * s, axis.y * s, axis.z * s, (float)System.Math.Cos(radians));
        }

        public void ToAngleAxis(out float angle, out Vector3 axis)
        {
            Quaternion q = w > 1f ? Normalize(this) : this;

            angle = 2f * (float)System.Math.Acos(q.w) * Rad2Deg;
            float den = (float)System.Math.Sqrt(1.0 - q.w * q.w);

            if (den > 0.0001f)
            {
                axis = new Vector3(q.x / den, q.y / den, q.z / den);
            }
            else
            {
                // Angle is 0 (or 360): axis direction doesn't matter.
                axis = new Vector3(1f, 0f, 0f);
            }
        }

        // ---------- Euler angles ----------

        static Vector3 MakePositive(Vector3 euler)
        {
            const float negativeFlip = -0.0001f * Rad2Deg;
            const float positiveFlip = 360.0f + negativeFlip;

            if (euler.x < negativeFlip) euler.x += 360.0f;
            else if (euler.x > positiveFlip) euler.x -= 360.0f;

            if (euler.y < negativeFlip) euler.y += 360.0f;
            else if (euler.y > positiveFlip) euler.y -= 360.0f;

            if (euler.z < negativeFlip) euler.z += 360.0f;
            else if (euler.z > positiveFlip) euler.z -= 360.0f;

            return euler;
        }

        /// <summary>Converts this quaternion to Euler angles (in radians), XYZ order.</summary>
        static Vector3 ToEulerRad(Quaternion q)
        {
            float sinrCosp = 2f * (q.w * q.x + q.y * q.z);
            float cosrCosp = 1f - 2f * (q.x * q.x + q.y * q.y);
            float roll = (float)System.Math.Atan2(sinrCosp, cosrCosp);

            float sinp = 2f * (q.w * q.y - q.z * q.x);
            float pitch = System.Math.Abs(sinp) >= 1f
                ? (float)(System.Math.PI / 2) * System.Math.Sign(sinp)
                : (float)System.Math.Asin(sinp);

            float sinyCosp = 2f * (q.w * q.z + q.x * q.y);
            float cosyCosp = 1f - 2f * (q.y * q.y + q.z * q.z);
            float yaw = (float)System.Math.Atan2(sinyCosp, cosyCosp);

            return new Vector3(roll, pitch, yaw);
        }

        static Quaternion FromEulerRad(Vector3 euler)
        {
            float cx = (float)System.Math.Cos(euler.x * 0.5f);
            float sx = (float)System.Math.Sin(euler.x * 0.5f);
            float cy = (float)System.Math.Cos(euler.y * 0.5f);
            float sy = (float)System.Math.Sin(euler.y * 0.5f);
            float cz = (float)System.Math.Cos(euler.z * 0.5f);
            float sz = (float)System.Math.Sin(euler.z * 0.5f);

            return new Quaternion(
                sx * cy * cz + cx * sy * sz,
                cx * sy * cz - sx * cy * sz,
                cx * cy * sz - sx * sy * cz,
                cx * cy * cz + sx * sy * sz);
        }

        public Vector3 eulerAngles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => MakePositive(ToEulerRad(this) * Rad2Deg);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this = FromEulerRad(value * Deg2Rad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Euler(float x, float y, float z) =>
            FromEulerRad(new Vector3(x * Deg2Rad, y * Deg2Rad, z * Deg2Rad));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Euler(Vector3 euler) => FromEulerRad(euler * Deg2Rad);

        // ---------- Angle / interpolation ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(Quaternion a, Quaternion b)
        {
            float dot = System.Math.Min(System.Math.Abs(Dot(a, b)), 1.0f);
            return IsEqualUsingDot(dot) ? 0.0f : (float)System.Math.Acos(dot) * 2.0f * Rad2Deg;
        }

        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
        {
            float angle = Angle(from, to);
            if (angle == 0.0f) return to;
            return SlerpUnclamped(from, to, System.Math.Min(1.0f, maxDegreesDelta / angle));
        }

        /// <summary>Spherically interpolates between a and b by t, clamped to [0, 1].</summary>
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
        {
            t = t < 0f ? 0f : (t > 1f ? 1f : t);
            return SlerpUnclamped(a, b, t);
        }

        public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float t)
        {
            float dot = Dot(a, b);

            if (dot < 0f)
            {
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
                dot = -dot;
            }

            const float dotThreshold = 0.9995f;
            if (dot > dotThreshold)
            {
                // Nearly identical rotations: fall back to linear interpolation.
                Quaternion result = new Quaternion(
                    a.x + (b.x - a.x) * t,
                    a.y + (b.y - a.y) * t,
                    a.z + (b.z - a.z) * t,
                    a.w + (b.w - a.w) * t);
                return Normalize(result);
            }

            float theta0 = (float)System.Math.Acos(dot);
            float theta = theta0 * t;
            float sinTheta = (float)System.Math.Sin(theta);
            float sinTheta0 = (float)System.Math.Sin(theta0);

            float s0 = (float)System.Math.Cos(theta) - dot * sinTheta / sinTheta0;
            float s1 = sinTheta / sinTheta0;

            return new Quaternion(
                s0 * a.x + s1 * b.x,
                s0 * a.y + s1 * b.y,
                s0 * a.z + s1 * b.z,
                s0 * a.w + s1 * b.w);
        }

        /// <summary>Linearly interpolates between a and b by t, clamped to [0, 1], and normalizes the result.</summary>
        public static Quaternion Lerp(Quaternion a, Quaternion b, float t)
        {
            t = t < 0f ? 0f : (t > 1f ? 1f : t);
            return LerpUnclamped(a, b, t);
        }

        public static Quaternion LerpUnclamped(Quaternion a, Quaternion b, float t)
        {
            if (Dot(a, b) < 0f)
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);

            return Normalize(new Quaternion(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t,
                a.w + (b.w - a.w) * t));
        }

        // ---------- Normalization ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Normalize(Quaternion q)
        {
            float mag = (float)System.Math.Sqrt(Dot(q, q));
            if (mag < float.Epsilon)
                return identity;

            return new Quaternion(q.x / mag, q.y / mag, q.z / mag, q.w / mag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize() => this = Normalize(this);

        public readonly Quaternion normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Normalize(this);
        }

        /// <summary>Returns the inverse (conjugate, for unit quaternions) of the rotation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Inverse(Quaternion rotation) => new Quaternion(-rotation.x, -rotation.y, -rotation.z, rotation.w);

        // ---------- Equality / formatting ----------

        public override readonly int GetHashCode() =>
            x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2) ^ (w.GetHashCode() >> 1);

        public override readonly bool Equals(object other) => other is Quaternion q && Equals(q);

        public readonly bool Equals(Quaternion other) =>
            x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);

        public override readonly string ToString() => ToString(null, null);

        public readonly string ToString(string format) => ToString(format, null);

        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F5";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return $"({x.ToString(format, formatProvider)}, {y.ToString(format, formatProvider)}, " +
                   $"{z.ToString(format, formatProvider)}, {w.ToString(format, formatProvider)})";
        }
    }
}