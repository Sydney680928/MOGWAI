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
    internal class PrimitivePurge : MOGPrimitive
    {
        public PrimitivePurge(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitivePurge(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
        public override async Task<EvalResult> EngineEval()
        {
            // 'A' purge

            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGName))
            {
                var name = Engine.StackPopName();

                if (Engine.VarPurge(name.Value))
                    return EvalResult.NoError;

                return EvalResult.Failure(Engine, Error.UnknownNameError, Name, name.Value);
            }
            else if (s[0] == typeof(MOGKey))
            {
                s = Engine.StackSign(2);

                if (s.Count == 0)
                    return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

                if (s[0] == typeof(MOGKey) && s[1] == typeof(MOGRecord))
                {
                    var key = Engine.StackPopKey();
                    var record = Engine.StackPopRecord();

                    if (record.RemoveItem(key.Value))
                    {
                        Engine.StackPush(record);
                        return EvalResult.NoError;
                    }

                    return EvalResult.Failure(Engine, Error.UnknownKeyError, Name, key.Value);
                }
                else if (s[1] == typeof(MOGRef))
                {
                    var n0 = Engine.StackPop();

                    var reference = Engine.StackPopRef();
                    var value = Engine.VarRead(reference.Value, false);

                    if (value == null)
                        return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                    // Le contenu de la variable doit être de type record

                    if (value is MOGRecord)
                    {
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
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, reference.ToString(), $"var type .{value.Type.Value} not allowed");
                    }
                }
            }
            else if (s[0] == typeof(MOGNumber))
            {
                s = Engine.StackSign(2);

                if (s.Count == 0)
                    return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

                if (s[1] == typeof(MOGList))
                {
                    var index = Engine.StackPopNumber();
                    var list = Engine.StackPopList();

                    var result = list.RemoveItem(index.IntValue);

                    if (result == EvalResult.NoError)
                        Engine.StackPush(list);

                    return result;
                }
                else if (s[1] == typeof(MOGData))
                {
                    var index = Engine.StackPopNumber();
                    var data = Engine.StackPopData();

                    var result = data.RemoveItem(index.IntValue);

                    if (result == EvalResult.NoError)
                        Engine.StackPush(data);

                    return result;
                }
                else if (s[1] == typeof(MOGRef))
                {
                    var n0 = Engine.StackPop();

                    var reference = Engine.StackPopRef();
                    var value = Engine.VarRead(reference.Value, false);

                    if (value == null)
                        return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                    // Le contenu de la variable doit être de type list ou data

                    if (value is MOGList || value is MOGData)
                    {
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
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, reference.ToString(), $"var type .{value.Type.Value} not allowed");
                    }
                }
            }         

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
