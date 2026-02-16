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

        public override MOGObject Clone()
        {
            var obj = new PrimitiveNotEqual(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
        public override async Task<EvalResult> EngineEval()
        {
            // xxx yyyy !=

            await Task.CompletedTask;

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGNumber))
            {
                // number number !=

                var n2 = Engine.StackPopNumber();
                var n1 = Engine.StackPopNumber();

                var r = new MOGBoolean(Engine, n1.Value != n2.Value, 0);
                Engine.StackPush(r);
            }
            else if (s[0] == typeof(MOGBoolean) && s[1] == typeof(MOGBoolean))
            {
                // bool bool !=

                var n2 = Engine.StackPopBoolean();
                var n1 = Engine.StackPopBoolean();

                var r = new MOGBoolean(Engine, n1.Value != n2.Value, 0);
                Engine.StackPush(r);
            }
            else if (s[0].IsSubclassOf(typeof(MOGBaseString)) && s[1].IsSubclassOf(typeof(MOGBaseString)))
            {
                // string string !=
                // name name !=
                // key key !=
                // word word !=
                // type type !=

                var n2 = Engine.StackPopBaseString();
                var n1 = Engine.StackPopBaseString();

                var r = new MOGBoolean(Engine, n1.Value != n2.Value, 0);
                Engine.StackPush(r);
            }
            else
            {
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
            }

            return EvalResult.NoError;
        }

    }
}
