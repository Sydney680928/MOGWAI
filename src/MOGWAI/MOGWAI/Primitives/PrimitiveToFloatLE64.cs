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
    internal class PrimitiveToFloatLE64 : MOGPrimitive
    {
        public PrimitiveToFloatLE64(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveToFloatLE64(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            var n0 = Engine.StackPop();

            if (n0 is MOGData data)
            {
                if (data.Items.Count < 8)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, ".data too small."));

                var bytes = new byte[] { data.Items[0], data.Items[1], data.Items[2], data.Items[3], data.Items[4], data.Items[5], data.Items[6], data.Items[7] };
                var x = EndianHelper.FromDataLEFloat64(bytes);  
                
                Engine.StackPushNumber(x);

                return Task.FromResult(EvalResult.NoError);
            }
            else if (n0 is MOGNumber number)
            {
                Single b = 0;

                try
                {
                    b = (Single)number.Value;
                }
                catch (Exception ex)
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, ex.Message));
                }

                var d = new MOGData(Engine);
                byte[] bytes = EndianHelper.ToDataLEFloat64(b);
                d.Items.AddRange(bytes);    

                Engine.StackPush(d);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
