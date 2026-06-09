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
    internal class PrimitiveMathLCM : PrimitiveParamsNumberNumber
    {
        public PrimitiveMathLCM(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> PerformOperation(MOGNumber number1, MOGNumber number2)
        {
            int a = Math.Abs(number1.IntValue);
            int b = Math.Abs(number2.IntValue);

            if (a == 0 || b == 0)
            {
                Engine.StackPushNumber(0);
            }
            else
            {
                var a1 = a;
                var b1 = b;

                while (b1 != 0)
                {
                    var temp = b1;
                    b1 = a1 % b1;
                    a1 = temp;
                }

                Engine.StackPushNumber(Math.Abs(a * b) / a1);
            }

            return Task.FromResult(EvalResult.NoError);
        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveMathLCM(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }
    }
}
