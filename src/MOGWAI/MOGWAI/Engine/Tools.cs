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

using Microsoft.VisualBasic;
using System.IO.Compression;
using System.Text;

namespace MOGWAI.Engine
{
    internal static class Tools
    {
        internal static bool Like(string text, string pattern)
        {
            return Microsoft.VisualBasic.CompilerServices.LikeOperator.LikeString(text, pattern, CompareMethod.Text);
        }

        internal static string BeginOfString(string s, int size)
        {
            if (s.Length > size)
                return s.Substring(0, size) + "...";

            return s;
        }

        internal static string ToUnicodeEscaped(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                if (c < 32)
                {
                    // Caractères de contrôle

                    sb.AppendFormat("\\u{0:X4}", (int)c);
                }
                else if (c > 127)
                {
                    // Caractères non-ASCII

                    sb.AppendFormat("\\u{0:X4}", (int)c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        internal static byte[] Compress(byte[] bytes)
        {
            using (MemoryStream memoryStream = new())
            {
                using (GZipStream gzipStream = new(memoryStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                    gzipStream.Flush();

                    return memoryStream.ToArray();
                }
            }
        }

        internal static byte[] Decompress(byte[] bytes)
        {
            using (MemoryStream memoryStream = new(bytes))
            {
                using (MemoryStream outputStream = new())
                {
                    using (GZipStream decompressStream = new(memoryStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                        decompressStream.Flush();

                        return outputStream.ToArray();
                    }
                }
            }
        }
    }
}
