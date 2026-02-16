# MOGWAI Integration Guide

Complete guide for integrating MOGWAI V8 runtime into your .NET applications.

**Version:** 8.0  
**Author:** Stéphane Sibué  
**License:** Apache 2.0  
**Last Updated:** February 2026  

---

## Table of Contents

1. [Installation](#installation)
2. [Quick Start](#quick-start)
3. [Constructor Options](#constructor-options)
4. [IDelegate Interface](#idelegate-interface)
5. [Custom Functions](#custom-functions)
6. [Stack Manipulation](#stack-manipulation)
7. [Error Handling](#error-handling)
8. [MOGWAI STUDIO Integration](#mogwai-studio-integration)
9. [Advanced Features](#advanced-features)
10. [Best Practices](#best-practices)

---

## Installation

### NuGet Package

```bash
dotnet add package MOGWAI
```

### Required Namespaces

```csharp
using MOGWAI.Engine;       // MogwaiEngine class
using MOGWAI.Objects;      // MOGNumber, MOGString, MOGList, etc.
using MOGWAI.Interfaces;   // IDelegate interface
using MOGWAI.Exceptions;   // Exception types (optional)
using System.Net;          // IPAddress for SocketServerDidStart
```

---

## Quick Start

### Minimal Console Application

```csharp
using MOGWAI.Engine;
using MOGWAI.Interfaces;
using MOGWAI.Objects;
using System.Net;

public class ConsoleApp : IDelegate
{
    private MogwaiEngine _engine;

    public ConsoleApp()
    {
        // Create engine with default settings
        _engine = new MogwaiEngine("ConsoleApp");
        _engine.Delegate = this;
    }

    public async Task Run()
    {
        // Execute a MOGWAI script
        var result = await _engine.RunAsync(@"
            'Hello from MOGWAI!' ?
            2 3 + ?
        ", debugMode: false);

        if (result.IsError)
        {
            Console.WriteLine($"Error: {result}");
        }
    }

    // Minimal IDelegate implementation
    public async Task ProgramStart(MogwaiEngine engine, string code)
    {
        await Task.CompletedTask;
    }

    public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
    {
        await Task.CompletedTask;
    }

    public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
    {
        Console.WriteLine(message);
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
    {
        Console.Write(message);
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
    {
        Console.Clear();
        return EvalResult.NoError;
    }

    public async Task<(EvalResult result, string? value)> Prompt(
        MogwaiEngine engine, string message)
    {
        Console.Write(message);
        return (EvalResult.NoError, Console.ReadLine());
    }

    // Advanced console methods (can return NoError if not needed)
    public async Task<EvalResult> ConsoleShow(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleHide(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y) => EvalResult.NoError;
    public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine) 
        => (EvalResult.NoError, 0, 0);
    public async Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color) => EvalResult.NoError;
    public async Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine) 
        => (EvalResult.NoError, 0);

    // Custom functions
    public string[] HostFunctions(MogwaiEngine engine) => Array.Empty<string>();

    public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
    {
        return EvalResult.NoExternalFunction;
    }

    // Runtime messages
    public async Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter)
    {
        return EvalResult.NoError;
    }

    // Debug output
    public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message) => EvalResult.NoError;
    public async Task<EvalResult> DebugClear(MogwaiEngine engine) => EvalResult.NoError;

    // Engine state
    public async Task<EvalResult> EngineDidPause(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> EngineDidResume(MogwaiEngine engine) => EvalResult.NoError;

    // STUDIO connection
    public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port) => EvalResult.NoError;
    public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine) => EvalResult.NoError;
}
```

---

## Constructor Options

### Three Constructor Signatures

```csharp
// 1. Simple - Default settings
public MogwaiEngine(string name)
  → keepAlive: false, useDefaultFolders: true

// 2. Control folders
public MogwaiEngine(string name, bool useDefaultFolders)
  → keepAlive: false

// 3. Full control
public MogwaiEngine(string name, bool keepAlive, bool useDefaultFolders)
```

### Parameter Details

#### `name` (string)
- **Purpose:** Identifies the engine, displayed in MOGWAI STUDIO
- **Example:** "MyApp", "MOGWAI CLI", "WinForms Debug"
- **Required:** Yes

#### `keepAlive` (bool)
- **Purpose:** Controls state persistence between `RunAsync()` calls
- **Default:** `false`
- **When `false`:** Engine resets completely after each execution (variables, functions, stack cleared)
- **When `true`:** State persists (useful for REPL, interactive sessions)

**Example:**
```csharp
var engine = new MogwaiEngine("CLI", keepAlive: true, useDefaultFolders: true);

await engine.RunAsync("42 -> 'x'", debugMode: false);
await engine.RunAsync("x 2 * ?", debugMode: false);  // Prints: 84
// Variable 'x' still exists because keepAlive = true
```

#### `useDefaultFolders` (bool)
- **Purpose:** Creates standard folder structure in user's Documents
- **Default:** `true`
- **When `false`:** No folders created, application manages its own paths
- **When `true`:** Creates `Documents/MOGWAI/Programs/`, `Files/`, `Usings/`

**Folder structure:**
```
Documents/
└── MOGWAI/
    ├── Programs/      ← User scripts (.mog files)
    ├── Files/         ← Data files
    └── Usings/        ← Shared modules/libraries
```

**Access paths:**
```csharp
string programsDir = engine.ProgramsDirectory;
string filesDir = engine.FilesDirectory;
string usingsDir = engine.UsingsDirectory;

// Or set custom paths
engine.ProgramsDirectory = @"C:\MyApp\Scripts";
```

### Usage Scenarios

#### Scenario 1: Quick Start / Tutorial

```csharp
var engine = new MogwaiEngine("MyApp");
```

✅ Perfect for getting started  
✅ Scripts can be placed in `Documents/MOGWAI/Programs/`  
✅ Clean state each execution  

---

#### Scenario 2: Embedded Application (WinForms, MAUI)

```csharp
var engine = new MogwaiEngine("WinFormsApp", useDefaultFolders: false);
```

✅ No folder creation in Documents  
✅ Use embedded resources or custom paths  
✅ Clean state each execution  

**Example - Embedded resources:**
```csharp
var script = GetEmbeddedResource("Scripts.Sample1.mog");
await engine.RunAsync(script, debugMode: false);
```

---

#### Scenario 3: CLI / REPL Application

```csharp
var engine = new MogwaiEngine("MOGWAI CLI", keepAlive: true, useDefaultFolders: true);

while (true)
{
    Console.Write("> ");
    string? line = Console.ReadLine();
    if (line == "exit") break;
    
    await engine.RunAsync(line, debugMode: false);
}
```

✅ Variables persist between commands  
✅ Use standard folders  
✅ Interactive session  

---

#### Scenario 4: Custom Configuration

```csharp
var engine = new MogwaiEngine("MyApp", keepAlive: true, useDefaultFolders: false);

// Set custom directories
engine.ProgramsDirectory = @"C:\MyApp\Scripts";
engine.FilesDirectory = @"C:\MyApp\Data";
engine.UsingsDirectory = @"C:\MyApp\Modules";
```

✅ Persistent state  
✅ Custom paths  
✅ Full control  

---

## IDelegate Interface

The `IDelegate` interface is the bridge between MOGWAI and your application.

### Complete Interface

```csharp
namespace MOGWAI.Interfaces;

public interface IDelegate
{
    // Lifecycle
    Task ProgramStart(MogwaiEngine engine, string code);
    Task ProgramEnd(MogwaiEngine engine, EvalResult result);
    Task<EvalResult> EngineDidPause(MogwaiEngine engine);
    Task<EvalResult> EngineDidResume(MogwaiEngine engine);

    // Console I/O - Basic
    Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message);
    Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message);
    Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine);
    Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message);

    // Console I/O - Advanced
    Task<EvalResult> ConsoleShow(MogwaiEngine engine);
    Task<EvalResult> ConsoleHide(MogwaiEngine engine);
    Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y);
    Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine);
    Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color);
    Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color);
    Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine);

    // Custom Functions
    string[] HostFunctions(MogwaiEngine engine);
    Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word);

    // Runtime Messages
    Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter);

    // Debug Output
    Task<EvalResult> DebugMessage(MogwaiEngine engine, string message);
    Task<EvalResult> DebugClear(MogwaiEngine engine);

    // MOGWAI STUDIO Connection
    Task<EvalResult> StudioDidConnect(MogwaiEngine engine);
    Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine);
    Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port);
    Task<EvalResult> SocketServerDidStop(MogwaiEngine engine);
}
```

### Core Methods

#### Lifecycle Hooks

```csharp
public async Task ProgramStart(MogwaiEngine engine, string code)
{
    // Called before script execution starts
    Console.WriteLine("Script starting...");
    await Task.CompletedTask;
}

public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
{
    // Called after script execution ends
    if (result.IsError)
        Console.WriteLine($"Script failed: {result.Error.Message}");
    else
        Console.WriteLine($"Script completed in {result.Duration.TotalMilliseconds}ms");
    
    await Task.CompletedTask;
}

public async Task<EvalResult> EngineDidPause(MogwaiEngine engine)
{
    // Called when execution is paused (breakpoint, debug)
    Console.WriteLine("Execution paused");
    return EvalResult.NoError;
}

public async Task<EvalResult> EngineDidResume(MogwaiEngine engine)
{
    // Called when execution resumes
    Console.WriteLine("Execution resumed");
    return EvalResult.NoError;
}
```

#### Console I/O

**Basic Console Functions:**

```csharp
public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
{
    // MOGWAI '?' or console.printLn
    Console.WriteLine(message);
    return EvalResult.NoError;
}

public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
{
    // MOGWAI '??' or console.print
    Console.Write(message);
    return EvalResult.NoError;
}

public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
{
    // MOGWAI console.clear
    Console.Clear();
    return EvalResult.NoError;
}

public async Task<(EvalResult result, string? value)> Prompt(
    MogwaiEngine engine, string message)
{
    // MOGWAI console.prompt
    Console.Write(message);
    string? input = Console.ReadLine();
    return (EvalResult.NoError, input);
}
```

**Advanced Console Functions:**

```csharp
public async Task<EvalResult> ConsoleShow(MogwaiEngine engine)
{
    // MOGWAI console.show - Show console window
    // Implementation depends on platform (Windows native calls, etc.)
    return EvalResult.NoError;
}

public async Task<EvalResult> ConsoleHide(MogwaiEngine engine)
{
    // MOGWAI console.hide - Hide console window
    // Implementation depends on platform
    return EvalResult.NoError;
}

public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
{
    // MOGWAI console.locate - Set cursor position
    Console.SetCursorPosition(x, y);
    return EvalResult.NoError;
}

public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(
    MogwaiEngine engine)
{
    // MOGWAI console.getCursorPosition
    int x = Console.CursorLeft;
    int y = Console.CursorTop;
    return (EvalResult.NoError, x, y);
}

public async Task<EvalResult> ConsoleSetForegroundColor(
    MogwaiEngine engine, string color)
{
    // MOGWAI console.setForegroundColor
    if (Enum.TryParse<ConsoleColor>(color, true, out var consoleColor))
    {
        Console.ForegroundColor = consoleColor;
        return EvalResult.NoError;
    }
    return EvalResult.Failure(engine, Error.BadArgumentValueError, "ConsoleSetForegroundColor");
}

public async Task<EvalResult> ConsoleSetBackgroundColor(
    MogwaiEngine engine, string color)
{
    // MOGWAI console.setBackgroundColor
    if (Enum.TryParse<ConsoleColor>(color, true, out var consoleColor))
    {
        Console.BackgroundColor = consoleColor;
        return EvalResult.NoError;
    }
    return EvalResult.Failure(engine, Error.BadArgumentValueError, "ConsoleSetBackgroundColor");
}

public async Task<(EvalResult result, int key)> ConsoleGetInputKey(
    MogwaiEngine engine)
{
    // MOGWAI console.getInputKey - Read single key press
    var keyInfo = Console.ReadKey(intercept: true);
    return (EvalResult.NoError, (int)keyInfo.Key);
}
```

**In MOGWAI:**
```mogwai
# Basic I/O
"Enter your name: " console.prompt -> 'name'
"Hello {name}!" eval ?

# Advanced console control
10 20 console.locate
"Red" console.setForegroundColor
"At position 10,20 in red" ?

# Read single key
console.getInputKey -> 'key'
"You pressed key: {key}" eval ?
```

---

## Custom Functions

### Declaring Custom Functions

```csharp
public string[] HostFunctions(MogwaiEngine engine)
{
    // Return list of custom function names
    return new[] { "double", "greet", "turtle.move", "turtle.turn" };
}
```

### Executing Custom Functions

```csharp
public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
{
    switch (word)
    {
        case "double":
            return ExecuteDouble(engine);
        
        case "greet":
            return ExecuteGreet(engine);
        
        case "turtle.move":
            return await ExecuteTurtleMove(engine);
        
        case "turtle.turn":
            return await ExecuteTurtleTurn(engine);
        
        default:
            return EvalResult.NoExternalFunction;
    }
}
```

### Function Implementation Pattern

#### 1. Validate Stack Signature

```csharp
private EvalResult ExecuteDouble(MogwaiEngine engine)
{
    // Check stack has at least 1 item
    var signature = engine.StackSign(1);
    if (signature.Count == 0)
        return EvalResult.Failure(engine, Error.TooFewArgumentsError, "double");
    
    // Check type is number
    if (signature[0] != typeof(MOGNumber))
        return EvalResult.Failure(engine, Error.BadArgumentTypeError, "double");
    
    // Pop, process, push
    var num = engine.StackPopNumber();
    engine.StackPush(new MOGNumber(num.Value * 2));
    
    return EvalResult.NoError;
}
```

#### 2. Multi-Parameter Functions

```csharp
private async Task<EvalResult> ExecuteTurtleMove(MogwaiEngine engine)
{
    // Signature: distance (number)
    var signature = engine.StackSign(1);
    if (signature.Count == 0)
        return EvalResult.Failure(engine, Error.TooFewArgumentsError, "turtle.move");
    
    if (signature[0] != typeof(MOGNumber))
        return EvalResult.Failure(engine, Error.BadArgumentTypeError, "turtle.move");
    
    // Pop parameter
    MOGNumber distance = engine.StackPopNumber();
    
    // Execute (with thread safety for UI)
    await Task.Run(() =>
    {
        Invoke(() =>
        {
            // Move turtle on UI thread
            MoveTurtle(distance.Value);
        });
    });
    
    return EvalResult.NoError;
}
```

#### 3. Functions with Return Values

```csharp
private EvalResult ExecuteGreet(MogwaiEngine engine)
{
    // Push result to stack
    engine.StackPush(new MOGString("Hello from custom function!"));
    return EvalResult.NoError;
}
```

**In MOGWAI:**
```mogwai
greet ?  # Prints: Hello from custom function!
```

---

## Stack Manipulation

### Stack Signature

```csharp
// Get types of top N items
var signature = engine.StackSign(3);

// signature is List<Type>
// Example: [typeof(MOGNumber), typeof(MOGString), typeof(MOGBoolean)]
```

### Pop Operations

```csharp
// Pop specific types
MOGNumber number = engine.StackPopNumber();
MOGString text = engine.StackPopString();
MOGBoolean bool = engine.StackPopBoolean();
MOGList list = engine.StackPopList();
MOGRecord record = engine.StackPopRecord();
MOGCode code = engine.StackPopCode();
MOGData data = engine.StackPopData();

// Generic pop
MOGObject obj = engine.StackPop();
```

### Push Operations

```csharp
// Push values to stack
engine.StackPush(new MOGNumber(42));
engine.StackPush(new MOGString("Hello"));
engine.StackPush(new MOGBoolean(true));
engine.StackPush(new MOGList(new[] { 
    new MOGNumber(1), 
    new MOGNumber(2) 
}));

// Create record
var record = new MOGRecord(engine);
record.Items["name"] = new MOGString("MOGWAI");
record.Items["version"] = new MOGNumber(8.0);
engine.StackPush(record);
```

### Stack Properties

```csharp
// Get stack size
int size = engine.StackSize;

// Check if stack is empty
if (size == 0)
{
    // Handle empty stack
}
```

---

## Error Handling

### EvalResult

```csharp
var result = await engine.RunAsync(script, debugMode: false);

if (result.IsError)
{
    Console.WriteLine($"Error Code: {result.Error.Code}");
    Console.WriteLine($"Message: {result.Error.Message}");
    Console.WriteLine($"Position: {result.StartErrorPosition}-{result.EndErrorPosition}");
}
else
{
    Console.WriteLine($"Success! Duration: {result.Duration.TotalMilliseconds}ms");
}
```

### Standard Errors

```csharp
// Common errors
Error.TooFewArgumentsError       // Stack doesn't have enough items
Error.BadArgumentTypeError       // Wrong type on stack
Error.DivideByZeroError         // Division by zero
Error.VariableNotFoundError     // Variable doesn't exist
Error.FunctionNotFoundError     // Function doesn't exist

// Return error from custom function
return EvalResult.Failure(engine, Error.BadArgumentTypeError, "myFunction");
```

### Custom Errors

```csharp
// Register custom error
var myError = engine.RegisterError(
    this,                           // IDelegate
    "INVALID_OPERATION",           // Error code
    "The requested operation is not valid in this context"
);

// Use custom error
return EvalResult.Failure(engine, myError, "myFunction");
```

---

## MOGWAI STUDIO Integration

### Enabling STUDIO Connection

```csharp
var engine = new MogwaiEngine("MyApp");
engine.Delegate = this;

// Start network server
await engine.StartNetworkCommunication();

// Keep running
while (true)
{
    await Task.Delay(250);
}
```

### Network Configuration

```csharp
// Default configuration (all interfaces, port 1968)
await engine.StartNetworkCommunication();

// Custom configuration
await engine.StartNetworkCommunication(
    address: "127.0.0.1",  // Localhost only
    port: 1968              // UDP discovery port
);
```

### Discovery Protocol

**STUDIO broadcasts** (UDP port 1968):
```json
{"Source": "MOGWAI STUDIO", "Function": "WHO IS HERE"}
```

**Runtime responds:**
```json
{
  "Source": "MOGWAI RUNTIME",
  "Function": "I AM HERE",
  "Parameters": [
    "MyApp",          // Engine name
    "63542",          // TCP port (auto-assigned 63000-65000)
    "8.0.0",          // MOGWAI version
    "Windows",        // Platform
    "x64",            // Architecture
    ".NET 9.0",       // Framework
    "..."             // Other info
  ]
}
```

**TCP connection** established on the port specified in response.

### MOGWAI STUDIO Callbacks

When STUDIO connects to your runtime, these callbacks are invoked:

```csharp
public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
{
    Console.WriteLine("MOGWAI STUDIO connected");
    StatusLabel.Text = "Connected to STUDIO";
    return EvalResult.NoError;
}

public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
{
    Console.WriteLine("MOGWAI STUDIO disconnected");
    StatusLabel.Text = "STUDIO disconnected";
    return EvalResult.NoError;
}

public async Task<EvalResult> SocketServerDidStart(
    MogwaiEngine engine, IPAddress address, int port)
{
    Console.WriteLine($"Socket server started on {address}:{port}");
    return EvalResult.NoError;
}

public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
{
    Console.WriteLine("Socket server stopped");
    return EvalResult.NoError;
}
```

### Runtime Messages and Debug Output

```csharp
public async Task<EvalResult> MessageReceivedFromRuntime(
    MogwaiEngine engine, string message, MOGObject parameter)
{
    // MOGWAI can send messages to the host application
    Console.WriteLine($"Runtime message: {message}");
    // parameter contains additional data
    return EvalResult.NoError;
}

public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
{
    // Debug output from MOGWAI scripts (console.debug)
    Console.WriteLine($"[DEBUG] {message}");
    return EvalResult.NoError;
}

public async Task<EvalResult> DebugClear(MogwaiEngine engine)
{
    // Clear debug output
    DebugOutputTextBox?.Clear();
    return EvalResult.NoError;
}
```

**In MOGWAI:**
```mogwai
# Send debug message
"Debug information here" console.debug

# Send message to host
"myMessage" "data" runtime.sendMessage
```

### STUDIO Features

Once connected, STUDIO provides:
- ✅ Set breakpoints by line number
- ✅ Step over / step into / step out
- ✅ View stack state
- ✅ Inspect variables
- ✅ Evaluate expressions
- ✅ Continue / pause execution

### Security Considerations

⚠️ **Important:** STUDIO connection allows full script control.

**Best practices:**
- Only enable on trusted networks (localhost, private LAN)
- Disable in production builds
- Add firewall rules if exposing to network

```csharp
#if DEBUG
    await engine.StartNetworkCommunication(address: "127.0.0.1");
#endif
```

### Firewall Configuration

Allow incoming connections:
- **UDP port 1968** (discovery)
- **TCP ports 63000-65000** (debug session)

---

## Advanced Features

### Halting Execution

```csharp
// Emergency stop (Ctrl+C handler)
Console.CancelKeyPress += (sender, e) =>
{
    engine.Halt();
    e.Cancel = true;
};
```

### Runtime Banner

```csharp
// Get MOGWAI version banner
string banner = MogwaiEngine.RuntimePrompt;
Console.WriteLine(banner);

// Output:
// MOGWAI version 8.0.0
// (c) Stéphane SIBUE 2015-2026
```

### Parse Without Execution

```csharp
// Parse code to check syntax
var objects = engine.Parse("2 3 + ?");

// objects is List<MOGObject>
foreach (var obj in objects)
{
    Console.WriteLine(obj.GetType().Name);
}
```

### Async Execution

```csharp
// Start script in background
var task = engine.RunAsync(script, debugMode: false);

// Do other work
await DoSomethingElse();

// Wait for completion
var result = await task;
```

### Multiple Concurrent Scripts

```csharp
// Run multiple scripts concurrently
var task1 = engine.RunAsync(script1, debugMode: false);
var task2 = engine.RunAsync(script2, debugMode: false);
var task3 = engine.RunAsync(script3, debugMode: false);

// Wait for all
await Task.WhenAll(task1, task2, task3);
```

---

## Best Practices

### Thread Safety

**UI Updates:** Always invoke on UI thread when updating UI from MOGWAI:

```csharp
private async Task<EvalResult> ExecuteTurtleMove(MogwaiEngine engine)
{
    var distance = engine.StackPopNumber();
    
    // WinForms
    Invoke(() =>
    {
        MoveTurtle(distance.Value);
    });
    
    // WPF
    Dispatcher.Invoke(() =>
    {
        MoveTurtle(distance.Value);
    });
    
    // MAUI
    MainThread.BeginInvokeOnMainThread(() =>
    {
        MoveTurtle(distance.Value);
    });
    
    return EvalResult.NoError;
}
```

### Error Handling

**Always check EvalResult:**

```csharp
var result = await engine.RunAsync(script, debugMode: false);

if (result.IsError)
{
    // Log error
    Logger.Error($"MOGWAI Error: {result.Error.Code}");
    
    // Show to user
    MessageBox.Show($"Script error: {result.Error.Message}");
    
    // Don't continue
    return;
}

// Continue with success path
```

### Resource Management

**Dispose properly:**

```csharp
public class MyApp : IDisposable
{
    private MogwaiEngine _engine;

    public MyApp()
    {
        _engine = new MogwaiEngine("MyApp");
        _engine.Delegate = this;
    }

    public void Dispose()
    {
        // Clean up MOGWAI resources
        _engine?.Halt();
        // Additional cleanup
    }
}
```

### Script Loading

**Embedded resources:**

```csharp
public string GetEmbeddedScript(string name)
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = $"MyApp.Scripts.{name}";
    
    using var stream = assembly.GetManifestResourceStream(resourceName);
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}

// Usage
var script = GetEmbeddedScript("Sample1.mog");
await engine.RunAsync(script, debugMode: false);
```

---

## Troubleshooting

### Common Issues

#### "Too Few Arguments" Error

**Problem:** Stack doesn't have enough items for function.

**Solution:** Check stack signature before popping:

```csharp
var signature = engine.StackSign(2); // Need 2 items
if (signature.Count < 2)
    return EvalResult.Failure(engine, Error.TooFewArgumentsError, "myFunction");
```

#### Cross-Thread Exceptions

**Problem:** Updating UI from MOGWAI thread.

**Solution:** Use Invoke/Dispatcher:

```csharp
Invoke(() => UpdateUI());  // WinForms
Dispatcher.Invoke(() => UpdateUI());  // WPF
MainThread.BeginInvokeOnMainThread(() => UpdateUI());  // MAUI
```

#### Variables Not Persisting

**Problem:** Variables lost between executions.

**Solution:** Use `keepAlive: true`:

```csharp
var engine = new MogwaiEngine("MyApp", keepAlive: true, useDefaultFolders: false);
```

---

## Complete Example: WinForms Application

```csharp
using MOGWAI.Engine;
using MOGWAI.Interfaces;
using MOGWAI.Objects;
using System.Net;

public partial class FormMain : Form, IDelegate
{
    private MogwaiEngine _engine;

    public FormMain()
    {
        InitializeComponent();
        
        // Create engine (no default folders for embedded app)
        _engine = new MogwaiEngine("WinForms App", useDefaultFolders: false);
        _engine.Delegate = this;
    }

    private async void RunButton_Click(object sender, EventArgs e)
    {
        // Execute code from TextBox
        var result = await _engine.RunAsync(CodeTextBox.Text, debugMode: false);
        
        if (result.IsError)
        {
            MessageBox.Show(
                $"Error: {result.Error.Message}\nPosition: {result.StartErrorPosition}", 
                "MOGWAI Error", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error
            );
        }
    }

    private void EnableStudioButton_Click(object sender, EventArgs e)
    {
        // Start STUDIO server in background
        _ = Task.Run(async () => 
        {
            await _engine.StartNetworkCommunication(address: "127.0.0.1");
        });
        
        StatusLabel.Text = "Waiting for STUDIO connection...";
    }

    // IDelegate implementation
    public async Task ProgramStart(MogwaiEngine engine, string code)
    {
        Invoke(() => StatusLabel.Text = "Running...");
        await Task.CompletedTask;
    }

    public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
    {
        Invoke(() => StatusLabel.Text = result.IsError ? "Error" : "Completed");
        await Task.CompletedTask;
    }

    public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
    {
        Invoke(() => OutputTextBox.AppendText(message + "\r\n"));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
    {
        Invoke(() => OutputTextBox.AppendText(message));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
    {
        Invoke(() => OutputTextBox.Clear());
        return EvalResult.NoError;
    }

    public async Task<(EvalResult result, string? value)> Prompt(
        MogwaiEngine engine, string message)
    {
        string? result = null;
        
        Invoke(() =>
        {
            using var inputDialog = new InputDialog(message);
            if (inputDialog.ShowDialog() == DialogResult.OK)
                result = inputDialog.InputValue;
        });
        
        return (EvalResult.NoError, result);
    }

    // Advanced console (not applicable for WinForms, return NoError)
    public async Task<EvalResult> ConsoleShow(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleHide(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y) => EvalResult.NoError;
    public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        => (EvalResult.NoError, 0, 0);
    public async Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color) => EvalResult.NoError;
    public async Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        => (EvalResult.NoError, 0);

    public string[] HostFunctions(MogwaiEngine engine)
    {
        return new[] { "turtle.move", "turtle.turn", "turtle.color" };
    }

    public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
    {
        switch (word)
        {
            case "turtle.move":
                return ExecuteTurtleMove(engine);
            case "turtle.turn":
                return ExecuteTurtleTurn(engine);
            case "turtle.color":
                return ExecuteTurtleColor(engine);
        }
        
        return EvalResult.NoExternalFunction;
    }

    public async Task<EvalResult> MessageReceivedFromRuntime(
        MogwaiEngine engine, string message, MOGObject parameter)
    {
        Invoke(() => OutputTextBox.AppendText($"[MSG] {message}\r\n"));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
    {
        Invoke(() => DebugTextBox?.AppendText($"{message}\r\n"));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> DebugClear(MogwaiEngine engine)
    {
        Invoke(() => DebugTextBox?.Clear());
        return EvalResult.NoError;
    }

    public async Task<EvalResult> EngineDidPause(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "Paused");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> EngineDidResume(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "Running");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "STUDIO Connected");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "STUDIO Disconnected");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> SocketServerDidStart(
        MogwaiEngine engine, IPAddress address, int port)
    {
        Invoke(() => StatusLabel.Text = $"Server: {address}:{port}");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "Server stopped");
        return EvalResult.NoError;
    }

    private EvalResult ExecuteTurtleMove(MogwaiEngine engine)
    {
        var sig = engine.StackSign(1);
        if (sig.Count == 0 || sig[0] != typeof(MOGNumber))
            return EvalResult.Failure(engine, Error.BadArgumentTypeError, "turtle.move");
        
        var distance = engine.StackPopNumber();
        
        Invoke(() =>
        {
            // Move turtle on UI
            MoveTurtle((int)distance.Value);
            TurtleCanvas.Refresh();
        });
        
        return EvalResult.NoError;
    }

    // ... Implement ExecuteTurtleTurn, ExecuteTurtleColor similarly
}
```

---

## Summary

### Key Points

1. ✅ Use `MogwaiEngine` class from `MOGWAI.Engine` namespace
2. ✅ Implement `IDelegate` interface for integration
3. ✅ Choose constructor based on use case (embedded vs CLI)
4. ✅ Always check `EvalResult.IsError`
5. ✅ Use thread safety for UI updates
6. ✅ Enable STUDIO for debugging with `StartNetworkCommunication()`

### Next Steps

- Read [MOGWAI Language Guide](MOGWAI_EN.md) for language syntax
- Read [Function Reference](MOGWAI_FUNCTIONS_EN.md) for built-in functions
- Explore [Examples](../examples/) for real-world integrations

---

**Happy integrating!** 🚀

*For questions or issues, visit: [https://github.com/[username]/mogwai/issues](https://github.com/[username]/mogwai/issues)*
