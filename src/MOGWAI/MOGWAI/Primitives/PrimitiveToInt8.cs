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
    internal class PrimitiveToInt8 : MOGPrimitive
    {
        public PrimitiveToInt8(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            var n0 = Engine.StackPop();

            if (n0 is MOGData data)
            {
                if (data.Items.Count == 0)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, ".data is empty."));

                Engine.StackPushNumber(data.Items[0]);
                return Task.FromResult(EvalResult.NoError);
            }
            else if (n0 is MOGNumber number)
            {
                sbyte b = 0;

                try
                {
                    b = (sbyte)number.IntValue;
                }
                catch (Exception ex)
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, ex.Message));
                }

                var d = new MOGData(Engine);
                d.Items.Add((byte)b);

                Engine.StackPush(d);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
