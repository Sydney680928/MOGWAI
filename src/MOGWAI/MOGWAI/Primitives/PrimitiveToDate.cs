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
    internal class PrimitiveToDate : PrimitiveParamsNumber
    {
        public PrimitiveToDate(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveToDate(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGNumber number)
        {
            // ticks ->date ----> [day: 28 ...


            var d = new DateTime((long)number.Value);
            var r = new MOGRecord(Engine);

            r.Items["day"] = new MOGNumber(Engine, d.Day);
            r.Items["month"] = new MOGNumber(Engine, d.Month);
            r.Items["year"] = new MOGNumber(Engine, d.Year);
            r.Items["hour"] = new MOGNumber(Engine, d.Hour);
            r.Items["minute"] = new MOGNumber(Engine, d.Minute);
            r.Items["second"] = new MOGNumber(Engine, d.Second);
            r.Items["dayOfYear"] = new MOGNumber(Engine, d.DayOfYear);
            r.Items["dayOfWeek"] = new MOGNumber(Engine, (int)d.DayOfWeek);

            Engine.StackPush(r);
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
