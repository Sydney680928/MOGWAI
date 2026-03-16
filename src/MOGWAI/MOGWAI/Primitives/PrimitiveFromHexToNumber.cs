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
    internal class PrimitiveFromHexToNumber : PrimitiveParamsString
    {
        public PrimitiveFromHexToNumber(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override  Task<EvalResult> PerformOperation(MOGString @string)
        {
            var s = @string.Value;

            if (s.StartsWith("0x") || s.StartsWith("0X"))
                s = s[2..];

            if (long.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out var result))
            {
                Engine.StackPushNumber(result);
                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, @string.Value));
        }
    }
}
