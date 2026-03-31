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
    internal class PrimitiveSWITCH : MOGPrimitive
    {
        public PrimitiveSWITCH(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveSWITCH(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // { { test } { code } { test } { code } { test } { code } } SWITCH

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGCode))
            {
                var globalCode = Engine.StackPopCode();

                // Il faut un nombre de paire d'éléments

                if (globalCode.Items.Count % 2 != 0)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "you must provide collection of test and code.");

                // Il faut QUE des codes

                foreach (var item in globalCode.Items)
                    if (item is not MOGCode)
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, "you must provide collecion of test and code.");

                // On execute chaque test et si OK on execute son code et on sort

                for (int i = 0; i < globalCode.Items.Count; i += 2)
                {
                    var test = globalCode.Items[i] as MOGCode;
                    var code = globalCode.Items[i + 1] as MOGCode;

                    var testResult = await test!.ExecuteScalar();

                    if (testResult.result != EvalResult.NoError)
                        return testResult.result;

                    var resultValue = testResult.value as MOGBoolean;

                    if (resultValue == null)
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "test result is not a boolean value.");

                    if (resultValue.Value)
                    {
                        // Résultat du test positif
                        // On exécute le code correspondant et on sort

                        return await code!.Execute();
                    }
                }

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
