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
    internal class PrimitiveToDataBE : MOGPrimitive
    {
        public override Version Birth => new(8, 5, 0);

        public PrimitiveToDataBE(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveToDataBE(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // number number ->dataBE

            var n = Engine.StackSign(2);

            if (n.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (n[0] == typeof(MOGNumber) && n[1] == typeof(MOGNumber))
            {
                var number = Engine.StackPopNumber();
                var value = Engine.StackPopNumber();   

                if (number.IntValue != 8 && number.IntValue != 16 && number.IntValue != 24 && number.IntValue != 32 && number.IntValue != 48 && number.IntValue != 64)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "only 8, 16, 24, 32, 48, 64 sizes are allowed"));
          
                try
                {
                    var bytes = EndianHelper.ToDataBE(value.LongValue, number.IntValue);
                    Engine.StackPushData(bytes);
                    return Task.FromResult(EvalResult.NoError);
                }
                catch (Exception ex)
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.ConvertError, Name, ex.Message));
                }   

            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
