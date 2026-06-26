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
    internal class PrimitiveMogwaiPrimitiveInfo : PrimitiveParamsName
    {
        public override Version Birth => new(8, 13, 0);

        public PrimitiveMogwaiPrimitiveInfo(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveMogwaiPrimitiveInfo(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGName name)
        {
            var primitive = Engine.GetPrimitive(name.Value, false);

            if (primitive == null)
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, this, $"Primitive '{name}' not found."));

            var record = new MOGRecord(Engine);
            record.SetName("name", primitive.Name); 
            record.SetString("birth", primitive.Birth.ToString());
            record.SetBoolean("isPublic", !primitive.IsPrivate);
            
            Engine.StackPush(record);   

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
