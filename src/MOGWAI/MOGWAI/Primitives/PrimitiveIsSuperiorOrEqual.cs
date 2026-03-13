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
    internal class PrimitiveIsSuperiorOrEqual : MOGPrimitive
    {
        public PrimitiveIsSuperiorOrEqual(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveIsSuperiorOrEqual(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
        public override Task<EvalResult> EngineEval()
        {
            // xxx yyyy >=

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGNumber))
            {
                // number number ==

                var n2 = Engine.StackPopNumber();
                var n1 = Engine.StackPopNumber();

                var r = new MOGBoolean(Engine, n1.Value >= n2.Value, 0);
                Engine.StackPush(r);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
