# MOGWAI Task Examples

This folder contains nine progressive examples illustrating the MOGWAI task system. Each example is self-contained and can be run directly from MOGWAI CLI or VS Code with the [MOGWAI Language Support](https://marketplace.visualstudio.com/items?itemName=mogwai.mogwai-language) extension.

Examples are ordered from simplest to most advanced. It is recommended to go through them in order.

---

## EX01 — First task: minimal lifecycle

**File:** `ex01_lifecycle.mog`

**Goal:** Understand the lifecycle of a child task and its associated events.

**What it does:**
- Declares a simple child task that performs a short operation
- Listens to and displays the `TASK_DID_START` and `TASK_DID_END` events
- Launches the task with `task start with` and waits for it to complete with `task.wait`

**Concepts covered:**
- Defining a task with `task 'NAME' do { ... }`
- Launching with `task start with`
- Retrieving the parameter in the child task from the stack
- `task.setResult` to return a value
- `TASK_DID_START` and `TASK_DID_END` events
- `task.wait` for synchronization

---

## EX02 — Parent → child communication via `task send`

**File:** `ex02_send.mog`

**Goal:** Show how the parent can send data to a running task.

**What it does:**
- Launches a child task that enters a message-waiting loop
- The parent sends several messages to the task via `task send`
- The task displays each received message via the `TASK_DID_RECEIVE` event
- The parent sends a termination message (`'STOP'`) to exit the loop
- The task terminates cleanly

**Concepts covered:**
- `task send` on the parent side
- `TASK_DID_RECEIVE` on the child side
- Message-waiting loop inside a task
- Controlled termination via message

---

## EX03 — Real-time progress reporting with `task.publish`

**File:** `ex03_publish.mog`

**Goal:** Illustrate the progress reporting pattern from a long-running task.

**What it does:**
- Launches a child task that simulates a multi-step process
- At each step, the task publishes its progress to the parent via `task.publish`
- The parent displays progress in real time via `TASK_DID_PUBLISH`
- At the end, the task returns a summary result

**Concepts covered:**
- `task.publish` on the child side
- `TASK_DID_PUBLISH` on the parent side
- Structured result with `task.setResult`
- Decoupling between processing and reporting

---

## EX04 — Error handling with `TASK_DID_FAIL`

**File:** `ex04_error.mog`

**Goal:** Show that unhandled errors in a child task do not crash the parent.

**What it does:**
- Launches two tasks simultaneously: one that succeeds, one that deliberately triggers an unhandled error (division by zero)
- The parent listens to `TASK_DID_FAIL` and displays the error information
- The program continues normally after the task failure

**Concepts covered:**
- Default behavior on unhandled error: `TASK_DID_FAIL`
- Contents of `eventData` in `TASK_DID_FAIL` (`task:`, `error:`, `message:` keys)
- Parent robustness against child errors
- Distinction between `TASK_DID_END` and `TASK_DID_FAIL`

---

## EX05 — Parallel tasks with `task.join`

**File:** `ex05_parallel.mog`

**Goal:** Show how to parallelize independent operations and synchronize at the end.

**What it does:**
- Launches several tasks in parallel, each performing a computation of fixed duration
- Each task returns its result via `task.setResult`
- The parent waits for all tasks with `task.join`
- After `task.join`, the parent collects and displays all results

**Concepts covered:**
- Launching multiple tasks
- True runtime parallelism
- `task.join` with `task.list`
- Retrieving results after `task.join`

---

## EX06 — Reusable worker pattern

**File:** `ex06_worker.mog`

**Goal:** Illustrate the reusable worker pattern: a task defined once, restarted multiple times with different parameters.

**What it does:**
- Defines a generic `WORKER` task that processes an item passed as parameter
- Runs the task sequentially on a list of items (restarted after each `task.wait`)
- Displays the result of each run

**Concepts covered:**
- Restarting a completed task with new parameters
- Worker pattern: single definition, multiple executions
- `task.wait` for sequential orchestration
- Task code reuse without redefinition

> This example deliberately uses sequential execution (one worker at a time) to isolate the restart concept. See EX05 for parallelization.

---

## EX07 — Controlled stop with `task.stop`

**File:** `ex07_stop.mog`

**Goal:** Show how the parent can stop a running task at any time.

**What it does:**
- Launches a long-running task that simulates an infinite process (loop with random delays)
- After a few seconds, the parent stops the task via `after` + `task.stop`
- `TASK_DID_END` is triggered with the last known result at the time of the stop

**Concepts covered:**
- `task.stop` for external termination
- `after` for deferred parent-side action
- `TASK_DID_END` behavior on forced stop
- `task.setResult` inside the loop to preserve intermediate state

---

## EX08 — Task tree: a child task creates its own sub-tasks

**File:** `ex08_tree.mog`

**Goal:** Illustrate task tree composition: a child task can itself create and manage child tasks.

**What it does:**
- The parent launches a `COORDINATOR` task
- `COORDINATOR` in turn creates two sub-tasks `SUB1` and `SUB2`
- Each sub-task performs a computation and returns a result
- `COORDINATOR` waits for its sub-tasks with `task.join`, aggregates their results, and returns the aggregated result to the parent
- The parent receives the final result from `COORDINATOR`

**Concepts covered:**
- Recursive nature of the task model (any runtime can be a parent)
- Encapsulating complexity in sub-trees
- Result aggregation across multiple levels
- Each runtime level has its own independent event handlers

---

## EX09 — Parallel downloads (complete reference example)

**File:** `ex09_download.mog`

**Goal:** A complete reference example bringing together all concepts, based on the official documentation example.

**What it does:**
- Downloads several HTML pages in parallel and saves them to disk
- Uses all lifecycle events (`TASK_DID_START`, `TASK_DID_PUBLISH`, `TASK_DID_END`, `TASK_DID_FAIL`)
- Measures the duration of each download and publishes it via `task.publish`
- Uses `guard` to handle file write errors
- Waits for all tasks to complete with `task.join`

**Concepts covered:**
- Synthesis of all previous concepts
- Path management (computed in parent, passed to child)
- Performance measurement with `now`
- Robustness with `guard`
- Complete production-ready pattern

> This example is an enriched and fully commented version of the example in the official documentation. It serves as a starting point for real-world parallel processing scripts.

---

## File structure

```
examples/tasks/
├── README.md
├── ex01_lifecycle.mog
├── ex02_send.mog
├── ex03_publish.mog
├── ex04_error.mog
├── ex05_parallel.mog
├── ex06_worker.mog
├── ex07_stop.mog
├── ex08_tree.mog
└── ex09_download.mog
```

---

*For full documentation on the MOGWAI task system, refer to the [language reference](../../docs/MOGWAI_EN.md).*
