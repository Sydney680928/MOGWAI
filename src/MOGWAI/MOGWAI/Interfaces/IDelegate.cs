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

using System.Net;
using MOGWAI.Engine;
using MOGWAI.Objects;

namespace MOGWAI.Interfaces
{
    public interface IDelegate
    {
        #region PROGRAM LIFECYCLE

        Task ProgramStart(MogwaiEngine engine, string code) => Task.CompletedTask;

        Task ProgramEnd(MogwaiEngine engine, EvalResult result) => Task.CompletedTask;

        Task<EvalResult> EngineDidPause(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);    

        Task<EvalResult> EngineDidResume(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);   

        Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter) => Task.FromResult(EvalResult.NoError);

        #endregion

        #region CONSOLE

        Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
        {
            Console.Clear();
            return Task.FromResult(EvalResult.NoError);
        }

        Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
        {
            Console.WriteLine(message); 
            return Task.FromResult(EvalResult.NoError);
        }

        Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
        {
            Console.Write(message); 
            return Task.FromResult(EvalResult.NoError);
        }

        Task<EvalResult> ConsoleShow(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);

        Task<EvalResult> ConsoleHide(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);

        Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
        {
            Console.SetCursorPosition(x, y); 
            return Task.FromResult(EvalResult.NoError);
        }
        
        Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        {
            return Task.FromResult((EvalResult.NoError, Console.CursorLeft, Console.CursorTop));
        }

        Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color) => Task.FromResult(EvalResult.NoError);

        Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color) => Task.FromResult(EvalResult.NoError);

        Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        {
            var keyInfo = Console.ReadKey(true);
            return Task.FromResult((EvalResult.NoError, (int)keyInfo.Key));
        }

        Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
        {
            Console.Write(message); 
            var input = Console.ReadLine();
            return Task.FromResult((EvalResult.NoError, input));
        }

        #endregion

        #region HOST FUNCTIONS

        string[] HostFunctions(MogwaiEngine engine) => [];

        Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word) => Task.FromResult(EvalResult.NoExternalFunction);

        #endregion

        #region SKILLS

        string[] Skills(MogwaiEngine engine) => []; 

        #endregion

        #region DEBUG

        Task<EvalResult> DebugMessage(MogwaiEngine engine, string message) => Task.FromResult(EvalResult.NoError);

        Task<EvalResult> DebugClear(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);

        #endregion

        #region STUDIO

        Task<EvalResult> StudioDidConnect(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);  

        Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);   

        Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port) => Task.FromResult(EvalResult.NoError); 

        Task<EvalResult> SocketServerDidStop(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);   

        #endregion
    }
}
