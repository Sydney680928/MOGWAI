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
    public class MOGNumber : MOGObject
    {
        public double Value { get; set; }

        public int IntValue => (int)Value;

        public MOGNumber(MogwaiEngine engine, double value) : base(engine)
        {
            Type = engine.GetType(typeof(MOGNumber));
            Value = value;
        }

        public MOGNumber(MogwaiEngine engine, double value, int originPosition) : this(engine, value)
        {
            StartPos = originPosition;
            EndPos = originPosition + value.ToString(System.Globalization.CultureInfo.InvariantCulture).Length - 1;
        }

        public override MOGNumber Clone()
        {
            var obj = new MOGNumber(Engine, Value, StartPos);
            obj.EndPos = EndPos;
            return obj;
        }

        public override string ToString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public override string ToJson()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
