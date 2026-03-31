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

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveGeneralPlus(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            var s = Engine.StackSign(2);

            if (s.Count < 2)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);


            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGNumber))
            {
                // number number +

                var n1 = Engine.StackPopNumber();
                var n2 = Engine.StackPopNumber();

                try
                {
                    n2.Value = n1.Value + n2.Value;
                    Engine.StackPush(n2);
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

                n2.AddItem(n1!);
                Engine.StackPush(n2);
            }
            else if (s[0] == typeof(MOGString) && s[1] == typeof(MOGString))
            {
                // string string  +

                var n1 = Engine.StackPopString();
                var n2 = Engine.StackPopString();

                n2.Value += n1.Value;
                Engine.StackPush(n2);
            }
            else if (s[1] == typeof(MOGString))
            {
                // string ? +

                var n1 = Engine.StackPop();
                var n2 = Engine.StackPopString();

                n2.Value += n1!.ToString();
                Engine.StackPush(n2);
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

                    var data1 = Engine.StackPopData();
                    var data2 = Engine.StackPopData();

                    data2.Items.AddRange(data1.Items);
                    Engine.StackPush(data2);
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
                }
            }
            else if (s[0] == typeof(MOGBinaryNumber) && s[1] == typeof(MOGBinaryNumber))
            {
                var bin1 = Engine.StackPopBinaryNumber();
                var bin2 = Engine.StackPopBinaryNumber();

                for (int i = bin1.Items.Count - 1; i >= 0; i--)
                    bin2.Items.Insert(0, bin1.Items[i]);

                Engine.StackPush(bin2);

                return EvalResult.NoError;
            }
            else if (s[1] == typeof(MOGRef))
            {
                var n0 = Engine.StackPop();

                var reference = Engine.StackPopRef();
                var value = Engine.VarRead(reference.Value, false);

                if (value == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());
                
                Engine.StackPush(value);
                Engine.StackPush(n0!);

                var r = await EngineEval();

                if (r.IsError)
                    return r;

                // On enlève la valeur modifiée de la stack qui ne sert à rien

                Engine.StackDrop();

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
