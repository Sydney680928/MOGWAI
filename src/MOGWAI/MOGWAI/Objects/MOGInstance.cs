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
using MOGWAI.Primitives;

namespace MOGWAI.Objects
{
    public class MOGInstance
    {
        public MogwaiEngine Engine { get; init; }

        public string ClassName { get; init; }

        public int Instance { get; init; }

        public Dictionary<string, MOGProperty> Privates { get; } = new();

        public Dictionary<string, MOGProperty> Properties { get; } = new();

        public Dictionary<string, MOGFunction> Funcs { get; } = new();

        public MOGInstance(MogwaiEngine engine, MOGClass @class, int instance)
        {
            Engine = engine;
            ClassName = @class.Name;
            Instance = instance;

            // Section privates

            foreach (var key in @class.Privates.Keys)
            {
                var p = @class.Privates[key];
                var prop = new MOGProperty(@class.Engine, key, p);

                Privates.Add(key, prop);
            }

            // Section props

            foreach (var key in @class.Properties.Keys)
            {
                var p = @class.Properties[key];
                var prop = new MOGProperty(@class.Engine, key, p);

                Properties.Add(key, prop);
            }

            // Section funcs

            foreach (var key in @class.Funcs.Keys)
            {
                var func = @class.Funcs[key].Clone();
                func.Instance = instance;

                var r = new MOGObjectReference(Engine, instance);
                r.PauseAllowed = false;

                var sto = Engine.GetPrimitive(typeof(PrimitiveSTO), false);
                
                if (sto == null)
                    throw new Exception("the primitive STO is not registered in the engine");

                sto.PauseAllowed = false;

                var name = new MOGName(Engine, "self");
                name.PauseAllowed = false;

                func.Items.InsertRange(0, [r, name, sto]);

                Funcs.Add(key, func);
            }
        }

        public async Task<EvalResult> GetPropertyAsync(string name, int instance = 0)
        {
            if (Properties.TryGetValue(name, out var prop))
            {
                Engine.StackPush(prop.Value ?? new MOGEmpty(Engine));
                return EvalResult.NoError;
            }

            if (Privates.TryGetValue(name, out var privateProp))
            {
                if (Instance == instance)
                {
                    Engine.StackPush(privateProp.Value ?? new MOGEmpty(Engine));
                    return EvalResult.NoError;
                }

                // On demande une propriété privée hors du code interne de l'instance
                // Interdit

                return EvalResult.Failure(Engine, Error.UnknownPropertyError, name);
            }

            if (Funcs.TryGetValue(name, out var func))
            {
                // On execute la fonction

                return await func.Execute();
            }

            return EvalResult.Failure(Engine, Error.UnknownPropertyError, name);
        }

        public EvalResult SetProperty(string name, MOGObject value, int instance = 0)
        {
            if (Properties.TryGetValue(name, out var prop))
            {
                if (prop.Type.Value != value.Type.Value && prop.Type.Value != "any")
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError);

                prop.Value = value;
                return EvalResult.NoError;
            }

            if (Privates.TryGetValue(name, out var privateProp))
            {
                if (Instance == instance)
                {
                    if (privateProp.Type.Value != value.Type.Value && privateProp.Type.Value != "any")
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError);

                    privateProp.Value = value;
                    return EvalResult.NoError;
                }
            }

            return EvalResult.Failure(Engine, Error.UnknownPropertyError, name);
        }
    }
}
