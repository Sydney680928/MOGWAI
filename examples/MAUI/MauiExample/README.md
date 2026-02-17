# MOGWAI RUNTIME - .NET MAUI Example

A simple cross-platform MOGWAI script editor for mobile and desktop platforms.

<!-- 
═══════════════════════════════════════════════════════════════════════
📸 SCREENSHOT PLACEHOLDER
═══════════════════════════════════════════════════════════════════════
Replace with: Screenshot of MAUI app running on multiple platforms
Example: Side-by-side Windows/Android/iOS screenshots showing the code editor
File: /images/examples/maui-multiplatform.png
═══════════════════════════════════════════════════════════════════════
-->

---

## Features

- **Cross-Platform** - Windows, macOS, Android, iOS
- **MOGWAI Integration** - Full MOGWAI runtime embedded
- **Modern UI** - Clean .NET MAUI interface
- **Code Editor** - Write and edit MOGWAI scripts
- **Script Execution** - Run scripts on any platform
- **Save/Load Scripts** - Manage scripts locally
- **STUDIO Support** - Debug scripts with MOGWAI STUDIO
- **All MOGWAI Functions** - Access to all 200+ built-in functions
- **Persistent State** - Variables persist between executions
- **HTML Console** - Rich output rendering via WebView
- **Adjustable Font Size** - Customize editor and output font size
- **Run/Pause Indicators** - Visual status indicators
- **Halt Execution** - Stop running scripts anytime

---

## Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- [.NET MAUI workload](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation)
- Visual Studio 2022 17.8+ or Visual Studio Code with C# Dev Kit
- MOGWAI package (automatically restored from NuGet)

### Install MAUI Workload

```bash
dotnet workload install maui
```

### Build and Run

```bash
# Navigate to the MAUI directory
cd examples/MOGWAI_RUNTIME

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run on Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0

# Run on Android (with emulator or device)
dotnet build -t:Run -f net9.0-android

# Run on iOS (macOS only, with simulator or device)
dotnet build -t:Run -f net9.0-ios

# Run on macOS (macOS only)
dotnet build -t:Run -f net9.0-maccatalyst
```

Or open the solution in Visual Studio 2022 and select your target platform.

---

## 📱 Supported Platforms

| Platform | Version | Status |
|----------|---------|--------|
| Windows | 10.0.19041+ | ✅ Fully supported |
| Android | 5.0+ (API 21+) | ✅ Fully supported |
| iOS | 11.0+ | ✅ Fully supported |
| macOS | 10.15+ | ✅ Fully supported |

---

## Application Features

### Script Editor

The app provides a simple code editor where you can write and execute MOGWAI scripts:

```mogwai
# All standard MOGWAI functions work
"Hello from MAUI!" ?

# Platform detection
mogwai.info -> 'info'
info->platform ?  # Prints: Android, iOS, Windows, or macOS

# Math and calculations
2 3 + ?          # Prints: 5
(1 2 3 4 5) sum ?  # Prints: 15

# String manipulation
"MOGWAI" ->lower ?  # Prints: mogwai

# Lists and records
[name: "Mobile", platform: "MAUI"] -> 'app'
app->name ?  # Prints: Mobile
```

### Persistent Variables

**Important:** Variables and functions **persist between script executions** (`keepAlive: true`).

```mogwai
# First execution
42 -> 'answer'

# Second execution (variables still exist)
answer 2 / ?  # Prints: 21
```

To reset the engine state, restart the app or clear variables manually:

```mogwai
# Reset engine
mogwai.reset
```

### Save and Load Scripts

- **Save:** Store scripts locally on device
- **Load:** Open previously saved scripts
- **Auto-save:** Optional auto-save functionality

### Run Scripts

- **Execute:** Run scripts with full MOGWAI runtime
- **Output:** View results in output panel
- **Errors:** See detailed error messages with line numbers

### Debug Mode

- **Enable Debug:** Run scripts in debug mode
- **STUDIO Connection:** Connect to MOGWAI STUDIO for breakpoints
- **Step Through:** Debug scripts interactively

---

## Implementation Details

### Engine Configuration

```csharp
var engine = new MogwaiEngine(
    name: "MAUI Runtime",
    keepAlive: true,            // Variables persist between script executions
    useDefaultFolders: false    // Mobile apps manage their own storage
);
```

**Why `keepAlive: true`?**
- Variables and functions **persist** between script executions
- Great for **interactive development** and testing
- Similar to CLI REPL behavior
- **Note:** You can reset the engine state via code or by restarting the app

**Why `useDefaultFolders: false`?**
- Mobile platforms have different storage models
- Each platform has its own app data directory
- Scripts stored in platform-specific locations (app data folder)

### Platform-Specific Storage

```csharp
// Get platform-specific app data directory
var appDataPath = FileSystem.AppDataDirectory;

// Store scripts
var scriptsPath = Path.Combine(appDataPath, "Scripts");
Directory.CreateDirectory(scriptsPath);

// Save script
File.WriteAllText(Path.Combine(scriptsPath, "myscript.mog"), scriptContent);
```

### IDelegate Implementation

The app implements standard `IDelegate` methods for console I/O:

```csharp
public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
{
    // Display in output panel
    await MainThread.InvokeOnMainThreadAsync(() =>
    {
        OutputTextBox.Text += message + "\n";
    });
    return EvalResult.NoError;
}

public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
{
    await MainThread.InvokeOnMainThreadAsync(() =>
    {
        OutputTextBox.Text = string.Empty;
    });
    return EvalResult.NoError;
}
```

### No Custom Functions

The app provides **no custom host functions** - only standard MOGWAI functions:

```csharp
public string[] HostFunctions(MogwaiEngine engine) => [];

public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
{
    return EvalResult.NoExternalFunction;
}
```

All 200+ built-in MOGWAI functions are available out of the box.

### Thread-Safe UI Updates

All UI operations use `MainThread.InvokeOnMainThreadAsync`:

```csharp
private async void RunButton_Clicked(object sender, EventArgs e)
{
    var script = CodeEditor.Text;
    
    // Run on background thread
    var result = await Task.Run(async () => 
        await _engine.RunAsync(script, debugMode: false)
    );
    
    // Update UI on main thread
    if (result.IsError)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await DisplayAlert("Error", result.Error.Message, "OK");
        });
    }
}
```

---

## Example Scripts

### Platform Detection

```mogwai
# Detect current platform
mogwai.info -> '$info'

"Running on: " $info->platform + ?
"MOGWAI version: " $info->version + ?
"Framework: " $info->framework + ?
```

### Math and Calculations

```mogwai
# Fibonacci sequence
to 'fibonacci' with [n: .number] do
{
    if (n 2 <=) then
    {
        1
    }
    else
    {
        n 1 - fibonacci
        n 2 - fibonacci
        +
    }
}

# Calculate first 10 Fibonacci numbers
10 1 for 'i' do
{
    i fibonacci ?
}
```

### List Processing

```mogwai
# Create and process lists
(1 2 3 4 5 6 7 8 9 10) -> 'numbers'

# Filter even numbers
() -> 'evens'
numbers foreach 'n' do { if (n n 2 mod 0 ==) then { evens n + -> 'evens' } }
evens ?  # Prints: (2 4 6 8 10)

# Map to squares
() -> 'squares'
evens foreach 'n' do { squares n n * + -> 'squares' }
squares ?  # Prints: (4 16 36 64 100)

# Calculate sum
squares sum -> 'total'

"Sum of squares of evens: {! total}" eval ?
```

### String Manipulation

```mogwai
# String operations
"MOGWAI Runtime" -> 'title'

title ->lower ?           # mogwai runtime
title ->upper ?           # MOGWAI RUNTIME
title size ?              # 14
title 0 6 sub ?           # MOGWAI
```

### Records and Data

```mogwai
# Create a record
[
    name: "Mobile App",
    version: "1.0",
    platform: "MAUI",
    features: ("Editor" "Runtime" "Debug")
] -> 'app'

# Access fields
"App: " app->name + ?
"Version: " app->version  + ?
"Platform: " app->platform + ?

# List features
app->features foreach 'f' do { "- " f + ? }
```

### Functions and Recursion

```mogwai
# Factorial with recursion
to 'factorial' with [n: .number] do
{
    if (n 1 <=) then
    {
        1
    }
    else
    {
        n n 1 - factorial *
    }
}

# Test
5 factorial ?  # Prints: 120
10 factorial ? # Prints: 3628800
```

---

## Debugging with MOGWAI STUDIO

### Network Configuration

Mobile devices can connect to MOGWAI STUDIO over WiFi:

```csharp
// Start on same network as development machine
await engine.StartNetworkCommunication(address: "0.0.0.0", port: 1968);
```

### Connection Steps

1. Ensure mobile device and PC are on **same WiFi network**
2. Start MOGWAI STUDIO on PC
3. Tap **"Connect to STUDIO"** in the mobile app
4. STUDIO will discover the mobile runtime
5. Set breakpoints and debug on device!

### Platform Considerations

- **Android:** May require network permissions in `AndroidManifest.xml`
- **iOS:** May require local network permission in `Info.plist`
- **Firewall:** Ensure ports 1968 and 63000-65000 are open

---

## Cross-Platform Benefits

### Write Once, Run Everywhere

The same MOGWAI script runs identically on all platforms:

```mogwai
# This script works on Windows, macOS, Android, iOS
to 'greet' with [name: .string] do
{
    "Hello, " name + "!" + ?
}

"World" greet
```

### Platform Information

Use `mogwai.info` to detect the current platform:

```mogwai
mogwai.info -> 'info'

# Available info
info->platform ?    # "Windows", "Android", "iOS", "macOS"
info->version ?     # MOGWAI version
info->framework ?   # .NET version
```

### Platform-Specific Behaviors

**Android:**
- Automatically prevents screen timeout during script execution
- Back button returns to editor (stops running script if needed)

**Windows:**
- Right-click to open script and file menus

**iOS/macOS:**
- Touch gestures for font size adjustment
- Standard iOS/macOS app behavior

### Storage Considerations

Each platform stores scripts in its native app data location:

- **Windows:** `C:\Users\[User]\AppData\Local\Packages\[AppId]\LocalState\`
- **Android:** `/data/user/0/[package]/files/`
- **iOS:** `[App Container]/Library/`
- **macOS:** `~/Library/Containers/[BundleId]/Data/`

---

## UI Examples

### XAML Integration

```xml
<!-- MainPage.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="MOGWAI_RUNTIME.MainPage">
    <Grid>
        <Editor x:Name="CodeEditor"
                Placeholder="Enter MOGWAI script here..."
                FontFamily="Courier New" />
        
        <Button Text="Run Script"
                Clicked="RunButton_Clicked" />
        
        <Label x:Name="OutputLabel"
               Text="Output will appear here" />
    </Grid>
</ContentPage>
```

### Code-Behind

```csharp
private async void RunButton_Clicked(object sender, EventArgs e)
{
    var script = CodeEditor.Text;
    var result = await _engine.RunAsync(script, debugMode: false);
    
    if (result.IsError)
    {
        await DisplayAlert("Error", result.Error.Message, "OK");
    }
}
```

---

## Distribution

### Android (APK/AAB)

```bash
dotnet publish -f net9.0-android -c Release
```

Output: `bin/Release/net9.0-android/publish/`

### iOS (IPA)

```bash
dotnet publish -f net9.0-ios -c Release
```

Requires Apple Developer account for distribution.

### Windows (MSIX)

```bash
dotnet publish -f net9.0-windows10.0.19041.0 -c Release
```

Output: `bin/Release/net9.0-windows10.0.19041.0/publish/`

### macOS (APP)

```bash
dotnet publish -f net9.0-maccatalyst -c Release
```

---

## Learning Resources

### MAUI Tutorials

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [MAUI Samples](https://github.com/dotnet/maui-samples)

### MOGWAI on Mobile

**Best practices:**
1. Keep scripts simple on mobile (limited resources)
2. Use async operations for long-running tasks
3. Provide feedback during script execution
4. Handle platform-specific capabilities gracefully

---

## UI Customization

### XAML Layout

The app uses a simple XAML layout:

```xml
<!-- MainPage.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="MOGWAI_RUNTIME.MainPage"
             Title="MOGWAI Runtime">
    <Grid RowDefinitions="*, Auto, 2*, Auto">
        
        <!-- Code Editor -->
        <Editor Grid.Row="0"
                x:Name="CodeEditor"
                Placeholder="Enter MOGWAI script here..."
                FontFamily="Courier New"
                FontSize="14" />
        
        <!-- Buttons -->
        <HorizontalStackLayout Grid.Row="1" Padding="10">
            <Button Text="Run" Clicked="RunButton_Clicked" />
            <Button Text="Clear" Clicked="ClearButton_Clicked" />
            <Button Text="Save" Clicked="SaveButton_Clicked" />
            <Button Text="Load" Clicked="LoadButton_Clicked" />
            <Button Text="Debug" Clicked="DebugButton_Clicked" />
        </HorizontalStackLayout>
        
        <!-- Output -->
        <Editor Grid.Row="2"
                x:Name="OutputEditor"
                IsReadOnly="True"
                Placeholder="Output will appear here..."
                FontFamily="Courier New"
                FontSize="14" />
        
        <!-- Status -->
        <Label Grid.Row="3"
               x:Name="StatusLabel"
               Text="Ready"
               Padding="10" />
    </Grid>
</ContentPage>
```

### Theming

MAUI supports light/dark themes automatically:

```xml
<!-- App.xaml -->
<Application.Resources>
    <Color x:Key="Primary">#512BD4</Color>
    <Color x:Key="Secondary">#DFD8F7</Color>
    <Color x:Key="Tertiary">#2B0B98</Color>
</Application.Resources>
```

### Custom Styling

Customize the editor appearance:

```xml
<Editor FontFamily="Consolas"
        FontSize="16"
        TextColor="{AppThemeBinding Light=Black, Dark=White}"
        BackgroundColor="{AppThemeBinding Light=White, Dark=#1E1E1E}" />
```

---

## Performance Tips

### Mobile Optimization

1. **Minimize allocations** in tight loops
2. **Use async/await** for long operations
3. **Cache compiled scripts** for repeated execution
4. **Limit debug output** on mobile
5. **Test on real devices** not just emulators

### Battery Considerations

```mogwai
# Avoid infinite loops on mobile
100 1 for 'i' do
{
    i ?
    100 wait  # Give CPU time to rest
}
```

---

## 📚 Documentation

- **[Language Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_EN.md)** - Complete MOGWAI language guide
- **[Function Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 240+ built-in functions
- **[Integration Guide](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI in your .NET apps


---

## 🔗 Related Examples

- **[MOGWAI CLI](https://github.com/Sydney680928/mogwai/tree/main/examples/Console)** - Command-line interface and REPL
- **[WinForms Example](https://github.com/Sydney680928/mogwai/tree/main/examples/WinForms)** - Turtle graphics with MOGWAI

---

## 🎯 Use Cases

MOGWAI Runtime on MAUI is perfect for:

- **Learning MOGWAI** - Learn the language on any device
- **Script Testing** - Test MOGWAI scripts on mobile before deployment
- **Cross-Platform Development** - Write scripts that work everywhere
- **Mobile Scripting** - Run calculations and algorithms on the go
- **Education** - Teaching programming on tablets and phones
- **Prototyping** - Quick script development without a full IDE

---

## License

Apache License 2.0

See [LICENSE](https://github.com/Sydney680928/mogwai/tree/main/LICENSE) for details.

---

## Contributing

Ideas for improving the MOGWAI Runtime editor?

- **Report Issues:** [GitHub Issues](https://github.com/Sydney680928/mogwai/issues)
- **Pull Requests:** Contributions welcome!

Suggestions:
- Syntax highlighting for MOGWAI code
- Code completion / IntelliSense
- Line numbers in editor
- Find/Replace functionality
- Script library management
- Export scripts
- Keyboard shortcuts

---

**Happy scripting with MOGWAI on any platform!**

*For more information, visit [mogwai.eu.com](https://www.mogwai.eu.com)*
