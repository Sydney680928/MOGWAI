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
    internal class PrimitiveIFELSE : MOGPrimitive
    {
        public PrimitiveIFELSE(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveIFELSE(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
        public override async Task<EvalResult> EngineEval()
        {
            // bool codeTrue codeFalse IFELSE

            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[2] == typeof(MOGBoolean) && s[1] == typeof(MOGCode) && s[0] == typeof(MOGCode))
            {
                var codeFalse = Engine.StackPopCode();
                var codeTrue = Engine.StackPopCode();
                var b = Engine.StackPopBoolean();

                if (b.Value)
                {
                    return await codeTrue.Execute();
                }
                else
                {
                    return await codeFalse.Execute();
                }
            }
            else
            {
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
            }
        }
    }
}
