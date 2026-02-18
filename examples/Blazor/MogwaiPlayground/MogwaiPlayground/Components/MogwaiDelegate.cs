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
using MOGWAI.Interfaces;
using MOGWAI.Objects;

namespace MogwaiPlayground.Components;

/// <summary>
/// IDelegate implementation for Blazor WASM.
/// Captures the MOGWAI engine output into a list of lines
/// displayed in the UI terminal.
/// </summary>
public class MogwaiDelegate : IDelegate
{
    // Callback invoked when a new line needs to be displayed

    public event Action<TerminalLine>? OnOutput;

    // Callback used to request user input (MOGWAI prompt)

    public Func<string, Task<string?>>? PromptHandler { get; set; }

    // ─── IDelegate ────────────────────────────────────────────────────────────

    public Task ProgramStart(MogwaiEngine engine, string code)
        => Task.CompletedTask;

    public Task ProgramEnd(MogwaiEngine engine, EvalResult result)
        => Task.CompletedTask;

    public Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
    {
        OnOutput?.Invoke(new TerminalLine(message, LineKind.Output));
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
    {
        // ConsolePrint sans saut de ligne — on l'ajoute quand même comme ligne
        // pour simplifier l'affichage (le terminal est ligne par ligne)
        OnOutput?.Invoke(new TerminalLine(message, LineKind.Output));
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
    {
        OnOutput?.Invoke(new TerminalLine(string.Empty, LineKind.Clear));
        return Task.FromResult(EvalResult.NoError);
    }

    public async Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
    {
        if (PromptHandler is not null)
        {
            var value = await PromptHandler(message);
            return (EvalResult.NoError, value);
        }

        // Fallback si aucun handler n'est branché
        return (EvalResult.NoError, null);
    }

    public string[] HostFunctions(MogwaiEngine engine)
        => Array.Empty<string>();

    public Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
        => Task.FromResult(EvalResult.NoExternalFunction);

    // ─── Advanced console ─────────────────────────────────────────────────────

    public async Task<EvalResult> ConsoleShow(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }
        
    public async Task<EvalResult> ConsoleHide(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int row, int col)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return (EvalResult.NoError, 0, 0);
    }

    public async Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return (EvalResult.NoError, -1);
    }

    // ─── Runtime ──────────────────────────────────────────────────────────────

    public async Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject obj)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    // ─── Debug / Studio ───────────────────────────────────────────────────────

    public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }
    
    public async Task<EvalResult> DebugClear(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<EvalResult> EngineDidPause(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<EvalResult> EngineDidResume(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }
    
    // ─── Socket ───────────────────────────────────────────────────────────────

    public async Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }
    
    public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
    {
        await Task.CompletedTask;
        return EvalResult.NoError;
    }
}

/// <summary>Represents a line in the terminal.</summary>

public record TerminalLine(string Text, LineKind Kind);

public enum LineKind
{
    /// <summary>Normal output line from the MOGWAI engine.</summary>
    Output,
    
    /// <summary>Error line.</summary>
    Error,
    
    /// <summary>Line entered by the user (echoed in the terminal).</summary>
    Input,
    
    /// <summary>Signal to clear the screen.</summary>
    Clear,
    
    /// <summary>System message (welcome, help, etc.)</summary>
    System,
}
