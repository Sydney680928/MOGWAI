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
    internal class PrimitiveMogwaiAssert : MOGPrimitive
    {
        public override Version Birth => new(8, 6, 0);

        public PrimitiveMogwaiAssert(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveMogwaiAssert(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // true "message" assert
            // (10 a ==) "a doit être égal à 10" assert

            var s = Engine.StackSign(2);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);    

            if (s[0] == typeof(MOGString))
            {
                if (s[1] == typeof(MOGList))
                {
                    // (10 a ==) "a doit être égal à 10" assert   

                    var message = Engine.StackPopString();
                    var condition = Engine.StackPopList();
                    var code = condition.ToCode();
                    var stackSize = Engine.StackSize;

                    var result = await code.Execute();

                    if (result.IsError)
                        return result;

                    if (Engine.StackSize != stackSize + 1)
                        return EvalResult.Failure(Engine, Error.StackCorruptionError, Name, "the condition must push exactly one boolean value on the stack");  

                    var testResult = Engine.StackPopBoolean();

                    if (testResult == null)
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, "test result must be a boolean");

                    if (!testResult.Value)
                        return EvalResult.Failure(Engine, Error.AssertError, message.Value);

                    return EvalResult.NoError;
                }
                else if (s[1] == typeof(MOGBoolean))
                {
                    // true "message" assert

                    var message = Engine.StackPopString();
                    var testResult = Engine.StackPopBoolean();

                    if (testResult == null)
                        return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name, "test result must be a boolean");

                    if (!testResult.Value)
                        return EvalResult.Failure(Engine, Error.AssertError, message.Value);

                    return EvalResult.NoError;
                }
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
