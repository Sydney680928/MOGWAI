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
    public class MOGWord : MOGBaseString
    {
        public MOGWord(MogwaiEngine engine, string value) : base(engine, value)
        {
            Type = engine.GetType(typeof(MOGWord));
        }

        public MOGWord(MogwaiEngine engine, string value, int originPosition) : this(engine, value)
        {
            if (originPosition > -1)
            {
                StartPos = originPosition;
                EndPos = originPosition + Value.Length - 1;
            }
        }

        public override async Task<EvalResult> EngineEval()
        {
            // This word is a plugin function ?

            var result = await Engine.ExecutePluginKeyword(Value);

            if (result != EvalResult.NoPluginFunction)
                return result;

            // This word is a function ?

            var func = Engine.GetFunction(Value);

            if (func != null)
                return await func.Execute();

            // This word is a var ?

            var value = Engine.VarRead(Value);

            if (value == null)
                return EvalResult.Failure(Engine, Error.UnknownWordError, Value);

            Engine.StackPush(value);

            return EvalResult.NoError;
        }

        public override MOGWord Clone()
        {
            var obj = new MOGWord(Engine, Value);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
