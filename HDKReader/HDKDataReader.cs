namespace HDKReader
{
    public static class HDKDataReader
    {
        /// <summary>
        /// Decode the HDK Status.
        /// </summary>
        /// <param name="input">An array of raw data</param>
        /// <returns>Returns the status.</returns>
        public static HDKStatus DecodeStatus(byte[] input)
        {
            var firstByte = input[1];
            var version = 0x0f & firstByte;

            if (version >= 3)
            {
                var video = (firstByte & (0x01 << 4)) != 0;
                var portrait = (firstByte & (0x01 << 5)) != 0;

                if (video)
                    return portrait ? HDKStatus.PortraitVideoInput : HDKStatus.LandscapeVideoInput;

                return HDKStatus.NoVideoInput;
            }

            return HDKStatus.Unknown;
        }

        /// <summary>
        /// Decode the Quaternion from raw data.
        /// </summary>
        /// <param name="input">An array of raw data.</param>
        /// <param name="output">An array of 4 elements that represents the x, y, z and w components of a Quaternion</param>
        public static void DecodeQuaternion(byte[] input, float[] output)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var index = 3;
            output[0] = (float)ReadInt16(input, index) / (float)(1 << 14);
            output[1] = (float)ReadInt16(input, index + 2) / (float)(1 << 14);
            output[2] = (float)ReadInt16(input, index + 4) / (float)(1 << 14);
            output[3] = (float)ReadInt16(input, index + 6) / (float)(1 << 14);
        }

        /// <summary>
        /// Decode the Quaternion from raw data.
        /// </summary>
        /// <param name="input">An array of raw data.</param>
        /// <param name="x">The x component of a Quaternion</param>
        /// <param name="y">The y component of a Quaternion</param>
        /// <param name="z">The z component of a Quaternion</param>
        /// <param name="w">The w component of a Quaternion</param>
        public static void DecodeQuaternion(byte[] input, ref float x, ref float y, ref float z, ref float w)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var index = 3;
            x = (float)ReadInt16(input, index) / (float)(1 << 14);
            y = (float)ReadInt16(input, index + 2) / (float)(1 << 14);
            z = (float)ReadInt16(input, index + 4) / (float)(1 << 14);
            w = (float)ReadInt16(input, index + 6) / (float)(1 << 14);
        }

        /// <summary>
        /// Decode the Angular Velocity from raw data.
        /// </summary>
        /// <param name="input">An array of raw data.</param>
        /// <param name="output">An array of 3 elements that represents the x, y and z coordinates of a Vector</param>
        public static void DecodeAngularVelocity(byte[] input, float[] output)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var firstByte = input[1];
            var version = 0x0f & firstByte;

            if (version >= 2)
            {
                var index = 8;
                output[0] = (float)ReadInt16(input, index) / (float)(1 << 11);
                output[1] = (float)ReadInt16(input, index + 2) / (float)(1 << 11);
                output[2] = (float)ReadInt16(input, index + 4) / (float)(1 << 11);
            }
        }

        /// <summary>
        /// Decode the Angular Velocity from raw data.
        /// </summary>
        /// <param name="input">An array of raw data.</param>
        /// <param name="x">The x coordinate of a Vector</param>
        /// <param name="y">The y coordinate of a Vector</param>
        /// <param name="z">The z coordinate of a Vector</param>
        /// <param name="w">The w coordinate of a Vector</param>
        public static void DecodeAngularVelocity(byte[] input, ref float x, ref float y, ref float z)
        {
            // 0: 0
            // 1: Code
            // 2: Sequence
            var firstByte = input[1];
            var version = 0x0f & firstByte;

            if (version >= 2)
            {
                var index = 8;
                x = (float)ReadInt16(input, index) / (float)(1 << 11);
                y = (float)ReadInt16(input, index + 2) / (float)(1 << 11);
                z = (float)ReadInt16(input, index + 4) / (float)(1 << 11);
            }
        }

        /// <summary>
        /// Returns a 16 bits integer from raw data.
        /// </summary>
        /// <param name="data">An array of raw data.</param>
        /// <param name="index">The offset.</param>
        /// <returns>Returns</returns>
        public static short ReadInt16(byte[] data, int index)
        {
            return (short)(data[index] | data[index + 1] << 8);
        }
    }
}
