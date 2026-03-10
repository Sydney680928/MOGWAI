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
    internal class PrimitiveLast : MOGPrimitive
    {
        public PrimitiveLast(MogwaiEngine engine, string name) : base(engine, name)
        {
        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveLast(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // string last
            // list last
            // data last

            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGString))
            {
                var @string = Engine.StackPopString();

                if (@string.Value.Length > 0)
                    @string.Value = @string.Value.Substring(@string.Value.Length - 1, 1);

                Engine.StackPush(@string);
                return EvalResult.NoError;
            }
            else if (s[0] == typeof(MOGList))
            {
                var list = Engine.StackPopList();

                if (list.Items.Count == 0)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);

                Engine.StackPush(list.Items.Last());
                return EvalResult.NoError;
            }
            else if (s[0] == typeof(MOGData))
            {
                var data = Engine.StackPopData();

                if (data.Items.Count == 0)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);

                Engine.StackPushNumber(data.Items.Last());
                return EvalResult.NoError;
            }
            else if (s[0] == typeof(MOGRef))
            {
                var n0 = Engine.StackPop();

                var reference = Engine.StackPopRef();
                var value = Engine.VarRead(reference.Value, false);

                if (value == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                Engine.StackPush(value);
                Engine.StackPush(n0!);

                return await EngineEval();
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);

        }
    }
}
