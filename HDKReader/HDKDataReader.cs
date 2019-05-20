using System;

namespace HDKReader
{
    public static class HDKDataReader
    {
        private static byte[] TempBuffer = new byte[2];

        public static void DecodeQuaternion(ref byte[] input, ref float[] output)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var index = 2;
            var x = (float)Read16(ref input, index) / (1 << 14);
            var y = (float)Read16(ref input, index + 2) / (1 << 14);
            var z = (float)Read16(ref input, index + 4) / (1 << 14);
            var w = (float)Read16(ref input, index + 6) / (1 << 14);

            output[0] = x;
            output[1] = y;
            output[2] = y;
            output[3] = w;
        }

        private static short Read16(ref byte[] arr, int index)
        {
            byte lsb = arr[index];
            byte msb = arr[index + 1];
            short result = (short)(lsb | msb << 8);
            return result;
        }
    }
}
