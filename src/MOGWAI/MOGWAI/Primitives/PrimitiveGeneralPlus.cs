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
    internal class PrimitiveGeneralPlus : MOGPrimitive
    {
        public PrimitiveGeneralPlus(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveGeneralPlus(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
        public override async Task<EvalResult> EngineEval()
        {
            await Task.CompletedTask;

            var s = Engine.StackSign(2);

            if (s.Count < 2)
            {
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);
            }

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGNumber))
            {
                // number number +

                var n2 = Engine.StackPopNumber();
                var n1 = Engine.StackPopNumber();

                try
                {
                    Engine.StackPushNumber(n1.Value + n2.Value);
                }
                catch (Exception ex)
                {
                    return EvalResult.Failure(Engine, Error.MathematicalError, Name, ex.Message);
                }
            }
            else if (s[1] == typeof(MOGList))
            {
                // list objet +

                var n1 = Engine.StackPop();
                var n2 = Engine.StackPopList();

                n2!.AddItem(n1!);
                Engine.StackPush(n2);
            }
            else if (s[0] == typeof(MOGString) || s[1] == typeof(MOGString))
            {
                // string string  +

                var n1 = Engine.StackPopString();
                var n2 = Engine.StackPopString();

                Engine.StackPushString(n2.Value + n1.Value);
            }
            else if (s[1] == typeof(MOGData))
            {
                if (s[0] == typeof(MOGNumber))
                {
                    // data number +

                    var number = Engine.StackPopNumber();
                    var data = Engine.StackPopData();
                    
                    var b = (byte)number.IntValue;
                    data.Items.Add(b);

                    Engine.StackPush(data);
                }
                else if (s[0] == typeof(MOGData))
                {
                    // data data +

                    var data2 = Engine.StackPopData();
                    var data = Engine.StackPopData();

                    data.Items.AddRange(data2.Items);
                    Engine.StackPush(data);
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
                }
            }
            else if (s[0] == typeof(MOGBinaryNumber) && s[1] == typeof(MOGBinaryNumber))
            {
                var bin0 = Engine.StackPopBinaryNumber();
                var bin1 = Engine.StackPopBinaryNumber();

                for (int i = bin0.Items.Count - 1; i >= 0; i--)                   
                    bin1.Items.Insert(0, bin0.Items[i]);

                Engine.StackPush(bin1);
                return EvalResult.NoError;
            }
            else
            {
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
            }

            return EvalResult.NoError;
        }
    }
}
