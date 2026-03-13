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
    internal class PrimitiveRcl : MOGPrimitive
    {
        public PrimitiveRcl(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveRcl(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // 'A' rcl

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] != typeof(MOGName))
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));

            var name = Engine.StackPopName();
            var value = Engine.VarRead(name.Value);

            if (value == null)
                return Task.FromResult(EvalResult.Failure(Engine, Error.UnknownNameError, Name, name.ToString()));

            Engine.StackPush(value);
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
