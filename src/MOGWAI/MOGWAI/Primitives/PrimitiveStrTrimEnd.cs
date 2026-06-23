using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveStrTrimEnd : PrimitiveParamsString
    {
        public override Version Birth => new(8, 11, 0);

        public PrimitiveStrTrimEnd(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveStrTrimEnd(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGString @string)
        {
            var trimmed = @string.Value.TrimEnd();
            Engine.StackPushString(trimmed);
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
