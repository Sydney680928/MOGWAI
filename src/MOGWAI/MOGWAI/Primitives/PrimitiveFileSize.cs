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
    internal class PrimitiveFileSize : PrimitiveParamsString
    {
        public PrimitiveFileSize(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveFileSize(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGString @string)
        {
            await Task.CompletedTask;

            if (!Engine.OpeninFileExists(@string.Value))
                return EvalResult.Failure(Engine, Error.UnknownFileError, Name);

            var size = Engine.OpeninFileSize(@string.Value);
            Engine.StackPushNumber(size);

            return EvalResult.NoError;

        }
    }
}
