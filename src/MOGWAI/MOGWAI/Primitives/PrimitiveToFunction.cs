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
    internal class PrimitiveToFunction : MOGPrimitive
    {
        public PrimitiveToFunction(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGCode))
            {
                var code = Engine.StackPopCode();
                Engine.StackPush(code.ToFunction());
                return Task.FromResult(EvalResult.NoError);
            }
            else if (s[0] == typeof(MOGList))
            {
                var list = Engine.StackPopList();
                Engine.StackPush(list.ToCode().ToFunction());
                return Task.FromResult(EvalResult.NoError);
            }
            else if (s[0] == typeof(MOGString))
            {
                var @string = Engine.StackPopString();
                List<MOGObject>? items = null;

                try
                {
                    items = Engine.Parse(@string.Value);
                }
                catch (Exception ex)
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.ParseError, ex.Message));
                }

                var func = new MOGFunction(Engine, items);
                Engine.StackPush(func);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
