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
    internal class PrimitiveSplit : MOGPrimitive
    {
        public PrimitiveSplit(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveSplit(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // ( 1 2 3) split ---> 1 2 3
            // DATA:102030 split ---> %10 %20 %30
            // "ABCD;EFGH;TYUI" ";" split---- > ("ABCD" "EFGH" "TYUI")

            var sign = Engine.StackSign(1);

            if (sign.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (sign[0] == typeof(MOGList))
            {
                var list = Engine.StackPopList();

                foreach (var s in list.Items)
                    Engine.StackPush(s);

                return Task.FromResult(EvalResult.NoError);
            }
            else if (sign[0] == typeof(MOGData))
            {
                var data = Engine.StackPopData();

                foreach (var s in data.Items)
                    Engine.StackPushNumber(s);

                return Task.FromResult(EvalResult.NoError);
            }

            sign = Engine.StackSign(2);

            if (sign.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (sign[0] == typeof(MOGString) && sign[1] == typeof(MOGString))
            {
                var n0 = Engine.StackPopString();
                var n1 = Engine.StackPopString();

                string[] fields = n1.Value.Split(n0.Value);
                var lst = new MOGList(Engine);

                for (int i = 0; i < fields.Length; i++)
                    lst.Items.Add(new MOGString(Engine, fields[i]));

                Engine.StackPush(lst);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
