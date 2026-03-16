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
    internal class PrimitiveMogwaiUsings : MOGPrimitive
    {
        public PrimitiveMogwaiUsings(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            var list = new MOGList(Engine);

            foreach (var p in Engine.PluginInformations)
            {
                var record = new MOGRecord(Engine);

                record.SetItem("id", new MOGString(Engine, p.Plugin.ID));
                record.SetItem("name", new MOGString(Engine, p.Plugin.Name));
                record.SetItem("author", new MOGString(Engine, p.Plugin.Author));
                record.SetItem("version", new MOGString(Engine, p.Plugin.Version.ToString()));
                record.SetItem("description", new MOGString(Engine, p.Plugin.Description));
                record.SetItem("namespace", new MOGString(Engine, p.Plugin.Namespace));

                var keywords = new MOGList(Engine);

                foreach (var key in p.Plugin.Keywords.Keys)
                    keywords.AddItem(new MOGName(Engine, key));

                record.SetItem("keywords", keywords);

                list.AddItem(record);
            }

            Engine.StackPush(list);

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
