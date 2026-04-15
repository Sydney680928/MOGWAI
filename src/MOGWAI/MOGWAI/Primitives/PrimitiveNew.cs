using MOGWAI.Engine;
using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Primitives
{
    internal class PrimitiveNew : PrimitiveParamsName
    {
        public PrimitiveNew(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveNew(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGName name)
        {
            if (Engine.Classes.TryGetValue(name.Value, out var @class))
            {              
                try
                {
                    var next = Engine.CurrentInstance + 1;
                    var instance = @class.CreateInstance(next);

                    Engine.ObjectReferences[next] = instance;               
                    Engine.CurrentInstance = next;
                    
                    var r = await instance.GetPropertyAsync("onInit", next);

                    if (r.IsError && r.Error != Error.UnknownPropertyError)
                        return r;

                    Engine.StackPushObjectReference(next);

                    return EvalResult.NoError; 
                }
                catch (Exception ex)
                {
                    return EvalResult.Failure(Engine, Error.InstanceCreationError, Name, ex.Message);
                }
            }
            else
            {
                return EvalResult.Failure(Engine, Error.UnknownClassError, Name);
            }
        }
    }
}
