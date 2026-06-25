# Changelog

All notable changes to MOGWAI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **`setRandomSeed` primitive** — sets the seed of the random number generator, making subsequent random operations deterministic and reproducible. Takes an integer seed. Passing `null` or `empty` clears the seed, returning the generator to non-deterministic (time-based) behavior.

  ```
  234 setRandomSeed   # subsequent random calls become deterministic
  null setRandomSeed  # back to a non-deterministic seed
  empty setRandomSeed # same effect as null
  ```

- **`mogwai.primitiveInfo` primitive** — returns a record with information about a given primitive. Takes a `name` and pushes a record containing the primitive's `name:` and its `birth:` (the MOGWAI version it was introduced in, as a string). Raises **MW.22** (bad argument value) if `name` does not match a known primitive.

  ```
  'calc' mogwai.primitiveInfo ?   # → [name: 'calc' birth: "8.12.0"]
  ```

- **`insert` primitive** — inserts an element at a given position in a `list` or a `data`. Takes the value to insert, the target `list`/`data`, and a zero-based index; an index equal to the collection's size appends at the end. Also works on references (`&var`) to a `list` or `data` variable, mutating it in place.

  For `list`, any value can be inserted. For `data`, the inserted value must be a byte (`0`–`255`); raises **MW.22** if it isn't. In both cases, raises **MW.22** if the index is out of range (negative or greater than the collection's size).

  ```
  "EEE" (1 2 3) 1 insert ?       # → (1 "EEE" 2 3)
  0xAA D:FFFFFFFF 1 insert ?     # → D:FFAAFFFFFF

  (1 2 3) -> 'L'
  "EEE" &L 1 insert             # L is now (1 "EEE" 2 3)
  ```

### Changed

- **`MOGPrimitive.Birth` property** — every `MOGPrimitive` now exposes a `Birth` property of type `Version`, recording the MOGWAI version in which it was introduced. Defaults to `8.0.0`. All existing primitives have been updated with their correct `Birth` value.

### Fixed

## [8.12.0] - 2026-06-17

### Added

- **`calc` primitive** — evaluates an infix mathematical expression given as a string and pushes the result onto the stack. Internally uses Dijkstra's Shunting-yard algorithm to convert the infix expression to RPN before execution. Supports the standard arithmetic operators (`+`, `-`, `*`, `/`), parentheses, all MOGWAI primitives and constants (`sin`, `cos`, `sqrt`, `pow`, `PI`, `E`, …), multi-argument functions (`pow(2, 10)`), local and global variables, and all MOGWAI sigils (`@`, `&`, `!`, `$`).

  ```
  500 -> 'X'
  3.14 -> 'Y'
  "5 * X + (7 + sin(Y))" calc ?   # → 2507.001...
  "sin(PI / 3)" calc ?             # → 0.866...
  "pow(2, 10)" calc ?              # → 1024
  ```
  
## [8.11.0] - 2026-06-16

### Added

- **Hyperbolic functions** — six new primitives mirroring the existing trigonometric set (`sin`, `cos`, `tan`, `asin`, `acos`, `atan`). All map directly to their `Math.*` counterparts in .NET.

  | Primitive | Description |
  |-----------|-------------|
  | `sinh` | Hyperbolic sine. Mirrors `Math.Sinh()`. |
  | `cosh` | Hyperbolic cosine. Mirrors `Math.Cosh()`. |
  | `tanh` | Hyperbolic tangent. Mirrors `Math.Tanh()`. |
  | `asinh` | Inverse hyperbolic sine. Mirrors `Math.Asinh()`. |
  | `acosh` | Inverse hyperbolic cosine. Mirrors `Math.Acosh()`. |
  | `atanh` | Inverse hyperbolic tangent. Mirrors `Math.Atanh()`. |

  ```
  1.5 sinh ?    # → 2.1292794550948173
  1.5 cosh ?    # → 2.352409615243247
  0.9 tanh ?    # → 0.7162978701990245
  2.0 asinh ?   # → 1.4436354751788103
  2.0 acosh ?   # → 1.3169578969248166
  0.9 atanh ?   # → 1.4721842907995872
  ```

- **`str.repeat` primitive** — builds a new string by repeating a source string a given number of times. Takes a string and a non-negative integer count; raises **MW.22** if the count is negative. A count of `0` returns an empty string.

  ```
  "E" 5 str.repeat ?    # → "EEEEE"
  "ab" 3 str.repeat ?   # → "ababab"
  "x" 0 str.repeat ?    # → ""
  ```

- **Version comparison primitives** — seven new primitives for comparing version strings and validating version format. Version strings follow the `System.Version` format: `"major.minor"`, `"major.minor.revision"`, or `"major.minor.revision.build"`. All comparison primitives take two version strings (`a b`) and push a boolean. If either argument is not a valid version string, **MW.22** (bad argument value) is raised.

  | Primitive | Description |
  |-----------|-------------|
  | `ver?`  | Returns `true` if the string is a valid version, `false` otherwise. Never raises an error. |
  | `ver>`  | Returns `true` if `a > b`. |
  | `ver<`  | Returns `true` if `a < b`. |
  | `ver>=` | Returns `true` if `a >= b`. |
  | `ver<=` | Returns `true` if `a <= b`. |
  | `ver==` | Returns `true` if `a == b`. |
  | `ver!=` | Returns `true` if `a != b`. |

  ```
  "8.10" ver?                          # → true
  "8" ver?                             # → false  (major only not supported)
  "8.10.0.0" "8.2" ver>               # → true
  "8.10.0.0" "8.10.0.0" ver==         # → true
  mogwai.info->version: "8.10" ver>=   # → true  (typical runtime version check)
  ```

- **String manipulation primitives** — twelve new primitives covering search, transformation, padding, insertion, removal, and URL decoding.

  **Search & test**

  | Primitive | Signature | Description |
  |-----------|-----------|-------------|
  | `str.indexOf` | `string search str.indexOf` | Returns the zero-based index of the first occurrence of `search` in `string`, or `-1` if not found. Case-sensitive. |
  | `str.startsWith` | `string prefix str.startsWith` | Returns `true` if `string` starts with `prefix`. Case-sensitive. |
  | `str.endsWith` | `string suffix str.endsWith` | Returns `true` if `string` ends with `suffix`. Case-sensitive. |

  **Transformation**

  | Primitive | Signature | Description |
  |-----------|-----------|-------------|
  | `str.replace` | `string old new str.replace` | Replaces all occurrences of `old` with `new` in `string`. Case-sensitive. |
  | `str.trim` | `string str.trim` | Removes leading and trailing whitespace characters (spaces, tabs, `\r`, `\n`). |
  | `str.trimStart` | `string str.trimStart` | Removes leading whitespace characters only. |
  | `str.trimEnd` | `string str.trimEnd` | Removes trailing whitespace characters only. |
  | `str.padLeft` | `string width str.padLeft` | Pads `string` on the left with spaces to reach `width`. Returns `string` unchanged if already at or above `width`. |
  | `str.padRight` | `string width str.padRight` | Pads `string` on the right with spaces to reach `width`. Returns `string` unchanged if already at or above `width`. |
  | `str.insert` | `string insertion index str.insert` | Inserts `insertion` into `string` at zero-based `index`. Raises **MW.22** if `index < 0` or `index > size of string`. |
  | `str.remove` | `string start count str.remove` | Removes `count` characters from `string` starting at zero-based `index` `start`. Raises **MW.22** if `start` or `count` are invalid. |

  **Encoding**

  | Primitive | Signature | Description |
  |-----------|-----------|-------------|
  | `->urlDecode` | `string ->urlDecode` | Decodes a URL-encoded string. Inverse of `->urlEncode`. |

  ```
  "E;Y;5" ";" "--" str.replace ?        # → "E--Y--5"
  "HELLO" "L" str.indexOf ?             # → 2
  "MOGWAI" "MO" str.startsWith ?        # → true
  "MOGWAI" "WAI" str.endsWith ?         # → true
  "  MOGWAI " str.trim ?                # → "MOGWAI"
  " MOGWAI " str.trimStart ?            # → "MOGWAI "
  " MOGWAI " str.trimEnd ?              # → " MOGWAI"
  "MOGWAI" 10 str.padLeft ?             # → "    MOGWAI"
  "HELLO LE MONDE" "-" 5 str.insert ?   # → "HELLO- LE MONDE"
  "HELLO LE MONDE" 5 3 str.remove ?     # → "HELLO MONDE"
  "Hello%20World" ->urlDecode ?         # → "Hello World"
  ```

### Fixed

- **`timer` syntax sugar — timer body left on the stack after parsing.** When a timer was defined using the `timer 'name' every N do { ... }` syntax, the parser correctly expanded the declaration but left the timer body on the stack as a residual value. This could silently corrupt subsequent stack operations. The residual value is now properly consumed by the parser.

## [8.10.0] - 2026-06-10

### Added

- **`round` primitive** — rounds a decimal number to the specified number of decimal places.
  When `n` is `0`, returns a whole number (no decimal point).

  ```
  5.78934 3 round ?    # → 5.789
  45.324322 0 round ?  # → 45
  ```

- **`log` primitive** — returns the natural logarithm (base *e*) of a number. Mirrors `Math.Log()` in C#.

  ```
  40 log ?   # → 3.6888794541139363
  ```

- **`log10` primitive** — returns the base-10 logarithm of a number. Mirrors `Math.Log10()` in C#.

  ```
  34 log10 ?   # → 1.5314789170422551
  ```

- **`exp` primitive** — returns *e* raised to the specified power. Mirrors `Math.Exp()` in C#.

  ```
  23 exp ?   # → 9744803446.248903
  ```

- **`E` primitive** — pushes the value of Euler's number (*e* = 2.718…) onto the stack.
  Complements the existing `PI` primitive.

  ```
  E ?   # → 2.718281828459045
  ```

- **`gcd` primitive** — returns the greatest common divisor of two integers, computed via
  the Euclidean algorithm. Both values are taken as absolute integers before processing.

  ```
  345 4 gcd ?   # → 1
  ```

- **`lcm` primitive** — returns the least common multiple of two integers. Both values are
  taken as absolute integers. Returns `0` if either argument is `0`.

  ```
  345 4 lcm ?   # → 1380
  ```

## [8.9.1] - 2026-06-08

### Added

- **`task.start` primitive** — launches a task without parameters. Complements `task 'name' start with` for tasks that require no input.

  ```
  task 'T1' do
  {
      # no parameter expected
      "Working..." ?
      true task.setResult
  }

  'T1' task.start
  'T1' task.wait
  ```

  Previously, launching a parameterless task required passing a dummy value (`null` or `empty`) and discarding it inside the task with `clear` or `drop`. `task.start` eliminates this workaround entirely.

### Changed

- **Error identifiers — corrected misspelled names.** Several public `Error` constants carried spelling mistakes (`Encounted`, `Unabled`) or a grammatical slip (`DoesNotExists`) in their C# identifiers. They have been renamed for correctness:

  - `HaltEncountedError` → `HaltEncounteredError`
  - `UnabledToFireEventError` → `UnableToFireEventError`
  - `UnabledToWriteValueError` → `UnableToWriteValueError`
  - `UnabledToWriteValueInUndeclaredVarError` → `UnableToWriteValueInUndeclaredVarError`
  - `UnabledToStartTaskError` → `UnableToStartTaskError`
  - `PathDoesNotExistsError` → `PathDoesNotExistError`

  **Breaking (C# host code only):** host applications that reference these error constants by name must update to the new identifiers. MOGWAI scripts are unaffected — they identify errors by code (`MW.x`), never by constant name.

### Fixed

- **Auto-evaluated records and lists — `!` flag incorrectly retained after evaluation.** When a record or list marked with `!` (auto-evaluation) was evaluated, the resulting object kept the auto-evaluation flag set. The final value was correct, but the engine was forced to re-evaluate the object on every subsequent access, incurring unnecessary overhead. The flag is now cleared on the evaluated result for both records and lists.

  ```
  [ ! x: rand y: rand ]   # → evaluated record, ! flag cleared
  (! now 50 $X)           # → evaluated list, ! flag cleared
  ```

- **Error messages — corrected English wording.** Several built-in error messages contained spelling or grammar mistakes: MW.2 (`encounted` → `encountered`), MW.6 / MW.47 / MW.48 / MW.61 (`unabled` → `unable`), MW.41 (`exits` → `exists`) and MW.71 (`does not exists` → `does not exist`). MW.47 and MW.48 now also end with `error`, consistent with every other message.

## [8.8.2] - 2026-06-05

### Fixed

- **String interpolation — quoted content in interpolated expressions caused premature truncation.** When an interpolated expression (`{! ... }`) contained double-quote characters, the string was incorrectly truncated at that point. Quoted content inside interpolated blocks is now handled correctly.

## [8.8.1] - 2026-06-04

### Fixed

- **`->json` — null value serialized as `null!` instead of `null`.** When converting a null value to JSON via `->json`, the output contained a spurious `!` character (`null!`), producing invalid JSON. Null values are now correctly serialized as `null`.

- **KeepAlive mode — stack was cleared between operations.** In KeepAlive mode, the stack must persist across successive operations, but it was being reset between each one, discarding any values left on the stack by previous operations. The stack is now correctly preserved between operations in KeepAlive mode.

## [8.8.0] - 2026-06-01

- **Skill system** — scripts can now verify at startup that they are running in the right host environment.

  A *skill* is a name declared by the host application that embeds MOGWAI, identifying a capability available in that specific execution context. The engine merges host-declared skills with any engine-level skills and deduplicates the result.

  Three new primitives:

  - **`skills`** — returns the merged, deduplicated list of all available skills. Returns an empty list `()` if no skills are declared.

    ```
    skills ?   # → ('APP_GIZMO' 'TUI' 'BLE')
    ```

  - **`hasSkill`** — tests whether a specific skill is present. Returns `true` or `false`. Never raises an error.

    ```
    if ('BLE' hasSkill) then
    {
        # BLE-specific code
    }
    ```

  - **`mogwai.assertSkill`** — asserts that a skill is present. If absent, displays the message and raises **MW.9** (`assert error`), stopping execution. If `MOGWAI.onError` is defined, it is called automatically. No-op if the skill is present.

    ```
    'APP_GIZMO' "This script requires GIZMO to run." mogwai.assertSkill
    'BLE' "This script requires BLE support." mogwai.assertSkill

    # rest of the script...
    ```

  Skills are also exposed via the `skills:` key of `mogwai.info`.

  **Host integration** — the `IDelegate` interface gains a `Skills()` method with a default implementation returning an empty array. Existing hosts require no changes.

  ```csharp
  public string[] Skills(MogwaiEngine engine) => ["APP_GIZMO", "TUI"];
  ```

- **`console.width` primitive** — returns the width of the console window in columns. Returns `0` in non-console hosts.

  ```
  console.width ?   # → 120
  ```

- **`console.height` primitive** — returns the height of the console window in rows. Returns `0` in non-console hosts.

  ```
  console.height ?   # → 30
  ```

- **`post` primitive** — hands control back to the runtime for the duration of the block execution.

  ```
  post
  {
      # this runs after pending events and timers
  }
  ```

  `post { }` with an empty block is valid.

- **`IDelegate` default implementations** — all non-essential methods of `IDelegate` now have default implementations, making MOGWAI embeddable with zero delegate code for simple use cases.

  The engine detects at startup whether it is running in a real console context (`engine.IsHostConsole`). Console-related defaults use `System.Console` when `true` and silently do nothing when `false`. Non-console hosts (WinForms, MAUI) are fully supported out of the box without any override.

  | Method | Default |
  |--------|---------|
  | `ConsolePrintLn` / `ConsolePrint` | `Console.WriteLine` / `Write` if `IsHostConsole` |
  | `ConsoleClearScreen` | `Console.Clear` if `IsHostConsole` |
  | `ConsoleLocate` | `Console.SetCursorPosition` if `IsHostConsole` |
  | `ConsoleGetCursorPosition` | `Console.CursorLeft/Top` if `IsHostConsole`, else `(0,0)` |
  | `ConsoleGetInputKey` | `Console.KeyAvailable` + `ReadKey` if `IsHostConsole`, else `-1` |
  | `Prompt` | `Console.Write` + `ReadLine` if `IsHostConsole`, else `null` |
  | `ConsoleWidth` / `ConsoleHeight` | `Console.WindowWidth/Height` if `IsHostConsole`, else `0` |
  | All other methods | no-op |
  | `HostFunctions` | `[]` |
  | `ExecuteHostFunction` | `EvalResult.NoExternalFunction` |
  | `Skills` | `[]` |

- **`mogwai.info` — `skills:` key added** — the record returned by `mogwai.info` now includes a `skills:` key containing the merged list of available skills, identical to the result of the `skills` primitive.

- **`path.home` primitive** — returns the home directory path as a string. Defaults to `Directory.GetCurrentDirectory()` at runtime construction time, unless overridden by the host or by `path.setHome`.

  ```
  path.home ?
  # Returns: "C:\Users\Username"
  ```

- **`path.setHome` primitive** — customizes the home directory path. The path is normalized via `Path.GetFullPath()`. Raises **MW.72** (`file operation error`) if the path cannot be resolved.

  ```
  "C:\MyHome" path.setHome
  ```

- **`HomeDirectory` property on `MogwaiEngine`** — exposes the home directory path for host-side get and set. Setting this property is equivalent to calling `path.setHome` from script code.
  
## [8.7.0] - 2026-05-26

### Added

- **`guid7` primitive** — generates a new UUID v7 and returns it as an uppercase string.

  Unlike `guid` (UUID v4, random), UUID v7 is time-ordered: the first 48 bits encode the current Unix timestamp in milliseconds, making it monotonically increasing and suitable for use as a sortable, database-friendly identifier.

  ```
  guid7 -> '$id'
  $id ?   # → "4843BAB6-6A90-4138-AC9F-DB7ABE0018CB"
  ```

- **`isAlive` primitive** — tests whether a class instance reference is still valid. Returns `true` if the instance is alive, `false` if it has been freed. Never raises an error regardless of the state of the reference. Complements `isEmpty` and `isNull` for defensive programming.

  ```
  $U1 isAlive   # → true or false

  if ($U1 isAlive) then
  {
      $U1->display:
  }
  ```
 
 - **`alive` primitive** — returns a list of all currently living instance references (`.objref`). Returns an empty list `()` if no instances are alive.

  ```
  alive ?   # → (§1 §2 §3 ...)

  # Filter by class
  alive foreach 'item' filter { item->className: 'User' == } -> '$users'
  ```

- **`frame` primitive** — returns a record describing the full structure of a named class.

  ```
  'Counter' frame ?
  # → [className: 'Counter' props: [value: .number] _props: [_step: .number] funcs: (onInit: increment: reset:) _funcs: ()]
  ```

  | Key | Content |
  |-----|---------|
  | `className:` | Class name |
  | `props:` | Public properties with their declared types |
  | `_props:` | Private properties with their declared types |
  | `funcs:` | Public method names |
  | `_funcs:` | Private method names |
   
- **`process.exec` primitive** — launches an external process, optionally sends data to its standard input (`stdin`), waits for it to finish, and captures its standard output (`stdout`) and standard error (`stderr`). Pushes a result record onto the stack.

  Unlike `process.start`, `process.exec` always waits for the process to finish and always captures both output streams.

  ```
  [filename: "dotnet" arguments: "--version"] process.exec -> '$r'
  $r status: get ?   # 0
  $r output: get ?   # "10.0.203"

  # With stdin input
  [filename: "myservice.exe" input: "42"] process.exec -> '$r'

  if ($r status: get 0 ==) then
  {
      $r output: get ?
  }
  ```

  | Key | Type | Description |
  |-----|------|-------------|
  | `filename:` | String | Path to the executable *(required)* |
  | `arguments:` | String | Command-line arguments *(optional)* |
  | `workingDirectory:` | String | Working directory *(optional)* |
  | `input:` | String | Data sent to `stdin` — stream closed after writing *(optional)* |

  Result record:

  | Key | Type | Description |
  |-----|------|-------------|
  | `status:` | Number | Exit code (0 = success) |
  | `output:` | String | Content of `stdout` (trailing newline trimmed) |
  | `error:` | String | Content of `stderr` (trailing newline trimmed) |

  `stdout` and `stderr` are read in parallel to prevent buffer deadlocks. Both streams are UTF-8 encoded. The process always runs without a visible window.

  Raises `MW.4` (internal error) if the process cannot be started.

### Fixed

- **`free` — instance not removed from registry when `onFree:` was defined.** When a class defined an `onFree:` lifecycle hook, calling `free` would execute the hook correctly but fail to remove the instance from the internal instance registry. As a result, the instance remained reachable and `isAlive` would incorrectly return `true` after the call. The instance is now properly removed in all cases.

- During a runtime reset, the stack was not being cleared correctly. 

## [8.6.0] - 2026-04-21

### Added

- **`mogwai.assert` primitive** — asserts that a condition is true. If the condition is false, raises error MW.9 (`assert error`) and stops execution. If `MOGWAI.onError` is defined, it will be called automatically.
  
  `mogwai.assert` accepts two forms for the condition argument:
  
  - A **list** — automatically evaluated. After execution, `mogwai.assert` verifies that exactly one value was pushed onto the stack (`MW.24` stack corruption if not) and that it is a boolean (`MW.21` bad argument type if not).
  - A **boolean** — used directly.
  
  Anything else raises `MW.21` (bad argument type).
  
  ```
  # Using a list (condition evaluated by assert)
  (a 10 ==) "a must equal 10" mogwai.assert
  
  # Using a boolean already on the stack
  a 10 ==  "a must equal 10" mogwai.assert
  ```
  
  The message is used in the error display. It is not accessible programmatically — `error.last` returns `MW.9`.

- **Object-Oriented Programming — class system**
  
  **MOGWAI** now supports a basic but complete class system. Classes group typed properties and methods, with explicit lifecycle management and no garbage collector.
  
  A class is defined with the `class ... do` sugar:
  
  ```
  class 'User' do
  {
      private:
      {
          x: .number
          y: .number
      }
  
      public:
      {
          id: .number
          name: .string
  
          display:
          {
              "ID={! self->id:} NAME={! self->name:}" eval ?
          }
      }
  }
  ```
  
  - `private:` section — properties and methods accessible only from within the class.
  - `public:` section — properties and methods accessible from outside the class.
  - Within each section, a name followed by a type sigil declares a **property** (initialized to `empty`); a name followed by a code block declares a **method**.
  - Two optional lifecycle hooks: `onInit:` (called automatically on `new` if defined) and `onFree:` (called automatically on `free` if defined). They can be placed in either section.

- **`new` primitive** — creates an instance of a named class. If `onInit:` is defined, it is called automatically.
  
  ```
  # Without onInit:
  'User' new -> '$U1'
  
  # With onInit: using ->params — pass a named record on the stack
  [id: 10 name: "SIBUE"] 'User' new -> '$U1'
  ```
  
  Each instance receives a unique internal handle noted `§N` (e.g. `§1`, `§2`). This number is never reused during the lifetime of the engine.

- **`free` primitive** — destroys a class instance. If `onFree:` is defined, it is called automatically before destruction. Any variable still holding a reference to the destroyed instance becomes invalid; attempting to use it raises an error.
  
  ```
  $U1 free
  ```

- **`self` variable** — automatically injected into every class method at execution time. Holds a reference to the current instance. Raises an error if used outside a class method.

- **`className:` reserved property** — a read-only public property automatically available on every class instance. Returns the class name as a string.
  
  ```
  $U1->className: ?   # → 'User'
  ```

- Added MW.95 — raised when a reserved property (`className:`) is written to or declared in a class definition.

- **Unified `->` / `<-` compact notation** — extended to all container types. The selector type determines the container:
  
  | Selector    | Container               | Example      |
  | ----------- | ----------------------- | ------------ |
  | `key:`      | Record / Class instance | `$U1->name:` |
  | `number`    | List / Byte array       | `$L->2`      |
  | `$variable` | Any                     | `$R->$K`     |
  
  Writing with `<-` requires the `&` sigil for in-place mutation: `"DUPONT" &$U1<-name:`.
  For computed values, use a `{! }` block: `{! rand 100 * ->int} &$U1<-x:`.

### Changed

- **Breaking change — `set` parameter order updated.**
  The value to write is now the **first** parameter, before the container and the key, for consistency with RPN conventions:
  
  ```
  # New order (v8.6+)
  100 [x: 10 y: 20] x: set   # → [x: 100 y: 20]
  
  # Previous order (v8.5 and earlier) — no longer valid
  [x: 10 y: 20] x: 100 set
  ```
  
  This affects all uses of `set` on records, lists, byte arrays, and class instances. The compact `<-` notation is unaffected.

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
  
  | Notation | Behavior                                    |
  | -------- | ------------------------------------------- |
  | `A`      | Reads A and pushes its value onto the stack |
  | `&A`     | Reference to A for in-place mutation        |
  | `@A`     | Statically resolved read (compile-time)     |
  | `!A`     | Evaluates the content of A directly         |
  
  `!A` is universal — its effect depends on the type of the object stored in A:
  
  | Type             | Effect of `!A`                        |
  | ---------------- | ------------------------------------- |
  | Block `{ }`      | Executes the code                     |
  | Function `« »`   | Executes the function                 |
  | String `"..."`   | Interpolates embedded `{! }` blocks   |
  | List `( )`       | Evaluates embedded blocks in elements |
  | Record `[ ]`     | Evaluates embedded blocks in fields   |
  | Number, boolean… | Silent no-op                          |
  
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
- Added explicit variable access with the `@` sigil notation. The performance gain is significant with frequent variable access.
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
- Fixed blocking of the single-threaded event loop by adding cooperative scheduling via `post`.
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

[Unreleased]: https://github.com/Sydney680928/mogwai/compare/v8.12.0...HEAD
[8.12.0]: https://github.com/Sydney680928/mogwai/compare/v8.11.0...v8.12.0
[8.11.0]: https://github.com/Sydney680928/mogwai/compare/v8.10.1...v8.11.0
[8.10.0]: https://github.com/Sydney680928/mogwai/compare/v8.9.1...v8.10.0
[8.9.1]: https://github.com/Sydney680928/mogwai/compare/v8.8.2...v8.9.1
[8.8.2]: https://github.com/Sydney680928/mogwai/compare/v8.8.1...v8.8.2
[8.8.1]: https://github.com/Sydney680928/mogwai/compare/v8.8.0...v8.8.1
[8.8.0]: https://github.com/Sydney680928/mogwai/compare/v8.7.0...v8.8.0
[8.7.0]: https://github.com/Sydney680928/mogwai/compare/v8.6.0...v8.7.0
[8.6.0]: https://github.com/Sydney680928/mogwai/compare/v8.5.0...v8.6.0
[8.5.0]: https://github.com/Sydney680928/mogwai/compare/v8.4.0...v8.5.0
[8.4.0]: https://github.com/Sydney680928/mogwai/compare/v8.3.0...v8.4.0
[8.3.0]: https://github.com/Sydney680928/mogwai/compare/v8.2.0...v8.3.0
[8.2.0]: https://github.com/Sydney680928/mogwai/compare/v8.1.0...v8.2.0
[8.1.0]: https://github.com/Sydney680928/mogwai/compare/v8.0.1...v8.1.0
[8.0.1]: https://github.com/Sydney680928/mogwai/compare/v8.0.0...v8.0.1
[8.0.0]: https://github.com/Sydney680928/mogwai/releases/tag/v8.0.0
