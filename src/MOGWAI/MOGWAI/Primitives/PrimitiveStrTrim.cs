using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveStrTrim : PrimitiveParamsString
    {
        public override Version Birth => new(8, 11, 0);

        public PrimitiveStrTrim(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveStrTrim(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGString @string)
        {
            var trimmed = @string.Value.Trim();
            Engine.StackPushString(trimmed);
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
