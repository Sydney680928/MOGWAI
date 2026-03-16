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
    internal class PrimitiveGet : MOGPrimitive
    {
        public PrimitiveGet(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override async Task<EvalResult> EngineEval()
        {
            // [x: 50 y: 100] x: get ---> 50
            // (1 2 3 4) 1 get ----> 2
            // D:FF01 0 get ---> 255
            // [id: 5 pos: [x: 5 y: 9]] (pos: y:) get ---> 9
            // ( 1 2 3 [id: 50 name: "SMITH"] [id: 60 name: "DOE"]) (3 name:) get ---> "SMITH"

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[1] == typeof(MOGRecord))
            {
                // Record

                if (s[0] == typeof(MOGKey))
                {
                    // record key get
                    // return null if key does not exists in the record

                    var key = Engine.StackPopKey();
                    var record = Engine.StackPopRecord();

                    var value = record.GetItem(key.Value);

                    if (value == null)
                    {
                        Engine.StackPushNull();
                    }
                    else
                    {
                       return  await value.EngineEval();
                    }

                    return EvalResult.NoError;
                }
                else if (s[0] == typeof(MOGList))
                {
                    // [id: 5 pos: [x: 5 y: 9]] (pos: y:) get ---> 9

                    var list = Engine.StackPopList();
                    var record = Engine.StackPopRecord();

                    // Only key and number in the lList

                    foreach (var item in list.Items)
                    {
                        if (item is not MOGKey && item is not MOGNumber)
                            return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "only key and number item allowed in the keys list");
                    }

                    // For each key we search the value
                    // If item found is other than record or list return null

                    MOGObject currentValue = record!;

                    foreach (var item in list.Items)
                    {
                        if (currentValue is MOGRecord rec && item is MOGKey key)
                        {
                            var v = rec.GetItem(key.Value);

                            if (v == null)
                            {
                                currentValue = new MOGNull(Engine, 0);
                                break;
                            }
                            else
                            {
                                currentValue = v;
                            }
                        }
                        else if (currentValue is MOGList lst && item is MOGNumber index)
                        {
                            var v = lst.GetItem(index.IntValue);

                            if (v == null)
                            {
                                currentValue = new MOGNull(Engine, 0);
                                break;
                            }
                            else
                            {
                                currentValue = v;
                            }
                        }
                        else
                        {
                            currentValue = new MOGNull(Engine, 0);
                            break;
                        }
                    }

                    return await currentValue.Clone().EngineEval();
                }
            }
            else if (s[1] == typeof(MOGList))
            {
                // List

                if (s[0] == typeof(MOGNumber))
                {
                    var index = Engine.StackPopNumber();
                    var list = Engine.StackPopList();

                    var value = list.GetItem(index.IntValue);

                    if (value == null)
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "index out of range");

                    return await value.Clone().EngineEval();
                }
                else if (s[0] == typeof(MOGList))
                {
                    // ( 1 2 3 [id: 50 name: "SMITH"] [id: 60 name: "DOE"]) (3 name:) get ---> "SMITH"

                    var searchList = Engine.StackPopList();
                    var list = Engine.StackPopList();

                    // Only key and number in the searchList

                    foreach (var item in searchList.Items)
                    {
                        if (item is not MOGKey && item is not MOGNumber)
                            return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "only key and number item allowed in the keys list");
                    }

                    // For each key we search the value
                    // If item found is other than record or list return null

                    MOGObject currentValue = list;

                    foreach (var item in searchList.Items)
                    {
                        if (currentValue is MOGRecord rec && item is MOGKey key)
                        {
                            var v = rec.GetItem(key.Value);

                            if (v == null)
                            {
                                currentValue = new MOGNull(Engine, 0);
                                break;
                            }
                            else
                            {
                                currentValue = v;
                            }
                        }
                        else if (currentValue is MOGList lst && item is MOGNumber index)
                        {
                            var v = lst.GetItem(index.IntValue);

                            if (v == null)
                            {
                                currentValue = new MOGNull(Engine, 0);
                                break;
                            }
                            else
                            {
                                currentValue = v;
                            }
                        }
                        else
                        {
                            currentValue = new MOGNull(Engine, 0);
                            break;
                        }
                    }

                    return await currentValue.Clone().EngineEval();
                }
            }
            else if (s[1] == typeof(MOGData) && s[0] == typeof(MOGNumber))
            {
                // data number get

                var number = Engine.StackPopNumber();
                var data = Engine.StackPopData();

                if (number.IntValue >= 0 && number.IntValue < data.Items.Count)
                {
                    Engine.StackPushNumber(data.Items[number.IntValue]);
                    return EvalResult.NoError;
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError);
                }
            }
            else if (s[1] == typeof(MOGRef))
            {
                var n0 = Engine.StackPop();

                var reference = Engine.StackPopRef();
                var value = Engine.VarRead(reference.Value, false);

                if (value == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                // Le contenu de la variable doit être de type
                // list, record ou data pour pouvoir être utilisée avec get 

                if (value is MOGList || value is MOGRecord || value is MOGData)
                {
                    Engine.StackPush(value);
                    Engine.StackPush(n0!);

                    return await EngineEval();                  
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
