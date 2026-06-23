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
using System.Text;

namespace MOGWAI.Primitives
{
    internal class PrimitiveStrPadRight : MOGPrimitive
    {
        public override Version Birth => new(8, 11, 0);

        public PrimitiveStrPadRight(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveStrPadRight(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGString))
            {
                var number = Engine.StackPopNumber();
                var @string = Engine.StackPopString();  

                if (number.IntValue < 0)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "number must be non-negative."));

                Engine.StackPushString(@string.Value.PadRight(number.IntValue));

                return Task.FromResult(EvalResult.NoError);
            }
    
            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
