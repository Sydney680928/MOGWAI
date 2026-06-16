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
    internal abstract class PrimitiveParamsStringString : MOGPrimitive
    {
        public PrimitiveParamsStringString(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public abstract Task<EvalResult> PerformOperation(MOGString @string1, MOGString @string2);

        public override Task<EvalResult> EngineEval()
        {
            // string1 string2 operation

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGString) && s[1] == typeof(MOGString))
            {
                var @string2 = Engine.StackPopString();
                var @string1 = Engine.StackPopString(); 

                return PerformOperation(@string1, @string2);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
