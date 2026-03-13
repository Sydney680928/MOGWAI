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
using System.Diagnostics;

namespace MOGWAI.Primitives
{
    internal class PrimitiveProcessStart : PrimitiveParamsRecord
    {
        public PrimitiveProcessStart(MogwaiEngine engine, string name) : base(engine, name)
        {
        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveConditionalOr(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // [filename: "toto.exe" arguments: "/u -K" workingDirectory: "C:\...." wait: true] process.start

            var filename = record.GetItem("filename") as MOGString;

            if (filename == null)
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, "filename key is mandatory"));

            var args = record.GetItem("arguments") as MOGString;
            var wd = record.GetItem("workingDirectory") as MOGString;
            var wait = record.GetItem("wait") as MOGBoolean;

            var process = new Process();
            process.StartInfo.FileName = filename!.Value;

            if (args != null)
                process.StartInfo.Arguments = args.Value;

            if (wd != null)
                process.StartInfo.WorkingDirectory = wd.Value;

            try
            {
                process.Start();

                if (wait != null && wait.Value)
                {
                    process.WaitForExit();
                    Engine.StackPushNumber(process.ExitCode);
                }
                else
                {
                    Engine.StackPushNumber(0);
                }
            }
            catch
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.InternalError, "Unable to start process"));
            }

            return Task.FromResult(EvalResult.NoError);
        }
    }
}
