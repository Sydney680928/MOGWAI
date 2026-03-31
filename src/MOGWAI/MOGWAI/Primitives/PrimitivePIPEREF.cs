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
    internal class PrimitivePIPEREF : MOGPrimitive
    {
        public PrimitivePIPEREF(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitivePIPEREF(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // ref list PIPEREF

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, this);

            if (s[0] == typeof(MOGList) && s[1] == typeof(MOGRef))
            {
                var actions = Engine.StackPopList();
                var target = Engine.StackPopRef();

                var targetValue = Engine.VarRead(target.Value, false);

                if (targetValue == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, this, target.Value);

                if (actions.Items.Count == 0)
                    return EvalResult.NoError;

                var savedTargetValue = targetValue.Clone();

                Engine.AddNewStack();

                Engine.StackPush(targetValue);

                foreach (var action in actions.Items)
                {
                    action.AutoEval = true;

                    var r = await action.EngineEval();

                    if (r.IsError)
                    {
                        Engine.RemoveLastStack();
                        Engine.VarWrite(target.Value, savedTargetValue);
                        return r;
                    }
                }

                Engine.RemoveLastStack();

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, this, "the target must be a var reference");
        }
    }
}
