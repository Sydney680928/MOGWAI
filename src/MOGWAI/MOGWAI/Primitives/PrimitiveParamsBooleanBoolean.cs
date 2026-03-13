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
    internal abstract class PrimitiveParamsBooleanBoolean : MOGPrimitive
    {
        protected PrimitiveParamsBooleanBoolean(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public abstract Task<EvalResult> PerformOperation(MOGBoolean bool1, MOGBoolean bool2);

        public override Task<EvalResult> EngineEval()
        {
            // boolean boolean operation (ex and or xor)

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGBoolean) && s[1] == typeof(MOGBoolean))
            {
                var b1 = Engine.StackPopBoolean();
                var b2 = Engine.StackPopBoolean();

                return PerformOperation(b1, b2);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
