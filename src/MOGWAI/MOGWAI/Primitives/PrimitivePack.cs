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
using System.Text;

namespace MOGWAI.Primitives
{
    internal class PrimitivePack : MOGPrimitive
    {
        public PrimitivePack(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitivePack(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            var obj = Engine.StackPop()!;

            try
            {
                var bytes = Encoding.UTF8.GetBytes(obj.ToString());
                var data = new MOGData(Engine, Engine.Compress(bytes));

                Engine.StackPush(data);

                return EvalResult.NoError;
            }
            catch
            {
                return EvalResult.Failure(Engine, Error.InternalError, Name);
            }
        }
    }
}
