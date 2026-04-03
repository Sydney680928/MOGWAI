namespace MOGWAI.Engine
{
    internal static class EndianHelper
    {
        public static byte[] ToDataLE(long value, int bits)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes.Take(bits / 8).ToArray();
        }

        public static byte[] ToDataBE(long value, int bits)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            Array.Reverse(bytes);

            return bytes.Skip(8 - bits / 8).ToArray();
        }

        public static long FromDataLE(byte[] data, int bits)
        {
            var bytes = new byte[8];

            Array.Copy(data, bytes, bits / 8);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt64(bytes, 0);
        }

        public static long FromDataBE(byte[] data, int bits)
        {
            var bytes = new byte[8];

            int byteCount = bits / 8;

            Array.Copy(data, 0, bytes, 8 - byteCount, byteCount);
            Array.Reverse(bytes);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt64(bytes, 0);
        }

        public static byte[] ToDataLEFloat32(float value)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        public static byte[] ToDataBEFloat32(float value)
        {
            var bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        public static float FromDataLEFloat32(byte[] data)
        {
            var bytes = new byte[4];
            Array.Copy(data, bytes, 4);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToSingle(bytes, 0);
        }

        public static float FromDataBEFloat32(byte[] data)
        {
            var bytes = new byte[4];
            Array.Copy(data, bytes, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToSingle(bytes, 0);
        }

        public static byte[] ToDataLEFloat64(double value)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        public static byte[] ToDataBEFloat64(double value)
        {
            var bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        public static double FromDataLEFloat64(byte[] data)
        {
            var bytes = new byte[8];
            Array.Copy(data, bytes, 8);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToDouble(bytes, 0);
        }

        public static double FromDataBEFloat64(byte[] data)
        {
            var bytes = new byte[8];
            Array.Copy(data, bytes, 8);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToDouble(bytes, 0);
        }
    }
}

