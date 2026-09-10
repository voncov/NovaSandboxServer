using System.Globalization;
using System.Runtime.CompilerServices;

namespace com.MirenlightStudio.NovaSandbox.Static
{
    /// <summary>
    /// Standalone 3D vector/point representation, ported for use outside the
    /// Unity engine runtime (no UnityEngine dependency, no native bindings).
    /// API surface mirrors UnityEngine.Vector3 so client/server math stays consistent.
    /// </summary>
    [Serializable]
    public struct Vector3 : IEquatable<Vector3>, IFormattable
    {
        public const float kEpsilon = 0.00001f;
        public const float kEpsilonNormalSqrt = 1e-15f;

        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0f;
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
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
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
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float newX, float newY, float newZ)
        {
            x = newX;
            y = newY;
            z = newZ;
        }

        // ---------- Static constants ----------

        static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);
        static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
        static readonly Vector3 upVector = new Vector3(0f, 1f, 0f);
        static readonly Vector3 downVector = new Vector3(0f, -1f, 0f);
        static readonly Vector3 leftVector = new Vector3(-1f, 0f, 0f);
        static readonly Vector3 rightVector = new Vector3(1f, 0f, 0f);
        static readonly Vector3 forwardVector = new Vector3(0f, 0f, 1f);
        static readonly Vector3 backVector = new Vector3(0f, 0f, -1f);
        static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public static Vector3 zero => zeroVector;
        public static Vector3 one => oneVector;
        public static Vector3 up => upVector;
        public static Vector3 down => downVector;
        public static Vector3 left => leftVector;
        public static Vector3 right => rightVector;
        public static Vector3 forward => forwardVector;
        public static Vector3 back => backVector;
        public static Vector3 positiveInfinity => positiveInfinityVector;
        public static Vector3 negativeInfinity => negativeInfinityVector;

        // ---------- Interpolation ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = Clamp01(t);
            return new Vector3(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) => new Vector3(
            a.x + (b.x - a.x) * t,
            a.y + (b.y - a.y) * t,
            a.z + (b.z - a.z) * t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            float toX = target.x - current.x;
            float toY = target.y - current.y;
            float toZ = target.z - current.z;

            float sqDist = toX * toX + toY * toY + toZ * toZ;

            if (sqDist == 0f || (maxDistanceDelta >= 0f && sqDist <= maxDistanceDelta * maxDistanceDelta))
                return target;

            float dist = (float)System.Math.Sqrt(sqDist);

            return new Vector3(
                current.x + toX / dist * maxDistanceDelta,
                current.y + toY / dist * maxDistanceDelta,
                current.z + toZ / dist * maxDistanceDelta);
        }

        /// <summary>
        /// Gradually changes a vector towards a target over time (critically damped spring).
        /// Unlike the Unity version, deltaTime must always be supplied explicitly since
        /// there is no engine Time class on the server.
        /// </summary>
        public static Vector3 SmoothDamp(
            Vector3 current,
            Vector3 target,
            ref Vector3 currentVelocity,
            float smoothTime,
            float deltaTime,
            float maxSpeed = float.PositiveInfinity)
        {
            smoothTime = System.Math.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime;

            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            float changeX = current.x - target.x;
            float changeY = current.y - target.y;
            float changeZ = current.z - target.z;

            float maxChange = maxSpeed * smoothTime;
            float maxChangeSq = maxChange * maxChange;
            float sqrMag = changeX * changeX + changeY * changeY + changeZ * changeZ;
            if (sqrMag > maxChangeSq)
            {
                float mag = (float)System.Math.Sqrt(sqrMag);
                changeX = changeX / mag * maxChange;
                changeY = changeY / mag * maxChange;
                changeZ = changeZ / mag * maxChange;
            }

            float targetX = current.x - changeX;
            float targetY = current.y - changeY;
            float targetZ = current.z - changeZ;

            float tempX = (currentVelocity.x + omega * changeX) * deltaTime;
            float tempY = (currentVelocity.y + omega * changeY) * deltaTime;
            float tempZ = (currentVelocity.z + omega * changeZ) * deltaTime;

            currentVelocity.x = (currentVelocity.x - omega * tempX) * exp;
            currentVelocity.y = (currentVelocity.y - omega * tempY) * exp;
            currentVelocity.z = (currentVelocity.z - omega * tempZ) * exp;

            float outX = targetX + (changeX + tempX) * exp;
            float outY = targetY + (changeY + tempY) * exp;
            float outZ = targetZ + (changeZ + tempZ) * exp;

            float origMinusCurrentX = target.x - current.x;
            float origMinusCurrentY = target.y - current.y;
            float origMinusCurrentZ = target.z - current.z;
            float outMinusOrigX = outX - target.x;
            float outMinusOrigY = outY - target.y;
            float outMinusOrigZ = outZ - target.z;

            if (origMinusCurrentX * outMinusOrigX + origMinusCurrentY * outMinusOrigY + origMinusCurrentZ * outMinusOrigZ > 0f)
            {
                outX = target.x;
                outY = target.y;
                outZ = target.z;

                currentVelocity.x = (outX - target.x) / deltaTime;
                currentVelocity.y = (outY - target.y) / deltaTime;
                currentVelocity.z = (outZ - target.z) / deltaTime;
            }

            return new Vector3(outX, outY, outZ);
        }

        // ---------- Core vector math ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(Vector3 scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs) => new Vector3(
            lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.x * rhs.y - lhs.y * rhs.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector3 lhs, Vector3 rhs) => lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        {
            float factor = -2f * Dot(inNormal, inDirection);
            return new Vector3(
                factor * inNormal.x + inDirection.x,
                factor * inNormal.y + inDirection.y,
                factor * inNormal.z + inDirection.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 value)
        {
            float mag = value.magnitude;
            return mag > kEpsilon
                ? new Vector3(value.x / mag, value.y / mag, value.z / mag)
                : zeroVector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = magnitude;
            if (mag > kEpsilon)
            {
                x /= mag;
                y /= mag;
                z /= mag;
            }
            else
            {
                x = 0f;
                y = 0f;
                z = 0f;
            }
        }

        public readonly Vector3 normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Normalize(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        {
            float sqrMag = Dot(onNormal, onNormal);
            if (sqrMag < float.Epsilon)
                return zero;

            float dotInvSqrMag = Dot(vector, onNormal) / sqrMag;
            return new Vector3(onNormal.x * dotInvSqrMag, onNormal.y * dotInvSqrMag, onNormal.z * dotInvSqrMag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        {
            float sqrMag = Dot(planeNormal, planeNormal);
            if (sqrMag < float.Epsilon)
                return vector;

            float dotInvSqrMag = Dot(vector, planeNormal) / sqrMag;
            return new Vector3(
                vector.x - planeNormal.x * dotInvSqrMag,
                vector.y - planeNormal.y * dotInvSqrMag,
                vector.z - planeNormal.z * dotInvSqrMag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(Vector3 from, Vector3 to)
        {
            float denominator = (float)System.Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < kEpsilonNormalSqrt)
                return 0f;

            float dot = Clamp(Dot(from, to) / denominator, -1f, 1f);
            return (float)System.Math.Acos(dot) * Rad2Deg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float unsignedAngle = Angle(from, to);

            float crossX = from.y * to.z - from.z * to.y;
            float crossY = from.z * to.x - from.x * to.z;
            float crossZ = from.x * to.y - from.y * to.x;
            float sign = System.Math.Sign(axis.x * crossX + axis.y * crossY + axis.z * crossZ);
            return unsignedAngle * sign;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float dz = a.z - b.z;
            return (float)System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            float sqrMag = vector.sqrMagnitude;
            if (sqrMag <= maxLength * maxLength)
                return vector;

            float mag = (float)System.Math.Sqrt(sqrMag);
            float nx = vector.x / mag;
            float ny = vector.y / mag;
            float nz = vector.z / mag;
            return new Vector3(nx * maxLength, ny * maxLength, nz * maxLength);
        }

        public readonly float magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)System.Math.Sqrt(x * x + y * y + z * z);
        }

        public readonly float sqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => x * x + y * y + z * z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 lhs, Vector3 rhs) => new Vector3(
            System.Math.Min(lhs.x, rhs.x), System.Math.Min(lhs.y, rhs.y), System.Math.Min(lhs.z, rhs.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 lhs, Vector3 rhs) => new Vector3(
            System.Math.Max(lhs.x, rhs.x), System.Math.Max(lhs.y, rhs.y), System.Math.Max(lhs.z, rhs.z));

        // ---------- Small local math helpers (no UnityEngine.Mathf dependency) ----------

        const float Rad2Deg = 57.29578f;
        const float Deg2Rad = 0.0174533f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Clamp01(float value) => value < 0f ? 0f : (value > 1f ? 1f : value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);

        // ---------- Operators ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.x * d, a.y * d, a.z * d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            float dx = lhs.x - rhs.x;
            float dy = lhs.y - rhs.y;
            float dz = lhs.z - rhs.z;
            return dx * dx + dy * dy + dz * dz < kEpsilon * kEpsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3 lhs, Vector3 rhs) => !(lhs == rhs);

        // ---------- Equality / formatting ----------

        public override readonly int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);

        public override readonly bool Equals(object other) => other is Vector3 v && Equals(v);

        public readonly bool Equals(Vector3 other) => x == other.x && y == other.y && z == other.z;

        public override readonly string ToString() => ToString(null, null);

        public readonly string ToString(string format) => ToString(format, null);

        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return $"({x.ToString(format, formatProvider)}, {y.ToString(format, formatProvider)}, {z.ToString(format, formatProvider)})";
        }
    }
}