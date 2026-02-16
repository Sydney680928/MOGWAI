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
using System.Text;

namespace MOGWAI.Primitives
{
    internal class PrimitiveJoin : MOGPrimitive
    {
        public PrimitiveJoin(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveJoin(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // ( "ABCD" "EFGH" "TYUI") ";" join ----> "ABCD;EFGH;TYUI"

            await Task.CompletedTask;

            var sign = Engine.StackSign(2);

            if (sign.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (sign[0] == typeof(MOGString) && sign[1] == typeof(MOGList))
            {
                var n0 = Engine.StackPopString();
                var n1 = Engine.StackPopList();

                var str = new StringBuilder();

                for (int i = 0; i < n1.Items.Count; i++)
                {
                    if (n1.Items[i] is MOGString s)
                    {
                        if (i > 0) str.Append(n0.Value);
                        str.Append(s.Value);
                    }
                }

                Engine.StackPushString(str.ToString());

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
