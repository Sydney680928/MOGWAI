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
    internal class PrimitiveToData : MOGPrimitive
    {
        public PrimitiveToData(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGNumber))
            {
                var n0 = Engine.StackPopNumber();

                if (n0.IntValue > Engine.StackSize)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

                var stk = Engine.StackArray();

                for (int i = 0; i < n0.IntValue; i++)
                {
                    if (stk[i] is MOGNumber number && number.IntValue >= 0 && number.IntValue <= 255)
                    {

                    }
                    else
                    {
                        return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "only numbers between 0 and 255 are allowed."));
                    }
                }

                var data = new MOGData(Engine);

                for (int i = 0; i < n0.IntValue; i++)
                {
                    var n = Engine.StackPopNumber();
                    data.Items.Insert(0, (byte)n.IntValue);
                }

                Engine.StackPush(data);

                return Task.FromResult(EvalResult.NoError);
            }
            else if (s[0] == typeof(MOGList))
            {
                var n0 = Engine.StackPopList();
                var bytes = new List<byte>();

                foreach (var item in n0.Items)
                {
                    if (item is MOGNumber number && number.IntValue >= 0 && number.IntValue <= 255)
                    {
                        bytes.Add((byte)number.IntValue);
                    }
                    else
                    {
                        return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "only numbers between 0 and 255 are allowed in the input list."));
                    }
                }

                var data = new MOGData(Engine, bytes);
                Engine.StackPush(data);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
