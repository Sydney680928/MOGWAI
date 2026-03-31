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
    internal class PrimitiveErrorGuard : MOGPrimitive
    {
        public PrimitiveErrorGuard(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveErrorGuard(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGCode) && s[1] == typeof(MOGCode))
            {
                var errorCode = Engine.StackPopCode();
                var code = Engine.StackPopCode();

                var backup = Engine.StackSave();

                var result = await code.Execute();

                if (result != EvalResult.NoError)
                {
                    var result2 = Engine.StackRestore(backup);

                    if (result2 != EvalResult.NoError)
                        return result2; 

                    Engine.LastError = result.Error;
                    
                    return await errorCode.Execute();
                }

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
