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
    internal class PrimitiveStackToSafeVars : MOGPrimitive
    {
        public PrimitiveStackToSafeVars(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveStackToSafeVars(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // 10 "SIBUE" 'Z' [id: .number name: .string x: .name] ->safeVars -------> id=50 name="SIBUE" x='Z'

            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] != typeof(MOGRecord))
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);

            // On récupère le record de référence et ses clés

            var recf = Engine.StackPopRecord();
            var keys = recf.Items.Keys.ToList();

            // Le record de référence ne doit porter QUE des types

            foreach (var k in keys)
            {
                if (recf.Items[k] is not MOGType)
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, "reference record must have .type values.");
            }

            // La pile doit au moins contenir le nombre de clés du record de référence

            if (Engine.StackSize < recf.Items.Count)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, "the stack does not contain enough elements.");

            // On récupère toute les valeurs depuis la pile

            var values = new List<MOGObject>();

            for (int i = 0; i < keys.Count; i++)
                values.Add(Engine.StackPop()!);

            // On vérifie la correspondance de types

            var index = 0;

            for (int i = keys.Count - 1; i >= 0; i--)
            {
                // On lit la valeur

                var pv = values[index++];

                // On récupère le type attendu

                var tv = recf.Items[keys[i]] as MOGType;

                // Si incorrect on arrête tout

                if (tv!.Value != "any" && tv!.Value != pv!.Type.Value)
                    return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, $"{tv} expected but {pv.Type} found for '{keys[i]}' parameter");
            }

            // On crée les variables locales

            index = 0;

            for (int i = keys.Count - 1; i >= 0; i--)
            {
                if (Engine.StrictMode && !Engine.VarExists(keys[i]))
                {
                    // On doit déclarer la variable avant de l'utiliser
                    // De type .any

                    var r1 = Engine.VarDeclareForType(keys[i], Engine.GetType("any")!);

                    if (r1 != EvalResult.NoError)
                        return r1;
                }

                var v = values[index++];
                var r = Engine.VarWrite(keys[i], v);

                if (r != EvalResult.NoError)
                    return r;
            }

            return EvalResult.NoError;
        }
    }
}
