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

namespace MOGWAI.Objects
{
    public class MOGVar : MOGBaseString
    {
        public MOGVar(MogwaiEngine engine, string value) : base(engine, value)
        {
            Type = engine.GetType(typeof(MOGVar));
        }

        public MOGVar(MogwaiEngine engine, string value, int originPosition) : this(engine, value)
        {
            StartPos = originPosition;
            EndPos = originPosition + Value.Length;
        }

        public override MOGVar Clone()
        {
            var obj = new MOGVar(Engine, Value);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override  Task<EvalResult> EngineEval()
        {
            var value = Engine.VarRead(Value);

            if (value == null)
                return Task.FromResult(EvalResult.Failure(Engine, Error.UnknownNameError, $"var '{Value}' is not defined."));

            if (AutoEval)
                return value.UserEval();

            Engine.StackPush(value);

            return Task.FromResult(EvalResult.NoError);
        }

        public override string ToString()
        {
            if (AutoEval)
            {
                return $"!{Value}";
            }
            else
            {
                return $"@{Value}";
            }
        }
    }
}
