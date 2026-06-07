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
using System.Text;

namespace MOGWAI.Objects
{
    public class MOGList : MOGBaseItems
    {
        public MOGList(MogwaiEngine engine) : base(engine)
        {
            Type = engine.GetType(typeof(MOGList));
        }

        public MOGList(MogwaiEngine engine, string content, int originPosition, MogwaiExecutionContext? context) : base(engine, content, originPosition, context)
        {
            Type = engine.GetType(typeof(MOGList));

            if (Items.Count > 0 && Items[0] is MOGWord word && word.Value == "!")
            {
                AutoEval = true;
                Items.RemoveAt(0);
            }
        }

        public MOGList(MogwaiEngine engine, List<MOGObject> items) : base(engine, items)
        {
            Type = engine.GetType(typeof(MOGList));

            if (Items.Count > 0 && Items[0] is MOGWord word && word.Value == "!")
            {
                AutoEval = true;
                Items.RemoveAt(0);
            }
        }

        private async Task Eval()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var stackSize = Engine.StackSize;

                var r = await item.EngineEval();

                if (r != EvalResult.NoError)
                    break;

                if (Engine.StackSize > stackSize)
                {
                    var value = Engine.StackPop();
                    Items[i] = value!;
                }
            }
        }

        public override MOGList Clone()
        {
            var obj = new MOGList(Engine, Items);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("(");

            if (AutoEval)
                sb.Append("! ");

            for (int i = 0; i < Items.Count; i++)
            {
                sb.Append(Items[i].ToString());

                if (i < Items.Count - 1)
                    sb.Append(" ");
            }

            sb.Append(")");

            return sb.ToString();
        }

        public override string ToJson()
        {
            var sb = new StringBuilder();

            sb.Append("[");

            var items = new string[Items.Count];
            int i = 0;

            foreach (var item in Items)
            {
                items[i++] = item.ToJson();
            }

            var s = string.Join(',', items);
            sb.Append(s);

            sb.Append("]");

            return sb.ToString();
        }

        public MOGCode ToCode()
        {
            var obj = new MOGCode(Engine, Items);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            if (AutoEval)
            {
                AutoEval = false;
                await Eval();
            }

            return await base.EngineEval();
        }

        public override async Task<EvalResult> UserEval()
        {
            await Eval();
            return await base.UserEval();
        }

        public bool CheckJusteOneType(Type type)
        {
            foreach (var item in Items)
            {
                if (item.GetType() != type)
                    return false;
            }

            return true;
        }
    }
}
