# Avalonia MOGWAI REPL

A cross-platform REPL and script editor for MOGWAI, built with Avalonia UI. Run MOGWAI scripts interactively on Windows, Linux and macOS.

![Avalonia MOGWAI REPL](../../../images/img16.png)

---

## Features

- **Cross-platform** - Runs on Windows, Linux and macOS
- **Script Editor** - Multi-line code editor with keyboard shortcuts
- **Run/Stop Controls** - Execute and interrupt scripts at any time
- **File Management** - Open, save and save-as `.mog` script files
- **Command History** - Navigate previous commands with `Ctrl+↑↓`
- **Prompt Dialog** - Native input dialog for `prompt` instructions
- **Studio Mode** - Connect MOGWAI VS Code extension for live debugging

---

## Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- VS Code (recommended) with [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit), or any .NET IDE
- MOGWAI package (automatically restored from NuGet)

### Build and Run

```bash
# Navigate to the Avalonia directory
cd examples/Avalonia/MogwaiRepl

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

Or open the folder in VS Code and press `F5`.

---

## Usage

### Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+Enter` | Run the script |
| `Ctrl+↑` | Previous command in history |
| `Ctrl+↓` | Next command in history |

### Studio Mode

Click **📡 Connect to Studio** to start the MOGWAI socket server on port `1968`.
The [MOGWAI VS Code extension](https://github.com/Sydney680928/mogwai) will connect automatically and enable live debugging.

Click **⏏ Disconnect from Studio** to stop the server.

---

## Example Scripts

### Hello World

```mogwai
"Hello from Avalonia!" ?
```

### Simple Calculation

```mogwai
2 3 + ?
```

### Variables and Interpolation

```mogwai
42 -> 'N'
"The answer is {! $N }" eval ?
```

### Loop

```mogwai
1 10 for 'i' do { i ? }
```

### Timer

```mogwai
timer 'T1' every 1000 do { "tick" ? }
'T1' timer.start
forever do { "waiting..." ? 500 wait }
```

### OOP (v8.6+)

```mogwai
class 'Point' do
{
    public:
        x: .number
        y: .number
}

new 'Point' -> 'p'
10 p<-x
20 p<-y
"Point: {! p->x }, {! p->y }" eval ?
free p
```

---

## Implementation Details

### Engine Configuration

```csharp
var engine = new MogwaiEngine("MogwaiRepl");
engine.Delegate = this; // ViewModel implements IDelegate
```

### IDelegate Implementation

The ViewModel implements `IDelegate` to capture MOGWAI output and route it to the UI:

```csharp
public Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
{
    Dispatcher.UIThread.Post(() => OutputLines.Add(message));
    return Task.FromResult(EvalResult.NoError);
}
```

### Thread-Safe UI Updates

All output from the MOGWAI runtime is dispatched to the UI thread via Avalonia's `Dispatcher`:

```csharp
private void AddLine(string line)
    => Dispatcher.UIThread.Post(() => OutputLines.Add(line));
```

### Prompt Dialog

The `prompt` instruction opens a native Avalonia dialog:

```csharp
public Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
{
    var tcs = new TaskCompletionSource<(EvalResult, string?)>();
    Dispatcher.UIThread.Post(async () =>
    {
        var dialog = new PromptWindow(message);
        var result = await dialog.ShowDialog<string?>(window);
        tcs.SetResult((EvalResult.NoError, result));
    });
    return tcs.Task;
}
```

### Studio Mode

Studio mode starts the MOGWAI socket server, allowing the VS Code extension to connect:

```csharp
await _engine.StartNetworkCommunication();
```

---

## Publishing

### Windows

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
```

### Linux

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64
```

### macOS (Apple Silicon)

```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64
```

### macOS (Intel)

```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64
```

> **Note:** On macOS, unsigned binaries may require `xattr -cr MogwaiRepl` to bypass Gatekeeper.

---

## Documentation

- **[Language Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_EN.md)** - Complete MOGWAI language guide
- **[Function Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 240+ built-in functions
- **[Integration Guide](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI in your .NET apps

---

## Related Examples

- **[MOGWAI CLI](https://github.com/Sydney680928/mogwai/tree/main/examples/Console)** - Command-line interface and REPL
- **[WinForms Example](https://github.com/Sydney680928/mogwai/tree/main/examples/WinFormsMogwai)** - Turtle graphics on Windows
- **[MAUI Example](https://github.com/Sydney680928/mogwai/tree/main/examples/MAUI)** - Cross-platform mobile app
- **[Blazor Example](https://github.com/Sydney680928/mogwai/tree/main/examples/Blazor)** - Blazor WASM app

---

## Use Cases

The Avalonia MOGWAI REPL is ideal for:

- **Development** - Interactive testing of MOGWAI scripts during development
- **Education** - Learning MOGWAI in a visual, cross-platform environment
- **Prototyping** - Quickly experimenting with MOGWAI features
- **Debugging** - Live debugging via Studio mode with the VS Code extension

---

## License

Apache License 2.0

See [LICENSE](https://github.com/Sydney680928/mogwai/tree/main/LICENSE) and [NOTICE](https://github.com/Sydney680928/mogwai/tree/main/NOTICE) for details.

---

## Contributing

Ideas for new features?

- **Report Issues:** [GitHub Issues](https://github.com/Sydney680928/mogwai/issues)
- **Pull Requests:** Contributions welcome!

Suggestions:
- Syntax highlighting with AvalonEdit
- Stack viewer panel
- Script output export
- Multiple tabs

---

*For more information, visit [mogwai.eu.com](https://www.mogwai.eu.com)*
