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
using System.Globalization;
using System.Text;

namespace MOGWAI.Objects
{
    public class MOGData : MOGObject
    {
        public List<byte> Items = new();

        public MOGData(MogwaiEngine engine) : base(engine)
        {
            Type = engine.GetType(typeof(MOGData));
        }

        public MOGData(MogwaiEngine engine, List<byte> items) : this(engine)
        {
            Items.AddRange(items);
        }

        public MOGData(MogwaiEngine engine, byte[] items) : this(engine)
        {
            Items.AddRange(items);
        }

        public MOGData(MogwaiEngine engine, string content, int originPosition) : this(engine)
        {
            // content = FF45AE12
            // taille paire
            // Composée QUE de valeurs hexa sur 2 caractères

            if (content.Length % 2 != 0)
                throw new InvalidDataException("content must be a collection of hex bytes.");

            var bytes = new List<byte>(content.Length / 2);

            for (int i = 0; i < content.Length; i += 2)
            {
                if (byte.TryParse(content.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    bytes.Add(b);
                }
                else
                {
                    throw new MogwaiInvalidRecordException("content must be a collection of hex bytes.");
                }
            }

            Items = bytes;

            Code = content;
            StartPos = originPosition;
            EndPos = originPosition + content.Length + 1;
        }

        public EvalResult RemoveItem(int index)
        {
            if (index < 0 || index >= Items.Count)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError);

            Items.RemoveAt(index);
            return EvalResult.NoError;
        }

        public override MOGObject Clone()
        {
            var obj = new MOGData(Engine, Items);
            obj.StartPos = StartPos;
            obj.EndPos = EndPos;
            return obj;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var item in Items)
                sb.Append(item.ToString("X02"));

            return $"D:{sb.ToString()}";
        }
    }
}
