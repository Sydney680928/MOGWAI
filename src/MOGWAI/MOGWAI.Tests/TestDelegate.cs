// Copyright 2026 Stéphane Sibué
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

namespace MOGWAI.Tests
{
    public class TestDelegate : IDelegate
    {
        // Capture tout ce que MOGWAI "affiche"
        
        public List<string> Output { get; } = new();

        public Task ProgramStart(MogwaiEngine engine, string code) => Task.CompletedTask;
        
        public Task ProgramEnd(MogwaiEngine engine, EvalResult result) => Task.CompletedTask;

        public Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
        {
            Output.Add(message);
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
        {
            Output.Add(message);
            return Task.FromResult(EvalResult.NoError);
        }

        // Tout le reste → NoError, on s'en fiche dans les tests
        
        public Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<(EvalResult, string?)> Prompt(MogwaiEngine engine, string message) => Task.FromResult((EvalResult.NoError, (string?)null));
       
        public Task<EvalResult> ConsoleShow(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> ConsoleHide(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y) => Task.FromResult(EvalResult.NoError);
        
        public Task<(EvalResult, int, int)> ConsoleGetCursorPosition(MogwaiEngine engine) => Task.FromResult((EvalResult.NoError, 0, 0));
        
        public Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color) => Task.FromResult(EvalResult.NoError);
        
        public Task<(EvalResult, int)> ConsoleGetInputKey(MogwaiEngine engine) => Task.FromResult((EvalResult.NoError, 0));
       
        public string[] HostFunctions(MogwaiEngine engine) => Array.Empty<string>();
        
        public Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word) => Task.FromResult(EvalResult.NoExternalFunction);
        
        public Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> DebugMessage(MogwaiEngine engine, string message) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> DebugClear(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> EngineDidPause(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> EngineDidResume(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> StudioDidConnect(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port) => Task.FromResult(EvalResult.NoError);
        
        public Task<EvalResult> SocketServerDidStop(MogwaiEngine engine) => Task.FromResult(EvalResult.NoError);
    }
}
