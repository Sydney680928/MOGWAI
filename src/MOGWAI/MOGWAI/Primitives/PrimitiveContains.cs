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
    internal class PrimitiveContains : MOGPrimitive
    {
        public PrimitiveContains(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            // (1 2 3) 3 contains ---> true
            // [id: 50 name: "SIBUE"] name: contains ---> true
            // DATA:EB5600FF 0x56 contains ---> true
            // "ERERR" "RT" contains --> true

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            var n0 = Engine.StackPop();
            var n1 = Engine.StackPop();
            var ss = n0!.ToString();

            if (n1 is MOGList list)
            {
                foreach (var item in list.Items)
                {
                    if (item.ToString() == ss)
                    {
                        Engine.StackPushBoolean(true);
                        return Task.FromResult(EvalResult.NoError);
                    }
                }

                Engine.StackPushBoolean(false);
                return Task.FromResult(EvalResult.NoError);
            }
            else if (n1 is MOGRecord record)
            {
                if (n0 is MOGKey key)
                {
                    foreach (var k in record.Items.Keys)
                    {
                        if (k == key.Value)
                        {
                            Engine.StackPushBoolean(true);
                            return Task.FromResult(EvalResult.NoError);
                        }
                    }

                    Engine.StackPushBoolean(false);
                    return Task.FromResult(EvalResult.NoError);
                }

                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, ".key expected."));
            }
            else if (n1 is MOGData data)
            {
                if (n0 is MOGNumber n)
                {
                    foreach (var bb in data.Items)
                    {
                        if (bb == n.Value)
                        {
                            Engine.StackPushBoolean(true);
                            return Task.FromResult(EvalResult.NoError);
                        }
                    }

                    Engine.StackPushBoolean(false);
                    return Task.FromResult(EvalResult.NoError);
                }
                else
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError));
                }
            }
            else if (n1 is MOGString str)
            {
                if (n0 is MOGString str2)
                {
                    var b = (str.Value.IndexOf(str2.Value) > -1);
                    Engine.StackPushBoolean(b);
                    return Task.FromResult(EvalResult.NoError);
                }

                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, ".string expected."));
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
