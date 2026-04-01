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
    internal class PrimitiveToNumberBE48 : PrimitiveParamsData
    {
        public PrimitiveToNumberBE48(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveToNumberBE48(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGData data)
        {
            try
            {
                var n = EndianHelper.FromDataBE(data.Items.ToArray(), 48); 
                Engine.StackPushNumber(n);
                return Task.FromResult(EvalResult.NoError);
            }
            catch (Exception ex)
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.ConvertError, Name, ex.Message));
            }
        }
    }
}
