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
    internal class PrimitiveRegexReplace : MOGPrimitive
    {
        public override Version Birth => new(8, 15, 0);

        public PrimitiveRegexReplace(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        private Task<EvalResult> RegexReplace(string input, string pattern, string replacement, int timeout)
        {
            try
            {
                var regexTimeout = timeout == 0 ? Regex.InfiniteMatchTimeout : TimeSpan.FromMilliseconds(timeout);
                var regex = new Regex(pattern, RegexOptions.None, regexTimeout);

                var result = regex.Replace(input, replacement);

                Engine.StackPushString(result);

                return Task.FromResult(EvalResult.NoError);
            }
            catch (ArgumentException ex)
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.InvalidRegexPattern, Name, ex.Message));
            }
            catch (RegexMatchTimeoutException)
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.RegexTimeoutExceeded, Name, $"Regex timeout exceeded ({timeout}ms)"));
            }
        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveRegexReplace(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> EngineEval()
        {
            // 2 possible signatures
            // "input" "pattern" "replacement" regex.replace
            // "input" "pattern" "replacement" timeout regex.replace

            // At least 3 arguments are required on the stack

            if (Engine.StackSize < 3)
                return Task.FromResult(EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name));

            // Check the 4-argument signature first if enough arguments are on the stack

            if (Engine.StackSize >= 4)
            {
                var s4 = Engine.StackSign(4);

                if (s4.Count == 4)
                {
                    // We expect a number, a string, a string, a string

                    if (s4[0] == typeof(MOGNumber) && s4[1] == typeof(MOGString) && s4[2] == typeof(MOGString) && s4[3] == typeof(MOGString))
                    {
                        var timeout = Engine.StackPopNumber();
                        var replacement = Engine.StackPopString();
                        var pattern = Engine.StackPopString();
                        var input = Engine.StackPopString();

                        // Call regex.replace with the user-defined timeout

                        return RegexReplace(input.Value, pattern.Value, replacement.Value, timeout.IntValue);
                    }
                }

                // Drop the 4-argument handling
                // Fall through to 3 arguments
            }

            // Check the 3-argument signature

            var s3 = Engine.StackSign(3);

            if (s3.Count == 3)
            {
                // We expect a string, a string, a string

                if (s3[0] == typeof(MOGString) && s3[1] == typeof(MOGString) && s3[2] == typeof(MOGString))
                {
                    var replacement = Engine.StackPopString();
                    var pattern = Engine.StackPopString();
                    var input = Engine.StackPopString();

                    // Call regex.replace without a user-defined timeout = 1000ms

                    return RegexReplace(input.Value, pattern.Value, replacement.Value, 1000);
                }
            }

            // Not enough arguments or unrecognized signature

            return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name));
        }
    }
}
