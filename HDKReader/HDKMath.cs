using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDKReader
{
    public struct HDKVector3
    {
        public float X;
        public float Y;
        public float Z;

        public void Set(int index, float value)
        {
            if (index == 0)
                X = value;
            else if (index == 1)
                Y = value;
            else
                Z = value;
        }

        public override string ToString() => $"X={X}, Y={Y}, Z={Z}";
    }

    public struct HDKQuaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public void Set(int index, float value)
        {
            if (index == 0)
                X = value;
            else if (index == 1)
                Y = value;
            else if (index == 2)
                Z = value;
            else
                W = value;
        }

        public override string ToString() => $"X={X}, Y={Y}, Z={Z}, W={W}";
    }
}