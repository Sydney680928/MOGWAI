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
using MOGWAI.Interfaces;
using MOGWAI.Objects;
using System.Net;

namespace MOGWAI_TEST
{
    internal class EngineDelegate : IDelegate
    {
        private MogwaiEngine _engine;
        private object _consoleAccessLocker = new();

        public EngineDelegate(MogwaiEngine engine)
        {
            _engine = engine;
        }

        public Task ProgramStart(MogwaiEngine engine, string code)
        {
            return Task.CompletedTask;
        }

        public Task ProgramEnd(MogwaiEngine engine, EvalResult result)
        {
            return Task.CompletedTask;
        }

        public Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
        {
            lock (_consoleAccessLocker)
                Console.Clear();


            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
        {
            lock (_consoleAccessLocker)
                Console.WriteLine(message);

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
        {
            lock (_consoleAccessLocker)
                Console.Write(message);

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleShow(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleHide(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        {
            var r = Console.GetCursorPosition();
            return Task.FromResult((EvalResult.NoError, r.Left, r.Top));
        }

        public Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color)
        {
            switch (color.ToLower())
            {
                case "black":
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case "blue":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case "cyan":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case "gray":
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "green":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "magenta":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case "red":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "white":
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "yellow":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color)
        {
            switch (color.ToLower())
            {
                case "black":
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case "blue":
                    Console.BackgroundColor = ConsoleColor.Blue;
                    break;
                case "cyan":
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    break;
                case "gray":
                    Console.BackgroundColor = ConsoleColor.Gray;
                    break;
                case "green":
                    Console.BackgroundColor = ConsoleColor.Green;
                    break;
                case "magenta":
                    Console.BackgroundColor = ConsoleColor.Magenta;
                    break;
                case "red":
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                case "white":
                    Console.BackgroundColor = ConsoleColor.White;
                    break;
                case "yellow":
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        {
            int key = -1;

            if (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(true);
                key = (int)keyInfo.Key;
            }

            return Task.FromResult((EvalResult.NoError, key));

        }

        public Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
        {
            Console.Write(message);
            var r = Console.ReadLine();
            return Task.FromResult((EvalResult.NoError, r));
        }

        public string[] HostFunctions(MogwaiEngine engine) => ["?s", "run"];

        public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
        {
            if (word == "?s")
            {
                if (engine.StackSize == 0)
                {
                    lock (_consoleAccessLocker)
                    {
                        Console.WriteLine("empty stack");
                        Console.WriteLine();
                    }
                }
                else
                {
                    var s = (int)(Math.Log10(engine.StackSize) + 1);
                    var format = new string('0', (int)s);
                    var items = engine.StackArray();

                    lock (_consoleAccessLocker)
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            Console.Write((i + 1).ToString(format));
                            Console.Write(" ");
                            Console.WriteLine(items[i].ToString());
                        }

                        Console.WriteLine();
                    }
                }

                return EvalResult.NoError;
            }
            else if (word == "run")
            {
                var s = engine.StackSign(1);

                if (s.Count == 0)
                    return EvalResult.Failure(engine, Error.TooFewArgumentsError, word);

                if (s[0] == typeof(MOGString))
                {
                    var codeFile = engine.StackPop() as MOGString;

                    try
                    {
                        var bytes = File.ReadAllBytes(codeFile!.Value);
                        var result = engine.GetCodeFormBytes(bytes);

                        if (result.code != null)
                        {
                            return await engine.RunAsync(result.code, false);
                        }
                        else
                        {
                            return EvalResult.Failure(engine, Error.ParseError, word);
                        }
                    }
                    catch
                    {
                        return EvalResult.Failure(engine, Error.FileOperationError, word);
                    }
                }

                return EvalResult.Failure(engine, Error.BadArgumentTypeError, word);
            }

            return EvalResult.NoExternalFunction;
        }

        public Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> EngineDidPause(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> EngineDidResume(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> DebugClear(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
