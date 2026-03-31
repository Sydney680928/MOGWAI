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
using System.Xml.Linq;

namespace MOGWAI.Primitives
{
    internal class PrimitiveSet : MOGPrimitive
    {
        public PrimitiveSet(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override async Task<EvalResult> EngineEval()
        {
            // (1 2 3) 0 50 set ---> (50 2 3)
            // [id: 5 x: 9] x: 500 put ----> [id: 5 x: 500]
            // D:FF0510 0 0x0 set ----> D:000510

            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[2] == typeof(MOGList))
            {
                // List

                var value = Engine.StackPop();
                var index = Engine.StackPopNumber();
                var list = Engine.StackPopList();

                var result = list.SetItem(index.IntValue, value!);

                if (result != EvalResult.NoError)
                    return result;

                Engine.StackPush(list);
                return EvalResult.NoError;
            }
            else if (s[2] == typeof(MOGRecord))
            {
                // Record

                var value = Engine.StackPop();
                var key = Engine.StackPopKey();
                var record = Engine.StackPopRecord();

                if (key == null)
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, "expected key"); 

                record.SetItem(key.Value, value!);
                Engine.StackPush(record);
                return EvalResult.NoError;
            }
            else if (s[2] == typeof(MOGData) && s[1] == typeof(MOGNumber))
            {
                // Data

                if (s[0] == typeof(MOGNumber))
                {
                    // data number number set

                    var value = Engine.StackPopNumber();
                    var index = Engine.StackPopNumber();
                    var data = Engine.StackPopData();

                    if (index.IntValue >= 0 && index.IntValue < data.Items.Count)
                    {
                        data!.Items[index.IntValue] = (byte)value.IntValue;
                        Engine.StackPush(data);
                        return EvalResult.NoError;
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);
                    }
                }
                else if (s[0] == typeof(MOGData))
                {
                    // data number data

                    var value = Engine.StackPopData();
                    var index = Engine.StackPopNumber();
                    var data = Engine.StackPopData();

                    if (index.IntValue >= 0 && index.IntValue < data.Items.Count && index.IntValue + value.Items.Count <= data.Items.Count)
                    {
                        for (int i = 0; i < value.Items.Count; i++)
                            data.Items[index.IntValue + i] = value.Items[i];

                        Engine.StackPush(data);
                        return EvalResult.NoError;
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);
                    }
                }
            }
            else if (s[2] == typeof(MOGRef))
            {
                var n0 = Engine.StackPop();
                var n1 = Engine.StackPop();

                var reference = Engine.StackPopRef();   
                var value = Engine.VarRead(reference.Value, false);

                if (value == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                // Le contenu de la variable doit être de type
                // list, record ou data pour pouvoir être modifié avec set

                if (value is MOGList || value is MOGRecord || value is MOGData)
                {
                    Engine.StackPush(value);
                    Engine.StackPush(n1!);
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
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, reference.ToString(), $"var type .{value.Type.Value} not allowed");
                }
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
