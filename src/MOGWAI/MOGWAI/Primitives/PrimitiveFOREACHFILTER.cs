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
    internal class PrimitiveFOREACHFILTER : MOGPrimitive
    {
        public PrimitiveFOREACHFILTER(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveFOREACHFILTER(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            // 0 code
            // 1 variable
            // 2 list base

            if (s[0] == typeof(MOGCode) && s[1] == typeof(MOGName) && s[2] == typeof(MOGList))
            {
                var code = Engine.StackPopCode();
                var name = Engine.StackPopName();
                var items = Engine.StackPopList();

                EvalResult result = EvalResult.NoError;

                if (!Engine.VarExists(name.Value) && Engine.StrictMode)
                {
                    var r = Engine.VarDeclareForType(name.Value, Engine.GetType("any")!);

                    if (r != EvalResult.NoError)
                        return r;
                }

                var newItems = new List<MOGObject>();

                Engine.AddNewStack();
                Engine.CreateBreakRequest();

                foreach (var item in items!.Items)
                {
                    if (Engine.BreakRequested || Engine.ExitRequested || Engine.ReturnRequested)
                        break;

                    result = Engine.VarWrite(name.Value, item);

                    if (result != EvalResult.NoError)
                        break;

                    result = await code.Execute();

                    if (result != EvalResult.NoError)
                        break;

                    if (Engine.StackSize != 1)
                    {
                        // Only one item pushed onto the stack ! 
                        // Must be a boolean value to know if the item must be kept or not.

                        result = EvalResult.Failure(Engine, Error.StackCorruptionError, this, "the filter code must push only one boolean result onto the stack");
                        break;
                    }

                    var b = Engine.StackPopBoolean();

                    if (b == null)
                    {
                        result = EvalResult.Failure(Engine, Error.BadArgumentTypeError, this, "the filter code must push a boolean result onto the stack");
                        break;
                    }   

                    if (b.Value)
                        newItems.Add(item.Clone());
                }

                Engine.RemoveBreakRequest();
                Engine.RemoveLastStack();

                var resultList = new MOGList(Engine, newItems);
                Engine.StackPush(resultList);

                return result;
            }
            
            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, this);
        }
    }
}
