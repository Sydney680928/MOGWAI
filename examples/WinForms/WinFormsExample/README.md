# WinForms MOGWAI - Turtle Graphics Example

A Windows Forms application demonstrating MOGWAI integration with turtle graphics and visual scripting.

<!-- 
═══════════════════════════════════════════════════════════════════════
📸 SCREENSHOT PLACEHOLDER
═══════════════════════════════════════════════════════════════════════
Replace with: Screenshot of WinForms app showing turtle graphics output
Example: Turtle drawing geometric patterns or fractals
File: /images/examples/winforms-turtle.png
═══════════════════════════════════════════════════════════════════════
-->

---

## ✨ Features

- 🐢 **Turtle Graphics** - Logo-style drawing with MOGWAI scripts
- 🎨 **Visual Output** - Real-time rendering of turtle movements
- 📝 **Code Editor** - Built-in editor with syntax highlighting
- ▶️ **Run/Stop Controls** - Execute scripts with visual feedback
- 🐛 **STUDIO Integration** - Connect to MOGWAI STUDIO for debugging
- 🎯 **Custom Functions** - Turtle-specific MOGWAI commands
- 💾 **Save/Load Scripts** - Manage your turtle graphics programs

---

## 🚀 Quick Start

### Prerequisites

- Windows 10/11
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- Visual Studio 2022 (recommended) or any .NET IDE
- MOGWAI package (automatically restored from NuGet)

### Build and Run

```bash
# Navigate to the WinForms directory
cd examples/WinFormsMogwai

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

Or open the solution in Visual Studio 2022 and press F5.

---

## 🎨 Turtle Graphics Commands

MOGWAI is extended with custom turtle graphics functions:

### Movement Commands

```mogwai
# Move forward/backward
100 turtle.forward        # Move forward 100 pixels
50 turtle.backward        # Move backward 50 pixels

# Turn left/right
90 turtle.right          # Turn right 90 degrees
45 turtle.left           # Turn left 45 degrees

# Pen control
turtle.penUp             # Lift pen (don't draw)
turtle.penDown           # Lower pen (draw)

# Position
0 0 turtle.goto          # Go to coordinates (0, 0)
turtle.home              # Return to center (0, 0) facing up
```

### Drawing Attributes

```mogwai
# Pen color
"Red" turtle.color       # Set pen color
"#FF5733" turtle.color   # Use hex color

# Pen width
3 turtle.width           # Set pen width to 3 pixels

# Clear screen
turtle.clear             # Clear drawing canvas
```

---

## 📝 Example Scripts

### Draw a Square

```mogwai
# Simple square
turtle.clear
turtle.penDown

4 1 for 'i' do
{
    100 turtle.forward
    90 turtle.right
}
```

<!-- 
═══════════════════════════════════════════════════════════════════════
📸 SCREENSHOT PLACEHOLDER
═══════════════════════════════════════════════════════════════════════
Replace with: Square drawn by turtle
File: /images/examples/turtle-square.png
═══════════════════════════════════════════════════════════════════════
-->

### Draw a Star

```mogwai
# Five-pointed star
turtle.clear
"Yellow" turtle.color
2 turtle.width

5 1 for 'i' do
{
    100 turtle.forward
    144 turtle.right
}
```

### Spiral Pattern

```mogwai
# Colorful spiral
turtle.clear

36 1 for 'i' do
{
    i 10 * turtle.forward
    10 turtle.right
    
    # Change color every 6 steps
    if (i 6 mod 0 ==) then
    {
        "Red" turtle.color
    }
    
    if (i 6 mod 3 ==) then
    {
        "Blue" turtle.color
    }
}
```

### Flower Pattern

```mogwai
# Draw a flower
turtle.clear
"Purple" turtle.color
1 turtle.width

12 1 for 'i' do
{
    # Draw petal
    6 1 for 'j' do
    {
        30 turtle.forward
        30 turtle.right
    }
    
    # Rotate to next petal
    30 turtle.right
}
```

---

## 🔧 Implementation Details

### Engine Configuration

```csharp
var engine = new MogwaiEngine(
    name: "WinForms Turtle",
    useDefaultFolders: false    // No Documents folder for embedded app
);
```

**Why `useDefaultFolders: false`?**
- Embedded application manages its own script storage
- No need for Documents/MOGWAI/ folder structure
- Scripts can be stored as embedded resources or in app directory

### Custom Turtle Functions

The application extends MOGWAI with turtle graphics functions via `IDelegate`:

```csharp
public string[] HostFunctions(MogwaiEngine engine)
{
    return new[] 
    {
        "turtle.forward",
        "turtle.backward",
        "turtle.right",
        "turtle.left",
        "turtle.penUp",
        "turtle.penDown",
        "turtle.color",
        "turtle.width",
        "turtle.clear",
        "turtle.home",
        "turtle.goto"
    };
}

public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
{
    switch (word)
    {
        case "turtle.forward":
            return ExecuteTurtleForward(engine);
        
        case "turtle.right":
            return ExecuteTurtleTurn(engine, clockwise: true);
        
        // ... other functions
    }
    
    return EvalResult.NoExternalFunction;
}
```

### Thread-Safe UI Updates

All turtle graphics operations use `Invoke` for thread safety:

```csharp
private EvalResult ExecuteTurtleForward(MogwaiEngine engine)
{
    var sig = engine.StackSign(1);
    if (sig.Count == 0 || sig[0] != typeof(MOGNumber))
        return EvalResult.Failure(engine, Error.BadArgumentTypeError, "turtle.forward");
    
    var distance = engine.StackPopNumber();
    
    // UI thread invocation
    Invoke(() =>
    {
        MoveTurtle(distance.Value);
        TurtleCanvas.Refresh();
    });
    
    return EvalResult.NoError;
}
```

---

## 🐛 Debugging with MOGWAI STUDIO

### Enable STUDIO Connection

1. Click **"Enable STUDIO"** button in the toolbar
2. Launch MOGWAI STUDIO
3. STUDIO will auto-discover the WinForms runtime
4. Set breakpoints in your turtle graphics scripts
5. Step through code and watch the turtle move!

### Debug Mode

Run scripts with `debugMode: true` to enable breakpoints:

```csharp
var result = await engine.RunAsync(scriptText, debugMode: true);
```

---

## 🎓 Learning Path

### Beginner Scripts

1. **Simple Line:** `100 turtle.forward`
2. **Simple Turn:** `100 turtle.forward 90 turtle.right 100 turtle.forward`
3. **Square** (see example above)
4. **Triangle:** Similar to square but with 3 sides and 120° turns

### Intermediate Scripts

1. **Star** (see example above)
2. **Circle:** Use small forward steps with small angle turns
3. **Spiral** (see example above)
4. **Multiple Shapes:** Draw several shapes in different positions

### Advanced Scripts

1. **Fractals:** Koch snowflake, Sierpinski triangle
2. **Recursive Patterns:** Tree structures, branching
3. **Parametric Designs:** User-input driven patterns
4. **Animations:** Use timers to animate drawings

---

## 🎨 Advanced Example: Fractal Tree

```mogwai
# Recursive fractal tree
to 'tree' with [length: .number] do
{
    if (length 5 >) then
    {
        length turtle.forward
        30 turtle.right
        length 0.7 * tree
        60 turtle.left
        length 0.7 * tree
        30 turtle.right
        length turtle.backward
    }
}

# Clear and draw
turtle.clear
"Brown" turtle.color
2 turtle.width
90 turtle.left
100 tree
```

<!-- 
═══════════════════════════════════════════════════════════════════════
📸 SCREENSHOT PLACEHOLDER
═══════════════════════════════════════════════════════════════════════
Replace with: Fractal tree drawn by turtle
File: /images/examples/turtle-fractal-tree.png
═══════════════════════════════════════════════════════════════════════
-->

---

## 💡 Tips and Tricks

### Performance

- Use `turtle.penUp` when repositioning without drawing
- Minimize `Refresh()` calls for complex drawings
- Consider batch rendering for animations

### Colors

Supported color formats:
- Named colors: `"Red"`, `"Blue"`, `"Green"`, etc.
- Hex colors: `"#FF5733"`, `"#00FF00"`
- RGB via custom function (if implemented)

### Canvas Size

The canvas is typically 800x600 pixels:
- Center: (0, 0)
- X range: -400 to +400
- Y range: -300 to +300

---

## 🔧 Customization

### Add Your Own Functions

Extend the turtle graphics with custom functions:

```csharp
public string[] HostFunctions(MogwaiEngine engine)
{
    return new[] 
    {
        // ... existing turtle functions
        "turtle.circle",
        "turtle.stamp",
        "turtle.fill"
    };
}
```

### Change Canvas Background

```csharp
TurtleCanvas.BackColor = Color.Black;  // Dark mode
```

### Export Drawings

Add a "Save Image" button:

```csharp
private void SaveImageButton_Click(object sender, EventArgs e)
{
    using var bitmap = new Bitmap(TurtleCanvas.Width, TurtleCanvas.Height);
    TurtleCanvas.DrawToBitmap(bitmap, TurtleCanvas.ClientRectangle);
    bitmap.Save("turtle_output.png");
}
```

---

## 📚 Documentation

- **[MOGWAI Language Guide](../../docs/EN/MOGWAI_EN.md)** - Complete language reference
- **[Function Reference](../../docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 200+ built-in functions
- **[Integration Guide](../../docs/EN/MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI

---

## 🔗 Related Examples

- **[MOGWAI CLI](../MOGWAI_CLI/)** - Command-line interface and REPL
- **[MAUI Example](../MOGWAI_RUNTIME/)** - Cross-platform mobile app

---

## 🎯 Use Cases

Turtle graphics with MOGWAI is perfect for:

- **Education:** Teaching programming concepts visually
- **Art:** Creating generative art and patterns
- **Prototyping:** Quick visualization of algorithms
- **Fun:** Exploring creative coding and recreational math

---

## 📄 License

Apache License 2.0

See [LICENSE](../../LICENSE) for details.

---

## 🤝 Contributing

Ideas for new turtle graphics features?

- **Report Issues:** [GitHub Issues](https://github.com/Sydney680928/mogwai/issues)
- **Pull Requests:** Contributions welcome!

Suggestions:
- Additional drawing primitives (circles, polygons)
- Fill operations
- Image stamping
- Animation features
- Export to SVG

---

**Happy turtle graphics with MOGWAI!** 🐢🎨

*For more information, visit [mogwai.eu.com](https://www.mogwai.eu.com)*
