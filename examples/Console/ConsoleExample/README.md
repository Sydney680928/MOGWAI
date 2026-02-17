# MOGWAI CLI - Console Example

A command-line interface and REPL (Read-Eval-Print-Loop) for interactive MOGWAI scripting.

<!-- 
═══════════════════════════════════════════════════════════════════════
📸 SCREENSHOT PLACEHOLDER
═══════════════════════════════════════════════════════════════════════
Replace with: Screenshot of MOGWAI CLI in action showing REPL session
File: /images/examples/cli-screenshot.png
═══════════════════════════════════════════════════════════════════════
-->

---

## Features

- **Interactive REPL** - Execute MOGWAI commands interactively
- **Script Execution** - Load and run `.mog` files
- **STUDIO Integration** - Connect to MOGWAI STUDIO for debugging
- **Persistent Variables** - Variables persist between commands
- **Default Folders** - Uses `Documents/MOGWAI/` for scripts and files
- ⌨**Command History** - Navigate previous commands (Up/Down arrows)
- **Ctrl+C Handling** - Gracefully halt script execution

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- MOGWAI package (automatically restored from NuGet)

### Build and Run

```bash
# Navigate to the CLI directory
cd examples/MOGWAI_CLI

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

---

## 💻 Usage

### Interactive Mode (REPL)

```
MOGWAI version 8.0.0
(c) Stéphane SIBUE 2015-2026

MOGWAI > 2 3 + ?
5

MOGWAI > "Hello from MOGWAI!" ?
Hello from MOGWAI!

MOGWAI > 42 -> '$answer'
MOGWAI > answer 2 / ?
21
```

### Execute Script File

```
MOGWAI > (! path.programs "myScript.mog") path.make run
```

### MOGWAI STUDIO Connection

Type `studio` to enable remote debugging with MOGWAI STUDIO:

```
MOGWAI > studio
Starting network communication on port 1968...
Waiting for MOGWAI STUDIO connection...
```

Once connected, you can:
- Set breakpoints in STUDIO
- Step through code execution
- Inspect variables and stack
- Evaluate expressions

### Built-in Commands

- `bye` - Exit the CLI
- `studio` - Enable MOGWAI STUDIO connection
- `mogwai.reset` - Reset the engine state
- `vars ?d` - List all variables
- `funcs ?d` - List all functions

---

## 📝 Example Scripts

### Basic Math

```mogwai
# Calculate factorial
to 'factorial' with [n: .number] do
{
    if (n 1 <=) then
    {
        1
    }
    else
    {
        n n 1 - factorial *
    }
}

5 factorial ?  # Prints: 120
```

### List Processing

```mogwai
# Create a list of squares
() -> 'result'
(1 2 3 4 5) foreach 'n' do { result n n * + -> 'result' }
result ?  # Prints: (1 4 9 16 25)
```

### File I/O

```mogwai
# Write to file
"Hello World" "output.txt" file.data.write

# Read from file
"output.txt" file.data.read ?
```

---

## Key Implementation Details

### Engine Configuration

```csharp
var engine = new MogwaiEngine(
    name: "MOGWAI CLI",
    keepAlive: true,           // Variables persist between commands
    useDefaultFolders: true    // Use Documents/MOGWAI/ structure
);
```

**Why `keepAlive: true`?**
- Variables and functions persist across REPL commands
- Essential for interactive sessions
- Allows building complex programs step-by-step

**Why `useDefaultFolders: true`?**
- Creates standard folder structure in Documents
- Users can save scripts in `Documents/MOGWAI/Programs/`
- Data files go to `Documents/MOGWAI/Files/`

### IDelegate Implementation

The CLI implements `IDelegate` to handle console I/O:

```csharp
public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
{
    Console.WriteLine(message);
    return EvalResult.NoError;
}

public async Task<(EvalResult result, string? value)> Prompt(
    MogwaiEngine engine, string message)
{
    Console.Write(message);
    return (EvalResult.NoError, Console.ReadLine());
}
```

### Ctrl+C Handler

```csharp
Console.CancelKeyPress += (sender, e) =>
{
    engine.Halt();
    e.Cancel = true;
};
```

---

## Default Folder Structure

When you first run MOGWAI CLI, it creates:

```
C:\Users\[YourName]\Documents\MOGWAI\
├── Programs\      ← Your MOGWAI scripts (.mog files)
├── Files\         ← Data files
└── Usings\        ← Shared modules/libraries
```

You can access these paths in MOGWAI:

```mogwai
path.programs ?    # Prints: C:\Users\...\Documents\MOGWAI\Programs
path.files ?       # Prints: C:\Users\...\Documents\MOGWAI\Files
path.usings ?      # Prints: C:\Users\...\Documents\MOGWAI\Usings
```

---

## Debugging with MOGWAI STUDIO

### Enable STUDIO Connection

1. Start MOGWAI CLI
2. Type `studio` and press Enter
3. Launch MOGWAI STUDIO
4. STUDIO will auto-discover the CLI runtime
5. Set breakpoints and start debugging

### Network Configuration

- **UDP Discovery:** Port 1968
- **TCP Debug:** Auto-assigned (63000-65000)
- **Address:** Listens on all interfaces (0.0.0.0)

For localhost-only debugging:

```csharp
await engine.StartNetworkCommunication(address: "127.0.0.1");
```

---

## Learning MOGWAI

### Try These Examples

**1. Variables and Math:**
```mogwai
MOGWAI > 10 -> '$x'
MOGWAI > 20 -> '$y'
MOGWAI > $x $y + ?
30
```

**2. Functions:**
```mogwai
MOGWAI > to 'double' with [n: .number] do { n 2 * }
MOGWAI > 5 double ?
10
```

**3. Lists:**
```mogwai
MOGWAI > (1 2 3) -> '$numbers'
MOGWAI > () -> '$result'
MOGWAI > numbers foreach 'n' do { result n 10 * + -> 'result' }
MOGWAI > result ?
(10 20 30)
```

**4. Records:**
```mogwai
MOGWAI > [name: "John", age: 30] -> 'person'
MOGWAI > person->name ?
John
MOGWAI > person->age ?
30
```

**5. Control Flow:**
```mogwai
MOGWAI > 15 -> 'x'
MOGWAI > if (x 10 >) then { "Greater!" ? } else { "Smaller!" ? }
Greater!
```

---

## Documentation

- **[MOGWAI Language Guide](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_EN.md)** - Complete language reference
- **[Function Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 200+ built-in functions
- **[Integration Guide](https://github.com/Sydney680928/mogwai/tree/main/docs/EN//MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI

---

## Related Examples

- **[WinForms Example](https://github.com/Sydney680928/mogwai/tree/main/examples/WinForms/WinFormsExample/)** - Turtle graphics with MOGWAI
  - **[MAUI Example](https://github.com/Sydney680928/mogwai/tree/main/examples/MAUI/MauiExample)** - Cross-platform mobile app

---

## License

Apache License 2.0

See [LICENSE](../../LICENSE) for details.

---

## Contributing

Found a bug or want to improve the CLI?

- **Report Issues:** [GitHub Issues](https://github.com/Sydney680928/mogwai/issues)
- **Pull Requests:** Contributions welcome!

---

**Happy scripting with MOGWAI CLI!** 🎉

*For more information, visit [mogwai.eu.com](https://www.mogwai.eu.com)*
