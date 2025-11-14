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

- **`GetLuminance()`** - Calculate luminance (Y component in YUV)
- **`GetChrominanceU()`** - Calculate U chrominance component
- **`GetChrominanceV()`** - Calculate V chrominance component

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

## Installation

```bash
dotnet add package FrameworkExtensions.System.Drawing
```

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
