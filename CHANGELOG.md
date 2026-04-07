# Changelog

All notable changes to MOGWAI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Fixed

## [8.5.0] - 2026-04-07

### Added

- Added `foreach...filter` loop: filters elements of a list by applying a predicate block to each element. Only the elements for which the block returns `true` are collected into a new list, which is pushed onto the stack.
  ```
  (1 2 3 4 5 6 7 8 9 10) foreach 'i' filter { i 5 >= i 8 <= and }
  # Returns (5 6 7 8)
  ```

- Added a new `bag` primitive that pushes onto the stack the container (record or list) of the currently executing block or function. This allows a block or function stored inside a record or list to reference its own container, enabling a prototype-based programming pattern.
  `bag` returns `null` if the executing code has no container (top-level context).
  The `Bag` property is assigned when an item is inserted into a record or list, and cleared when it is extracted.
  ```
  [x: 10 y: 20 s: « ! bag x: get bag y: get + »] -> '$R'
  !$R   # → [x: 10  y: 20  s: 30]
  ```

#### New primitives — Endianness conversion (integer)
- Added `->dataLE8/16/24/32/48/64` and `->dataBE8/16/24/32/48/64` — convert a number to `DATA` in Little or Big Endian byte order, fixed size.
- Added `dataLE8/16/24/32/48/64->` and `dataBE8/16/24/32/48/64->` — convert a `DATA` to a number, interpreting bytes in Little or Big Endian byte order, fixed size.
- Added `->dataLE` and `->dataBE` — dynamic-size variants (number + size in bits → DATA).
- Added `dataLE->` and `dataBE->` — dynamic-size variants (DATA + size in bits → number).
- Added `LongValue` property to `MOGNumber` for 64-bit integer access.

Supported sizes: 8, 16, 24, 32, 48, 64 bits. Overflow is silently truncated (consistent with C# numeric cast behavior).

#### New primitives — Endianness conversion (float)
- Added `->dataLE32F` and `->dataBE32F` — convert a number to `DATA` as IEEE 754 single-precision float (4 bytes), in Little or Big Endian byte order.
- Added `->dataLE64F` and `->dataBE64F` — convert a number to `DATA` as IEEE 754 double-precision float (8 bytes), in Little or Big Endian byte order.
- Added `dataLE32F->` and `dataBE32F->` — convert a `DATA` to a number, interpreting bytes as IEEE 754 single-precision float.
- Added `dataLE64F->` and `dataBE64F->` — convert a `DATA` to a number, interpreting bytes as IEEE 754 double-precision float.

#### New primitives — Typed integer conversion
- Added `->i8`, `->i16`, `->i32`, `->i64` — bidirectional conversion between number and `DATA` as signed integers (Little Endian). If the argument is a number, returns a `DATA`. If the argument is a `DATA`, returns a number.
- Added `->u8`, `->u16`, `->u32`, `->u64` — same as above for unsigned integers.

#### New primitive — Bit testing
- Added `bit?` — returns `true` if the bit at the specified position of a binary object (`B:`) is set. Position is zero-based, starting from the rightmost bit.
  ```
  B:110011 1 bit?   # → true
  B:110011 2 bit?   # → false
  ```

#### New error
- Added `ConvertError` (MW.32) raised when a type conversion fails.

### Changed

- Renamed `using` to `mogwai.using` and `usings` to `mogwai.usings` for consistency with the `mogwai.*` namespace convention.

### Fixed

- Fixed incorrect byte order in `->i16`, `->i32`, `->i64`, `->u16`, `->u32`, `->u64`. These primitives were producing Big Endian output instead of Little Endian. The fix uses `BinaryPrimitives.WriteInt*/WriteUInt*LittleEndian` explicitly, making the behavior correct and portable across all architectures.

## [8.4.0] - 2026-03-27

### Added

- **`!A` sigil — direct evaluation of a variable's content**
  A new prefix sigil `!` can now be applied to any variable to immediately evaluate its content, without pushing the object onto the stack first.
 
  This completes the variable sigil set:
 
  | Notation | Behavior |
  |----------|----------|
  | `A`      | Reads A and pushes its value onto the stack |
  | `&A`     | Reference to A for in-place mutation |
  | `@A`     | Statically resolved read (compile-time) |
  | `!A`     | Evaluates the content of A directly |
 
  `!A` is universal — its effect depends on the type of the object stored in A:
 
  | Type | Effect of `!A` |
  |------|----------------|
  | Block `{ }` | Executes the code |
  | Function `« »` | Executes the function |
  | String `"..."` | Interpolates embedded `{! }` blocks |
  | List `( )` | Evaluates embedded blocks in elements |
  | Record `[ ]` | Evaluates embedded blocks in fields |
  | Number, boolean… | Silent no-op |
 
  Examples:
 
  ```mogwai
  # block
  100 -> 'A'
  { A 10 * } -> 'B'
  !B    # → 1000
 
  # string interpolation
  "We are in { ! now ->date year: get }" -> 'C'
  !C    # → "We are in 2026"
  ```
 
  **Containers are lazy.** Everything inside a container is deferred until `!` is applied — the container stores expressions, not values. This means `!A` on a composite object always evaluates with the **current state** of the program:
 
  ```mogwai
  10 -> 'A'
  { A 200 * } -> 'B'
  [ x: { A 10 * }
    y: "We are in { ! now ->date year: get }"
    z: !B ] -> 'R'
 
  !R       # → [ x: 100   y: "We are in 2026"   z: 2000 ]
  20 -> 'A'
  !R       # → [ x: 200   y: "We are in 2026"   z: 4000 ]
  ```
 
  Internally, `!A` sets the `AutoEval` flag on the object referenced by A and dispatches it directly — the object never lands on the stack as an intermediate value, making it slightly more efficient than the equivalent `A eval` sequence.
 
  For non-executable types (numbers, booleans, etc.), `!A` behaves identically to `A` — it is a silent no-op, no error is raised.
 
  The semantics of `!` are consistent with its existing use inside containers (`{ ! ... }`, `« ! ... »`, `( ! ... )`, `[ ! ... ]`): it always means *"resolve everything evaluable in this object"*, regardless of where it appears.
 
  **Circular reference detection** The runtime now detects circular references during evaluation and raises an error instead of looping indefinitely.
 
  When `!A` is called, the variable A is registered as being evaluated. If the evaluation chain reaches `!A` again before it completes, a circular reference error is returned immediately via `EvalResult`. The variable is released as soon as the evaluation completes, whether the result is a success or an error.
 
  ```mogwai
  { !B } -> 'A'
  { !A } -> 'B'
  !A    # → error: circular reference detected (A → B → A)
  ```
 
  The error includes the full chain of variable names involved in the cycle.
 
 
- Added `-->` in-place pipeline operator: applies a sequence of transformations directly to a referenced variable — e.g. `(->upper butfirst butlast) --> &A`.
  - New private primitive `PIPEREF` to support `-->`: pushes the actual value of the variable (not a copy) onto a private stack, evaluates each item in the list, then discards the private stack.
  - `PIPEREF` is transactional: a snapshot is taken before the pipeline starts and restored automatically if any item raises an error.
  - Quotations are valid items in the pipeline list, enabling complex inline logic mid-pipeline.
  - Empty pipeline list is a no-op with immediate early exit.

### Changed

- AOT compatibility — The MOGWAI engine is now fully compatible with .NET Native AOT publishing. Removed all dynamic JSON serialization in favor of source-generated contexts, replaced reflection-based assembly access with static attribute reading, and suppressed plugin system warnings by design. No behavioral changes.

- Performance improvement — Optimized the core execution loop in MOGCode.Execute(). Avoid unnecessary async state machine allocations on each iteration, and consolidated control flow flags into a single check. ~13% speedup measured on intensive benchmarks.
  
## [8.3.0] - 2026-03-17

### Added

- Added variable reference support with `&varname` notation. It is now possible to mutate variable content without pushing a copy onto the stack. The performance gain is significant with large lists, records, data, and strings.
Primitives with this capability are `+`, `set`, `get`, `butfirst`, `butlast`, `last`, `first`, `sub`, and `size`.
- Added host function detection by the parser to avoid delegate calls at runtime, improving execution performance.
- Added new `char->` primitive that returns the ASCII code from a single string character.
- Added explicit variable access with `@varname` notation. The performance gain is significant with frequent variable access.
- Added `foreach` loop over string characters.

### Changed

- Changed dictionary access to use `TryGetValue` instead of a `ContainsKey` check followed by value retrieval, improving lookup performance across all dictionaries used by the runtime.
- Changed variable and function name validation. Now, names must start with a letter or the _ character only.
- Refactored synchronous primitives: removed spurious `async`/`await Task.CompletedTask`, replaced with `Task.FromResult()`. No behavioral change.
- Removed systematic primitive cloning during execution, resulting in a performance gain.
- Removed all `LINK` calls during execution.
- Optimized primitive dictionaries to improve lookup speed.

## [8.2.0] - 2026-03-09

### Added

- Added a new classic-style syntax for calling functions and primitives with named parameters: `foo[x: 50 y: 20]`, as an alternative to the existing RPN style `[x: 50 y: 20] foo` and Objective-C style `[foo x: 50 y: 20]`.
- Added a new classic-style syntax for calling functions and primitives with list of parameters: `foo(2 3 4)`, as an alternative to the existing RPN style `2 3 4 foo`.

### Changed

- On error, the parser returns the position in the source code (used by MOGWAI STUDIO).

### Fixed

- Fixed UI freeze in Blazor WebAssembly playground when using `forever` loops.
- Fixed blocking of the single-threaded event loop by adding cooperative yielding.
- Fixed timers and events not working correctly alongside long-running scripts in the browser.
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

   These methods simplify object creation by not requiring an explicit Engine reference

 - Added convenience methods to MOGRecord for adding typed objects:
   - `SetString(string key, string value)` - Add MOGString
   - `SetNumber(string key, double value)` - Add MOGNumber 
   - `SetName(string key, string value)` - Add MOGName
   - `SetKey(string key, string value)` - Add MOGKey
   - `SetWord(string key, string value)` - Add MOGWord
   - `SetBoolean(string key, bool value)` - Add MOGBool
   - `SetNull(string key)` - Add MOGNull
   - `SetEmpty(string key)` - Add MOGEmpty
  
   These methods simplify object creation by not requiring an explicit Engine reference

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

[Unreleased]: https://github.com/Sydney680928/mogwai/compare/v8.5.0...HEAD
[8.5.0]: https://github.com/Sydney680928/mogwai/compare/v8.4.0...v8.5.0
[8.4.0]: https://github.com/Sydney680928/mogwai/compare/v8.3.0...v8.4.0
[8.3.0]: https://github.com/Sydney680928/mogwai/compare/v8.2.0...v8.3.0
[8.2.0]: https://github.com/Sydney680928/mogwai/compare/v8.1.0...v8.2.0
[8.1.0]: https://github.com/Sydney680928/mogwai/compare/v8.0.1...v8.1.0
[8.0.1]: https://github.com/Sydney680928/mogwai/compare/v8.0.0...v8.0.1
[8.0.0]: https://github.com/Sydney680928/mogwai/releases/tag/v8.0.0
