using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MOGWAI.Engine;
using MOGWAI.Interfaces;
using MOGWAI.Objects;

namespace MogwaiRepl.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDelegate
{
    private MogwaiEngine _engine;
    private readonly List<string> _history = new();
    private int _historyIndex = -1;

    [ObservableProperty]
    private string _inputCode = string.Empty;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string? _currentFilePath;

    [ObservableProperty]
    private string _studioStatus = "📡 Connect to Studio";

    [ObservableProperty]
    private bool _isStudioRunning;

    public ObservableCollection<string> OutputLines { get; } = new();

    public string Title => CurrentFilePath is null
        ? "🐉 MOGWAI REPL"
        : $"🐉 MOGWAI REPL — {Path.GetFileName(CurrentFilePath)}";

    public MainWindowViewModel()
    {
        _engine = new MogwaiEngine("MogwaiRepl");
        _engine.Delegate = this;
    }

    // --- RUN ---

    [RelayCommand]
    private async Task RunAsync()
    {
        if (string.IsNullOrWhiteSpace(InputCode)) return;

        if (_history.Count == 0 || _history[^1] != InputCode)
            _history.Add(InputCode);
        _historyIndex = -1;

        IsRunning = true;
        AddLine($"» {InputCode.Replace("\n", " ↵ ")}");

        try
        {
            var result = await _engine.RunAsync(InputCode, debugMode: false);
            if (result != EvalResult.NoError)
                AddLine($"[MW error: {result}]");
        }
        catch (Exception ex)
        {
            AddLine($"[EXCEPTION] {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    // --- STOP ---

    [RelayCommand]
    private void Stop()
    {
        _engine.Halt();
    }

    // --- CLEAR OUTPUT ---

    [RelayCommand]
    private void ClearOutput()
    {
        OutputLines.Clear();
    }

    // --- NEW ---

    [RelayCommand]
    private void New()
    {
        InputCode = string.Empty;
        CurrentFilePath = null;
        OnPropertyChanged(nameof(Title));
    }

    // --- OPEN ---

    [RelayCommand]
    private async Task OpenAsync()
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Open a MOGWAI file",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("MOGWAI Script")
                {
                    Patterns = new[] { "*.mog" }
                },
                new Avalonia.Platform.Storage.FilePickerFileType("All files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        };

        var window = GetMainWindow();
        if (window is null) return;

        var files = await window.StorageProvider.OpenFilePickerAsync(dialog);
        if (files.Count == 0) return;

        var file = files[0];
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        InputCode = await reader.ReadToEndAsync();
        CurrentFilePath = file.Path.LocalPath;
        OnPropertyChanged(nameof(Title));
    }

    // --- SAVE ---

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (CurrentFilePath is not null)
            await File.WriteAllTextAsync(CurrentFilePath, InputCode);
        else
            await SaveAsAsync();
    }

    // --- SAVE AS ---

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            Title = "Save MOGWAI file",
            SuggestedFileName = "script.mog",
            FileTypeChoices = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("MOGWAI Script")
                {
                    Patterns = new[] { "*.mog" }
                }
            }
        };

        var window = GetMainWindow();
        if (window is null) return;

        var file = await window.StorageProvider.SaveFilePickerAsync(dialog);
        if (file is null) return;

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(InputCode);
        CurrentFilePath = file.Path.LocalPath;
        OnPropertyChanged(nameof(Title));
    }

    // --- HISTORY ---

    [RelayCommand]
    private void HistoryUp()
    {
        if (_history.Count == 0) return;
        if (_historyIndex == -1)
            _historyIndex = _history.Count - 1;
        else if (_historyIndex > 0)
            _historyIndex--;
        InputCode = _history[_historyIndex];
    }

    [RelayCommand]
    private void HistoryDown()
    {
        if (_historyIndex == -1) return;
        if (_historyIndex < _history.Count - 1)
        {
            _historyIndex++;
            InputCode = _history[_historyIndex];
        }
        else
        {
            _historyIndex = -1;
            InputCode = string.Empty;
        }
    }

    // --- STUDIO ---

    [RelayCommand]
    private async Task ToggleStudioAsync()
    {
        if (IsStudioRunning)
        {
            _engine.Halt();
            return;
        }

        IsStudioRunning = true;
        StudioStatus = "⏏ Disconnect from Studio";
        AddLine("[STUDIO] Server started on port 1968");

        await _engine.StartNetworkCommunication();

        IsStudioRunning = false;
        StudioStatus = "📡 Connect to Studio";
        AddLine("[STUDIO] Server stopped");
    }

    // --- HELPERS ---

    private void AddLine(string line)
        => Dispatcher.UIThread.Post(() => OutputLines.Add(line));

    private static Avalonia.Controls.Window? GetMainWindow()
        => (Avalonia.Application.Current?.ApplicationLifetime
            as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;

    // --- IDelegate ---

    public Task ProgramStart(MogwaiEngine engine, string code)
        => Task.CompletedTask;

    public Task ProgramEnd(MogwaiEngine engine, EvalResult result)
        => Task.CompletedTask;

    public Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
    {
        Dispatcher.UIThread.Post(() => OutputLines.Clear());
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
    {
        AddLine(message);
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
    {
        AddLine(message);
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> ConsoleShow(MogwaiEngine engine)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> ConsoleHide(MogwaiEngine engine)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
        => Task.FromResult(EvalResult.NoError);

    public Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        => Task.FromResult((EvalResult.NoError, 0, 0));

    public Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color)
        => Task.FromResult(EvalResult.NoError);

    public Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        => Task.FromResult((EvalResult.NoError, -1));

    public Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
    {
        var tcs = new TaskCompletionSource<(EvalResult, string?)>();

        Dispatcher.UIThread.Post(async () =>
        {
            var window = GetMainWindow();
            if (window is null)
            {
                tcs.SetResult((EvalResult.NoError, null));
                return;
            }

            var dialog = new Views.PromptWindow(message);
            var result = await dialog.ShowDialog<string?>(window);
            tcs.SetResult((EvalResult.NoError, result));
        });

        return tcs.Task;
    }

    public string[] HostFunctions(MogwaiEngine engine)
        => [];

    public Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
        => Task.FromResult(EvalResult.NoExternalFunction);

    public Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> DebugClear(MogwaiEngine engine)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> EngineDidPause(MogwaiEngine engine)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> EngineDidResume(MogwaiEngine engine)
        => Task.FromResult(EvalResult.NoError);

    public Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
    {
        Dispatcher.UIThread.Post(() =>
            AddLine("[STUDIO] Client connected"));
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
    {
        Dispatcher.UIThread.Post(() =>
            AddLine("[STUDIO] Client disconnected"));
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port)
    {
        Dispatcher.UIThread.Post(() =>
            AddLine($"[STUDIO] Socket server started on {address}:{port}"));
        return Task.FromResult(EvalResult.NoError);
    }

    public Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
    {
        Dispatcher.UIThread.Post(() =>
            AddLine("[STUDIO] Socket server stopped"));
        return Task.FromResult(EvalResult.NoError);
    }
}