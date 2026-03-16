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
    internal class PrimitiveSTODIVIDE : MOGPrimitive
    {
        public PrimitiveSTODIVIDE(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            // number name STO/


            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGName) && s[1] == typeof(MOGNumber))
            {
                var name = Engine.StackPopName();
                var number = Engine.StackPopNumber();

                if (number.Value == 0)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.DivisionByZeroError, Name));

                var value = Engine.VarRead(name.Value);

                if (value == null)
                {
                    Engine.VarWrite(name.Value, new MOGNumber(Engine, 0, 0));
                }
                else
                {
                    if (value is MOGNumber numberValue)
                    {
                        Engine.VarWrite(name.Value, new MOGNumber(Engine, numberValue.Value / number.Value, 0));
                    }
                    else
                    {
                        Engine.VarWrite(name.Value, new MOGNumber(Engine, 0, 0));
                    }
                }

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
