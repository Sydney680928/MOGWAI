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
using MOGWAI.Exceptions;
using System.Text;

namespace MOGWAI.Objects
{
    public class MOGRecord : MOGObject
    {
        public Dictionary<string, MOGObject> Items { get; private set; } = new();

        public MOGRecord(MogwaiEngine engine) : base(engine)
        {
            Type = engine.GetType(typeof(MOGRecord));
        }

        public MOGRecord(MogwaiEngine engine, string content, int originPosition, MogwaiExecutionContext? context) : this(engine)
        {
            var parser = new Parser();
            parser.Parse(engine, content, originPosition, context);
            CreateItems(parser.ParsedObjects);
            Code = content;
            StartPos = originPosition;
            EndPos = originPosition + content.Length + 1;
            ExecutionContext = context;
        }

        public MOGRecord(MogwaiEngine engine, List<MOGObject> items) : this(engine)
        {
            CreateItems(items);
        }

        public MOGRecord(MogwaiEngine engine, Dictionary<string, MOGObject> items) : this(engine)
        {
            foreach (var key in items.Keys)
                Items[key] = items[key].Clone();
        }

        public void SetItem(string key, MOGObject value)
        {
            value.Bag = this;
            Items[key] = value;
        }

        public void SetString(string key, string value) => SetItem(key, new MOGString(Engine, value));  
        
        public void SetName(string key, string value) => SetItem(key,new MOGName(Engine, value));   

        public void SetWord(string key, string value) => SetItem(key, new MOGWord(Engine, value));   

        public void SetNumber(string key, double value) => SetItem(key, new MOGNumber(Engine, value));   

        public void SetBoolean(string key, bool value) => SetItem(key, new MOGBoolean(Engine, value));

        public void SetNull(string key) => SetItem(key, new MOGNull(Engine));    

        public void SetEmpty(string key) => SetItem(key, new MOGEmpty(Engine));  

        public void SetKey(string key, string value) => SetItem(key, new MOGKey(Engine, value)); 

        public MOGObject? GetItem(string key)
        {
            if (Items.TryGetValue(key, out var item))
                return item;

            return null;
        }

        public bool RemoveItem(string key) => Items.Remove(key);

        private void CreateItems(List<MOGObject> items)
        {
            // Si le 1er item est le mot "!" AutoEval = true

            if (items.Count > 0 && items[0] is MOGWord word && word.Value == "!")
            {
                AutoEval = true;
                items.RemoveAt(0);
            }

            // On doit avoir un nombre pair d'éléments (clé/valeur)

            if (items.Count % 2 != 0)
                throw new MogwaiInvalidRecordException("the number of items in a record must be even, as they are interpreted as key/value pairs");

            // On doit avoir 1 clé et une valuer et rien d'autre

            var dic = new Dictionary<string, MOGObject>();

            for (int i = 0; i < items.Count; i += 2)
            {
                if (items[i] is MOGKey key)
                {
                    dic[key.Value] = items[i + 1];
                    items[i + 1].Bag = this;
                }
                else
                {
                    throw new MogwaiInvalidRecordException("the key has the wrong type");
                }
            }

            Items = dic;
        }

        private async Task<EvalResult> Eval()
        {
            foreach (var key in Items.Keys)
            {
                var item = Items[key];
                var stackSize = Engine.StackSize;

                var r = await item.UserEval();

                if (r != EvalResult.NoError)
                {
                    Engine.LastParserStartErrorPosition = StartPos;    
                    Engine.LastParserEndErrorPosition = EndPos;
                    return r;
                }

                if (Engine.StackSize > stackSize)
                {
                    var value = Engine.StackPop();
                    Items[key] = value!;
                }
            }

            return EvalResult.NoError;
        }

        public override MOGRecord Clone()
        {
            var obj = new MOGRecord(Engine);
            obj.UpdateFromOther(this);

            foreach (var key in Items.Keys)
            {
                obj.Items[key] = Items[key].Clone();
                obj.Items[key].Bag = obj;
            }

            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            if (AutoEval)
            {
                AutoEval = false;

                var result = await Eval();

                if (result != EvalResult.NoError)
                    return result;
            }

            return await base.EngineEval();
        }

        public override async Task<EvalResult> UserEval()
        {
            var result = await Eval();

            if (result != EvalResult.NoError)
                return result;

            return await base.EngineEval();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (AutoEval)
                sb.Append("!");

            foreach (var key in Items.Keys)
            {
                if (sb.Length > 0)
                    sb.Append(" ");

                sb.Append(key);
                sb.Append(": ");
                sb.Append(Items[key]);
            }

            return $"[{sb}]";
        }

        public override string ToJson()
        {
            var sb = new StringBuilder();

            sb.Append("{");

            var items = new string[Items.Keys.Count];
            int i = 0;

            foreach (var key in Items.Keys)
            {
                var value = Items[key].ToJson();
                var item = $"\"{key}\":{value}";
                items[i++] = item;
            }

            var s = string.Join(',', items);
            sb.Append(s);

            sb.Append("}");

            return sb.ToString();
        }
    }
}
