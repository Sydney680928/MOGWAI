using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveCond : PrimitiveParamsString
    {
        public override Version Birth => new(8,16,0);
        
        public PrimitiveCond(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveCond(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGString @string)
        {
            string expression = @string.Value;

            // Conversion infixe condition → RPN via boolean Shunting-yard

            List<MOGObject> rpnTokens;

            try
            {
                rpnTokens = BoolShuntingYard.Convert(expression, Engine, StartPos, EndPos);
            }
            catch (Exception ex)
            {
                return EvalResult.Failure(Engine, Error.ParseError, Name, ex.Message);
            }

            // Construit un bloc MOGWAI et l'exécute

            var block = new MOGCode(Engine, rpnTokens);
            return await block.Execute();
        }
    }
}
