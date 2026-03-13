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
    internal class PrimitiveFOREACH : MOGPrimitive
    {
        public PrimitiveFOREACH(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveFOREACH(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
        public override async Task<EvalResult> EngineEval()
        {
            // List name code FOREACH
            // (1 2 3) 'i' { i ? } FOREACH
            // {1 2 3} 'i' { i ? } FOREACH
            // «1 2 3» 'i' { i ? } FOREACH
            
            // D:010203 'i' { i ? } FOREACH 

            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, this);

            // 0 code
            // 1 variable
            // 2 list base

            if (s[0] == typeof(MOGCode) && s[1] == typeof(MOGName))
            {
                if (s[2].IsSubclassOf(typeof(MOGBaseItems)))
                {
                    var code = Engine.StackPopCode();
                    var name = Engine.StackPopName();
                    var items = Engine.StackPop() as MOGBaseItems;

                    EvalResult result = EvalResult.NoError;

                    if (!Engine.VarExists(name.Value) && Engine.StrictMode)
                    {
                        // On doit déclarer la variable avant de l'utiliser
                        // De type .any

                        var r = Engine.VarDeclareForType(name.Value, Engine.GetType("any")!);

                        if (r != EvalResult.NoError)
                            return r;
                    }

                    Engine.CreateBreakRequest();

                    foreach (var item in items!.Items)
                    {
                        if (Engine.BreakRequested || Engine.ExitRequested || Engine.ReturnRequested)
                            break;

                        result = Engine.VarWrite(name.Value, item);
                        
                        if (result.IsError)
                            break;

                        result = await code.Execute();
                        
                        if (result.IsError)
                            break;
                    }

                    Engine.RemoveBreakRequest();

                    return result;
                }
                else if (s[2] == typeof(MOGData))
                {
                    var code = Engine.StackPopCode();
                    var name = Engine.StackPopName();
                    var data = Engine.StackPopData();

                    EvalResult result = EvalResult.NoError;

                    if (!Engine.VarExists(name.Value) && Engine.StrictMode)
                    {
                        // On doit déclarer la variable avant de l'utiliser
                        // De type .any

                        var r = Engine.VarDeclareForType(name.Value, Engine.GetType("any")!);

                        if (r != EvalResult.NoError)
                            return r;
                    }

                    Engine.CreateBreakRequest();

                    foreach (var item in data.Items)
                    {
                        if (Engine.BreakRequested || Engine.ExitRequested || Engine.ReturnRequested)
                            break;

                        result = Engine.VarWrite(name.Value, new MOGNumber(Engine,item));
                        if (result != EvalResult.NoError)
                            break;

                        result = await code.Execute();
                        if (result != EvalResult.NoError)
                            break;
                    }

                    Engine.RemoveBreakRequest();

                    return result;
                }
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, this);
        }
    }
}
