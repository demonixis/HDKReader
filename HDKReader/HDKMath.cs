using System;

namespace HDKReader
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero => new Vector3();

        public static Vector3 FromValues(ref float[] values)
        {
            return new Vector3(values[4], values[5], values[6]);
        }

        public void Set(int x, int y, int z)
        {
            X = ((float)x) / (1 << 9);
            Y = ((float)y) / (1 << 9);
            Z = ((float)z) / (1 << 9);
        }

        public override string ToString() => $"X={X}, Y={Y}, Z={Z}";

        public string Serialize() => $"{X}_{Y}_{Z}";
    }

    public struct Quaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Quaternion FromValues(ref float[] values)
        {
            return new Quaternion(values[0], values[1], values[2], values[3]);
        }

        public static Quaternion Identity => new Quaternion(0, 0, 0, 1);

        public void Multiply(Quaternion q)
        {
            var x = W * q.X + X * q.W + Y * q.Z - Z * q.Y;
            var y = W * q.Y - X * q.Z + Y * q.W + Z * q.X;
            var z = W * q.Z + X * q.Y - Y * q.X + Z * q.W;
            var w = W * q.W - X * q.X - Y * q.Y - Z * q.Z;

            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        public void Normalize()
        {
            var len = Length();
            X /= len;
            Y /= len;
            Z /= len;
            W /= len;
        }

        public void Set(int x, int y, int z, int w)
        {
            X = ((float)x) / (1 << 14);
            Y = ((float)y) / (1 << 14);
            Z = ((float)z) / (1 << 14);
            W = ((float)w) / (1 << 14);

           /* Quaternion rotateX = new Quaternion
            {
                X = (float)Math.Sqrt(0.5),
                Y = 0.0f,
                Z = 0.0f,
                W = (float)Math.Sqrt(0.5)
            };

            Multiply(rotateX);
            Normalize();

            var tmp = Y;
            Y = Z;
            Z = tmp;
            X = -X;*/
        }

        public void Set2(int w, int x, int y, int z)
        {
            X = ((float)x) / (1 << 14);
            Y = ((float)y) / (1 << 14);
            Z = ((float)z) / (1 << 14);
            W = ((float)w) / (1 << 14);

            /* Quaternion rotateX = new Quaternion
             {
                 X = (float)Math.Sqrt(0.5),
                 Y = 0.0f,
                 Z = 0.0f,
                 W = (float)Math.Sqrt(0.5)
             };

             Multiply(rotateX);
             Normalize();

             var tmp = Y;
             Y = Z;
             Z = tmp;
             X = -X;*/
        }

        public override string ToString() => $"X={X}, Y={Y}, Z={Z}, W={W}";

        public string Serialize() => $"{X}_{Y}_{Z}_{W}";
    }
}