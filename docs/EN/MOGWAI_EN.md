# MOGWAI BASICS

## Table of Contents

- [INTRODUCTION](#introduction)
- [GETTING OFF TO A GOOD START](#getting-off-to-a-good-start)
- [DISPLAYING VALUES](#displaying-values)
- [SCREEN INPUT](#screen-input)
- [VARIABLES](#variables)
- [IN-PLACE VARIABLE MUTATION](#in-place-variable-mutation)
- [CONSTANTS](#constants)
- [TYPES](#types)
- [THE STACK](#the-stack)
- [TESTS](#tests)
- [LOOPS](#loops)
- [MATHEMATICAL FUNCTIONS](#mathematical-functions)
- [STRINGS](#strings)
- [CONVERSION FUNCTIONS](#conversion-functions)
- [LISTS](#lists)
- [RECORDS](#records)
- [BYTE ARRAYS](#byte-arrays)
- [ENDIANNESS CONVERSION](#endianness-conversion)
- [BINARY NUMBERS](#binary-numbers)
- [TIME MANAGEMENT](#time-management)
- [FUNCTION DECLARATION](#function-declaration)
- [ERROR HANDLING](#error-handling)
- [MAKING A PAUSE](#making-a-pause)
- [EXITING A FUNCTION, A LOOP OR THE PROGRAM](#exiting-a-function-a-loop-or-the-program)
- [AUTOMATIC VARIABLE CREATION](#automatic-variable-creation)
- [OBJECT EVALUATION](#object-evaluation)
- [FLAGS](#flags)
- [FILE MANAGEMENT](#file-management)
- [TIMERS](#timers)
- [EVENTS](#events)
- [OBJECT-ORIENTED PROGRAMMING](#object-oriented-programming)
- [TASKS](#tasks)
- [SKILLS](#skills)


# INTRODUCTION

I have been developing for a very long time and have had the opportunity to use many different technologies and very varied languages, but I believe the language that resonated with me the most is RPL.

RPL stands for Reverse Polish Lisp, and it's the name of a language created by HP for its scientific and financial calculators.
HP 48SX calculator programmable in RPL.

RPL is very similar to FORTH. Like FORTH, it uses a stack to take parameters and store results, and like FORTH, it uses Reverse Polish Notation (RPN, not to be confused with the RPL language, of course).

Thus in RPN we don't write `2+2` to perform an addition, but `2 2 +`, it's a bit confusing at first, but with this notation there's no need for parentheses or local variables (in theory).

Of course, entering RPL programs was done directly on the machine, and the ergonomics were not ideal, but HP had implemented a whole battery of tricks and functions to make the exercise bearable.

At first glance, RPL doesn't have a very simple syntax, but by digging into the subject we quickly realize the power it can unleash.
 
## An opportunity to seize
Actually, the idea of launching the development of **MOGWAI** came the day we needed, at work, to be able to simulate a Bluetooth Low Energy peripheral.

When developing a mobile application (that's my job) that uses Bluetooth Low Energy communication to communicate with a given device, the device in question doesn't exist yet because it must first be physically designed and its internal software must then be written, tested and validated.

This whole procedure takes time and generally, to avoid losing too much time, we start developing the mobile application well before the electronic board is able to exchange any information. Keeping the BLE communication part "for the end" is not a good idea.

Indeed, for ideal integration and so that as many people as possible can use the application with their eyes closed (communication included), the BLE dimension must be integrated from the beginning.

So we developed a tool that allows us to simulate the operation of a BLE-communicating device even before it exists. This allows us to validate the exchanges to be implemented well in advance and also allows us to realize very early on all the little things that had not been properly planned.

It is therefore a very important tool for obtaining a robust mobile application in terms of BLE communication. Moreover, it allows the electronic and embedded part to validate very early on crucial choices in terms of communication via Bluetooth Low Energy.

With this type of engine, the "deep" functions of the simulator remain very generic by performing all the necessary operations under the direction of code that is modifiable at will, in real time, without recompilation, because it is the execution engine code that will take care of the entire "logic" part of the simulation. It will be enough to store for each peripheral a set of scripts adapted to its operating mode and adapted to the tests to be carried out. The flexibility obtained was enormous!

The simulator must be able to execute very varied instructions. It must be able to generate the structure of the BLE peripheral to simulate, and also perform tasks that will make it react as if it were the real peripheral. For this, you ideally need to be able to "program" the simulator. And it's for this basic use that **MOGWAI** was developed. It's an execution engine that can be included in an application that needs to be "motorized".

The BLE simulator was the ideal project to launch the development of **MOGWAI**.

## A slow maturation

The first version of **MOGWAI** was developed in .NET Standard with the C# language. The **MOGWAI** library was included in the simulator that was developed in UWP. Since the simulator had to take the role of a BLE peripheral, a machine equipped with a BLE chip capable of supporting this role was needed (generally BLE chips in desktop PCs only know how to support the Central BLE role). Raspberry PI 3s are equipped with a BLE chip capable of taking both roles. By installing Windows 10 IOT on a Raspberry PI 3, we were able to run the first version of the simulator without any problem, motorized by the first version of **MOGWAI**. This tool saved us a lot of time at the time.

As the BLE simulator's needs grew, the **MOGWAI** engine was extended, improved, and many new features were added. Today **MOGWAI** can handle serial connections, HTTP requests, SQLite databases and has more than 200 primitives.

I'm now at version 6, still developed in C# for .NET. This allows it to be used on Windows, but also on Linux and Mac OSX with X86, X64 and ARM architectures. For example, **MOGWAI** runs natively on a Raspberry PI 3 under Raspbian (Linux ARM).

## MOGWAI CLI to use the language in interactive mode

To "play" with **MOGWAI** I developed an interactive console application that allows you to use all the features of the language. This application is called [**MOGWAI CLI**](https://github.com/Sydney680928/MOGWAI_CLI).

It is quite possible to write **MOGWAI** programs with a simple notepad, but it is still more pleasant to have appropriate development tools. [**MOGWAI Studio**](https://studio.mogwai.eu.com) is an IDE dedicated to **MOGWAI**.

# GETTING OFF TO A GOOD START

There is a reflex to adopt with **MOGWAI**, which is to place the `mogwai.reset` function as the first instruction of your programs.

It ensures you have an absolutely clean execution engine, no variables, no timers, no tasks, nothing at all.

For example, the **MOGWAI CLI** application that allows you to "play" with **MOGWAI** never resets the execution context, which means that everything you create as you type lines is kept, which allows you to chain commands to perform operations step by step to test.

So don't forget, to reset everything, use the `mogwai.reset` function.
 
# DISPLAYING VALUES

There are mainly 2 functions for displaying values on the screen.

`console.println` displays the object in position 1 on the stack and automatically performs a line break. To gain conciseness it is possible to use `?` instead.

`console.print` performs the same operation without automatic line break. This function can be replaced by `??`.

```
# We display the value 15 and the string "HELLO !" on 2 separate lines
15 ?
"HELLO !" ?

# We display the message "IT IS 2025" in 2 parts, a string and a number.
"IT IS " ??
2025 ?
```

To clear the screen, you must use the `console.clear` function.

To get the dimensions of the console window, use `console.width` and `console.height`, which return respectively the number of columns and the number of rows:

```
console.width -> '$w'
console.height -> '$h'
"Console: {! $w } x {! $h }" eval ?
```

# SCREEN INPUT

To enter data on the screen there are 2 functions, `console.input` and `console.prompt`.

The simplest is `console.input` which waits for keyboard input ending with a carriage return (`ENTER` key). The entered information is placed on the stack as a character string.

```
# We switch to input mode and store the result in the variable $X

console.input -> '$X'
```

The `console.prompt` function works exactly like input but it also allows you to display a prompt message. This message is placed on the stack before calling the `console.prompt` function.

```
# We ask for the name 
# And we store the information in the variable '$NOM"

"What is your name ? " console.prompt -> '$NOM'
```

# VARIABLES

Variables are defined by a name. If the name starts with the `$` symbol, it will be global, in all other cases, it will be local.

By default, a variable does not need to be declared to be used. The first assignment creates it if it doesn't already exist.

By default a variable has no predefined type, it takes the type of the last value assigned to it but it is possible to lock the type of a variable if a declaration is made prior to its use. It then takes the declared type and an error is raised if you try to assign it a value of another type.

Typed variables are declared with the `=>` function.

```
# We assign the numeric value 50 to the local variable 'A'
50 -> 'A'

# We then assign a character string to this variable
"Hello !" -> 'A'

# We assign the numeric value 500 to the global variable '$R'
500 -> '$R'

# We declare the global variable '$Z' with the type of the default value 0 (which is a number).
# Which will allow it to store numbers
0 => '$Z'

# We can now only store numbers in '$Z'
1500 -> '$Z'

# Otherwise an error is raised
"Hello !" -> '$Z'

# If we want to declare a variable that accepts storing any type 
# Of values, we must use the type .any with the 'empty' value as default value
empty => '$X'
1500 -> '$X'
"Hello !" -> '$X'
```

It is possible to make prior declaration of variables mandatory before using them. Simply use the `mogwai.strict` function with `true` or `false` to enable or disable this requirement.

```
# We activate the mandatory declaration of variables before using them 
true mogwai.strict
```

When a variable no longer needs to exist, it is possible to explicitly delete it using the `purge` function.

A local variable will be automatically deleted anyway when the code exits its scope.

If you try to delete a variable that doesn't exist, an error is raised.

```
# We delete the local variable A 
'A' purge
```

To place the value of a variable on the stack, simply invoke its name without apostrophes.

To speed up execution, you can use the `@` character to explicitly access a variable.

```
# We assign 'A' and 'B' with numbers.
20 -> 'A'
30 -> 'B'

# We perform the sum of the 2 variables and store the result in the variable 'C'
A B + -> 'C'

# Or use the @ character to speed up execution
@A @B + -> 'C'
```

To immediately evaluate the content of a variable, you can use the `!` prefix sigil. This is useful when a variable contains an object that embeds executable code, such as a block, a function, a string with interpolation blocks, a list or a record.

```
# A contains a number — !A behaves exactly like A
100 -> 'A'
!A    # → 100

# B contains a block — !B executes it immediately
{ A 10 * } -> 'B'
!B    # → 1000

# C contains a string with an interpolation block — !C resolves it
"The value of A is {! A}" -> 'C'
!C    # → "The value of A is 100"
```

For plain scalar types (numbers, booleans…), `!A` behaves identically to `A` — it is a silent no-op, no error is raised.

The four prefix sigils available for a variable are:

| Notation | Behavior |
|----------|----------|
| `A`      | Reads A and pushes its value onto the stack |
| `&A`     | Reference to A for in-place mutation |
| `@A`     | Statically resolved read (compile-time) |
| `!A`     | Evaluates the content of A directly |

With the `rcl` function, it is possible to place the value of a variable on the stack using its name.

```
# We retrieve the value of a variable via its name (with apostrophes).
100 -> 'A'
'A' rcl

# 100 is placed on the stack.
```

To store in a numeric variable the result of a mathematical operation on itself (like adding 1 to the value of variable X), there are 4 additional assignment functions:

`->+` Adds a number to a variable.

```
100 -> 'A'
10 ->+ 'A' 
# Now A equals 110.
```

`->-` Subtracts a number from a variable.

```
100 -> 'A'
10 ->- 'A' 
# Now A equals 90.
```

`->*` Multiplies a number and a variable.

```
100 -> 'A'
10 ->* 'A' 
# Now A equals 1000.
```

`->/` Divides a number and a variable.

```
100 -> 'A'
10 ->/ 'A' 
# Now A equals 10.
```

If the variable doesn't exist, it is created with the default value 0, the operation will then be performed from this value.

If the variable is not numeric, it is initialized as if it didn't exist before.

If the variable is not of numeric type and has been declared (locked type), an error is raised.

To save time there are also 2 functions to increment and decrement a numeric variable.

`++` Increments a variable.

```
100 -> 'A'
'A' ++
# Now A equals 101.
```

`--` Decrements a variable.

```
100 -> 'A'
'A' --
# Now A equals 99.
```

The vars function returns the list of all global variables used:

```
# We create 3 global variables $A, $B and $C

50 -> '$A'
100 -> '$B'
$A $B + -> '$C'

# We list the global variables used
vars

# Places the list ('$A' '$B' '$C') on the stack
```

The lvars function returns the list of all local variables used:

```
# We create 3 local variables A, B and C

50 -> 'A'
100 -> 'B'
A B + -> 'C'

# We list the local variables used

lvars

# Places the list ('A' 'B' 'C') on the stack
```

It is possible to check the existence of a variable with the `exists` function.

This function returns `true` if the variable name passed as parameter exists (local or global variable).

```
# We create 1 local variable A

50 -> 'A'

'A' exists

# Places true on the stack
```

# IN-PLACE VARIABLE MUTATION

When you push a variable's value onto the stack using `A` or `@A`, you push a **copy** of its content. Any transformation you apply produces a new value that must be explicitly stored back into the variable.

```
"bonjour" -> 'A'
A ->upper butfirst butlast -> 'A'
# A now contains "ONJOU"
```

For simple cases this works well, but for complex objects such as large lists, pushing and rebuilding copies on every operation can become costly. **MOGWAI** provides the `&` prefix to push the **direct reference** to a variable instead of a copy.

## The `&` reference prefix

Prefixing a variable name with `&` pushes the variable's actual content — not a copy — onto the stack. Any function that supports references will then modify the variable directly, without creating intermediate copies.

```
"bonjour" -> 'A'
&A ->upper
# A now contains "BONJOUR" — modified in place
```

Not all functions support references. If you use `&` with a function that does not support it, a `bad argument type` error is raised.

## The `-->` in-place pipeline operator

When you need to apply a sequence of transformations to a variable in place, repeating `&` before each step is verbose:

```
"bonjour" -> 'A'
&A ->upper  &A butfirst  &A butlast
# A now contains "ONJOU"
```

The `-->` operator solves this by applying an entire list of transformations to a variable in a single expression:

```
"bonjour" -> 'A'
(->upper butfirst butlast) --> &A
# A now contains "ONJOU"
```

Each item in the list is applied in sequence, using the current value of `A` as input. The variable is updated after each step.

### Using quotations in the pipeline

The items in the list can be regular functions or quotations. A quotation receives the current value of the variable on its stack and can perform any operation, as long as the final result is left on the stack:

```
"hello world" -> 'A'
(->upper { " !" + }) --> &A
# A now contains "HELLO WORLD !"
```

### Transactional behavior

The `-->` operator is **transactional**. Before the pipeline starts, a snapshot of the variable is taken. If any step raises an error, the variable is automatically restored to its original value and the error is propagated.

```
"bonjour" -> 'A'
guard
{
    (->upper sqrt butlast) --> &A
}
else
{
    # An error was raised on ->sqrt (not applicable to a string)
    # A has been restored to its original value
    A ?  # displays "bonjour"
}
```

### Empty pipeline

An empty list `()` is a no-op: the variable is left unchanged.

```
"bonjour" -> 'A'
() --> &A
# A still contains "bonjour"
```

# TYPES

**MOGWAI** manipulates objects with different types.

Each type has a name that starts with a dot. For example, the type corresponding to a character string is named `.string`.

The `->type` function allows you to retrieve the type of the object on the stack.

```
# The type of a number is .number
1567 ->type ?

# We can test the type of a variable and make decisions accordingly
234 -> 'A'
if (A ->type .number ==) then {"A is a number" ?} else {"A is not a number" ?}
```

The main types manipulated by **MOGWAI** are as follows:

| Name | Type | Example |
|------|------|---------|
| `.number` | Number (double precision real) | 154 or -56.34 |
| `.string` | Character string | "Hello world" |
| `.boolean` | Boolean value | true / false |
| `.list` | List of objects | (5 "X1" 12.78) |
| `.code` | Code block | {2 2 + ?} |
| `.function` | Function | «2 2 + ?» |
| `.name` | Symbolic name | 'A' |
| `.key` | Key used in a RECORD | latitude: |
| `.data` | Byte array | DATA:FF3456ED23 |
| `.binary` | Binary number | BIN:110011110011 |
| `.record` | RECORD (dictionary) | [x: 50 y: 200] |
| `.null` | Null value | null -> 'A' |
| `.ref` | Reference to a variable | &A |
| `.objref` | Reference to a class instance | §56 |
| `.any` | Free type (variant) | |


# THE STACK

**MOGWAI** is a language that uses a LIFO stack to provide parameters to functions and retrieve results. 
You can place any object manipulated by **MOGWAI** on the stack (see the TYPES chapter).

For example, when you write `2 8 +` to perform an addition, **MOGWAI** will perform a series of operations during execution:

1. Place 2 on the stack (2 is in position 1).
2. Place 8 on the stack (8 is in position 1, and 2 in position 2).
3. Execute the `+` function which will take the 2 values at the top of the stack, add them and place the result on the stack.

In the end on the stack, 2 and 8 have disappeared (we say they were consumed by the `+` function), replaced by the result of their sum (the value 10).


## Stack manipulation functions

The stack can be manipulated because in certain cases it's very practical. This often avoids using intermediate local variables. In the end the code is faster.

For example, if you want to perform a calculation, display the result, then perform another calculation from this result and display it too, theoretically you need an intermediate variable:

```
# We do a 1st calculation and display it.
# But we must keep a trace of the result for another calculation later.

# We do the 1st calculation and store the result in A.
2 7 + -> 'A' 

# We display the result of the 1st calculation.
A ?

# We do the second calculation from the result of the previous calculation which we display immediately.
A 200 * ?
```

By manipulating the stack we can avoid the intermediate variable and make the code more compact and faster. For this we will use the `dup` function which duplicates the 1st element of the stack:

```
# We do the 1st calculation and duplicate the result to display it.
# Then we do the second calculation from the result of the previous calculation which we display.

2 7 + dup ?
200 * ?
```

## Available stack functions

| Function | Action                                                                 |
|:--------:|------------------------------------------------------------------------|
| `dup`    | Duplicates the 1st element of the stack.                                    |
| `swap`   | Swaps the 1st and 2nd element of the stack.                      |
| `clear`  | Empties the stack.                                                          |
| `depth`  | Places the stack size at the time of the request on the stack.         |
| `drop`   | Removes the 1st element from the stack.                                    |

 
## The `sign` function

It is possible to determine the type of stack elements without removing the elements from the stack. The `sign` function which takes as parameter the number of elements to inspect returns a list containing the types of the inspected elements.

```
# We place 3 values of different types on the stack

10 "EE" (1 2)

# We inspect these 3 values

3 sign

# sign places the list (.list .string .number) on the stack
# Which correspond to the types of the elements present on the stack
# In position zero in the list the type of the last element placed on the stack
```

If we try to inspect more elements than are actually present on the stack, the `sign` function returns an empty list.

The `sign` function is very useful to verify, without modifying the stack, that the parameters present are indeed of the expected type.

# TESTS

## The `if` instruction

`if` allows you to perform tests and make decisions.

When the test is positive, a code block is executed. It is also possible to define a code block to execute when the test is negative.

```
50 -> 'A'

if (A 50 ==) then 
{
    "A has a value of 50" ?
}
else
{
    "A does not have a value of 50" ?
}
```

It is imperative that the test clause (the code placed between parentheses) places a boolean value on the stack. If this is not the case, an error is raised.

```
# This expression will work 
if (true) then {"TRUE !" ?} else {"FALSE !" ?}

# This expression will raise an error
if ("TOTO") then {"TRUE !" ?} else {"FALSE !" ?}
```

## Boolean logical operations (return `true` or `false`)

| Test      | Meaning             |
|-----------|---------------------------|
| `X Y ==`  | X equal to Y?             |
| `X Y !=`  | X different from Y?        |
| `X Y >`   | X greater than Y?         |
| `X Y <`   | X less than Y?         |
| `X Y >=`  | X greater than or equal to Y? |
| `X Y <=`  | X less than or equal to Y? |
| `X not`   | Logical inversion of X    |
| `X Y or`  | X OR Y                    |
| `X Y and` | X AND Y                    |
| `X Y xor` | EXCLUSIVE OR between X and Y  |

 
## Binary logical operations (return a number)


| Test      | Meaning             |
|-----------|---------------------------|
| `X Y &`   | Binary AND                |
| `X Y |`   | Binary OR                |
| `X Y ^`   | Binary EXCLUSIVE OR       |
| `X Y ~`   | Binary NOT               |


## The `switch` instruction

To avoid cascading `if .. else` you can use the `switch` instruction.

This instruction is composed of several test / code block pairs.

At the 1st test encountered that returns `true`, its code block is executed and only that one.

```
# We want to display a message according to the value of the variable 'a'

150 -> 'a'

switch 
{
    (a 100 <) then
    { 
        "100" ?
    }
	
    (a 200 <) then
    { 
        "< 200" ?
    }
	
    (true) then
    { 
        "DEFAULT" ?
    }
}
```

If you absolutely want to have a code block that executes even if no other is selected (a sort of default block), simply put a block at the end whose test cannot fail (ideally we put `true` directly in the test).

# LOOPS

## `repeat` loop

To execute a code block a certain number of times, you must use `repeat`.

```
# We will display the numbers from 1 to 10
# The variable 'I' serves as the loop counter

0 -> 'I'

10 repeat
{
    'I' ++
    I ?
}

# We will display the numbers from 1 to 10
# The variable 'I' serves as the loop counter
# We exit the loop when 'I' equals 5

0 -> 'I'

10 repeat
{
    'I' ++
    I ?

    if (I 5 ==) then {break}
}
```

## `during` loop

To execute a code block for a certain duration, you must use `during`.

The duration is expressed in milliseconds (1000 = 1 second).

```
# We will execute the code for 10 seconds

0 -> 'I'

during 10000 do 
{
    'I' ++
    I ?
}
```

## `for` loop

To use an automatically managed loop counter, you must use `for`.

```
# We will display the numbers from 1 to 10
# The variable 'I' serves as the loop counter


1 10 for 'I' do
{
    I ?
}

# We will display the numbers from 10 to 1
# The variable 'I' serves as the loop counter


10 1 for 'I' step -1 do
{
    I ?
}

# We will display the numbers from 10 to 1
# The variable 'I' serves as the loop counter
# When we reach the value 5 we exit the loop


10 1 for 'I' step -1 do
{
    I ?

    if (I 5 ==) then {break}
}
```

## `foreach...do` loop

To iterate each element of a list or a data, you must use `foreach...do`.

The block executes on the **main stack**: it has full access to whatever is already on the stack, and anything it leaves on the stack remains there after the loop.

```
# We display each element of the list

("L1" "L2" "L3" "L4" "L5" "L6" "L7") foreach 'item' do { item ? } 

# We display each element of the data

D:01020304 foreach 'item' do { item ? } 
```

## `foreach...transform` loop

To transform each element of a list, you must use `foreach...transform`.

The block executes on its **own isolated stack**, separate from the main stack. It has access to local and global variables, but cannot read from or write to the main stack. The value left on the block's stack at the end of each iteration becomes the transformed element in the result list.

```
# We transform each element of the list

("L1" "L2" "L3" "L4" "L5" "L6" "L7") foreach 'item' transform { "-" item + } 
# Returns the list ("-L1" "-L2" "-L3" "-L4" "-L5" "-L6" "-L7")

(1 2 3 4 5) foreach 'item' transform { item 2 * } 
# Returns the list (2 4 6 8 10)
```

## `foreach...filter` loop

To filter the elements of a list, you must use `foreach...filter`.

The block executes on its **own isolated stack**, separate from the main stack. It has access to local and global variables, but cannot read from or write to the main stack. The block must leave a boolean value on its stack: only the elements for which the block returns `true` are collected into a new list, which is pushed onto the main stack.

```
# We keep only the even numbers

(1 2 3 4 5 6 7 8 9 10) foreach 'item' filter { item 2 mod 0 == }
# Returns the list (2 4 6 8 10)

# We keep only the elements between 5 and 8 inclusive

(1 2 3 4 5 6 7 8 9 10) foreach 'i' filter { i 5 >= i 8 <= and }
# Returns the list (5 6 7 8)
```

The same result can be achieved with `foreach...do` by managing an accumulator manually, but `foreach...filter` expresses the intent more directly and concisely.

## `forever` loop

To execute a loop indefinitely, you must use `forever`.

```
# We execute the following code indefinitely

0 -> 'I'

forever do {'I' ++ ?}

# We execute the following code indefinitely
# But we exit when 'I' has the value 456

0 -> 'I'

forever do 
{
    'I' ++
    I ?

    if (I 456 ==) then {break}
}
```

## `while` loop

To execute a code block as long as a condition is true, you must use `while`.

With this notation (while at the beginning of the loop), the test is performed first:

```
# As long as I is less than 100 we display it

0 -> 'I'

while (I 100 <) do
{
    'I' ++
    I ?
}
```

## `do… while` loop

To execute a code block as long as a condition is true, you must use `do … while`.

With this notation, the loop code is executed and the test is performed at the end:

```
# As long as I is less than 100 we display it

0 -> 'I'

do
{
    'I' ++
    I ?
} while (I 100 <)
```
 
# MATHEMATICAL FUNCTIONS

| Function | Usage                                                                                                                                                         | Example        |
|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------|
| `->deg`  | Converts a radian angle to degrees.                                                                                                                            | `0.05 ->deg`   |
| `->rad`  | Converts a degree angle to radians.                                                                                                                            | `3.14 ->rad`   |
| `+`      | Adds 2 numbers.                                                                                                                                             | `5 7 +`        |
| `-`      | Subtracts 2 numbers.                                                                                                                                          | `5 7 -`        |
| `*`      | Multiplies 2 numbers.                                                                                                                                          | `5 7 *`        |
| `/`      | Divides 2 numbers.                                                                                                                                             | `5 7 /`        |
| `abs`    | Returns the absolute value of a number.                                                                                                                       | `-56 abs`      |
| `acos`   | Returns the arc cosine of an angle in radians.                                                                                                                  | `0.5 acos`     |
| `asin`   | Returns the arc sine of an angle in radians.                                                                                                                    | `0.5 asin`     |
| `atan`   | Returns the arc tangent of an angle in radians.                                                                                                                 | `0.5 atan`     |
| `ceil`   | Returns the value of the smallest integer greater than or equal to the specified number.                                                                                 | `56.89 ceil`   |
| `cos`    | Returns the cosine of an angle in radians.                                                                                                                     | `0.5 cos`      |
| `max`    | Returns the maximum value of a list.<br> Only numbers are allowed.| `(1 2 3) max`  |
| `average`   | Returns the average of a list.<br> Only numbers are allowed.| `(1 2 3) mean` |
| `min`    | Returns the minimum value of a list.<br> Only numbers are allowed.| `(1 2 3) min`  |
| `pow`    | Returns a specified number raised to the specified power.                                                                                                   | `100 2 pow`    |
| `rand`   | Generates a random number between 0 and 1.                                                                                                              | `rand ->'A'`   |
| `>>`  | Performs a bit shift on a specified number.<br>The shift is performed to the right| `100 4 >>`  |
| `<<`  | Performs a bit shift on a specified number.<br>The shift is performed to the left| `100 4 <<`  |
| `sin`    | Returns the sine of an angle in radians.                                                                                                                       | `0.5 sin`      |
| `sqrt`   | Returns the square root of a number.                                                                                                                        | `16 sqrt`      |
| `sum`    | Returns the sum of a list.<br> Only numbers are taken into account.<br> Returns null if the list contains no numbers.                             | `(1 2 3) sum`  |
| `tan`    | Returns the tangent of an angle in radians.                                                                                                                    | `0.5 tan`      |
| `PI`     | Returns PI in degrees.                                                                                                                                         | `PI`           |
| `floor`  | Returns the largest integral value less than or equal to the specified number.                                                                              | `45.8 floor`   |
| `mod`    | Returns the remainder of the integer division of one number by another.                                                                                            | `100 3 mod`    |

 
# STRINGS

**MOGWAI** has many character string processing functions.

## Concatenation

The `+` function allows you to concatenate 2 character strings.

This function has a certain "intelligence" because depending on the context it knows how to adapt.


| Operation               | Result         |
|-------------------------|------------------|
| `"HELLO " "LE MONDE" +` | "HELLO LE MONDE" |
| `"HELLO" 3 +`           | "HELLO3"         |
| `3 "HELLO" +`           | "3HELLO"         |

## Extraction

There are several functions to extract part of a character string.

| Operation                   | Result        |
|-----------------------------|-----------------|
| `"HELLO LE MONDE" 0 5 sub`  | "HELLO"         |
| `"HELLO LE MONDE" butfirst` | "ELLO LE MONDE" |
| `"HELLO LE MONDE" butlast`  | "HELLO LE MOND" |
| `"HELLO LE MONDE" first`    | "H"             |
| `"HELLO LE MONDE" last`     | "E"             |
| `"HELLO LE MONDE" 3 left`   | "HEL"           |
| `"HELLO LE MONDE" 3 right`  | "NDE"           |

## Size

To retrieve the size of a character string, you must use the `size` function.

```
# We retrieve the size of a character string and display it

"HELLO LE MONDE" size ?
```

## Finding elements

To search for a substring in a character string, you must use the `where` function which returns a list composed of all corresponding positions.

```
# We search for the location of all the letters "E"

"HELLO WORLD" "O" where

# The answer will be the list (4 7)
```

## Transformations

To transform a character string you can use the following functions:

| Operation                  | Result           |
|----------------------------|------------------|
| `"HELLO WORLD" ->lower`    | "hello world"    |
| `"hello world" ->upper`    | "HELLO WORLD"    |
| `("X" "Y" "Z") ";" join`   | "X;Y;Z"          |
| `"X;Y;Z" ";" split`        | ("X" "Y" "Z")    |

## Formatting a number

It is possible to format a number using the `->format` function which takes as parameters the number to format and the format to apply.

The format to apply is a character string describing what form the number should take:

| Operation                | Result |
|--------------------------|----------|
| `50.678 "0.00" ->format` | "50.68"  |
| `34 "000" ->format`      | "034"    |

```
# We display the current date in dd/mm/yyyy format
# See the DATE MANAGEMENT chapter to fully understand the following code.

now ->date -> 'dt'

dt day: get "00" ->format ??
"/" ??
dt month: get "00" ->format ??
"/" ??
dt year: get "0000" ->format ?
```

## Including values in a string

It is possible to include directly in a string elements from variables or functions. 
It is thus possible to compose a string very easily without having to perform tedious element-by-element construction operations.
To indicate the location of an element to incorporate into a character string, you must use the self-evaluated code block notation.

For example, to incorporate the contents of the `name` variable, simply write:

`"The name is {! name}" eval`

It is the `eval` function that will take the string and replace all incorporated elements with their true value.
If the evaluation of an incorporated element causes an error (non-existent variable, erroneous code), the replacement of this element is not performed.

In our example, if the `name` variable contains `"DOE John"` the evaluation will give:

`"The name is DOE John"`

You can also place code. For example, you can display the name in uppercase:

`"The name is {! name ->upper}" eval`

Which will give: `"Le nom est DOE JOHN"`

```
"DOE John" -> 'name'
50 -> 'age'

"{! name} is {! age} years old" eval ?

# This will display "DOE John is 50 years old"
```

# CONVERSION FUNCTIONS

To convert an object to another (for example a character string to a number or vice versa) **MOGWAI** has conversion functions that start or end with the `->` symbol.

| Operation                    | Result                                      |
|------------------------------|-----------------------------------------------|
| `D:4142434445 ->ascii`    | "ABCDE"                                       |
| `D:4142434445 ->ascii7`   | "ABCDE"                                       |
| `45 ->str`                   | "45"                                          |
| `"45" ->num`                 | 45                                            |
| `D:FF5612AE5678 ->base64` | "/1YSrlZ4"                                    |
| `"/1YSrlZ4" base64->`        | D:FF5612AE5678                             |
| `1968 ->bin`                 | B:11110110000                               |
| `(64 65 66) ->data`          | D:414243                                   |
| `64 65 66 3 ->data`          | D:414243                                   |
| `0.56 ->deg`                 | 32.08563652732611                             |
| `123.67432 "0.00" ->format`  | "123.67"                                      |
| `234 ->hex`                  | "EA"                                          |
| `20 ->i8`                    | D:14                                       |
| `20 ->i16`                   | D:0014                                     |
| `20 ->i32`                   | D:00000014                                 |
| `20 ->i64`                   | D:0000000000000014                         |
| `-30 ->u8`                   | D:E2                                       |
| `-30 ->u16`                  | D:FFE2                                     |
| `-30 ->u32`                  | D:FFFFFFE2                                 |
| `-30 ->u64`                  | D:FFFFFFFFFFFFFFE2                         |
| `56.9865 ->int`              | 56                                            |
| `"latitude" ->key`           | latitude:                                     |
| `"rand" ->keyword`           | rand                                          |
| `45 56 78 3 ->list`          | (45 56 78 3)                                  |
| `D:414243 ->list`         | (65 66 67)                                    |
| `"HELLO LE MONDE" ->lower`   | "hello le monde"                              |
| `"hello le monde" ->upper`   | "HELLO LE MONDE"                              |
| `D:2345E323 ->md5`        | D:0E9751A0F9AF52C737038B4F2108A907         |
| `"latitude" ->name`          | 'latitude'                                    |
| `(2 3 +) ->program`          | « 2 3 + »                                     |
| `35.3 ->rad`                 | 0.6161012259539983                            |
| `D:12ED45FE89 ->sha1`     | D:8B1FB372469A9B52DED84498FF26CEE06C07910B |
| `123 ->type`                 | .number                                       |
| `(1 2 3) ->type`             | .list                                         |
| `'latitude' ->type`          | .name                                         |
| `latitude: ->type`           | .key                                          |
| `"latitude" ->type`          | .string                                       |
| `"Hello !" ->utf8`           | D:48656C6C6F2021                           |
| `D:48656C6C6F2021 utf8->` | "Hello !"                                     |

# LISTS

**MOGWAI** lists are not typed, they can contain a collection of any objects.

Lists are noted with parentheses. The objects they contain are simply separated by spaces.

For example `(1 2 7)` is a list of numbers, `("X1" "X2" "X3")` is a list of character strings and `("X1" "X2" "X3" 45 67 (1 2 3) true)` is a list of lots of different objects (a list can contain lists).

## Creating a list

The simplest method to create a list is to enter it directly (as above).

You can also place on the stack the elements that must compose it, indicate how many to take and use the `->list` function.

```
# We create a list from the objects that are on the stack.

10 20 30 40 50 5 ->list

# This instruction will place the list (10 20 30 40 50) on the stack
```

You can also type the list directly in your code:

```
# We create a list directly in the code

(10 20 30 40 50)

# This instruction will place the list (10 20 30 40 50) on the stack
```

## Adding elements to a list

The + function allows you to add an element to a list.

```
# We add 1 element to a list.

(10 20 30) 40 +

# This instruction will place the list (10 20 30 40) on the stack

# We add a list to a list.

(10 20 30) (100 200) +

# This instruction will place the list (10 20 30 (100 200)) on the stack
```
 
## Retrieving the size (number of elements) of a list

The `size` function returns the size of a list.

```
# We retrieve the size of a list to display it

(10 20 30 40) size ?

# Will display 4
```` 

## Modifying an element of a list

The `set` function allows you to modify a particular element. You must provide its index (from 0 to size-1) and the new value:

```
# We modify the 3rd element of the list (we replace 55 with "Z")

(10 "E" 55 20 30) 2 "Z" set

# This instruction will place the list (10 "E" "Z" 20 30) on the stack
```

## Retrieving an element from a list

The get function allows you to retrieve an element from a list. As with `set`, you must provide its index (from 0 to size-1):

```
# We retrieve the 5th element of the list

(10 20 30 40 50 60 70) 5 get

# This instruction will place the value 60 on the stack
```

If the specified index is not in the possible range (from 0 to size-1), the function returns `null` and does not raise an error.
 
## Retrieving an element "buried" in a list

If a list is composed of sub-lists and/or sub-records (see later the presentation of records which are key/value associations), it may be interesting to give in a single operation the "path" to follow to retrieve the information:

```
# Method 1, basic, we retrieve information in multiple operations
# We will first retrieve the 2nd record, then the value of its key name:

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) 1 get

# This operation places the record [id: 1 name: "SMITH"] on the stack
# Then we retrieve the value of the key name:

name: get

# Which places "SMITH" on the stack

# Method 2, we retrieve information in a single operation

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) (1 name:) get

# This operation directly places "SMITH" on the stack
```

If the path leads to nothing (bad path), the returned value will be the null value.

```
# If the path is bad

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) (5 name:) get

# This operation directly places null on the stack because element 5 of the list
# Does not exist.
```

## Extracting part of a list

The `extract` function allows you to extract only certain elements from a list in a single operation. It takes as parameters the source list and a list of indexes to extract:

```
# We extract elements 1 2 4 from the list

(10 "E" 55 20 30) (1 2 4) extract

# This instruction will place the list ("E" 55 30) on the stack
```

If you request indexes that don't exist (outside the indexes of the source list), values of type `null` will be added in their place.
 

## Retrieving the 1st element of a list

There are 2 ways, the 1st is the one we just saw, using the `get` function with an index of zero.

The second way is to use the `first` function, which does exactly the same thing. If the list is empty, it returns `null`.

```
# We retrieve the 1st element of the list in 2 ways

# With the get function

(10 20 30 40 50 60 70) 0 get

# This instruction will place the value 10 on the stack

# With the first function

(10 20 30 40 50 60 70) first

# This instruction will place the value 10 on the stack
```

## Retrieving the last element of a list

You've probably guessed it, the `last` function returns the last element of a list, and the `null` value if the list is empty.

```
# We retrieve the last element of the list

(10 20 30 40 50 60 70) last

# This instruction will place the value 70 on the stack
```

## Deleting an element from a list

To delete an element from a list, you must use the purge function with the list and the index to delete as parameters. If the index is < 0 an error is raised. If the index is >= size the operation is simply ignored.

```
# We delete the 3rd element of the list, which is the value 40

(10 20 30 40 50 60 70) 3 purge

# This instruction will place (10 20 30 50 60 70) on the stack
```
 
## Extracting elements from a list from a given index

To extract a sub-list, you must use the `sub` function with the starting index and the number of elements to retrieve as parameters. If the starting index is outside the list, an error is raised.

This function returns a list composed of the selected elements.

If you request more elements than possible, the response will be composed of the maximum possible elements.

```
# We retrieve part of a list

(10 20 30 40 50 60 70) 2 3 sub

# This instruction will place (30 40 50) on the stack

# We retrieve part of a list by requesting too many elements

(10 20 30 40 50 60 70) 2 30 sub

# This instruction will place (30 40 50 60 70) on the stack

# We retrieve part of a list starting from an index that is too large

(10 20 30 40 50 60 70) 20 3 sub

# This instruction will raise the error "bad argument value"
```

## Retrieving an entire list except the 1st element or the last element

It is the `butfirst` function that allows you to retrieve an entire list except the 1st element.

The `butlast` function allows you to retrieve an entire list except the last element.

If the list is empty or if it consists of a single element, these functions return an empty list.

```
# We retrieve a list without its 1st element

(10 20 30 40 50 60 70) butfirst

# This instruction will place (20 30 40 50 60 70) on the stack

# We retrieve a list without its last element

(10 20 30 40 50 60 70) butlast

# This instruction will place (10 20 30 40 50 60) on the stack
```

## Converting a list to a byte array (data)

You can create a data object (byte array) from a list.

Only numbers between 0 and 255 are allowed.

```
# Example 1: We create a data object from a list of bytes expressed in hexadecimal

(0x10 0x20 0x30 0x40) ->data

# This instruction places the data object D:10203040 on the stack

# Example 2: We are not obliged to use hexadecimal notation

(100 200 120 10) ->data

# This instruction will place the data object D:64C8780A on the stack
```

## Finding the location of values

To search for the location of values in a list, you must use the `where` function.

This function returns all locations of a value that is passed to it as a parameter.

```
# We search for the indexes of the value "XX"

(10 20 "XX" "EA" 670 true "XX") "XX" where

# This instruction will place (2 6) on the stack
```

## Checking that a value is present at least once in a list

The `contains` function returns a boolean value indicating whether a value is present (at least once) in a list.

```
# We verify that the value "JEU" is present in the list

("L1" "L2" "L3" "L4" "L5" "L6" "L7") "L4" contains

# This instruction will place true on the stack
```

## Mathematical functions

Some mathematical functions use lists as input parameters. This is the case for example with the `sum`, `average`, `min`, `max` functions.

The "Mathematical functions" paragraph explains their use.

# RECORDS

**MOGWAI** records are objects that allow you to associate a value with a key (similar to a dictionary).

## The KEY object

The key of an association is assigned to a `.key` type object which is a name that must end with the `:` (colon) symbol.

## The RECORD object

A `.record` type object is delimited by brackets `[ ]` and contains a series of key/value pairs.
A record can be empty, in which case it is simply noted as `[]`.

For example, a record containing an x and y value will have an `x:` key and a `y:` key and their value, which would give: `[x: 100 y: 50]`.
The value can be any **MOGWAI** object, and why not a key (which is a **MOGWAI** object so authorized), or another record.

A key can only be present once in a record. If this is not the case, only the value of the last occurrence of the key is taken into account.

`[x: 10 y: 20 x: 100]` is equivalent to writing `[x: 100 y: 20]`

## Adding or modifying keys

To add a new key or modify an existing key, you must use the `set` function by specifying the record to process, the key to use and the associated value.

```
# Example 1: We add the z: key with the value 300

[x: 100 y: 200] z: 300 set

# This instruction places [x: 100 y: 200 z: 300] on the stack

# Example 2: We modify the y: key by giving it the value 2000 instead of 200

[x: 100 y: 200] y: 2000 set

# This instruction places [x: 100 y: 2000] on the stack
```

## Retrieving the value of a key

To retrieve the value of a key, you must use the `get` function by indicating the record and the key.

```
# We retrieve the value of the y: key

[x: 100 y: 200] y: get

# This instruction places 200 on the stack
```
 
## Retrieving a key "buried" in a record

If a record is composed of sub-records and/or sub-lists, it may be interesting to give in a single operation the "path" to follow to retrieve the information.

```
# Method 1, basic, we retrieve information in multiple operations
# We will first retrieve the value of the gps: key then the value of the latitude: key

[id: 1 name: "DOE" gps: [latitude: 45 longitude: 5]] gps:

# This operation places the record [latitude: 45 longitude: 5] on the stack
# Then we retrieve the value of the latitude: key

latitude: get

# Which places 45 on the stack

# Method 2, we retrieve the information in a single operation

[id: 1 name: "DOE" gps: [latitude: 45 longitude: 5]] (gps: latitude:) get

# This operation directly places 45 on the stack
```

## Retrieving the size of a record (number of keys)

The `size` function returns the number of keys present in a record.

```
# We retrieve the number of keys in the record

[x: 100 y: 200] size

# This instruction places 2 on the stack
```

## Retrieving the list of keys from a record

The keys function returns the list of keys from a record.

```
# We retrieve the list of keys from a record

[x: 100 y: 200] keys

# This instruction places (x: y:) on the stack
```

## Extracting part of a record

The `extract` function allows you to extract only certain keys from a record in a single operation. It takes as parameters the source record and a list of keys to extract.

```
# We extract the x: y: keys from the record

[x: 100 y: 200 z: 70 u: 10] (x: y:) extract

# This instruction will place the record [x: 100 y: 200] on the stack
```

If you request a key that doesn't exist, a error is raised.

## Checking that a key is present in a record

The `contains` function returns a boolean value indicating whether a key is in a list.

```
# We check that the x: key is present in a record

[x: 10 y: 20] y: contains

# This instruction will place true on the stack
```

## Deleting a key in a record

The `purge` function allows you to delete a key. It takes as parameters the record and the key to delete.

```
# We delete the x: key from the record

[x: 10 y: 20] x: purge

# This instruction will place [y: 20] on the stack
```

## "Shorter" notation for get and set

**MOGWAI** provides a compact notation for reading and writing values in any container — records, lists, byte arrays, and class instances — using the `->` and `<-` symbols.

This notation is only accepted with a variable name on the left side, not directly with a literal value.

The selector placed on the right side of `->` or `<-` determines both the operation and the type of container:

| Selector | Container | Operation |
|----------|-----------|-----------|
| `key:` | Record / Class instance | Read or write a named field |
| `number` | List / Byte array | Read or write by index (0-based) |
| `$variable` | Any | Dynamic read or write using a key or index stored in a variable |

### Reading with `->`

```
# Record: retrieve the value of y:
[x: 10 y: 20] -> '$R'
$R->y: ?
# Equivalent to: $R y: get ?
# Places 20 on the stack

# List: retrieve item at index 2
(10 20 30) -> '$L'
$L->2 ?
# Equivalent to: $L 2 get ?
# Places 30 on the stack

# Byte array: retrieve byte at index 1
D:FFAAEE -> '$D'
$D->1 ?
# Equivalent to: $D 1 get ?
# Places 0xAA on the stack

# Dynamic key stored in a variable
z: -> '$K'
[x: 10 y: 20 z: 30] -> '$R'
$R->$K ?
# Places 30 on the stack
```

### Writing with `<-`

The value to write must be placed on the stack before the `<-` expression. For simple values, this is straightforward. For computed values, use a `{! }` block.

```
# Record: write a value to an existing key
[x: 10 y: 20] -> '$R'
1000 &$R<-y:
$R ?
# Places [x: 10 y: 1000] on the stack
# Equivalent to: 1000 &$R y: set

# Record: add a new key (upsert)
500 &$R<-z:
$R ?
# Places [x: 10 y: 1000 z: 500] on the stack

# List: write a value at index 1
(10 20 30) -> '$L'
2000 &$L<-1
$L ?
# Places (10 2000 30) on the stack

# Byte array: write a byte at index 1
D:FFFFFFFF -> '$D'
0xAA &$D<-1
$D ?
# Places D:FFAAFFFF on the stack

# Computed value: use a {! } block
{! rand 100 * ->int} &$R<-x:
```

> **Note:** The `&` sigil before the variable name indicates an in-place mutation. Without `&`, the modified copy is placed on the stack and the original variable is not changed.

> **Breaking change (v8.6):** The parameter order of the verbose `set` function has been updated for consistency with RPN conventions. The value to write is now the **first** parameter, before the container and the key: `value container key: set`. Code written for **MOGWAI** 6 or 7 using the previous order (`container key: value set`) must be updated.

# BYTE ARRAYS

In the industrial field, it is very often necessary to manipulate byte arrays.

Commands are sent in the form of byte arrays, information is received in the same form. It is often a matter of manipulating this data in all sorts of ways.

**MOGWAI** having initially been created to simulate a device using Bluetooth Low Energy, naturally has a whole battery of functions to manipulate byte arrays and bytes themselves as simply as possible.

A byte array is named DATA in **MOGWAI** and the type is `.data`.

It is possible to create a DATA directly with the `D:` notation followed by the bytes that compose it in hexadecimal format.

```
# We create a byte array composed of 4 bytes
# Which are AB 56 32 FF

D:AB5632FF

# Places the array of 4 bytes on the stack
```

You can also create an empty DATA with the `D:` notation.

```
# We create an empty DATA and store it
# In the global variable $D

D: -> '$D'
```

You can add a byte to the DATA with the `+` function:

```
# We create a byte array composed of 4 bytes
# Which are 0xAB 0x56 0x32 0xFF

D:AB5632FF

# It is placed on the stack
# We now add a byte with value 0x56

0x56 +

# On the stack there is now D:AB5632FF56
```

The `size` function returns the size (the number of bytes) of the DATA.

You can concatenate 2 DATA with the `+` function:

```
# On place 2 DATA dans 2 variables globales

D:FF56EB23 -> '$A'
D:89CD34 -> '$B'

# We concatenate the 2 DATA that we store in another global variable

$A $B + -> '$C'

# $C now contains D:FF56EB2389CD34
```

To retrieve a particular byte from a DATA, you must use the `get` function (the 1st byte has index zero):

```
# We create a DATA composed of 4 bytes and we
# Extract the byte placed in 3rd position

D:FF56EB23 2 get

# The value 0xEB (235 in decimal) is placed on the stack
```

To modify the value of a particular byte, you must use the `set` function:

```
# We create a DATA composed of 4 bytes
# Then we modify the byte placed at position 1
# The value 0x56 will be replaced by 0x34

D:FF56EB23 1 0x34 set 

# D:FF34EB23 is placed on the stack
```

To modify part of a DATA with another DATA, you must also use the `set` function:

```
# We will replace the first 2 bytes of a DATA

D:FFC0AB0146 0 D:AABB set ?

# There is now D:AABBAB0146 on the stack
````

To delete a particular byte, you must use the `purge` function:

```
# We create a DATA composed of 4 bytes
# Then we delete the byte placed at position 1

D:FF56EB23 1 purge 

# D:FFEB23 is placed on the stack
```
 
To extract part of a DATA, you must use the `sub` function:

```
# We create a data composed of 6 bytes
# We extract 3 bytes starting from the 3rd byte

D:010203EB5634 2 3 sub

# D:03EB56 is placed on the stack
```

The `extract` function allows you to extract only certain elements from a data in a single operation. It takes as parameters the source data and a list of indexes to extract:

```
# We extract elements 1 2 4 from the data

D:FF45AB23EA (1 2 4) extract

# This instruction will place the data D:45ABEA on the stack
```

If you request indexes that don't exist (outside the indexes of the source data), an error is raised.

It is possible to transform a DATA into a list of numbers with the `->list` function:

```
# We transform a DATA into a list

D:FF45EB12AD89 ->list

# The list (255 69 235 18 173 137) is placed on the stack
```

From a list of numbers you can create a DATA with the `->data` function.
Warning, only numbers between 0 and 255 will be taken into account, other elements of the list will be ignored:

```
# We transform a list into DATA

(50 25 45 36 0xFF) ->data

# D:32192D24FF is placed on the stack
```

Also with the `->data` function, it is possible to create a DATA directly from the elements placed on the stack.

You just need to indicate how many elements to use. Warning, elements that are not numbers or whose value is not between 0 and 255 are not allowed:

```
# We transform the stack elements into DATA
# We must indicate how many elements to use
# Here 6

50 25 45 36 12 0xFF 6 ->data ?

# DATA:32192D240CFF is placed on the stack
```

To find all occurrences of a byte in a DATA, you must use the `where` function:

```
# We will search for all occurrences of the value 0xC0 in a DATA

D:FFC005FA12C056EC 0xC0 where

# where will place the list (1 5) on the stack
# Because in this DATA, the value 0xC0 is present at position 1 and 5
```
 
You can also find the locations of a DATA in another:

```
# We will search for all occurrences of 0xFFC0 in a DATA
# This is equivalent to searching for a DATA in another (here DATA:FFC0)

D:FFC005FA12C056EC DATA:FFC0 where

# where will place the list (0) on the stack
# Because in this DATA, the value 0xFFC0 is present at position 0 only
```

## Conversion functions to a DATA

To efficiently manipulate byte arrays, you must be able to convert numbers into different formats.
For example, take a number and convert it to an unsigned integer on 16 bits (2 bytes), or to a signed integer on 32 bits (4 bytes) as needed.

**MOGWAI** offers for this purpose a series of conversion functions that take a number as a parameter and return the corresponding DATA after conversion.

For example, after converting a number to a signed integer on 32 bits, you will get a DATA composed of the 4 bytes corresponding to the result of the requested conversion.

Once the conversion is complete, it is quite simple to insert the result (which is a DATA) into a DATA with the `set` function.

Number conversion functions returning a DATA:

| Operation   | Usage                                                | Result                 |
|-------------|------------------------------------------------------|------------------------|
| `50 ->u8`   | Conversion to unsigned integer on 8 bits (1 byte)    | D:32                |
| `50 ->u16`  | Conversion to unsigned integer on 16 bits (2 bytes)  | D:0032              |
| `50 ->u32`  | Conversion to unsigned integer on 32 bits (4 bytes)  | D:00000032          |
| `50 ->u64`  | Conversion to unsigned integer on 64 bits (8 bytes)  | D:0000000000000032  |
| `-50 ->i8`  | Conversion to signed integer on 8 bits (1 byte)      | D:CE                |
| `-50 ->i16` | Conversion to signed integer on 16 bits (2 bytes)    | D:FFCE              |
| `-50 ->i32` | Conversion to signed integer on 32 bits (4 bytes)    | D:FFFFFFCE          |
| `-50 ->i64` | Conversion to signed integer on 64 bits (8 bytes)    | D:FFFFFFFFFFFFFFCE  |

If a number that is too large or too small is provided as a parameter, it will be truncated during conversion without raising an error.
 
## Advanced DATA display

To visualize the content of a DATA more simply, you can use the `?d` function which will display the dump of a DATA.

```
# We will download the main page of google.fr
# We store the response in the local variable R

[uri: "https://www.google.fr"] http.get -> 'R'

# The function returns a record composed of 2 keys
# state: of type .boolean which indicates if everything went well
# response: of type .data which contains the downloaded resource

if (R state: get) then
{
	# Everything is ok we can retrieve the response
	# Which we store in the local variable B
	
	R response: get -> 'B' 
	
	# We extract the first 1000 bytes and display them
	# In the form of a dump
	
	B 0 1000 sub ?d
}
```

Here is an example of a DATA dump display:

```
00000000  3C 21 64 6F 63 74 79 70 65 20 68 74 6D 6C 3E 3C  | <!doctype html><  |
00000010  68 74 6D 6C 20 69 74 65 6D 73 63 6F 70 65 3D 22  | html itemscope="  |
00000020  22 20 69 74 65 6D 74 79 70 65 3D 22 68 74 74 70  | " itemtype="http  |
00000030  3A 2F 2F 73 63 68 65 6D 61 2E 6F 72 67 2F 57 65  | ://schema.org/We  |
00000040  62 50 61 67 65 22 20 6C 61 6E 67 3D 22 66 72 22  | bPage" lang="fr"  |
00000050  3E 3C 68 65 61 64 3E 3C 6D 65 74 61 20 63 6F 6E  | ><head><meta con  |
00000060  74 65 6E 74 3D 22 74 65 78 74 2F 68 74 6D 6C 3B  | tent="text/html;  |
00000070  20 63 68 61 72 73 65 74 3D 55 54 46 2D 38 22 20  |  charset=UTF-8"   |
00000080  68 74 74 70 2D 65 71 75 69 76 3D 22 43 6F 6E 74  | http-equiv="Cont  |
00000090  65 6E 74 2D 54 79 70 65 22 3E 3C 6D 65 74 61 20  | ent-Type"><meta   |
000000A0  63 6F 6E 74 65 6E 74 3D 22 2F 69 6D 61 67 65 73  | content="/images  |
000000B0  2F 62 72 61 6E 64 69 6E 67 2F 67 6F 6F 67 6C 65  | /branding/google  |
000000C0  67 2F 31 78 2F 67 6F 6F 67 6C 65 67 5F 73 74 61  | g/1x/googleg_sta  |
000000D0  6E 64 61 72 64 5F 63 6F 6C 6F 72 5F 31 32 38 64  | ndard_color_128d  |
000000E0  70 2E 70 6E 67 22 20 69 74 65 6D 70 72 6F 70 3D  | p.png" itemprop=  |
000000F0  22 69 6D 61 67 65 22 3E 3C 74 69 74 6C 65 3E 47  | "image"><title>G  |
00000100  6F 6F 67 6C 65 3C 2F 74 69 74 6C 65 3E 3C 73 63  | oogle</title><sc  |
00000110  72 69 70 74 20 6E 6F 6E 63 65 3D 22 66 4C 6F 78  | ript nonce="fLox  |
00000120  59 71 79 59 4B 73 59 35 69 6E 59 78 79 4E 4F 4C  | YqyYKsY5inYxyNOL  |
00000130  6E 41 22 3E 28 66 75 6E 63 74 69 6F 6E 28 29 7B  | nA">(function(){  |
00000140  76 61 72 20 5F 67 3D 7B 6B 45 49 3A 27 47 72 39  | var _g={kEI:'Gr9  |
00000150  62 61 50 58 77 45 2D 79 59 6B 64 55 50 5F 39 79  | baPXwE-yYkdUP_9y  |
00000160  43 32 51 59 27 2C 6B 45 58 50 49 3A 27 30 2C 32  | C2QY',kEXPI:'0,2  |
00000170  30 32 37 39 32 2C 36 32 2C 32 2C 36 30 39 36 32  | 02792,62,2,60962  |
00000180  35 2C 33 38 38 2C 32 38 38 37 34 31 34 2C 31 31  | 5,388,2887414,11  |
00000190  30 31 2C 35 35 32 37 37 32 2C 34 32 35 36 30 33  | 01,552772,425603  |
000001A0  2C 32 34 37 33 31 39 2C 34 32 37 32 35 2C 35 32  | ,247319,42725,52  |
000001B0  33 30 32 38 30 2C 31 31 34 30 32 2C 33 32 37 36  | 30280,11402,3276  |
000001C0  38 39 33 33 2C 34 30 34 33 37 30 39 2C 32 35 32  | 8933,4043709,252  |
000001D0  32 38 36 38 31 2C 31 33 38 32 36 38 2C 31 34 31  | 28681,138268,141  |
000001E0  31 38 2C 31 31 39 34 30 2C 35 33 32 32 32 2C 36  | 18,11940,53222,6  |
```

## DATA and character strings

Some conversion functions related to character strings take DATA as parameters or return DATA:

| Operation              | Usage                                                                                                     | Result    |
|------------------------|-----------------------------------------------------------------------------------------------------------|-------------|
| `D:414243 ->ascii`  | Returns the ASCII character string (8 bits)<br> composed with the bytes of the DATA passed as parameter.  | "ABC"       |
| `D:414243 ->ascii7` | Returns the ASCII character string (7 bits)<br> composed with the bytes of the DATA passed as parameter.  | "ABC"       |
| `D:414243 ->utf8`   | Returns the UTF8 character string<br> composed with the bytes of the DATA passed as parameter.            | "ABC"       |
| `D:414243 ->base64` | Returns the byte array in the form<br> of a character string encoded in base 64.                | "QUJD"      |
| `"ABC" ascii->`        | Returns the byte array corresponding to the ASCII conversion (8 bits)<br> of a character string. | D:414243 |
| `"ABC" ascii7->`       | Returns the byte array corresponding to the ASCII conversion (7 bits)<br> of a character string. | D:414243 |
| `"QUJD" base64->`      | Returns the byte array corresponding to the decoding<br> of a string encoded in base 64.                 | D:414243 |

## Other available functions

Hash key calculation functions:

| Operation            | Usage                                     | Result                                      |
|----------------------|-------------------------------------------|-----------------------------------------------|
| `D:414243 ->md5`  | Returns the MD5 hash key of a DATA  | D:902FBDD2B1DF0C4F70B4A5D23525E932         |
| `D:414243 ->sha1` | Returns the SHA1 hash key of a DATA | D:3C01BDBB26F358BAB27F267924AA2C9A03FCFDB8 |

It is possible, with the `->compress` function to compress a DATA, and decompress it with the `->decompress` function:

```
# We will download the main page of google.fr
# We store the response in the local variable R

[uri: "https://www.google.fr"] http.get -> 'R'

# The function returns a record composed of 2 keys
# state: of type .boolean which indicates if everything went well
# response: of type .data which contains the downloaded resource

if (R state: get) then
{
	# Everything is ok we can retrieve the response
	# Which we store in the local variable B
	# And we compress the response which we store in the local variable C
	
	R response: get -> 'B' 
	
	B ->compress -> 'C'
	
	# We can display the size difference
	
	B size ?
	C size ?
	
	# We decompress C and display its size
	
	C ->decompress size ?
}
```

# ENDIANNESS CONVERSION

In IoT and BLE contexts, payloads exchanged with hardware devices require explicit control of byte order (endianness). **MOGWAI** provides a complete set of primitives to convert numbers to `DATA` with a specific byte order, and vice versa.

Two byte orders are supported:
- **Little Endian (LE)**: the least significant byte comes first. Used by most BLE profiles and x86/x64 architectures.
- **Big Endian (BE)**: the most significant byte comes first. Used by some hardware protocols and network standards.

Supported sizes: **8, 16, 24, 32, 48 and 64 bits**.

> If the value is too large for the requested number of bits, the most significant bytes are silently truncated — consistent with C# numeric cast behavior.

---

## Fixed-size conversion — Number to DATA

These primitives take a number from the stack and return the corresponding `DATA` in the specified byte order and size.

### Little Endian

| Primitive | Example | Result |
|---|---|---|
| `->dataLE8` | `42 ->dataLE8` | `D:2A` |
| `->dataLE16` | `42 ->dataLE16` | `D:2A00` |
| `->dataLE24` | `42 ->dataLE24` | `D:2A0000` |
| `->dataLE32` | `42 ->dataLE32` | `D:2A000000` |
| `->dataLE48` | `42 ->dataLE48` | `D:2A0000000000` |
| `->dataLE64` | `42 ->dataLE64` | `D:2A00000000000000` |

### Big Endian

| Primitive | Example | Result |
|---|---|---|
| `->dataBE8` | `42 ->dataBE8` | `D:2A` |
| `->dataBE16` | `42 ->dataBE16` | `D:002A` |
| `->dataBE24` | `42 ->dataBE24` | `D:00002A` |
| `->dataBE32` | `42 ->dataBE32` | `D:0000002A` |
| `->dataBE48` | `42 ->dataBE48` | `D:0000000000002A` |
| `->dataBE64` | `42 ->dataBE64` | `D:000000000000002A` |

---

## Fixed-size conversion — DATA to Number

These primitives take a `DATA` from the stack and return the corresponding number, interpreting the bytes in the specified byte order and size.

The naming convention follows the **MOGWAI** direction rule: `->` as a prefix means *produce this type*, `->` as a suffix means *consume this type*. So `dataLE32->` reads a 32-bit Little Endian `DATA` and returns a number.

### Little Endian

| Primitive | Example | Result |
|---|---|---|
| `dataLE8->` | `D:2A dataLE8->` | `42` |
| `dataLE16->` | `D:2A00 dataLE16->` | `42` |
| `dataLE24->` | `D:2A0000 dataLE24->` | `42` |
| `dataLE32->` | `D:2A000000 dataLE32->` | `42` |
| `dataLE48->` | `D:2A0000000000 dataLE48->` | `42` |
| `dataLE64->` | `D:2A00000000000000 dataLE64->` | `42` |

### Big Endian

| Primitive | Example | Result |
|---|---|---|
| `dataBE8->` | `D:2A dataBE8->` | `42` |
| `dataBE16->` | `D:002A dataBE16->` | `42` |
| `dataBE24->` | `D:00002A dataBE24->` | `42` |
| `dataBE32->` | `D:0000002A dataBE32->` | `42` |
| `dataBE48->` | `D:0000000000002A dataBE48->` | `42` |
| `dataBE64->` | `D:000000000000002A dataBE64->` | `42` |

---

## Dynamic-size conversion

When the size is not known at script-writing time, you can use the dynamic variants. The size (in bits) is taken from the stack along with the number or `DATA`.

### Number to DATA

| Primitive | Stack signature | Example | Result |
|---|---|---|---|
| `->dataLE` | `number size →` | `42 32 ->dataLE` | `D:2A000000` |
| `->dataBE` | `number size →` | `42 32 ->dataBE` | `D:0000002A` |

### DATA to Number

| Primitive | Stack signature | Example | Result |
|---|---|---|---|
| `dataLE->` | `DATA size →` | `D:2A000000 32 dataLE->` | `42` |
| `dataBE->` | `DATA size →` | `D:0000002A 32 dataBE->` | `42` |

If a size other than 8, 16, 24, 32, 48 or 64 is provided, a `BadArgumentTypeError` is raised.

---

## Float conversion

These primitives convert between `DATA` and floating-point numbers following the IEEE 754 standard. Two sizes are supported: **32 bits** (single precision) and **64 bits** (double precision).

The `F` suffix in the primitive name indicates a floating-point type, as opposed to the integer primitives above.

### Number to DATA (float)

| Primitive | Example | Result |
|---|---|---|
| `->dataLE32F` | `1.0 ->dataLE32F` | `D:0000803F` |
| `->dataBE32F` | `1.0 ->dataBE32F` | `D:3F800000` |
| `->dataLE64F` | `1.0 ->dataLE64F` | `D:000000000000F03F` |
| `->dataBE64F` | `1.0 ->dataBE64F` | `D:3FF0000000000000` |

### DATA to Number (float)

| Primitive | Example | Result |
|---|---|---|
| `dataLE32F->` | `D:0000803F dataLE32F->` | `1.0` |
| `dataBE32F->` | `D:3F800000 dataBE32F->` | `1.0` |
| `dataLE64F->` | `D:000000000000F03F dataLE64F->` | `1.0` |
| `dataBE64F->` | `D:3FF0000000000000 dataBE64F->` | `1.0` |

> If the `DATA` passed to a float conversion primitive is too small (less than 4 bytes for 32-bit, less than 8 bytes for 64-bit), a `BadArgumentValueError` is raised.

---

## Practical examples

```
# Round-trip verification (integer)
90000 ->dataBE32 dataBE32->
# → 90000 ✓

# Round-trip verification (float)
3.14 ->dataLE32F dataLE32F->
# → 3.14 ✓ (rounded to single precision)

# Building a BLE command payload
# Header (1 byte) + value (32-bit LE) + checksum (1 byte)
D:AA -> '$payload'
1234 ->dataLE32 -> '$value'
$payload $value + 0xFF + -> '$payload'
# → D:AAD2040000FF

# Reading a temperature sensor value (IEEE 754 float, Little Endian)
D:0000803F dataLE32F->
# → 1.0

# Reading a 48-bit MAC address
D:AABBCCDDEEFF dataLE48->
# → numeric value of the MAC address

# Dynamic size from a configuration variable
32 -> 'bits'
90000 bits ->dataLE
# → D:905F0100... (same as ->dataLE32)
```

# BINARY NUMBERS

To simplify the manipulation of bits of a number, it is possible to use a **MOGWAI** object of type `.binary`.

In **MOGWAI**, a binary number starts with `B:` followed by the bits used. For example the binary number `11001101` in binary is written in **MOGWAI** as `B:11001101`.

You cannot manage a binary number of more than 64 bits.

The `size` function returns the size (in bits) of the binary number.

It is possible to assemble 2 binary numbers with the `+` function:

```
# We assemble 2 binary numbers
# The 1st is 1 bit, and the second 7 bits
# The total will therefore be 8 bits in the end

B:1 B:1111111 + 

# Places B:11111111 on the stack
```

With the `->bin` function, you can create a binary number from a regular number. The number of bits of the created binary number will be limited to those necessary to represent the original number.

For example, the number 112 in binary is written as `1110000`, so the created binary number has a size of 7 bits.

You car also specify the size of the created binary number with the `->bin..` functions as `->bin8`, `->bin16`, `->bin32` and `->bin64`. In this case, the created binary number will be padded with zeros on the left to reach the specified size.

The `up` function allows you to raise a given bit, and the `down` function allows the opposite. You must give these functions the number of the bit to modify (the 1st bit has number 0):

```
# We create a 16-bit binary number having
# The value 112

112 ->bin16

# Places B:0000000001110000 on the stack

# We raise bit 15 and lower bit 5

15 up 
5 down

# Places B:1000000001010000 on the stack
```` 

To extract part of a binary number you must use the `sub` function by indicating from which bit to perform the extraction and how many bits to extract. The function returns a binary number composed of the extracted bits:

```
# We create a 16-bit binary number having
# The value 112

112 ->bin16

# Places B:0000000001110000 on the stack

# We extract 8 bits starting from bit 3

3 8 sub

# Places B:00001110 on the stack
```
 
It is also possible to perform bit shifts with the `>>` and `<<` functions. You must indicate by how many bits to shift (`<<` shifts to the left, `>>` to the right):

```
# We shift the binary number B:00000001 by 2 bits to the left

B:00000001 2 <<

# Places B:00000100 on the stack

# We shift to the right by a single bit

1 >>

# Places B:00000010 on the stack
```

The `not` function allows you to apply a binary not:

```
# We apply a binary not to B:11000111

B:11000111 not

# Places B:00111000 on the stack
```

The `bit?` function tests whether a specific bit is set (1) in a binary number. You must give it the number of the bit to test (the 1st bit has number 0). It returns `true` if the bit is 1, `false` otherwise:

```
# We test bit 1 of B:110011

B:110011 1 bit?

# Places true on the stack

# We test bit 2

B:110011 2 bit?

# Places false on the stack
```

To convert a binary number to a regular number you must use the `->num` function:

```
# We retrieve the numeric value of B:10011011

B:10011011 ->num

# Places 155 on the stack
```
 
# TIME MANAGEMENT

**MOGWAI** knows how to manipulate information concerning dates and durations.

A date is a number that represents the number of 100-nanosecond intervals that have elapsed since midnight, January 1st 0001. For example, the value representing the date of 05/03/2012 at 4:45 PM is 6.3466562759E+17.

Of course in this form it is not very practical, which is why **MOGWAI** has a whole series of functions to perform operations on dates and durations.

## Retrieving the current date

The `now` function returns (places on the stack) the current date of your machine.

## Retrieving the components of a date

To retrieve all the components (day, month, year, hour, etc.) of a date you must use the `->date` conversion function which takes as parameter a date (in numeric format) and returns a record containing all the components of this date.

The returned components are as follows (the keys of the returned record):

| Key          | Value                                                   |
|--------------|---------------------------------------------------------|
| `day:`       | Day of the month.                                       |
| `month:`     | Month.                                                  |
| `year:`      | Year.                                                   |
| `hour:`      | Hour.                                                   |
| `minute:`    | Minute.                                                 |
| `second:`    | Second.                                                 |
| `dayOfYear:` | Day number in the year (e.g. 244th day).               |
| `dayOfWeek:` | Day number in the week (Sunday=0, Monday=1, etc).      |

The returned components are all numbers.

```
# We retrieve the components of the date provided by now

now ->date

# Will place on the stack for example
# [day: 23 month: 5 year: 2025 hour: 12 minute: 19 second: 51 dayOfYear: 143 dayOfWeek: 5]
```

This function returns all the components of a date.


## Creating a date from scratch

To create a date from its components, simply provide a record containing the components of the date and use the `date->` function again which will return this date in numeric format.

It is not necessary to provide all the components, only the day, month and year are required. Those that are omitted are considered to be zero.

`[day: 15 month: 4 year: 2015] ->date` creates the date `15/04/2015 at 00:00:00`

`[day: 15 month: 4 year: 2015 hour : 15] ->date` creates the date `15/04/2015 at 15:00:00`

## Calculating durations

It is also possible to calculate durations. A duration being a difference between 2 dates, you can quite easily perform such calculations with **MOGWAI**.

```
# We calculate the real time actually elapsed during a 2450 ms pause with de wait function

now -> 'begin'
2450 wait
now -> 'end'
end begin - ->duration ?

# Result = [days: 0 hours: 0 minutes: 0 seconds: 2 milliseconds: 461]
# That is 2 seconds and 461 milliseconds
```
 
To retrieve the time elapsed between 2 moments (2 dates) simply subtract the arrival date from the departure date and use the `->duration` function to extract the components of this duration.

The returned value is a record composed of 5 keys:

| Key              | Value                            |
|------------------|----------------------------------|
| `days:`          | Number of days of the duration.  |
| `hours:`         | Number of hours of the duration. |
| `minutes:`       | Number of minutes of the duration.|
| `secondes:`      | Number of seconds of the duration.|
| `ms:` | Number of milliseconds of the duration.|

It is also possible to retrieve these components directly. In this case you get the total duration in the requested unit.

```
# We calculate the time elapsed during a 2450 ms pause

now -> 'begin'
2450 wait
now -> 'end'
end begin - ->duration seconds: get ?

# Result = 2.4551168 seconds

# We calculate the time elapsed during a 2450 ms pause
# With a more compact writing.

now -> 'begin' 2450 sleep now wait - ->duration seconds: get ?
```
 
# FUNCTION DECLARATION

In addition to all the functions provided as standard by **MOGWAI**, you can create your own functions (type `.function`).

There are different ways to declare functions, we will see them one after another. The differences lie in the level of security of the passed parameters (verification of parameter types more or less advanced) and the values type returned.

A function must be declared before it can be used.


## Declaring a basic function

A basic function takes all its parameters from the stack. When it is declared nothing is said about the expected parameters. Of course, a function can have no parameters.

```
# We create a function carre that takes a number as parameter and returns its square
# We take the parameter from the stack, duplicate it then perform their multiplication
# The result remains on the stack, the function is finished.

to 'carre' do { dup * }

# To use it:

5 carre

# Places the value 25 (the square of 5) on the stack

# You can also use a more traditional notation by passing parameters as a list attached directly to the function name.

carre(5)
```` 

A function, in addition to using all those provided by **MOGWAI**, can use those you define. For example to create the 'cube' function which will calculate the cube of a number we will use the 'carre' function that we defined above:

```
# We create a function cube that takes a number as parameter and returns its cube
# We take the parameter from the stack, duplicate it then calculate its square
# Then we multiply the 2 values to obtain the cube.
# The result remains on the stack, the function is finished.

to 'cube' do { dup carre * }

# To use it:

5 cube

# or cube(5)

# Places the value 125 (the cube of 5) on the stack
```
 
## Declaring a function with verified parameter types

It is possible to create a function with parameters verified at the time of the call. This avoids having to perform all the verifications in the body of the function. These are operations that can be tedious and costly in time.

For example in the previous function 'carre' nothing is verified, if you pass a character string as a parameter instead of a number an error will be raised at the time of performing the multiplication. Ideally you should verify that the type of the parameter is indeed `.number` before doing anything.

To avoid this you can indicate the expected parameters and their type as soon as the function is declared:

```
# We create a function carre with verification of the input parameter type.

to 'carre' with [x: .number] do { x dup * }

# The expected parameter will be placed in the local variable 'x' and its type will be verified.
# If the number of parameters passed is insufficient or the type of one of the parameters is
# Wrong, an error is raised.

5 carre 

# Will place 25 on the stack

"EEE" carre

# The type is incorrect!
# An error is raised with an explanatory message:
# bad argument type
# ->safeVars
# .number expected but .string found for 'x' parameter

clear carre

# If we empty the stack and call the function without any parameters
# An error is raised:
# too few arguments
# ->safeVars

You can define if needed an unlimited number of verified parameters:
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' with [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

5 9 156 fx

# Which will place 5*156+9 or 789 on the stack
```

If you need to pass a parameter without checking its type you must use the `.any` type instead of a specific type:

```
# We create a function that displays a particular message if the type is a number.

to 'nPrint' with [x: .any] do
«
    if (x ->type .number ==) then
    {
        "It is a number !" ?
    }
    else
    {
        "It is not a number !" ?
    }
»

# We call with a number as parameter…

234 nPrint

# Will display the message "It is a number !"

# If we call with a boolean…

true nPrint

# Will display the message "It is not a number !"
```

## Declaring a function with named parameters

It is also possible to declare a function whose parameters are explicitly named and types verified (belt and suspenders). You can even define default values.
For code readability it is much clearer and security is maximal with this way of doing things.

The parameters are passed via a record whose keys are the names of the parameters and the values are those of the parameters.

If we declare our previous function 'fx' with this method it would look like this:

```
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

[a: 5 b: 9 x: 156] fx

# Which will place 5*156+9 or 789 on the stack
```
 
It is possible to call this type of function in a less RPN way (parameters then function) by including the function name in the 1st position of the parameters record (this notation is only possible with function calls, this type of notation for a record does not exist elsewhere):

```
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

[fx a: 5 b: 9 x: 156]

# Which will place 5*156+9 or 789 on the stack
```

It is also possible to call this type of function in another, less RPN way, by including the function name just before the parameters record, without a space between the function name and the parameters record:

```
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

fx[a: 5 b: 9 x: 156]

# Which will place 5*156+9 or 789 on the stack
```

To declare default values, simply stipulate the type and the default value in a list. Thus, if the parameter is not provided, the default value will be used:

```
# We create a function 'foo' that has an optional boolean parameter save:
# If we don't specify it, it will have true as default value

to 'foo' params [id: .number name: .string save: (.boolean true)] do 
{
    " " ?
    "id   = {! id}" eval ?
    "name = {! name}" eval ?
    "save = {! save}" eval ?
}

[foo id: 10 name: "DOE John"]
[foo id: 30 name: "SMITH Mike" save: false]

# During the 1st call, the save: parameter will have the value true (default value)
# During the 2nd call, it will have the value false (parameter provided explicitly)
```

## Checking type of returned values

You can also check the type of the returned value of a function. To do this, simply indicate the expected types before the `do` keyword with the `returns` keyword and the list of types expected when declaring the function:

```
# We create a function carre with verification of the input parameter type and verification of returned value type.

to 'carre' with [x: .number] returns (.number) do { x dup * }

```

The `returns` keyword can be used with all the types of function declaration seen above (basic, with verified parameters, with named parameters).

## Retrieving the list of declared functions

The `funcs` function returns the list of declared functions in the form of a list of names. It is possible for example, during the program, to verify that a function exists before trying to use it.

```
# We create the functions carre and cube

to 'carre' do { dup * }

to 'cube' do { dup carre * }

# We list the existing functions

funcs 

# Places the list ('carre' 'cube') on the stack
```

# ERROR HANDLING

In case of a problem, **MOGWAI**, like most programming languages, raises an error and stops the program.
It is possible to manage the triggering of an error and ensure that the program does not crash stupidly.

## The trap instruction

To avoid stopping the program in case of an error, the `trap` instruction allows you to "protect" a block of code. If an error occurs, the protected code stops and the code continues just after the `trap` instruction. The stack is restore to the state it was in before the protected code was executed, so no need to worry about the state of the stack after an error, it is automatically restored to the state before the `trap` instruction.

```
# We will generate an error by using a variable that does not exist yet.
# The code will be protected by the trap instruction.

trap 
{ 
    "trap begins." ?
	
    10 a *
	
    "This message will never be displayed." ?
}

"exit of the trap." ?
"the code continue…" ?
```

## The guard instruction

The `guard` instruction is a bit more advanced than `trap`. It allows you to execute code if an error occurs. The stack is also restored to the state it was in before the protected code was executed.

```
# We will generate an error by using a variable that does not exist yet.
# The code will be protected by the guard instruction.

guard 
{ 
    "guard begins." ?
	
    10 a *
	
    "This message will never be displayed." ?
}
else
{
    "An error has occurred in the guard!" ?
}

"exit of the guard" ?
"the code continues…" ?
```
 
## Knowing the last error raised

Knowing that an error occurred without killing the code is good but knowing what error was raised is better to be able to react.

The `error.last` function returns the code of the last generated error. The code is a string that gives information about the error.

The `error.reset` function allows you to reset (no error) the code of the last error. So it is good practice to reset this information once you have finished handling the last error because it will not reset itself.

```
# We will generate an error by using a variable that does not exist yet.
# The code will be protected by the guard instruction.

guard 
{ 
    "guard begins." ?
	
    10 a *
	
    "This message will never be displayed." ?
}
else
{
    "The error " ?? error.last ?? " happened!" ?
    error.reset
}

"exit of the guard" ?
```

## Artificially raising an error

It is possible to raise an error using the `error.throw` function which takes as parameter the string code of the error to raise.

List of main errors:

| Code   | Label                  |
|--------|--------------------------|
| MW.0   | no error.                                         |
| MW.1   | parse error.                                      |
| MW.2   | halt encounted error.                             |
| MW.3   | empty code error.                                 |
| MW.4   | internal error.                                   |
| MW.5   | platform not supported error.                     |
| MW.6   | unabled to fire event error.                      |
| MW.7   | operation not supported error.                    |
| MW.8   | circular reference error.                         |
| MW.9   | assert error.                                     |
| MW.10  | generic error.                                    |
| MW.11  | primitive not found error.                        |
| MW.20  | too few arguments error.                          |
| MW.21  | bad argument type error.                          |
| MW.22  | bad argument value error.                         |
| MW.23  | stack size error.                                 |
| MW.24  | stack corruption error.                           |
| MW.30  | division by zero error.                           |
| MW.31  | mathematical error.                               |
| MW.32  | convert error.                                    |
| MW.40  | unknown name error.                               |
| MW.41  | name already exits error.                         |
| MW.42  | function already exists error.                    |
| MW.43  | name already used by function error.              |
| MW.44  | name already used by var error.                   |
| MW.45  | unknown key error.                                |
| MW.46  | invalid name error.                               |
| MW.47  | unabled to write value in var.                    |
| MW.48  | unabled to write value in undeclared var.         |
| MW.50  | unknown word error.                               |
| MW.60  | task creation error.                              |
| MW.61  | unabled to start task error.                      |
| MW.62  | invalid outside of a task error.                  |
| MW.70  | invalid path error.                               |
| MW.71  | path does not exists error.                       |
| MW.72  | file operation error.                             |
| MW.73  | unknown file error.                               |
| MW.80  | using error.                                      |
| MW.81  | using already exists error.                       |
| MW.90  | class definition error.                           |
| MW.91  | unknown class error.                              |
| MW.92  | instance creation error.                          |
| MW.93  | unknown instance error.                           |
| MW.94  | unknown property error.                           |
| MW.95  | reserved property error.                          |
| MW.!!! | fatal error.                                      |

# MAKING A PAUSE

It is sometimes necessary to pause in a program or function.

## The `wait` function

With the `wait` function, the program is suspended for the number of milliseconds passed as parameter and events and timers continue to work.

```
# We will display the numbers from 1 to 100
# With a pause of 250 milliseconds between each

1 100 for 'i' do
{
    i ?
    250 wait
}
```

## The `post` function

The `post` function posts a block of code to the engine's execution queue. The block executes in the next scheduler cycle, after pending events and timers — without creating an intermediate timer.



The main use case is deferred execution from an event handler — for example to allow the TUI interface to refresh before a long computation. With `post`, the engine processes pending events before executing the block:

```
# Wait for a key press without blocking the scheduler
while (console.getInputKey -1 ==) do
{
    post { }
}
```

`post { }` with an empty block is valid — useful to let the scheduler process pending events without executing any additional code. `post` is more efficient than `after 0 do { }`: it does not create a timer, it posts the block directly to the execution queue.

```
# Post and then perform some work
post
{
    "This runs after all pending events." ?
}
```

# EXITING A FUNCTION, A LOOP OR THE PROGRAM

The flow of a program can be "broken" by the 5 functions `mogwai.exit`, `mogwai.halt`, `mogwai.assert`, `break` and `return`.
 
## The `mogwai.exit` function

It is possible at any time to stop the program, to exit it.
The `mogwai.exit` function takes care of that.

When a program terminates without error (normal stop or caused by the `mogwai.exit` instruction), the reserved function `MOGWAI.onStop` is automatically executed by **MOGWAI**. If it is defined in your code it will be called automatically:

```
# We define the function that will be executed at the end
# Normal program

to 'MOGWAI.onStop' do 
{
    "The program has just ended." ?
}

# We perform an infinite task
# But if a value < 50 comes out we stop the program

forever do
{
    # We draw a random number and store it
    # In the local variable 'r'
    rand 1000 * ->int -> 'r'
	
    # We display it and take a short break
    r ? 250 wait
	
    # If the number is < 50 then we stop the program
    if (r 50 <) then {exit}
}

# So the code that follows will never be executed
# But the MOGWAI.onStop function will be automatically executed

"Death code!" ?
```
 
## The `mogwai.halt` function

The `mogwai.halt` function behaves exactly like the `mogwai.exit` function, but it raises error "MW.2", "halt encounted error" instead of saying nothing at all. So it's an error stop.

When a program terminates on an error (`mogwai.halt` raises an error), the reserved function `MOGWAI.onError` is automatically executed by **MOGWAI**. If it is defined in your code it will be called automatically. Inside `MOGWAI.onError`, `error.last` returns the code of the error that triggered the stop — it is the only runtime information available at that point:

```
# We define the function that will be executed
# If an error is raised in the program

to 'MOGWAI.onError' do 
{
    "An error has occurred: " ?? error.last ?
}

# We perform an infinite task
# But if a value < 50 comes out we stop the program
# With halt which causes an error stop

forever do
{
    # We draw a random number and store it
    # In the local variable 'r'
    rand 1000 * ->int -> 'r'
	
    # We display it and take a short break
    r ? 250 wait
	
    # If the number is < 50 then we stop the program
    if (r 50 <) then {halt}
}

# So the code that follows will never be executed
# But the MOGWAI.onError function will be automatically executed

"Death code!" ?
```

## The `mogwai.assert` function

`mogwai.assert` verifies that a condition is true. If it is false, it raises error `MW.9` (`assert error`) and stops execution. If `MOGWAI.onError` is defined, it will be called automatically.

`mogwai.assert` takes two parameters: a condition and a message.

The condition can be:
- A **list** — it is automatically evaluated. After execution, `mogwai.assert` verifies that exactly one value was pushed onto the stack by the test code (`MW.24` stack corruption error if not), and that this value is a boolean (`MW.21` bad argument type if not).
- A **boolean** already on the stack — used directly.

Any other type raises `MW.21` (bad argument type).

The message is a string displayed alongside the error. It is not accessible programmatically — `error.last` returns `MW.9`.

```
# Using a list — the condition is evaluated by mogwai.assert
(a 10 ==) "a must equal 10" mogwai.assert

# Using a boolean already on the stack
a 0 >  "a must be positive" mogwai.assert
a islist "a must be a list" mogwai.assert
```

`mogwai.assert` is particularly useful for validating preconditions in functions, or for writing in-script tests:

```
to 'divide' with [x: .number y: .number] do
{
    (y 0 !=) "divisor must not be zero" mogwai.assert
    x y /
}
```

## The `break` function

When you are in a loop (see the LOOPS chapter) it is possible to exit "by force" with the `break` function which can be used in `while`, `do ... while`, `for`, `foreach`, `during`, `repeat` and `forever` loops.

```
# We display the numbers from 1 to 100 with a for loop
# If the current number is > 10 we exit the loop

1 100 for 'i' do
{
    i ?
	
    if (i 10 >) then {break}
}

# The code continues here

"Continuing the program…" ?
```
 
## The `return` function

It allows you to exit a function prematurely.

```
to 'displayValue' with [value: .number] do 
{
    # We display the value as is
    # Unless it's the value 5 which we replace with a message
    # We could have done it with an else but it's just for the example
	
    if (value 5 !=) then
    {
        value ?
        return
    }

    "Valeur 5 interdite !" ?
}

1 10 for 'i' do
{
    i displayValue
}
```
 
# AUTOMATIC VARIABLE CREATION

## The `->vars` function

The `->vars` function avoids many operations when you want to create local variables from a source such as a record, or from the stack.

### `->vars` from a record

If you have a record and you need to retrieve the included values to manipulate them, the basic solution is to retrieve the values to manually assign them to local variables before processing them.

```
# We need to process the values carried by the x: and y: keys
# Of a record that is stored in the local variable 'r'

[x: 50 y: 30] -> 'r'

# Basic method, we manually retrieve the values
# To store them in local variables with the same name as the keys

r x: get -> 'x'
r y: get -> 'y'

# Now, we can process the values
# Through the local variables
```

You can simplify the code by using the `->vars` function

```
# We need to process the values carried by the x: and y: keys
# Of a record that is stored in the local variable 'r'

[x: 50 y: 30] -> 'r'

# Faster and automatic method
# To store them in local variables with the same name as the keys

r ->vars

# Now, we can process the values with the local variables x and y
# Which were automatically created by ->vars and which carry the values
# that the corresponding keys have in the RECORD.
```

### `->vars` from the stack

It is possible to automatically extract elements from the stack and store them in local variables with `->vars`.

Simply specify the list of variables to create as a parameter. The number of elements corresponding to the number of variables in the list will be taken from the stack and stored in the corresponding local variables. If there are not enough elements on the stack to fill all the listed variables, the function raises an error without modifying the stack.

```
# We place 5 elements on the stack for the test

56 
"HELLO" 
12.34 
(1 2 3) 
true

# We store the stack elements in the local variables a b and c

('a' 'b' 'c') ->vars

# The variables a b and c have been created with the values
# a=12.34, b=(1 2 3) and c=true
# The elements "HELLO" and 56 have not been taken
```

## The `->safeVars` function

With the `->safeVars` function it is possible to verify that the values present on the stack are indeed those expected. You can verify their number and type, and automatically assign local variables with the stack values. In case of non-compliance an error is raised.

```
# We place 5 elements on the stack for the test

56 
"HELLO" 
12.34 
(1 2 3) 
true

# We store the stack elements in the local variables a b and c
# We determine which types are expected

[a: .number b: .list c: .boolean] ->safeVars

# The variables a b and c have been created with the values
# a=12.34, b=(1 2 3) and c=true
# The elements "HELLO" and 56 have not been taken
# By the way, ->safeVars verified that the value taken from the stack for the variable
# 'a' is of type .number, for 'b' of type .list and for 'c' of type .boolean.
```
 
This function is used automatically when you declare a function with the `with` keyword:

```
to 'carre' with [x: .number] do « x x * »

5 carre 

# Will place 25 on the stack
```

## The `->params` function

The `->params` function allows you to pass named parameters (key/value pairs in a record) and verify that the expected parameters are indeed present and that their type matches. If everything is correct, the local variables corresponding to the expected parameters are automatically created with the corresponding values.

This function takes 2 records as parameters. The 1st contains the values to retrieve, the second describes the expected parameters and their type.

For example, to retrieve 2 parameters, named nom and age, nom being a character string and age a number, we will have as parameter definition record:

`[nom: .string age: .number]`

So to pass "STEPHANE" for the name and 55 for the age we will have:

`[nom: "STEPHANE" age: 55] [nom: .string age: .number] ->params`

As everything matches, **MOGWAI** will create the local variables `'nom'` with the value "STEPHANE" and `'age'` with the value 55.

```
# We pass as parameter a name of type character string
# And an age which is a number

[nom: "STEPHANE" age: 55] [nom: .string age: .number] ->params

nom ?
age ?
```

If you pass values with the wrong type, an error is raised:

```
[nom: "STEPHANE" age: "TOO OLD"] [nom: .string age: .number] ->params

# age does not have the right type
# An error is raised
```

If you pass more parameters than expected, they will simply be ignored. On the other hand, if you don't pass all the expected parameters, an error is raised.
 
To pass a parameter with any type, you must use the .any type

```
[nom: "STEPHANE" age: 55 libre: true] [nom: .string age: .number unrestricted: .any] ->params

nom ?
age ?
unrestricted ?

# Will display:
# STEPHANE
# 55
# true
```

This function is used automatically when you declare a function with the `params` keyword:

```
to 'fx' params [a: .number b: .number x: .number] do « a x * b + »

[a: 5 b: 9 x: 156] fx

# Which will place 5*156+9 or 789 on the stack
```

# Check stack conformity at the end of a function

## The `check` function

If you want to make sure that a function leaves the stack in a certain state, you can use the `check` function at the end of the function body or else where you want. It takes as parameter a list describing the expected state of the stack (type of each element to check). If the stack does not match the expected state, an error is raised.

```
# We place a number and a character string on the stack

56 "HELLO"
    
# We check that the stack is composed of a character string on top of a number
# If not, an error is raised

(.string .number) check

# The first element is the last placed on the stack, the second element is the one before, etc.
```
 
# OBJECT EVALUATION

**MOGWAI** allows you to place direct references to variables, functions and even executable code in certain objects.

Objects that can support this possibility are records, lists and character strings.

When you use direct references, they will not be automatically replaced by their value at the moment you use them.

## Evaluating a list

If you have a variable `A` with the value 100, and you place the list `(4 5 A 50)` on the stack, you will have `(4 5 A 50)` on the stack and not `(4 5 100 50)`.

For the list to use the true value of `A` you must evaluate it using the `eval` function.

So if you place `(4 5 A 50)` on the stack and use `eval` right after, you will finally have `(4 5 100 50)` on the stack.

## Evaluating a record

The same thing is possible with a record:

`[x: 10 y: 50 z: A] eval` will give `[x: 10 y: 50 z: 100]`

## Evaluating a character string

For character strings, you must use the code block notation in which you just display the name of the variable to replace.

If you need to include the value of `A` in a character string, you can for example write:

`"The value of A is {! A}" eval` which will give `"The value of A is 100"`

The `!` symbol must be stuck to the opening brace of the code block otherwise the sequence will not be recognized.

## Using code directly in objects

It is possible to use code in the objects seen previously:

```
# We will display the multiplication table of 7

0 9 for 'i' do
{
  "7 x {! i} = {! i 7 *}" eval ?
}

# Which will display
# 7 x 0 = 0
# 7 x 1 = 7
# 7 x 2 = 14
# ...
# 7 x 6 = 42
# 7 x 7 = 49
# 7 x 8 = 56
# 7 x 9 = 63
```

You can do the same with a list, with `A` having the value 100:

`(A {! A 2 *} {! A 3 *}) eval` will give `(100 200 300)`

Or with a record:

`[x: A y: {! A 2 *} z: {! A 3 *}] eval` will give `[x: 100 y: 200 z: 300]`

## Faster notation for evaluation

The `eval` function can be replaced in lists and records by the `!` symbol in first position.

If we take our previous examples:

`(! A {! A 2 *} {! A 3 *})` will give `(100 200 300)`

`[! x: A y: {! A 2 *} z: {! A 3 *}]` will give `[x: 100 y: 200 z: 300]`

It is no longer necessary to call the `eval` function, the evaluation is performed directly before placing the value on the stack.

## Evaluating a variable with `!`

When a variable contains an object that embeds executable code, using `!` as a prefix sigil evaluates it directly — without pushing it onto the stack first. This is more efficient than the equivalent `A eval` sequence and expresses intent more clearly at the call site.

```
100 -> 'A'
{ A 200 * } -> 'B'
"We are in {! now ->date year: get }" -> 'C'

!B    # → 20000
!C    # → "We are in 2026"
```

`!A` is universal: it works on blocks, functions, strings, lists and records. For plain scalar types (numbers, booleans…) it is a silent no-op.

## Containers are lazy

Everything inside a container — block, function, string, list or record — is deferred until evaluation is triggered. The container stores expressions, not values. This means `!A` on a composite object always evaluates with the **current state** of the program at the moment of the call.

```
10 -> 'A'
{ A 200 * } -> 'B'
[ x: { A 10 * }
  y: "We are in {! now ->date year: get }"
  z: !B ] -> 'R'

!R    # → [ x: 100   y: "We are in 2026"   z: 2000 ]

20 -> 'A'
!R    # → [ x: 200   y: "We are in 2026"   z: 4000 ]
```

The record `R` behaves as a **live template**: it captures intent, not state. Each `!R` is a fresh evaluation.

## Circular reference detection

Because containers are lazy, it is possible to write code where evaluating a variable triggers the evaluation of itself, directly or through a chain of variables. **MOGWAI** detects these situations automatically and raises an error instead of looping indefinitely.

```
{ !B } -> 'A'
{ !A } -> 'B'
!A    # → error: circular reference detected (A → B → A)
```

The error message includes the full chain of variable names involved in the cycle, making it easy to identify the problem.
 
# FLAGS

Flags are used to indicate a state. A flag has a name and a state that can be either activated or deactivated.

## Activating a flag

It is the `flag.set` function that activates a flag. It takes as parameter the name of the flag to activate: `'MY_FLAG' flag.set`

## Deactivating a flag

It is the `flag.clear` function that deactivates a flag. It also takes as parameter the name of the flag to deactivate: `'MY_FLAG' flag.clear`

## Checking that a flag is activated

To check if a flag is activated, you must use the `flag.isSet` function which returns `true` if the flag is activated, and `false` otherwise.

It takes as parameter the name of the flag to check: `if ('MY_FLAG' flag.isSet) then { ... }`

## Checking that a flag is deactivated

To check if a flag is deactivated, you must use the `flag.isClear` function which returns `true` if the flag is deactivated, and `false` otherwise

It takes as parameter the name of the flag to check: `if ('MY_FLAG' flag.isClear) then { ... }`

## Listing activated flags

The `flags` function returns the list of all activated flags. Deactivated flags are considered non-existent and therefore do not appear in this list.

# FILE MANAGEMENT

**MOGWAI** version 8 introduces a completely redesigned file management system. Unlike previous versions which used a node-based approach inspired by HP calculator RPL, V8 adopts a conventional path-based system that is easier to use and better aligned with modern operating systems.

The **MOGWAI** runtime can work in two ways: either it uses the predefined folder structure, or it relies on its host application or script code to provide file paths.

## Default Paths

By default, **MOGWAI** uses a specific folder structure whose root is located in the current user's `documents` folder.

Thus on Windows, in the current user's `documents` folder you will find the following structure:
```
MOGWAI/
  ├─ Programs/
  ├─ Usings/
  └─ Files/
```

The `Programs` folder contains programs, the `Usings` folder contains extension libraries (called "usings" in MOGWAI terminology, such as MOGWAI_SERIAL for example), and the `Files` folder contains data files used and created by programs.

The following functions directly return the paths to these folders:

| Function        | Usage                                                 |
|-----------------|-------------------------------------------------------|
| `path.programs` | Returns the standard programs folder.                 |
| `path.files`    | Returns the standard files folder.                    |
| `path.usings`   | Returns the standard extension libraries folder.      |
| `path.home`     | Returns the home directory path.                      |

Some file management functions will use these default paths if no path is specified.

It is possible to customize these default paths using the `path.setPrograms`, `path.setFiles`, `path.setUsings`, and `path.setHome` functions. For example, if you want your programs to be stored in a different folder, you can use `path.setPrograms` to define the new path:

```mogwai
"C:\MyPrograms" path.setPrograms
```

If the **MOGWAI** host application provides specific paths, these paths will be used instead of the default paths. These alternative folders will not be automatically created by **MOGWAI**; it is the responsibility of the host application or script code to ensure these folders exist and are accessible.

## System Folder Paths

Some important operating system folders are accessible through specific functions. For example, the `path.desktop` function returns the path to the user's desktop, while `path.documents` returns the path to the user's documents folder.

| Function             | Usage                                                                   |
|----------------------|-------------------------------------------------------------------------|
| `path.desktop`       | Returns the current user's desktop folder.                              |
| `path.documents`     | Returns the current user's documents folder.                            |
| `path.music`         | Returns the folder where the current user's music files are stored.     |
| `path.videos`        | Returns the folder where the current user's videos are stored.          |
| `path.pictures`      | Returns the folder where the current user's pictures are stored.        |
| `path.programData`   | Returns the system's 'ProgramData' folder.                              |
| `path.tempDirectory` | Returns the temporary files folder.                                     |
| `path.tempFilename`  | Returns a complete path to a new temporary file created by the system.  |

## Path Construction

To generate a file or folder path, you can use the `path.make` function. This function takes a list of path segments as an argument and combines them to create a complete path.

For example, to create a path to a file named `data.txt` in the `Files` folder of the default structure, you can use the `path.make` function as follows:

```mogwai
# Version with auto-evaluation of the segment list via the ! character at the start of the list
(! path.files "data.txt") path.make

# Result on Windows: "C:\Users\Username\Documents\MOGWAI.8\Files\data.txt"

# Version with manual evaluation of the segment list
(path.files "data.txt") eval path.make
```

## Folder Management

**MOGWAI** provides functions to manipulate folders in the file system:

| Function | Example | Usage |
|----------|---------|-------|
| `dir.exists` | `"C:\Temp" dir.exists` | Returns `true` if the folder exists. |
| `dir.create` | `"C:\Temp\MyFolder" dir.create` | Creates a new folder. |
| `dir.purge` | `"C:\Temp\MyFolder" dir.purge` | Deletes a folder and all its contents. |
| `dir.rename` | `"OldName" "NewName" dir.rename` | Renames a folder. |
| `dir.current` | `dir.current` | Returns the current working folder. |
| `dir.setCurrent` | `"C:\Projects" dir.setCurrent` | Sets the current working folder. |
| `dir.directories` | `"C:\Temp" dir.directories` | Returns the list of subfolders in a folder. |
| `dir.files` | `"C:\Temp" dir.files` | Returns the list of files contained in a folder. |

### Examples

```mogwai
# Create a working folder
(! path.files "MyProject") path.make -> 'projectDir'

if (projectDir dir.exists not) then
{
    projectDir dir.create
    "Folder created" ?
}

# List files in a folder
path.files dir.files -> 'fileList'
fileList ?

# List subfolders
path.files dir.directories -> 'dirList'
dirList ?
```

## File Management

**MOGWAI** provides two approaches for manipulating files:

### Complete Read/Write (Binary)

To read or write a complete file in a single operation:

| Function | Example | Usage |
|----------|---------|-------|
| `file.data.read` | `"data.bin" file.data.read` | Reads all binary content of a file at once. |
| `file.data.write` | `bytearray "data.bin" file.data.write` | Writes complete binary data to a file. |

### Sequential Read/Write with Handles

For sequential operations (line-by-line reading, progressive writing, large files), use file handles.

**A handle is a string** representing the unique hexadecimal identifier of the opened file stream (filestream). This handle must be kept for all subsequent operations on the file.

| Function | Example | Usage |
|----------|---------|-------|
| `file.open` | `"data.txt" file.open` | Opens a file for reading and returns a handle. |
| `file.create` | `"data.txt" file.create` | Opens a file for writing (clears the file if it exists) and returns a handle. |
| `file.append` | `"log.txt" file.append` | Opens a file for writing at the end (preserves existing content) and returns a handle. |
| `file.read` | `handle size file.read` | Reads up to `size` bytes from an open file and returns a DATA. |
| `file.readLine` | `handle file.readLine` | Reads a complete line (terminated by `\n` or `\r\n`) and returns a DATA. |
| `file.write` | `data handle file.write` | Writes data to an open file. **Does not** automatically add a line break. |
| `file.size` | `handle file.size` | Returns the total size (in bytes) of a file opened for reading. |
| `file.eof` | `handle file.eof` | Returns `true` if the end of the file opened for reading is reached. |
| `file.close` | `handle file.close` | Closes an open file. Always close files after use! |

### Conversion Between DATA and String

Text file reading functions (`file.readLine`, `file.read`) return DATA (byte arrays) that must be converted to strings according to the file's encoding. Similarly, to write text to a file, strings must first be converted to DATA.

**MOGWAI** provides conversion functions in both directions:

#### DATA to String (Reading)

| Function | Example | Usage |
|----------|---------|-------|
| `utf8->` | `data utf8->` | Converts a DATA to a string with UTF-8 encoding. |
| `ascii->` | `data ascii->` | Converts a DATA to a string with ASCII encoding. |
| `ascii7->` | `data ascii7->` | Converts a DATA to a string with ASCII 7-bit encoding. |

#### String to DATA (Writing)

| Function | Example | Usage |
|----------|---------|-------|
| `->utf8` | `string ->utf8` | Converts a string to DATA with UTF-8 encoding. |
| `->ascii` | `string ->ascii` | Converts a string to DATA with ASCII encoding. |
| `->ascii7` | `string ->ascii7` | Converts a string to DATA with ASCII 7-bit encoding. |

#### Line Breaks

`file.write` does **not** automatically add a line break. To write lines, you must manually add line break bytes to the DATA:

| Notation | Usage |
|----------|-------|
| `D:0D0A` | Windows line break (CR LF: Carriage Return + Line Feed) |
| `D:0A` | Unix/Linux/Mac line break (LF: Line Feed only) |

**Example**: `"My line" ->utf8 D:0D0A + handle file.write`

The `+` operator concatenates DATA to create a single byte array.

### File Manipulation

| Function | Example | Usage |
|----------|---------|-------|
| `file.exists` | `"data.txt" file.exists` | Returns `true` if the file exists, `false` otherwise. |
| `file.info` | `"data.txt" file.info` | Returns a record containing all file metadata. |
| `file.copy` | `"source.txt" "dest.txt" file.copy` | Copies a file. |
| `file.rename` | `"old.txt" "new.txt" file.rename` | Renames a file. |
| `file.purge` | `"data.txt" file.purge` | Deletes a file. |

#### Metadata Returned by file.info

The `file.info` function returns a record containing the following information:

| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `name` | String | File name with extension | `"FIND NUMBER.mog"` |
| `fullName` | String | Full absolute file path | `"C:\Users\...\FIND NUMBER.mog"` |
| `directoryName` | String | Path of the folder containing the file | `"C:\Users\...\Progs"` |
| `extension` | String | File extension | `".mog"` |
| `modifiedTime` | Number | Last modification date (.NET ticks) | `6.390445690514954E+17` |
| `lastAccessTime` | Number | Last access date (.NET ticks) | `6.390643650826527E+17` |
| `length` | Number | File size in bytes | `992` |
| `isReadOnly` | Boolean | Read-only file | `false` |
| `isArchive` | Boolean | Archive attribute (Windows) | `true` |
| `isHidden` | Boolean | Hidden file | `false` |
| `isSystem` | Boolean | System file | `false` |

**Note**: Timestamps are in .NET ticks (number of 100-nanosecond intervals since 01/01/0001). Use the `->date` function to convert these values to a record with `day:`, `month:`, `year:`, etc.

**⚠️ Important**: If the file does not exist, `file.info` raises an error. Use `file.exists` to check for existence before calling `file.info`.

### Examples

**Complete binary read/write:**
```mogwai
# Read an entire binary file
"image.png" file.data.read -> 'imageData'

# Write binary data
imageData "copy.png" file.data.write
```

**Sequential reading with handle:**
```mogwai
# Open a text file for reading
(! path.files "data.txt") path.make file.open -> 'fileHandle'

# The handle is a hexadecimal string, for example: "A3F5B2C8"
fileHandle ?

# Get the file size
fileHandle file.size -> 'fileSize'
"File size: {! fileSize} bytes" eval ?

# Read line by line until the end
while (fileHandle file.eof not) do
{
    # Read a line (returns a DATA)
    fileHandle file.readLine -> 'lineData'
    
    # Convert the DATA to a UTF-8 string
    lineData utf8-> -> 'line'
    
    # Display the line
    line ?
}

# Always close the file
fileHandle file.close
```

**Reading by byte blocks:**
```mogwai
# Read a file in 1024-byte blocks
"bigfile.dat" file.open -> 'h'

while (h file.eof not) do
{
    # Read up to 1024 bytes
    h 1024 file.read -> 'chunk'
    
    # Process the chunk
    chunk process
}

h file.close
```

**Sequential writing with handle:**
```mogwai
# Open a file for writing (overwrites existing content)
(! path.files "report.txt") path.make file.create -> 'h'

# Write multiple lines with UTF-8 conversion and line breaks
"=== REPORT ===" ->utf8 D:0D0A + h file.write
"Important data with accents: éàç" ->utf8 D:0D0A + h file.write
"End of report" ->utf8 D:0D0A + h file.write

# Close the file
h file.close
```

**Note**: `D:0D0A` represents the CR LF bytes (Carriage Return + Line Feed, Windows line break). For a Unix/Linux line break, use `D:0A` only. The `+` operator concatenates DATA.

**Writing with different encodings:**
```mogwai
"test.txt" file.create -> 'h'

# Line in UTF-8 (supports all characters)
"Français: éèêë" ->utf8 D:0D0A + h file.write

# Line in ASCII (basic characters)
"English: Hello" ->ascii D:0D0A + h file.write

# Line in ASCII 7-bit (strictly 7 bits)
"Basic: ABC123" ->ascii7 D:0D0A + h file.write

h file.close
```

**Append mode (adding to the end):**
```mogwai
# Add to an existing file (log mode)
(! path.files "debug.log") path.make file.append -> 'logHandle'

"[2025-02-10 14:30] New log entry" ->utf8 D:0D0A + logHandle file.write

logHandle file.close
```

**Managing multiple files simultaneously:**
```mogwai
# Open one file for reading and another for writing
(! path.files "input.txt") path.make file.open -> 'handleIn'
(! path.files "output.txt") path.make file.create -> 'handleOut'

# Read line by line from input and write to output
while (handleIn file.eof not) do
{
    # Read a line and convert to UTF-8
    handleIn file.readLine utf8-> -> 'line'
    
    # Process the line (example: convert to uppercase)
    line upper -> 'processedLine'
    
    # Convert back to DATA, add line break and write
    processedLine ->utf8 D:0D0A + handleOut file.write
}

# Close both files
handleIn file.close
handleOut file.close
```

**Copying and manipulating files:**
```mogwai
# Copy a file
(! path.files "original.txt") path.make 
(! path.files "copy.txt") path.make 
file.copy

# Rename a file
(! path.files "copy.txt") path.make
(! path.files "backup.txt") path.make
file.rename

# Delete a file
(! path.files "temp.txt") path.make file.purge
```

**Getting file metadata:**
```mogwai
# Retrieve all file information
"data.txt" file.info -> 'info'

# Display the complete record
info ?

# Access specific fields
info length: get -> 'size'
"File size: {! size} bytes" eval ?

info extension: get -> 'ext'
"Extension: {! ext}" eval ?

# Convert timestamp to readable date
info modifiedTime: get ->date -> 'dateModif'
"Last modified: {! dateModif day: get}/{! dateModif month: get}/{! dateModif year: get}" eval ?

# Check attributes
info isReadOnly: get -> 'readonly'
if (readonly) then
{
    "File is read-only, cannot modify it" ?
}
```

**Checking size before loading a file:**
```mogwai
"bigfile.dat" file.info -> 'info'
info length: get -> 'size'

if (size 10000000 >) then
{
    "File too large ({! size} bytes), block processing recommended" eval ?
    
    # Process by blocks
    "bigfile.dat" file.open -> 'h'
    while (h file.eof not) do
    {
        h 1024 file.read process
    }
    h file.close
}
else
{
    "File of reasonable size, complete read" ?
    "bigfile.dat" file.data.read process
}
```

**Checking if a file was recently modified:**
```mogwai
"config.txt" file.info -> 'info'
info modifiedTime: get -> 'mtime'

# Get the current timestamp
now ->timestamp -> 'current'

# Calculate the difference (1 day = 864000000000 ticks = 86400 * 10000000)
current mtime - -> 'diff'

if (diff 864000000000 >) then
{
    "Outdated configuration (more than 24h), update recommended" ?
}
else
{
    "Configuration up to date" ?
}
```

**Handling missing files with file.exists:**
```mogwai
# Check existence before getting information
if ("config.txt" file.exists) then
{
    "config.txt" file.info -> 'info'
    info length: get ?
}
else
{
    "File config.txt not found!" ?
}

# Alternative: use guard/else to catch the error
guard
{
    "config.txt" file.info -> 'info'
    info length: get ?
}
else
{
    "File config.txt not found!" ?
}
```

# TIMERS

**MOGWAI** allows you to execute code at regular intervals. Timers manage this.

You can create as many timers as you want. Timers use their own stack and therefore cannot disturb that of your main program. The downside of this is that the code of a timer does not have access to what you may have placed on the stack and therefore cannot use it to pass parameters for example. This partitioning is necessary because as the code of a timer can be triggered at any time, it must not disturb the proper functioning of your program.

The code of a timer has access to the global variables of the program currently running.

To use a timer, you must declare it then activate it. At any time you can stop it and delete it.

> **Note**: For parallel execution of code in separate processes, see [TASKS](#tasks). Tasks provide more isolation and robust error handling compared to timers.

## Timer of type `after`

A timer of type `after` will only trigger once, after a defined period. The period is defined in milliseconds.

When it triggers, its code is executed (with its own stack) and it stops. To use it again simply restart it.

## Timer of type `every`

A timer of type `every` will trigger at regular intervals. The period is also defined in milliseconds.

When it triggers, its code is executed (with its own stack) then it is reprogrammed to trigger again after the defined period has elapsed.

## Declaring a timer

Declaring a timer is done with a very simple syntax.

For a timer of type after:

```
# We declare a timer of type after
# After 5 seconds it will display a message

timer 'timer1' after 5000 do 
{
    "Hello !" ? 
}

# We activate the timer

'timer1' timer.start

# We wait without doing anything for it to trigger

forever do
{

}
```

After 5 seconds, the message "Hello !" will be displayed, then nothing more will happen.

For a timer of type every:

```
# We declare a timer of type every
# After 5 seconds it will display a message

timer 'timer1' every 5000 do 
«
    "Hello !" ? 
»

# We activate the timer

'timer1' timer.start

# We wait without doing anything for it to trigger every 5 seconds

forever do
{

}
```

Every 5 seconds the message "Hello !" will be displayed.

Here are the available functions to manage timers:

`timer.start` activates the timer whose name is passed as a parameter: `'timer1' timer.start`

`timer.stop` stops the timer whose name is passed as a parameter: `'timer1' timer.stop`

`timer.purge` deletes the timer whose name is passed as a parameter: `'timer1' timer.purge`

`timer.state` returns true if the timer whose name is passed as a parameter is active: `'timer1' timer.state`

`timer.list` returns the list of declared timers.

## Suspending timer triggering

It may be necessary to ensure that timers do not trigger for a certain time.

The `DI` (disable interrupts) function allows you to block the triggering of timers.

To reactivate timers use the `EI` (enable interrupts) function.

The `DI` function prevents the execution of the timer code but does not suspend the timer itself, timer function is queued.

## Launching code with an execution delay

There are cases where you need to launch code after a certain delay. **MOGWAI** has a mechanism based on `after` type timers to achieve this functionality:

```
# We launch a function in 2 seconds

after 2000 do
{
    "Hello world !" ?
}

# We wait without doing anything for it to trigger

forever do
{

}
```

You have no control over the execution of the function, impossible to delete it before its execution.
 
# EVENTS

**MOGWAI** can trigger and respond to events. An event is defined by a name (for example 'MY_EVENT') and by code to execute when it is triggered.

An event can be triggered by the **MOGWAI** code currently running or by the application that hosts the engine (**MOGWAI CLI** hosts the **MOGWAI** runtime, and as such can trigger events in your code).

Your **MOGWAI** code can also generate events intended for the application that hosts the runtime. It's a way to communicate with it.

> Interactions between the runtime and the hosting application (the host) will not be covered in this documentation but in the one that explains how to integrate **MOGWAI** into a host application.

## Declaring an event

To respond to an event you must declare it. For example, to declare the event 'MY_EVENT' which will simply display "Hello !" when it is triggered, simply enter:

```
onEvent 'MY_EVENT' do
{
    "Hello !" ?
}
```

When the event 'MY_EVENT' is triggered, **MOGWAI** will execute the associated code.

The code of an event systematically has the local variable `eventData` which carries the parameter of the event (for example a name or a number). This value is provided by the one who triggers the event. If no value is associated, `eventData` carries the `null` value.

## Triggering an event

From your **MOGWAI** code you can trigger events at any time.

It is the `event.fire` function that allows you to trigger an event. It takes as parameters the name of the event and the associated parameter (if no parameter use `null`): `'MY_EVENT' null event.fire`

## Listing supported events

It is possible at any time to list all the events to which you can respond. It is the `event.list` function that handles this. It returns the list of names of declared events.

## Removing support for an event

For your **MOGWAI** application to stop responding to an event, you must delete it with the `event.purge` function which takes as parameter the name of the event to delete.

## Events and Tasks

Events are also used by **MOGWAI** to manage communication between parent and child tasks. See the [TASKS](#tasks) section for detailed information on how tasks use events for inter-process communication.

## Putting events on hold

As with timers, events can be blocked by the `DI` (disable interrupts) function. Warning, if you use the `DI` function, events are not forgotten, they are queued and when interrupts are reactivated by the `EI` (enable interrupts) function they will all be executed one after the other.
 
You can test this behavior with the following code:

```
mogwai.reset
console.clear

event 'MY_EVENT' do
{
    "Hello num {! eventData}" eval ?
}

1 100 for 'i' do
{
    'MY_EVENT' i event.fire
    1000 wait
	
    if (i 10 ==) then { DI }
	
    if (i 20 ==) then { EI }
}
```

# OBJECT-ORIENTED PROGRAMMING

**MOGWAI** provides a basic but complete object-oriented programming system. It allows you to define classes that group data and behavior, create instances from those classes, and manage their lifecycle explicitly.

This system is intentionally kept simple: no inheritance, no garbage collector. You are in full control of instance creation and destruction.

## Defining a Class

A class is defined with the `class` keyword, followed by its name as a string, the `do` keyword, and a block containing two sections:

- `private:` — private properties and methods, accessible only from within the class
- `public:` — public properties and methods, accessible from outside the class

Properties are declared with a name followed by a type (`.number`, `.string`, `.bool`, `.any`, etc.). Methods are declared with a name followed by a code block `{ }`.

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

## Properties and Methods

Within a section, **MOGWAI** distinguishes properties from methods by their declared value:

- A **type sigil** (`.number`, `.string`, etc.) declares a property. It will be initialized to `empty` regardless of its type. The type annotation is used for validation when a value is assigned. You can check whether a property has been initialized using `isEmpty`.
- A **code block** `{ }` declares a method.

The name `className:` is reserved and cannot be used as a property or method name in a class definition. Attempting to declare it raises error MW.95 (reserved property).

## Lifecycle Hooks

Two special methods are automatically called by the engine if they are defined. They can be placed in either `private:` or `public:`:

- `onInit:` is called automatically when a new instance is created with `new`. It receives the named parameters passed at creation.
- `onFree:` is called automatically just before an instance is destroyed with `free`.

## Creating and Destroying Instances

Use `new` to create an instance and `free` to destroy it.

```
# Create an instance, onInit: is called automatically
[id: 10 name: "SIBUE"] 'User' new -> '$U1'

# Destroy the instance, onFree: is called automatically
$U1 free
```

Each instance is assigned a unique internal handle (noted `§453` for instance number 453). This number is never reused during the lifetime of the engine — a destroyed instance handle is permanently invalid.

Multiple variables can hold a reference to the same instance. If the instance is destroyed, all variables pointing to it become invalid. Any attempt to use them will raise an error.

To safely test whether an instance reference is still valid before using it, use the `isAlive` predicate:

```
# Check whether an instance is still alive
$U1 isAlive   # → true or false

# Guard pattern before accessing the instance
if ($U1 isAlive) then
{
    $U1->display:
}

# Defensive check at the start of a function
if ($U1 isAlive not) then { "Instance has been freed" ? mogwai.halt }
```

`isAlive` performs an O(1) lookup in the instance registry and returns `true` if the instance is still alive, `false` otherwise. It never raises an error when called on an instance reference — but passing a value that is not an instance reference raises MW.21 (bad argument type).

## Accessing Properties and Methods

Public properties and methods are accessed with the `->` and `<-` compact notation, or with the verbose `get` and `set` forms:

```
# Read a public property
$U1->name: ?
# Equivalent to: $U1 name: get ?

# Write a public property
"DUPONT" &$U1<-name:
# Equivalent to: "DUPONT" &$U1 name: set

# Call a public method
$U1->display:
# Equivalent to: $U1 display: get
```

Attempting to access a `private:` member from outside the class raises an error.

The property `className:` is a reserved read-only public property automatically available on every instance. It returns the name of the class the instance belongs to:

```
$U1->className: ?   # → 'User'
```

Attempting to write to `className:`, or to declare it explicitly in a class definition, raises error MW.95 (reserved property).

## The `self` Variable

Inside any method, the variable `self` is automatically available and holds a reference to the current instance. It can be used to read or write the instance's own properties and to call its other methods:

```
display:
{
    "USER={! self}" eval ?
    self->show:         # calls a private method
}
```

Using `self` outside of a method raises an error.

## Validating Method Parameters

Any method can validate its inputs in three ways depending on the level of safety required.

**`->vars`** is the simplest option. It extracts values from the stack or from a record and automatically assigns them to local variables, without any type validation:

```
setCoords:
{
    ('x' 'y') ->vars

    x self<-x:
    y self<-y:
}
```

If there are not enough elements on the stack to fill all the listed variables, `->vars` raises an error.

**`->safeVars`** works like `->vars` but also validates the number and type of stack values. An error is raised immediately if the values do not match:

```
setCoords:
{
    [.number .number] ->safeVars 'x' 'y'

    x self<-x:
    y self<-y:
}
```

**`->params`** expects a named parameter record on the stack. It validates names, types, and optional default values. This is the natural choice for `onInit:` since instances are created with a named record:

```
onInit:
{
    [id: .number name: .string index: (.number 0)] ->params

    id self<-id:
    name self<-name:
    index self<-index:
}
```

If the record does not match the declared parameter names and types, `->params` raises an error immediately.

## Complete Example

```
mogwai.reset
console.clear

class 'User' do
{
    private:
    {
        x: .number
        y: .number
        z: .number

        onInit:
        {
            [id: .number name: .string] ->params

            id self<-id:
            name self<-name:

            rand 100 * ->int self<-x:
            rand 100 * ->int self<-y:
            rand 100 * ->int self<-z:
        }

        onFree:
        {
            "FREE {! self}" eval ?
        }

        show:
        {
            "ID={! self->id:}" eval ?
            "NAME={! self->name:}" eval ?
            self->show2:
        }

        show2:
        {
            "X={! self->x:}" eval ?
            "Y={! self->y:}" eval ?
            "Z={! self->z:}" eval ?
        }
    }

    public:
    {
        id: .number
        name: .string

        display:
        {
            "USER={! self}" eval ?
            self->show:
        }
    }
}

[id: 10 name: "SIBUE"] 'User' new -> '$U1'
[id: 20 name: "DUPONT"] 'User' new -> '$U2'

$U1->display:
" " ?
$U2->display:
" " ?

$U1 free
$U2 free
```

The output of this program will look like this:

```
USER §1
ID=10
NAME=SIBUE
X=42
Y=67
Z=13

USER §2
ID=20
NAME=DUPONT
X=88
Y=5
Z=71

FREE §1
FREE §2
```

## Listing All Live Instances

The `alive` function returns a list of all currently living instance references (`.objref`). This is useful for iteration, debugging, or cleanup.

```
alive ?
# → (§1 §2 §3 ...)
```

You can filter by class using `foreach...filter`:

```
alive foreach 'item' filter { item->className: 'User' == } -> '$users'
```

## Inspecting a Class Structure

The `frame` function returns a record describing the full structure of a class — its name, public and private properties, and public and private methods.

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
$F->props: ?
$F->_props: ?
$F->funcs: ?
$F->_funcs: ?
```

# TASKS

**MOGWAI** makes it very easy to create parallel tasks.
These tasks are called child tasks.

Child tasks communicate with their parent task through events (see [EVENTS](#events)). Like [TIMERS](#timers), tasks run with their own isolated stack and can be managed independently.

A child task can itself create child tasks. There is no limit other than available memory. It is recommended not to launch too many tasks in parallel to avoid saturating memory and degrading performance.

Child task code is defined in the parent task, but executes in parallel with it. The parent task can continue to do other things while child tasks are executing.

To illustrate task usage, we will use an example that downloads HTML pages in the background and saves them to disk. We will launch as many parallel tasks as there are pages to download. This will show the lifecycle of each task.

## How a Task Works

### Events for Communication

A child task cannot directly communicate with its parent task - it must use events that will be triggered in the parent task's code.

The parent task can only communicate with its child tasks via events that will be triggered in the concerned child task's code.

Child tasks have no way to talk directly to each other.
They don't know about each other and from their perspective only the parent task exists.

The events that can be triggered by a child task to its parent task are:

| Event | Usage |
|-------|-------|
| `TASK_DID_START` | Event triggered when the task has started.<br>The local variable `eventData` contains the name of the concerned task (e.g. 'T1'). |
| `TASK_DID_END` | Event triggered when the task is completed.<br>The local variable `eventData` contains a record composed of the task name (key task:) and the value returned by task.result (key result:) from the child task. |
| `TASK_DID_FAIL` | Event triggered when an error has been raised in the child task's code.<br>The local variable `eventData` contains a record composed of 3 keys: the `task:` key carrying the concerned task's name, the `error:` key carrying the error code, and the `message:` key carrying the error message. |
| `TASK_DID_PUBLISH` | Event triggered when a child task sends data to its parent task.<br>The local variable `eventData` contains a record with a `task:` key that carries the concerned task's name, and the `message:` key which contains the message. The message can be of any type supported by **MOGWAI**. |
| `TASK_DID_RECEIVE` | Event triggered ==in a child task's code== when the parent task sends it data.<br>The local variable `eventData` contains the data which can be of any type supported by **MOGWAI**. |

### Parent Task Functions

To manage child tasks, a parent task has the following functions:

| Function | Example | Usage |
|----------|---------|-------|
| `task.isRunning` | `'T1' task.isRunning` | Returns `true` if the task passed as parameter is currently executing. |
| `task.join` | `('T1' 'T2' 'T3') task.join` | Suspends the program until all listed tasks are completed. |
| `task.list` | `task.list` | Returns the list of all defined tasks, regardless of their status. |
| `task.purge` | `'T1' task.purge` | Deletes the task passed as parameter. If the task was running, it is stopped before being deleted. |
| `task.result` | `'T1' task.result` | Returns the result of the task passed as parameter. The result can be of any type supported by MOGWAI. |
| `task.start` | `'T1' task.start` | Launches the task passed as parameter without passing it any parameter. This function returns immediately. |
| `task start with` | `task 'T1' start with object` | Launches the task passed as parameter by passing it a MOGWAI object. The object is placed on the child task's stack just before launch. This function returns immediately. |
| `task send` | `task 'T1' send object` | Sends the passed object to task 'T1'. The child task receives the object via the `TASK_DID_RECEIVE` event. |
| `task.wait` | `'T1' task.wait` | Executes the task passed as parameter and waits until it completes before returning. |
| `task.stop` | `'T1' task.stop` | Stops the task passed as parameter. Stopping a child task triggers the `TASK_DID_END` event in the parent task with the result value at the moment of stop. |
 
### Child Task Functions

A child task can use the following functions:

| Function | Example | Usage |
|----------|---------|-------|
| `task.name` | | Returns the child task's name. |
| `task.publish` | `object task.publish` | Sends an object to the parent task via the `TASK_DID_PUBLISH` event.<br>The object can be of any type supported by **MOGWAI**. |
| `task.setResult` | `object task.setResult` | Sets the task's result. It can be of any type supported by **MOGWAI**. |


### Passing Parameters to a Child Task

When a parent task launches a child task, it can pass it a **MOGWAI** object as parameter. This object is placed on the child task's stack just before launch. It's up to the child task to retrieve this object.

To pass parameters to a child task, simply place them in a **MOGWAI** object and pass it to the parent task's `task 'T1' start with object` function. The child task retrieves this object because it is automatically placed on the stack at the beginning of its code.

If the child task requires no parameter, use `'T1' task.start` instead.

Warning: if you try to launch a child task that is already running, `task start with` and `task.start` will raise an error. It is recommended to verify that the task is not already running before launching it.

## Behavior on Unhandled Error

If a child task raises an error that is not handled by a `guard` or `trap` in its code, the child task is automatically stopped and the `TASK_DID_FAIL` event is triggered in the parent task with the error information as described above.

## Waiting for a Child Task to Complete

If you want to wait for a child task to complete before continuing program execution, simply use the parent task's `task.wait` function by passing it the name of the concerned child task.

The child task must have been launched with `task start with` or `task.start` beforehand for `task.wait` to work. If the child task has not been launched, `task.wait` will return immediately.

## Waiting for Multiple Child Tasks to Complete

If you want to wait for multiple child tasks to complete before continuing program execution, simply use the parent task's `task.join` function by passing it the list of concerned child tasks.

## Restarting a Completed Child Task

A child task that has been launched and has completed can be restarted. Simply call it again with `task start with` or `task.start`, optionally passing it a new object as parameter.

## Best Practices

- Always use `guard` in tasks to capture errors.
- Limit to a maximum of 50-100 simultaneous tasks.
- Use `task.setResult` to return success/failure status or other information to the parent task.
- Prefer `task.join` over waiting loops with `task.isRunning`.
- Child tasks don't know MOGWAI 8's standard paths (see example).

## Complete Example

In this example, we will download several HTML pages in parallel and save them to disk.

```
# TASK DEMO

mogwai.reset

console.clear

# Prepare the TASKDEMO download folder for the demo
# We use MOGWAI 8's standard paths for files (path.files)

(! path.files "TASKDEMO") path.make dir.create

# Declare all child task monitoring events

onEvent 'TASK_DID_START' do 
{ 
	"TASK DID START {! eventData}" eval ?
}

onEvent 'TASK_DID_END' do 
{ 
	"TASK DID END {! eventData}" eval ?
}

onEvent 'TASK_DID_FAIL' do 
{ 
	"TASK DID FAIL {! eventData}" eval ?
}

onEvent 'TASK_DID_PUBLISH' do 
{ 
	"TASK DID PUBLISH {! eventData}" eval ?
}

# Prepare the information that will be provided to each download task
# The download url
# The file to use to save the downloaded data

(
	[name: 'T1' url: "https://www.google.fr" filename: "google.bin"]
	
	[name: 'T2' url: "https://www.coding4phone.com" filename: "c4p.bin"]
	
	[name: 'T3' url: "https://www.mogwai.eu.com" filename: "mogwai.bin"]
)

foreach 'item' do
{
	item ->vars
	
	task name do
	{
		# The startup parameter (placed by task start with)
		# is in this example a record of type [url: "..." filename: "..."]
		# which allows it to retrieve which url to download and in which file to save the information
		
		# ->vars will take the record and create a corresponding local variable for each key
		# The local variables url and filename will be automatically created with the values carried by the record
		
		->vars
		
		now -> 'begin'
		
		[! http.get uri: url] -> 'r'
		
		now begin - ->duration -> 'd'
		
		if (r->state) then
		{
			"Download duration: ({! d->ms:} ms)" eval task.publish
			
			guard
			{			
				(! path.files filename) path.make r->response: file.data.write
				true task.setResult
			}
			else
			{
				false task.setResult
			}
		}
		else
		{
			"Download error!" task.publish
			
			false task.setResult
		}
	}
	
	# We must provide the child task with the complete path
	# A child task doesn't know MOGWAI 8's standard folders
	
	(! path.files "TASKDEMO" filename) path.make -> 'filename'
	
	# We launch the task by providing it with the record containing the information
	# it needs
	
	task name start with [! url: url filename: filename]
}

('T1' 'T2' 'T3') task.join

"PROGRAM COMPLETED" ?
```

This program creates three download tasks in parallel. Each task downloads an HTML page and saves it to disk. Task monitoring events are triggered to display messages in the console at each stage of the child tasks' lifecycle. Finally, the program waits until all tasks are completed before displaying "PROGRAM COMPLETED".

The console output of this program will look like this:

```
TASK DID START 'T3'
TASK DID START 'T2'
TASK DID START 'T1'
TASK DID PUBLISH [task: 'T2' message: "Download duration: (475 ms)"]
TASK DID END [task: 'T2' result: true]
TASK DID PUBLISH [task: 'T1' message: "Download duration: (579 ms)"]
TASK DID END [task: 'T1' result: true]
TASK DID PUBLISH [task: 'T3' message: "Download duration: (807 ms)"]
TASK DID END [task: 'T3' result: true]
PROGRAM COMPLETED
```

# SKILLS

A *skill* is a name declared by the host application that embeds MOGWAI, identifying a capability available in that specific execution context. Skills allow a script to verify at startup that it is running in the right environment before executing.

For example, an application like GIZMO (a TUI interface builder powered by MOGWAI) can declare a skill `'APP_GIZMO'`. A script written for GIZMO can then check for that skill at the top and exit cleanly with an informative message if it is missing.

## Querying available skills

The `skills` function returns the complete list of skills declared in the current context:

```
skills ?   # → ('APP_GIZMO' 'TUI' 'BLE')
```

The `hasSkill` function tests whether a specific skill is present and returns a boolean:

```
if ('APP_GIZMO' hasSkill) then
{
    # GIZMO-specific code
}
```

## Asserting a required skill

The `mogwai.assertSkill` function checks for a skill and stops execution with an error message if it is absent. It is the recommended way to declare script prerequisites:

```
'APP_GIZMO' "This script requires GIZMO to run." mogwai.assertSkill
'BLE' "This script requires BLE support." mogwai.assertSkill

# rest of the script...
```

If a required skill is missing, `mogwai.assertSkill` raises MW.9 (`assert error`) and calls `MOGWAI.onError` if it is defined. If all required skills are present, `mogwai.assertSkill` is a no-op.

## Skills in `mogwai.info`

The skills available in the current context are also accessible via the `skills:` key of the `mogwai.info` record:

```
mogwai.info -> '$info'
$info skills: get ?   # → ('APP_GIZMO' 'TUI')
```



