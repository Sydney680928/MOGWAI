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
    internal class PrimitiveExtract : MOGPrimitive
    {
        public PrimitiveExtract(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveExtract(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // [x: 50 y: 100 z: 10] (x: y:) extract ----> [x: 50 y: 100]
            // (1 2 3 4) (0 2) extract ---> (1 3)
            // D:FFAB5612AE (0 2) extract ---> D:FF56

            await Task.CompletedTask;

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] != typeof(MOGList))
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);

            if (s[1] == typeof(MOGRecord))
            {
                var keys = Engine.StackPopList();
                var record = Engine.StackPopRecord();

                var newRecord = new MOGRecord(Engine);

                foreach (var key in keys.Items)
                {
                    if (key is MOGKey k)
                    {
                        if (record.Items.TryGetValue(k.Value, out var value))
                        {
                            newRecord.Items[k.Value] = value;
                        }
                        else
                        {
                            newRecord.Items[k.Value] = new MOGNull(Engine);
                        }
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "only keys are allowed !");
                    }
                }

                Engine.StackPush(newRecord);
            }
            else if (s[1] == typeof(MOGList))
            {
                var indexes = Engine.StackPopList();
                var list = Engine.StackPopList();

                var newList = new MOGList(Engine);

                foreach (var index in indexes.Items)
                {
                    if (index is MOGNumber idx)
                    {
                        if (idx.IntValue >= 0 && idx.IntValue < list.Items.Count)
                        {
                            var value = list.Items[idx.IntValue];
                            newList.Items.Add(value);
                        }
                        else
                        {
                            newList.Items.Add(new MOGNull(Engine));
                        }
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "only numbers are allowed !");
                    }
                }

                Engine.StackPush(newList);
            }
            else if (s[1] == typeof(MOGData))
            {
                var indexes = Engine.StackPopList() ;
                var data = Engine.StackPopData();

                var newData = new MOGData(Engine);

                foreach (var index in indexes.Items)
                {
                    if (index is MOGNumber idx)
                    {
                        if (idx.IntValue >= 0 && idx.IntValue < data.Items.Count)
                        {
                            var value = data.Items[idx.IntValue];
                            newData.Items.Add(value);
                        }
                        else
                        {
                            return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "index out of range !");
                        }
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "index out of range !");
                    }
                }

                Engine.StackPush(newData);
            }
            else
            {
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
            }

            return EvalResult.NoError;
        }
    }
}
