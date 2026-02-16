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
    public abstract class MOGBaseItems : MOGObject
    {
        public List<MOGObject> Items { get; private set; } = new();

        public int Size => Items.Count;

        public MOGBaseItems(MogwaiEngine engine) : base(engine)
        {

        }

        public MOGBaseItems(MogwaiEngine engine, string content, int originPosition, MogwaiExecutionContext? context) : base(engine)
        {
            var parser = new Parser();
            parser.Parse(engine, content, originPosition, context);
            Items = parser.ParsedObjects;
            Code = content;
            ExecutionContext = context;
            StartPos = originPosition - 1;
            EndPos = StartPos + content.Length + 1;
        }

        public MOGBaseItems(MogwaiEngine engine, List<MOGObject> items) : base(engine)
        {
            foreach (var item in items)
            {
                Items.Add(item.Clone());
            }
        }

        public MOGObject? GetItem(int index)
        {
            if (index < 0 || index >= Items.Count)
                return null;

            return Items[index];
        }

        public EvalResult SetItem(int index, MOGObject item)
        {
            if (index < 0 || index >= Items.Count)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError);

            Items[index] = item;
            return EvalResult.NoError;
        }

        public void AddItem(MOGObject item)
        {
            Items.Add(item);
        }

        public EvalResult RemoveItem(int index)
        {
            if (index < 0 || index >= Items.Count)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError);

            Items.RemoveAt(index);
            return EvalResult.NoError;
        }

        public EvalResult InsertItem(int index, MOGObject item)
        {
            if (index < 0 || index >= Items.Count)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError);

            Items.Insert(index, item);
            return EvalResult.NoError;
        }
    }
}
