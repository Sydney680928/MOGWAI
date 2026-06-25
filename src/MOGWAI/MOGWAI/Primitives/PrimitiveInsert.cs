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
    internal class PrimitiveInsert : MOGPrimitive
    {
        public override Version Birth => new(8, 13, 0);

        public PrimitiveInsert(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveInsert(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // "XXX" (1 2 3) 1 insert => (1 "XXX" 2 3)   
            // 0xEA D:FF015629 1 insert => D:FFEA015629

            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGList))
            {
                var index = Engine.StackPopNumber();
                var list = Engine.StackPopList();
                var value = Engine.StackPop();

                if (index.IntValue < 0)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "index out of range");

                if (index.IntValue > list.Items.Count)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "index out of range");

                list.Items.Insert(index.IntValue, value!);

                Engine.StackPush(list);

                return EvalResult.NoError;
            }
            else if (s[0] == typeof(MOGNumber) && s[1] == typeof(MOGData) && s[2] == typeof(MOGNumber))
            {
                var index = Engine.StackPopNumber();
                var data = Engine.StackPopData();
                var value = Engine.StackPopNumber();

                if (index.IntValue < 0)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "index out of range");

                if (index.IntValue > data.Items.Count)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "index out of range");

                var v = value.IntValue;

                if (v < 0 || v > 255)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "value out of range");

                data.Items.Insert(index.IntValue, (byte)v);

                Engine.StackPush(data);

                return EvalResult.NoError;
            }
            else if (s[1] == typeof(MOGRef))
            {
                var index = Engine.StackPopNumber();
                var reference = Engine.StackPopRef();
                var value = Engine.StackPop();

                var v = Engine.VarRead(reference.Value, false);

                if (v == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                // Le contenu de la variable doit être de type
                // list ou data pour pouvoir être modifié avec insert

                if (v is MOGList || v is MOGData)
                {
                    Engine.StackPush(value!);
                    Engine.StackPush(v!);
                    Engine.StackPush(index);

                    var r = await EngineEval();

                    if (r.IsError)
                        return r;

                    // On enlève la valeur modifiée de la stack qui ne sert à rien

                    Engine.StackDrop();

                    return EvalResult.NoError;
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, reference.ToString(), $"var type .{v.Type.Value} not allowed");
                }
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
