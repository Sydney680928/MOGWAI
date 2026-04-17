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

        public Dictionary<string, MOGProperty> PrivateProperties { get; } = new();

        public Dictionary<string, MOGProperty> PublicProperties { get; } = new();

        public Dictionary<string, MOGFunction> PrivateFunctions { get; } = new();

        public Dictionary<string, MOGFunction> PublicFunctions { get; } = new();

        public MOGInstance(MogwaiEngine engine, MOGClass @class, int instance)
        {
            Engine = engine;
            ClassName = @class.Name;
            Instance = instance;

            var parser = new Parser();

            // Private Properties

            foreach (var key in @class.PrivateProperties.Keys)
            {
                var p = @class.PrivateProperties[key];
                var prop = new MOGProperty(@class.Engine, key, p);

                PrivateProperties.Add(key, prop);
            }

            // Public Properties

            foreach (var key in @class.PublicProperties.Keys)
            {
                var p = @class.PublicProperties[key];
                var prop = new MOGProperty(@class.Engine, key, p);

                PublicProperties.Add(key, prop);
            }

            // Private Functions

            foreach (var key in @class.PrivateFunctions.Keys)
            {
                var func = @class.PrivateFunctions[key];
                var code = "«" + func.Code + "»";   

                parser.Parse(engine, code, func.StartPos, null);

                if (parser.ParsedObjects.Count > 0 && parser.ParsedObjects[0] is MOGFunction function)
                {
                    function.Instance = instance;

                    var r = new MOGObjectReference(Engine, instance);
                    r.PauseAllowed = false;

                    var sto = Engine.GetPrimitive(typeof(PrimitiveSTO), true);

                    if (sto == null)
                        throw new Exception("the primitive STO is not registered in the engine");

                    sto.PauseAllowed = false;

                    var name = new MOGName(Engine, "self");
                    name.PauseAllowed = false;

                    function.Items.InsertRange(0, [r, name, sto]);

                    PrivateFunctions.Add(key, function);
                }
            }

            // Public Functions

            foreach (var key in @class.PublicFunctions.Keys)
            {
                var func = @class.PublicFunctions[key];
                var code = "«" + func.Code + "»";

                parser.Parse(engine, code, func.StartPos, null);

                if (parser.ParsedObjects.Count > 0 && parser.ParsedObjects[0] is MOGFunction function)
                {
                    function.Instance = instance;

                    var r = new MOGObjectReference(Engine, instance, -1);
                    r.PauseAllowed = false;

                    var sto = Engine.GetPrimitive(typeof(PrimitiveSTO), true);

                    if (sto == null)
                        throw new Exception("the primitive STO is not registered in the engine");

                    sto.PauseAllowed = false;

                    var name = new MOGName(Engine, "self", -1);
                    name.PauseAllowed = false;

                    function.Items.InsertRange(0, [r, name, sto]);

                    PublicFunctions.Add(key, function);
                }
            }
        }

        public async Task<EvalResult> GetPropertyAsync(string name, int instance = 0)
        {
            // className is a reserved property that returns the name of the class of the instance, it is not stored in the properties dictionaries and is always public
            
            if (name == "className")
            {
                Engine.StackPushName(ClassName);
                return EvalResult.NoError;
            }

            if (PublicProperties.TryGetValue(name, out var prop))
            {
                Engine.StackPush(prop.Value ?? new MOGEmpty(Engine));
                return EvalResult.NoError;
            }
            else if (PrivateProperties.TryGetValue(name, out var privateProp))
            {
                if (Instance == instance)
                {
                    Engine.StackPush(privateProp.Value ?? new MOGEmpty(Engine));
                    return EvalResult.NoError;
                }

                return EvalResult.Failure(Engine, Error.UnknownPropertyError, $"{name} property is private");
            }
            else if (PublicFunctions.TryGetValue(name, out var publicFunc))
            {
                // On execute la fonction

                return await publicFunc.Execute();
            }
            else if (PrivateFunctions.TryGetValue(name, out var privateFunc))
            {
                if (Instance == instance)
                {
                    // On execute la fonction

                    return await privateFunc.Execute();
                }

                return EvalResult.Failure(Engine, Error.UnknownPropertyError, $"{name} function is private");
            }

            return EvalResult.Failure(Engine, Error.UnknownPropertyError, name);

        }

        public EvalResult SetProperty(string name, MOGObject value, int instance = 0)
        {
            // className is not used here because the instance is already linked to a class and we don't want to allow changing the class of an instance by setting a property with the same name as the class

            if (name == "className")
                return EvalResult.Failure(Engine, Error.ReservedPropertyError, name);

            if (PublicProperties.TryGetValue(name, out var publicProp))
            {
                if (publicProp.Type.Value != value.Type.Value && publicProp.Type.Value != "any")
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, $"type {publicProp.Type} expected");

                publicProp.Value = value;
                return EvalResult.NoError;
            }

            if (PrivateProperties.TryGetValue(name, out var privateProp))
            {
                if (Instance == instance)
                {
                    if (privateProp.Type.Value != value.Type.Value && privateProp.Type.Value != "any")
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError, $"type {privateProp.Type} expected");

                    privateProp.Value = value;
                    return EvalResult.NoError;
                }

                return EvalResult.Failure(Engine, Error.UnknownPropertyError, $"{name} property is private");
            }

            return EvalResult.Failure(Engine, Error.UnknownPropertyError, name);
        }
    }
}
