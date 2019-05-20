using System;
using System.IO;

namespace HDKReader
{
    public static class HDKDataReader
    {
        private static float[] SensorData = new float[7];
        private static byte[] TempBuffer = new byte[4];

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
                SensorData[4] = ToFloat(ReadInt16(stream), 9);
                SensorData[5] = ToFloat(ReadInt16(stream), 9);
                SensorData[6] = ToFloat(ReadInt16(stream), 9);
            }

            return ref SensorData;
        }

        public static ref float[] DecodeSensorData2(ref byte[] buffer, int size)
        {
            // Little endian
            //Array.Reverse(buffer);
            var index = 2;

            SensorData[0] = (float)ReadInt16(ref buffer, index) / (1 << 14);
            SensorData[1] = (float)ReadInt16(ref buffer, index + 2) / (1 << 14);
            SensorData[2] = (float)ReadInt16(ref buffer, index + 4)/ (1 << 14);
            SensorData[3] = (float)ReadInt16(ref buffer, index + 6) / (1 << 14);

            return ref SensorData;
        }

        private static float ToFloat(int data, int fractionalBytes) => ((float)data) / (1 << fractionalBytes);

        #region Read / Write

        private static sbyte ReadUInt8(ref byte[] buffer, int index)
        {
            return (sbyte)buffer[index++];
        }

        private static short ReadInt16(MemoryStream stream)
        {
            stream.Read(TempBuffer, 0, 4);
            return BitConverter.ToInt16(TempBuffer, 0);
        }

        private static ushort ReadInt16(ref byte[] arr, int index)
        {
            var b = new byte[] { arr[index], arr[index + 1], arr[index + 2], arr[index + 3] };
            return BitConverter.ToUInt16(arr, 0);
        }

        private static float ReadFloat(ref byte[] buffer, int index)
        {
            return BitConverter.ToSingle(buffer, index);
        }

        #endregion
    }
}
