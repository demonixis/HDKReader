namespace HDKReader
{
    public static class HDKDataReader
    {
        public static void DecodeQuaternion(ref byte[] input, ref float[] output)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var index = 3;
            output[0] = (float)Read16(ref input, index) / (float)(1 << 14);
            output[1] = (float)Read16(ref input, index + 2) / (float)(1 << 14);
            output[2] = (float)Read16(ref input, index + 4) / (float)(1 << 14);
            output[3] = (float)Read16(ref input, index + 6) / (float)(1 << 14);
        }

        public static void DecodeQuaternion(ref byte[] input, ref float x, ref float y, ref float z, ref float w)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var index = 3;
            x = (float)Read16(ref input, index) / (float)(1 << 14);
            y = (float)Read16(ref input, index + 2) / (float)(1 << 14);
            z = (float)Read16(ref input, index + 4) / (float)(1 << 14);
            w = (float)Read16(ref input, index + 6) / (float)(1 << 14);
        }

        public static void DecodeAcceleration(ref byte[] input, ref float[] output)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var index = 8;
            output[0] = (float)Read16(ref input, index) / (float)(1 << 11);
            output[1] = (float)Read16(ref input, index + 2) / (float)(1 << 11);
            output[2] = (float)Read16(ref input, index + 4) / (float)(1 << 11);
        }

        public static void DecodeAcceleration(ref byte[] input, ref float x, ref float y, ref float z)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var index = 8;
            x = (float)Read16(ref input, index) / (float)(1 << 11);
            y = (float)Read16(ref input, index + 2) / (float)(1 << 11);
            z = (float)Read16(ref input, index + 4) / (float)(1 << 11);
        }

        public static short Read16(ref byte[] arr, int index)
        {
            return (short)(arr[index] | arr[index + 1] << 8);
        }
    }
}
