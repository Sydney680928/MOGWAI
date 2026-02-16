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
    internal class PrimitiveSize : MOGPrimitive
    {
        public PrimitiveSize(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveSize(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // list size
            // record size
            // string size
            // data size
            // binary size

            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0].IsSubclassOf(typeof(MOGBaseString)))
            {
                var o = Engine.StackPopString();
                Engine.StackPushNumber(o.Value.Length);
            }
            else if (s[0] == typeof(MOGList))
            {
                var o = Engine.StackPopList();
                Engine.StackPushNumber(o.Items.Count);
            }
            else if (s[0] == typeof(MOGRecord))
            {
                var o = Engine.StackPopRecord();
                Engine.StackPushNumber(o.Items.Count);
            }
            else if (s[0] == typeof(MOGData))
            {
                var o = Engine.StackPopData();
                Engine.StackPushNumber(o.Items.Count);
            }
            else if (s[0] == typeof(MOGBinaryNumber))
            {
                var o = Engine.StackPopBinaryNumber();
                Engine.StackPushNumber(o.Items.Count);
            }
            else
            {
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
            }

            return EvalResult.NoError;
        }
    }
}
