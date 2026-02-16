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
    internal class PrimitiveStackToVars : MOGPrimitive
    {
        public PrimitiveStackToVars(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveStackToVars(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // 10 20 30 ( 'A' 'B' 'C') ->vars -----> A=10 B=20 C=30
            // [id: 50 name: "SIBUE" x: 'Z'] ->vars -------> id=50 name="SIBUE" x='Z'

            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGList))
            {
                // Signature 10 20 30 ( 'A' 'B' 'C') ->vars

                var list = Engine.StackPopList();

                // La liste ne doit comporter QUE des names

                foreach (var item in list.Items)
                {
                    if (item is not MOGName)
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, "the list parameter can only contain names.");
                }

                // La stack doit comporter assez d'éléments

                if (Engine.StackSize < list.Size)
                    return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name, "the stack does not contain enough elements.");

                // Pour chaque name on prend un item de la stack et on crée une variable avec
                // On travaille à l'envers pour que les paramètres soient dans le bon sens

                for (int i = list.Size - 1; i >= 0; i--)
                {
                    var name = list.Items[i] as MOGName;
                    var item = Engine.StackPop();

                    if (Engine.StrictMode && !Engine.VarExists(name!.Value))
                    {
                        // On doit déclarer la variable avant de l'utiliser
                        // De type .any

                        var r1 = Engine.VarDeclareForType(name.Value, Engine.GetType("any")!);

                        if (r1 != EvalResult.NoError)
                            return r1;
                    }

                    var r2 = Engine.VarWrite(name!.Value, item!);

                    if (r2 != EvalResult.NoError)
                        return r2;
                }

                return EvalResult.NoError;
            }
            else if (s[0] == typeof(MOGRecord))
            {
                // Signature [id: 50 name: "SIBUE" x: 'Z'] ->vars

                var record = Engine.StackPopRecord();

                foreach (var key in record!.Items.Keys)
                {
                    var item = record.Items[key];

                    if (Engine.StrictMode && !Engine.VarExists(key))
                    {
                        // On doit déclarer la variable avant de l'utiliser
                        // De type .any

                        var r1 = Engine.VarDeclareForType(key, Engine.GetType("any")!);

                        if (r1 != EvalResult.NoError)
                            return r1;
                    }

                    Engine.VarWrite(key, item);
                }

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
