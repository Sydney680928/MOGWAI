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
    internal class PrimitiveFromDuration : MOGPrimitive
    {
        public PrimitiveFromDuration(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override Task<EvalResult> EngineEval()
        {
            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            if (s[0] == typeof(MOGRecord))
            {
                var record = Engine.StackPop() as MOGRecord;

                int days = 0;
                int hours = 0;
                int minutes = 0;
                int seconds = 0;
                int milliseconds = 0;

                if (record!.GetItem("days") is MOGNumber d)
                    days = d.IntValue;

                if (record!.GetItem("hours") is MOGNumber h)
                    hours = h.IntValue;

                if (record!.GetItem("minutes") is MOGNumber mm)
                    minutes = mm.IntValue;

                if (record!.GetItem("seconds") is MOGNumber ss)
                    seconds = ss.IntValue;

                if (record!.GetItem("ms") is MOGNumber ms)
                    milliseconds = ms.IntValue;

                try
                {
                    var dt = new TimeSpan(days, hours, minutes, seconds, milliseconds);
                    Engine.StackPushNumber(dt.Ticks);
                    return Task.FromResult(EvalResult.NoError);
                }
                catch
                {

                }

                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "day: month: year: keys are mandatories"));
            }

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
