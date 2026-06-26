# MOGWAI - An Embeddable RPN Scripting Engine for .NET

**Embeddable. Extensible. NativeAOT-friendly.** A small stack-based RPN runtime you can drop into any .NET app — desktop, mobile, or IoT.

**10+ years of development** | **304 built-in functions** | **NativeAOT-ready** | **Open Source (Apache 2.0)**

---

## What is MOGWAI?

MOGWAI is a lightweight scripting engine you embed in your .NET applications — to script complex workflows, expose safe user-customizable logic, or design your own DSL, all without leaving the .NET runtime (NativeAOT included). Under the hood it's a stack-based, concatenative language in the tradition of the legendary HP calculators (HP 28S, HP 48) — which gives it clean, unambiguous semantics with no operator precedence to reason about.

### The stack, in 30 seconds

MOGWAI reads left to right. Values are pushed onto a stack; operators consume values from it and push the result back. There's no operator precedence and no parentheses — the order on the stack *is* the program.

```
3            →  [ 3 ]
4            →  [ 3 4 ]
+            →  [ 7 ]
2            →  [ 7 2 ]
*            →  [ 14 ]
```

The same calculation on a single line — `3 4 + 2 *` — also leaves `[ 14 ]` on the stack. That's the whole idea: small pieces compose, and what you see is exactly what happens.

### Not ready for RPN yet? Use `calc`

New to stack-based thinking? You don't have to convert every formula by hand. The `calc` primitive accepts a regular infix expression as a string — parentheses, operator precedence and all — converts it to RPN under the hood (Dijkstra's Shunting-yard algorithm), and runs it immediately.

```
"5 * X + (7 + sin(Y))" calc
```

Write formulas the way you already know them, and grow into RPN at your own pace.

---

## Key Features

- **Stack-Based RPN Syntax** - Clean, unambiguous, no operator precedence
- **Infix Expressions via `calc`** - Write classic math formulas (`"5 * X + 2"`), auto-converted to RPN via Shunting-yard
- **304 Built-in Functions** - Math, strings, lists, files, HTTP, and more
- **Async/Await Support** - Modern asynchronous execution
- **Plugin System** - Clean plugin contract via `MOGWAI.IPlugin` — official plugins in development
- **Battle-Tested** - 10+ years of real-world usage
- **Extensible** - Easy integration with .NET applications
- **NativeAOT-Ready** - Embed in ahead-of-time compiled .NET apps
- **Cross-Platform** - Windows, Linux, macOS, Android, iOS
- **VS Code Extension** - Syntax highlighting, autocompletion, run & debug directly from VS Code ([install](https://marketplace.visualstudio.com/items?itemName=mogwai.mogwai-language))

---

## Quick Start

### Installation

```bash
dotnet add package MOGWAI
```

### Hello World

```csharp
using MOGWAI.Engine;
using MOGWAI.Interfaces;

// Create the engine
var engine = new MogwaiEngine("MyApp");
engine.Delegate = this; // Your class implementing IDelegate

// Execute a script
var result = await engine.RunAsync(@"
    console.clear
    'Hello from MOGWAI!' ?
    2 3 + ?
", debugMode: false);

if (result.IsError)
{
    Console.WriteLine($"Error: {result}");
}
```

**Note:** This creates a folder structure in `Documents/MOGWAI/` for scripts and data files.

---

## MOGWAI Language Overview

### Stack-Based Programming

MOGWAI uses **Reverse Polish Notation** where operators follow operands:

```mogwai
# Traditional: (2 + 3) * 4
# MOGWAI:
2 3 + 4 *

# Result: 20
```

### Variables

```mogwai
# Store values
42 -> 'answer'
"Hello" -> 'greeting'

# Use variables
answer 2 / ?           # Prints: 21
greeting " World" + ?  # Prints: Hello World
```

### Functions

```mogwai
# Define a function
to 'square' with [n: .number] do
{
    n n *
}

# Use it
5 square ?  # Prints: 25
```

### Lists

```mogwai
# Create a list
(1 2 3 4 5) -> 'numbers'

# Map function
numbers foreach 'n' do { n 2 * } -> 'doubled'

doubled ?  # Prints: (2 4 6 8 10)
```

### Records

```mogwai
# Create a record
[name: "MOGWAI" version: "8.13.0" author: "Stéphane Sibué"] -> 'info'

# Access fields
info->name ?      # Prints: MOGWAI
info->version ?   # Prints: 8.13.0
```

### Conditional Logic

```mogwai
# If-then-else
if (x 10 >) then
{
    "Greater than 10" ?
}
else
{
    "Less or equal to 10" ?
}
```

---

## Integration in Your .NET Application

### 1. Implement IDelegate

Your host application must implement the `IDelegate` interface:

```csharp
using MOGWAI.Engine;
using MOGWAI.Interfaces;
using MOGWAI.Objects;
using System.Net;

public class MyApp : IDelegate
{
    private MogwaiEngine _engine;

    public MyApp()
    {
        _engine = new MogwaiEngine("MyApp");
        _engine.Delegate = this;
    }

    // Called when a script starts
    public async Task ProgramStart(MogwaiEngine engine, string code)
    {
        Console.WriteLine("Script starting...");
        await Task.CompletedTask;
    }

    // Called when a script ends
    public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
    {
        Console.WriteLine($"Script ended: {result}");
        await Task.CompletedTask;
    }

    // MOGWAI's ? or console.printLn function
    public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
    {
        Console.WriteLine(message);
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    // MOGWAI's ?? or console.print function
    public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
    {
        Console.Write(message);
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    // Clear console
    public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
    {
        Console.Clear();
        await Task.CompletedTask;
        return EvalResult.NoError;
    }

    // Input from user
    public async Task<(EvalResult result, string? value)> Prompt(
        MogwaiEngine engine, string message)
    {
        Console.Write(message);
        string? input = Console.ReadLine();
        return (EvalResult.NoError, input);
    }

    // Advanced console methods (minimal implementation)
    public async Task<EvalResult> ConsoleShow(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleHide(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
    {
        Console.SetCursorPosition(x, y);
        return EvalResult.NoError;
    }
    public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        => (EvalResult.NoError, Console.CursorLeft, Console.CursorTop);
    public async Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color)
    {
        if (Enum.TryParse<ConsoleColor>(color, true, out var cc))
            Console.ForegroundColor = cc;
        return EvalResult.NoError;
    }
    public async Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color)
    {
        if (Enum.TryParse<ConsoleColor>(color, true, out var cc))
            Console.BackgroundColor = cc;
        return EvalResult.NoError;
    }
    public async Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
    {
        var k = Console.ReadKey(true);
        return (EvalResult.NoError, (int)k.Key);
    }

    // List custom functions provided by your app
    public string[] HostFunctions(MogwaiEngine engine)
    {
        return new[] { "myCustomFunction" };
    }

    // Execute custom functions
    public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
    {
        if (word == "myCustomFunction")
        {
            Console.WriteLine("Custom function called!");
            return EvalResult.NoError;
        }

        return EvalResult.NoExternalFunction;
    }

    // Runtime messages
    public async Task<EvalResult> MessageReceivedFromRuntime(
        MogwaiEngine engine, string message, MOGObject parameter)
        => EvalResult.NoError;

    // Debug output
    public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
        => EvalResult.NoError;
    public async Task<EvalResult> DebugClear(MogwaiEngine engine)
        => EvalResult.NoError;

    // Engine state
    public async Task<EvalResult> EngineDidPause(MogwaiEngine engine)
        => EvalResult.NoError;
    public async Task<EvalResult> EngineDidResume(MogwaiEngine engine)
        => EvalResult.NoError;

    // STUDIO connection
    public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
        => EvalResult.NoError;
    public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
        => EvalResult.NoError;
    public async Task<EvalResult> SocketServerDidStart(
        MogwaiEngine engine, IPAddress address, int port)
        => EvalResult.NoError;
    public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
        => EvalResult.NoError;
}
```

### 2. Add Custom Functions

Extend MOGWAI with your own functions by manipulating the stack:

```csharp
public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
{
    switch (word)
    {
        case "double":
            // Check stack signature
            var sig = engine.StackSign(1);
            if (sig.Count == 0)
                return EvalResult.Failure(engine, Error.TooFewArgumentsError, word);

            if (sig[0] != typeof(MOGNumber))
                return EvalResult.Failure(engine, Error.BadArgumentTypeError, word);

            // Pop value, double it, push back
            var num = engine.StackPopNumber();
            engine.StackPush(new MOGNumber(num.Value * 2));

            return EvalResult.NoError;

        case "greet":
            // Push a string to the stack
            engine.StackPush(new MOGString("Hello from custom function!"));
            return EvalResult.NoError;
    }

    return EvalResult.NoExternalFunction;
}
```

**In MOGWAI script:**

```mogwai
5 double ?     # Prints: 10
greet ?        # Prints: Hello from custom function!
```

---

## Constructor Options

### Basic Usage (Default)

```csharp
var engine = new MogwaiEngine("MyApp");
```

**Default settings:**

- `keepAlive: false` - Engine resets state between executions
- `useDefaultFolders: true` - Creates `Documents/MOGWAI/` folder structure

**Best for:** Getting started, tutorials, standalone applications

---

### Embedded Applications

```csharp
var engine = new MogwaiEngine("MyApp", useDefaultFolders: false);
```

**Settings:**

- `keepAlive: false` - Clean state each execution
- `useDefaultFolders: false` - No folder creation, use custom paths

**Best for:** WinForms, MAUI, backend services with embedded scripts

---

### CLI / REPL Applications

```csharp
var engine = new MogwaiEngine("MOGWAI CLI", keepAlive: true, useDefaultFolders: true);

// Global variables persist between executions
await engine.RunAsync("42 -> '$X'", debugMode: false);
await engine.RunAsync("$X 2 * ?", debugMode: false);  // Prints: 84
```

**Settings:**

- `keepAlive: true` - Variables persist between calls
- `useDefaultFolders: true` - Use Documents/MOGWAI/ structure

**Best for:** Interactive scripting, REPL, CLI tools

---

### Full Control

```csharp
var engine = new MogwaiEngine(
    name: "MyApp",
    keepAlive: true,         // State persists
    useDefaultFolders: false // Custom file management
);
```

---

## Default Folder Structure

When `useDefaultFolders: true`, MOGWAI creates:

```
Documents/
└── MOGWAI/
    ├── Programs/      ← User scripts (.mog files)
    ├── Files/         ← Data files
    └── Usings/        ← Shared modules
```

Access these paths:

```csharp
string programsPath = engine.ProgramsDirectory;
string filesPath = engine.FilesDirectory;
string usingsPath = engine.UsingsDirectory;
```

---

## Remote Debugging Protocol

MOGWAI ships with a built-in discovery and debug protocol that powers tools like the [VS Code extension](https://marketplace.visualstudio.com/items?itemName=mogwai.mogwai-language) today, and will power **MOGWAI STUDIO** (a visual IDE, currently in early private development) once it's released.

### Enabling Remote Debugging

```csharp
var engine = new MogwaiEngine("MyApp");
engine.Delegate = this;

// Start network server for remote tooling (VS Code, future STUDIO)
await engine.StartNetworkCommunication();

// Keep running while a debugger is connected
while (true)
{
    await Task.Delay(250);
}
```

### MOGWAI CLI Debug Mode

In MOGWAI CLI, type **`studio`** to enable the network connection:

```
MOGWAI > studio
Starting network communication on port 1968...
Waiting for connection...
```

### Discovery Protocol

Tooling uses **UDP broadcast** (port 1968) to discover running MOGWAI instances:

**Tool sends:**

```json
{"Source": "MOGWAI STUDIO", "Function": "WHO IS HERE"}
```

**Runtime responds:**

```json
{
  "Source": "MOGWAI RUNTIME",
  "Function": "I AM HERE",
  "Parameters": ["MyApp", "63542", "8.13.0", "Windows", ...]
}
```

**TCP debug connection** is then established on an auto-assigned port (63000–65000).

### What This Enables

Once connected, a debugger can provide:

- Visual breakpoints
- Step-by-step execution (step over, step into, step out)
- Variable inspection
- Stack visualization
- Expression evaluation

### Network Ports

- **UDP Discovery**: Port 1968 (default, configurable)
- **TCP Debug**: Port 63000-65000 (auto-assigned)

### Custom Configuration

```csharp
// Listen on specific address and port
await engine.StartNetworkCommunication(address: "127.0.0.1", port: 1968);
```

### Security

**Important:** A connected debugger has full script control.
Only enable on trusted networks (localhost, private LAN).

---

## Use Cases

### Is MOGWAI a Good Fit for You?

MOGWAI is a focused tool, not a general-purpose language. It shines when:

- You want **zero operator precedence ambiguity** — the stack is the single source of truth
- You're embedding a scripting runtime in a **.NET** application, including **NativeAOT** builds
- You appreciate the **concatenative programming** model in the tradition of Forth, Factor, PostScript and HP RPL
- You need a **lightweight, extensible runtime** with a clean plugin contract
- You want to offer safe, hot-swappable scripting to your users — update logic without recompiling or redeploying your app

### HTTP API Calls

```mogwai
# GET request
[http.get uri: "https://api.example.com/data"] -> 'response'

if (response->state) then
{
    response->response json.parse -> 'data'
    data->items size "Found {!} items" eval ?
}
```

### A Documented Real-World Case

MOGWAI scripts drive an electronic board test bench in production — see the [full use case](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/use-cases/use-case-01-test-bench.md) and the [behind-the-scenes story](https://coding4phone.com/?p=1990&lang=en).

---

## Advanced Features

### Asynchronous Execution

```csharp
// Run script asynchronously
var result = await engine.RunAsync(script, debugMode: false);

// Multiple scripts concurrently
var task1 = engine.RunAsync(script1, debugMode: false);
var task2 = engine.RunAsync(script2, debugMode: false);
await Task.WhenAll(task1, task2);
```

### Halt Execution

```csharp
// Stop a running script (Ctrl+C handler)
Console.CancelKeyPress += (sender, e) =>
{
    engine.Halt();
    e.Cancel = true;
};
```

### Stack Manipulation

```csharp
// Check what's on the stack
var signature = engine.StackSign(3); // Get types of top 3 items

// Pop values
var number = engine.StackPopNumber();
var text = engine.StackPopString();
var list = engine.StackPopList();
var record = engine.StackPopRecord();

// Push values
engine.StackPush(new MOGNumber(42));
engine.StackPush(new MOGString("Hello"));
engine.StackPush(new MOGBoolean(true));
```

### Error Handling

```csharp
var result = await engine.RunAsync(script, debugMode: false);

if (result.IsError)
{
    Console.WriteLine($"Error: {result.Error.Code}");
    Console.WriteLine($"Message: {result.Error.Message}");
    Console.WriteLine($"Position: {result.StartErrorPosition}-{result.EndErrorPosition}");
}
```

---

## Why MOGWAI?

### Born from Real Needs

Created in 2015 to simulate Bluetooth Low Energy devices for IoT testing. Over 10 years, MOGWAI evolved into a full-featured, general-purpose embeddable scripting engine for .NET applications.

### Inspired by HP Calculators

The syntax is inspired by the legendary HP 28S and HP 48 calculators, bringing the elegance of RPN to modern software development.

### Battle-Tested

- **10+ years of real-world usage** - From prototyping to production environments
- **A documented case** - See how MOGWAI scripts drive an [electronic board test bench in production](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/use-cases/use-case-01-test-bench.md) ([read the story](https://coding4phone.com/?p=1990&lang=en))

### Unique Features

- **Stack-based** with modern conveniences (variables, functions, records)
- **RPN syntax** for clarity and consistency
- **`calc` primitive** for infix-to-RPN conversion when you're not ready to go full RPN
- **Extensible** - easily add domain-specific functions, or build a plugin via `MOGWAI.IPlugin`

---

## Documentation

### Complete Guides

- **[Integration Guide](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI in your .NET application
- **[Language Reference](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/MOGWAI_EN.md)** - Complete MOGWAI language guide
- **[Function Reference](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 304 built-in functions documented
- **[VS Code Extension Guide](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/MOGWAI_VSCODE.md)** - How to use VS Code with the MOGWAI runtime

### Examples

- **[MOGWAI CLI](https://github.com/Sydney680928/mogwai/tree/main/examples/Console)** - Command-line interface and REPL
- **[Avalonia Example](https://github.com/Sydney680928/mogwai/tree/main/examples/Avalonia)** - Cross-platform REPL with debug mode (Windows, Linux, macOS)
- **[WinForms Example](https://github.com/Sydney680928/mogwai/tree/main/examples/WinForms)** - Turtle graphics with MOGWAI
- **[MAUI Example](https://github.com/Sydney680928/mogwai/tree/main/examples/MAUI)** - Cross-platform mobile app
- **[Blazor Example](https://github.com/Sydney680928/mogwai/tree/main/examples/Blazor)** - Try MOGWAI directly in your browser

### Blog Articles

New to MOGWAI? Start with [The Origin Story](https://coding4phone.com/?p=2066&lang=en), [Anatomy of MOGWAI](https://coding4phone.com/?p=2615&lang=en), or see more on [coding4phone.com](https://www.coding4phone.com).

---

## What's Included

- **MOGWAI.dll** - The complete runtime (.NET 9.0+)
- **304 Functions** - Math, strings, lists, I/O, HTTP, dates, and more
- **Type System** - Numbers, strings, booleans, lists, records, data, code
- **Async Support** - Modern task-based execution
- **Thread-Safe** - Built-in concurrency management
- **Remote Debug Protocol** - Powers the VS Code extension today, MOGWAI STUDIO tomorrow

---

## Support & Community

- **GitHub**: [https://github.com/Sydney680928/mogwai](https://github.com/Sydney680928/mogwai)
- **Issues**: [https://github.com/Sydney680928/mogwai/issues](https://github.com/Sydney680928/mogwai/issues)
- **Website**: [MOGWAI](https://www.mogwai.eu.com)
- **Author**: [Stéphane Sibué](https://www.coding4phone.com)

---

## License

**Apache License 2.0**

MOGWAI is free and open source software. See [LICENSE](https://github.com/Sydney680928/mogwai/blob/main/LICENSE) for details.

```
Copyright 2015-2026 Stéphane Sibué

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
```

---

## Get Started Now

```bash
dotnet add package MOGWAI
```

**Happy scripting with MOGWAI!** 🎉

---

*MOGWAI - Where stack-based elegance meets modern .NET power* ✨
