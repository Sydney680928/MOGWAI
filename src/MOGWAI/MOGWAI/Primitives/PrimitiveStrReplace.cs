using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveStrReplace : MOGPrimitive
    {
        public PrimitiveStrReplace(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveStrReplace(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // string old new str.replace

            var s = Engine.StackSign(3);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGString) && s[1] == typeof(MOGString) && s[2] == typeof(MOGString))
            {
                var newStr = Engine.StackPopString();
                var oldStr = Engine.StackPopString();
                var str = Engine.StackPopString();

                var @string = str.Value.Replace(oldStr.Value, newStr.Value);

                Engine.StackPushString(@string);

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
