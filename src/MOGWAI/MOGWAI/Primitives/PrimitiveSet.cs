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

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveSet(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // (1 2 3) 0 50 set ---> (50 2 3)
            // [id: 5 x: 9] x: 500 set ----> [id: 5 x: 500]
            // D:FF0510 0 0x0 set ----> D:000510

            // 50 (1 2 3) 0 set ---> (50 2 3)
            // 500 [id: 5 x: 9] x: set ----> [id: 5 x: 500]
            // 0x00 D:FF0510 0 set ----> D:000510
            // 100 §0345 x: set ----> §0300.x = 100

            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[1] == typeof(MOGList))
            {
                // List
               
                var index = Engine.StackPopNumber();
                var list = Engine.StackPopList();
                var value = Engine.StackPop();

                var result = list.SetItem(index.IntValue, value!);

                if (result != EvalResult.NoError)
                    return result;

                Engine.StackPush(list);
                return EvalResult.NoError;
            }
            else if (s[1] == typeof(MOGRecord))
            {
                // Record
              
                var key = Engine.StackPopKey();
                var record = Engine.StackPopRecord();
                var value = Engine.StackPop();

                if (key == null)
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, "expected key");

                record.SetItem(key.Value, value!);
                Engine.StackPush(record);
                return EvalResult.NoError;
            }
            else if (s[1] == typeof(MOGData) && s[0] == typeof(MOGNumber))
            {
                // Data

                if (s[2] == typeof(MOGNumber))
                {
                    // data number number set
                    
                    var index = Engine.StackPopNumber();
                    var data = Engine.StackPopData();
                    var value = Engine.StackPopNumber();

                    if (index.IntValue >= 0 && index.IntValue < data.Items.Count)
                    {
                        data.Items[index.IntValue] = (byte)value.IntValue;
                        Engine.StackPush(data);
                        return EvalResult.NoError;
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);
                    }
                }
                else if (s[2] == typeof(MOGData))
                {
                    // data number data
                   
                    var index = Engine.StackPopNumber();
                    var data = Engine.StackPopData();
                    var value = Engine.StackPopData();

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
            else if (s[1] == typeof(MOGObjectReference) && s[0] == typeof(MOGKey))
            {
                // objref key value set
               
                var key = Engine.StackPopKey(); 
                var objref = Engine.StackPopObjectReference();
                var value = Engine.StackPop();

                int instance = 0;

                if (Bag is MOGCode code)
                    instance = code.Instance;

                if (Engine.ObjectReferences.TryGetValue(objref.Value, out var obj))
                {
                    return obj.SetProperty(key.Value, value!, instance);
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.UnknownInstanceError);
                }
            }
            else if (s[1] == typeof(MOGRef))
            {               
                var n1 = Engine.StackPop();
                var reference = Engine.StackPopRef();
                var n0 = Engine.StackPop();

                var value = Engine.VarRead(reference.Value, false);

                if (value == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                // Le contenu de la variable doit être de type
                // list, record ou data pour pouvoir être modifié avec set

                if (value is MOGList || value is MOGRecord || value is MOGData)
                {
                    Engine.StackPush(n0!);
                    Engine.StackPush(value);
                    Engine.StackPush(n1!);                   

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
