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
using MOGWAI.Objects;

namespace MOGWAI.Primitives
{
    internal class PrimitiveTestBitFromBinaryNumber : MOGPrimitive
    {
        public override Version Birth => new(8, 5, 0);

        public PrimitiveTestBitFromBinaryNumber(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGBinaryNumber))
            {
                var number = Engine.StackPopNumber();
                var binaryNumber = Engine.StackPopBinaryNumber();

                if (number.IntValue < 0 || number.IntValue > binaryNumber.Size - 1)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));

                Engine.StackPushBoolean(binaryNumber.Items[number.IntValue]);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveTestBitFromBinaryNumber(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
    }
}
