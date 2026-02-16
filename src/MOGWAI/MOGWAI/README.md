# MOGWAI - Modern RPN Scripting Language

A powerful stack-based RPN (Reverse Polish Notation) scripting language for industrial IoT automation, embedded systems, and creative programming.

**10 years of development** | **200+ built-in functions** | **Industrial-grade** | **Open Source (Apache 2.0)**

---

## 🚀 Quick Start

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

## ✨ Key Features

- ✅ **Stack-Based RPN Syntax** - Inspired by HP calculators (HP 28S, HP 48)
- ✅ **200+ Built-in Functions** - Math, strings, lists, dates, HTTP, file I/O, and more
- ✅ **Async/Await Support** - Modern asynchronous execution
- ✅ **IoT & BLE Functions** - Bluetooth Low Energy, serial communication
- ✅ **Industrial Automation** - Designed for embedded systems and real-time control
- ✅ **Extensible** - Add your own custom functions easily
- ✅ **Cross-Platform** - Runs on Windows, Linux, macOS (.NET 9.0+)
- ✅ **Thread-Safe** - Built-in task management
- ✅ **MOGWAI STUDIO Integration** - Visual debugging with breakpoints

---

## 📚 MOGWAI Language Overview

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
[name: "MOGWAI", version: 8.0, author: "Stéphane Sibué"] -> 'info'

# Access fields
info->name ?      # Prints: MOGWAI
info->version ?   # Prints: 8.0
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

## 🔌 Integration in Your .NET Application

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

## 🎯 Constructor Options

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

// Variables persist between executions
await engine.RunAsync("42 -> 'x'", debugMode: false);
await engine.RunAsync("x 2 * ?", debugMode: false);  // Prints: 84
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

## 📁 Default Folder Structure

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

## 🐛 MOGWAI STUDIO Integration

### Enabling Remote Debugging

Connect your runtime to MOGWAI STUDIO for visual debugging:

```csharp
var engine = new MogwaiEngine("MyApp");
engine.Delegate = this;

// Start network server for STUDIO
await engine.StartNetworkCommunication();

// Keep running while STUDIO is connected
while (true)
{
    await Task.Delay(250);
}
```

### MOGWAI CLI Studio Mode

In MOGWAI CLI, type **`studio`** to enable STUDIO connection:

```
MOGWAI > studio
Starting network communication on port 1968...
Waiting for MOGWAI STUDIO connection...
```

### Discovery Protocol

**STUDIO uses UDP broadcast** (port 1968) to discover runtimes:

**STUDIO sends:**

```json
{"Source": "MOGWAI STUDIO", "Function": "WHO IS HERE"}
```

**Runtime responds:**

```json
{
  "Source": "MOGWAI RUNTIME",
  "Function": "I AM HERE",
  "Parameters": ["MyApp", "63542", "8.0.0", "Windows", ...]
}
```

**TCP debug connection** established on auto-assigned port (63000-65000).

### Features in STUDIO

Once connected, STUDIO provides:

- ✅ Visual breakpoints
- ✅ Step-by-step execution (step over, step into, step out)
- ✅ Variable inspection
- ✅ Stack visualization
- ✅ Expression evaluation

### Network Ports

- **UDP Discovery**: Port 1968 (default, configurable)
- **TCP Debug**: Port 63000-65000 (auto-assigned)

### Custom Configuration

```csharp
// Listen on specific address and port
await engine.StartNetworkCommunication(address: "127.0.0.1", port: 1968);
```

### Security

**Important:** The STUDIO connection allows full script control.
Only enable on trusted networks (localhost, private LAN).

---

## 🎯 Use Cases

### Industrial IoT Automation

```mogwai
# Read sensor via BLE
"AA:BB:CC:DD:EE:FF" ble.connect -> 'device'
device "temperature" ble.read -> 'temp'

# Control based on value
if (temp 25 >) then
{
    "fan" gpio.on
    "Temperature too high: {temp}°C" eval ?
}
```

### Data Processing

```mogwai
# Load and process CSV
"data.csv" file.read csv.parse -> 'data'

# Filter and transform
data 
    [ where: {row row->age 18 >=} ]
    [ select: {row row->name} ]
    -> 'adults'

adults ?
```

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

---

## 🛠️ Advanced Features

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

## 🌟 Why MOGWAI?

### Born from Real Needs

MOGWAI was created to simulate Bluetooth Low Energy devices for IoT testing. Over 10 years, it evolved into a full-featured scripting language used in industrial automation and embedded systems.

### Inspired by HP Calculators

The syntax is inspired by the legendary HP 28S and HP 48 calculators, bringing the elegance of RPN to modern software development.

### Battle-Tested

- ✅ **3 years in production** in industrial environments
- ✅ **Real-world IoT applications** (astronomical clocks, public lighting)
- ✅ **Thousands of scripts** executed in field deployments

### Unique Features

- **Stack-based** with modern conveniences (variables, functions, records)
- **RPN syntax** for clarity and consistency
- **IoT-focused** with BLE, HTTP, serial communication built-in
- **Extensible** - easily add domain-specific functions

---

## 📖 Documentation

### Complete Guides

- **[Integration Guide](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI in your .NET application
- **[Language Reference](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/MOGWAI_EN.md)** - Complete MOGWAI language guide
- **[Function Reference](https://github.com/Sydney680928/mogwai/blob/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 200+ built-in functions documented

### Examples

- **[WinForms Example](https://github.com/[username]/mogwai/tree/main/examples/WinFormsMogwai)** - Turtle graphics and more with MOGWAI
- **[Console Example](https://github.com/[username]/mogwai/tree/main/examples/MOGWAI_CLI)** - Console integration (MOGWAI CLI source code)
- **[IoT Example](https://github.com/[username]/mogwai/tree/main/examples/MOGWAI_RUNTIME)** - .NET MAUI program example

---

## 📦 What's Included

- ✅ **MOGWAI.dll** - The complete runtime (.NET 9.0+)
- ✅ **200+ Functions** - Math, strings, lists, I/O, HTTP, BLE, dates, and more
- ✅ **Type System** - Numbers, strings, booleans, lists, records, data, code
- ✅ **Async Support** - Modern task-based execution
- ✅ **Thread-Safe** - Built-in concurrency management
- ✅ **STUDIO Protocol** - Network debugging support

---

## 🤝 Support & Community

- **GitHub**: [https://github.com/Sydney680928/mogwai](https://github.com/Sydney680928/mogwai)
- **Issues**: [https://github.com/Sydney680928/mogwai/issues](https://github.com/Sydney680928/mogwai/issues)
- **Website**: [MOGWAI](https://www.mogwai.eu.com)
- **Author**: [Stéphane Sibué](https://www.coding4phone.com)

---

## 📄 License

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

## 🚀 Get Started Now

```bash
dotnet add package MOGWAI
```

**Happy scripting with MOGWAI!** 🎉

---

*MOGWAI - Where stack-based elegance meets modern .NET power* ✨
