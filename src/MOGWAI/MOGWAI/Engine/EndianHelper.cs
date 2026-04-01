// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
            
            Array.Copy(data, bytes, bits / 8);
            Array.Reverse(bytes);
            
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}
