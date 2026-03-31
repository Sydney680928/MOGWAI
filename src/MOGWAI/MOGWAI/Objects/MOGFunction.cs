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

namespace MOGWAI.Objects
{
    public class MOGFunction : MOGCode
    {
        public string Name { get; internal set; } = Guid.NewGuid().ToString();

        public MOGFunction(MogwaiEngine engine) : base(engine)
        {
            Type = engine.GetType(typeof(MOGFunction));
            PauseAllowed = false;
        }

        public MOGFunction(MogwaiEngine engine, string content, int originPosition, MogwaiExecutionContext? context) : base(engine, content, originPosition, context)
        {
            Type = engine.GetType(typeof(MOGFunction));
            PauseAllowed = false;

            if (Items.Count > 0 && Items[0] is MOGWord word && word.Value == "!")
            {
                AutoEval = true;
                Items.RemoveAt(0);
            }
        }

        public MOGFunction(MogwaiEngine engine, List<MOGObject> items) : base(engine, items)
        {
            Type = engine.GetType(typeof(MOGFunction));
            PauseAllowed = false;

            if (Items.Count > 0 && Items[0] is MOGWord word && word.Value == "!")
            {
                AutoEval = true;
                Items.RemoveAt(0);
            }
        }

        public override MOGFunction Clone()
        {
            var obj = new MOGFunction(Engine);
            obj.UpdateFromOther(this);

            foreach (var item in Items)
            {
                var newItem = item.Clone();
                newItem.Bag = this;

                obj.Items.Add(newItem);
            }

            return obj;
        }

        public override async Task<EvalResult> Execute()
        {
            Engine.VarPushContext(Name);
            var r = await base.Execute();
            Engine.ReturnRequested = false;
            Engine.VarPopContext();

            return r;
        }

        public override async Task<EvalResult> EngineEval()
        {
            if (AutoEval)
            {
                return await Execute();
            }
            else
            {
                return await base.EngineEval();
            }
        }

        public MOGCode ToCode()
        {
            var obj = new MOGCode(Engine, Items);
            obj.StartPos = StartPos;
            obj.EndPos = EndPos;
            return obj;
        }

        public override string ToString() => $"«{ToStringCode()}»";
    }
}
