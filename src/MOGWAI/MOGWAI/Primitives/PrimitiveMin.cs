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
    internal class PrimitiveMin : PrimitiveParamsList
    {
        public PrimitiveMin(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> PerformOperation(MOGList list)
        {
            if (list.Items.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));

            if (list.Items[0] is not MOGNumber)
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));

            double min = (list.Items[0] as MOGNumber)!.Value;

            for (int i = 1; i < list.Items.Count; i++)
            {
                if (list.Items[i] is MOGNumber number)
                {
                    if (number.Value < min)
                        min = number.Value;
                }
                else
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));
                }
            }

            Engine.StackPushNumber(min);

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
