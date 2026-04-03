using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveToDataFromFloatLE64 : PrimitiveParamsNumber
    {
        public PrimitiveToDataFromFloatLE64(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveToDataFromFloatLE64(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGNumber number)
        {
            var d = new MOGData(Engine);
            byte[] bytes = EndianHelper.ToDataLEFloat64(number.Value);
            d.Items.AddRange(bytes);

            Engine.StackPush(d);

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
