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
using System.Buffers.Binary;

namespace MOGWAI.Primitives
{
    internal class PrimitiveToUInt16 : MOGPrimitive
    {
        public override Version Birth => new(8, 5, 0);

        public PrimitiveToUInt16(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveToUInt16(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // DATA:1 ->uint8    ----> .number
            // .number ->uint8   ----> DATA:1

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            var n0 = Engine.StackPop();

            if (n0 is MOGData data)
            {
                // DATA:2 ->uint16

                if (data.Items.Count < 2)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, ".data too small."));

                var x = BinaryPrimitives.ReadUInt16LittleEndian(data.Items.ToArray());
                Engine.StackPushNumber(x);

                return Task.FromResult(EvalResult.NoError);
            }
            else if (n0 is MOGNumber number)
            {
                // 56 ->uint16

                UInt16 b = 0;

                try
                {
                    b = (UInt16)number.IntValue;
                }
                catch (Exception ex)
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, ex.Message));
                }

                byte[] bytes = new byte[2];
                BinaryPrimitives.WriteUInt16LittleEndian(bytes, b);

                var d = new MOGData(Engine);
                d.Items.AddRange(bytes);    
                Engine.StackPush(d);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
