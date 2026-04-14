using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveFree : MOGPrimitive
    {
        public PrimitiveFree(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveFree(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError);

            if (s[0] == typeof(MOGObjectReference))
            {
                var @ref = Engine.StackPopObjectReference();
                
                if (Engine.ObjectReferences.TryGetValue(@ref.Value, out var obj))
                {
                    var r = await obj.GetPropertyAsync("onFree");

                    if (r.Error != Error.UnknownPropertyError)
                        return r;

                    Engine.ObjectReferences.Remove(@ref.Value);
                
                    return EvalResult.NoError;
                }
                else
                {
                    return EvalResult.Failure(Engine, Error.UnknownInstanceError);
                }
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError);
        }
    }
}
