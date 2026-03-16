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

using System.Text;

namespace MOGWAI.Engine
{
    public class EvalResult
    {
        public readonly static EvalResult NoError = new EvalResult();

        public readonly static EvalResult NoExternalFunction = new EvalResult
        {
            Error = Error.UnknownWordError
        };

        public readonly static EvalResult NoPluginFunction = new EvalResult
        {
            Error = Error.UnknownWordError
        };

        public Error Error { get; init; } = Error.None;

        public string[] Informations { get; init; }

        public TimeSpan Duration { get; set; }

        public int StartErrorPosition { get; set; } = -1;

        public int EndErrorPosition { get; set; } = -1;

        public bool IsError => Error != Error.None; 

        public bool IsSuccess => Error == Error.None;   

        public MogwaiExecutionContext? ExecutionContext { get; private set; }

        private EvalResult()
        {
            Error = Error.None;
            Informations = [];
        }

        public static EvalResult Failure(MogwaiEngine engine, Error error, params string[] informations)
        {
            engine.LastError = error;

            return new EvalResult
            {
                Error = error,
                ExecutionContext = engine.CurrentEvalObject?.ExecutionContext,
                Informations = informations
            };
        }

        public static EvalResult Failure(MogwaiEngine engine, Error error, MOGPrimitive primitive, params string[] informations)
        {
            engine.LastError = error;

            var inf = informations.ToList();
            inf.Insert(0, primitive.FriendlyName);

            return new EvalResult
            {
                Error = error,
                ExecutionContext = primitive.ExecutionContext,
                Informations = inf.ToArray()
            };
        }

        public static EvalResult ParseFailure(MogwaiEngine engine, params string[] informations)
        {
            engine.LastError = Error.ParseError;

            return new EvalResult
            {
                Error = engine.LastError,
                StartErrorPosition = engine.LastParserStartErrorPosition,
                EndErrorPosition = engine.LastParserEndErrorPosition,
                ExecutionContext = engine.LastParserExecutionContext,
                Informations = informations
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Error == Error.None)
            {
                sb.AppendLine("OK");
            }
            else
            {
                sb.AppendLine(Error.ToString());

                if (Informations.Length > 0)
                {
                    foreach (var info in Informations)
                    {
                        sb.AppendLine(info);
                    }
                }
            }

            if (Duration > TimeSpan.Zero)
                sb.AppendLine($"execution time {Duration}");

            return sb.ToString().TrimEnd();
        }
    }
}
