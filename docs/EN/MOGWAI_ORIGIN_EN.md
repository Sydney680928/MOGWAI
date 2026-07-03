# The Origin of MOGWAI

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

As the BLE simulator's needs grew, the **MOGWAI** engine was extended, improved, and many new features were added. Today **MOGWAI** handles HTTP requests natively and has 350+ primitives. Additional capabilities such as serial connections or SQLite databases are provided through extension libraries (called *usings* in MOGWAI terminology).

I'm now at version 8, still developed in C# for .NET. This allows it to be used on Windows, but also on Linux and macOS with X86, X64 and ARM architectures. For example, **MOGWAI** runs natively on a Raspberry PI 3 under Raspbian (Linux ARM).

## MOGWAI CLI to use the language in interactive mode

To "play" with **MOGWAI** I developed an interactive console application that allows you to use all the features of the language. This application is called **MOGWAI CLI**.

It is quite possible to write **MOGWAI** programs with a simple notepad, but it is still more pleasant to have appropriate development tools. [**MOGWAI Studio**](https://studio.mogwai.eu.com) is an IDE dedicated to **MOGWAI**.
