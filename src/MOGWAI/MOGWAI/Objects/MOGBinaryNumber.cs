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

using MOGWAI.Engine;
using MOGWAI.Exceptions;
using System.Text;

namespace MOGWAI.Objects
{
    public class MOGBinaryNumber : MOGObject
    {
        private static int[] _powers = new int[64];

        public List<bool> Items { get; private set; } = new();

        public int Size => Items.Count;

        static MOGBinaryNumber()
        {
            for (int i = 0; i < 64; i++)
                _powers[i] = (int)Math.Pow(2, i);
        }

        internal static MOGBinaryNumber FromNumber(MOGNumber number, int size = 0)
        {
            var bytes = BitConverter.GetBytes(number.IntValue);
            var bits = new List<bool>();

            for (int i = 0; i < bytes.Length; i++)
                for (int bit = 0; bit < 8; bit++)
                    bits.Add((bytes[i] & (1 << bit)) != 0);

            while (bits[bits.Count - 1] == false && bits.Count > 1)
                bits.RemoveAt(bits.Count - 1);

            if (size > 0)
                while (bits.Count < size)
                    bits.Add(false);

            return new MOGBinaryNumber(number.Engine, bits);
        }

        public MOGBinaryNumber(MogwaiEngine engine, int size) : base(engine)
        {
            Type = engine.GetType(typeof(MOGBinaryNumber));

            if (size < 1)
            {
                throw new MogwaiInvalidBinaryNumberException("Binary number size must be a value from 1 to 64 bits");
            }
            else if (size > 64)
            {
                throw new MogwaiInvalidBinaryNumberException("Binary number cannot exceed 64 bits.");
            }

            for (int i = 0; i < size; i++)
                Items.Add(false);
        }

        public MOGBinaryNumber(MogwaiEngine engine, List<bool> items) : base(engine)
        {
            Type = engine.GetType(typeof(MOGBinaryNumber));

            if (items.Count > 64)
                throw new MogwaiInvalidBinaryNumberException("Binary number cannot exceed 6 bits.");

            Items.AddRange(items);
        }

        public MOGBinaryNumber(MogwaiEngine engine, bool[] items) : base(engine)
        {
            Type = engine.GetType(typeof(MOGBinaryNumber));

            if (items.Length > 64)
                throw new MogwaiInvalidBinaryNumberException("binary number cannot exceed 64 bits.");

            Items.AddRange(items);
        }

        public MOGBinaryNumber(MogwaiEngine engine, string content) : base(engine)
        {
            Type = engine.GetType(typeof(MOGBinaryNumber));

            if (content.Length > 64)
                throw new MogwaiInvalidBinaryNumberException("binary number cannot exceed 64 bits.");

            foreach (char c in content)
            {
                if (c == '0')
                {
                    Items.Insert(0, false);
                }
                else if (c == '1')
                {
                    Items.Insert(0, true);
                }
                else
                {
                    throw new MogwaiInvalidBinaryNumberException($"invalid character '{c}' in binary string.");
                }
            }
        }

        public MOGBinaryNumber(MogwaiEngine engine, string content, int originPosition) : this(engine, content)
        {
            Code = content;
            StartPos = originPosition;
            EndPos = originPosition + content.Length + 1;
        }

        public MOGNumber ToNumber()
        {
            int value = 0;

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i])
                    value += _powers[i];
            }

            return new MOGNumber(Engine, value);
        }

        public void LeftShift(int count)
        {
            if (count > Items.Count)
                count = Items.Count;

            for (int i = 0; i < count; i++)
            {
                Items.Insert(0, false);
                Items.RemoveAt(Items.Count - 1);
            }
        }

        public void RightShift(int count)
        {
            if (count > Items.Count)
                count = Items.Count;

            for (int i = 0; i < count; i++)
            {
                Items.Add(false);
                Items.RemoveAt(0);
            }
        }

        public void Up(int position)
        {
            if (position >= 0 && position < Items.Count)
                Items[position] = true;
        }

        public void Down(int position)
        {
            if (position >= 0 && position < Items.Count)
                Items[position] = false;
        }

        public void Not()
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i] = !Items[i];
        }

        public override MOGObject Clone()
        {
            var obj = new MOGBinaryNumber(Engine, Items);
            obj.StartPos = StartPos;
            obj.EndPos = EndPos;
            return obj;
        }

        public string ToBinString()
        {
            var sb = new StringBuilder();

            for (int i = Items.Count - 1; i >= 0; i--)
                sb.Append(Items[i] ? "1" : "0");

            return sb.ToString();
        }

        public override string ToString()
        {
            return "B:" + ToBinString();
        }

        public override string ToJson()
        {
            return "B:" + ToBinString();
        }
    }
}
