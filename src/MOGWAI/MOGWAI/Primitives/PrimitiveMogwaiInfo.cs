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
    internal class PrimitiveMogwaiInfo : MOGPrimitive
    {
        public PrimitiveMogwaiInfo(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            var infos = Engine.GetVersionInformations();

            var record = new MOGRecord(Engine);

            record.SetItem("name", new MOGString(Engine, Engine.Name));
            record.SetItem("version", new MOGString(Engine, infos["mogwai"]));
            record.SetItem("platform", new MOGString(Engine, infos["platform"]));
            record.SetItem("architecture", new MOGString(Engine, infos["architecture"]));
            record.SetItem("OSdescription", new MOGString(Engine, infos["OSdescription"]));
            record.SetItem("framework", new MOGString(Engine, infos["framework"]));
            record.SetItem("runtimeID", new MOGString(Engine, infos["runtimeIdentifier"]));
            record.SetItem("prompt", new MOGString(Engine, infos["prompt"]));

            var primitives = new MOGList(Engine);

            foreach (var p in Engine.Primitives)
                primitives.AddItem(new MOGName(Engine, p));

            record.SetItem("primitives", primitives);

            var keywords = new MOGList(Engine);

            foreach (var p in Engine.PluginInformations)
                foreach (var k in p.Plugin.Keywords.Keys)
                    keywords.AddItem(new MOGName(Engine, k));

            record.SetItem("externalKeywords", keywords);

            var hostKeywords = new MOGList(Engine);

            if (Engine.Delegate != null)
            {
                foreach (var hk in Engine.Delegate.HostFunctions(Engine))
                    hostKeywords.AddItem(new MOGName(Engine, hk));
            }

            record.SetItem("hostKeywords", hostKeywords);

            record.SetItem("debug", new MOGBoolean(Engine, Engine.DebugMode));
            record.SetItem("keepAlive", new MOGBoolean(Engine, Engine.KeepAlive));
            record.SetItem("isTask", new MOGBoolean(Engine, Engine.IsTask));

            Engine.StackPush(record);

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
