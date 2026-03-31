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
    internal class PrimitivePrintLn : MOGPrimitive
    {
        public PrimitivePrintLn(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitivePrintLn(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            if (Engine.StackSize == 0)
            {
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);
            }

            var obj = Engine.StackPop();
            string message;

            if (obj is MOGString s)
            {
                message = s.Value;
            }
            else
            {
                message = obj!.ToString();
            }

            if (Engine.Delegate != null)
                await Engine.Delegate.ConsolePrintLn(Engine, message);

            return EvalResult.NoError;
        }
    }
}
