# Font Bundling Implementation

## Overview

HtmlRendererCore.PdfSharp now includes support for bundled fonts to work reliably in environments without access to system fonts, such as:
- Docker containers
- Azure Functions / AWS Lambda
- Restricted security contexts
- Headless servers

## Implementation Details

### 1. Custom Font Resolver
**File**: `Fonts/EmbeddedFontResolver.cs`

The `EmbeddedFontResolver` class implements PdfSharpCore's `IFontResolver` interface to load fonts from embedded resources instead of the system. Key features:
- Loads TTF font files embedded as assembly resources
- Caches font data in memory for performance
- Provides fallback font resolution
- Maps common font names to Liberation equivalents

### 2. Font Mappings
The adapter automatically maps common web fonts to their Liberation equivalents:

| Requested Font      | Maps To           |
|---------------------|-------------------|
| Arial               | Liberation Sans   |
| Helvetica           | Liberation Sans   |
| sans-serif          | Liberation Sans   |
| Times New Roman     | Liberation Serif  |
| Times               | Liberation Serif  |
| serif               | Liberation Serif  |
| Courier New         | Liberation Mono   |
| Courier             | Liberation Mono   |
| monospace           | Liberation Mono   |

### 3. Liberation Fonts
We use Liberation Fonts as they are:
- **Open Source**: Licensed under SIL Open Font License 1.1
- **Metric Compatible**: Drop-in replacements for Arial, Times New Roman, and Courier New
- **High Quality**: Maintained by Red Hat/Liberation Fonts project
- **Complete**: Include Regular, Bold, Italic, and Bold-Italic variants

### 4. Modified Files

#### `PdfSharpAdapter.cs`
- Added `InitializeFontResolver()` method to set up the custom font resolver
- Updated font family mappings to use Liberation fonts
- Initialize embedded font resolver in constructor

#### `HtmlRendererCore.PdfSharp.csproj`
- Added `<EmbeddedResource Include="Fonts\*.ttf" />` to embed all TTF files
- Fonts are compiled into the assembly as resources

## Setup Instructions

### For Developers

1. **Download Liberation Fonts**:
   ```powershell
   cd HtmlRendererCore.PdfSharp\Fonts
   .\download-fonts.ps1
   ```

   Or manually download from:
   https://github.com/liberationfonts/liberation-fonts/releases

2. **Place Font Files** in `HtmlRendererCore.PdfSharp\Fonts\`:
   - LiberationSans-Regular.ttf
   - LiberationSans-Bold.ttf
   - LiberationSans-Italic.ttf
   - LiberationSans-BoldItalic.ttf
   - LiberationSerif-Regular.ttf
   - LiberationSerif-Bold.ttf
   - LiberationSerif-Italic.ttf
   - LiberationSerif-BoldItalic.ttf
   - LiberationMono-Regular.ttf
   - LiberationMono-Bold.ttf
   - LiberationMono-Italic.ttf
   - LiberationMono-BoldItalic.ttf

3. **Build the Project**:
   ```bash
   dotnet build
   ```
   The fonts will be embedded automatically.

### For Users

No action required! The fonts are embedded in the assembly. The library will automatically:
1. Use embedded fonts when system fonts are not available
2. Fall back to system fonts if they are available
3. Provide Liberation fonts as replacements for common fonts

## Testing in Docker

To test the font resolution in a Docker environment:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY . .

# No font installation needed - fonts are embedded!
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

## Customization

### Adding Additional Fonts

1. Place TTF/OTF files in the `Fonts` directory
2. Update `EmbeddedFontResolver.RegisterBundledFonts()` to register the new fonts:

```csharp
["customfont-regular"] = ("HtmlRendererCore.PdfSharp.Fonts.CustomFont-Regular.ttf", "Custom Font", false, false),
["customfont-bold"] = ("HtmlRendererCore.PdfSharp.Fonts.CustomFont-Bold.ttf", "Custom Font", true, false),
// Add more variants...
```

3. Add font family mappings in `PdfSharpAdapter.cs`:

```csharp
AddFontFamilyMapping("MyFont", "Custom Font");
```

### Font File Size Considerations

Liberation Fonts add approximately:
- Liberation Sans: ~500 KB (4 files)
- Liberation Serif: ~550 KB (4 files)
- Liberation Mono: ~400 KB (4 files)
- **Total**: ~1.5 MB to the assembly

This is a reasonable tradeoff for Docker/cloud compatibility.

## Troubleshooting

### Fonts Not Loading
1. Verify font files are in `Fonts` directory before building
2. Check build output for embedded resource warnings
3. Ensure font files match the expected naming convention

### Font Rendering Issues
1. Check if the requested font is mapped in `EmbeddedFontResolver`
2. Verify the font style (Bold/Italic) exists
3. The system falls back to Regular style if specific style is unavailable

### Debug Output
Enable debug output to see font resolution messages:
```csharp
System.Diagnostics.Debug.WriteLine("Font resolver initialized");
```

## License Compliance

**Liberation Fonts**: SIL Open Font License 1.1
- Free for commercial and non-commercial use
- Can be bundled with applications
- Can be modified and redistributed
- No attribution required (but appreciated)

License text: https://github.com/liberationfonts/liberation-fonts/blob/main/LICENSE

## Performance

- Font data is loaded once during initialization
- Fonts are cached in memory
- No performance impact compared to system fonts
- Slightly increased assembly size (~1.5 MB)

## Future Enhancements

Potential improvements:
- [ ] Support for web font loading (WOFF/WOFF2)
- [ ] On-demand font loading (lazy loading)
- [ ] Additional font family support
- [ ] Font subsetting to reduce size
- [ ] Custom font configuration via config file
