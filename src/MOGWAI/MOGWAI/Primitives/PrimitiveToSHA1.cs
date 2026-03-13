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
using System.Security.Cryptography;

namespace MOGWAI.Primitives
{
    internal class PrimitiveToSHA1 : PrimitiveParamsData
    {
        public PrimitiveToSHA1(MogwaiEngine engine, string name) : base(engine, name)
        {
        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveToSHA1(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGData data)
        {

            var b = data.Items.ToArray();

            using (var sha1 = SHA1.Create())
            {
                sha1.Initialize();
                var h = sha1.ComputeHash(b);

                if (h != null)
                {
                    var hash = new MOGData(Engine, h);
                    Engine.StackPush(hash);
                    return Task.FromResult(EvalResult.NoError);
                }

                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));
            }
        }
    }
}
