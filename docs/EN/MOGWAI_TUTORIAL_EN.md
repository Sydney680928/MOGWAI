# MOGWAI, Step by Step

A hands-on introduction to the MOGWAI scripting language, one small idea at a time.

> This guide assumes you already know how to program — variables, loops, functions, basic data types. What it does **not** assume is any familiarity with stack-based (RPN) languages. That's the one real learning curve here, and we'll take it slow.

---

## 1. What is MOGWAI?

MOGWAI is a lightweight, embeddable scripting engine for .NET. You drop it into an application — desktop, mobile, server, IoT device — and it gives that application a small, safe, extensible language of its own.

That's a deliberately broad description, because MOGWAI itself is deliberately general-purpose. It's not tied to any particular kind of application or industry. People use it for things like:

- letting end users customize behavior in an app without shipping a new build
- driving small automation workflows or scripted sequences
- scripting game logic or simple games (there's a complete Snake implementation written entirely in MOGWAI)
- building small interactive tools — calculators, TUI apps, REPLs
- exposing a safe, sandboxable scripting surface inside a larger .NET codebase

Under the hood, MOGWAI is a **stack-based, concatenative language** — the same family as Forth, Factor, PostScript, and the RPN calculators some of us grew up with. That heritage is where the "RPN" in its description comes from, but don't let it narrow your idea of what MOGWAI is *for*. The stack-based design is an implementation choice that happens to make the language extremely simple and unambiguous — there's no operator precedence to memorize, no parsing ambiguity. It's a means, not the point.

A few practical facts before we start:

- MOGWAI runs inside a **host application** written in C# / .NET. The host embeds a `MogwaiEngine` and runs scripts through it.
- Every MOGWAI script is plain text. Comments start with `#`.
- MOGWAI ships with **over 300 built-in primitives** covering math, strings, lists, records, files, HTTP, regular expressions, dates, binary data, and more. This tutorial only needs a handful of them to get you comfortable; the rest is reference material for later.

You can try everything in this tutorial without installing anything, using the [online playground](https://sydney680928.github.io/MOGWAI/), or by running the MOGWAI CLI locally.

---

## 2. Thinking in a Stack

This is the one concept worth genuinely pausing on. Once it clicks, everything else in MOGWAI follows naturally.

### Forget "function(arguments)" for a moment

In most languages you write, you nest calls inside one another: `add(multiply(3, 4), 2)`. To read that, you work from the inside out, and the order of evaluation isn't the order you read the text in.

MOGWAI does away with that entirely. There's a single **stack** — think of it as a pile of plates. You can only ever look at, add to, or remove from the top. A MOGWAI script is a sequence of instructions read strictly left to right:

- a **value** (a number, a string, ...) gets **pushed** onto the top of the stack
- an **operator or function** **pops** however many values it needs off the top, does its work, and **pushes the result** back

That's the entire execution model. No exceptions, no precedence rules.

### A first calculation, step by step

Let's trace `3 4 + 2 *` one token at a time:

```
3            # push 3           → stack: [ 3 ]
4            # push 4           → stack: [ 3 4 ]
+            # pop 4 and 3, push 3+4    → stack: [ 7 ]
2            # push 2           → stack: [ 7 2 ]
*            # pop 2 and 7, push 7*2    → stack: [ 14 ]
```

At the end, `14` sits on top of the stack. Written as a one-liner, that's:

```
3 4 + 2 * ?
```

The trailing `?` is MOGWAI's "print the top of the stack" instruction — we'll use it constantly in this tutorial to see results. This line displays `14`.

This is called **Reverse Polish Notation (RPN)**: the operator comes *after* its operands, instead of between them. `3 4 +` reads as "3, 4, add" rather than "3 + 4" — but it produces exactly the same result. The tokens on the page are, quite literally, the order of execution. What you see is what happens — nothing is evaluated out of order, nothing needs a mental "inside-out" pass.

### Why bother?

Two very concrete benefits fall out of this:

- **Zero ambiguity.** There is no operator precedence to remember, because there's no precedence at all — just left-to-right execution. `3 4 + 2 *` can only ever mean one thing.
- **Composability.** Small pieces chain together naturally. Any sequence of instructions that leaves one clean value on the stack can be dropped into a larger sequence, exactly like plugging one pipe into another.

### Try a few more

```
5 3 - ?          # → 2      (5, 3, subtract)
10 2 /  ?        # → 5      (10, 2, divide)
2 3 4 + * ?      # → 14     (2, then 3+4=7, then 2*7=14)
```

That last one is worth tracing by hand: push `2`, push `3`, push `4`, `+` pops `4` and `3` and pushes `7` — stack is now `[ 2 7 ]` — then `*` pops `7` and `2` and pushes `14`.

### Not ready to convert every formula by hand? You don't have to.

MOGWAI includes a `calc` primitive that accepts a classic infix expression — parentheses, operator precedence, the works — as a string, and evaluates it for you:

```
"5 * 3 + (7 + 2)" calc ?      # → 24
```

This is a genuinely useful bridge while you're still building RPN intuition, and plenty of real MOGWAI code leans on it for anything formula-heavy. We'll come back to it in more detail later. For the rest of this tutorial, though, we'll stick to native RPN — it's worth building the habit early, and once you've traced a few of these by hand, it stops feeling unusual very quickly.

---

*Next: writing and running your first complete MOGWAI program.*

---

## 3. Your First Program

A MOGWAI script is just a plain-text sequence of instructions, executed top to bottom, left to right. There's no `main` function, no boilerplate — the script *is* the program.

### The one habit to build immediately

Before anything else, get into the habit of starting every script with:

```
mogwai.reset
```

This gives you a perfectly clean engine: no leftover variables, no running timers, no pending tasks — nothing carried over from a previous run. It matters less in a one-shot embedding scenario, but it matters a lot the moment you're experimenting interactively (the MOGWAI CLI, the online playground) where state would otherwise silently pile up between runs. Cheap habit, real payoff — just always put it first.

### Hello, MOGWAI

```
mogwai.reset

"Hello from MOGWAI!" ?
```

Two things happen here: the string `"Hello from MOGWAI!"` is pushed onto the stack, and `?` pops it and prints it, followed by a newline. That's the whole program.

### The two display instructions

You'll use these two constantly, so let's be precise about them from the start:

- `?` — prints the top of the stack, **with** a trailing newline. Shorthand for `console.println`.
- `??` — prints the top of the stack, **without** a trailing newline. Shorthand for `console.print`.

Both accept *any* type directly — a number, a string, a boolean, a list — no conversion needed:

```
mogwai.reset

"Result: " ??
2 3 + ?
```

This prints:

```
Result: 5
```

> **Note if you're using the online playground.** The [Blazor-based playground](https://sydney680928.github.io/MOGWAI/) renders its output a line at a time, so `??` behaves the same as `?` there — every print ends up on its own line, regardless of which one you used. The distinction is real and matters in most host environments (console apps, the CLI, embedded apps); just don't be surprised if you don't see it in the playground specifically.

### A slightly bigger first program

Let's put a few things together — nothing here has been introduced yet in detail (variables and functions are next), but it should already read fairly naturally:

```
mogwai.reset

"MOGWAI says hello!" ?
"The answer to a few small calculations:" ?

3 4 + ?          # → 7
10 2 / ?         # → 5
2 8 * ?          # → 16
```

Run it, and you should see:

```
MOGWAI says hello!
The answer to a few small calculations:
7
5
16
```

That's a complete, valid MOGWAI program. Everything from here on is about giving you more building blocks to put inside one.

---

## 4. Variables

### Storing a value

A variable is created the first time you assign it a value — there's no separate declaration step required. Assignment uses the `->` operator, with the value first (it's coming off the stack, remember) and the variable's name in quotes:

```
mogwai.reset

500 -> 'A'
A ?              # → 500
```

Note the asymmetry: when you *assign* to `A`, you write its name in quotes, `'A'` — you're naming a target, not reading a value. When you *read* `A`, you write it bare, no quotes — that pushes its current value onto the stack.

A variable isn't locked to the type of its first value, either — assigning something new simply replaces both the value and, if needed, the type:

```
mogwai.reset

500 -> 'A'
A ?                    # → 500

"Hello!" -> 'A'
A ?                    # → Hello!
```

### Local vs. global

That's a **local** variable — it only exists for the duration of the current script execution (or the current function call, as we'll see later). If you prefix the name with `$`, it becomes **global** instead:

```
mogwai.reset

500 -> '$R'
$R ?             # → 500
```

The practical difference: when the host engine is set up with `keepAlive: true` (typically for interactive use — a REPL, the online playground), global variables survive across multiple separate script executions, while locals are scoped to a single run. For a one-shot embedding scenario, this distinction matters less — but it's worth knowing the `$` prefix is what it means, since you'll see it throughout MOGWAI code and examples.

### Using a variable in a calculation

Reading a variable just means writing its bare name — it pushes its current value onto the stack like any other value would:

```
mogwai.reset

20 -> 'A'
30 -> 'B'

A B + -> 'C'
C ?              # → 50
```

### Locking a type, requiring declarations

Two things are worth knowing about even if we won't dwell on them here — you'll run into both in real MOGWAI code:

A variable can be locked to a single type from the start, using `=>` instead of `->`:

```
500 => 'A'
```

From that point on, `A` only ever accepts numbers — assigning it a string would raise an error instead of silently changing its type.

Separately, an engine can be told to *require* that every variable be declared before it's used, with `mogwai.strict`:

```
true mogwai.strict
100 => 'A'
A ?
```

Once strict mode is on, using a variable that was never declared raises an error rather than silently creating it. Both of these are optional safety nets rather than something you need from day one — we're only naming them here so the notation doesn't look unfamiliar later.

### Deleting a variable

```
mogwai.reset

10 -> 'A'
'A' purge
```

`purge` removes a variable explicitly. In practice you rarely need it for locals — they disappear automatically once their scope ends — but it's there when you want to free something deliberately, or reclaim a name.

> **A note on what's coming later.** You'll sometimes see variables written with extra prefixes in MOGWAI code — `@A`, `&A`, `!A`. These are all still "the variable A" underneath, just accessed in different ways (a faster read, an in-place mutation, an immediate evaluation). We're deliberately leaving those out for now — plain `A` is all you need to be productive, and we'll come back to the others once functions and containers (lists, records) are on the table, where they actually start to matter.

---

*Next: the handful of basic types every MOGWAI value carries, and how to tell them apart.*

---

## 5. Basic Types

Every value in MOGWAI carries a type, and every type name starts with a dot: `.number`, `.string`, `.boolean`, and so on. You can ask any value for its type with `->type`:

```
mogwai.reset

1567 ->type ?         # → .number
"Hello" ->type ?      # → .string
true ->type ?         # → .boolean
```

For this tutorial, three types matter right away:

| Type | What it is | Example |
|------|------------|---------|
| `.number` | A number — MOGWAI doesn't distinguish integers from decimals, it's all one numeric type | `154` or `-56.34` |
| `.string` | A character string | `"Hello world"` |
| `.boolean` | A truth value | `true` / `false` |

You've already been using all three without needing to think about it — `3 4 +` works on `.number` values, `"Hello!" ?` works on a `.string`.

A type check is often used to branch on, exactly the way you'd expect:

```
mogwai.reset

234 -> 'A'
if (A ->type .number ==) then { "A is a number" ? } else { "A is not a number" ? }
```

(`if` / `then` / `else` get their own proper introduction in the control flow section — for now, just notice `->type .number ==` reads naturally left to right: get the type of `A`, compare it to `.number`.)

Beyond these three, MOGWAI has several more types you'll meet as this tutorial goes on — `.list`, `.record`, `.function`, `.code`, `.data`, and a few others — each with its own section ahead. There's no need to memorize the full list now; `->type` is always there when you want to check what you're actually holding.

---

## 6. Strings

Strings get a fair amount of dedicated support in MOGWAI — this section covers the everyday operations; there's a much larger family of `str.*` primitives in the function reference for anything more specialized.

### Concatenation

`+` concatenates strings — and it's a little smarter than a plain string operator, since it also accepts a number on either side and converts it automatically:

```
mogwai.reset

"HELLO " "WORLD" + ?      # → HELLO WORLD
"HELLO" 3 + ?             # → HELLO3
3 "HELLO" + ?             # → 3HELLO
```

### Extracting part of a string

A handful of primitives cover the common cases:

```
mogwai.reset

"HELLO WORLD" 0 5 sub ?       # → HELLO       (from index 0, 5 characters)
"HELLO WORLD" first ?         # → H
"HELLO WORLD" last ?          # → D
"HELLO WORLD" 3 left ?        # → HEL
"HELLO WORLD" 3 right ?       # → RLD
"HELLO WORLD" butfirst ?      # → ELLO WORLD
"HELLO WORLD" butlast ?       # → HELLO WORL
```

### Size and searching

```
mogwai.reset

"HELLO WORLD" size ?          # → 11

"HELLO WORLD" "O" where ?     # → (4 7)   — every position where "O" occurs
```

### Case and joining

```
mogwai.reset

"HELLO WORLD" ->lower ?              # → hello world
"hello world" ->upper ?              # → HELLO WORLD

("X" "Y" "Z") ";" join ?             # → X;Y;Z
"X;Y;Z" ";" split ?                  # → (X Y Z)
```

### Building strings from variables — interpolation

Rather than concatenating pieces by hand, you can write a template string with interpolation blocks — `{! ... }` — and let MOGWAI fill them in for you with `eval`:

```
mogwai.reset

"DOE John" -> 'name'
50 -> 'age'

"{! name} is {! age} years old" eval ?

# → DOE John is 50 years old
```

Anything between `{! }` is evaluated as ordinary MOGWAI code, not just a bare variable — so you can chain operations right there:

```
mogwai.reset

"DOE John" -> 'name'

"Name in caps: {! name ->upper}" eval ?

# → Name in caps: DOE JOHN
```

### Escape sequences

Inside a string literal, a backslash introduces an escape sequence — the usual suspects are there: `\"` for a literal quote, `\\` for a literal backslash, `\n` for a newline, `\t` for a tab. These are resolved when the string is evaluated:

```
mogwai.reset

"Hello, \"World\" !" eval ?     # → Hello, "World" !
"Line1\nLine2" eval ?           # → Line1 and Line2, on two separate lines
```

---

*Next: making decisions and repeating actions — conditions and loops.*

---

## 7. Control Flow

### Conditions with `if` / `then` / `else`

`if` takes a test in parentheses, a block to run `then`, and optionally a block to run `else`:

```
mogwai.reset

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

Read the test the same way you'd read any RPN expression: `A 50 ==` pushes `A`, pushes `50`, then pops both and pushes `true` or `false`. The test **must** leave a boolean on the stack — `if ("TOTO") then {...}` raises an error, since a string isn't a valid condition.

Here's the comparison and boolean toolkit you'll use inside those parentheses:

| Expression | Meaning |
|------------|---------|
| `X Y ==`  | Is X equal to Y? |
| `X Y !=`  | Is X different from Y? |
| `X Y >`   | Is X greater than Y? |
| `X Y <`   | Is X less than Y? |
| `X Y >=`  | Is X greater than or equal to Y? |
| `X Y <=`  | Is X less than or equal to Y? |
| `X not`   | Logical NOT of X |
| `X Y or`  | X OR Y |
| `X Y and` | X AND Y |
| `X Y xor` | X XOR Y |

These combine exactly the way you'd expect, left to right:

```
mogwai.reset

15 -> 'age'

if (age 18 >= age 65 < and) then
{
    "Standard rate applies" ?
}
```

### Avoiding a cascade of `if` / `else`: `switch`

When you have several mutually exclusive conditions, `switch` reads better than a chain of `if` / `else if`. It's a series of `(test) then { ... }` pairs; the **first** test that returns `true` runs its block, and only that one:

```
mogwai.reset

150 -> 'a'

switch
{
    (a 100 <) then
    {
        "< 100" ?
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

# → < 200
```

That last `(true) then { ... }` is the common way to write a default / catch-all branch — a test that can never fail.

### One small notation to know before we loop: `++` and `--`

Loops constantly need a counter to bump. Rather than writing `A 1 + -> 'A'` every time, MOGWAI gives you a shorthand — pass the **quoted** variable name to `++` or `--` and it's incremented or decremented in place:

```
mogwai.reset

100 -> 'A'
'A' ++
A ?              # → 101
```

You'll see `'I' ++` throughout the loop examples below.

### `repeat` — run a block a fixed number of times

```
mogwai.reset

0 -> 'I'

10 repeat
{
    'I' ++
    I ?
}

# → 1 2 3 4 5 6 7 8 9 10, each on its own line
```

### `for` — a managed loop counter

`for` handles the counter variable itself — you just give it a start value, an end value, and a name:

```
mogwai.reset

1 10 for 'I' do
{
    I ?
}

# Counting down instead, with an explicit step
10 1 for 'I' step -1 do
{
    I ?
}
```

### `while` and `do … while`

`while` tests the condition **before** each iteration; `do … while` tests it **after**, so the block always runs at least once:

```
mogwai.reset

0 -> 'I'

while (I 100 <) do
{
    'I' ++
    I ?
}
```

```
mogwai.reset

0 -> 'I'

do
{
    'I' ++
    I ?
} while (I 100 <)
```

### `forever` — and how to actually stop it

```
mogwai.reset

0 -> 'I'

forever do
{
    'I' ++
    I ?

    if (I 456 ==) then { break }
}
```

`break` exits the innermost loop immediately — it works the same way in every loop type above, whenever you need to leave before the natural end condition.

### `foreach` — walking a list

`foreach` comes in three flavors, and the one you reach for depends on what you're trying to produce:

```
mogwai.reset

# Just visit each element — foreach...do
("L1" "L2" "L3") foreach 'item' do { item ? }

# Build a new list by transforming each element — foreach...transform
(1 2 3 4 5) foreach 'item' transform { item 2 * }
# → (2 4 6 8 10)

# Keep only the elements matching a condition — foreach...filter
(1 2 3 4 5 6 7 8 9 10) foreach 'item' filter { item 2 mod 0 == }
# → (2 4 6 8 10)
```

`foreach...do` runs on the main stack, exactly like the rest of your script — nothing surprising there. `foreach...transform` and `foreach...filter` are a bit more particular: each iteration runs on its **own isolated stack** rather than the main one, so it can freely read local and global variables but can't reach into or leave anything on the stack outside the block. What it does leave behind — the transformed value, or the boolean deciding inclusion — is what the loop collects into the resulting list. We'll come back to lists properly in the next section; this is just enough to make these loops make sense when you see them.

---

*Next: lists — MOGWAI's ordered collections, and the operations that go with them.*

---

## 8. Lists

A list is an ordered collection of values — and unlike some languages, a MOGWAI list isn't typed to hold only one kind of thing. Lists are written with parentheses, elements separated by spaces (no commas):

```
(1 2 7)                              # a list of numbers
("X1" "X2" "X3")                     # a list of strings
("X1" 45 (1 2 3) true)               # a mix — a list can even contain lists
```

### Creating a list

The literal notation above is the simplest way. You can also build one from values already on the stack, telling `->list` how many to gather:

```
mogwai.reset

10 20 30 40 50 5 ->list ?      # → (10 20 30 40 50)
```

### Adding an element

`+` appends to a list — and if you append another list, it goes in as a single nested element rather than being flattened:

```
mogwai.reset

(10 20 30) 40 + ?              # → (10 20 30 40)
(10 20 30) (100 200) + ?       # → (10 20 30 (100 200))
```

### Reading and writing by index

Indexes are zero-based. `get` reads, `set` writes — and returns the modified list rather than mutating in place:

```
mogwai.reset

(10 20 30 40 50 60 70) 5 get ?          # → 60

(10 "E" 55 20 30) 2 "Z" set ?           # → (10 "E" "Z" 20 30)
```

Asking `get` for an index outside the list's range raises an error (**MW.22**, bad argument value) — lists are strict about that. As we'll see next, records are more forgiving when a key doesn't exist.

### Size, first, last

```
mogwai.reset

(10 20 30 40) size ?                    # → 4
(10 20 30 40 50 60 70) first ?          # → 10
(10 20 30 40 50 60 70) last ?           # → 70
```

### Sorting

`sort` works when every element shares the same type — numbers, strings, and a few identifier-like types. Mixed-type lists are returned unchanged rather than raising an error:

```
mogwai.reset

(1 10 2 5) sort ?      # → (1 2 5 10)
```

### Searching

`contains` answers a simple yes/no; `where` tells you every position a value occurs at:

```
mogwai.reset

("L1" "L2" "L3" "L4") "L4" contains ?     # → true

(10 20 "XX" "EA" 670 true "XX") "XX" where ?    # → (2 6)
```

### A preview: reaching into nested structures in one step

Once lists start containing records (key/value structures — properly introduced next), you'll often want a value that's several levels deep. Rather than pulling it out step by step, you can hand `get` a **path** — a list of indexes and keys to follow — and it resolves the whole thing in one operation:

```
mogwai.reset

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) (1 name:) get ?

# → SMITH
```

We'll see in a moment that records handle a missing key more gently than lists handle a bad index — worth keeping in mind once you start combining the two.

That's the everyday list toolkit. There's a longer tail of list-related primitives in the function reference — `insert`, `extract`, `sub`, converting a list to a byte array, and more — for when you need them.

---

*Next: records — MOGWAI's key/value structures, and how they pair naturally with lists.*

---

## 9. Records

A record is MOGWAI's key/value structure — think of it as a dictionary, or the fields of an object. Records are written with square brackets, and each entry is a **key** (a name ending in `:`) followed by its value:

```
[x: 100 y: 50]              # a record with two keys, x: and y:
[]                           # an empty record
```

A key can only appear once — if you write it twice, the last value wins: `[x: 10 x: 100]` is the same as `[x: 100]`.

### Reading a value

`get` takes the record and the key:

```
mogwai.reset

[x: 100 y: 200] y: get ?      # → 200
```

### Adding or modifying a key

`set` also works the same way for both cases — adding a key that doesn't exist yet, or overwriting one that does:

```
mogwai.reset

[x: 100 y: 200] z: 300 set ?      # → [x: 100 y: 200 z: 300]
[x: 100 y: 200] y: 2000 set ?     # → [x: 100 y: 2000]
```

### A gentler `get` than lists

This is the one place records and lists genuinely diverge: asking a list for an out-of-range index raises an error, as we saw a moment ago — but asking a record for a key that doesn't exist simply returns `null`, with no error raised. Worth keeping straight, since it's easy to assume both containers behave the same way.

### Reaching into nested structures in one step

Same idea as the list "buried path" preview from the previous section — a record made of nested records (or lists) can be navigated in a single `get`, by handing it a path instead of a single key:

```
mogwai.reset

[id: 1 name: "DOE" gps: [latitude: 45 longitude: 5]] (gps: latitude:) get ?

# → 45
```

### The rest of the everyday toolkit

```
mogwai.reset

[x: 100 y: 200] size ?                     # → 2                (number of keys)
[x: 100 y: 200] keys ?                     # → (x: y:)          (list of keys)

[x: 100 y: 200 z: 70 u: 10] (x: y:) extract ?    # → [x: 100 y: 200]
# Asking for a key that doesn't exist doesn't raise an error either —
# it's simply included in the result with the value null:
[x: 20 y: 20 z: 100] (x: t:) extract ?           # → [x: 20 t: null]

[x: 10 y: 20] y: contains ?                # → true
[x: 10 y: 20] x: purge ?                   # → [y: 20]
```

### A shorthand worth knowing early: `->key:` and `<-key:`

Because reading and writing a record field by name is so common, MOGWAI has compact shorthands for both — a variable name followed directly by `->` or `<-` and the key. Reading:

```
mogwai.reset

[x: 10 y: 20] -> 'R'
R->y: ?          # → 20, exactly equivalent to: R y: get ?
```

Writing works the same way, with the new value pushed first — but note that on its own, `<-` doesn't touch the original variable. It leaves the **modified copy** on the stack, exactly like `set` does:

```
mogwai.reset

[x: 10 y: 20] -> 'R'
1000 R<-y: ?        # → [x: 10 y: 1000] — R itself is still [x: 10 y: 20]
```

If you actually want `R` itself updated, that's where the `&` sigil comes in — `&R<-y:` mutates `R` in place instead of handing back a copy. We've been deferring `&` on purpose; it gets a proper introduction, alongside everything else it does, in the sigils section coming up.

---

*Next: functions — declaring your own behavior, from the simplest form to fully validated, named parameters.*

---

## 10. Functions

Everything you've called so far — `+`, `sort`, `str.upper`, `console.println`... — is a function MOGWAI already provides. This section is about writing your own.

### Declaring a basic function

`to 'name' do { ... }` declares a function. The block takes whatever it needs straight off the stack, exactly the way any built-in primitive does:

```
mogwai.reset

to 'square' do { dup * }

5 square ?          # → 25
```

`dup` duplicates the top of the stack — so `5 square` runs as: push `5`, `dup` it (stack is now `[5 5]`), then `*` multiplies them. A function is a `.function` value under the hood — you're free to call one from inside another, the way `cube` reuses `square` here:

```
mogwai.reset

to 'square' do { dup * }
to 'cube' do { dup square * }

5 cube ?          # → 125
```

### Two ways to call any function: native RPN or classic-style

You've already seen this pattern for built-in primitives, and it applies identically to functions you declare: arguments first in native RPN, or the function name first with parentheses in the more familiar classic style.

```
mogwai.reset

to 'square' do { dup * }

5 square ?         # native RPN
square(5) ?        # classic-style — strictly equivalent
```

For several arguments, they're simply space-separated inside the parentheses — remember, never commas:

```
mogwai.reset

to 'fx' with [a: .number b: .number x: .number] do { a x * b + }

5 9 156 fx ?           # native RPN
fx(5 9 156) ?           # classic-style
```

(`fx` here uses typed parameters — introduced properly just below. The calling convention is the same regardless of how the function was declared.)

### Verifying parameter types

A basic function trusts whatever is on the stack. If you'd rather have MOGWAI check the types for you — and give the parameters names instead of digging them out with `dup`/`swap`-style stack juggling — declare it `with` a list of typed parameters:

```
mogwai.reset

to 'square' with [x: .number] do { x dup * }

5 square ?          # → 25

"EEE" square ?
# raises an error:
#   bad argument type
#   .number expected but .string found for 'x' parameter
```

Calling it with too few values on the stack raises a **too few arguments** error instead — the check happens before the body ever runs.

Multiple typed parameters are declared the same way, in the order they're expected on the stack:

```
mogwai.reset

# y = a*x + b, i.e. in RPN: a x * b +
to 'fx' with [a: .number b: .number x: .number] do { a x * b + }

5 9 156 fx ?          # → 789
```

If you need a parameter whose type shouldn't be checked at all, use `.any`:

```
mogwai.reset

to 'nPrint' with [x: .any] do
{
    if (x ->type .number ==) then
    {
        "It is a number !" ?
    }
    else
    {
        "It is not a number !" ?
    }
}

234 nPrint      # → It is a number !
true nPrint     # → It is not a number !
```

### Named parameters

Positional parameters get less readable as their count grows — was the third argument `x` or `b`? Named parameters solve that by passing everything through a single record instead, with the keys as names. Declare with `params` instead of `with`:

```
mogwai.reset

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

[a: 5 b: 9 x: 156] fx ?          # → 789
```

Named-parameter functions get their own classic-style sugar too — square brackets instead of parentheses, and the function name can go either right before the record or as the record's first entry:

```
mogwai.reset

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

[a: 5 b: 9 x: 156] fx ?       # native — record, then function
fx[a: 5 b: 9 x: 156] ?        # classic-style
[fx a: 5 b: 9 x: 156] ?       # function name as the record's first entry
```

All three are strictly equivalent — pick whichever reads best in context.

### Default values

Give a parameter a default by pairing its type with a value in a small list, `(.type default)`. If the caller's record doesn't include that key, the default is used instead — and any extra keys the caller *does* provide beyond the declared ones are simply ignored:

```
mogwai.reset

to 'foo' params [id: .number name: .string save: (.boolean true)] do
{
    "id   = {! id}" eval ?
    "name = {! name}" eval ?
    "save = {! save}" eval ?
}

[foo id: 10 name: "DOE John"]
# save = true (default)

[foo id: 30 name: "SMITH Mike" save: false]
# save = false (explicitly provided)
```

### One gotcha to remember: functions expecting a single list

This one applies to any call written classic-style, built-in or user-defined: the parentheses just move whatever's inside onto the stack — they don't group values into a list. A function whose *whole* parameter is one list (`max`, `min`, `sum`, `sort`, or one you write yourself that way) needs that list wrapped in its own parentheses inside the call:

```
mogwai.reset

max((1 2 3)) ?      # correct — inner (1 2 3) is the list, outer () is the call
max(1 2 3) ?         # wrong — pushes 3 separate values, not a list
```

### Checking what a function returns

Just as `with` checks the types going in, `returns` checks the type coming out — add it before `do`, with the expected type(s) in a list:

```
mogwai.reset

to 'square' with [x: .number] returns (.number) do { x dup * }
```

`returns` works alongside any of the declaration styles above — basic, `with`, or `params`.

### Listing what you've declared

```
mogwai.reset

to 'square' do { dup * }
to 'cube' do { dup square * }

funcs ?          # → ('square' 'cube')
```

Handy for checking a function exists before relying on it — useful once you start writing code that composes functions dynamically.

---

*Next: a handful of advanced notations — `&`, `!`, `@`, and `-->` — that make working with variables and containers faster and more expressive.*

---

## 11. Advanced Sigils

Back in the variables section, we deliberately left a few notations for later — plain `A` was all you needed to get productive. Now that functions and containers (lists, records) are on the table, these actually earn their keep. There are four ways to read a variable in MOGWAI:

| Notation | Behavior |
|----------|----------|
| `A`  | Pushes a **copy** of A's value onto the stack |
| `&A` | Pushes a **reference** to A, for in-place mutation |
| `@A` | A statically resolved read — faster, same result as `A` |
| `!A` | Evaluates A's content directly |

### `&` — mutating a variable in place

Plain `A` always gives you a copy. Transform it, and you have to explicitly store the result back:

```
mogwai.reset

"bonjour" -> 'A'
A ->upper butfirst butlast -> 'A'
A ?          # → ONJOU
```

That's fine for small values, but rebuilding and re-storing a copy on every step gets expensive for anything large — a big list, for instance. Prefixing a variable with `&` pushes a direct reference instead, so a supporting function modifies it **in place**, with no copy involved:

```
mogwai.reset

"bonjour" -> 'A'
&A ->upper
A ?          # → BONJOUR — modified directly, no re-assignment needed
```

This is exactly the mechanism behind the record/list write shorthand from the previous section: `&$R<-y:` mutates `$R` directly, where plain `$R<-y:` would have left an unassigned copy on the stack. Not every function supports references — passing `&A` to one that doesn't raises a `bad argument type` error.

The performance difference is substantial — in practice, using `&` instead of the copy-and-reassign pattern can be well over a thousand times faster on non-trivial data. Worth reaching for whenever you're repeatedly transforming the same variable.

### `-->` — chaining several in-place transformations

Prefixing every single step with `&` gets verbose once you're chaining several transformations:

```
mogwai.reset

"bonjour" -> 'A'
&A ->upper  &A butfirst  &A butlast
A ?          # → ONJOU
```

The `-->` operator applies an entire list of transformations to a variable in one expression instead — each step runs in sequence, feeding off the current value of the variable:

```
mogwai.reset

"bonjour" -> 'A'
(->upper butfirst butlast) --> &A
A ?          # → ONJOU
```

Steps can also be full quotations (`{ ... }` blocks) rather than bare function names, when a step needs to do more than call one function:

```
mogwai.reset

"hello world" -> 'A'
(->upper { " !" + }) --> &A
A ?          # → HELLO WORLD !
```

`-->` is also **transactional**: if any step in the pipeline raises an error, the variable is rolled back to its value from before the pipeline started, and the error propagates normally.

```
mogwai.reset

"bonjour" -> 'A'
guard
{
    (->upper sqrt butlast) --> &A
}
else
{
    A ?     # → bonjour — untouched, because sqrt failed on a string
}
```

(`guard` / `else` get a proper introduction in the error handling section — for now, just note that the failed pipeline left `A` exactly as it started.)

### `!` — evaluating on the spot

Some variables hold more than a plain value — a code block, a function, or a string containing `{! ... }` interpolation blocks. By default, none of that content is resolved automatically; MOGWAI stores what you wrote, not its result, until you explicitly ask for it. The `!` prefix does that in one step, in place of writing `A eval`:

```
mogwai.reset

100 -> 'A'
{ A 200 * } -> 'B'
"We are in {! now ->date year: get }" -> 'C'

!B    # → 20000
!C    # → We are in 2026
```

`!A` works uniformly on blocks, functions, strings, lists, and records — and for a plain scalar (a number, a boolean...), it's simply a harmless no-op, identical to `A`. That's what made `!A` safe to use even before we'd introduced containers — it never does the wrong thing.

The same `!` shows up as a **prefix inside a list or record literal**, as a shortcut for calling `eval` on the whole thing right after building it:

```
mogwai.reset

100 -> 'A'

(A {! A 2 *} {! A 3 *}) eval ?     # → (100 200 300)
(! A {! A 2 *} {! A 3 *}) ?        # → (100 200 300) — same result, no separate eval
```

One thing worth knowing before you rely on this: containers are **lazy** by design — everything inside is stored as an expression, not pre-computed, so every `!A` evaluation reflects the program's state *at the moment you ask*, not at the moment the container was built. That's usually exactly what you want, and it's also why MOGWAI automatically detects and rejects circular references (a variable whose evaluation depends, directly or through a chain, on itself) instead of looping forever.

### `@` — a faster read, same result

`@A` reads a variable the same way plain `A` does — same value, same behavior — just resolved a little faster, since MOGWAI can settle where to look it up ahead of time rather than at the moment it runs. It's a micro-optimization more than a new capability: reach for it in hot loops or performance-sensitive code, and don't worry about it otherwise.

---

*Next: handling errors gracefully — catching them, inspecting them, and raising your own.*

---

## 12. Error Handling

By default, an error in MOGWAI stops the program — same as an unhandled exception in most languages. This section covers keeping that from happening when you'd rather recover.

### `trap` — protect a block, silently

`trap` runs a block; if anything inside raises an error, execution of that block stops right there and simply continues with whatever comes after `trap` — no error propagates, nothing is reported. The stack is automatically restored to its pre-`trap` state, so a failed block never leaves it in a half-modified condition:

```
mogwai.reset

trap
{
    "trap begins." ?
    10 a *                              # 'a' doesn't exist — this raises an error
    "This message will never be displayed." ?
}

"exit of the trap." ?
"the code continues..." ?
```

### `guard` / `else` — protect, and react

`guard` is `trap` with a recovery block attached — if the protected code fails, the `else` block runs instead:

```
mogwai.reset

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
```

### Finding out what went wrong

`error.last` returns the code of the most recent error, as a string — useful inside an `else` block to react differently depending on what actually failed. It doesn't reset itself, so once you're done handling it, clear it explicitly with `error.reset`:

```
mogwai.reset

guard
{
    10 a *
}
else
{
    "The error " ?? error.last ?? " happened!" ?
    error.reset
}
```

### Raising your own errors

`error.throw` raises an error deliberately, given its code as a string. MOGWAI's built-in errors all follow the `MW.n` scheme — a few of the ones you'll run into most:

| Code | Meaning |
|------|---------|
| `MW.9` | assert error |
| `MW.20` | too few arguments |
| `MW.21` | bad argument type |
| `MW.22` | bad argument value |
| `MW.30` | division by zero |
| `MW.40` | unknown name |

The full list — several dozen codes covering everything from tasks to files to OOP — is in the language reference; there's no need to memorize it, `error.last` is what you'll actually read at runtime.

`error.throw` isn't limited to the built-in `MW.n` codes, either — pass it any string and, if it isn't a recognized code, MOGWAI raises it as a **user error** carrying that string:

```
mogwai.reset

"INVALID_LICENSE_KEY" error.throw
# → user error (INVALID_LICENSE_KEY)
```

Handy for signaling your own application-specific error conditions through the same `guard` / `error.last` mechanism as built-in errors.

### Asserting a precondition: `mogwai.assert`

Rather than writing an `if` and raising an error by hand, `mogwai.assert` checks a condition and stops execution with `MW.9` if it's false, alongside a message of your choosing. The condition can be a boolean already on the stack, or a list — in which case `mogwai.assert` evaluates it for you and checks the result is a single boolean:

```
mogwai.reset

to 'divide' with [x: .number y: .number] do
{
    (y 0 !=) "divisor must not be zero" mogwai.assert
    x y /
}

10 2 divide ?      # → 5
10 0 divide ?       # → error MW.9: "divisor must not be zero"
```

This is the natural way to validate a function's preconditions up front, instead of letting a bad input fail somewhere deeper and less obvious.

### What happens when a program stops

MOGWAI recognizes two special function names — declaring them is entirely **optional**. If you don't define them, nothing special happens on exit; if you do, MOGWAI calls the matching one automatically depending on how the program ends:

- **`MOGWAI.onStop`** — runs on a clean exit, whether the script simply reaches its end or calls `mogwai.exit` explicitly.
- **`MOGWAI.onError`** — runs when the program stops **because of an unhandled error**, including one raised deliberately with `mogwai.halt` (which behaves exactly like `mogwai.exit`, except it raises `MW.2` instead of exiting quietly). Only `error.last` is available for context at that point.

Only the matching one of the two ever runs for a given stop, and only if you've actually declared it:

```
mogwai.reset

to 'MOGWAI.onStop' do { "The program has just ended." ? }
to 'MOGWAI.onError' do { "An error has occurred: " ?? error.last ? }

forever do
{
    rand 1000 * ->int -> 'r'
    r ? 250 wait

    if (r 50 <) then { exit }     # clean stop → MOGWAI.onStop runs
}
```

### Leaving early: `break` and `return`

`break` exits the innermost loop immediately — we already used it a few times in the control flow section. `return` is its counterpart for functions: it exits the current function on the spot, leaving whatever's already on the stack as the result:

```
mogwai.reset

to 'displayValue' with [value: .number] do
{
    if (value 5 !=) then
    {
        value ?
        return
    }

    "Value 5 is not allowed!" ?
}

1 10 for 'i' do { i displayValue }
```

---

*Next: object-oriented MOGWAI — classes, instances, and methods.*

---

## 13. Object-Oriented Programming

MOGWAI's object system is deliberately minimal — classes, instances, properties, methods, no inheritance, no garbage collector. You create instances and destroy them explicitly; nothing happens behind your back.

### Defining a class

`class 'Name' do { ... }` declares a class, with two sections inside: `private:` for members only accessible from within the class, and `public:` for everything callable from outside.

Inside either section, what distinguishes a **property** from a **method** is simply what follows its name: a type (`.number`, `.string`, ...) declares a property; a code block `{ }` declares a method.

```
mogwai.reset

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

A property is initialized to `empty` regardless of its declared type — you can check whether one has actually been set with `isEmpty`.

### `new` and `free` — the instance lifecycle

Two special method names are called automatically if you define them: `onInit:` when an instance is created, `onFree:` right before it's destroyed. Creation takes a named-parameter record — exactly the record syntax from the functions section — followed by the class name and `new`:

```
mogwai.reset

[id: 10 name: "SIBUE"] 'User' new -> '$U1'    # onInit: runs automatically

$U1 free                                       # onFree: runs automatically
```

Each instance gets a unique handle, displayed as `§` followed by a number (`§453`) — never reused for the lifetime of the engine. If several variables reference the same instance and it gets freed, all of them become invalid at once. Rather than risk using a stale reference, check first with `isAlive`:

```
mogwai.reset

if ($U1 isAlive) then
{
    $U1->display:
}
```

### Accessing properties and methods

Public members use the same `->` / `<-` compact notation you already know from records — reading, writing, and calling a method are all the same shape:

```
mogwai.reset

$U1->name: ?                 # read a property — equivalent to: $U1 name: get ?
"DUPONT" &$U1<-name:         # write a property in place — equivalent to: "DUPONT" &$U1 name: set
$U1->display:                # call a method — equivalent to: $U1 display: get
```

Trying to reach a `private:` member from outside the class raises an error — that's the whole point of the two sections. Every instance also has a read-only `className:` property, provided automatically, telling you which class it was built from.

### `self` — referring to the current instance

Inside any method, `self` is automatically available and refers to the instance the method was called on — use it to read or write the instance's own properties, or call its other methods:

```
show:
{
    "USER={! self}" eval ?
    self->show2:            # calling another method on the same instance
}
```

### Validating what a method receives

The same three levels of rigor from the functions section apply to methods too — `->vars` (no checking), `->safeVars` (checks count and type from the stack), `->params` (checks a named-parameter record, the natural fit for `onInit:`, since instances are always created with one):

```
onInit:
{
    [id: .number name: .string index: (.number 0)] ->params

    id self<-id:
    name self<-name:
    index self<-index:
}
```

### Putting it together

```
mogwai.reset

class 'User' do
{
    private:
    {
        x: .number
        y: .number

        onInit:
        {
            [id: .number name: .string] ->params
            id self<-id:
            name self<-name:
            rand 100 * ->int self<-x:
            rand 100 * ->int self<-y:
        }

        onFree:
        {
            "FREE {! self}" eval ?
        }
    }

    public:
    {
        id: .number
        name: .string

        display:
        {
            "USER={! self} — {! self->name:} at ({! self->x:},{! self->y:})" eval ?
        }
    }
}

[id: 10 name: "SIBUE"] 'User' new -> '$U1'
$U1->display:
$U1 free
```

### A couple of introspection tools

`alive` lists every currently living instance, across all classes — handy for cleanup or debugging:

```
mogwai.reset

alive ?                                                          # → (§1 §2 §3 ...)
alive foreach 'item' filter { item->className: 'User' == } ?     # only the Users
```

`frame` describes a class's whole structure — properties, private properties, methods, private methods — as a record:

```
mogwai.reset

'Counter' frame ?
# → [className: 'Counter' props: [value: .number] _props: [_step: .number] funcs: (onInit: increment: reset:) _funcs: ()]
```

---

*Next: tasks — running isolated pieces of code concurrently.*

---

## 14. Tasks

A **task** is a child unit of execution — its own isolated stack, running in parallel with the code that launched it (the **parent**). The parent can keep doing other things while a task runs; a task can itself launch further child tasks, with no limit besides available memory.

The one rule that shapes everything else here: **tasks never talk to each other directly**. A child task only knows its parent exists — not its siblings — and all communication, in both directions, happens through **events**.

### Events, briefly

An event is a name plus a block of code to run when it's triggered — declared with `onEvent`:

```
mogwai.reset

onEvent 'MY_EVENT' do
{
    "Hello, event data was: {! eventData}" eval ?
}
```

Whatever value the event was triggered with is available inside the block as the local variable `eventData`. Tasks use exactly this mechanism to report back to their parent — you'll declare an `onEvent` handler for each task lifecycle event you care about.

### The events a task fires to its parent

| Event | `eventData` contains |
|-------|----------------------|
| `TASK_DID_START` | the task's name |
| `TASK_DID_END` | a record with the task name and its result (`task:` / `result:`) |
| `TASK_DID_FAIL` | a record with the task name, the error code, and the message |
| `TASK_DID_PUBLISH` | a record with the task name and whatever the task chose to publish (`task:` / `message:`) |

### Declaring and starting a task

`task 'name' do { ... }` declares a task, the same way `to 'name' do { ... }` declares a function. Start it with `task.start` (no parameter) or `task 'name' start with object` (passing it a MOGWAI object, placed on the task's own stack right before it begins):

```
mogwai.reset

onEvent 'TASK_DID_PUBLISH' do { "Progress: {! eventData->message:}" eval ? }
onEvent 'TASK_DID_END' do { "Task {! eventData->task:} finished — result: {! eventData->result:}" eval ? }

task 'sumTask' do
{
    ->vars                      # unpacks the record passed at launch — here, one key: limit

    0 -> 'total'

    1 limit for 'i' do
    {
        total i + -> 'total'
        if (i 25 mod 0 ==) then { "reached {! i}" eval task.publish }
    }

    total task.setResult
}

task 'sumTask' start with [limit: 100]

'sumTask' task.wait

"done" ?
```

Inside the task, `task.publish` sends a progress update (arrives in the parent as `TASK_DID_PUBLISH`), and `task.setResult` records the value the parent will see once the task ends (`TASK_DID_END`). `task.wait` blocks the parent until that one task completes; if you're running several in parallel, `task.join` does the same for a whole list of task names at once — `('T1' 'T2' 'T3') task.join`.

### Handling errors inside a task

If a task raises an error that it doesn't catch itself, MOGWAI stops that task and fires `TASK_DID_FAIL` in the parent — the task doesn't crash the whole program. Still, the recommended habit is to wrap a task's body in `guard`, so you control what "failure" means and can report it cleanly through `task.setResult` rather than relying on the default failure event.

### A few practical limits

- A task that's already running can't be started again — `task.start` / `task start with` raise an error if you try. Check `task.isRunning` first if there's any doubt.
- A completed task can simply be started again, optionally with a new parameter object.
- MOGWAI's own guidance is to keep to roughly 50–100 simultaneous tasks — plenty for most real workloads, but not unlimited.

This tutorial's example is deliberately simple; the language reference walks through a fuller one — several tasks downloading files in parallel and reporting progress — that shows the same pieces at real-world scale.

---

*Next: a taste of what's next once you're comfortable with the basics — a few of MOGWAI's more powerful built-in primitive families.*

---

## 15. A Glimpse of What Else Is In There

Everything so far has been core language — enough to write real programs. MOGWAI also ships with a large standard library (300+ primitives), and this last stop is a quick tour of three families that tend to surprise people with how much they cover. Full details for all of these — and everything else — live in the function reference.

### `calc` — infix math, revisited

Back in the very first section, we mentioned `calc` as a bridge for anyone not yet fluent in RPN. It's worth another look now that you've seen more of the language: it accepts a full infix expression as a string, parentheses and operator precedence included, and evaluates it immediately using the classic Shunting-yard algorithm under the hood:

```
mogwai.reset

"5 * X + (7 + sin(Y))" calc ?
```

Genuinely useful any time a formula is easier to read in its familiar mathematical form than spelled out in RPN.

### `regex.*` — pattern matching

MOGWAI exposes the standard .NET regular expression engine, so any pattern you already know from C#, .NET, or most other regex-capable languages works unchanged. Five primitives cover the everyday cases:

```
mogwai.reset

"stephane@coding4phone.com" "^[\w.-]+@[\w.-]+\.\w+$" regex.isMatch ?
# → true

"2026-07-02" "(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})" "${day}/${month}/${year}" regex.replace ?
# → 02/07/2026
```

`regex.isMatch` for a yes/no test, `regex.match` / `regex.matches` for pulling data out (including named capture groups), `regex.replace` for search-and-replace, `regex.split` for splitting a string on a pattern. Every one of them accepts an optional timeout, so a runaway pattern raises an error instead of freezing your program.

### `http.*` — talking to the web

A full set of HTTP verbs — `http.get`, `http.head`, `http.post`, `http.put`, `http.patch`, `http.delete` — let a MOGWAI script call out to any web API. Parameters go in a record, and so does the response:

```
mogwai.reset

[
    uri: "https://api.github.com/orgs/dotnet/repos"
    requestHeaders: [User-Agent: "MyApp"]
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

`state:` tells you whether the call succeeded, `statusCode:` and `response:` carry the actual result, and `error:` explains what went wrong when it doesn't. The same record-in, record-out shape holds for every verb — once you know `http.get`, the rest read the same way.

---

## Where to Go From Here

That's the tour. You now have a working mental model for every core piece of MOGWAI — the stack, variables, types, control flow, lists and records, functions, sigils, error handling, OOP, and tasks — plus a sense of the scale of what's available beyond that.

From here:

- The **[Language Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_EN.md)** covers everything in this tutorial in more depth, plus topics we didn't touch — files, dates, binary data, timers, flags.
- The **[Function Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** documents all 300+ built-in primitives.
- The **[online playground](https://sydney680928.github.io/MOGWAI/)** is the fastest way to try anything from this tutorial without installing a thing.
- The **[GitHub repository](https://github.com/Sydney680928/mogwai)** has the source, examples, and a growing set of blog articles for deeper dives into specific features.

Welcome to MOGWAI — enjoy the stack.
