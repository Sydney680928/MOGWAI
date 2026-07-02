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
using System.Text.RegularExpressions;
using System.Threading;

namespace MOGWAI.Primitives
{
    internal class PrimitiveRegexMatch : MOGPrimitive
    {
        public override Version Birth => new(8, 15, 0);

        public PrimitiveRegexMatch(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        private Task<EvalResult> RegexMatch(string input, string pattern, int timeout)
        {
            var record = new MOGRecord(Engine);

            try
            {
                var regex = new Regex(pattern, RegexOptions.None, timeout == 0 ? Regex.InfiniteMatchTimeout : TimeSpan.FromMilliseconds(timeout));
                var match = regex.Match(input);

                record.SetBoolean("success", match.Success);

                if (match.Success)
                {
                    record.SetString("value", match.Value);
                    record.SetNumber("index", match.Index);
                    record.SetNumber("length", match.Length);

                    // groups: (names only)

                    var namedGroups = new MOGRecord(Engine);

                    foreach (var groupName in regex.GetGroupNames())
                    {
                        // GetGroupNames() also includes numeric groups ("0", "1"...)
                        // We only keep actual names (non-numeric)

                        if (!int.TryParse(groupName, out _))
                        {
                            var g = match.Groups[groupName];

                            if (g.Success)
                                namedGroups.SetString(groupName, g.Value);
                        }
                    }

                    record.SetItem("groups", namedGroups);

                    // groupsByIndex: (position 0 = full match)

                    var byIndex = new MOGList(Engine);

                    foreach (Group g in match.Groups)
                        byIndex.AddString(g.Success ? g.Value : "");

                    record.SetItem("groupsByIndex", byIndex);
                }

                Engine.StackPush(record);
            }
            catch (ArgumentException ex)
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.InvalidRegexPattern, Name, ex.Message));
            }
            catch (RegexMatchTimeoutException)
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.RegexTimeoutExceeded, Name, $"Regex timeout exceeded ({timeout}ms)"));
            }

            return Task.FromResult(EvalResult.NoError);
        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveRegexMatch(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // 2 possible signatures
            // "input" "pattern" regex.match
            // "input" "pattern" timeout regex.match

            // At least 2 arguments are required on the stack

            if (Engine.StackSize < 2)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            // Check the 3-argument signature first if enough arguments are on the stack

            if (Engine.StackSize >= 3)
            {
                var s3 = Engine.StackSign(3);

                if (s3.Count == 3)
                {
                    // We expect a number, a string, a string

                    if (s3[0] == typeof(MOGNumber) && s3[1] == typeof(MOGString) && s3[2] == typeof(MOGString))
                    {
                        var timeout = Engine.StackPopNumber();
                        var pattern = Engine.StackPopString();
                        var input = Engine.StackPopString();

                        // Call regex.match with the user-defined timeout

                        return RegexMatch(input.Value, pattern.Value, timeout.IntValue);
                    }
                }

                // Drop the 3-argument handling
                // Fall through to 2 arguments      
            }

            // Check the 2-argument signature

            var s2 = Engine.StackSign(2);

            if (s2.Count == 2)
            {
                // We expect a string, a string

                if (s2[0] == typeof(MOGString) && s2[1] == typeof(MOGString))
                {
                    var pattern = Engine.StackPopString();
                    var input = Engine.StackPopString();

                    // Call regex.match without a user-defined timeout = 1000ms

                    return RegexMatch(input.Value, pattern.Value, 1000);
                }
            }

            // Not enough arguments or unrecognized signature

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}