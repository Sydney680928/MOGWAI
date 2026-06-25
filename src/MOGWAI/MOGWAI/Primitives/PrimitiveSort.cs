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
    internal class PrimitiveSort : MOGPrimitive
    {
        public override Version Birth => new(8, 13, 0);

        public PrimitiveSort(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveSort(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // (1 5 4) sort => (1 4 5)  
            // ("A" "C" "B") sort => ("A" "B" "C")  
            // (x: z: y:) sort => (x: y: z:)
            // ('a' 'c' 'b') sort => ('a' 'b' 'c')  

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGList))
            {
                var list = Engine.StackPopList();

                if (list.Items.Count > 0)
                {
                    // Quel type d'éléments contient la liste ?

                    MOGType? type = list.Items[0].Type;

                    foreach (var item in list.Items)
                    {
                        if (item.Type != type)
                        {
                            type = null;
                            break;
                        }
                    }

                    if (type != null)
                    {
                        if (type.Value == "number")
                        {
                            // Numbers

                            var numbers = new List<double>();  

                            foreach (MOGNumber item in list.Items)
                                numbers.Add(item.Value);

                            numbers.Sort();

                            list.Items.Clear();

                            foreach (double item in numbers) 
                                list.Items.Add(new MOGNumber(Engine, item));
                        }
                        else if (type.Value == "name")
                        {
                            // Names

                            var names = new List<string>();

                            foreach (MOGName item in list.Items)
                                names.Add(item.Value);

                            names.Sort();

                            list.Items.Clear();

                            foreach (string item in names)
                                list.Items.Add(new MOGName(Engine, item));
                        }
                        else if (type.Value == "key")
                        {
                            // Keys

                            var keys = new List<string>();

                            foreach (MOGKey item in list.Items)
                                keys.Add(item.Value);

                            keys.Sort();

                            list.Items.Clear();

                            foreach (string item in keys)
                                list.Items.Add(new MOGKey(Engine, item));
                        }
                        else if (type.Value == "string")
                        {
                            // String

                            var strings = new List<string>();

                            foreach (MOGString item in list.Items)
                                strings.Add(item.Value);

                            strings.Sort();

                            list.Items.Clear();

                            foreach (string item in strings)
                                list.Items.Add(new MOGString(Engine, item));
                        }
                        else if (type.Value == "word")
                        {
                            // Words

                            var words = new List<string>(); 

                            foreach (MOGWord item in list.Items)
                                words.Add(item.Value);

                            words.Sort();

                            list.Items.Clear();

                            foreach (string item in words)
                                list.Items.Add(new MOGWord(Engine, item));
                        }
                    }
                }

                Engine.StackPush(list);

                return EvalResult.NoError;
            }
            else if (s[0] == typeof(MOGRef))
            {
                var reference = Engine.StackPopRef();
                var v = Engine.VarRead(reference.Value, false);

                if (v == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, reference.ToString());

                // Le contenu de la variable doit être de type
                // list pour pouvoir être modifié avec sort

                if (v is MOGList)
                {
                    Engine.StackPush(v);

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
