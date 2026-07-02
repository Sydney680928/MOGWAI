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
    internal class PrimitiveRegexIsMatch : MOGPrimitive
    {
        public PrimitiveRegexIsMatch(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveRegexIsMatch(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // 2 signatures possibles
            // "input" "pattern" regex.isMatch
            // "input" "pattern" timeout regex.isMatch

            // Il faut au moins 2 arguments sur la stack

            if (Engine.StackSize < 2)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            // On regarde la signature avec 3 arguments en 1er si assez d'arguments sur la pile

            if (Engine.StackSize >= 3)
            {
                var s3 = Engine.StackSign(3);

                if (s3.Count == 3)
                {
                    // On doit avoir un int, un string, un string

                    if (s3[0] == typeof(MOGNumber) && s3[1] == typeof(MOGString) && s3[2] == typeof(MOGString))
                    {
                        var timeout = Engine.StackPopNumber();
                        var pattern = Engine.StackPopString();
                        var input = Engine.StackPopString();

                        // On appelle la fonction regex.isMatch avec timeout
                    }
                }

                // On laisse tomber le traitement avec 3 arguments
                // On passe à 2 arguments      
            }

            // On regarde la signature avec 2 arguments

            var s2 = Engine.StackSign(2);

            if (s2.Count == 2)
            {
                // On doit avoir un string, un string

                if (s2[0] == typeof(MOGString) && s2[1] == typeof(MOGString))
                {
                    var pattern = Engine.StackPopString();
                    var input = Engine.StackPopString();

                    // On appelle la fonction regex.isMatch sans timeout
                }
            }

            // Pas assez d'arguments ou signature non reconnue

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
