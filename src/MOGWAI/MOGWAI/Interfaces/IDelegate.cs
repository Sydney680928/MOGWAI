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
        Task ProgramStart(MogwaiEngine engine, string code);

        Task ProgramEnd(MogwaiEngine engine, EvalResult result);

        Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine);

        Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message);

        Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message); 

        Task<EvalResult> ConsoleShow(MogwaiEngine engine);

        Task<EvalResult> ConsoleHide(MogwaiEngine engine);

        Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y);
        
        Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine);

        Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color);

        Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color);

        Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine);

        Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message);
        
        string[] HostFunctions(MogwaiEngine engine);

        Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word);

        Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter);

        Task<EvalResult> DebugMessage(MogwaiEngine engine, string message);

        Task<EvalResult> DebugClear(MogwaiEngine engine);
        
        Task<EvalResult> EngineDidPause(MogwaiEngine engine);

        Task<EvalResult> EngineDidResume(MogwaiEngine engine);

        Task<EvalResult> StudioDidConnect(MogwaiEngine engine);

        Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine);

        Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port);
        
        Task<EvalResult> SocketServerDidStop(MogwaiEngine engine);

    }
}
