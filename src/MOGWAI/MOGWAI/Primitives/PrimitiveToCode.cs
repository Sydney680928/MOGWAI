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
    internal class PrimitiveToCode : MOGPrimitive
    {
        public PrimitiveToCode(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveToSHA256(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGFunction))
            {
                var function = Engine.StackPopFunction();
                Engine.StackPush(function.ToCode());
                return Task.FromResult(EvalResult.NoError);
            }
            else if (s[0] == typeof(MOGList))
            {
                var list = Engine.StackPopList();
                Engine.StackPush(list.ToCode());
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

                var code = new MOGCode(Engine, items);
                Engine.StackPush(code);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
