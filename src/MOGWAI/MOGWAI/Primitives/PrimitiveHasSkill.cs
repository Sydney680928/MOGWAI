using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveHasSkill : PrimitiveParamsName
    {
        public PrimitiveHasSkill(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveHasSkill(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGName name)
        {
            var skills = Engine.GetSkills();    
            Engine.StackPushBoolean(skills.Contains(name.Value));
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
