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
    internal class PrimitiveUnescape : PrimitiveParamsString
    {
        public PrimitiveUnescape(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveUnescape(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGString @string)
        {
            await Task.CompletedTask;

            try
            {

                var unescaped = JsonSerializer.Deserialize<string>($"\"{@string.Value}\"");

                if (unescaped != null)
                {
                    Engine.StackPushString(unescaped);
                    return EvalResult.NoError;
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.InternalError, this, "unescape operation failed.");
                }
            }
            catch (Exception ex)
            {
                return EvalResult.Failure(Engine, Error.InternalError, this, "unescape operation failed: Invalid string format.", ex.Message);
            }
        }
    }
}
