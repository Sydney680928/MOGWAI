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

namespace MOGWAI.Objects
{
    public class MOGObjectReference : MOGObject
    {
        public int Value { get; set; }

        public MOGObjectReference(MogwaiEngine engine, int value) : base(engine)
        {
            Type = engine.GetType(typeof(MOGObjectReference));
            Value = value;
        }

        public MOGObjectReference(MogwaiEngine engine, int value, int originPosition) : this(engine, value)
        {
            if (originPosition > -1)
            {
                StartPos = originPosition;
                EndPos = originPosition + value.ToString(System.Globalization.CultureInfo.InvariantCulture).Length;
            }
        }

        public override MOGObjectReference Clone()
        {
            var obj = new MOGObjectReference(Engine, Value, StartPos);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override string ToString()
        {
            return $"§{Value}";
        }

        public override string ToJson()
        {
            return $"§{Value}";
        }
    }
}
