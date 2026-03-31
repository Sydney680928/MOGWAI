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
    internal class PrimitiveAFTER : MOGPrimitive
    {
        public PrimitiveAFTER(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            // function interval name AFTER

            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGName) && s[1] == typeof(MOGNumber) && s[2] == typeof(MOGFunction))
            {
                var name = Engine.StackPopName();
                var interval = Engine.StackPopNumber();
                var function = Engine.StackPopFunction();

                return Task.FromResult(Engine.CreateNewTimer(name.Value, interval.IntValue, false, function));
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveAFTER(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
    }
}
