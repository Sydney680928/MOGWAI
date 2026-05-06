// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MOGWAI.Engine;
using MOGWAI_CLI;
using System.Globalization;

CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

Console.Title = "MOGWAI CLI";
Console.Clear();

Console.WriteLine("█   █   ███    ████  █     █   ███   ███");
Console.WriteLine("██ ██  █   █  █      █  █  █  █   █   █");
Console.WriteLine("█ █ █  █   █  █  ██  █  █  █  █████   █");
Console.WriteLine("█   █  █   █  █   █  ██ █ ██  █   █   █");
Console.WriteLine("█   █   ███    ████   █   █   █   █  ███");
Console.WriteLine();
Console.WriteLine(MogwaiEngine.RuntimePrompt);
Console.WriteLine();
Console.WriteLine("Type 'edit' to open the code editor, 'studio' to start network communication, or 'bye' to exit.");
Console.WriteLine();

FileAssociationHelper.EnsureFileAssociation();

var engine = new MogwaiEngine("MOGWAI CLI", true, true);
var engineDelegate = new EngineDelegate(engine);
engine.Delegate = engineDelegate;

// L'éditeur est géré ici, sur le thread principal, comme BYE et STUDIO.
// Terminal.Gui DOIT tourner sur le thread principal

var editor = new MogwaiEditor(engine, engineDelegate);

Console.CancelKeyPress += (_, e) =>
{
    if (e.SpecialKey == ConsoleSpecialKey.ControlC)
    {
        engine.Halt();
        e.Cancel = true;
    }
};

if (args.Length > 0)
{
    try
    {
        var filename = Path.GetFileName(args[0]);
        Console.WriteLine($"Running {filename}...");

        await Task.Delay(2000);

        var code   = File.ReadAllText(args[0]);
        var result = await engine.RunAsync(code, false);

        Console.WriteLine();
        Console.WriteLine(result);
        Console.ReadLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    return;
}

while (true)
{
    Console.WriteLine();
    Console.Write("MOGWAI > ");

    var input = Console.ReadLine() ?? string.Empty;
    var cmd   = input.Trim().ToUpper();

    if (cmd == "BYE")
    {
        // Avertissement si l'éditeur contient du code non sauvegardé

        if (editor.HasUnsavedChanges)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠  The editor has unsaved changes.");
            Console.ResetColor();
            Console.Write("   Exit anyway? (y/N) ");

            var confirm = Console.ReadLine();

            if (!string.Equals(confirm?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
                continue;
        }

        break;
    }
    else if (cmd == "EDIT")
    {
        // Boucle éditeur/run : l'éditeur se rouvre automatiquement après
        // chaque exécution F5, jusqu'à ce que l'utilisateur quitte (Ctrl+Q).

        do
        {
            editor.Open();

            // F5 depuis l'éditeur → PendingRunCode non-null.
            // On exécute ici, dans la console propre (Terminal.Gui fermé).

            if (editor.PendingRunCode is string codeToRun)
            {
                try
                {
                    Console.WriteLine();
                    Console.WriteLine("── Run ─────────────────────────────");

                    var task = engine.RunAsync(codeToRun, true);

                    while (task.Status != TaskStatus.RanToCompletion)
                    {
                        if (Console.KeyAvailable)
                        {
                            var k = Console.ReadKey(true);

                            if (k.Key == ConsoleKey.F10)
                                engine.DebugFireNextStepSignal();
                            else if (k.Key == ConsoleKey.F5)
                                engine.DebugFireResumeSignal();
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine(task.Result);
                    Console.WriteLine("────────────────────────────────────");
                    Console.WriteLine("Returning to editor...");
                    await Task.Delay(1200); // laisse le temps de lire le résultat
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(1200);
                }
            }

        } while (editor.PendingRunCode != null);

        // PendingRunCode == null → l'utilisateur a quitté l'éditeur (Ctrl+Q / Exit)

        continue;
    }
    else if (cmd == "STUDIO")
    {
        await engine.StartNetworkCommunication();

        while (true)
            await Task.Delay(250);
    }

    try
    {
        var task = engine.RunAsync(input, true);

        while (task.Status != TaskStatus.RanToCompletion)
        {
            if (Console.KeyAvailable)
            {
                var k = Console.ReadKey(true);

                if (k.Key == ConsoleKey.F10)
                    engine.DebugFireNextStepSignal();
                else if (k.Key == ConsoleKey.F5)
                    engine.DebugFireResumeSignal();
            }
        }

        Console.WriteLine();
        Console.WriteLine(task.Result);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
