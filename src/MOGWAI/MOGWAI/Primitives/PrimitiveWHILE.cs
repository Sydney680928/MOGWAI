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
    internal class PrimitiveWHILE : MOGPrimitive
    {
        public PrimitiveWHILE(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveWHILE(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
        public override async Task<EvalResult> EngineEval()
        {
            // condition code WHILE

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGCode) && s[1] == typeof(MOGCode))
            {
                var code = Engine.StackPopCode();
                var condition = Engine.StackPopCode();

                Engine.CreateBreakRequest();

                var result = EvalResult.NoError;

                while (true)
                {
                    var initialStackSize = Engine.StackSize;

                    result = await condition.Execute();
                    if (result != EvalResult.NoError)
                        break;

                    var currentStackSize = Engine.StackSize;
                    if (currentStackSize != initialStackSize + 1)
                    {
                        result = EvalResult.Failure(Engine, Error.StackSizeError, Name, Name);
                        break;
                    }

                    var conditionResult = Engine.StackPop() as MOGBoolean;
                    if (conditionResult == null)
                    {
                        result = EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
                        break;
                    }

                    if (conditionResult.Value)
                    {
                        result = await code.Execute();

                        if (result != EvalResult.NoError)
                            break;

                        if (Engine.ExitRequested || Engine.BreakRequested)
                            break;
                    }
                    else
                    {
                        result = EvalResult.NoError;
                        break;
                    }
                }

                Engine.RemoveBreakRequest();

                return result;

            }
            else

                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
