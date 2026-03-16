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
    internal class PrimitiveRight : MOGPrimitive
    {
        public PrimitiveRight(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            // string number right
            // data number right

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGString))
            {
                var number = Engine.StackPopNumber();
                var @string = Engine.StackPopString();

                if (number!.IntValue < 1)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "characters number must be >0."));

                if (@string.Value.Length < number.IntValue)
                {
                    Engine.StackPushString(@string.Value);
                }
                else
                {
                    Engine.StackPushString(@string.Value.Substring(@string.Value.Length - number.IntValue, number.IntValue));
                }

                return Task.FromResult(EvalResult.NoError);
            }
            else if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGData))
            {
                var number = Engine.StackPopNumber();
                var data = Engine.StackPopData();

                if (number.IntValue < 1)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "bytes number must be >0."));

                if (data!.Items.Count < number.IntValue)
                {
                    Engine.StackPush(data);
                }
                else
                {
                    var items = new List<byte>();

                    for (int i = data.Items.Count - number.IntValue; i < data.Items.Count; i++)
                        items.Add(data.Items[i]);

                    data.Items = items;

                    Engine.StackPush(data);
                }

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
