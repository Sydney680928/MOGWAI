using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveMathNegate : PrimitiveParamsNumber
    {
        public PrimitiveMathNegate(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> PerformOperation(MOGNumber number)
        {
            Engine.StackPushNumber(-number.Value);
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
