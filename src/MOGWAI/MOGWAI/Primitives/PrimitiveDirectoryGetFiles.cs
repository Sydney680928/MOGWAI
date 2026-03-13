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
    internal class PrimitiveDirectoryGetFiles : PrimitiveParamsString
    {
        public PrimitiveDirectoryGetFiles(MogwaiEngine engine, string name) : base(engine, name)
        {
        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveDirectoryGetFiles(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGString @string)
        {
            if (@string.Value.Length == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));

            if (!Path.Exists(@string.Value))
                return Task.FromResult(EvalResult.Failure(Engine, Error.InvalidPathError, Name));

            try
            {
                var items = Directory.GetFiles(@string.Value);
                var list = new MOGList(Engine);

                foreach (var item in items)
                    list.Items.Add(new MOGString(Engine, Path.GetFileName(item)));

                Engine.StackPush(list);

                return Task.FromResult(EvalResult.NoError);
            }
            catch
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));
            }
        }
    }
}
