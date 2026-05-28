using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MOGWAI.Primitives
{
    internal class PrimitiveMogwaiAssertSkill : MOGPrimitive
    {
        public PrimitiveMogwaiAssertSkill(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveMogwaiAssertSkill(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // 'SKILL' "message" assertSkill
            // 'BLE' "Ce programme nécessite la fonction BLE !" assertSkill

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));    

            if (s[0] == typeof(MOGString) && s[1] == typeof(MOGName))
            {
                var message = Engine.StackPopString();  
                var skillName = Engine.StackPopName();  

                var skills = Engine.GetSkills();
                var has = skills.Contains(skillName.Value);

                if (!has)
                    return Task.FromResult(EvalResult.Failure(Engine, Error.AssertError, $"Requested skill {skillName}", message.Value));

                return Task.FromResult(EvalResult.NoError);                
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
