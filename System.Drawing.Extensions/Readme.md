# Extensions to System.Drawing

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/System.Drawing.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.System.Drawing)](https://www.nuget.org/packages/FrameworkExtensions.System.Drawing/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods for System.Drawing types (bitmaps, images, colors, graphics).

---

## Extension Methods

### Bitmap Extensions (`Bitmap`)

#### Bitmap Locking

- **`Lock(...)`** - Lock bitmap for direct pixel access (8 overloads for different combinations)
  - `Lock()` - Lock entire bitmap with ReadWrite mode
  - `Lock(Rectangle)` - Lock region
  - `Lock(ImageLockMode)` - Lock with specific mode
  - `Lock(PixelFormat)` - Lock with specific format
  - `Lock(Rectangle, ImageLockMode)`
  - `Lock(Rectangle, PixelFormat)`
  - `Lock(ImageLockMode, PixelFormat)`
  - `Lock(Rectangle, ImageLockMode, PixelFormat)` - Full control
  - Returns format-specific `IBitmapLocker` implementation

#### Transformations

- **`ConvertPixelFormat(PixelFormat)`** - Convert bitmap to different pixel format
- **`Crop(Rectangle, PixelFormat = DontCare)`** - Extract region from bitmap
- **`Resize(int width, int height, InterpolationMode = Bicubic)`** - Resize bitmap
- **`Rotated(float angle, Point? center = null)`** - Create rotated copy of bitmap
- **`RotateInplace(float angle, Point? center = null)`** - Rotate bitmap in-place
- **`RotateTo(Bitmap target, float angle, Point? center = null)`** - Rotate to target bitmap

---

### Image Extensions (`Image`)

#### Multi-Page Support

- **`GetPageAt(int page)`** - Get specific page from multi-page image (e.g., TIFF)
- **`GetPageCount()`** - Get total number of pages in image

#### Save Operations

- **`SaveToPng(FileInfo/string)`** - Save image as PNG
- **`SaveToTiff(string)`** - Save image as TIFF
- **`SaveToJpeg(string/Stream, double quality = 1)`** - Save as JPEG with quality (0.0-1.0)

#### Conversions

- **`ToIcon(int targetRes = 0)`** - Convert image to Icon
- **`ToBase64DataUri()`** - Convert to Base64 data URI string
- **`FromBase64DataUri(string)`** (static-like extension on string) - Create image from Base64 data URI

#### Image Processing

- **`MakeGrayscale()`** - Convert image to grayscale
- **`Threshold(byte threshold = 127)`** - Apply threshold filter (black/white)
- **`ApplyPixelProcessor(Func<Color, Color> processor)`** - Apply custom pixel transformation function
- **`MirrorAlongX()`** - Mirror horizontally
- **`MirrorAlongY()`** - Mirror vertically
- **`Resize(int longSide)`** - Resize to fit within square, keep aspect
- **`Resize(int longSide, Color fillColor)`** - Resize with fill color
- **`Resize(int width, int height, bool keepAspect = true, Color? fillColor = null)`** - Resize with options
- **`Resize(int width = -1, int height = -1, InterpolationMode = Default)`** - Resize with interpolation
- **`Rotate(float angle)`** - Rotate image by angle
- **`GetRectangle(Rectangle)`** - Extract rectangular region
- **`ReplaceColorWithTransparency(Color)`** - Replace specific color with transparency

---

### Color Extensions (`Color`)

#### Color Space Conversion

- **`Rgb`** / **`RgbNormalized`** - Convert to RGB color space
- **`Hsl`** / **`HslNormalized`** - Convert to HSL (Hue, Saturation, Lightness)
- **`Hsv`** / **`HsvNormalized`** - Convert to HSV (Hue, Saturation, Value)
- **`Hwb`** / **`HwbNormalized`** - Convert to HWB (Hue, Whiteness, Blackness)
- **`Cmyk`** / **`CmykNormalized`** - Convert to CMYK (Cyan, Magenta, Yellow, Key)
- **`Xyz`** / **`XyzNormalized`** - Convert to CIE XYZ tristimulus values
- **`Lab`** / **`LabNormalized`** - Convert to CIE L*a*b* perceptual color space
- **`Yuv`** / **`YuvNormalized`** - Convert to YUV (Luma + chrominance)
- **`YCbCr`** / **`YCbCrNormalized`** - Convert to YCbCr digital video encoding
- **`Din99`** / **`Din99Normalized`** - Convert to DIN99 (DIN 6176) perceptual space

#### Color Comparison

- **`IsLike(Color other, byte luminanceDelta = 24, byte chromaUDelta = 7, byte chromaVDelta = 6)`** - Compare colors in YUV space with tolerance
- **`IsLikeNaive(Color other, int tolerance = 2)`** - Simple RGB comparison with tolerance

#### Color Blending

- **`BlendWith(Color other, float current, float max)`** - Interpolate between two colors

---

### Graphics Extensions (`Graphics`)

- **`DrawString(float x, float y, string text, Font font, Brush brush, ContentAlignment anchor)`** - Draw text with anchor positioning
- **`DrawCross(float/int x, float/int y, float/int size, Pen pen)`** - Draw cross marker (4 overloads including Point/PointF)
- **`DrawCircle(Pen pen, float centerX, float centerY, float radius)`** - Draw circle outline
- **`FillCircle(Brush brush, float centerX, float centerY, float radius)`** - Draw filled circle

---

### Rectangle Extensions (`Rectangle`)

- **`MultiplyBy(int factor)`** - Scale rectangle uniformly
- **`MultiplyBy(int xfactor, int yfactor)`** - Scale rectangle with different X/Y factors
- **`CollidesWith(Rectangle/RectangleF)`** - Check rectangle collision
- **`CollidesWith(Point/PointF)`** - Check if point is inside
- **`CollidesWith(int x, int y)` / `CollidesWith(float x, float y)`** - Check if coordinates are inside
- **`Center()`** - Get center point
- **`SetLeft/SetRight/SetTop/SetBottom(int)`** - Create new rectangle with modified edge

---

### RectangleF Extensions (`RectangleF`)

(Similar methods to Rectangle, for floating-point rectangles)

---

### Size Extensions (`Size`)

- **`Center()`** - Get center point

---

### Point Extensions (`Point`)

(Generated from T4 template - numeric operations on points)

---

### FileInfo Extensions (`FileInfo`)

- **`GetIcon(bool smallIcon = false, bool linkOverlay = false)`** - Get Windows shell icon for file
  - Uses native SHGetFileInfo API

---

## Custom Types

### IBitmapLocker Interface

Format-specific bitmap pixel access implementations:

- **ARGB32BitmapLocker** - 32-bit ARGB (Format32bppArgb)
- **RGB32BitmapLocker** - 32-bit RGB (Format32bppRgb)
- **RGB24BitmapLocker** - 24-bit RGB (Format24bppRgb)
- **RGB565BitmapLocker** - 16-bit RGB 565 (Format16bppRgb565)
- **ARGB1555BitmapLocker** - 16-bit ARGB 1555 (Format16bppArgb1555)
- **Gray16BitmapLocker** - 16-bit grayscale (Format16bppGrayScale)
- **RGB555BitmapLocker** - 16-bit RGB 555 (Format16bppRgb555)
- **Indexed8BitmapLocker** - 8-bit indexed (Format8bppIndexed)
- **IndexedBitmapLocker** - 1-bit and 4-bit indexed (Format1bppIndexed, Format4bppIndexed)
- **UnsupportedDrawingBitmapLocker** - Fallback for unsupported formats

Each locker provides optimized pixel access for its specific format.

---

## Color Spaces (`System.Drawing.ColorSpaces`)

A comprehensive color space conversion and comparison library with zero-cost generic abstractions.

### Color Space Types

| Type | Components | Description |
|------|------------|-------------|
| `Rgb` / `RgbNormalized` | R, G, B | Standard RGB color model |
| `Hsl` / `HslNormalized` | H, S, L | Hue, Saturation, Lightness |
| `Hsv` / `HsvNormalized` | H, S, V | Hue, Saturation, Value |
| `Hwb` / `HwbNormalized` | H, W, B | Hue, Whiteness, Blackness |
| `Lab` / `LabNormalized` | L, a, b | CIE L*a*b* perceptual color space |
| `Xyz` / `XyzNormalized` | X, Y, Z | CIE XYZ tristimulus values |
| `Yuv` / `YuvNormalized` | Y, U, V | Luma + chrominance (PAL/NTSC) |
| `YCbCr` / `YCbCrNormalized` | Y, Cb, Cr | Digital video color encoding |
| `Din99` / `Din99Normalized` | L, a, b | DIN 6176 perceptual space |
| `Cmyk` / `CmykNormalized` | C, M, Y, K | Subtractive printing model |

### Distance Calculators (`System.Drawing.ColorSpaces.Distances`)

| Calculator | Color Space | Description |
|------------|-------------|-------------|
| `EuclideanDistance<T>` | Any 3-component | √(Δc1² + Δc2² + Δc3²) |
| `EuclideanDistance4<T>` | Any 4-component | For CMYK and similar |
| `ManhattanDistance<T>` | Any 3-component | \|Δc1\| + \|Δc2\| + \|Δc3\| |
| `ChebyshevDistance<T>` | Any 3-component | max(\|Δc1\|, \|Δc2\|, \|Δc3\|) |
| `WeightedEuclideanRgbDistance` | RGB | Perception-weighted RGB |
| `RedmeanDistance` | RGB | Compuserve algorithm |
| `CIE76Distance` | Lab | ΔE*ab (basic Lab distance) |
| `CIE94Distance` | Lab | Improved perceptual formula |
| `CIEDE2000Distance` | Lab | Most accurate perceptual ΔE |
| `CMCDistance` | Lab | Textile industry (l=1, c=1) |
| `CMCAcceptabilityDistance` | Lab | Textile acceptability (l=2, c=1) |
| `DIN99Distance` | DIN99 | German standard DIN 6176 |

All calculators also have `*Squared` variants for faster comparisons.

### Palette Search

```csharp
using System.Drawing.ColorSpaces;
using System.Drawing.ColorSpaces.Distances;

// Find closest color in palette using perceptual distance
var palette = new[] { Color.Red, Color.Green, Color.Blue };
var index = PaletteSearch.GetMostSimilarColorIndex<CIEDE2000Distance>(palette, targetColor);

// Use any color space for comparison
var index = PaletteSearch.GetMostSimilarColorIndex<EuclideanDistance<Yuv>>(palette, targetColor);

// Cache lookups for repeated queries (e.g., image processing)
var cached = new CachedPalette<EuclideanDistance<Yuv>>(palette);
foreach (var pixel in imagePixels) {
  var paletteIndex = cached.GetIndex(pixel);  // O(1) after first lookup
}
```

### Color Interpolation (`System.Drawing.ColorSpaces.Interpolation`)

| Type | Description |
|------|-------------|
| `ColorLerp<T>` | Linear interpolation in any 3-component color space |
| `ColorLerp4<T>` | Linear interpolation in 4-component spaces (CMYK) |
| `CircularHueLerp<T>` | Hue-aware interpolation for HSL/HSV/HWB |
| `ColorGradient<T>` | Multi-stop gradient with configurable interpolation |

```csharp
using System.Drawing.ColorSpaces.Interpolation;

// Interpolate in perceptual Lab space
var lerp = new ColorLerp<Lab>();
var midpoint = lerp.Lerp(color1, color2, 0.5f);

// Create smooth gradients
var gradient = new ColorGradient<Hsl>(Color.Red, Color.Blue);
var colors = gradient.GetColors(10);  // 10 evenly-spaced colors
```

---

## Installation

```bash
dotnet add package FrameworkExtensions.System.Drawing
```

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
