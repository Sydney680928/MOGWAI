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
    internal class PrimitiveFileClose : PrimitiveParamsString
    {
        public PrimitiveFileClose(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveFileClose(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGString @string)
        {
            await Task.CompletedTask;

            if (@string.Value.Length == 0)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);

            try
            {
                if (Engine.OpeninFileExists(@string.Value))
                {
                    Engine.CloseOpeninFile(@string.Value);                
                }
                else if (Engine.OpenoutFileExists(@string.Value))
                {
                    Engine.CloseOpenoutFile(@string.Value);
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.UnknownFileError, Name);
                }
                
                return EvalResult.NoError;
            }
            catch
            {
                return EvalResult.Failure(Engine, Error.FileOperationError, Name);
            }
        }
    }
}
