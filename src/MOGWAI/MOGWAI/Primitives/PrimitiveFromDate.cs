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
    internal class PrimitiveFromDate : MOGPrimitive
    {
        public PrimitiveFromDate(MogwaiEngine engine, string name, bool isPrivate = false, string helpText = "") : base(engine, name, isPrivate, helpText)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveFromDate(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            await Task.CompletedTask;

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGRecord))
            {
                var record = Engine.StackPopRecord();

                // les clés day: month: year: sont obligatoires

                if (record.Items.ContainsKey("day") && record.Items.ContainsKey("month") && record.Items.ContainsKey("year"))
                {
                    var now = DateTime.Now;
                    int day;
                    int month;
                    int year;
                    int hour = 0;
                    int minute = 0;
                    int second = 0;

                    if (record.GetItem("day") is MOGNumber d && record.GetItem("month") is MOGNumber m && record.GetItem("year") is MOGNumber y)
                    {
                        day = d.IntValue;
                        month = m.IntValue;
                        year = y.IntValue;

                        if (record.GetItem("hour") is MOGNumber hh)
                            hour = hh.IntValue;

                        if (record.GetItem("minute") is MOGNumber mm)
                            minute = mm.IntValue;

                        if (record.GetItem("second") is MOGNumber ss)
                            second = ss.IntValue;

                        try
                        {
                            var dt = new DateTime(year, month, day, hour, minute, second);
                            Engine.StackPushNumber(dt.Ticks);
                            return EvalResult.NoError;
                        }
                        catch
                        {

                        }
                    }
                }

                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "day: month: year: keys are mandatories");
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
