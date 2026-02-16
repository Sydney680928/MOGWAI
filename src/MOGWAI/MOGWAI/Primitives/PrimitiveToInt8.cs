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

        public override MOGObject Clone()
        {
            var obj = new PrimitiveToInt8(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            var n0 = Engine.StackPop();

            if (n0 is MOGData data)
            {
                if (data.Items.Count == 0)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, ".data is empty.");

                Engine.StackPushNumber(data.Items[0]);
                return EvalResult.NoError;
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
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, ex.Message);
                }

                var d = new MOGData(Engine);
                d.Items.Add((byte)b);

                Engine.StackPush(d);

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
