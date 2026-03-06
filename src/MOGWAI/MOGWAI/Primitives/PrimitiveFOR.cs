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
    internal class PrimitiveFOR : MOGPrimitive
    {
        public PrimitiveFOR(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveFOR(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // 1 2 'i' {...} FOR

            await Task.CompletedTask;

            var s = Engine.StackSign(4);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, this);

            if (s[0] == typeof(MOGCode) && s[1] == typeof(MOGName) && s[2] == typeof(MOGNumber) && s[3] == typeof(MOGNumber))
            {
                var code = Engine.StackPopCode();
                var name = Engine.StackPopName();
                var end = Engine.StackPopNumber();
                var start = Engine.StackPopNumber();

                var direction = Math.Sign(end!.Value - start!.Value);

                // If start and end are equal, execute the code once by forcing direction to 1
                
                if (direction == 0)
                    direction = 1;

                EvalResult result = EvalResult.NoError;

                if (Engine.StrictMode && !Engine.VarExists(name.Value))
                {
                    // We must declare the variable before using it
                    // Of type .any

                    var r = Engine.VarDeclareForType(name.Value, Engine.GetType("any")!);

                    if (r != EvalResult.NoError)
                        return r;
                }

                Engine.CreateBreakRequest();

                for (double i = start.Value; direction > 0 ? i <= end.Value : i >= end.Value; i += direction)
                {
                    if (Engine.BreakRequested || Engine.ExitRequested || Engine.ReturnRequested)
                        break;

                    result = Engine.VarWrite(name.Value, new MOGNumber(Engine, i, 0));
                    if (result != EvalResult.NoError)
                        break;

                    result = await code.Execute();

                    if (result != EvalResult.NoError)
                        break;
                }

                Engine.RemoveBreakRequest();

                return result;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, this);
        }
    }
}
