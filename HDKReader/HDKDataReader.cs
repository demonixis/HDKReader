using System;
using System.IO;

namespace HDKReader
{
    public static class HDKDataReader
    {
        private static float[] SensorData = new float[7];
        private static byte[] TempBuffer = new byte[4];

        public static bool DecodeSensorData(ref byte[] buffer, int size, ref Vector3 acceleration, ref Quaternion orientation)
        {
            var index = 1;

            if (size - index != 16)
            {
                Console.WriteLine($"Invalid packet size (expected 16 but got {size}");
                return false;
            }

            Read8(ref buffer, ref index); // Version
            Read8(ref buffer, ref index); // Sequence

            orientation.Set(
                Read16(ref buffer, ref index),
                Read16(ref buffer, ref index),
                Read16(ref buffer, ref index),
                Read16(ref buffer, ref index));

            acceleration.Set(
                Read16(ref buffer, ref index),
                Read16(ref buffer, ref index),
                Read16(ref buffer, ref index));

            return true;
        }

        public static ref float[] DecodeSensorData(ref byte[] buffer, int size)
        {
            if (size - 1 != 16)
            {
                Console.WriteLine($"Invalid packet size (expected 16 but got {size}");
                return ref SensorData;
            }

            using (var stream = new MemoryStream(buffer))
            {
                stream.Position = 3;

                // Orientation
                SensorData[0] = ToFloat(ReadInt16(stream), 14);
                SensorData[1] = ToFloat(ReadInt16(stream), 14);
                SensorData[2] = ToFloat(ReadInt16(stream), 14);
                SensorData[3] = ToFloat(ReadInt16(stream), 14);
                // Acceleration
                SensorData[4] = ToFloat(ReadInt16(stream), 14);
                SensorData[5] = ToFloat(ReadInt16(stream), 14);
                SensorData[6] = ToFloat(ReadInt16(stream), 14);
            }

            return ref SensorData;
        }

        private static short ReadInt16(MemoryStream stream)
        {
            stream.Read(TempBuffer, 0, 4);
            return BitConverter.ToInt16(TempBuffer, 0);
        }

        private static float ToFloat(int data, int btw) => ((float)data) / (1 << btw);

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
