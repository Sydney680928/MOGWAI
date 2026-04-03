using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveToDataFromFloatBE32 : PrimitiveParamsNumber
    {
        public PrimitiveToDataFromFloatBE32(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveToDataFromFloatLE32(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGNumber number)
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
            byte[] bytes = EndianHelper.ToDataBEFloat32(b);
            d.Items.AddRange(bytes);

            Engine.StackPush(d);

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
