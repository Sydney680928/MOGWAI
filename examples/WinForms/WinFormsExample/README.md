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

## Features

- **Turtle Graphics** - Logo-style drawing with MOGWAI scripts
- **Visual Output** - Real-time rendering of turtle movements
- **Code Editor** - Built-in editor with syntax highlighting
- ▶**Run/Stop Controls** - Execute scripts with visual feedback
- **STUDIO Integration** - Connect to MOGWAI STUDIO for debugging
- **Custom Functions** - Turtle-specific MOGWAI commands
- **Save/Load Scripts** - Manage your turtle graphics programs

---

## Quick Start

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

## Turtle Graphics Commands

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

## Example Scripts

### Draw a Square

```mogwai
# Simple square
turtle.clear
turtle.penDown

4 repeat
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

clg

5 repeat
{
    500 turtle.move
    144 turtle.turn
}
```

### Flower Pattern

```mogwai
# Draw a flower

clg

1 12 for 'i' do 
{ 
	# Draw petal
    
	1 6 for 'j' do
	{       
		100 turtle.move
		30 turtle.turn
	}
   
	# Rotate to next petal
   
	30 turtle.turn
}

```
---

## Implementation Details

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
    return [
        "clg",
        "refresh",
        "turtle.penDown",
        "turtle.penUp",
        "turtle.show",
        "turtle.hide",
        "turtle.move",
        "turtle.turn"
        ];
}

public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
{
    // Called when MOGWAI encounters a keyword it doesn't know.
    // In this case, it asks the host if it can respond.

    switch (word)
    {
        case "clg":
            return await ClgExtension(engine, word);

        case "refresh":
            return await RefreshExtension(engine, word);

        case "turtle.penDown":
            return await PenDownExtension(engine, word);

        case "turtle.penUp":
            return await PenUpExtension(engine, word);

        case "turtle.show":
            return await ShowTurtleExtension(engine, word);

        case "turtle.hide":
            return await HideTurtleExtension(engine, word);

        case "turtle.move":
            return await MoveExtension(engine, word);

        case "turtle.turn":
            return await TurnExtension(engine, word);
    }

    return EvalResult.NoExternalFunction;
}
```

### Thread-Safe UI Updates

All turtle graphics operations use `Invoke` for thread safety:

```csharp
public void TurtleForward(int distance)
{
    if (InvokeRequired)
    {
        Invoke(() => { TurtleForward(distance); });
    }
    else
    {
        double a = DegToRad(180 - _turtleAngle);
        double x = _turtleX + distance * Math.Sin(a);
        double y = _turtleY + distance * Math.Cos(a);

        if (_penIsDown)
        {
            TurtleDrawLine(_turtleX, _turtleY, x, y);
        }

        _turtleX = x;
        _turtleY = y;

        if (_turtleIsVisible)
        {
            DrawTurtlePictureBox.Invalidate();
        }
    }
}
```

---

## Learning Path

### Beginner Scripts

1. **Simple Line:** `100 turtle.move`
2. **Simple Turn:** `100 turtle.move 90 turtle.turn 100 turtle.move`
3. **Square** (see example above)
4. **Triangle:** Similar to square but with 3 sides and 120° turns

### Intermediate Scripts

1. **Star** (see example above)
2. **Circle:** Use small forward steps with small angle turns
3. **Spiral** (see example above)
4. **Multiple Shapes:** Draw several shapes in different positions


## Advanced Example: Fractal Tree

```mogwai
# Recursive fractal tree

to 'tree' with [length: .number] do
{
    if (length 5 >) then
    {
        length turtle.move
        30 turtle.turn
        length 0.7 * tree
        -60 turtle.turn
        length 0.7 * tree
        30 turtle.turn
        length -1 * turtle.move
    }
}

# Clear and draw

clg
90 turtle.turn
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

## Documentation

- **[Language Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_EN.md)** - Complete MOGWAI language guide
- **[Function Reference](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_FUNCTIONS_EN.md)** - All 240+ built-in functions
- **[Integration Guide](https://github.com/Sydney680928/mogwai/tree/main/docs/EN/MOGWAI_INTEGRATION_GUIDE_EN.md)** - How to integrate MOGWAI in your .NET apps

---

## Related Examples

- **[MOGWAI CLI](https://github.com/Sydney680928/mogwai/tree/main/examples/Console)** - Command-line interface and REPL
- **[MAUI Example](https://github.com/Sydney680928/mogwai/tree/main/examples/MAUI)** - Cross-platform mobile app

---

## Use Cases

Turtle graphics with MOGWAI is perfect for:

- **Education:** Teaching programming concepts visually
- **Art:** Creating generative art and patterns
- **Prototyping:** Quick visualization of algorithms
- **Fun:** Exploring creative coding and recreational math

---

## License

Apache License 2.0

See [LICENSE](https://github.com/Sydney680928/mogwai/tree/main/LICENSE) and [NOTICE](https://github.com/Sydney680928/mogwai/tree/main/NOTICE) for details.

---

## Contributing

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

**Happy turtle graphics with MOGWAI!**

*For more information, visit [mogwai.eu.com](https://www.mogwai.eu.com)*
