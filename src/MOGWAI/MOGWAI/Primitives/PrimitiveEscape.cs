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
using System.Text.Json;

namespace MOGWAI.Primitives
{
    internal class PrimitiveEscape : PrimitiveParamsString
    {
        public PrimitiveEscape(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> PerformOperation(MOGString @string)
        {
            try
            {
                var encoded = JsonSerializer.Serialize(@string.Value);
                Engine.StackPushString(encoded.Trim('\"'));
                return Task.FromResult(EvalResult.NoError);
            }
            catch (Exception ex)
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.InternalError, this, ex.Message));
            }
        }
    }
}
