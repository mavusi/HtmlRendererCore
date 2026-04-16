# Developer Quick Start Guide

This guide is for developers contributing to HtmlRendererCore or building from source.

## Prerequisites

- .NET 8.0, 9.0, or 10.0 SDK
- Visual Studio 2022 or VS Code
- Git

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/mavusi/HtmlRendererCore.git
cd HtmlRendererCore
```

### 2. Download Liberation Fonts

**Option A: Automated (Recommended)**
```powershell
cd HtmlRendererCore.PdfSharp\Fonts
.\download-fonts.ps1
```

**Option B: Manual**
1. Download from: https://github.com/liberationfonts/liberation-fonts/releases
2. Extract the `.ttf` files from `liberation-fonts-ttf-*.tar.gz`
3. Copy all 12 TTF files to `HtmlRendererCore.PdfSharp\Fonts\`

### 3. Build the Project

```bash
dotnet build
```

The fonts will be embedded as resources during the build process.

### 4. Run Tests

```bash
dotnet test
```

## Required Font Files

Place these files in `HtmlRendererCore.PdfSharp\Fonts\`:

```
LiberationSans-Regular.ttf
LiberationSans-Bold.ttf
LiberationSans-Italic.ttf
LiberationSans-BoldItalic.ttf
LiberationSerif-Regular.ttf
LiberationSerif-Bold.ttf
LiberationSerif-Italic.ttf
LiberationSerif-BoldItalic.ttf
LiberationMono-Regular.ttf
LiberationMono-Bold.ttf
LiberationMono-Italic.ttf
LiberationMono-BoldItalic.ttf
```

## Project Structure

```
HtmlRendererCore/
├── HtmlRendererCore.PdfSharp/
│   ├── Adapters/
│   │   └── PdfSharpAdapter.cs          # Main adapter with font resolver init
│   ├── Fonts/
│   │   ├── EmbeddedFontResolver.cs     # Custom font resolver implementation
│   │   ├── *.ttf                       # Liberation font files (not in repo)
│   │   ├── README.md                   # Font download instructions
│   │   ├── IMPLEMENTATION.md           # Technical documentation
│   │   └── download-fonts.ps1          # Automated download script
│   └── HtmlRendererCore.PdfSharp.csproj
└── HtmlRendererCore.PdfSharp.Tests/
```

## Building a Package

To create a NuGet package:

```bash
dotnet pack -c Release
```

The package will include embedded fonts automatically.

## Testing in Docker

Create a test Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet build -c Release

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /src/bin/Release/ .

# No font installation needed - fonts are embedded!
ENTRYPOINT ["dotnet", "test-app.dll"]
```

## Common Issues

### Fonts Not Embedded
**Problem**: Build succeeds but fonts aren't in the assembly.
**Solution**: Ensure font files exist in `Fonts/` directory before building.

### Font Resolution Fails
**Problem**: PDFs generated but fonts look wrong.
**Solution**: Check that `EmbeddedFontResolver.Initialize()` is called and font names match mappings.

### Build Warnings
**Problem**: "Could not find part of path" warnings during build.
**Solution**: This is expected if fonts haven't been downloaded yet. Download fonts and rebuild.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Ensure all tests pass
5. Submit a pull request

## Additional Resources

- [Font Implementation Details](HtmlRendererCore.PdfSharp/Fonts/IMPLEMENTATION.md)
- [Font README](HtmlRendererCore.PdfSharp/Fonts/README.md)
- [Liberation Fonts License](https://github.com/liberationfonts/liberation-fonts/blob/main/LICENSE)

## Need Help?

- Open an issue on GitHub
- Check existing issues for solutions
- Review the documentation files in the Fonts directory
