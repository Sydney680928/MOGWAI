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
    internal class PrimitiveFileWrite : MOGPrimitive
    {
        public PrimitiveFileWrite(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveFileWrite(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // fileid data file.write

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGData) && s[1] == typeof(MOGString))
            {
                var data = Engine.StackPopData();
                var file = Engine.StackPopString();

                if (!Engine.OpenoutFileExists(file.Value))
                    return Task.FromResult(EvalResult.Failure(Engine, Error.UnknownFileError, Name));

                try
                {
                    Engine.FileWrite(file.Value, data.Items.ToArray());
                    return Task.FromResult(EvalResult.NoError);
                }
                catch
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.FileOperationError, Name));
                }
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
