// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MOGWAI.Engine;
using MOGWAI.Objects;

namespace MOGWAI.Primitives
{
    internal class PrimitiveFrame : PrimitiveParamsName
    {
        public override Version Birth => new(8, 7, 0);

        public PrimitiveFrame(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveFrame(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGName name)
        {
            if (Engine.Classes.TryGetValue(name.Value, out var @class))
            {                          
                var publicProperies = new MOGRecord(Engine);

                foreach (var key in @class.PublicProperties.Keys)
                {
                    var prop = @class.PublicProperties[key];
                    publicProperies.SetItem(key, prop);
                }

                var privateProperties = new MOGRecord(Engine);

                foreach (var key in @class.PrivateProperties.Keys)
                {
                    var prop = @class.PrivateProperties[key];
                    privateProperties.SetItem(key, prop);
                }

                var publicFunctions = new MOGList(Engine);

                foreach (var key in @class.PublicFunctions.Keys)
                    publicFunctions.AddKey(key);

                var privateFunctions = new MOGList(Engine);

                foreach (var key in @class.PrivateFunctions.Keys)
                    privateFunctions.AddKey(key);

                var frame = new MOGRecord(Engine);

                frame.SetName("className",name.Value); 
                frame.SetItem("props", publicProperies);
                frame.SetItem("_props", privateProperties);
                frame.SetItem("funcs", publicFunctions);
                frame.SetItem("_funcs", privateFunctions);

                Engine.StackPush(frame);    

                return Task.FromResult(EvalResult.NoError);
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.UnknownClassError, Name, name.Value));
        }
    }
}
