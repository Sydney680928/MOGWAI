# Changelog

All notable changes to MOGWAI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Fixed


## [8.2.0] - 2026-03-09

### Added

- Added a new classic-style syntax for calling functions and primitives with named parameters: `foo[x: 50 y: 20]`, as an alternative to the existing RPN style `[x: 50 y: 20] foo` and Objective-C style `[foo x: 50 y: 20]`.
- Added a new classic-style syntax for calling functions and primitives with list of parameters: `foo(2 3 4)`, as an alternative to the existing RPN style `2 3 4 foo`.

### Changed

- On error, the parser returns the position in the source code (used by MOGWAI STUDIO).

### Fixed

- Fixed UI freeze in Blazor WebAssembly playground when using `forever` loops.
- Added cooperative yielding to prevent blocking the single-threaded event loop.
- Timers and events now work correctly alongside long-running scripts in the browser.
- `for` loop infinite loop when start equals end. (Issues #4)
- `->unescape` does not handle `\"` escape sequence. (Issues #5)

## [8.1.0] - 2026-02-25

### Added

- Primitive '+/-' to negate a number (e.g. 5 +/- ==> -5)
- Added new error : OperationNotSupportedError (MW.7)
- Added convenience methods to MOGBaseItems for adding typed objects:
  - `AddString(string value)` - Add MOGString
  - `AddNumber(double value)` - Add MOGNumber 
  - `AddName(string value)` - Add MOGName
  - `AddKey(string value)` - Add MOGKey
  - `AddWord(string value)` - Add MOGWord
  - `AddBoolean(bool value)` - Add MOGBool
  - `AddNull()` - Add MOGNull
  - `AddEmpty()` - Add MOGEmpty

   These methods simplify object creation by not requiring explicit Engine reference

 - Added convenience methods to MOGRecord for adding typed objects:
   - `SetString(string key, string value)` - Add MOGString
   - `SetNumber(string key, double value)` - Add MOGNumber 
   - `SetName(string key, string value)` - Add MOGName
   - `SetKey(string key, string value)` - Add MOGKey
   - `SetWord(string key, string value)` - Add MOGWord
   - `SetBoolean(string key, bool value)` - Add MOGBool
   - `SetNull(string key)` - Add MOGNull
   - `SetEmpty(string key)` - Add MOGEmpty
  
   These methods simplify object creation by not requiring explicit Engine reference

 - Added new `foreach` usage that allows transforming items while iterating:
   - `(1 2 3 4) foreach 'item' transform { item 2 * }` returns `(2 4 6 8)`
   - `(1 2 3 4) foreach 'item' transform { item 2 * ->str }` returns `("2" "4" "6" "8")`

### Changed

  - Changed `get` primitive on MOGRecord to return MOGNull instead of throwing error when key is not found
  - Changed keys named after reserved words (primitives, host functions, extended functions) are allowed

### Fixed

- Fixed bug with MOGRecord AutoEval capability

## [8.0.1] - 2026-02-17

### Fixed

- Corrected GitHub repository URL in NuGet package metadata (no functional changes)

## [8.0.0] - 2026-02-17

### Added

**Open Source Release**

- MOGWAI is now open source under Apache 2.0 license
- Published on NuGet as `MOGWAI` package
- Available on GitHub at https://github.com/Sydney680928/mogwai

**Documentation**

- Complete integration guide for embedding MOGWAI in .NET applications
- Comprehensive language reference with syntax and semantics
- Function reference documenting all 240 built-in primitives
- Three working examples with full source code:
  - MOGWAI CLI: Console REPL with interactive mode
  - WinFormsMogwai: Turtle graphics demonstration
  - MOGWAI_RUNTIME: Cross-platform .NET MAUI application

**Core Features**

- 240 carefully selected primitives covering:
  - Math operations (arithmetic, trigonometry, statistics)
  - String manipulation (search, replace, format, encoding)
  - Record types (structured data)
  - List opérations
  - Control flow (if/then/else, loops, functions)
  - File I/O (read, write, CSV parsing)
  - HTTP client (GET, POST, headers, JSON)
  - Timers and events
  - Tasks

**Platform Support**

- Windows (x64, ARM64)
- Linux (x64, ARM64)
- macOS (x64, ARM64)
- Android via .NET MAUI
- iOS via .NET MAUI

**Development Tools**

- Visual debugging protocol for MOGWAI STUDIO integration
  - UDP discovery on port 1968
  - TCP debugging on ports 63000-65000
  - Breakpoint support
  - Stack inspection
  - Variable watches

### Changed

**Architecture**

- Major namespace reorganization for clarity:
  - `MOGWAI.Engine`: Core execution engine and runtime
  - `MOGWAI.Objects`: Type system (MOGNumber, MOGString, MOGList, etc.)
  - `MOGWAI.Interfaces`: Extension points (IDelegate, IPlugin)
  - `MOGWAI.Primitives`: Built-in function implementations
  - `MOGWAI.Exceptions`: Error handling and custom exceptions

**Primitive Refinement**

- Reduced from 300+ primitives in v7 to 240 in v8
- Removed rarely-used functions based on production usage analysis
- Kept only essential, well-tested primitives
- Each function now has comprehensive documentation

**Performance & Quality**

- Improved async/await implementation throughout codebase
- Enhanced error messages with better context and suggestions
- Reduced memory footprint for embedded systems
- Optimized stack operations for better performance

**Technology Stack**

- Updated to .NET 9.0 as target framework
- Removed legacy .NET Framework support
- Full async/await support in all I/O operations
- Improved cross-platform compatibility

**Interface Refinement**

- IDelegate interface refined to 24 essential methods
- Clearer separation between required and optional methods
- Better documentation of extension points

### Removed

**Deprecated Features**

- Experimental primitives from v7 that saw minimal production usage
- Legacy synchronous I/O functions (replaced by async equivalents)
- .NET Framework 4.x support (now requires .NET 9.0+)
- Undocumented internal APIs

### Fixed

**Stability Improvements**

- Fixed race condition in timer cleanup for long-running scripts
- Corrected timezone conversion edge cases in astronomical calculations
- Fixed stack overflow in deeply nested function calls (now limited to safe depth)
- Improved error handling in async primitives

## Version History Summary

**v8.x - Open Source Era**

- Public releases on GitHub and NuGet
- Community-driven development
- Complete documentation and examples

**v7.x - Async/Await Implementation (Internal)**

- Complete rewrite to async/await
- MOGWAI STUDIO debugging protocol
- Enhanced type system

**v6.x - Production Hardening (Internal)**

- Field deployment in astronomical clocks
- Performance optimizations
- Bug fixes from production use

**v5.x - Initial Production Deployment (Internal)**

- First deployment in astronomical clocks controlling street lighting
- GPS integration for sunrise/sunset calculations
- Proven in 24/7 operation

**v1-4.x - Development and Prototyping (Internal)**

- Language design and experimentation
- Core primitive development
- BLE simulation capabilities

---

## Links

- **Repository**: https://github.com/Sydney680928/mogwai
- **NuGet Package**: https://www.nuget.org/packages/MOGWAI
- **Documentation**: https://github.com/Sydney680928/mogwai/tree/main/docs
- **Issue Tracker**: https://github.com/Sydney680928/mogwai/issues
- **Releases**: https://github.com/Sydney680928/mogwai/releases

---

[Unreleased]: https://github.com/Sydney680928/mogwai/compare/v8.2.0...HEAD
[8.2.0]: https://github.com/Sydney680928/mogwai/compare/v8.1.0...v8.2.0
[8.1.0]: https://github.com/Sydney680928/mogwai/compare/v8.0.1...v8.1.0
[8.0.1]: https://github.com/Sydney680928/mogwai/compare/v8.0.0...v8.0.1
[8.0.0]: https://github.com/Sydney680928/mogwai/releases/tag/v8.0.0
