using System;
using System.Numerics;

namespace HDKFrontEnd
{
    public static class MathUtils
    {
        public static float Rad2Deg(float rad) => (180.0f * rad / ((float)Math.PI));

        public static void Rad2Deg(ref Vector3 v)
        {
            v.X = Rad2Deg(v.X);
            v.Y = Rad2Deg(v.Y);
            v.Z = Rad2Deg(v.Z);
        }

        public static float ArcTanAngle(float X, float Y)
        {
            if (X == 0)
            {
                if (Y == 1)
                    return (float)Math.PI / 2.0f;
                else
                    return (float)-Math.PI / 2.0f;
            }
            else if (X > 0)
                return (float)Math.Atan(Y / X);
            else if (X < 0)
            {
                if (Y > 0)
                    return (float)Math.Atan(Y / X) + (float)Math.PI;
                else
                    return (float)Math.Atan(Y / X) - (float)Math.PI;
            }
            else
                return 0;
        }

        //returns Euler angles that point from one point to another
        public static Vector3 AngleTo(Vector3 from, Vector3 location)
        {
            Vector3 angle = new Vector3();
            Vector3 v3 = Vector3.Normalize(location - from);
            angle.X = (float)Math.Asin(v3.Y);
            angle.Y = ArcTanAngle(-v3.Z, -v3.X);
            return angle;
        }

        public static void ToEuler(float x, float y, float z, float w, ref Vector3 result)
        {
            var rotation = new Quaternion(x, y, z, w);
            var forward = Vector3.Transform(new Vector3(1, 0, 0), rotation);
            var up = Vector3.Transform(new Vector3(0, 1, 0), rotation);
            result = AngleTo(new Vector3(), forward);
            if (result.X == (float)Math.PI)
            {
                result.Y = ArcTanAngle(up.Z, up.X);
                result.Z = 0;
            }
            else if (result.X == -(float)Math.PI)
            {
                result.Y = ArcTanAngle(-up.Z, -up.X);
                result.Z = 0;
            }
            else
            {
                up = Vector3.Transform(up, Matrix4x4.CreateRotationY(-result.Y));
                up = Vector3.Transform(up, Matrix4x4.CreateRotationX(-result.X));
                result.Z = ArcTanAngle(up.Y, -up.X);
            }
        }

        public static void ToEuler(this Quaternion quaternion, ref Vector3 result)
        {
            ToEuler(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W, ref result);
        }

        public static Vector3 ToEuler(this Quaternion quaternion)
        {
            Vector3 result = Vector3.Zero;
            ToEuler(quaternion, ref result);
            return result;
        }

        public static Quaternion Euler(this Quaternion q, float x, float y, float z) => Quaternion.CreateFromYawPitchRoll(y, x, z);

        public static Quaternion Euler(this Quaternion q, Vector3 rotation) => Euler(q, rotation.X, rotation.Y, rotation.Z);
    }
}