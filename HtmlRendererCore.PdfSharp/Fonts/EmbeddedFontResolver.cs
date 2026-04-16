using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HtmlRendererCore.PdfSharp.Fonts
{
    /// <summary>
    /// Custom font resolver that loads fonts from embedded resources.
    /// This allows the library to work in environments without access to system fonts (e.g., Docker containers).
    /// </summary>
    internal class EmbeddedFontResolver : IFontResolver
    {
        private static readonly Dictionary<string, FontData> _fontCache = new Dictionary<string, FontData>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        /// <summary>
        /// Default font name to use when requested font is not available
        /// </summary>
        public string DefaultFontName => "Liberation Sans";

        /// <summary>
        /// Represents font data loaded from embedded resources
        /// </summary>
        private class FontData
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public string FaceName { get; set; } = string.Empty;
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
        }

        /// <summary>
        /// Initialize the font resolver by loading all embedded font resources
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            lock (_lockObject)
            {
                if (_initialized)
                    return;

                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceNames = assembly.GetManifestResourceNames();

                    // Register default font mappings for bundled fonts
                    RegisterBundledFonts(assembly, resourceNames);

                    _initialized = true;
                }
                catch (Exception ex)
                {
                    // Log or handle initialization errors
                    System.Diagnostics.Debug.WriteLine($"Error initializing embedded fonts: {ex.Message}");
                }
            }
        }

        private static void RegisterBundledFonts(Assembly assembly, string[] resourceNames)
        {
            // Define the bundled fonts with their resource paths and properties
            var fontMappings = new Dictionary<string, (string ResourcePath, string FaceName, bool IsBold, bool IsItalic)>
            {
                // Liberation fonts (metrics-compatible with Arial, Times New Roman, Courier New)
                ["liberationsans-regular"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-Regular.ttf", "Liberation Sans", false, false),
                ["liberationsans-bold"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-Bold.ttf", "Liberation Sans", true, false),
                ["liberationsans-italic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-Italic.ttf", "Liberation Sans", false, true),
                ["liberationsans-bolditalic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-BoldItalic.ttf", "Liberation Sans", true, true),

                ["liberationserif-regular"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-Regular.ttf", "Liberation Serif", false, false),
                ["liberationserif-bold"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-Bold.ttf", "Liberation Serif", true, false),
                ["liberationserif-italic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-Italic.ttf", "Liberation Serif", false, true),
                ["liberationserif-bolditalic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-BoldItalic.ttf", "Liberation Serif", true, true),

                ["liberationmono-regular"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-Regular.ttf", "Liberation Mono", false, false),
                ["liberationmono-bold"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-Bold.ttf", "Liberation Mono", true, false),
                ["liberationmono-italic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-Italic.ttf", "Liberation Mono", false, true),
                ["liberationmono-bolditalic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-BoldItalic.ttf", "Liberation Mono", true, true),

                // Map common font names to Liberation fonts
                ["arial"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-Regular.ttf", "Liberation Sans", false, false),
                ["arial-bold"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-Bold.ttf", "Liberation Sans", true, false),
                ["arial-italic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-Italic.ttf", "Liberation Sans", false, true),
                ["arial-bolditalic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSans-BoldItalic.ttf", "Liberation Sans", true, true),

                ["timesnewroman"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-Regular.ttf", "Liberation Serif", false, false),
                ["timesnewroman-bold"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-Bold.ttf", "Liberation Serif", true, false),
                ["timesnewroman-italic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-Italic.ttf", "Liberation Serif", false, true),
                ["timesnewroman-bolditalic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationSerif-BoldItalic.ttf", "Liberation Serif", true, true),

                ["couriernew"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-Regular.ttf", "Liberation Mono", false, false),
                ["couriernew-bold"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-Bold.ttf", "Liberation Mono", true, false),
                ["couriernew-italic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-Italic.ttf", "Liberation Mono", false, true),
                ["couriernew-bolditalic"] = ("HtmlRendererCore.PdfSharp.Fonts.LiberationMono-BoldItalic.ttf", "Liberation Mono", true, true),
            };

            foreach (var mapping in fontMappings)
            {
                var key = mapping.Key;
                var (resourcePath, faceName, isBold, isItalic) = mapping.Value;

                try
                {
                    // Check if the resource exists
                    using var stream = assembly.GetManifestResourceStream(resourcePath);
                    if (stream != null)
                    {
                        var fontData = new byte[stream.Length];
                        stream.Read(fontData, 0, fontData.Length);

                        _fontCache[key] = new FontData
                        {
                            Data = fontData,
                            FaceName = faceName,
                            IsBold = isBold,
                            IsItalic = isItalic
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not load font resource {resourcePath}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get the font key for the given font family name and style
        /// </summary>
        private static string GetFontKey(string familyName, bool isBold, bool isItalic)
        {
            var normalizedFamily = familyName.Replace(" ", "").ToLowerInvariant();

            // Map common font families to their Liberation equivalents
            normalizedFamily = normalizedFamily switch
            {
                "arial" or "helvetica" or "sans-serif" => "liberationsans",
                "timesnewroman" or "times" or "serif" => "liberationserif",
                "couriernew" or "courier" or "monospace" => "liberationmono",
                _ => normalizedFamily
            };

            var suffix = (isBold, isItalic) switch
            {
                (true, true) => "-bolditalic",
                (true, false) => "-bold",
                (false, true) => "-italic",
                _ => "-regular"
            };

            return normalizedFamily + suffix;
        }

        public byte[] GetFont(string faceName)
        {
            if (!_initialized)
                Initialize();

            // Parse the faceName to extract family and style
            var isBold = faceName.Contains("Bold", StringComparison.OrdinalIgnoreCase);
            var isItalic = faceName.Contains("Italic", StringComparison.OrdinalIgnoreCase);

            var familyName = faceName
                .Replace("Bold", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Italic", "", StringComparison.OrdinalIgnoreCase)
                .Replace("#", "")
                .Trim();

            var key = GetFontKey(familyName, isBold, isItalic);

            if (_fontCache.TryGetValue(key, out var fontData))
            {
                return fontData.Data;
            }

            // Fallback to regular variant if specific style not found
            var regularKey = GetFontKey(familyName, false, false);
            if (_fontCache.TryGetValue(regularKey, out fontData))
            {
                return fontData.Data;
            }

            // Ultimate fallback to Liberation Sans Regular
            if (_fontCache.TryGetValue("liberationsans-regular", out fontData))
            {
                return fontData.Data;
            }

            return Array.Empty<byte>();
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (!_initialized)
                Initialize();

            var key = GetFontKey(familyName, isBold, isItalic);

            if (_fontCache.TryGetValue(key, out var fontData))
            {
                return new FontResolverInfo(key, isBold, isItalic);
            }

            // Try to find a fallback font
            var regularKey = GetFontKey(familyName, false, false);
            if (_fontCache.TryGetValue(regularKey, out fontData))
            {
                return new FontResolverInfo(regularKey, isBold, isItalic);
            }

            // Ultimate fallback to Liberation Sans
            return new FontResolverInfo("liberationsans-regular", isBold, isItalic);
        }
    }
}
