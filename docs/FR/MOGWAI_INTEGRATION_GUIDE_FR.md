# Guide d'intégration MOGWAI

Guide complet pour intégrer le runtime MOGWAI V8 dans vos applications .NET.

**Version :** 8.8  
**Auteur :** Stéphane Sibué  
**Licence :** Apache 2.0  
**Dernière mise à jour :** Mai 2026  

---

## Table des matières

1. [Installation](#installation)
2. [Démarrage rapide](#quick-start)
3. [Options du constructeur](#constructor-options)
4. [Interface IDelegate](#idelegate-interface)
5. [Fonctions personnalisées](#custom-functions)
6. [Skills](#skills)
7. [Manipulation de la pile](#stack-manipulation)
8. [Gestion des erreurs](#error-handling)
9. [Intégration MOGWAI STUDIO](#mogwai-studio-integration)
10. [Fonctionnalités avancées](#advanced-features)
11. [Bonnes pratiques](#best-practices)

---

## Installation

### Package NuGet

```bash
dotnet add package MOGWAI
```

### Espaces de noms requis

```csharp
using MOGWAI.Engine;       // MogwaiEngine class
using MOGWAI.Objects;      // MOGNumber, MOGString, MOGList, etc.
using MOGWAI.Interfaces;   // IDelegate interface
using MOGWAI.Exceptions;   // Exception types (optional)
using System.Net;          // IPAddress for SocketServerDidStart
```

---

## Démarrage rapide

### Application console minimale

Depuis la version 8.8, toutes les méthodes de `IDelegate` ont des implémentations par défaut. Une intégration minimale ne nécessite aucune implémentation — le moteur est prêt à exécuter des scripts console sans écrire une seule ligne de délégué :

```csharp
using MOGWAI.Engine;
using MOGWAI.Interfaces;

public class ConsoleApp : IDelegate
{
    private MogwaiEngine _engine;

    public ConsoleApp()
    {
        _engine = new MogwaiEngine("ConsoleApp");
        _engine.Delegate = this;
    }

    public async Task Run()
    {
        var result = await _engine.RunAsync(@"
            'Hello from MOGWAI!' ?
            2 3 + ?
        ", debugMode: false);

        if (result.IsError)
            Console.WriteLine($"Error: {result}");
    }
}
```

Les implémentations par défaut utilisent automatiquement `System.Console` pour toutes les entrées/sorties console lorsque `engine.IsHostConsole` est `true` (détecté au démarrage du moteur). Pour les hôtes non-console (WinForms, MAUI), toutes les méthodes console sont des no-ops par défaut — pas d'exception, pas de crash.

Ne surchargez que les méthodes qui nécessitent un comportement personnalisé pour votre application. Consultez la section [Interface IDelegate](#idelegate-interface) pour la liste complète des valeurs par défaut.

---

## Options du constructeur

### Trois signatures de constructeur

```csharp
// 1. Simple - Default settings
public MogwaiEngine(string name)
  → keepAlive: false, useDefaultFolders: true

// 2. Control folders
public MogwaiEngine(string name, bool useDefaultFolders)
  → keepAlive: false

// 3. Full control
public MogwaiEngine(string name, bool keepAlive, bool useDefaultFolders)
```

### Détail des paramètres

#### `name` (string)

- **Rôle :** Identifie le moteur, affiché dans MOGWAI STUDIO
- **Exemple :** « MyApp », « MOGWAI CLI », « WinForms Debug »
- **Obligatoire :** Oui

#### `keepAlive` (bool)

- **Rôle :** Contrôle la persistance de l'état entre les appels `RunAsync()`
- **Défaut :** `false`
- **Si `false` :** Le moteur se réinitialise complètement après chaque exécution (variables, fonctions et pile effacées)
- **Si `true` :** L'état persiste (utile pour un REPL ou une session interactive)

**Exemple :**

```csharp
var engine = new MogwaiEngine("CLI", keepAlive: true, useDefaultFolders: true);

await engine.RunAsync("42 -> 'x'", debugMode: false);
await engine.RunAsync("x 2 * ?", debugMode: false);  // Prints: 84
// Variable 'x' still exists because keepAlive = true
```

#### `useDefaultFolders` (bool)

- **Rôle :** Crée une structure de dossiers standard dans le dossier Documents de l'utilisateur
- **Défaut :** `true`
- **Si `false` :** Aucun dossier créé, l'application gère ses propres chemins
- **Si `true` :** Crée `Documents/MOGWAI/Programs/`, `Files/`, `Usings/`

**Structure des dossiers :**

```
Documents/
└── MOGWAI/
    ├── Programs/      ← User scripts (.mog files)
    ├── Files/         ← Data files
    └── Usings/        ← Shared modules/libraries
```

**Accès aux chemins :**

```csharp
string programsDir = engine.ProgramsDirectory;
string filesDir = engine.FilesDirectory;
string usingsDir = engine.UsingsDirectory;

// Or set custom paths
engine.ProgramsDirectory = @"C:\MyApp\Scripts";
```

### Scénarios d'utilisation

#### Scénario 1 : Démarrage rapide / Tutoriel

```csharp
var engine = new MogwaiEngine("MyApp");
```

✅ Idéal pour débuter  
✅ Les scripts peuvent être placés dans `Documents/MOGWAI/Programs/`  
✅ État propre à chaque exécution  

---

#### Scénario 2 : Application embarquée (WinForms, MAUI)

```csharp
var engine = new MogwaiEngine("WinFormsApp", useDefaultFolders: false);
```

✅ Pas de création de dossiers dans Documents  
✅ Utilisation de ressources embarquées ou de chemins personnalisés  
✅ État propre à chaque exécution  

**Exemple — Ressources embarquées :**

```csharp
var script = GetEmbeddedResource("Scripts.Sample1.mog");
await engine.RunAsync(script, debugMode: false);
```

---

#### Scénario 3 : Application CLI / REPL

```csharp
var engine = new MogwaiEngine("MOGWAI CLI", keepAlive: true, useDefaultFolders: true);

while (true)
{
    Console.Write("> ");
    string? line = Console.ReadLine();
    if (line == "exit") break;

    await engine.RunAsync(line, debugMode: false);
}
```

✅ Variables persistantes entre les commandes  
✅ Utilisation des dossiers standard  
✅ Session interactive  

---

#### Scénario 4 : Configuration personnalisée

```csharp
var engine = new MogwaiEngine("MyApp", keepAlive: true, useDefaultFolders: false);

// Set custom directories
engine.ProgramsDirectory = @"C:\MyApp\Scripts";
engine.FilesDirectory = @"C:\MyApp\Data";
engine.UsingsDirectory = @"C:\MyApp\Modules";
```

✅ État persistant  
✅ Chemins personnalisés  
✅ Contrôle total  

---

## Interface IDelegate

L'interface `IDelegate` est le pont entre MOGWAI et votre application.

### Interface complète

Depuis la version 8.8, toutes les méthodes ont des implémentations par défaut. Ne surchargez que ce que votre application gère réellement.

```csharp
namespace MOGWAI.Interfaces;

public interface IDelegate
{
    // Lifecycle — no-op par défaut
    Task ProgramStart(MogwaiEngine engine, string code);
    Task ProgramEnd(MogwaiEngine engine, EvalResult result);
    Task<EvalResult> EngineDidPause(MogwaiEngine engine);
    Task<EvalResult> EngineDidResume(MogwaiEngine engine);

    // Console I/O — System.Console par défaut (si engine.IsHostConsole est true)
    Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message);
    Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message);
    Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine);
    Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message);
    Task<EvalResult> ConsoleShow(MogwaiEngine engine);
    Task<EvalResult> ConsoleHide(MogwaiEngine engine);
    Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y);
    Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine);
    Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color);
    Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color);
    Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine);
    Task<int> ConsoleWidth(MogwaiEngine engine);
    Task<int> ConsoleHeight(MogwaiEngine engine);

    // Fonctions personnalisées — liste vide / signal de délégation par défaut
    string[] HostFunctions(MogwaiEngine engine);
    Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word);

    // Skills — liste vide par défaut
    string[] Skills(MogwaiEngine engine);

    // Messages runtime — no-op par défaut
    Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter);

    // Sortie debug — no-op par défaut
    Task<EvalResult> DebugMessage(MogwaiEngine engine, string message);
    Task<EvalResult> DebugClear(MogwaiEngine engine);

    // Connexion MOGWAI STUDIO — no-op par défaut
    Task<EvalResult> StudioDidConnect(MogwaiEngine engine);
    Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine);
    Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port);
    Task<EvalResult> SocketServerDidStop(MogwaiEngine engine);
}
```

### Implémentations par défaut et `engine.IsHostConsole`

Le moteur détecte au démarrage s'il s'exécute dans un vrai contexte console (`engine.IsHostConsole`). Toutes les implémentations par défaut des méthodes console utilisent ce flag : elles appellent `System.Console` si `true`, et ne font rien si `false`.

Un hôte WinForms ou MAUI qui ne surcharge aucune méthode console ne plantera pas — il obtient simplement des no-ops. Un hôte console pure obtient un comportement `System.Console` complet gratuitement.

| Méthode | Comportement par défaut |
|--------|------------------------|
| `ConsolePrintLn` | `Console.WriteLine` si `IsHostConsole`, sinon no-op |
| `ConsolePrint` | `Console.Write` si `IsHostConsole`, sinon no-op |
| `ConsoleClearScreen` | `Console.Clear` si `IsHostConsole`, sinon no-op |
| `ConsoleLocate` | `Console.SetCursorPosition` si `IsHostConsole`, sinon no-op |
| `ConsoleGetCursorPosition` | `Console.CursorLeft/Top` si `IsHostConsole`, sinon `(0,0)` |
| `ConsoleGetInputKey` | `Console.KeyAvailable` + `ReadKey` si `IsHostConsole`, sinon `-1` |
| `Prompt` | `Console.Write` + `ReadLine` si `IsHostConsole`, sinon `null` |
| `ConsoleWidth` | `Console.WindowWidth` si `IsHostConsole`, sinon `0` |
| `ConsoleHeight` | `Console.WindowHeight` si `IsHostConsole`, sinon `0` |
| `ConsoleShow` / `ConsoleHide` | no-op |
| `ConsoleSetForegroundColor` / `ConsoleSetBackgroundColor` | no-op |
| `ProgramStart` / `ProgramEnd` | no-op |
| `EngineDidPause` / `EngineDidResume` | no-op |
| `MessageReceivedFromRuntime` | no-op |
| `DebugMessage` / `DebugClear` | no-op |
| `StudioDidConnect` / `StudioDidDisconnect` | no-op |
| `SocketServerDidStart` / `SocketServerDidStop` | no-op |
| `HostFunctions` | `[]` (tableau vide) |
| `ExecuteHostFunction` | `EvalResult.NoExternalFunction` (signal de délégation) |
| `Skills` | `[]` (tableau vide) |

### Méthodes principales

#### Hooks de cycle de vie

```csharp
public async Task ProgramStart(MogwaiEngine engine, string code)
{
    // Appelé avant le début de l'exécution du script
    Console.WriteLine("Script starting...");
    await Task.CompletedTask;
}

public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
{
    // Appelé après la fin de l'exécution du script
    if (result.IsError)
        Console.WriteLine($"Script failed: {result.Error.Message}");
    else
        Console.WriteLine($"Script completed in {result.Duration.TotalMilliseconds}ms");

    await Task.CompletedTask;
}
```

#### Entrées/Sorties console

Ne surchargez que les méthodes pertinentes pour votre hôte. Pour une application WinForms, `ConsolePrintLn` et `ConsolePrint` redirigent typiquement vers un TextBox :

```csharp
public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
{
    Invoke(() => OutputTextBox.AppendText(message + "\r\n"));
    return EvalResult.NoError;
}

public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
{
    Invoke(() => OutputTextBox.AppendText(message));
    return EvalResult.NoError;
}
```

Pour les hôtes non-console, `ConsoleGetInputKey` doit capturer les événements clavier via une queue :

```csharp
private readonly ConcurrentQueue<int> _keyQueue = new();

// Branché sur l'événement KeyDown du formulaire
protected override void OnKeyDown(KeyEventArgs e)
{
    _keyQueue.Enqueue((int)e.KeyCode);
    base.OnKeyDown(e);
}

public Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
{
    int key = _keyQueue.TryDequeue(out int k) ? k : -1;
    return Task.FromResult((EvalResult.NoError, key));
}
```

**Dans MOGWAI — polling avec `yield` pour le scheduling coopératif :**

```
# Attendre l'appui d'une touche sans affamer le scheduler
while (console.getInputKey -1 ==) do
{
    yield { }
}
```

---

## Fonctions personnalisées

### Déclarer des fonctions personnalisées

```csharp
public string[] HostFunctions(MogwaiEngine engine)
{
    // Return list of custom function names
    return new[] { "double", "greet", "turtle.move", "turtle.turn" };
}
```

### Exécuter des fonctions personnalisées

```csharp
public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
{
    switch (word)
    {
        case "double":
            return ExecuteDouble(engine);

        case "greet":
            return ExecuteGreet(engine);

        case "turtle.move":
            return await ExecuteTurtleMove(engine);

        case "turtle.turn":
            return await ExecuteTurtleTurn(engine);

        default:
            return EvalResult.NoExternalFunction;
    }
}
```

### Le signal `NoExternalFunction`

`EvalResult.NoExternalFunction` est un signal de délégation, pas une erreur fonctionnelle. Quand MOGWAI rencontre un mot inconnu, il interroge l'hôte via `ExecuteHostFunction`. L'hôte peut répondre de trois façons :

- **`EvalResult.NoExternalFunction`** — l'hôte ne connaît pas ce mot. MOGWAI continue sa propre chaîne de résolution. Si rien ne résout le mot, MW.50 (`unknown word`) est levée.
- **`EvalResult.NoError`** — l'hôte a reconnu et exécuté la fonction avec succès.
- **Une erreur** — l'hôte a reconnu la fonction mais son exécution a échoué.

L'implémentation par défaut retourne `NoExternalFunction`, ce qui est correct pour un hôte sans fonctions personnalisées — les mots inconnus passent simplement à la résolution interne de MOGWAI.
```

### Patron d'implémentation d'une fonction

#### 1. Valider la signature de pile

```csharp
private EvalResult ExecuteDouble(MogwaiEngine engine)
{
    // Check stack has at least 1 item
    var signature = engine.StackSign(1);
    if (signature.Count == 0)
        return EvalResult.Failure(engine, Error.TooFewArgumentsError, "double");

    // Check type is number
    if (signature[0] != typeof(MOGNumber))
        return EvalResult.Failure(engine, Error.BadArgumentTypeError, "double");

    // Pop, process, push
    var num = engine.StackPopNumber();
    engine.StackPush(new MOGNumber(num.Value * 2));

    return EvalResult.NoError;
}
```

#### 2. Fonctions à plusieurs paramètres

```csharp
private async Task<EvalResult> ExecuteTurtleMove(MogwaiEngine engine)
{
    // Signature: distance (number)
    var signature = engine.StackSign(1);
    if (signature.Count == 0)
        return EvalResult.Failure(engine, Error.TooFewArgumentsError, "turtle.move");

    if (signature[0] != typeof(MOGNumber))
        return EvalResult.Failure(engine, Error.BadArgumentTypeError, "turtle.move");

    // Pop parameter
    MOGNumber distance = engine.StackPopNumber();

    // Execute (with thread safety for UI)
    await Task.Run(() =>
    {
        Invoke(() =>
        {
            // Move turtle on UI thread
            MoveTurtle(distance.Value);
        });
    });

    return EvalResult.NoError;
}
```

#### 3. Fonctions avec valeur de retour

```csharp
private EvalResult ExecuteGreet(MogwaiEngine engine)
{
    // Push result to stack
    engine.StackPush(new MOGString("Hello from custom function!"));
    return EvalResult.NoError;
}
```

**Dans MOGWAI :**

```mogwai
greet ?  # Prints: Hello from custom function!
```

---

## Skills

Les skills sont des noms déclarés par l'hôte qui identifient des capacités disponibles dans le contexte d'exécution courant. Ils permettent aux scripts MOGWAI de vérifier au démarrage qu'ils s'exécutent dans le bon environnement.

### Déclarer des skills

Surchargez la méthode `Skills()` dans votre délégué :

```csharp
public string[] Skills(MogwaiEngine engine)
{
    return ["APP_GIZMO", "TUI", "BLE"];
}
```

L'implémentation par défaut retourne un tableau vide — aucun skill déclaré. Le moteur fusionne les skills de l'hôte avec les skills éventuels du moteur et déduplique le résultat.

### Utilisation des skills dans les scripts

**Vérifier la disponibilité d'un skill :**

```
if ('APP_GIZMO' hasSkill) then
{
    # code spécifique à GIZMO
}
```

**Asserter les skills requis en début de script :**

```
'APP_GIZMO' "Ce script nécessite GIZMO pour s'exécuter." mogwai.assertSkill
'BLE' "Ce script nécessite le support BLE." mogwai.assertSkill

# suite du script...
```

Si un skill requis est absent, `mogwai.assertSkill` lève MW.9 (`assert error`) et arrête l'exécution. Si `MOGWAI.onError` est défini, il est appelé automatiquement.

**Lister tous les skills disponibles :**

```
skills ?   # → ('APP_GIZMO' 'TUI' 'BLE')
```

Les skills sont également accessibles via `mogwai.info` :

```
mogwai.info -> '$info'
$info skills: get ?   # → ('APP_GIZMO' 'TUI' 'BLE')
```

---

## Manipulation de la pile

### Signature de pile

```csharp
// Get types of top N items
var signature = engine.StackSign(3);

// signature is List<Type>
// Example: [typeof(MOGNumber), typeof(MOGString), typeof(MOGBoolean)]
```

### Opérations de dépilage (Pop)

```csharp
// Pop specific types
MOGNumber number = engine.StackPopNumber();
MOGString text = engine.StackPopString();
MOGBoolean bool = engine.StackPopBoolean();
MOGList list = engine.StackPopList();
MOGRecord record = engine.StackPopRecord();
MOGCode code = engine.StackPopCode();
MOGData data = engine.StackPopData();

// Generic pop
MOGObject obj = engine.StackPop();
```

### Opérations d'empilement (Push)

```csharp
// Push values to stack
engine.StackPush(new MOGNumber(42));
engine.StackPush(new MOGString("Hello"));
engine.StackPush(new MOGBoolean(true));
engine.StackPush(new MOGList(new[] { 
    new MOGNumber(1), 
    new MOGNumber(2) 
}));

// Create record
var record = new MOGRecord(engine);
record.Items["name"] = new MOGString("MOGWAI");
record.Items["version"] = new MOGNumber(8.0);
engine.StackPush(record);
```

### Propriétés de la pile

```csharp
// Get stack size
int size = engine.StackSize;

// Check if stack is empty
if (size == 0)
{
    // Handle empty stack
}
```

---

## Gestion des erreurs

### EvalResult

```csharp
var result = await engine.RunAsync(script, debugMode: false);

if (result.IsError)
{
    Console.WriteLine($"Error Code: {result.Error.Code}");
    Console.WriteLine($"Message: {result.Error.Message}");
    Console.WriteLine($"Position: {result.StartErrorPosition}-{result.EndErrorPosition}");
}
else
{
    Console.WriteLine($"Success! Duration: {result.Duration.TotalMilliseconds}ms");
}
```

### Erreurs standard

```csharp
// Common errors
Error.TooFewArgumentsError       // Stack doesn't have enough items
Error.BadArgumentTypeError       // Wrong type on stack
Error.DivideByZeroError         // Division by zero
Error.VariableNotFoundError     // Variable doesn't exist
Error.FunctionNotFoundError     // Function doesn't exist

// Return error from custom function
return EvalResult.Failure(engine, Error.BadArgumentTypeError, "myFunction");
```

### Erreurs personnalisées

```csharp
// Register custom error
var myError = engine.RegisterError(
    this,                           // IDelegate
    "INVALID_OPERATION",           // Error code
    "The requested operation is not valid in this context"
);

// Use custom error
return EvalResult.Failure(engine, myError, "myFunction");
```

---

## Intégration MOGWAI STUDIO

### Activer la connexion STUDIO

```csharp
var engine = new MogwaiEngine("MyApp");
engine.Delegate = this;

// Start network server
await engine.StartNetworkCommunication();

// Keep running
while (true)
{
    await Task.Delay(250);
}
```

### Configuration réseau

```csharp
// Default configuration (all interfaces, port 1968)
await engine.StartNetworkCommunication();

// Custom configuration
await engine.StartNetworkCommunication(
    address: "127.0.0.1",  // Localhost only
    port: 1968              // UDP discovery port
);
```

### Protocole de découverte

**STUDIO diffuse** (port UDP 1968) :

```json
{"Source": "MOGWAI STUDIO", "Function": "WHO IS HERE"}
```

**Le runtime répond :**

```json
{
  "Source": "MOGWAI RUNTIME",
  "Function": "I AM HERE",
  "Parameters": [
    "MyApp",          // Engine name
    "63542",          // TCP port (auto-assigned 63000-65000)
    "8.0.0",          // MOGWAI version
    "Windows",        // Platform
    "x64",            // Architecture
    ".NET 9.0",       // Framework
    "..."             // Other info
  ]
}
```

**La connexion TCP** est établie sur le port indiqué dans la réponse.

### Callbacks MOGWAI STUDIO

Lorsque STUDIO se connecte à votre runtime, ces callbacks sont invoqués :

```csharp
public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
{
    Console.WriteLine("MOGWAI STUDIO connected");
    StatusLabel.Text = "Connected to STUDIO";
    return EvalResult.NoError;
}

public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
{
    Console.WriteLine("MOGWAI STUDIO disconnected");
    StatusLabel.Text = "STUDIO disconnected";
    return EvalResult.NoError;
}

public async Task<EvalResult> SocketServerDidStart(
    MogwaiEngine engine, IPAddress address, int port)
{
    Console.WriteLine($"Socket server started on {address}:{port}");
    return EvalResult.NoError;
}

public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
{
    Console.WriteLine("Socket server stopped");
    return EvalResult.NoError;
}
```

### Messages du runtime et sortie de débogage

```csharp
public async Task<EvalResult> MessageReceivedFromRuntime(
    MogwaiEngine engine, string message, MOGObject parameter)
{
    // MOGWAI can send messages to the host application
    Console.WriteLine($"Runtime message: {message}");
    // parameter contains additional data
    return EvalResult.NoError;
}

public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
{
    // Debug output from MOGWAI scripts (console.debug)
    Console.WriteLine($"[DEBUG] {message}");
    return EvalResult.NoError;
}

public async Task<EvalResult> DebugClear(MogwaiEngine engine)
{
    // Clear debug output
    DebugOutputTextBox?.Clear();
    return EvalResult.NoError;
}
```

**Dans MOGWAI :**

```mogwai
# Send debug message
"Debug information here" console.debug

# Send message to host
"myMessage" "data" runtime.sendMessage
```

### Fonctionnalités de STUDIO

Une fois connecté, STUDIO permet de :

- ✅ Poser des points d'arrêt par numéro de ligne
- ✅ Avancer pas à pas (step over / step into / step out)
- ✅ Visualiser l'état de la pile
- ✅ Inspecter les variables
- ✅ Évaluer des expressions
- ✅ Continuer / mettre en pause l'exécution

### Considérations de sécurité

⚠️ **Important :** La connexion STUDIO permet un contrôle complet des scripts.

**Bonnes pratiques :**

- Activer uniquement sur des réseaux de confiance (localhost, LAN privé)
- Désactiver dans les builds de production
- Ajouter des règles de pare-feu si exposition réseau

```csharp
#if DEBUG
    await engine.StartNetworkCommunication(address: "127.0.0.1");
#endif
```

### Configuration du pare-feu

Autoriser les connexions entrantes sur :

- **Port UDP 1968** (découverte)
- **Ports TCP 63000–65000** (session de débogage)

---

## Fonctionnalités avancées

### Interrompre l'exécution

```csharp
// Emergency stop (Ctrl+C handler)
Console.CancelKeyPress += (sender, e) =>
{
    engine.Halt();
    e.Cancel = true;
};
```

### Bannière du runtime

```csharp
// Get MOGWAI version banner
string banner = MogwaiEngine.RuntimePrompt;
Console.WriteLine(banner);

// Output:
// MOGWAI version 8.0.0
// (c) Stéphane SIBUE 2015-2026
```

### Parser sans exécuter

```csharp
// Parse code to check syntax
var objects = engine.Parse("2 3 + ?");

// objects is List<MOGObject>
foreach (var obj in objects)
{
    Console.WriteLine(obj.GetType().Name);
}
```

### Exécution asynchrone

```csharp
// Start script in background
var task = engine.RunAsync(script, debugMode: false);

// Do other work
await DoSomethingElse();

// Wait for completion
var result = await task;
```

### Plusieurs scripts concurrents

```csharp
// Run multiple scripts concurrently
var task1 = engine.RunAsync(script1, debugMode: false);
var task2 = engine.RunAsync(script2, debugMode: false);
var task3 = engine.RunAsync(script3, debugMode: false);

// Wait for all
await Task.WhenAll(task1, task2, task3);
```

---

## Bonnes pratiques

### Sécurité des threads

**Mises à jour de l'interface :** Invoquer toujours sur le thread UI lors de mises à jour depuis MOGWAI :

```csharp
private async Task<EvalResult> ExecuteTurtleMove(MogwaiEngine engine)
{
    var distance = engine.StackPopNumber();

    // WinForms
    Invoke(() =>
    {
        MoveTurtle(distance.Value);
    });

    // WPF
    Dispatcher.Invoke(() =>
    {
        MoveTurtle(distance.Value);
    });

    // MAUI
    MainThread.BeginInvokeOnMainThread(() =>
    {
        MoveTurtle(distance.Value);
    });

    return EvalResult.NoError;
}
```

### Gestion des erreurs

**Toujours vérifier EvalResult :**

```csharp
var result = await engine.RunAsync(script, debugMode: false);

if (result.IsError)
{
    // Log error
    Logger.Error($"MOGWAI Error: {result.Error.Code}");

    // Show to user
    MessageBox.Show($"Script error: {result.Error.Message}");

    // Don't continue
    return;
}

// Continue with success path
```

### Gestion des ressources

**Libérer correctement :**

```csharp
public class MyApp : IDisposable
{
    private MogwaiEngine _engine;

    public MyApp()
    {
        _engine = new MogwaiEngine("MyApp");
        _engine.Delegate = this;
    }

    public void Dispose()
    {
        // Clean up MOGWAI resources
        _engine?.Halt();
        // Additional cleanup
    }
}
```

### Chargement des scripts

**Ressources embarquées :**

```csharp
public string GetEmbeddedScript(string name)
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = $"MyApp.Scripts.{name}";

    using var stream = assembly.GetManifestResourceStream(resourceName);
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}

// Usage
var script = GetEmbeddedScript("Sample1.mog");
await engine.RunAsync(script, debugMode: false);
```

---

## Dépannage

### Problèmes courants

#### Erreur « Too Few Arguments »

**Problème :** La pile ne contient pas suffisamment d'éléments pour la fonction.

**Solution :** Vérifier la signature de pile avant de dépiler :

```csharp
var signature = engine.StackSign(2); // Need 2 items
if (signature.Count < 2)
    return EvalResult.Failure(engine, Error.TooFewArgumentsError, "myFunction");
```

#### Exceptions entre threads

**Problème :** Mise à jour de l'interface depuis le thread MOGWAI.

**Solution :** Utiliser Invoke/Dispatcher :

```csharp
Invoke(() => UpdateUI());  // WinForms
Dispatcher.Invoke(() => UpdateUI());  // WPF
MainThread.BeginInvokeOnMainThread(() => UpdateUI());  // MAUI
```

#### Variables non persistantes

**Problème :** Variables perdues entre les exécutions.

**Solution :** Utiliser `keepAlive: true` :

```csharp
var engine = new MogwaiEngine("MyApp", keepAlive: true, useDefaultFolders: false);
```

---

## Exemple complet : Application WinForms

```csharp
using MOGWAI.Engine;
using MOGWAI.Interfaces;
using MOGWAI.Objects;
using System.Net;

public partial class FormMain : Form, IDelegate
{
    private MogwaiEngine _engine;

    public FormMain()
    {
        InitializeComponent();

        // Create engine (no default folders for embedded app)
        _engine = new MogwaiEngine("WinForms App", useDefaultFolders: false);
        _engine.Delegate = this;
    }

    private async void RunButton_Click(object sender, EventArgs e)
    {
        // Execute code from TextBox
        var result = await _engine.RunAsync(CodeTextBox.Text, debugMode: false);

        if (result.IsError)
        {
            MessageBox.Show(
                $"Error: {result.Error.Message}\nPosition: {result.StartErrorPosition}", 
                "MOGWAI Error", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error
            );
        }
    }

    private void EnableStudioButton_Click(object sender, EventArgs e)
    {
        // Start STUDIO server in background
        _ = Task.Run(async () => 
        {
            await _engine.StartNetworkCommunication(address: "127.0.0.1");
        });

        StatusLabel.Text = "Waiting for STUDIO connection...";
    }

    // IDelegate implementation
    public async Task ProgramStart(MogwaiEngine engine, string code)
    {
        Invoke(() => StatusLabel.Text = "Running...");
        await Task.CompletedTask;
    }

    public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
    {
        Invoke(() => StatusLabel.Text = result.IsError ? "Error" : "Completed");
        await Task.CompletedTask;
    }

    public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
    {
        Invoke(() => OutputTextBox.AppendText(message + "\r\n"));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
    {
        Invoke(() => OutputTextBox.AppendText(message));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
    {
        Invoke(() => OutputTextBox.Clear());
        return EvalResult.NoError;
    }

    public async Task<(EvalResult result, string? value)> Prompt(
        MogwaiEngine engine, string message)
    {
        string? result = null;

        Invoke(() =>
        {
            using var inputDialog = new InputDialog(message);
            if (inputDialog.ShowDialog() == DialogResult.OK)
                result = inputDialog.InputValue;
        });

        return (EvalResult.NoError, result);
    }

    // Advanced console (not applicable for WinForms, return NoError)
    public async Task<EvalResult> ConsoleShow(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleHide(MogwaiEngine engine) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y) => EvalResult.NoError;
    public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        => (EvalResult.NoError, 0, 0);
    public async Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color) => EvalResult.NoError;
    public async Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color) => EvalResult.NoError;
    public async Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        => (EvalResult.NoError, 0);

    public string[] HostFunctions(MogwaiEngine engine)
    {
        return new[] { "turtle.move", "turtle.turn", "turtle.color" };
    }

    public string[] Skills(MogwaiEngine engine)
    {
        return ["APP_WINFORMS"];
    }

    public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
    {
        switch (word)
        {
            case "turtle.move":
                return ExecuteTurtleMove(engine);
            case "turtle.turn":
                return ExecuteTurtleTurn(engine);
            case "turtle.color":
                return ExecuteTurtleColor(engine);
        }

        return EvalResult.NoExternalFunction;
    }

    public async Task<EvalResult> MessageReceivedFromRuntime(
        MogwaiEngine engine, string message, MOGObject parameter)
    {
        Invoke(() => OutputTextBox.AppendText($"[MSG] {message}\r\n"));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
    {
        Invoke(() => DebugTextBox?.AppendText($"{message}\r\n"));
        return EvalResult.NoError;
    }

    public async Task<EvalResult> DebugClear(MogwaiEngine engine)
    {
        Invoke(() => DebugTextBox?.Clear());
        return EvalResult.NoError;
    }

    public async Task<EvalResult> EngineDidPause(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "Paused");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> EngineDidResume(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "Running");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "STUDIO Connected");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "STUDIO Disconnected");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> SocketServerDidStart(
        MogwaiEngine engine, IPAddress address, int port)
    {
        Invoke(() => StatusLabel.Text = $"Server: {address}:{port}");
        return EvalResult.NoError;
    }

    public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
    {
        Invoke(() => StatusLabel.Text = "Server stopped");
        return EvalResult.NoError;
    }

    private EvalResult ExecuteTurtleMove(MogwaiEngine engine)
    {
        var sig = engine.StackSign(1);
        if (sig.Count == 0 || sig[0] != typeof(MOGNumber))
            return EvalResult.Failure(engine, Error.BadArgumentTypeError, "turtle.move");

        var distance = engine.StackPopNumber();

        Invoke(() =>
        {
            // Move turtle on UI
            MoveTurtle((int)distance.Value);
            TurtleCanvas.Refresh();
        });

        return EvalResult.NoError;
    }

    // ... Implement ExecuteTurtleTurn, ExecuteTurtleColor similarly
}
```

---

## Récapitulatif

### Points clés

1. ✅ Utiliser la classe `MogwaiEngine` depuis l'espace de noms `MOGWAI.Engine`
2. ✅ Implémenter l'interface `IDelegate` pour l'intégration
3. ✅ Choisir le constructeur adapté au cas d'usage (embarqué vs CLI)
4. ✅ Toujours vérifier `EvalResult.IsError`
5. ✅ Assurer la sécurité des threads pour les mises à jour UI
6. ✅ Activer STUDIO pour le débogage avec `StartNetworkCommunication()`

### Prochaines étapes

- Lire le [Guide du langage MOGWAI](../docs/FR/MOGWAI_FR.md) pour la syntaxe du langage
- Lire la [Référence des fonctions](../docs/FR/MOGWAI_FUNCTIONS_FR.md) pour les fonctions intégrées
- Explorer les [Exemples](../examples/) pour des intégrations réelles

---

**Bonne intégration !** 🚀

*Pour toute question ou problème, rendez-vous sur : [https://github.com/Sydney680928/mogwai/issues](https://github.com/Sydney680928/mogwai/issues)*

