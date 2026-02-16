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
    internal class PrimitiveStackCheck : PrimitiveParamsList
    {
        public PrimitiveStackCheck(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveStackCheck(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGList list)
        {
            // (.name .number) check

            await Task.CompletedTask;

            // On doit fournir une liste uniquement composée de types

            var expectedTypes = new List<string>();

            for (int i = 0; i < list.Items.Count; i++)
            {
                if (list.Items[i] is MOGType item)
                {
                    expectedTypes.Add(item.ToString());
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, "list must contains only type values");
                }
            }

            var s = Engine.StackSign(expectedTypes.Count);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.StackCorruptionError, $"{list.Items.Count} items expected, but stack size is {Engine.StackSize}");

            // On compose la liste de types réels de la stack

            var realTypes = new List<string>();

            for (int i = 0; i < s.Count; i++)
            {
                var t = Engine.GetType(s[i]);

                if (t == null)
                    return EvalResult.Failure(Engine, Error.FatalError, $"unknown engine type for '{s[i].Name}' internal class");

                realTypes.Add(t.ToString());
            }

            for (int i = 0; i < expectedTypes.Count; i++)
            {
                if (expectedTypes[i] != ".any" && expectedTypes[i] != realTypes[i])
                {
                    var sb = new StringBuilder();

                    sb.Append("stack types expected ");
                    sb.Append(list.ToString());
                    sb.Append(" but actually (");

                    for (int j = 0; j < realTypes.Count; j++)   
                    {
                        if (j > 0)
                            sb.Append(" ");

                        sb.Append(realTypes[j]);
                    }

                    sb.Append(")");

                    return EvalResult.Failure(Engine, Error.StackCorruptionError, sb.ToString());
                }
            }

            return EvalResult.NoError;
        }
    }
}
