# Bundled Fonts

This directory contains embedded fonts used by HtmlRendererCore.PdfSharp. These fonts are embedded as resources to ensure the library works in environments without access to system fonts (e.g., Docker containers, restricted environments).

## Included Fonts

We use **Liberation Fonts**, which are open-source, metric-compatible replacements for common proprietary fonts:

- **Liberation Sans** - Replacement for Arial/Helvetica
- **Liberation Serif** - Replacement for Times New Roman
- **Liberation Mono** - Replacement for Courier New

### Font Files Needed

Each font family requires 4 files (Regular, Bold, Italic, Bold-Italic):

#### Liberation Sans (Arial replacement)
- `LiberationSans-Regular.ttf`
- `LiberationSans-Bold.ttf`
- `LiberationSans-Italic.ttf`
- `LiberationSans-BoldItalic.ttf`

#### Liberation Serif (Times New Roman replacement)
- `LiberationSerif-Regular.ttf`
- `LiberationSerif-Bold.ttf`
- `LiberationSerif-Italic.ttf`
- `LiberationSerif-BoldItalic.ttf`

#### Liberation Mono (Courier New replacement)
- `LiberationMono-Regular.ttf`
- `LiberationMono-Bold.ttf`
- `LiberationMono-Italic.ttf`
- `LiberationMono-BoldItalic.ttf`

## How to Obtain Liberation Fonts

### Option 1: Direct Download from GitHub
Liberation Fonts are available on GitHub:
https://github.com/liberationfonts/liberation-fonts/releases

Download the latest release (e.g., liberation-fonts-ttf-2.1.5.tar.gz), extract it, and copy the TTF files to this directory.

### Option 2: Package Managers

**Ubuntu/Debian:**
```bash
sudo apt-get install fonts-liberation
# Fonts will be in /usr/share/fonts/truetype/liberation/
```

**Fedora/RHEL:**
```bash
sudo dnf install liberation-fonts
# Fonts will be in /usr/share/fonts/liberation/
```

**macOS (Homebrew):**
```bash
brew install liberation-fonts
```

**Windows:**
Download from the GitHub releases page linked above.

## License

Liberation Fonts are licensed under the SIL Open Font License 1.1, which allows:
- Free use in commercial and non-commercial applications
- Modification and redistribution
- Embedding in documents and applications

License: https://github.com/liberationfonts/liberation-fonts/blob/main/LICENSE

## Alternative Fonts

If you prefer different fonts, you can add other open-source fonts to this directory. Supported formats include:
- TrueType (.ttf)
- OpenType (.otf)

Make sure to update the `EmbeddedFontResolver.cs` to register your custom fonts.

### Other Open Source Font Options:
- **Noto Fonts** (Google) - Comprehensive Unicode coverage
- **Roboto** (Google) - Modern sans-serif
- **Open Sans** - Clean, readable sans-serif
- **Source Sans Pro** (Adobe) - Professional sans-serif
- **Fira Sans** (Mozilla) - Designed for Firefox OS

All of these are available under open licenses (SIL OFL, Apache 2.0, etc.).

## Font Mapping

The library automatically maps common font names to Liberation fonts:
- `Arial`, `Helvetica`, `sans-serif` → Liberation Sans
- `Times New Roman`, `Times`, `serif` → Liberation Serif
- `Courier New`, `Courier`, `monospace` → Liberation Mono

This ensures that HTML documents using standard fonts will render correctly even in restricted environments.
