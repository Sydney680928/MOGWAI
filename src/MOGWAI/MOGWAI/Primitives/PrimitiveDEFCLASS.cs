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
    internal class PrimitiveDEFCLASS : MOGPrimitive
    {
        public override Version Birth => new(8, 6, 0);

        public PrimitiveDEFCLASS(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            // 'name' record DEFCLASS

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, this));

            if (s[0] == typeof(MOGRecord) && s[1] == typeof(MOGName))
            {
                var defRecord = Engine.StackPopRecord();
                var name = Engine.StackPopName();

                if (Engine.Classes.ContainsKey(name.Value))
                    return Task.FromResult(EvalResult.Failure(Engine, Error.ClassDefinitionError, this, $"class {name} already exists"));

                MOGClass? c;

                try
                {
                    c = new MOGClass(Engine, name.Value, defRecord);
                }
                catch (Exception ex)
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.ClassDefinitionError, this, ex.Message));
                }

                Engine.Classes.Add(c.Name, c);
                
                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, this));   
        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveDEFCLASS(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
    }
}
