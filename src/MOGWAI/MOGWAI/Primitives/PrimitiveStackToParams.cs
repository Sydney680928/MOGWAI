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
    internal class PrimitiveStackToParams : MOGPrimitive
    {
        public PrimitiveStackToParams(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveStackToParams(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // [id: 50 name: "SIBUE" x: 'Z'] [id: .number name: .string u: (.boolean true)] ->params -------> id=50 name="SIBUE u=true"

            await Task.CompletedTask;

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] != typeof(MOGRecord) || s[1] != typeof(MOGRecord))
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);

            var n0 = Engine.StackPopRecord();
            var n1 = Engine.StackPopRecord();

            // On décompose n0 en liste de paramètes ayant type + éventuellement valeur par défaut

            var pDefinitions = new List<ParamDefinition>();

            foreach (var key in n0.Items.Keys)
            {
                // La clé porte un type ou une liste avec (type defaultValue)

                var value = n0.Items[key];

                if (value is MOGType v)
                {
                    // OK

                    var np = new ParamDefinition(key, v, null);
                    pDefinitions.Add(np);
                }
                else if (value is MOGList list)
                {
                    // La liste doit être composée de 2 élements

                    if (list.Size != 2)
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, $"{key}: parameter", "default value list definition must have 2 items (type defaultValue).");

                    // L'item 0 doit être un type

                    if (list.Items[0] is MOGType type)
                    {
                        // L'item 1 doit être une valeur du type ou sans importance si type .any

                        if (list.Items[1] is MOGObject defaultValue && (type.Value == "any" || defaultValue.Type.Value == type.Value))
                        {
                            // OK

                            var np = new ParamDefinition(key, type, defaultValue);
                            pDefinitions.Add(np);
                        }
                        else
                        {
                            return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, $"{key}: parameter", "default value list definition must have a value with the good type in second position.");
                        }
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, $"{key}: parameter", "default value list definition must have a type in first position.");
                    }
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, $"{key}: parameter", "parameter definition is a type or a list (type defaultValue).");
                }
            }

            foreach (var p in pDefinitions)
            {
                if (n1.Items.Keys.Contains(p.VarName) && n1.Items[p.VarName] is MOGObject pv)
                {
                    // On a une valeur fournie pour ce paramètre
                    // Il doit être du bon type (sauf si le type attendu est .any)

                    if (p.Type.Value == "any" || pv.Type.Value == p.Type.Value)
                    {
                        // Tout est OK
                        // La valeur a le bon type
                        // On peut prendre en compte la valeur

                        p.Value = pv;
                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, $"{p.VarName}: type is invalid !", $"{p.Type} expected", $"{pv.Type} provided");
                    }
                }
                else
                {
                    // On n'a pas de valeur fournie pour ce paramètre
                    // Si on a une valeur par défaut c'est pas grave, sinon erreur !

                    if (p.Value == null)
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, $"{p.VarName}: parameter is mandatory !");
                }
            }

            // On crée les variables
            // Normalement on ne devrait pas avoir de valeur à null
            // Pour le moment on ne bloque pas, on place juste MOGNull comme valeur dans ce cas là

            EvalResult result = EvalResult.NoError;

            foreach (var pdef in pDefinitions)
            {
                if (Engine.StrictMode && !Engine.VarExists(pdef.VarName))
                {
                    // On doit déclarer la variable avant de l'utiliser
                    // De type .any

                    var r1 = Engine.VarDeclareForType(pdef.VarName, Engine.GetType("any")!);

                    if (r1 != EvalResult.NoError)
                        return r1;
                }

                result = Engine.VarWrite(pdef.VarName, pdef.Value ?? new MOGNull(Engine));

                if (result != EvalResult.NoError)
                    break;
            }

            return result;
        }
        private class ParamDefinition
        {
            public string VarName { get; set; }

            public MOGType Type { get; set; }

            public MOGObject? Value { get; set; }

            public ParamDefinition(string varName, MOGType type, MOGObject? value)
            {
                VarName = varName;
                Type = type;
                Value = value;
            }
        }
    }
}
