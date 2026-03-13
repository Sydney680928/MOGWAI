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
    internal class PrimitiveWhere : MOGPrimitive
    {
        public PrimitiveWhere(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveWhere(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // (1 2 3 5 6 3) 3 where ---> (2 5)
            // DATA:EB5600FF56 0x56 where ---> (1 4)
            // "ERERRE" "RE" where --> (1 4)

            var sign = Engine.StackSign(2);

            if (sign.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            var n0 = Engine.StackPop();
            var n1 = Engine.StackPop();
            var ss = n0!.ToString();

            if (n1 is MOGList list)
            {
                var lst = new MOGList(Engine);

                for (int i = 0; i < list.Items.Count; i++)
                    if (list.Items[i].ToString() == ss) lst.Items.Add(new MOGNumber(Engine, i));

                Engine.StackPush(lst);

                return Task.FromResult(EvalResult.NoError);
            }
            else if (n1 is MOGData data)
            {
                if (n0 is MOGNumber n)
                {
                    var lst = new MOGList(Engine);

                    for (int i = 0; i < data.Items.Count; i++)
                        if (data.Items[i] == n.Value) lst.Items.Add(new MOGNumber(Engine, i));

                    Engine.StackPush(lst);

                    return Task.FromResult(EvalResult.NoError);
                }
                else if (n0 is MOGData data2)
                {
                    var lst = new MOGList(Engine);
                    byte[] b2 = data2.Items.ToArray();

                    for (int i = 0; i < data.Items.Count; i++)
                    {
                        if (i + data2.Items.Count > data.Items.Count)
                            break;

                        byte[] b1 = data.Items.GetRange(i, b2.Length).ToArray();
                        bool isEqual = true;

                        for (int j = 0; j < b1.Length; j++)
                        {
                            if (b1[j] != b2[j])
                            {
                                isEqual = false;
                                break;
                            }
                        }

                        if (isEqual) lst.Items.Add(new MOGNumber(Engine, i));
                    }

                    Engine.StackPush(lst);

                    return Task.FromResult(EvalResult.NoError);
                }
                else
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, ".number expected"));
                }
            }
            else if (n1 is MOGString str)
            {
                if (n0 is MOGString str2)
                {
                    var lst = new MOGList(Engine);

                    for (int i = 0; i < str.Value.Length; i++)
                    {
                        if (i + str2.Value.Length > str.Value.Length)
                            break;

                        var s = str.Value.Substring(i, str2.Value.Length);

                        if (s == str2.Value)
                            lst.Items.Add(new MOGNumber(Engine, i));
                    }

                    Engine.StackPush(lst);

                    return Task.FromResult(EvalResult.NoError);
                }
                else
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, ".string expected"));
                }
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
