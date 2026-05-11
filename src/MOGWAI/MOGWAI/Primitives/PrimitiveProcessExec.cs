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
using System.Text;

namespace MOGWAI.Primitives
{
    /// <summary>
    /// process.exec — launches a process, captures stdout/stderr, optionally writes to stdin.
    /// Pushes a result record [status: exitCode output: "..." error: "..."] onto the stack.
    ///
    /// Usage:
    ///   [filename: "myservice.exe" arguments: "--mode calc" input: "data"] process.exec -> 'r'
    ///   r status: get   # exit code (0 = success)
    ///   r output: get   # stdout
    ///   r error: get    # stderr
    /// </summary>
    internal class PrimitiveProcessExec : PrimitiveParamsRecord
    {
        public PrimitiveProcessExec(MogwaiEngine engine, string name) : base(engine, name)
        {
        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveProcessExec(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // [
            //   filename:         "myservice.exe"   (required)
            //   arguments:        "--flag value"    (optional)
            //   workingDirectory: "C:\..."          (optional)
            //   input:            "stdin data"      (optional)
            // ] process.exec

            var filename = record.GetItem("filename") as MOGString;

            if (filename == null)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, "filename key is mandatory");

            var args  = record.GetItem("arguments")        as MOGString;
            var wd    = record.GetItem("workingDirectory") as MOGString;
            var input = record.GetItem("input")            as MOGString;

            var process = new Process();

            process.StartInfo.FileName               = filename.Value;
            process.StartInfo.UseShellExecute        = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError  = true;
            process.StartInfo.RedirectStandardInput  = input is not null;
            process.StartInfo.CreateNoWindow         = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding  = Encoding.UTF8;

            if (args is not null)
                process.StartInfo.Arguments = args.Value;

            if (wd is not null)
                process.StartInfo.WorkingDirectory = wd.Value;

            try
            {
                process.Start();

                // Write stdin before reading stdout/stderr to avoid deadlock
                if (input is not null)
                {
                    await process.StandardInput.WriteAsync(input.Value);
                    process.StandardInput.Close();
                }

                // Read stdout and stderr in parallel to avoid buffer deadlock
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask  = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                var output = await outputTask;
                var error  = await errorTask;

                var result = new MOGRecord(Engine);
                result.SetNumber("status", process.ExitCode);
                result.SetString("output", output.TrimEnd('\r', '\n'));
                result.SetString("error",  error.TrimEnd('\r', '\n'));

                Engine.StackPush(result);
            }
            catch
            {
                return EvalResult.Failure(Engine, Error.InternalError, "Unable to execute process");
            }

            return EvalResult.NoError;
        }
    }
}
