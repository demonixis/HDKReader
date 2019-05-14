using System;

namespace HDKReader
{
    public static class HDKDataReader
    {
        public static bool DecodeSensorConfig(ref byte[] buffer, int size, ref HDKVector3 acceleration, ref HDKQuaternion orientation)
        {
            if (size != 16)
            {
                //Console.WriteLine($"Invalid packet size (expected 16 but got {size}");
                //return false;
            }

            var index = 1;
            Read8(ref buffer, ref index); // Version
            Read8(ref buffer, ref index); // Sequence

            for (var i = 0; i < 4; i++)
                orientation.Set(i, Read16(ref buffer, ref index));

            for (var i = 0; i < 3; i++)
                acceleration.Set(i, Read16(ref buffer, ref index));

            return true;
        }

        #region Read / Write

        private static void SkipCommand(ref byte[] buffer, ref int index)
        {
            index++;
        }

        private static int Read8(ref byte[] buffer, ref int index)
        {
            return buffer[index++];
        }

        private static int Read16(ref byte[] buffer, ref int index)
        {
            var value = buffer[index] | (buffer[index + 1] << 8);
            index += 2;
            return value;
        }

        private static float Read162(ref byte[] buffer, ref int index)
        {
            var value = BitConverter.ToSingle(buffer, index);
            index += 2;
            return value;
        }

        private static int Read32(ref byte[] buffer, ref int index)
        {
            var value = buffer[index] | (buffer[index + 1] << 8) | (buffer[index + 2] << 16 | buffer[index + 3] << 24);
            index += 4;
            return value;
        }

        private static float ReadFloat(ref byte[] buffer, ref int index)
        {
            var value = (float)buffer[index];
            index += 4;
            return value;
        }

        private static float ReadFixed(ref byte[] buffer, ref int index)
        {
            var value = (float)(buffer[index] | (buffer[index + 1] << 8) | (buffer[index + 2] << 16) | (buffer[index + 3] << 24)) / 1000000.0f;
            index += 4;
            return value;
        }

        private static void Write8(int value, ref byte[] buffer, ref int index)
        {
            buffer[index++] = (byte)value;
        }

        private static void Write16(int value, ref byte[] buffer, ref int index)
        {
            Write8(value & 0xff, ref buffer, ref index);
            Write8((value >> 8) & 0xff, ref buffer, ref index);
        }

        private static void Write32(int value, ref byte[] buffer, ref int index)
        {
            Write16(value & 0xffff, ref buffer, ref index);
            Write16((value >> 16) & 0xffff, ref buffer, ref index);
        }

        #endregion
    }
}
