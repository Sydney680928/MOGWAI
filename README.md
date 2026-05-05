<img src="./images/img01.png" title="" alt="MOGWAI" data-align="center">

# [MOGWAI](https://www.mogwai.eu.com) - A powerful stack-based RPN scripting engine for .NET

![GitHub Stars](https://img.shields.io/github/stars/Sydney680928/mogwai?style=social)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/MOGWAI.svg)](https://www.nuget.org/packages/MOGWAI/)
[![Download CLI](https://img.shields.io/github/v/release/Sydney680928/mogwai?label=CLI%20Download&logo=github&color=brightgreen)](https://github.com/Sydney680928/mogwai/releases/latest)
[![VS Code Extension](https://img.shields.io/badge/VS%20Code-Extension-007ACC?logo=visualstudiocode)](https://marketplace.visualstudio.com/items?itemName=mogwai.mogwai-language)

**Embeddable, extensible, production-ready.** **From IoT automation to desktop scripting, one elegant RPN runtime for your .NET apps.**

---

## What is MOGWAI?

MOGWAI is a modern implementation of RPN (Reverse Polish Notation) for the .NET ecosystem. Inspired by the legendary HP calculators (HP 28S, HP 48), it brings the elegance and power of stack-based, concatenative programming to your applications — whether you're scripting complex workflows, embedding a runtime in a desktop or mobile app, designing a custom DSL, or automating IoT pipelines.

### Key Features

- **Stack-Based RPN Syntax** - Clean, unambiguous, no operator precedence
- **280+ Built-in Functions** - Math, strings, lists, files, HTTP, and more
- **Async/Await Support** - Modern asynchronous execution
- **Plugin System** - Clean plugin contract via `MOGWAI.IPlugin` — official plugins in development
- **Battle-Tested** - 10+ years of real-world usage
- **Extensible** - Easy integration with .NET applications
- **Cross-Platform** - Windows, Linux, macOS, Android, iOS
- **Visual Debugging** - MOGWAI STUDIO integration (coming soon)

---

## Get Started in Seconds

### Download MOGWAI CLI — no build required

Grab the ready-to-run binary for your platform and start writing MOGWAI scripts immediately. No .NET SDK needed.

[Windows x64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-win-x64.zip)  
[macOS x64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-osx-x64.zip)  
[macOS arm64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-osx-arm64.zip)  
[Linux x64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-linux-x64.zip)  
[Linux arm64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-linux-arm64.zip)  

### VS Code Extension

Get syntax highlighting and run your MOGWAI scripts directly from VS Code via the CLI:

[**Install MOGWAI Language Extension**](https://marketplace.visualstudio.com/items?itemName=mogwai.mogwai-language) from the VS Code Marketplace

---

## The MOGWAI Ecosystem

### MOGWAI Runtime (Open Source)

The core scripting engine, available as a NuGet package. Embed MOGWAI in your .NET applications.

- **License:** Apache 2.0
- **Package:** [MOGWAI on NuGet](https://www.nuget.org/packages/MOGWAI/)
- **Status:** Production ready

### MOGWAI CLI (Open Source)

Command-line interface for running MOGWAI scripts and interactive REPL sessions.

- **License:** Apache 2.0
- **Repository:** [MOGWAI CLI](https://github.com/Sydney680928/mogwai/tree/main/examples/Console/ConsoleExample)
- **Status:** Functional

**Ready-to-run binaries available** — no .NET SDK required:

[Windows x64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-win-x64.zip)  
[macOS x64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-osx-x64.zip)  
[macOS arm64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-osx-arm64.zip)  
[Linux x64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-linux-x64.zip)  
[Linux arm64](https://github.com/Sydney680928/mogwai/releases/latest/download/mogwai-cli-linux-arm64.zip)  

### MOGWAI VS Code Extension (Open Source)

Syntax highlighting, code editing, and script execution via MOGWAI CLI — directly inside VS Code.

- **License:** Apache 2.0
- **Marketplace:** [mogwai.mogwai-language](https://marketplace.visualstudio.com/items?itemName=mogwai.mogwai-language)
- **Status:** Available

### MOGWAI STUDIO (Coming Soon)

Visual IDE for MOGWAI development with debugging, breakpoints, and code editing.

- **License:** Proprietary (Freemium model)
- **Status:** Early private development — not yet publicly available
- **Features:** Visual debugger, syntax highlighting, project management
- **Release:** TBA

---

## Blog Articles

A curated list of English articles about MOGWAI on [coding4phone.com](https://coding4phone.com):

- [MOGWAI in Production: How a Scripting Language Powers an Industrial Test Bench](https://coding4phone.com/?p=1990&lang=en)
- [MOGWAI Under the Hood: Syntactic Sugar and the RPN Canonical Form](https://coding4phone.com/?p=2003&lang=en)
- [MOGWAI — The `-->` Operator: Transforming a Variable In Place, Cleanly](https://coding4phone.com/?p=2010&lang=en)
- [Dynamic Variable Assignment in MOGWAI](https://coding4phone.com/?p=2026&lang=en)
- [Loops in MOGWAI](https://coding4phone.com/?p=2041&lang=en)
- [Timers in MOGWAI](https://coding4phone.com/?p=2060&lang=en)
- [MOGWAI — The Origin Story](https://coding4phone.com/?p=2066&lang=en)
- [Events in MOGWAI](https://coding4phone.com/?p=2080&lang=en)
- [Tasks in MOGWAI](https://coding4phone.com/?p=2194&lang=en)
- [Code editor in MOGWAI CLI](https://coding4phone.com/?p=2301&lang=en)
- [Bytes aren’t scary. Manipulating binary data with MOGWAI](https://coding4phone.com/?p=2251&lang=en)
- [MOGWAI v8.6 — Objects and Assertions](https://coding4phone.com/?p=2324&lang=en)

---

## Quick Start

### Installation

Install the MOGWAI runtime via NuGet:

```bash
dotnet add package MOGWAI
```

### Hello World

```csharp
using MOGWAI.Engine;
using MOGWAI.Interfaces;

var engine = new MogwaiEngine("MyApp");
engine.Delegate = this; // Your class implementing IDelegate

var result = await engine.RunAsync(@"
    \"Hello from MOGWAI!\" ?
    2 3 + ?
", debugMode: false);
```

![MOGWAI](./images/img02.png)

### MOGWAI Language Example

```mogwai
# Variables
42 -> '$answer'
"Hello World" -> '$greeting'

# Functions
to 'square' with [n: .number] do
{
    n n *
}

# Lists
(1 2 3 4 5) foreach 'n' transform { n square } -> '$result'
$result ? # Outputs the list of squares (1 4 9 16 25)

# Records
[name: "MOGWAI", version: "8.0"] -> 'info'

# Conditionals
if (answer 40 >) then
{
    "Answer is greater than 40" ?
}
```

> **Note on variables:** Variables prefixed with `$` are **global**. When the engine is instantiated with `keepAlive: true`, global variables persist across multiple script executions — making them the natural choice for interactive sessions like the REPL or the [Blazor playground](https://sydney680928.github.io/MOGWAI/). Local variables (without `$`) are scoped to a single execution and are the recommended approach for one-shot embedding scenarios.
> ```csharp
> // Global variables persist across executions
> var engine = new MogwaiEngine("MyApp", keepAlive: true, useDefaultFolders: false);
>
> // Global variables are reset on each execution (default)
> var engine = new MogwaiEngine("MyApp");
> ```

---

## Why MOGWAI instead of Lua, Python, or JavaScript?

This is a fair question. Here's the honest comparison:

| | MOGWAI | Lua | IronPython | Jint (JS) |
|---|---|---|---|---|
| Paradigm | Stack-based RPN | Infix | Infix | Infix |
| .NET native | ✅ | Binding layer | Partial | ✅ |
| NativeAOT | ✅ | ❌ | ❌ | ❌ |
| Plugin system | ✅ | ❌ | ❌ | ❌ |
| Bundle size | Minimal | Small | Heavy | Medium |
| Learning curve | Different | Low | Low | Low |

**Choose MOGWAI if:**
- You want **zero operator precedence ambiguity** — the stack is the truth
- You're embedding in a **.NET NativeAOT** application
- You appreciate the **concatenative programming** model (Forth, Factor, PostScript, HP RPL)
- You need a **lightweight, extensible runtime** with a clean plugin contract

**Choose Lua/JS if:**
- Your team is already fluent in infix syntax
- You need a large existing ecosystem of scripts

MOGWAI isn't trying to replace general-purpose scripting — it's the right tool when **stack semantics and .NET-native embedding** matter.

---

## Build from Source

### Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/download) or later
- Git

### Clone and Build

```bash
# Clone repository
git clone https://github.com/Sydney680928/mogwai.git
cd mogwai

# Restore dependencies
dotnet restore src/MOGWAI/MOGWAI.sln

# Build
dotnet build src/MOGWAI/MOGWAI.sln --configuration Release

# Pack (optional)
dotnet pack src/MOGWAI/MOGWAI.sln --configuration Release
```

The compiled assembly will be in `src/MOGWAI/MOGWAI/bin/Release/net9.0/`.

### Project Structure

```
mogwai/
├── .github/
│   └── workflows/                  # GitHub Actions (CI, GitHub Pages deployment)
├── docs/
│   ├── EN/
│   │   ├── use-cases/              # Use case articles
│   │   ├── MOGWAI_EN.md            # Language reference
│   │   ├── MOGWAI_FUNCTIONS_EN.md  # Function reference
│   │   └── MOGWAI_INTEGRATION_GUIDE_EN.md  # Integration guide
│   └── FR/
│       ├── cas d'usage/            # Use case articles in french
│       ├── MOGWAI_FR.md            # Language reference in french
│       ├── MOGWAI_FUNCTIONS_FR.md  # Function reference in french
│       └── MOGWAI_INTEGRATION_GUIDE_FR.md  # Integration guide in french
├── examples/
│   ├── Blazor/
│   │   └── MogwaiPlayground/       # Blazor WASM interactive playground
│   ├── Console/
│   │   └── ConsoleExample/
│   │       └── MOGWAI_CLI/         # Command-line interface and REPL
│   ├── MAUI/
│   │   └── MauiExample/            # Cross-platform mobile app
│   └── WinForms/
│       └── WinFormsExample/        # Turtle graphics demo
├── images/                         # Screenshots and media
├── src/
│   └── MOGWAI/
│       ├── MOGWAI.sln              # Main solution
│       ├── MOGWAI/
│       │   ├── Engine/             # Core runtime engine
│       │   ├── Objects/            # MOGWAI object types
│       │   ├── Primitives/         # Built-in functions (280+ primitives)
│       │   ├── Interfaces/         # Public interfaces (IDelegate, IPlugin)
│       │   └── Exceptions/         # Exception types
│       ├── MOGWAI.Tests/           # Unit tests
│       └── MOGWAI_TEST/            # Lightweight CLI runner for in-solution testing
├── LICENSE                         # Apache 2.0 license
├── NOTICE                          # Copyright notice
└── README.md                       # This file
```

---

## Documentation

### Complete Guides

- **[Language Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_EN.md)** - Complete MOGWAI language guide
- **[Function Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 280+ built-in functions
- **[Integration Guide](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI in your .NET apps

### Examples

Examples are available:

- **[MOGWAI CLI](https://github.com/Sydney680928/mogwai/tree/main/examples/Console)** - Command-line interface and REPL
- **[WinForms Example](https://github.com/Sydney680928/mogwai/tree/main/examples/WinForms)** - Turtle graphics with MOGWAI
- **[MAUI Example](https://github.com/Sydney680928/mogwai/tree/main/examples/MAUI)** - Cross-platform mobile app
- **[Blazor Example](https://github.com/Sydney680928/mogwai/tree/main/examples/Blazor)** - Blazor WASM app

### Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

**Latest Release:** [v8.6.0](https://github.com/Sydney680928/mogwai/releases/tag/v8.6.0)

---

## MOGWAI STUDIO (Coming Soon)

![MOGWAI](./images/img04.png)

MOGWAI STUDIO v2 is a visual IDE for MOGWAI 8, currently in early private development. A previous version of MOGWAI STUDIO exists but targets MOGWAI 6 and is now obsolete.

### Planned Features

- **Visual Debugger** - Set breakpoints, step through code
- **Syntax Highlighting** - Color-coded MOGWAI syntax
- **Variable Inspector** - Examine variables in real-time
- **Stack Visualization** - See the stack state at any point
- **Project Management** - Organize scripts and modules
- **Network Debugging** - Debug remote MOGWAI runtimes
- **Code Completion** - IntelliSense for MOGWAI functions

### Availability

**Status:** Early private development — not yet publicly available
**Release Model:** Freemium (Free version + Pro version)
**Price:** Pro version planned at €29 (one-time purchase)
**Distribution:** Gumroad + installer package

Stay tuned for updates on [mogwai.eu.com](https://www.mogwai.eu.com)!

---

## Use Cases

### Industrial IoT Automation

```mogwai
# Read sensor via BLE (requires MOGWAI_BLE plugin — coming soon)
"AA:BB:CC:DD:EE:FF" ble.connect -> 'device'
device "temperature" ble.read -> 'temp'

# Control based on value
if (temp 25 >) then
{
    "fan" gpio.on
}
```

> *Note: Official MOGWAI plugins (BLE, Serial...) are currently in development and not yet publicly available. Third-party plugins can already be built today by implementing the `MOGWAI.IPlugin` interface.*

![MOGWAI](./images/img07.png)

[Use Case #1 — Electronic Board Test Bench](docs/EN/use-cases/use-case-01-test-bench.md)

### Embedded Applications

```mogwai
# WinForms turtle graphics
100 turtle.forward
90 turtle.right
100 turtle.forward
"Square complete!" ?
```

![MOGWAI](./images/img05.png)

### Blazor WASM Applications

![MOGWAI](./images/img08.png)

You can test it live on [Blazor REPL](https://sydney680928.github.io/MOGWAI/)

---

## Roadmap

### Version 8.0 (Released)

- Complete rewrite with namespace organization
- 240+ primitives
- Apache 2.0 open source license
- Published on NuGet
- .NET 9.0 support
- Complete documentation

### Version 8.4

- Plugin contract via `MOGWAI.IPlugin` interface
- AOT compatibility (`IsAotCompatible`)
- Major performance optimizations (O(1) primitive lookup, LINQ removal in hot paths)
- `bag` primitive
- `!A` auto-eval sigil
- `-->` in-place pipeline operator
- Binary data primitives (DATA/BIN families)

### Version 8.5

- Enhanced debugging protocol
- 280+ built-in primitives (approaching 300)
- Additional examples and documentation

### Version 8.6 (Latest)

- Basic POO support
- MOGWAI STUDIO v2 (early private development — rebuilt from scratch for MOGWAI 8)

### Future Plans

- MOGWAI STUDIO v2 public release
- Community plugins marketplace
- Additional language integrations
- Extended function library

---

## Contributing

Contributions are welcome! Here's how you can help:

### Reporting Issues

Found a bug or have a feature request? Please open an issue on GitHub:

- **Bug Reports:** Use the bug report template
- **Feature Requests:** Describe the use case and expected behavior
- **Questions:** Use GitHub Discussions

### Contributing Code

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Commit with clear messages (`git commit -m 'Add amazing feature'`)
5. Push to your fork (`git push origin feature/amazing-feature`)
6. Open a Pull Request

### Code Style

- Follow existing code conventions
- Add XML documentation comments for public APIs
- Keep functions focused and well-named

### Testing

MOGWAI includes a `MOGWAI.Tests` project covering core engine behavior. When contributing:

- Run the existing unit tests to verify nothing is broken
- Add unit tests for new primitives or engine features when possible
- Ensure your changes compile without warnings
- Verify existing examples still work correctly

---

## Why MOGWAI?

### Born from Real Needs

Created in 2015 to simulate Bluetooth Low Energy devices for IoT testing. Over 10 years, MOGWAI evolved into a full-featured scripting language used in industrial automation.

### Battle-Tested

- **10+ years of real-world usage** - From prototyping to industrial environments
- **Thousands of scripts** - Executed in field deployments

### HP Calculator Heritage

Inspired by the legendary HP 28S and HP 48 calculators, MOGWAI brings RPN elegance to modern software:

- **Clear semantics** - No operator precedence confusion
- **Stack-based** - Natural for complex calculations
- **Composable** - Build complex operations from simple parts

---

## License

### MOGWAI Runtime & CLI

**Apache License 2.0**

```
Copyright 2015-2026 Stéphane Sibué

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```

See [LICENSE](LICENSE) and [NOTICE](NOTICE) for details.

### MOGWAI STUDIO

**Proprietary License** - Freemium model (Free + Pro versions)

MOGWAI STUDIO is not open source and will be distributed under a proprietary license. More details will be announced upon release.

---

## Links

- **Website:** [mogwai.eu.com](https://www.mogwai.eu.com)
- **NuGet Package:** [MOGWAI](https://www.nuget.org/packages/MOGWAI/)
- **Author:** [Stéphane Sibué](https://www.coding4phone.com)
- **GitHub:** [Sydney680928/mogwai](https://github.com/Sydney680928/mogwai)
- **Issues:** [Report a bug](https://github.com/Sydney680928/mogwai/issues)

---

## Community & Support

- **GitHub Issues:** For bug reports and feature requests
- **GitHub Discussions:** For questions and community support
- **Email:** Contact via [coding4phone.com](https://www.coding4phone.com)

---

## Acknowledgments

- **HP Calculators** - For inspiring the RPN approach
- **Open Source Community** - For .NET and supporting tools
- **Early Adopters** - For feedback and real-world testing

---

## Have Questions or Feedback?

We'd love to hear from you!

- **Found a bug?** [Open an issue](https://github.com/Sydney680928/mogwai/issues/new)
- **Have an idea?** [Start a discussion](https://github.com/Sydney680928/mogwai/discussions/new)
- **Like MOGWAI?** Star the repo to show your support!
- **Using MOGWAI in production?** We'd love to hear your story!

Even a simple "I tried it and it works!" is valuable feedback!

---

**Made with ❤️ by [Stéphane Sibué](https://www.coding4phone.com)**

*MOGWAI - Where stack-based elegance meets modern .NET power* ✨
