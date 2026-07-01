# GLOSSARY

## LANGUAGE FUNCTIONS

### `mogwai.reset`

Forces **MOGWAI** to perform a runtime reset.

***

### `mogwai.info`

Returns a record containing various information about the runtime and the system it runs on.

```
mogwai.info ?d
```

Will display:

```
name:                "MOGWAI CLI"
version:             "8.8.0.0"
platform:            "WINDOWS"
architecture:        "X64"
OSdescription:       "Microsoft Windows 10.0.26200"
framework:           ".NET 9.0.13"
runtimeID:           "win-x64"
prompt:              "MOGWAI RUNTIME 8.8.0.0...
primitives:          ('+' '-' '*' '/' 'sin' 'cos' 'tan' 'asin' 'acos' '...
externalKeywords:    ()
hostKeywords:        ('?s' 'run' 'edit' 'file.edit' 'file.select')
skills:              ('APP_GIZMO' 'TUI')
debug:               true
keepAlive:           true
isTask:              false
```

| Key                 | Meaning                                                                                 |
| ------------------- | --------------------------------------------------------------------------------------- |
| `name:`             | Runtime name.                                                                           |
| `version:`          | Runtime version.                                                                        |
| `platform:`         | Name of the platform on which the runtime runs.                                         |
| `architecture:`     | Platform architecture.                                                                  |
| `OSdescription:`    | Full platform description.                                                              |
| `framework:`        | .NET runtime version                                                                    |
| `runtimeID:`        | Platform runtime ID.                                                                    |
| `prompt:`           | **MOGWAI** runtime prompt.                                                              |
| `primitives:`       | List of available primitives.                                                           |
| `externalKeywords:` | List of available external keywords (functions provided by extensions).                 |
| `hostKeywords:`     | List of available host keywords (functions provided by the host).                       |
| `skills:`           | List of skills declared by the host and the engine (see `skills`).                      |
| `debug:`            | true if the runtime is in debug mode.                                                   |
| `extensions:`       | List of loaded extensions.                                                              |
| `keepAlive:`        | true if the **MOGWAI** runtime keeps its execution context from one session to another. |
| `isTask:`           | true if the **MOGWAI** runtime is a child task.                                         |

***

### `mogwai.exit`

Forces the runtime to stop the current execution without raising an error.

***

### `mogwai.halt`

Forces the runtime to stop the current execution and raises error MW.2 "halt encountered".

***

### `mogwai.assert`

Asserts that a condition is true. If the condition is false, raises error MW.9 (`assert error`) and stops execution. If `MOGWAI.onError` is defined, it is called automatically.

Takes two parameters: a condition and a message string.

The condition can be a **list** (automatically evaluated; after execution, exactly one value must have been pushed onto the stack — `MW.24` if not — and it must be a boolean — `MW.21` if not) or a **boolean** already on the stack. Any other type raises `MW.21` (bad argument type).

The message is used in the error display. It is not accessible programmatically — `error.last` returns `MW.9`.

```
# Condition as a list — evaluated by mogwai.assert
(a 10 ==) "a must equal 10" mogwai.assert

# Condition as a boolean already on the stack
a 0 >  "a must be positive" mogwai.assert
```

***

### `mogwai.assertSkill`

Asserts that a skill is available in the current execution context. If the skill is absent, displays the message and raises error **MW.9** (`assert error`), stopping execution. If `MOGWAI.onError` is defined, it is called automatically.

If the skill is present, `mogwai.assertSkill` is a no-op.

**Signature:** `name "message" mogwai.assertSkill`

The message is used in the error display. It is not accessible programmatically — `error.last` returns `MW.9`.

Typical use at the start of a script to assert that required skills are available:

```
'APP_GIZMO' "This script requires GIZMO to run." mogwai.assertSkill
'BLE' "This script requires BLE support." mogwai.assertSkill

# rest of the script...
```

***

### `mogwai.cclear`

Clears the cache of procedures included via the `mogwai.include` command.
<br>Ensures that the included code is the latest version.

***

### `mogwai.strict`

If `true` is passed as parameter, all variables must be declared before being used.<br>
Variables are declared with the `=>` function :

```
100 => 'A' # Declares variable A as .number type and assigns it the value 100.
```

> By default `mogwai.strict` is disabled. 

*** 

### `mogwai.isTask`

Returns `true` if the runtime is a child task (see task management).

***

### `mogwai.primitiveInfo`

Returns a record with information about a given primitive. Raises **MW.22** (bad argument value) if `name` does not match a known primitive.

**Signature:** `name mogwai.primitiveInfo → record`

| Key | Type | Description |
|---|---|---|
| `name:` | Name | The primitive's name. |
| `birth:` | String | The MOGWAI version in which the primitive was introduced. |

```
'calc' mogwai.primitiveInfo ?   # → [name: 'calc' birth: "8.12.0"]
```

***

### `mogwai.sendMessage`

Sends a message to the host. The message is a record containing at least the `type:` key.

```
"MY_EVENT" 567 mogwai.sendMessage
# Sends the following message to the host by the MessageReceivedFromRuntime delegate function.
# The number 567 is passed as parameter to the host.
# Task<EvalResult> MessageReceivedFromRuntime(Engine engine, string message, MOGObject parameter);
```

***

### `env.machineName`

Returns the name of the machine on which the runtime is running as a string. 

*** 

### `funcs`

Returns the list of defined user functions.

***

### `->`

Stores a value in a variable.

```
50 -> 'A'
```

***

### `->+`

Adds a value to a variable.

```
50 ->+ 'A'
```

***

### `->-`

Subtracts a value from a variable.

```
50 ->- 'A'
```

***

### `->*`

Multiplies a variable by a value:

```
50 ->* 'A'
```

***

### `->/`

Divides a variable by a value:

```
50 ->/ 'A'
```

***

### `++`

Increments a variable.

```
'A' ++
```

***

### `--`

Decrements a variable.

```
'A' --
```

***

### `&`

Pushes a direct reference to a variable onto the stack, instead of a copy of its value. Functions that support references modify the variable directly, without creating intermediate copies.

```
"bonjour" -> 'A'
&A ->upper
# A now contains "BONJOUR" — modified in place
```

> Not all functions support references. If you use `&` with a function that does not support it, a `bad argument type` error is raised.

***

### `-->`

Applies a list of transformations to a variable in place. Each item in the list is applied in sequence using the current value of the variable as input.

```
"bonjour" -> 'A'
(->upper butfirst butlast) --> &A
# A now contains "ONJOU"
```

Items in the list can be regular functions or quotations:

```
"hello world" -> 'A'
(->upper { " !" + }) --> &A
# A now contains "HELLO WORLD !"
```

The operation is **transactional**: a snapshot of the variable is taken before the pipeline starts. If any step raises an error, the variable is automatically restored to its original value and the error is propagated.

An empty list `()` is a no-op: the variable is left unchanged.

***

### `rcl`

Pushes the value of a variable whose name is passed as parameter onto the stack.

```
100 -> '$A'
'$A' rcl ?

# Displays 100
```

***

### `rclx`

Verbose form of the `&` sigil. Pushes a direct reference to a variable (whose name is passed as parameter) onto the stack, instead of a copy of its value. `'x' rclx` is equivalent to `&x`.

```
100 -> 'x'
'x' rclx # Pushes a reference to x, without copying its value
```

***

### `purge`

Deletes a variable whose name is passed as parameter.

```
'$A' purge
```

***

### `exists`

Returns `true` if the variable whose name is passed as parameter exists.

```
'$A' exists
```

***

### `eval`

Evaluates an object on the stack.

The behavior differs depending on the type of object evaluated:

- Functions and code blocks are executed. 
- Strings are updated with control characters and replacement blocks.
- Dynamic elements of a list are replaced by their current value.
- Dynamic elements of a record are replaced by their current value.

```
"Mr. X" -> 'name'
"The name is {! Name}" eval ?

# Displays "The name is Mr. X"

[x: 50 name: name] eval

# Pushes [x: 50 name: "Mr. X"] onto the stack
```

***

### `mogwai.include`

Includes and immediately executes code from a file.

```
"my code.mog" mogwai.include
```

***

### `mogwai.using`

Imports an extension library in ***MOGWAI*** format.

If the extension is in the `path.usings` directory, you can just specify its name (with a name object) without the path and extension.

```
'MOGWAI_SERIAL' mogwai.using
```

If the extension is not in the `path.usings` directory, you must specify its full name with path and extension (with a string object). 

```
"my extensions/MOGWAI_SERIAL.dll" mogwai.using
```

***

### `mogwai.usings`

Lists the usings performed and available.

***

### `get`

Returns the value of a key in a record or class instance, an element of a list or a byte array.

| Action                          | Result                 |
| ------------------------------- | ---------------------- |
| `(1 2 3 4) 1 get`               | will return 2          |
| `[x: 10 y: 20] x: get`          | will return 10         |
| `[x: 10 l: (1 2 3)] (l: 1) get` | will return 2          |
| `D:FFEA10 1 get`                | will return 234 (0xEA) |
| `$U1 name: get`                 | will return the value of the `name:` property of instance `$U1` |

When called on a class instance, `get` also executes the method if the key refers to a method rather than a property.

See also the compact `->` notation in [RECORDS](#records).

***

### `set`

Modifies the value of a key in a record or class instance, an element of a list or a byte array.

> **Breaking change (v8.6):** The parameter order has been updated for RPN consistency. The value to write is now the **first** parameter: `value container key: set`. Code using the previous order (`container key: value set`) must be updated.

| Action                      | Result                       |
| --------------------------- | ---------------------------- |
| `10 (1 2 3 4) 0 set`        | will return `(10 2 3 4)`     |
| `100 [x: 10 y: 20] x: set`  | will return `[x: 100 y: 20]` |
| `0xAA D:FFEA10 0 set`       | will return `D:AAEA10`       |
| `"DUPONT" &$U1 name: set`   | writes `"DUPONT"` into the `name:` property of instance `$U1` |

When writing to a class instance, `set` only accepts keys declared in the `public:` or `private:` sections of the class. Attempting to write to an undeclared key raises an error.

See also the compact `<-` notation in [RECORDS](#records).

***

### `size`

Returns the size of a record, list, data, binary or string.

***

### `keys`

Returns a list composed of the keys of a record.

```
[x: 10 y: 50 z: 100] keys 

# Will push (x: y: z:) onto the stack
```

***

### `first`

Returns the first element of a string, list or data.

***

### `last`

Returns the last element of a string, list or data.

***

### `butfirst`

Returns all elements except the first of a string, list or data.

***

### `butlast`

Returns all elements except the last of a string, list or data.

***

### `contains`

Returns `true` if an element is present in a string, record, list or data.

| Action                         | Result              |
| ------------------------------ | ------------------- |
| `"TOTO" "T" contains`          | will return `true`. |
| `[x: 50 y: 100] x: contains`   | will return `true`. |
| `(10 "EEE" 20 50) 20 contains` | will return `true`. |
| `D:FF00FFAB 0xFF contains`     | will return `true`. |

***

### `where`

Returns a list of all locations of an element in a string, list or data.

| Action                       | Result                    |
| ---------------------------- | ------------------------- |
| `"HELLO WORLD" "O" where`    | will return `(4 7)`       |
| `(10 100 40 10 24) 10 where` | will return `(0 3)`       |
| `D:45ED23FF0645DD 0x45`      | where will return `(0 5)` |

***

### `split`

Returns a list composed of elements of a string separated by a string containing the separator (which can consist of multiple characters).

| Action                  | Result                                |
| ----------------------- | ------------------------------------- |
| `"X1;X45;Z34;12" split` | will return `("X1" "X45" "Z34" "12")` |

***

### `join`

Recomposes a string from elements of a list and a separator.

> Inverse function of split.

| Action                             | Result                        |
| ---------------------------------- | ----------------------------- |
| `("X1" "X45" "Z34" "12") ";" join` | will return `"X1;X45;Z34;12"` |

### `like`

Returns `true` if a string matches a particular pattern.

```
?           = Any single character
*           = Zero or more characters
#           = Any digit (0 to 9)
[charlist]  = Any single character in charlist
[!charlist] = Any single character not in charlist
```

```
"MR SMITH 62" "M? SM*H ??" like

# Pushes true onto the stack
```

***

### `right`

Returns the last n characters of a string or bytes of a data.

```
"Hello world!" 6 right

# Pushes "world!" onto the stack

D:56231245 3 right # Pushes D:231245 onto the stack
```

*** 

### `left`

Returns the first n characters of a string or bytes of a data.  

```
"Hello world!" 6 left

# Pushes "Hello " onto the stack

D:56231245 3 left # Pushes D:562312 onto the stack
```

***

### `extract`

Extracts multiple elements from a list or data by specifying which elements to extract in a list.

```
(10 20 30 40) (0 1 3) extract 

# will return (10 20 40)

D:FF45AB23 (0 1 3) extract

# will return DATA:FF4523

[x: 10 y: 20 z: 100] (x: z:) extract 

# will return [x: 10 z: 100]
```

***

### `wait`

Suspends the runtime for a time expressed in milliseconds without blocking the processing of event and timer type messages.

***

### `post`

Posts a block of code to the engine's execution queue. The block executes in the next scheduler cycle, after pending events and timers — without creating an intermediate timer.

**Signature:** `post { ... }`

`post { }` with an empty block is valid — useful to let the scheduler process pending events without executing any additional code.

Functionally equivalent to `after 0 do { }`, with clearer intent.

The main use case is deferred execution from an event handler — for example to allow the TUI interface to refresh before a long computation. With `post`, the engine processes pending events before executing the block:

```
# Wait for a key press
while (console.getInputKey -1 ==) do
{
    post { }
}
```

```
# Post and do some work after other pending events
post
{
    # this runs after all currently pending events
    updateDisplay
}
```

***

### `rand`

Returns a random number between 0 and 1.

***

### `sub`

Extracts a part of a list, data or binary number by specifying the start and extent.

```
(10 20 30 40 50) 1 3 sub
# Pushes (20 30 40) onto the stack

D:05FFEDAB2312 2 3 sub 
# Pushes DATA:EDAB23 onto the stack

B:1001111001111 2 4 sub 
# Pushes B:0011 onto the stack
```

***

### `break`

Forces exit from a for, while, foreach, forever and during loop.

***

### `foreach...transform`

Iterates each element of a list, applies a transformation block to it, and returns a new list of the transformed elements.

The block executes on its **own isolated stack**, separate from the main stack. It has access to local and global variables, but cannot read from or write to the main stack. The value left on the block's stack at the end of each iteration becomes the corresponding element in the result list.

The loop variable name is specified between the `foreach` and `transform` keywords.

```
(1 2 3 4 5) foreach 'item' transform { item 2 * }
# Returns (2 4 6 8 10)

("L1" "L2" "L3") foreach 'item' transform { "-" item + }
# Returns ("-L1" "-L2" "-L3")
```

***

### `foreach...filter`

Iterates each element of a list, applies a predicate block to it, and returns a new list containing only the elements for which the block evaluates to `true`.

The block executes on its **own isolated stack**, separate from the main stack. It has access to local and global variables, but cannot read from or write to the main stack.

The loop variable name is specified between the `foreach` and `filter` keywords.

```
(1 2 3 4 5 6 7 8 9 10) foreach 'i' filter { i 5 >= i 8 <= and }
# Returns (5 6 7 8)

(1 2 3 4 5 6 7 8 9 10) foreach 'item' filter { item 2 mod 0 == }
# Returns (2 4 6 8 10)
```

***

### `return`

Forces exit from a function.

***

### `flags`

Returns the list of all active flags.

***

### `flag.set`

Activates the flag whose name is passed as parameter.

***

### `flag.clear`

Deactivates the flag whose name is passed as parameter.

***

### `flag.isSet`

Returns true if the flag whose name is passed as parameter is active.

***

### `flag.isClear`

Returns true if the flag whose name is passed as parameter is inactive.

***

### `unique`

Returns a unique code as a string.
<br>Ex: "DEC378AF69F246B6A1688799F70A987A"

***

### `guid`

Returns a unique code in UUID v4 (or GUID) format as a string. The value is randomly generated.
<br>Ex: "392BDA7A-9BEB-43B2-ACC7-05C8A06B0F44"

***

### `guid7`

Returns a unique code in UUID v7 format as a string. Unlike `guid` (UUID v4), UUID v7 is time-ordered: the first 48 bits encode the current Unix timestamp in milliseconds, making it monotonically increasing and suitable for use as a sortable, database-friendly identifier.
<br>Ex: "4843BAB6-6A90-4138-AC9F-DB7ABE0018CB"

***

### `json->`

Creates a list or record from a json formatted string.

***

### `->json`

Creates a json formatted string from a list or record.

***

### `->escape`

Escapes a string passed as parameter.

Quotes are replaced by `\"`, line breaks by `\r` and/or `\n`, etc… 

***

### `->unescape`

Unescapes a string passed as parameter (see ->escape).

***

### `error.last`

Returns the code of the last raised error.

***

### `error.reset`

Resets the code of the last raised error to "MW.0" (no error).

***

### `error.throw`

Artificially raises the error whose code is passed as parameter.

***

### `+`

Adds 2 objects together.

Possible combinations are:

- 2 numbers
- 1 list and an object
- 2 strings
- 1 data and a byte
- 2 data
- 2 lists

***

### `-`

Subtracts 2 numbers.

***

### `*`

Multiplies 2 numbers.

***

### `/`

Divides 2 numbers.

***

### `<`

Returns `true` if the first parameter is less than the second.

***

### `>`

Returns `true` if the first parameter is greater than the second.

***

### `<=`

Returns `true` if the first parameter is less than or equal to the second.

***

### `>=`

Returns `true` if the first parameter is greater than or equal to the second.

***

### `==`

Returns `true` if the first parameter is equal to the second.

***

### `!=`

Returns `true` if the first parameter is different from the second.

***

### `and`

Performs the logical AND operation between the first and second parameter.

***

### `or`

Performs the logical OR operation between the first and second parameter.

***

### `xor`

Performs the logical XOR operation between the first and second parameter.

***

### `not`

Performs the logical NOT operation between the first and second parameter.

***

### `isnull`

Returns `true` if the object passed as parameter is `null`.

***

### `isEmpty`

Returns `true` if the object on the stack is `empty`.

### `drop`

Removes the first element from the stack.

***

### `swap`

Swaps the first 2 elements of the stack.

***

### `dup`

Duplicates the first element of the stack.

***

### `depth`

Returns the number of elements in the stack.

***

### `clear`

Clears the stack.

***

### `sign`

Returns a list containing the type of the n elements of the stack without modifying the stack.

```
# Place elements on the stack
10 "EEE"

# Request the type of these 2 elements
2 sign

# The list (.string .number) is pushed onto the stack
```

***

### `->type`

Returns the type of the object passed as parameter.

***

### `->compress`

Returns a data that is the result of compressing a data passed as parameter.

***

### `->decompress`

Returns a data that is the result of decompressing a data passed as parameter. The data passed as parameter is normally the result of the `compress` function.

***

### `->pack`

Serializes an object passed as parameter and returns the result as a data.

***

### `->unpack`

Deserializes a data passed as parameter and returns the result as an object. The data passed as parameter is normally the result of the `->pack` function.

***

### `vars`

Returns the list of all existing global variables.

***

### `lvars`

Returns the list of all existing local variables.

***

### `console.print` or `??`

Displays a string on screen without a line break.

```
"Hello " console.print "world!" console.println

# Will display:
# Hello world!
```

***

### `console.println` or `?`

Displays a string on screen with a line break.

***

### `?d`

Displays lists, records and data on screen in a "clearer" version.

```
(10 20 30 40 50) ?d
```

Will display:

```
0 : 10
1 : 20
2 : 30
3 : 40
4 : 50
```

```
[x: 100 y: 50 z: "HELLO"] ?d
```

Will display:

```
x:  100
y:  50
z:  "HELLO"
```

```
D:5612FFEA1789AD34C5FAFEFF01021020ABACA0 ?d
```

Will display:

```
00000000  56 12 FF EA 17 89 AD 34 C5 FA FE FF 01 02 10 20  | V.ÿê.?­4Åúþÿ...   |
00000010  AB AC A0                                         | «¬               |
```

***

### `console.clear`

Clears the screen.

***

### `console.input`

Waits for keyboard input (ending with validation by the `ENTER` key) and returns the corresponding string.

***

### `console.prompt`

Like the `input` function but displays a prompt message passed as parameter.

```
"What is your first name? " console.prompt
"Your first name is: " swap + ?
```

Will display the prompt, then you can enter (for example STEPHANE)

```
What is your first name? STEPHANE
```

Then once the input (STEPHANE) is validated:

```
Your first name is: STEPHANE
```

***

### `console.show`

Shows the output console (if managed by the host).

> Has no effect in **MOGWAI CLI**.

***

### `console.hide`

Hides the output console (if managed by the host).

> Has no effect in **MOGWAI CLI**.

***

### `->list`

Builds a list from elements present on the stack. You must pass the number of elements to take as parameter. An error is raised if the stack does not contain enough elements.

```
10 20 30 40 50 5 ->list ?

# Pushes (10 20 30 40 50) onto the stack
```

***

### `->int`

Converts a number passed as parameter to an integer.

***

### `->str`

Converts an object passed as parameter to a string.

***

### `str.repeat`

Builds a new string by repeating a source string a given number of times.

**Signature:** `string count str.repeat`

The count must be a non-negative integer. A count of `0` returns an empty string. A negative count raises **MW.22** (bad argument value).

```
"E" 5 str.repeat ?    # → "EEEEE"
"ab" 3 str.repeat ?   # → "ababab"
"x" 0 str.repeat ?    # → ""
```

***

### `->format`

Converts a number to a string using a format.

```
50 "000" ->format ?
# Will display 050

50.8 "000.000" ->format ?
# Will display 050.800
```

***

### `->vars`

Extracts values and assigns them to locally created variables.

With a record, extracts the values of all keys and creates the corresponding local variables for the extracted keys:

```
[x: 10 y: 20 z: 50] ->vars 
"x={! x}" eval ? 
"y={! y}" eval ? 
"z={! z}" eval ?
```

Will display:

```
x=10
y=20
z=50
```

***

With the stack, extracts values and creates the corresponding local variables:

```
20 30 40 ('a' 'b' 'c') ->vars 
"a={! a}" eval ? 
"b={! b}" eval ? 
"c={! c}" eval ?
```

Will display:

```
a=20
b=30
c=40
```

***

### `->safeVars`

Verifies that the values present on the stack are as expected. 
You can check their number and type, and automatically assign local variables with stack values. An error is raised in case of non-compliance.

```
"EEE" 50 [x: .string y: .number] ->safeVars 
"x={! x}" eval ? 
"y={! y}" eval ?
```

Will display:

```
x=EEE
y=50
```

***

### `->params`

Allows passing named parameters (key/value pairs in a record) and verifying that expected parameters are present and their types match. 
If everything is correct, local variables corresponding to expected parameters are automatically created with their corresponding values.

```
[x: 100 y: "HELLO"] [x: .number y: .string] ->params 
"x={! x}" eval ? 
"y={! y}" eval ?
```

Will display:

```
x=100
y=HELLO
```

***

### `check`

Verifies that the n first elements of the stack are of the expected type.

```
10 "EEE" 20 4 (.number .number .string .number) check
# No error is raised because the 4 first elements of the stack are of the expected type.

10 "EEE" 20 4 (.string .number .string .number) check
# An error is raised :
# stack corruption error (MW.24)
# stack types expected (.string .number .string .number) but actually (.number .number .string .number)
```

***

### `->num`

Converts a string to a number when possible. 
If impossible, an error is raised.

***

### `->char`

Converts a number to a character according to the Unicode standard.

***

### `char->`

Converts a single string character to this unicode strandard code.

***

### `->name`

Converts a string or key to a name.

***

### `->key`

Converts a string or name to a key.

***

### `->data`

Takes n elements from the stack and makes a data. 
The number of elements to take is passed as parameter.

```
0xFF 0xAB 0x45 3 ->data

# Pushes D:FFAB45 onto the stack
```

***

### `->hex`

Converts a number to a string in hexadecimal format.

```
255 ->hex 

# Pushes "FF" onto the stack
```

***

### `hex->`

Converts a hexadecimal format string to a number.

```
"FF" hex->

# Pushes the number 255 onto the stack
```

***

### `->bin`

Converts a number to a binary object.

```
278 ->bin

# Pushes B:100010110 onto the stack
```

***

### `->bin8`

Converts a number to a binary object with 8 bits.

```
278 ->bin8 # Pushes B:100010110 onto the stack
```

***

### `->bin16`

Converts a number to a binary object with 16 bits.

```
278 ->bin16 # Pushes B:0000000100010110 onto the stack
```

*** 

### `->bin24`

Converts a number to a binary object with 24 bits.

```
278 ->bin24 # Pushes B:000000000000000100010110 onto the stack
```

***

### `->bin32`

Converts a number to a binary object with 32 bits.

```
278 ->bin32 # Pushes B:00000000000000000000000100010110 onto the stack
```

***

### `->bin48`

Converts a number to a binary object with 48 bits.

```
278 ->bin48 # Pushes B:000000000000000000000000000000000000000100010110 onto the stack
```

***

### `->bin64`

Converts a number to a binary object with 64 bits.

```
278 ->bin64 # Pushes B:0000000000000000000000000000000000000000000000000000000100010110 onto the stack
```

***

### `->upper`

Converts a string to uppercase.

***

### `->lower`

Converts a string to lowercase.

***

### `->function`

Converts a list or a string to a function.

```
( 2 2 + ) ->function
# Pushes « 2 2 + » onto the stack

" 2 2 + " ->function
# Pushes « 2 2 + » onto the stack
```

***

### `->primitive`

Converts a string to a **MOGWAI** primitive.

> Warning, the primitive is placed on the stack and is not automatically executed. To execute it, you must use the eval function.

***

### `->code`

Converts a list or a string to a code block. The code block is not executed, it is just pushed onto the stack. To execute it, you must use the eval function.

```
( 2 2 + ) ->code
# Pushes { 2 2 + } onto the stack

" 2 2 + " ->code
# Pushes { 2 2 + } onto the stack
```

***

### `->u8`

Converts a number to an unsigned 8-bit integer. 
The result is returned as a data.

***

### `->i8`

Converts a number to a signed 8-bit integer. 
The result is returned as a data.

***

### `->u16`

Converts a number to an unsigned 16-bit integer. 
The result is returned as a data.

***

### `->i16`

Converts a number to a signed 16-bit integer. 
The result is returned as a data.

***

### `->u32`

Converts a number to an unsigned 32-bit integer. 
The result is returned as a data.

***

### `->i32`

Converts a number to a signed 32-bit integer. 
The result is returned as a data.

***

### `->u64`

Converts a number to an unsigned 64-bit integer. 
The result is returned as a data.

***

### `->i64`

Converts a number to a signed 64-bit integer. 
The result is returned as a data.

***

### `->dataLE8` / `->dataLE16` / `->dataLE24` / `->dataLE32` / `->dataLE48` / `->dataLE64`

Converts a number to a DATA in **Little Endian** byte order, with the specified size in bits.

```
42 ->dataLE32   # → D:2A000000
42 ->dataLE16   # → D:2A00
42 ->dataLE48   # → D:2A0000000000
```

If the value is too large for the requested size, the most significant bytes are silently truncated.

***

### `->dataBE8` / `->dataBE16` / `->dataBE24` / `->dataBE32` / `->dataBE48` / `->dataBE64`

Converts a number to a DATA in **Big Endian** byte order, with the specified size in bits.

```
42 ->dataBE32   # → D:0000002A
42 ->dataBE16   # → D:002A
42 ->dataBE48   # → D:0000000000002A
```

If the value is too large for the requested size, the most significant bytes are silently truncated.

***

### `dataLE8->` / `dataLE16->` / `dataLE24->` / `dataLE32->` / `dataLE48->` / `dataLE64->`

Converts a DATA to a number, interpreting the bytes in **Little Endian** byte order, with the specified size in bits.

```
D:2A000000 dataLE32->   # -> 42
D:2A00 dataLE16->       # -> 42
```

***

### `dataBE8->` / `dataBE16->` / `dataBE24->` / `dataBE32->` / `dataBE48->` / `dataBE64->`

Converts a DATA to a number, interpreting the bytes in **Big Endian** byte order, with the specified size in bits.

```
D:0000002A dataBE32->   # -> 42
D:002A dataBE16->       # -> 42
```

***

### `->dataLE` / `->dataBE`

Dynamic-size variants of `->dataLEx` / `->dataBEx`. The size (in bits) is taken from the stack along with the number.

Supported sizes: 8, 16, 24, 32, 48, 64. Any other value raises a `BadArgumentTypeError`.

```
42 32 ->dataLE   # -> D:2A000000
42 32 ->dataBE   # -> D:0000002A
```

***

### `dataLE->` / `dataBE->`

Dynamic-size variants of `dataLEx->` / `dataBEx->`. The size (in bits) is taken from the stack along with the DATA.

Supported sizes: 8, 16, 24, 32, 48, 64. Any other value raises a `BadArgumentTypeError`.

```
D:2A000000 32 dataLE->   # -> 42
D:0000002A 32 dataBE->   # -> 42
```

***

### `->dataLE32F` / `->dataBE32F` / `->dataLE64F` / `->dataBE64F`

Converts a floating-point number to a DATA following the **IEEE 754** standard, in the specified byte order and size.

- `32F` variants use single precision (4 bytes).
- `64F` variants use double precision (8 bytes).

```
1.0 ->dataLE32F   # -> D:0000803F
1.0 ->dataBE32F   # -> D:3F800000
1.0 ->dataLE64F   # -> D:000000000000F03F
1.0 ->dataBE64F   # -> D:3FF0000000000000
```

***

### `dataLE32F->` / `dataBE32F->` / `dataLE64F->` / `dataBE64F->`

Converts a DATA to a floating-point number following the **IEEE 754** standard, interpreting the bytes in the specified byte order and size.

- `32F` variants expect at least 4 bytes.
- `64F` variants expect at least 8 bytes.

If the DATA is too small, a `BadArgumentValueError` is raised.

```
D:0000803F dataLE32F->   # -> 1.0
D:3F800000 dataBE32F->   # -> 1.0
D:000000000000F03F dataLE64F->   # -> 1.0
D:3FF0000000000000 dataBE64F->   # -> 1.0
```

***

### `utf8->`

Converts a data to a UTF-8 encoded string.

***

### `->utf8`

Converts a string to a UTF-8 encoded data.

***

### `ascii7->`

Converts a data to a 7-bit ASCII encoded string.

***

### `->ascii7`

Converts a string to a 7-bit ASCII encoded data.

***

### `ascii->`

Converts a data to a 8-bit ASCII encoded string.

### `->ascii`

Converts a string to an 8-bit ASCII encoded data.

***

### `->base64`

Converts a data to a base 64 encoded string.

***

### `base64->`

Converts a base 64 encoded string to a data.

***

### `->md5`

Returns the md5 hash of a data. 
The hash is provided as a data.

***

### `->sha1`

Returns the sha1 hash of a data.
The hash is provided as a data.

***

### `->sha256`

Returns the sha256 hash of a data.
The hash is provided as a data.

***

### `->sha512`

Returns the sha512 hash of a data.
The hash is provided as a data.

***

### `>>` and `<<`

Performs a bit shift on a number or binary object. 
The shift is passed as parameter. 
`>>` shifts bits to the right, `<<` to the left.

```
500 2 >>
# Pushes 125 onto the stack

B:01101111 2 >>
# Pushes B:00011011 onto the stack

B:01101111 2 <<
# Pushes B:10111100 onto the stack
```

***

### `~`

Inverts each bit of a number passed as parameter.

***

### `&`

Binary AND between 2 numbers passed as parameters.

***

### `|`

Binary OR between 2 numbers passed as parameters.

***

### `^`

Binary XOR between 2 numbers passed as parameters.

***

### `up`

Sets a particular bit of a binary object.

```
B:110001 2 up
# Pushes BIN:110101 onto the stack
```

***

### `down`

Clears a particular bit of a binary object.

```
B:110101 2 down
# Pushes BIN:110001 onto the stack
```

***

### `bit?`

Returns `true` if the bit at the specified position of a binary object is set (1), `false` otherwise. The position is zero-based, starting from the rightmost bit.

```
B:110011 0 bit?
# Pushes true onto the stack (rightmost bit is 1)

B:110011 1 bit?
# Pushes true onto the stack

B:110011 2 bit?
# Pushes false onto the stack
```

***

### `sin`

Returns the sine of an angle passed as parameter. The angle is in radians.

***

### `cos`

Returns the cosine of an angle passed as parameter. The angle is in radians.

***

### `tan`

Returns the tangent of an angle passed as parameter. The angle is in radians.

***

### `asin`

Returns the angle in radians whose sine is passed as parameter.

***

### `acos`

Returns the angle in radians whose cosine is passed as parameter.

***

### `atan`

Returns the angle in radians whose tangent is passed as parameter.

***

### `sinh`

Returns the hyperbolic sine of the number passed as parameter. Mirrors `Math.Sinh()` in .NET.

```
1.5 sinh ?   # → 2.1292794550948173
```

***

### `cosh`

Returns the hyperbolic cosine of the number passed as parameter. Mirrors `Math.Cosh()` in .NET.

```
1.5 cosh ?   # → 2.352409615243247
```

***

### `tanh`

Returns the hyperbolic tangent of the number passed as parameter. Mirrors `Math.Tanh()` in .NET.

```
0.9 tanh ?   # → 0.7162978701990245
```

***

### `asinh`

Returns the inverse hyperbolic sine of the number passed as parameter. Mirrors `Math.Asinh()` in .NET.

```
2.0 asinh ?   # → 1.4436354751788103
```

***

### `acosh`

Returns the inverse hyperbolic cosine of the number passed as parameter. Mirrors `Math.Acosh()` in .NET.

```
2.0 acosh ?   # → 1.3169578969248166
```

***

### `atanh`

Returns the inverse hyperbolic tangent of the number passed as parameter. Mirrors `Math.Atanh()` in .NET.

```
0.9 atanh ?   # → 1.4721842907995872
```

### `PI`

Returns the number PI.

***

### `->deg`

Returns the angle in degrees of an angle in radians passed as parameter.

```
PI 3 / ->deg
# Pushes 60 onto the stack
```

***

### `->rad`

Returns the angle in radians of an angle in degrees passed as parameter.

```
60 ->rad
# Pushes 1.0471975511965976 onto the stack
```

***

### `abs`

Returns the absolute value of the number passed as parameter.

***

### `+/-`

Inverts the sign of the number passed as parameter.

```
2 +/- # Pushes -2 onto the stack
```

***

### `sqrt`

Returns the square root of the number passed as parameter.

***

### `floor`

Returns the largest integral value less than or equal to the number passed as parameter.

***

### `ceil`

Returns the smallest integral value greater than or equal to the number passed as parameter.

***

### `pow`

Returns a number passed as parameter raised to the power passed as parameter.

```
50 3 pow
# Pushes 125000 onto the stack
```

***

### `mod`

Returns the remainder of integer division of one number by another.

```
65 3 mod ?
# Pushes 2 onto the stack
```

***

### `E`

Returns Euler's number (*e* = 2.718…). Complements `PI`.

```
E ?
# Pushes 2.718281828459045 onto the stack
```

***

### `round`

Returns the number passed as first parameter rounded to the number of decimal places passed as second parameter. When `n` is `0`, returns a whole number.

```
5.78934 3 round ?    # Pushes 5.789 onto the stack
45.324322 0 round ?  # Pushes 45 onto the stack
```

***

### `log`

Returns the natural logarithm (base *e*) of the number passed as parameter.

```
40 log ?
# Pushes 3.6888794541139363 onto the stack
```

***

### `log10`

Returns the base-10 logarithm of the number passed as parameter.

```
34 log10 ?
# Pushes 1.5314789170422551 onto the stack
```

***

### `exp`

Returns *e* raised to the power passed as parameter.

```
23 exp ?
# Pushes 9744803446.248903 onto the stack
```

***

### `gcd`

Returns the greatest common divisor of two integers, computed via the Euclidean algorithm. Both values are taken as absolute integers before processing.

```
345 4 gcd ?
# Pushes 1 onto the stack
```

***

### `lcm`

Returns the least common multiple of two integers. Both values are taken as absolute integers. Returns `0` if either argument is `0`.

```
345 4 lcm ?
# Pushes 1380 onto the stack
```

***

### `min`

Returns the smallest number present in a list. 

> Only numbers are allowed.

```
(56 34 9 27) min
# Pushes 9 onto the stack
```

***

### `max`

Returns the largest number present in a list. 

> Only numbers are allowed.

```
(1 56 34 9 27) max
# Pushes 56 onto the stack
```

***

### `sum`

Returns the sum of all numbers present in a list.

> Only numbers are allowed.

```
(1 56 34 9 27) sum
# Pushes 127 onto the stack
```

***

### `average`

Returns the average of all numbers present in a list.

> Only numbers are allowed.

```
(1 56 34 9 27) average ?
# Pushes 25.4 onto the stack
```

***

### `calc`

Evaluates a mathematical expression written in standard infix notation (e.g. `"5 * X + 2"` instead of RPN) and pushes the result onto the stack. Internally, the expression is converted to RPN using Dijkstra's Shunting-yard algorithm, then executed as regular MOGWAI code.

Supports the four arithmetic operators (`+ - * /`), parentheses, all MOGWAI primitives and constants (`sin`, `cos`, `sqrt`, `pow`, `PI`, `E`, …), multi-argument functions (e.g. `pow(x, y)`), local and global variables, and the `@`, `&` and `!` sigils.

> Useful for newcomers, or whenever an expression is more naturally expressed in infix form than in RPN.

```
500 -> 'X'
3.14 -> 'Y'
"5 * X + (7 + sin(Y))" calc ?
# Pushes 2507.0015926529068 onto the stack

"pow(2, 10)" calc ?
# Pushes 1024 onto the stack

"sin(PI / 3)" calc ?
# Pushes 0.8660254037844387 onto the stack
```

***

### `console.locate`

Requests the runtime host to position the cursor at the coordinates passed as parameter. 
The host is not obligated to respond. 

> MOGWAI CLI handles this function.

```
5 7 console.locate
```

***

### `console.cursor`

Returns the current cursor coordinates on the host screen. 
If the host does not handle this information, coordinates 0 0 are returned. 

> **MOGWAI CLI** handles this function.

***

### `console.setForegroundColor`

Requests the host to change the character display color by passing the color name to use as parameter.

Colors defined in **MOGWAI CLI** are:

- 'black'
- 'blue'
- 'cyan'
- 'gray'
- 'green'
- 'magenta'
- 'red'
- 'white'
- 'yellow'

```
'red' console.setForegroundColor
```

***

### `console.setBackgroundColor`

Requests the host to change the screen background color.

> In **MOGWAI CLI**, uses the same colors as for `console.setForegroundColor`.

***

### `console.getInputKey`

Requests the host to provide the code of the currently pressed key. -1 if no key is currently pressed.

***

### `console.width`

Returns the width of the console window in columns.

```
console.width ?   # → 120
```

***

### `console.height`

Returns the height of the console window in rows.

```
console.height ?   # → 30
```

***

### `http.get`

Performs an http get on a uri by specifying the necessary header values.

Parameters are passed via a record:

```
[
    uri: "https://api.github.com/orgs/dotnet/repos" 
    requestHeaders: [User-Agent: ".NET Foundation Repository Reporter" token: "XXXXX"]
] http.get
```

| Key              | Mandatory | Usage                                                          |
| ---------------- | --------- | --------------------------------------------------------------- |
| `uri:`           | Yes       | A string, the target URI.                                       |
| `requestHeaders:`| No        | A record mapping header names to string values.                 |

The response is a record containing the following keys:

| Key               | Usage                                                                                                        |
| ----------------- | -------------------------------------------------------------------------------------------------------------- |
| `state:`          | `true` if the request completed and the HTTP status code is a success code (2xx), `false` otherwise.            |
| `statusCode:`     | The status code actually returned (e.g. 200). May be absent for some network-level failures (e.g. DNS, TLS).    |
| `response:`       | A data containing the response body. Present whenever a response was actually received, success or HTTP error. |
| `responseHeaders:`| A record mapping each response header name to a list of values (always a list, even for a single value).        |
| `error:`          | A string describing the failure. Only present when `state:` is `false`.                                         |

```
[
    uri: "https://api.github.com/orgs/dotnet/repos" 
    requestHeaders: [User-Agent: ".NET Foundation Repository Reporter"]
] http.get -> 'result'

if (result->state:) then
{
    "OK - {! result->statusCode:}" eval ?
}
else
{
    "Failed - {! result->error:}" eval ?
}
```

### `http.head`

Performs an HTTP HEAD request on a uri. Identical to `http.get` but no response body is ever returned — only the headers. Useful for checking whether a resource exists or retrieving its metadata (size, content type, last modified date) without downloading its content.

| Key               | Mandatory | Usage                                                          |
| ----------------- | --------- | --------------------------------------------------------------- |
| `uri:`            | Yes       | A string, the target URI.                                       |
| `requestHeaders:` | No        | A record mapping header names to string values.                 |

The response is a record containing the following keys:

| Key               | Usage                                                                                                          |
| ----------------- | --------------------------------------------------------------------------------------------------------------- |
| `state:`          | `true` if the request completed and the HTTP status code is a success code (2xx), `false` otherwise.            |
| `statusCode:`     | The status code actually returned. May be absent for some network-level failures (e.g. DNS, TLS).               |
| `responseHeaders:`| A record mapping each response header name to a list of values (always a list, even for a single value).        |
| `error:`          | A string describing the failure. Only present when `state:` is `false`.                                         |

Note: `response:` is intentionally absent — HEAD never returns a body by definition.

```
[
    uri: "https://api.example.com/resource"
    requestHeaders: [User-Agent: "MOGWAI"]
] http.head -> 'result'

if (result->state:) then
{
    "Exists - statusCode: {! result->statusCode:}" eval ?
}
else
{
    "Failed - {! result->error:}" eval ?
}
```

### `http.post`

Performs an http post on a uri by specifying request headers, content headers and content.

All parameters are defined in a record passed as parameter:

```
[
    uri: "https://api.github.com/orgs/dotnet/repos" 
    requestHeaders: [ ]
    contentHeaders: [ ]
    content: DATA
] http.post
```

| Key               | Mandatory | Usage                                                          |
| ----------------- | --------- | --------------------------------------------------------------- |
| `uri:`            | Yes       | A string, the target URI.                                       |
| `content:`        | Yes       | A data, the request body.                                       |
| `requestHeaders:` | No        | A record mapping header names to string values.                 |
| `contentHeaders:` | No        | A record mapping content header names (e.g. `Content-Type`) to string values. |

The response, a record, is formatted exactly like that of the `http.get` function.

### `http.put`

Performs an http put on a uri by specifying request headers, content headers and content. Same parameters and response format as `http.post`.

```
[
    uri: "https://api.example.com/items/42" 
    contentHeaders: [Content-Type: "application/json"]
    content: {! "{\"name\":\"updated\"}" ->utf8 }
] http.put
```

### `http.patch`

Performs an http patch on a uri by specifying request headers, content headers and content. Same parameters and response format as `http.post`. Unlike `http.put`, which replaces a resource entirely, `http.patch` is meant for partial updates (only the fields to change need to be sent).

```
[
    uri: "https://api.example.com/items/42" 
    contentHeaders: [Content-Type: "application/json"]
    content: {! "{\"name\":\"updated\"}" ->utf8 }
] http.patch
```

### `http.delete`

Performs an http delete on a uri by specifying the necessary header values. No request body is sent.

| Key               | Mandatory | Usage                                                          |
| ----------------- | --------- | --------------------------------------------------------------- |
| `uri:`            | Yes       | A string, the target URI.                                       |
| `requestHeaders:` | No        | A record mapping header names to string values.                 |

The response, a record, is formatted exactly like that of the `http.get` function. A successful deletion often returns an empty `response:` (HTTP 204 No Content), which is a valid empty data, not an error.

```
[ uri: "https://api.example.com/items/42" ] http.delete
```

***

***

### `udp.send`

Sends a UDP datagram to a host/port. No response is expected (fire and forget).

| Key          | Mandatory | Usage                                              |
| ------------ | --------- | -------------------------------------------------- |
| `host:`      | Yes       | A string, the target IP address or hostname.       |
| `port:`      | Yes       | A number, the target port.                         |
| `data:`      | Yes       | A data, the datagram payload.                      |
| `localPort:` | No        | A number, the local port to bind to. An ephemeral port is used if absent. |

```
[
    host: "127.0.0.1"
    port: 5000
    data: {! "Hello from MOGWAI" ->utf8 }
] udp.send -> 'result'

if (result->state:) then
{
    "udp.send OK" ?
}
else
{
    "Failed - {! result->error:}" eval ?
}
```

The response is a record containing the following keys:

| Key      | Usage                                                     |
| -------- | --------------------------------------------------------- |
| `state:` | `true` if the datagram was sent successfully.             |
| `error:` | A string describing the failure. Only present when `state:` is `false`. |

***

### `udp.receive`

Listens on a local UDP port and waits for an incoming datagram.

| Key          | Mandatory | Usage                                              |
| ------------ | --------- | -------------------------------------------------- |
| `localPort:` | Yes       | A number, the local port to listen on.             |
| `timeout:`   | Yes       | A number, the maximum wait time in ms.             |

```
[
    localPort: 5001
    timeout: 3000
] udp.receive -> 'result'

if (result->state:) then
{
    result->data: ->utf8str ?
}
else
{
    "Failed - {! result->error:}" eval ?
}
```

The response is a record containing the following keys:

| Key           | Usage                                                                                      |
| ------------- | ------------------------------------------------------------------------------------------ |
| `state:`      | `true` if a datagram was received within the timeout.                                      |
| `data:`       | A data containing the received datagram payload.                                           |
| `remoteHost:` | A string, the IP address of the sender.                                                    |
| `remotePort:` | A number, the port of the sender.                                                          |
| `error:`      | A string describing the failure (`"timeout"` if no datagram was received in time). Only present when `state:` is `false`. |

***

### `udp.sendReceive`

Sends a UDP datagram and waits for a response in a single operation. This is the most common pattern for request/response protocols over UDP.

| Key          | Mandatory | Usage                                              |
| ------------ | --------- | -------------------------------------------------- |
| `host:`      | Yes       | A string, the target IP address or hostname.       |
| `port:`      | Yes       | A number, the target port.                         |
| `data:`      | Yes       | A data, the datagram payload.                      |
| `timeout:`   | Yes       | A number, the maximum wait time for a response in ms. |
| `localPort:` | No        | A number, the local port to bind to. An ephemeral port is used if absent. |

The response, a record, is formatted exactly like that of the `udp.receive` function.

```
[
    host: "127.0.0.1"
    port: 5000
    data: {! "Hello" ->utf8 }
    timeout: 3000
] udp.sendReceive -> 'result'

if (result->state:) then
{
    result->data: ->utf8str ?
}
else
{
    "Failed - {! result->error:}" eval ?
}
```

### `->uri`

Composes a uri from a record whose keys correspond to the different parts of a uri:

```
[
    url: "https://www.google.com" 
    path: "api/v0/login" 
    query: [id: "50" name: "DOE"]
] ->uri 

# Pushes "https://www.google.com:443/api/v0/login?id=50&name=DOE" onto the stack
```

***

### `->urlEncode`

Encodes a URL string passed as parameter. 

This function can be used to encode the entire URL, including query string values. 
URL encoding converts characters that are not allowed in a URL to character-entity equivalents. 
For example, when the characters < and > are embedded in a block of text to be transmitted in a URL, they are encoded as %3c and %3e.

***

### `->urlDecode`

Decodes a URL-encoded string. Inverse of `->urlEncode`.

**Signature:** `string ->urlDecode → string`

```
"Hello%20World" ->urlDecode ?    # → "Hello World"
```

See also: `->urlEncode`.

***

### `process.start`

Starts a process. 

Process information is provided via a record composed of the following keys:

| Key                 | Usage                                                               |
| ------------------- | ------------------------------------------------------------------- |
| `filename:`         | File to execute (e.g. notepad.exe)                                  |
| `arguments:`        | Arguments to use to start the process.                              |
| `workingDirectory:` | Sets the current directory for the process.                         |
| `wait:`             | If `true`, waits for the end of process execution before returning. |

> Only the `filename:` key is required.

```
[
    filename: "toto.exe" 
    arguments: "/u -K" 
    workingDirectory: "C:\...." 
    wait: true ] process.start
```

***

### `process.exec`

Launches a process, captures its output, and returns a result record.
Unlike `process.start`, `process.exec` always waits for the process to finish and captures `stdout` and `stderr`.

Process information is provided via a record composed of the following keys:

| Key                 | Usage                                                                  |
| ------------------- | ---------------------------------------------------------------------- |
| `filename:`         | File to execute (e.g. myservice.exe)                                   |
| `arguments:`        | Arguments to pass to the process.                                      |
| `workingDirectory:` | Sets the current directory for the process.                            |
| `input:`            | Optional string sent to the process via `stdin`. Omit if not needed.   |

> Only the `filename:` key is required.

Pushes a result record onto the stack:

| Key       | Type   | Description                                     |
| --------- | ------ | ----------------------------------------------- |
| `status:` | Number | Exit code returned by the process (0 = success) |
| `output:` | String | Content written to `stdout` by the process      |
| `error:`  | String | Content written to `stderr` by the process      |

```
[filename: "myservice.exe" arguments: "--mode calc" input: "42"] process.exec -> 'r'

r status: get -> 'code'
r output: get -> 'result'
r error:  get -> 'err'

if (code 0 ==) then
{
    "Result: {! result}" eval ?
}
else
{
    "Error: {! err}" eval ?
}
```

***

## DEBUGGING FUNCTIONS (used with MOGWAI STUDIO)

### `debug.write`

Requests the host and **MOGWAI STUDIO** (if connected) to display a message in the debug console.

```
"Debug message" debug.write
```

***

### `debug.clear`

Requests the host and **MOGWAI STUDIO** (if connected) to clear the debug screen.

***

### `debug.halt` or `¤`

Performs a pause. Corresponds to a breakpoint.

The program must be started in debug mode for the breakpoint to be taken into account. 
When execution reaches this instruction, the runtime pauses.
It is then possible to step through if necessary.

```
1 10 for 'i' do
{
    i ?
    100 wait

    # Place a breakpoint here
    debug.halt
}
```

***

### `debug.tron`

Activates tracing. The duration between each instruction is defined as parameter in milliseconds. 
If **MOGWAI STUDIO** is connected, it displays the currently executing instruction in real time.

```
250 debug.tron
```

***

### `debug.troff`

Deactivates tracing.

***

## TIME MANAGEMENT FUNCTIONS

### `now`

Returns the current date of your machine as a number representing the number of 100-nanosecond intervals that have elapsed since midnight, January 1, 0001.

For example, the number 6.389664359647076E+17 corresponds to the date 21/10/2025 at 11:39:56

***

### `->date`

Converts a numeric date to date and time components.

This function returns a record composed of the following keys:

| Key          | Usage                                                          |
| ------------ | -------------------------------------------------------------- |
| `day:`       | Day.                                                           |
| `month:`     | Month.                                                         |
| `year:`      | Year.                                                          |
| `hour:`      | Hours.                                                         |
| `minute:`    | Minutes.                                                       |
| `second:`    | Seconds.                                                       |
| `dayOfYear:` | Day number in the year.                                        |
| `dayOfWeek:` | Day number in the week.<br>(Sunday=0, Monday=1, …, Saturday=6) |

```
now ->date

# If today is 21/10/2025 at 11:51:29
# Pushes [day: 21 month: 10 year: 2025 hour: 11 minute: 51 second: 29 dayOfYear: 294 dayOfWeek: 2] onto the stack
```

***

### `date->`

Converts date and time components to a numeric date. The record passed as parameter contains the same keys as the record returned by the `->date` function.

```
[day: 21 month: 10 year: 2025 hour: 11 minute: 51 second: 29] date->
# Pushes 6.38966438969E+17 onto the stack
```

***

### `->duration`

Returns a duration as a record composed of the following keys:

| Key        | Usage                           |
| ---------- | ------------------------------- |
| `days:`    | Number of days elapsed.         |
| `hours:`   | Number of hours elapsed.        |
| `minutes:` | Number of minutes elapsed.      |
| `seconds:` | Number of seconds elapsed.      |
| `ms:`      | Number of milliseconds elapsed. |

Typically, to calculate the time elapsed between 2 moments, you can store the `now` at the start, then at the end subtract the start `now` from the current `now`, then use the `->duration` function to get the time elapsed between these 2 moments.

```
now 2500 wait now - abs ->duration

# For a total duration of 2 seconds and 507 milliseconds
# Pushes [days: 0 hours: 0 minutes: 0 seconds: 2 ms: 507] onto the stack
```

***

### `duration->`

Converts a duration record (see `->duration`) to a number of milliseconds.

```
[days: 0 hours: 0 minutes: 0 seconds: 2 ms: 507] duration->
# Pushes 25070000 onto the stack
```

***

### `->durations`

Converts a number of milliseconds to a list of durations in different units (ms, seconds, minutes, hours, days).

```
25070000 ->durations
# Pushes [totalDays: 2.9016203703703704E-05 totalHours: 0.0006963888888888889 totalMinutes: 0.04178333333333333 totalSeconds: 2.507 totalMs: 2507]) onto the stack
```

***

## TASK MANAGEMENT FUNCTIONS

### `task.start`

Launches the task whose name is passed as parameter without passing it any parameter. This function returns immediately.

Use `task.start` when the task requires no input. For tasks that need a parameter, use `task start with` instead.

```
'T1' task.start
```

***

### `task.wait`

Waits for the task whose name is passed as parameter for the end of its execution before returning.

***

### `task.isRunning`

Returns true if the child task whose name is passed as parameter is currently running.

***

### `task.stop`

Stops the task whose name is passed as parameter. The task is stopped as soon as possible, but it is not an immediate stop.

***

### `task.purge`

Deletes the task whose name is passed as parameter.

***

### `task.list`

Returns the list of names of all existing child tasks.

***

### `task.setResult`

Allows a child task to store its result. This function can only be used from within a child task's code. The result can be of any type managed by **MOGWAI**.

```
"MyResult" task.setResult
54 task.setResult
```

***

### `task.result`

Returns the result of the child task whose name is passed as parameter. By default, the result has value `null`.

***

### `task.name`

Returns the name of the child task. 

> This function can only be used from within a child task's code.

***

### `task.join`

Waits for all tasks listed as parameter to finish before returning.

```
('T1' 'T2' 'T3') task.join
```

***

### `task.publish`

Allows a child task to publish (send) a value to its parent task. 
The published value can be of any type managed by **MOGWAI**.

> This function can only be used from within a child task's code. 

```
"MyValue" task.publish

2345 task.publish
```

***

## EVENT MANAGEMENT FUNCTIONS

### `event.purge`

Removes handling of an event whose name is passed as parameter.

***

### `event.list`

Returns the list of all declared events being handled.

***

### `event.fire`

Triggers an event towards the runtime. 
Pass as parameters the event name, an object that accompanies the event and will be retrieved via the local variable `eventData` in the event code.

```
'MyEvent' "Hello" event.fire
```

***

## TIMER MANAGEMENT FUNCTIONS

### `timer.start`

Starts the timer whose name is passed as parameter.

***

### `timer.stop`

Stops the timer whose name is passed as parameter.

***

### `timer.purge`

Deletes the timer whose name is passed as parameter.

***

#### `timer.state`

Returns `true` if the timer is running.

***

### `timer.list`

Returns the list of all declared timers regardless of their status (running or stopped).

***

### `timer.state`

Returns `true` if the timer whose name is passed as parameter is active, `false` otherwise.

```
'timer1' timer.state # Pushes true if timer1 is active
```

***

### `DI`

Suspends triggering of all timers and events. 

> Warning, they are put on hold and will be executed when interrupts are enabled again.

***

### `EI`

Allows triggering of timers and events.

***

## FILE MANAGEMENT FUNCTIONS

**MOGWAI** version 8 introduces a completely redesigned file management system using a conventional path-based approach instead of the node-based system from previous versions.

### Path Management

### `path.programs`

Returns the standard programs folder path.

```
path.programs ?
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Programs"
```

***

### `path.files`

Returns the standard files folder path.

```
path.files ?
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Files"
```

***

### `path.usings`

Returns the standard extension libraries folder path.

```
path.usings ?
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Usings"
```

***

### `path.desktop`

Returns the current user's desktop folder.

***

### `path.documents`

Returns the current user's documents folder.

***

### `path.music`

Returns the folder where the current user's music files are stored.

***

### `path.videos`

Returns the folder where the current user's videos are stored.

***

### `path.pictures`

Returns the folder where the current user's pictures are stored.

***

### `path.programData`

Returns the system's 'ProgramData' folder.

***

### `path.tempDirectory`

Returns the temporary files folder.

***

### `path.tempFilename`

Returns a complete path to a new temporary file created by the system.

***

### `path.make`

Generates a file or folder path from a list of segments. 

Pass a list of path segments as parameter. The list can use auto-evaluation with the `!` character.

```
(! path.files "data.txt") path.make
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Files\data.txt"

(path.files "MyFolder" "report.txt") eval path.make
```

***

### `path.setPrograms`

Customizes the default programs folder path.

```
"C:\MyPrograms" path.setPrograms
```

***

### `path.setFiles`

Customizes the default files folder path.

```
"D:\MyData" path.setFiles
```

***

### `path.setUsings`

Customizes the default extension libraries folder path.

```
"C:\MyLibraries" path.setUsings
```

***

### `path.home`

Returns the home directory path. Defaults to the runtime's current directory (`Directory.GetCurrentDirectory()` at construction time) unless overridden by the host application or by `path.setHome`.

```
path.home ?
# Returns: "C:\Users\Username"
```

***

### `path.setHome`

Customizes the home directory path. The path is normalized via `Path.GetFullPath()`. Raises **MW.72** (`file operation error`) if the path cannot be resolved.

```
"C:\MyHome" path.setHome
```

***

### Directory Management

### `dir.exists`

Returns `true` if the folder exists at the specified path.

```
"C:\Temp" dir.exists
```

***

### `dir.create`

Creates a new folder at the specified path. Creates parent directories recursively if needed.

```
"C:\Temp\MyFolder\SubFolder" dir.create
```

***

### `dir.purge`

Deletes a folder and all its contents at the specified path.

```
"C:\Temp\MyFolder" dir.purge
```

***

### `dir.rename`

Renames a folder. Pass old path and new path as parameters.

```
"C:\Temp\OldName" "C:\Temp\NewName" dir.rename
```

***

### `dir.current`

Returns the current working folder path.

```
dir.current ?
# Returns: "C:\Projects"
```

***

### `dir.setCurrent`

Sets the current working folder.

```
"C:\Projects" dir.setCurrent
```

***

### `dir.directories`

Returns the list of subfolders in the specified folder path.

```
"C:\Temp" dir.directories
# Returns: ("Folder1" "Folder2" "Folder3")

path.files dir.directories
```

***

### `dir.files`

Returns the list of files in the specified folder path.

```
"C:\Temp" dir.files
# Returns: ("file1.txt" "file2.dat" "report.pdf")

path.files dir.files
```

***

### File Management - Complete Read/Write

### `file.data.read`

Reads all binary content of a file at once and returns it as DATA.

Pass the complete file path as parameter.

```
"C:\data.bin" file.data.read
(! path.files "image.png") path.make file.data.read
```

***

### `file.data.write`

Writes complete binary data to a file.

Pass the complete file path and the DATA as parameters.

```
"C:\MyFile.bin" DATA:FF45ABEA23 file.data.write
# Writes bytes 0xFF, 0x45, 0xAB, 0xEA and 0x23 to the file.

imageData (! path.files "copy.png") path.make file.data.write
```

***

### File Management - Sequential Operations with Handles

**A handle is a string** representing the unique hexadecimal identifier of the opened file stream (e.g., "A3F5B2C8"). This handle must be kept for all subsequent operations on the file.

### `file.open`

Opens a file for reading and returns a handle.

```
"data.txt" file.open -> 'handle'
(! path.files "report.txt") path.make file.open -> 'h'
```

***

### `file.create`

Opens a file for writing (clears the file if it exists) and returns a handle.

```
"report.txt" file.create -> 'handle'
(! path.files "output.txt") path.make file.create -> 'h'
```

***

### `file.append`

Opens a file for writing at the end (preserves existing content) and returns a handle.

Used for log files or adding content to existing files.

```
"log.txt" file.append -> 'handle'
(! path.files "debug.log") path.make file.append -> 'h'
```

***

### `file.read`

Reads up to `size` bytes from an open file and returns a DATA.

Pass the handle and size as parameters.

```
handle 1024 file.read
# Reads up to 1024 bytes from the file
```

***

### `file.readLine`

Reads a complete line (terminated by `\n` or `\r\n`) from an open file and returns a DATA.

Pass the handle as parameter.

```
handle file.readLine
# Returns the line as DATA (must be converted to string with utf8->, ascii->, etc.)

handle file.readLine utf8-> -> 'line'
```

***

### `file.write`

Writes data to an open file. **Does not** automatically add a line break.

Pass the DATA and handle as parameters. To write lines, manually add line break bytes (`D:0D0A` for Windows, `D:0A` for Unix/Linux).

```
"Hello" ->utf8 D:0D0A + handle file.write
# Writes "Hello" with a Windows line break

"Line without break" ->utf8 handle file.write
```

***

### `file.size`

Returns the total size (in bytes) of a file opened for reading.

Pass the handle as parameter.

```
handle file.size -> 'fileSize'
"File size: {! fileSize} bytes" eval ?
```

***

### `file.eof`

Returns `true` if the end of the file opened for reading is reached.

Pass the handle as parameter. Used in loops to read files sequentially.

```
while (handle file.eof not) do
{
    handle file.readLine utf8-> ?
}
```

***

### `file.close`

Closes an open file. **Always close files after use!**

Pass the handle as parameter.

```
handle file.close
```

***

### File Manipulation

### `file.exists`

Returns `true` if the file exists at the specified path, `false` otherwise.

```
"data.txt" file.exists
(! path.files "config.txt") path.make file.exists
```

***

### `file.info`

Returns a record containing all file metadata.

Pass the file path as parameter.

The record contains the following keys:

| Key               | Type    | Description                            |
| ----------------- | ------- | -------------------------------------- |
| `name:`           | String  | File name with extension               |
| `fullName:`       | String  | Full absolute file path                |
| `directoryName:`  | String  | Path of the folder containing the file |
| `extension:`      | String  | File extension                         |
| `modifiedTime:`   | Number  | Last modification date (.NET ticks)    |
| `lastAccessTime:` | Number  | Last access date (.NET ticks)          |
| `length:`         | Number  | File size in bytes                     |
| `isReadOnly:`     | Boolean | Read-only file                         |
| `isArchive:`      | Boolean | Archive attribute (Windows)            |
| `isHidden:`       | Boolean | Hidden file                            |
| `isSystem:`       | Boolean | System file                            |

```
"data.txt" file.info -> 'info'
info length: get -> 'size'
"File size: {! size} bytes" eval ?

# Convert timestamp to readable date
info modifiedTime: get ->date -> 'dateModif'
```

**Note**: Timestamps are in .NET ticks (number of 100-nanosecond intervals since 01/01/0001). Use the `->date` function to convert to a date record with `day:`, `month:`, `year:`, etc.

**Important**: If the file does not exist, `file.info` raises an error. Use `file.exists` to check existence before calling `file.info`.

***

### `file.copy`

Copies a file. Pass source path and destination path as parameters.

```
"source.txt" "dest.txt" file.copy
(! path.files "original.txt") path.make 
(! path.files "copy.txt") path.make 
file.copy
```

***

### `file.rename`

Renames a file. Pass old path and new path as parameters.

```
"old.txt" "new.txt" file.rename
(! path.files "temp.txt") path.make
(! path.files "backup.txt") path.make
file.rename
```

***

### `file.purge`

Deletes a file at the specified path.

```
"temp.txt" file.purge
(! path.files "old_data.bin") path.make file.purge
```

***

### Data Conversion Functions

Text file reading functions (`file.readLine`, `file.read`) return DATA (byte arrays) that must be converted to strings according to the file's encoding. Similarly, to write text to a file, strings must first be converted to DATA.

### `utf8->`

Converts a DATA to a string with UTF-8 encoding.

```
data utf8->
handle file.readLine utf8-> -> 'line'
```

***

### `ascii->`

Converts a DATA to a string with ASCII encoding.

```
data ascii->
handle file.readLine ascii-> -> 'line'
```

***

### `ascii7->`

Converts a DATA to a string with ASCII 7-bit encoding.

```
data ascii7->
```

***

### `->utf8`

Converts a string to DATA with UTF-8 encoding.

Used before writing text to a file.

```
"Hello" ->utf8
"Français: éèêë" ->utf8 D:0D0A + handle file.write
```

***

### `->ascii`

Converts a string to DATA with ASCII encoding.

```
"Hello" ->ascii
"English: Hello" ->ascii D:0D0A + handle file.write
```

***

### `->ascii7`

Converts a string to DATA with ASCII 7-bit encoding.

```
"Basic" ->ascii7
"ABC123" ->ascii7 D:0D0A + handle file.write
```

***

### Line Break Constants

When writing text files, line breaks must be added manually:

- `D:0D0A` - Windows line break (CR LF: Carriage Return + Line Feed)
- `D:0A` - Unix/Linux/Mac line break (LF: Line Feed only)

```
"My line" ->utf8 D:0D0A + handle file.write
```

The `+` operator concatenates DATA to create a single byte array.

***

## CLASS MANAGEMENT FUNCTIONS

### `class`

Sugar keyword used to define a class. Must be followed by the class name as a string, the `do` keyword, and a block containing `private:` and `public:` sections.

```
class 'Counter' do
{
    private:
    {
        _step: .number
    }

    public:
    {
        value: .number

        onInit:
        {
            [step: (.number 1)] ->params
            self->reset:
            step self<-_step:
        }

        increment:
        {
            self->value: self->_step: + self<-value:
        }

        reset:
        {
            0 self<-value:
        }
    }
}
```

Within a section, a name followed by a type sigil declares a **property** (initialized to `empty`). A name followed by a code block declares a **method**.

The `private:` section is accessible only from within the class. The `public:` section is accessible from outside.

Two special method names are reserved as optional lifecycle hooks: `onInit:` (called automatically on `new` if defined) and `onFree:` (called automatically on `free` if defined). They can be placed in either section.

***

### `new`

Creates a new instance of a class. If the class defines an `onInit:` method, it is called automatically with any value present on the stack. `onInit:` is optional.

```
# Without parameters
'Counter' new -> '$C'

# With named parameters (when onInit: uses ->params)
[step: 10] 'Counter' new -> '$C'
```

Each instance is assigned a unique internal handle noted `§N` (e.g. `§1`, `§2`). This number is never reused during the lifetime of the engine.

***

### `free`

Destroys a class instance. If the class defines an `onFree:` method, it is called automatically before destruction.

```
$C free
```

After `free`, any variable still holding a reference to the destroyed instance becomes invalid. Any attempt to use it raises an error.

***

### `isAlive`

Returns `true` if the instance reference on the stack is still valid (i.e. the instance has not been freed), `false` otherwise.

```
$U1 isAlive   # → true or false

if ($U1 isAlive) then
{
    $U1->display:
}
```

`isAlive` performs an O(1) lookup in the instance registry. It never raises an error when called on an instance reference — but passing a value that is not an instance reference raises MW.21 (bad argument type).

***

### `self`

Available inside any class method. Pushes the current instance reference onto the stack.

```
display:
{
    "USER={! self}" eval ?
    self->name: ?
}
```

Using `self` outside of a class method raises an error.

***

### `className:` (reserved property)

A read-only public property automatically available on every class instance. Returns the name of the class the instance belongs to.

```
$U1->className: ?   # → 'User'
```

Attempting to write to `className:`, or to declare it explicitly in a class definition, raises error MW.95 (reserved property).

***

### `alive`

Returns a list of all currently living instance references (`.objref`). Useful for iteration, debugging, or cleanup.

```
alive ?
# → (§1 §2 §3 ...)
```

You can filter by class using `foreach...filter`:

```
alive foreach 'item' filter { item->className: 'User' == } -> '$users'
```

If no instances are alive, returns an empty list `()`.

***

### `frame`

Returns a record describing the full structure of a named class — its public and private properties and methods.

```
'Counter' frame ?
# → [className: 'Counter' props: [value: .number] _props: [_step: .number] funcs: (onInit: increment: reset:) _funcs: ()]
```

The returned record contains the following keys:

| Key | Content |
|-----|---------|
| `className:` | Class name |
| `props:` | Public properties with their declared types |
| `_props:` | Private properties with their declared types |
| `funcs:` | Public method names |
| `_funcs:` | Private method names |

```
'Counter' frame -> '$F'
$F->className: ?
$F->props: ?
$F->_props: ?
$F->funcs: ?
$F->_funcs: ?
```

***

### `skills`

Returns the merged, deduplicated list of all skills declared in the current execution context (engine skills and host skills). Returns an empty list `()` if no skills are declared.

Skills are names that identify capabilities available in the host that embeds MOGWAI. They allow a script to verify it is running in the right environment before executing.

```
skills ?   # → ('APP_GIZMO' 'TUI' 'BLE')
```

See also: `hasSkill`, `mogwai.assertSkill`, `mogwai.info` (`skills:` key).

***

### `hasSkill`

Tests whether a skill is present in the current execution context. Returns `true` if the skill is available, `false` otherwise. Never raises an error.

**Signature:** `name hasSkill → bool`

```
'APP_GIZMO' hasSkill   # → true or false

# Conditional execution based on skill availability
if ('BLE' hasSkill) then
{
    # code that uses BLE...
}
```

***

### `ver?`

Returns `true` if the string is a valid version, `false` otherwise. A valid version string follows the `System.Version` format: `"major.minor"`, `"major.minor.revision"`, or `"major.minor.revision.build"`. A string with only a major component (e.g. `"8"`) is not valid.

Never raises an error.

**Signature:** `string ver? → bool`

```
"8.10" ver?       # → true
"8.10.1" ver?     # → true
"8.10.1.5" ver?   # → true
"8" ver?          # → false
"hello" ver?      # → false
```

See also: `ver>`, `ver<`, `ver>=`, `ver<=`, `ver==`, `ver!=`.

***

### `ver>` `ver<` `ver>=` `ver<=` `ver==` `ver!=`

Compares two version strings and returns a boolean. Version strings follow the `System.Version` format: `"major.minor"`, `"major.minor.revision"`, or `"major.minor.revision.build"`.

If either argument is not a valid version string, **MW.22** (bad argument value) is raised.

**Signature:** `"a" "b" ver> → bool` *(and similarly for all variants)*

| Primitive | Returns `true` if… |
|-----------|---------------------|
| `ver>`    | `a > b`             |
| `ver<`    | `a < b`             |
| `ver>=`   | `a >= b`            |
| `ver<=`   | `a <= b`            |
| `ver==`   | `a == b`            |
| `ver!=`   | `a != b`            |

```
"8.10.0.0" "8.2" ver>               # → true
"8.2" "8.10.0.0" ver<               # → true
"8.10.0.0" "8.10.0.0" ver==         # → true
"8.10.0.0" "8.9" ver!=              # → true

# Typical runtime version check
mogwai.info->version: "8.10" ver>=
if { ... }
```

See also: `ver?`, `mogwai.info`, `hasSkill`.

***

### `str.indexOf`

Returns the zero-based index of the first occurrence of a substring in a string. Returns `-1` if not found. Case-sensitive.

**Signature:** `string search str.indexOf → integer`

```
"HELLO" "L" str.indexOf ?     # → 2
"HELLO" "X" str.indexOf ?     # → -1
```

See also: `contains`, `str.startsWith`, `str.endsWith`.

***

### `str.startsWith`

Returns `true` if a string starts with the given prefix. Case-sensitive.

**Signature:** `string prefix str.startsWith → bool`

```
"MOGWAI" "MO" str.startsWith ?    # → true
"MOGWAI" "WAI" str.startsWith ?   # → false
```

See also: `str.endsWith`, `str.indexOf`, `contains`.

***

### `str.endsWith`

Returns `true` if a string ends with the given suffix. Case-sensitive.

**Signature:** `string suffix str.endsWith → bool`

```
"MOGWAI" "WAI" str.endsWith ?    # → true
"MOGWAI" "MO" str.endsWith ?     # → false
```

See also: `str.startsWith`, `str.indexOf`, `contains`.

***

### `str.replace`

Replaces all occurrences of a substring with another in a string. Case-sensitive.

**Signature:** `string old new str.replace → string`

```
"E;Y;5" ";" "--" str.replace ?    # → "E--Y--5"
"HELLO" "L" "R" str.replace ?     # → "HERRO"
```

***

### `str.trim`

Removes leading and trailing whitespace characters (spaces, tabs, `\r`, `\n`).

**Signature:** `string str.trim → string`

```
"  MOGWAI " str.trim ?    # → "MOGWAI"
```

See also: `str.trimStart`, `str.trimEnd`.

***

### `str.trimStart`

Removes leading whitespace characters (spaces, tabs, `\r`, `\n`).

**Signature:** `string str.trimStart → string`

```
" MOGWAI " str.trimStart ?    # → "MOGWAI "
```

See also: `str.trim`, `str.trimEnd`.

***

### `str.trimEnd`

Removes trailing whitespace characters (spaces, tabs, `\r`, `\n`).

**Signature:** `string str.trimEnd → string`

```
" MOGWAI " str.trimEnd ?    # → " MOGWAI"
```

See also: `str.trim`, `str.trimStart`.

***

### `str.padLeft`

Pads a string on the left with spaces to reach the specified width. Returns the string unchanged if already at or above `width`.

**Signature:** `string width str.padLeft → string`

```
"MOGWAI" 10 str.padLeft ?    # → "    MOGWAI"
"MOGWAI" 3 str.padLeft ?     # → "MOGWAI"  (unchanged, already longer)
```

See also: `str.padRight`.

***

### `str.padRight`

Pads a string on the right with spaces to reach the specified width. Returns the string unchanged if already at or above `width`.

**Signature:** `string width str.padRight → string`

```
"MOGWAI" 10 str.padRight ?    # → "MOGWAI    "
"MOGWAI" 3 str.padRight ?     # → "MOGWAI"  (unchanged, already longer)
```

See also: `str.padLeft`.

***

### `str.insert`

Inserts a string into another at a zero-based index. Raises **MW.22** if `index < 0` or `index > size of string`.

**Signature:** `string insertion index str.insert → string`

```
"HELLO LE MONDE" "-" 5 str.insert ?    # → "HELLO- LE MONDE"
```

***

### `str.remove`

Removes `count` characters from a string starting at zero-based index `start`. Raises **MW.22** if `start` or `count` are invalid.

**Signature:** `string start count str.remove → string`

```
"HELLO LE MONDE" 5 3 str.remove ?    # → "HELLO MONDE"
```

***

### `insert`

Inserts an element at a given position in a `list` or a `data`. An index equal to the collection's size appends at the end. Also works on references (`&var`) to a `list` or `data` variable, mutating it in place.

For `list`, any value can be inserted. For `data`, the inserted value must be a byte (`0`–`255`); raises **MW.22** if it isn't. In both cases, raises **MW.22** if the index is out of range (negative or greater than the collection's size).

**Signature:** `value collection index insert → collection`

```
"EEE" (1 2 3) 1 insert ?       # → (1 "EEE" 2 3)
0xAA D:FFFFFFFF 1 insert ?     # → D:FFAAFFFFFF

(1 2 3) -> 'L'
"EEE" &L 1 insert             # L is now (1 "EEE" 2 3)
```

***

### `sort`

Sorts a list in ascending order. Sorting only occurs if all elements of the list share the same type, and that type is one of `.string`, `.number`, `.name`, `.key` or `.word`. If the list contains elements of mixed types, it is returned unchanged. Also works on a reference (`&var`) to a list variable, sorting it in place.

**Signature:** `list sort → list`

```
(1 10 2 5) sort ?    # → (1 2 5 10)
```

***
