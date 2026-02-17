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
using System.Globalization;

namespace MOGWAI_CLI
{
    internal class Program
    {
        private static readonly MogwaiEngine _engine = new MogwaiEngine("MOGWAI CLI", true, true);

        static async Task Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            Console.Title = "MOGWAI CLI";
            Console.Clear();

            Console.WriteLine(MogwaiEngine.RuntimePrompt);
            Console.WriteLine();

            await Task.Delay(2000);

            Console.CancelKeyPress += Console_CancelKeyPress;

            _engine.Delegate = new EngineDelegate(_engine);

            if (args.Length > 0)
            {             
                try
                {
                    var code = File.ReadAllText(args[0]);
                    var result = await _engine.RunAsync(code, false);

                    Console.WriteLine();
                    Console.WriteLine(result);

                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return;
            }

            while (true)
            {
                Console.WriteLine();
                Console.Write("MOGWAI > ");

                var code = Console.ReadLine() ?? string.Empty;

                if (code.ToUpper() == "BYE")
                {
                    break;
                }
                else if (code.ToUpper() == "STUDIO")
                {
                    await _engine.StartNetworkCommunication();

                    while (true)
                    {
                        await Task.Delay(250);
                    }
                }

                try
                {
                    var task = _engine.RunAsync(code, true);

                    while (task.Status != TaskStatus.RanToCompletion)
                    {
                        if (Console.KeyAvailable)
                        {
                            var k = Console.ReadKey(true);

                            if (k.Key == ConsoleKey.F10)
                            {
                                _engine.DebugFireNextStepSignal();
                            }
                            else if (k.Key == ConsoleKey.F5)
                            {
                                _engine.DebugFireResumeSignal();
                            }
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine(task.Result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                _engine.Halt();
                e.Cancel = true;
            }
        }
    }
}
