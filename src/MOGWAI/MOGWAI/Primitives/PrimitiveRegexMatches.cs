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

namespace MOGWAI.Primitives
{
    internal class PrimitiveRegexMatches : MOGPrimitive
    {
        public override Version Birth => new(8, 15, 0);

        public PrimitiveRegexMatches(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        private Task<EvalResult> RegexMatches(string input, string pattern, int timeout, int maxResults)
        {
            if (maxResults <= 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "maxResults must be greater than 0"));

            try
            {
                var regexTimeout = timeout == 0 ? Regex.InfiniteMatchTimeout : TimeSpan.FromMilliseconds(timeout);
                var regex = new Regex(pattern, RegexOptions.None, regexTimeout);

                var matchesList = new MOGList(Engine);
                var count = 0;
                var truncated = false;

                var match = regex.Match(input);

                while (match.Success)
                {
                    if (count >= maxResults)
                    {
                        truncated = true;
                        break;
                    }

                    var matchRecord = new MOGRecord(Engine);

                    matchRecord.SetString("value", match.Value);
                    matchRecord.SetNumber("index", match.Index);
                    matchRecord.SetNumber("length", match.Length);

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

                    matchRecord.SetItem("groups", namedGroups);

                    // groupsByIndex: (position 0 = full match)

                    var byIndex = new MOGList(Engine);

                    foreach (Group g in match.Groups)
                        byIndex.AddString(g.Success ? g.Value : "");

                    matchRecord.SetItem("groupsByIndex", byIndex);

                    matchesList.AddItem(matchRecord);
                    count++;

                    match = match.NextMatch();
                }

                var result = new MOGRecord(Engine);
                result.SetItem("matches", matchesList);
                result.SetBoolean("truncated", truncated);

                Engine.StackPush(result);
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
            var obj = new PrimitiveRegexMatches(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // 3 possible signatures
            // "input" "pattern" regex.matches
            // "input" "pattern" timeout regex.matches
            // "input" "pattern" timeout maxResults regex.matches

            if (Engine.StackSize < 2)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            // 4-argument signature

            if (Engine.StackSize >= 4)
            {
                var s4 = Engine.StackSign(4);

                if (s4.Count == 4)
                {
                    if (s4[0] == typeof(MOGNumber) && s4[1] == typeof(MOGNumber) && s4[2] == typeof(MOGString) && s4[3] == typeof(MOGString))
                    {
                        var maxResults = Engine.StackPopNumber();
                        var timeout = Engine.StackPopNumber();
                        var pattern = Engine.StackPopString();
                        var input = Engine.StackPopString();

                        return RegexMatches(input.Value, pattern.Value, timeout.IntValue, maxResults.IntValue);
                    }
                }

                // Fall through to 3-argument handling
            }

            // 3-argument signature

            if (Engine.StackSize >= 3)
            {
                var s3 = Engine.StackSign(3);

                if (s3.Count == 3)
                {
                    if (s3[0] == typeof(MOGNumber) && s3[1] == typeof(MOGString) && s3[2] == typeof(MOGString))
                    {
                        var timeout = Engine.StackPopNumber();
                        var pattern = Engine.StackPopString();
                        var input = Engine.StackPopString();

                        return RegexMatches(input.Value, pattern.Value, timeout.IntValue, 1000);
                    }
                }

                // Fall through to 2-argument handling
            }

            // 2-argument signature

            var s2 = Engine.StackSign(2);

            if (s2.Count == 2)
            {
                if (s2[0] == typeof(MOGString) && s2[1] == typeof(MOGString))
                {
                    var pattern = Engine.StackPopString();
                    var input = Engine.StackPopString();

                    return RegexMatches(input.Value, pattern.Value, 1000, 1000);
                }
            }

            // Not enough arguments or unrecognized signature

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}