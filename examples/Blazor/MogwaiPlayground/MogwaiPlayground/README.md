# MOGWAI Playground

> Try MOGWAI in your browser — no installation required.

An interactive web REPL for the [MOGWAI](https://github.com/Sydney680928/mogwai) RPN scripting language, built with **Blazor WebAssembly**.  
The MOGWAI engine runs **entirely client-side** via WebAssembly — no server, no data transmitted.

## Quick Start

```bash
dotnet run
```

Then open `https://localhost:5000`.

## Project Structure

```
MogwaiPlayground/
├── Components/
│   └── MogwaiDelegate.cs     # Bridge between MOGWAI engine and Blazor UI
├── Pages/
│   └── Home.razor            # Interactive terminal (REPL)
├── Layout/
│   └── MainLayout.razor
├── wwwroot/
│   ├── index.html
│   └── css/app.css
└── .github/workflows/
    └── deploy.yml            # Automatic GitHub Pages deployment
```

## Stack

- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) — .NET in the browser
- [MOGWAI](https://www.nuget.org/packages/MOGWAI/) — RPN engine (NuGet)
- GitHub Pages — free static hosting

## GitHub Pages Deployment

1. Go to **Settings → Pages → Source** and select **GitHub Actions**
2. Push to `main` — the workflow triggers automatically
3. Your playground will be live at `https://<user>.github.io/<repo>/`

> **Note:** Features requiring file system access, BLE, or serial ports are not available
> in the browser. The core RPN engine works in full.

## License

Apache 2.0 — see [LICENSE](https://github.com/Sydney680928/mogwai/tree/main/LICENSE)
