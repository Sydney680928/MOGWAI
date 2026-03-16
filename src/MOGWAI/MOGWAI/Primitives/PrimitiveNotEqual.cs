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
    internal class PrimitiveNotEqual : MOGPrimitive
    {
        public PrimitiveNotEqual(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            // xxx yyyy !=

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGNumber))
            {
                var n2 = Engine.StackPopNumber();
                var n1 = Engine.StackPopNumber();

                Engine.StackPush(new MOGBoolean(Engine, n1.Value != n2.Value, 0));
            }
            else if (s[0] == typeof(MOGBoolean) && s[1] == typeof(MOGBoolean))
            {
                var n2 = Engine.StackPopBoolean();
                var n1 = Engine.StackPopBoolean();

                Engine.StackPush(new MOGBoolean(Engine, n1.Value != n2.Value, 0));
            }
            else if (s[0].IsSubclassOf(typeof(MOGBaseString)) && s[1].IsSubclassOf(typeof(MOGBaseString)))
            {
                var n2 = Engine.StackPopBaseString();
                var n1 = Engine.StackPopBaseString();

                Engine.StackPush(new MOGBoolean(Engine, n1.Value != n2.Value, 0));
            }
            else
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
            }

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
