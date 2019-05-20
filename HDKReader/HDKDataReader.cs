using System;

namespace HDKReader
{
    public static class HDKDataReader
    {
        private static byte[] TempBuffer = new byte[2];

        public static void DecodeQuaternion(ref byte[] input, ref float[] output)
        {
            var index = 2;
            output[0] = (float)ReadInt16(ref input, index) / (1 << 15);
            output[1] = (float)ReadInt16(ref input, index + 2) / (1 << 15);
            output[2] = (float)ReadInt16(ref input, index + 4)/ (1 << 15);
            output[3] = (float)ReadInt16(ref input, index + 6) / (1 << 15);
        }

        #region Read / Write

        private static ushort ReadInt16(ref byte[] arr, int index)
        {
            TempBuffer[0] = arr[index];
            TempBuffer[1] = arr[index + 1];

            return BitConverter.ToUInt16(TempBuffer, 0);
        }

        #endregion
    }
}
