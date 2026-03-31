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
    internal class PrimitiveLATER : MOGPrimitive
    {
        public PrimitiveLATER(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveLATER(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // function interval LATER

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGFunction))
            {
                var interval = Engine.StackPopNumber();
                var function = Engine.StackPopFunction();

                var name = Guid.NewGuid().ToString();
                var result = Engine.CreateNewTimer(name, interval.IntValue, false, function, true);

                if (result == EvalResult.NoError)
                {
                    var timer = Engine.GetTimer(name);

                    if (timer != null)
                    {
                        result = timer.Start();
                    }
                    else
                    {
                        result = EvalResult.Failure(Engine, Error.UnknownNameError, Name, name);
                    }
                }

                return Task.FromResult(result);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
