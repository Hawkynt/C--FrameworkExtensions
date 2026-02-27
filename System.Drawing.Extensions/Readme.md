# Extensions to System.Drawing

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/System.Drawing.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.System.Drawing)](https://www.nuget.org/packages/FrameworkExtensions.System.Drawing/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods for System.Drawing types (bitmaps, images, colors, graphics) with a comprehensive image processing pipeline including pixel art scalers, resamplers, ditherers, quantizers, and color spaces.

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
- **`FlipHorizontal()`** - Mirror left-to-right, returns new bitmap
- **`FlipVertical()`** - Mirror top-to-bottom, returns new bitmap
- **`MirrorAlongAxis(PointF p1, PointF p2)`** - Mirror pixels across an arbitrary line defined by two points (bilinear interpolation)
- **`ZoomToPoint(PointF center, float factor)`** - Crop/zoom centered on a specific point with given magnification factor
- **`Straighten(float angle)`** - Deskew by rotating then cropping to the largest inscribed axis-aligned rectangle
- **`Skew(float angleX, float angleY)`** - Shear/skew transformation with horizontal and vertical shear angles
- **`AutoRotate()`** - Read EXIF orientation tag (0x0112) and apply the correct rotation/flip to normalize the image

#### Color Quantization & Dithering

- **`ReduceColors<TQuantizer, TDitherer>(quantizer, ditherer, colorCount, isHighQuality)`** - Reduce image to indexed palette
  - Returns indexed bitmap (1bpp for 2 colors, 4bpp for ≤16, 8bpp for ≤256)
  - `isHighQuality: true` uses OkLab perceptual color space (slower, better gradients)
  - `isHighQuality: false` uses linear RGB with Euclidean distance (faster)

```csharp
using var original = new Bitmap("photo.png");

// High quality: OkLab color space, Floyd-Steinberg dithering
using var indexed = original.ReduceColors(
    new WuQuantizer(),
    ErrorDiffusion.FloydSteinberg,
    16,
    isHighQuality: true);

// Fast: Linear RGB, serpentine dithering
using var fast = original.ReduceColors(
    new OctreeQuantizer(),
    ErrorDiffusion.Atkinson.Serpentine,
    256,
    isHighQuality: false);

indexed.Save("indexed.gif");
```

#### Pixel Art Scaling

- **`Upscale(IPixelScaler)`** - Scale bitmap using a fixed-factor pixel art scaler

```csharp
using var source = new Bitmap("sprite.png");
using var scaled2x = source.Upscale(Eagle.X2);
using var hq4x = source.Upscale(Hqnx.X4);
```

#### Resampling

- **`Resample<TResampler>(width, height)`** - Resample bitmap to arbitrary size using a resampler
- **`Resample(IResampler, width, height)`** - Resample with parameterized resampler instance

```csharp
using var photo = bitmap.Resample<Lanczos3>(newWidth, newHeight);
using var sharp = bitmap.Resample(new Bicubic(-0.75f), newWidth, newHeight);
```

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
- **`Lab`** / **`LabNormalized`** - Convert to CIE L\*a\*b\* perceptual color space
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

Format-specific bitmap pixel access implementations providing optimized read/write access to bitmap data. The correct locker is automatically selected based on the bitmap's pixel format when calling `Lock()`.

| Locker                             | Pixel Format           | Bits | Description                                |
| ---------------------------------- | ---------------------- | ---- | ------------------------------------------ |
| **Argb16161616BitmapLocker**       | `Format64bppArgb`      | 64   | High dynamic range ARGB (16 bits/channel)  |
| **Argb1555BitmapLocker**           | `Format16bppArgb1555`  | 16   | 1-bit alpha + 5 bits RGB                   |
| **Argb8888BitmapLocker**           | `Format32bppArgb`      | 32   | Standard 32-bit ARGB (8 bits/channel)      |
| **Gray16BitmapLocker**             | `Format16bppGrayScale` | 16   | 16-bit grayscale                           |
| **Indexed1BitmapLocker**           | `Format1bppIndexed`    | 1    | Monochrome indexed (2 colors)              |
| **Indexed4BitmapLocker**           | `Format4bppIndexed`    | 4    | 16-color indexed                           |
| **Indexed8BitmapLocker**           | `Format8bppIndexed`    | 8    | 256-color indexed                          |
| **PArgb16161616BitmapLocker**      | `Format64bppPArgb`     | 64   | Premultiplied alpha 64-bit                 |
| **PArgb8888BitmapLocker**          | `Format32bppPArgb`     | 32   | Premultiplied alpha 32-bit                 |
| **Rgb161616BitmapLocker**          | `Format48bppRgb`       | 48   | High dynamic range RGB (16 bits/channel)   |
| **Rgb555BitmapLocker**             | `Format16bppRgb555`    | 16   | 5 bits per RGB channel                     |
| **Rgb565BitmapLocker**             | `Format16bppRgb565`    | 16   | 5-6-5 bits RGB                             |
| **Rgb888BitmapLocker**             | `Format24bppRgb`       | 24   | Standard 24-bit RGB                        |
| **Rgb888XBitmapLocker**            | `Format32bppRgb`       | 32   | 24-bit RGB with padding byte               |
| **SubRegionBitmapLocker**          | Any                    | -    | Wraps another locker for sub-region access |
| **UnsupportedDrawingBitmapLocker** | Other                  | -    | Fallback using `GetPixel`/`SetPixel`       |

Each locker provides:
- Direct pointer access to pixel data via `BitmapData`
- Indexed pixel access via `this[x, y]`
- Fast `GetPixelBgra8888()` and `SetPixelBgra8888()` methods
- Drawing primitives: lines, rectangles, circles, ellipses, crosses
- `CopyFrom`/`BlendWith` operations for bitmap compositing
- Grid-based tile copying for sprite sheets and tile maps
- Automatic disposal via `IDisposable`

---

## Color Spaces

A comprehensive color space conversion and comparison library with zero-cost generic abstractions.

### Standard Color Space Types

| Type                        | Components | Category    | Description                          |
| --------------------------- | ---------- | ----------- | ------------------------------------ |
| `Cmyk` / `CmykNormalized`   | C, M, Y, K | Subtractive | Subtractive printing model           |
| `Din99` / `Din99Normalized` | L, a, b    | Perceptual  | DIN 6176 perceptual space            |
| `Hsl` / `HslNormalized`     | H, S, L    | Cylindrical | Hue, Saturation, Lightness           |
| `Hsv` / `HsvNormalized`     | H, S, V    | Cylindrical | Hue, Saturation, Value               |
| `Hwb` / `HwbNormalized`     | H, W, B    | Cylindrical | Hue, Whiteness, Blackness            |
| `Lab` / `LabNormalized`     | L, a, b    | Perceptual  | CIE L\*a\*b\* perceptual color space |
| `Rgb` / `RgbNormalized`     | R, G, B    | Standard    | Standard RGB color model             |
| `Xyz` / `XyzNormalized`     | X, Y, Z    | HDR         | CIE XYZ tristimulus values           |
| `YCbCr` / `YCbCrNormalized` | Y, Cb, Cr  | YUV         | Digital video color encoding         |
| `Yuv` / `YuvNormalized`     | Y, U, V    | YUV         | Luma + chrominance (PAL/NTSC)        |

### Extended Color Space Types

Internal processing color spaces used in the generic color pipeline:

| Type           | Components | Category    | Description                                          |
| -------------- | ---------- | ----------- | ---------------------------------------------------- |
| `LchF`         | L, C, h    | Cylindrical | CIE LCh (cylindrical Lab) with hue angle             |
| `OklabF`       | L, a, b    | Perceptual  | Oklab perceptually uniform color space               |
| `OklchF`       | L, C, h    | Perceptual  | OkLCh cylindrical form of Oklab                      |
| `HunterLabF`   | L, a, b    | Perceptual  | Hunter Lab color space                               |
| `LuvF`         | L, u, v    | Perceptual  | CIE L\*u\*v\* color space                            |
| `ICtCpF`       | I, Ct, Cp  | HDR         | ICtCp perceptual HDR color space                     |
| `JzAzBzF`      | Jz, Az, Bz | HDR         | JzAzBz perceptual HDR color space                    |
| `JzCzhzF`      | Jz, Cz, hz | HDR         | JzCzhz cylindrical form of JzAzBz                    |
| `AcesCgF`      | R, G, B    | Wide Gamut  | ACEScg linear working space for VFX/animation        |
| `AdobeRgbF`    | R, G, B    | Wide Gamut  | Adobe RGB (1998) wide gamut for print (50% coverage) |
| `DisplayP3F`   | R, G, B    | Wide Gamut  | Display P3 (Apple devices, wider than sRGB)          |
| `ProPhotoRgbF` | R, G, B    | Wide Gamut  | ProPhoto RGB ultra-wide gamut for photography        |

### Working Color Spaces

Internal processing types used by the quantization and dithering pipeline:

| Type          | Components    | Description                                          |
| ------------- | ------------- | ---------------------------------------------------- |
| `LinearRgbaF` | R, G, B, A    | Linear RGB with alpha (float, non-premultiplied)     |
| `LinearRgbF`  | R, G, B       | Linear RGB without alpha (float)                     |
| `CmykaF`      | C, M, Y, K, A | CMYK with alpha (float)                              |
| `OklabaF`     | L, a, b, A    | OkLab with alpha for perceptually uniform operations |

### Distance Calculators (`Hawkynt.ColorProcessing.Metrics`)

Color metrics implementing `IColorMetric<T>` for palette lookups, quantization, and color comparison.

#### Generic Metrics

| Calculator          | Color Space           | Squared Variant            | Description                                                                                            |
| ------------------- | --------------------- | -------------------------- | ------------------------------------------------------------------------------------------------------ |
| `Euclidean3F<TKey>` | Any 3-component float | `EuclideanSquared3F<TKey>` | $\sqrt{\Delta c_1^2 + \Delta c_2^2 + \Delta c_3^2}$                                                    |
| `Euclidean4F<TKey>` | Any 4-component float | `EuclideanSquared4F<TKey>` | $\sqrt{\Delta c_1^2 + \Delta c_2^2 + \Delta c_3^2 + \Delta c_4^2}$                                     |
| `Euclidean3B<TKey>` | Any 3-component byte  | `EuclideanSquared3B<TKey>` | Byte version (0-255 per channel)                                                                       |
| `Euclidean4B<TKey>` | Any 4-component byte  | `EuclideanSquared4B<TKey>` | Byte version with alpha                                                                                |
| `Chebyshev3F<TKey>` | Any 3-component float | -                          | $\max(\lvert\Delta c_1\rvert, \lvert\Delta c_2\rvert, \lvert\Delta c_3\rvert)$                         |
| `Chebyshev4F<TKey>` | Any 4-component float | -                          | $\max(\lvert\Delta c_1\rvert, \lvert\Delta c_2\rvert, \lvert\Delta c_3\rvert, \lvert\Delta c_4\rvert)$ |
| `Chebyshev3B<TKey>` | Any 3-component byte  | -                          | Byte version                                                                                           |
| `Chebyshev4B<TKey>` | Any 4-component byte  | -                          | Byte version with alpha                                                                                |
| `Manhattan3F<TKey>` | Any 3-component float | -                          | $\lvert\Delta c_1\rvert + \lvert\Delta c_2\rvert + \lvert\Delta c_3\rvert$                             |
| `Manhattan4F<TKey>` | Any 4-component float | -                          | $\lvert\Delta c_1\rvert + \lvert\Delta c_2\rvert + \lvert\Delta c_3\rvert + \lvert\Delta c_4\rvert$    |

#### Weighted Metrics

| Calculator                  | Squared Variant                    | Description                                                                             |
| --------------------------- | ---------------------------------- | --------------------------------------------------------------------------------------- |
| `WeightedEuclidean3F<TKey>` | `WeightedEuclideanSquared3F<TKey>` | $\sqrt{w_1 \Delta c_1^2 + w_2 \Delta c_2^2 + w_3 \Delta c_3^2}$                         |
| `WeightedEuclidean4F<TKey>` | `WeightedEuclideanSquared4F<TKey>` | $\sqrt{w_1 \Delta c_1^2 + w_2 \Delta c_2^2 + w_3 \Delta c_3^2 + w_4 \Delta c_4^2}$      |
| `WeightedChebyshev3F<TKey>` | -                                  | $\max(w_1\lvert\Delta c_1\rvert, w_2\lvert\Delta c_2\rvert, w_3\lvert\Delta c_3\rvert)$ |
| `WeightedManhattan3F<TKey>` | -                                  | $w_1\lvert\Delta c_1\rvert + w_2\lvert\Delta c_2\rvert + w_3\lvert\Delta c_3\rvert$     |

#### Perceptual Lab Metrics (`Hawkynt.ColorProcessing.Metrics.Lab`)

| Calculator                                                              | Squared Variant    | Reference                                                                                                                                             | Description                                |
| ----------------------------------------------------------------------- | ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| [`CIE76`](https://en.wikipedia.org/wiki/Color_difference#CIE76)         | `CIE76Squared`     | [CIE 1976](https://www.researchgate.net/publication/229712679_The_Development_of_the_CIE_1976_Lab_Uniform_Colour-Space_and_Colour-Difference_Formula) | $\Delta E^*_{ab}$ - Euclidean in Lab space |
| [`CIE94`](https://en.wikipedia.org/wiki/Color_difference#CIE94)         | `CIE94Squared`     | [CIE 1994](https://doi.org/10.1002/col.5080200107)                                                                                                    | Improved perceptual formula with weighting |
| [`CIEDE2000`](https://en.wikipedia.org/wiki/Color_difference#CIEDE2000) | `CIEDE2000Squared` | [CIE 2000](https://doi.org/10.1002/col.10049)                                                                                                         | Most accurate perceptual $\Delta E$        |
| [`CMC`](https://en.wikipedia.org/wiki/Color_difference#CMC_l:c_(1984))  | -                  | [BS 6923](https://standards.globalspec.com/std/320474/bs-6923)                                                                                        | Textile industry (l=1, c=1)                |
| [`DIN99Distance`](https://de.wikipedia.org/wiki/DIN99-Farbraum)         | -                  | [DIN 6176](https://www.wikiwand.com/de/articles/DIN99-Farbraum)                                                                                       | German industrial standard                 |

#### RGB-Specific Metrics (`Hawkynt.ColorProcessing.Metrics.Rgb`)

| Calculator                                             | Squared Variant     | Reference                                                      | Description                                   |
| ------------------------------------------------------ | ------------------- | -------------------------------------------------------------- | --------------------------------------------- |
| [`CompuPhase`](https://www.compuphase.com/cmetric.htm) | `CompuPhaseSquared` | [Redmean](https://en.wikipedia.org/wiki/Color_difference#sRGB) | Weighted RGB approximation using mean red     |
| [`PngQuant`](https://github.com/pornel/pngquant)       | `PngQuantSquared`   | [pngquant](https://pngquant.org/)                              | Considers blending on black/white backgrounds |

#### Equality Comparators

| Comparator                | Description                         |
| ------------------------- | ----------------------------------- |
| `ExactEquality<TKey>`     | Exact bit-level match only          |
| `ThresholdEquality<TKey>` | Match within configurable tolerance |

**Note**: Squared variants are faster (no sqrt) when only relative comparison is needed.

### Palette Lookup

The `PaletteLookup<TWork, TMetric>` struct provides efficient nearest-neighbor color matching with automatic caching.

#### Members

| Member                                           | Type    | Description                         |
| ------------------------------------------------ | ------- | ----------------------------------- |
| `Count`                                          | `int`   | Number of colors in the palette     |
| `this[int index]`                                | `TWork` | Get palette color at index          |
| `FindNearest(in TWork color)`                    | `int`   | Find index of nearest palette color |
| `FindNearestColor(in TWork color)`               | `TWork` | Find nearest palette color directly |
| `FindNearest(in TWork color, out TWork nearest)` | `int`   | Get both index and nearest color    |

#### Usage

```csharp
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Working;

// Create palette lookup with a metric
var lookup = new PaletteLookup<LinearRgbaF, EuclideanSquared4F<LinearRgbaF>>(
    workPalette,
    default);

// Get palette size
int paletteSize = lookup.Count;

// Access palette colors directly
LinearRgbaF firstColor = lookup[0];

// Find nearest by index
int index = lookup.FindNearest(targetColor);

// Find nearest color directly
LinearRgbaF nearest = lookup.FindNearestColor(targetColor);

// Get both index and color in one call
int idx = lookup.FindNearest(targetColor, out LinearRgbaF nearestColor);

// Efficient batch processing - results are cached automatically
foreach (var pixel in imagePixels) {
  var paletteIndex = lookup.FindNearest(pixel);  // O(1) for repeated colors
}

// Using different metrics
var labLookup = new PaletteLookup<LabF, CIEDE2000>(labPalette, default);
var rgbLookup = new PaletteLookup<LinearRgbF, CompuPhaseSquared>(rgbPalette, default);
```

### Color Interpolation (`System.Drawing.ColorSpaces.Interpolation`)

| Type                 | Description                                         |
| -------------------- | --------------------------------------------------- |
| `CircularHueLerp<T>` | Hue-aware interpolation for HSL/HSV/HWB             |
| `ColorGradient<T>`   | Multi-stop gradient with configurable interpolation |
| `ColorLerp<T>`       | Linear interpolation in any 3-component color space |
| `ColorLerp4<T>`      | Linear interpolation in 4-component spaces (CMYK)   |

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

## Color Quantization

Quantizers reduce the number of colors in an image to generate optimized palettes. They analyze color distribution and select representative colors that best preserve visual quality.

### Adaptive Quantizers

Adaptive quantizers analyze the image to generate an optimal palette for each specific image.

| Quantizer                                                                                                             | Author                | Year | Type       | Reference                                                                                                                                                  |
| --------------------------------------------------------------------------------------------------------------------- | --------------------- | ---- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`PngQuantQuantizer`](https://pngquant.org/)                                                                          | Kornel Lesinski       | 2009 | Hybrid     | [pngquant algorithm](https://pngquant.org/)                                                                                                                |
| [`WuQuantizer`](http://www.ece.mcmaster.ca/~xwu/cq.c)                                                                 | Xiaolin Wu            | 1991 | Variance   | [Graphics Gems II](https://dl.acm.org/doi/book/10.5555/90767)                                                                                              |
| [`MedianCutQuantizer`](https://dl.acm.org/doi/10.1145/965145.801294)                                                  | Paul Heckbert         | 1982 | Splitting  | [SIGGRAPH '82](https://dl.acm.org/doi/10.1145/965145.801294)                                                                                               |
| [`OctreeQuantizer`](https://www.cubic.org/docs/octree.htm)                                                            | Gervautz, Purgathofer | 1988 | Tree       | [Graphics Gems](https://dl.acm.org/doi/book/10.5555/90767)                                                                                                 |
| [`NeuquantQuantizer`](https://scientificgems.wordpress.com/stuff/neuquant-fast-high-quality-image-quantization/)      | Anthony Dekker        | 1994 | Neural     | [Network: Computing](https://doi.org/10.1088/0954-898X_5_3_003)                                                                                            |
| [`KMeansQuantizer`](https://en.wikipedia.org/wiki/K-means_clustering)                                                 | J. MacQueen           | 1967 | Clustering | [K-means++](https://dl.acm.org/doi/10.5555/1283383.1283494)                                                                                                |
| [`BisectingKMeansQuantizer`](https://www.cs.cmu.edu/~dunja/KDDpapers/Steinbach_IR.pdf)                                | M. Steinbach et al.   | 2000 | Clustering | [Bisecting K-Means](https://www.cs.cmu.edu/~dunja/KDDpapers/Steinbach_IR.pdf)                                                                              |
| [`IncrementalKMeansQuantizer`](https://en.wikipedia.org/wiki/Online_machine_learning)                                 | -                     | -    | Clustering | [Online K-Means](https://dl.acm.org/doi/10.1145/2020408.2020576)                                                                                           |
| [`GaussianMixtureQuantizer`](https://en.wikipedia.org/wiki/Mixture_model#Gaussian_mixture_model)                      | Various               | 1977 | Clustering | [EM Algorithm](https://www.jstor.org/stable/2984875)                                                                                                       |
| [`ColorQuantizationNetworkQuantizer`](https://en.wikipedia.org/wiki/Learning_vector_quantization)                     | Various               | 1992 | Neural     | [LVQ](https://link.springer.com/chapter/10.1007/978-3-642-97610-0_6)                                                                                       |
| [`AduQuantizer`](https://en.wikipedia.org/wiki/Competitive_learning)                                                  | -                     | -    | Clustering | [Competitive learning](https://en.wikipedia.org/wiki/Competitive_learning)                                                                                 |
| [`VarianceBasedQuantizer`](https://en.wikipedia.org/wiki/Variance)                                                    | -                     | -    | Variance   | [Weighted variance optimization](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/quantize.c)                                               |
| [`VarianceCutQuantizer`](https://en.wikipedia.org/wiki/Variance)                                                      | -                     | -    | Variance   | [Maximum variance splitting](https://github.com/kornelski/pngquant/blob/main/lib/mediancut.c)                                                              |
| [`BinarySplittingQuantizer`](https://en.wikipedia.org/wiki/Principal_component_analysis)                              | -                     | -    | Splitting  | [Principal axis splitting](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/quantize.c)                                                     |
| [`SpatialColorQuantizer`](https://en.wikipedia.org/wiki/Simulated_annealing)                                          | -                     | -    | Spatial    | [Simulated annealing](https://doi.org/10.1126/science.220.4598.671)                                                                                        |
| [`FuzzyCMeansQuantizer`](https://en.wikipedia.org/wiki/Fuzzy_clustering#Fuzzy_C-means_clustering)                     | J.C. Bezdek           | 1981 | Clustering | [Pattern Recognition](https://doi.org/10.1007/978-1-4757-0450-1). [Ref](https://github.com/scikit-learn/scikit-learn/blob/main/sklearn/cluster/_kmeans.py) |
| [`PopularityQuantizer`](https://en.wikipedia.org/wiki/Color_quantization#Popularity_algorithm)                        | -                     | -    | Histogram  | [Most frequent colors](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/quantize.c)                                                         |
| [`UniformQuantizer`](https://en.wikipedia.org/wiki/Color_quantization#Uniform_quantization)                           | -                     | -    | Fixed      | [Uniform RGB grid](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/quantize.c)                                                             |
| [`HierarchicalCompetitiveLearningQuantizer`](https://www.sciencedirect.com/science/article/abs/pii/S0167865597001165) | P. Scheunders         | 1997 | Clustering | [Hierarchical CL](https://www.sciencedirect.com/science/article/abs/pii/S0167865597001165) - initialization-independent progressive splitting              |
| [`GeneticCMeansQuantizer`](https://www.sciencedirect.com/science/article/abs/pii/S0167865597001165)                   | P. Scheunders         | 1997 | Clustering | [Genetic Algorithm + C-Means](https://www.sciencedirect.com/science/article/abs/pii/S0167865597001165) - global optimization avoids local optima           |
| [`EnhancedOctreeQuantizer`](http://www.leptonica.org/papers/colorquant.pdf)                                           | D.S. Bloomberg        | 2008 | Tree       | [Enhanced Octree](http://www.leptonica.org/color-quantization.html) - variance tracking, adaptive thresholds                                               |
| [`OctreeSomQuantizer`](https://doi.org/10.14311/NNW.2015.25.006)                                                      | Park, Kim, Cha        | 2015 | Neural     | [Octree-SOM](https://doi.org/10.14311/NNW.2015.25.006) - two-rate learning reduces color loss                                                              |
| [`HuffmanQuantizer`](https://en.wikipedia.org/wiki/Huffman_coding)                                                    | -                     | -    | Clustering | Huffman-inspired bottom-up merging - preserves dominant colors exactly                                                                                     |

#### Quantizer Parameters

| Quantizer                                    | Parameter                      | Type    | Default    | Range        | Description                                   |
| -------------------------------------------- | ------------------------------ | ------- | ---------- | ------------ | --------------------------------------------- |
| **PngQuantQuantizer**                        | `MedianCutIterations`          | `int`   | `3`        | 1-10         | Median Cut passes with weight adjustment      |
|                                              | `KMeansIterations`             | `int`   | `10`       | 1-100        | K-means/Voronoi refinement iterations         |
|                                              | `ConvergenceThreshold`         | `float` | `0.0001f`  | 0.00001-0.01 | K-means convergence threshold                 |
|                                              | `ErrorBoostFactor`             | `float` | `2.0f`     | 1-5          | Weight boost for underrepresented colors      |
| **KMeansQuantizer**                          | `MaxIterations`                | `int`   | `100`      | 10-1000      | Maximum clustering iterations                 |
|                                              | `ConvergenceThreshold`         | `float` | `0.001f`   | 0.0001-0.1   | Stop when centroids move less than this       |
| **BisectingKMeansQuantizer**                 | `MaxIterationsPerSplit`        | `int`   | `10`       | 1-50         | K-means iterations per bisection              |
|                                              | `BisectionTrials`              | `int`   | `3`        | 1-10         | Trials per split (keeps best result)          |
|                                              | `ConvergenceThreshold`         | `float` | `0.001f`   | 0.0001-0.1   | Stop when centroids move less than this       |
| **IncrementalKMeansQuantizer**               | `RefinementPasses`             | `int`   | `3`        | 0-10         | Additional refinement passes after initial    |
| **GaussianMixtureQuantizer**                 | `MaxIterations`                | `int`   | `50`       | 10-500       | Maximum EM iterations                         |
|                                              | `ConvergenceThreshold`         | `float` | `0.0001f`  | 0.00001-0.01 | Log-likelihood convergence threshold          |
|                                              | `MinVariance`                  | `float` | `0.0001f`  | 0.00001-0.01 | Minimum variance (prevents singular matrices) |
|                                              | `MaxSampleSize`                | `int`   | `10000`    | 1000-100000  | Maximum histogram entries to process          |
| **ColorQuantizationNetworkQuantizer**        | `MaxEpochs`                    | `int`   | `100`      | 10-500       | Training epochs                               |
|                                              | `InitialLearningRate`          | `float` | `0.3f`     | 0.01-1.0     | Initial learning rate                         |
|                                              | `ConscienceFactor`             | `float` | `0.1f`     | 0-1          | Balanced neuron usage factor                  |
|                                              | `UseFrequencySensitive`        | `bool`  | `true`     | -            | Enable frequency-sensitive learning           |
|                                              | `MaxSampleSize`                | `int`   | `10000`    | 1000-100000  | Maximum histogram entries to process          |
| **AduQuantizer**                             | `IterationCount`               | `int`   | `10`       | 1-100        | Competitive learning iterations               |
| **NeuquantQuantizer**                        | `MaxIterations`                | `int`   | `100`      | 1-1000       | Network training iterations                   |
|                                              | `InitialAlpha`                 | `float` | `0.1f`     | 0.01-1.0     | Initial learning rate                         |
| **SpatialColorQuantizer**                    | `MaxIterations`                | `int`   | `100`      | 10-500       | Annealing iterations                          |
|                                              | `SpatialWeight`                | `float` | `0.5f`     | 0-1          | Weight for spatial coherence                  |
|                                              | `InitialTemperature`           | `float` | `1.0f`     | 0.1-10       | Starting annealing temperature                |
| **FuzzyCMeansQuantizer**                     | `MaxIterations`                | `int`   | `100`      | 10-500       | Clustering iterations                         |
|                                              | `Fuzziness`                    | `float` | `2.0f`     | 1.1-5        | Cluster overlap (higher = softer)             |
|                                              | `MaxSampleSize`                | `int`   | `10000`    | 1000-100000  | Maximum histogram entries to process          |
| **HierarchicalCompetitiveLearningQuantizer** | `EpochsPerSplit`               | `int`   | `5`        | 1-20         | CL training epochs after each cluster split   |
|                                              | `InitialLearningRate`          | `float` | `0.1f`     | 0.01-0.5     | Initial learning rate for CL                  |
|                                              | `MaxSampleSize`                | `int`   | `8192`     | 1000-100000  | Maximum histogram entries to process          |
| **GeneticCMeansQuantizer**                   | `PopulationSize`               | `int`   | `20`       | 10-100       | Number of candidate palettes                  |
|                                              | `Generations`                  | `int`   | `50`       | 10-500       | Number of generations to evolve               |
|                                              | `TournamentSize`               | `int`   | `3`        | 2-10         | Tournament selection size                     |
|                                              | `MutationSigma`                | `float` | `10.0f`    | 1-50         | Gaussian mutation standard deviation          |
|                                              | `EliteCount`                   | `int`   | `2`        | 0-10         | Elite individuals preserved each generation   |
|                                              | `CMeansIterationsPerOffspring` | `int`   | `3`        | 1-10         | C-Means iterations per offspring              |
|                                              | `MaxSampleSize`                | `int`   | `10000`    | 1000-100000  | Maximum histogram entries to process          |
| **EnhancedOctreeQuantizer**                  | `MaxLevel`                     | `int`   | `5`        | 3-7          | Tree depth (2^level max leaves)               |
|                                              | `ReservedLevel2Colors`         | `int`   | `64`       | 0-128        | Colors reserved at level 2 for coverage       |
| **OctreeSomQuantizer**                       | `MaxEpochs`                    | `int`   | `50`       | 10-200       | SOM training epochs                           |
|                                              | `WinnerLearningRate`           | `float` | `0.1f`     | 0.01-0.5     | Learning rate for BMU                         |
|                                              | `NeighborLearningRate`         | `float` | `0.001f`   | 0.0001-0.01  | Learning rate for neighbors (1% of winner)    |
|                                              | `MaxSampleSize`                | `int`   | `8192`     | 1000-100000  | Maximum histogram entries to process          |
| **HuffmanQuantizer**                         | `CandidatesToExamine`          | `int`   | `20`       | 5-100        | Top candidates for merge selection            |
|                                              | `SimilarityWeight`             | `float` | `10000.0f` | 100-100000   | Balance frequency vs. similarity in merge     |

### Quantizer Wrappers

Wrappers that enhance other quantizers by applying additional preprocessing or post-processing. Wrappers can be chained for combined effects.

#### Preprocessing Wrappers

| Wrapper                                                                             | Description                                                                                                                                             |
| ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`PcaQuantizerWrapper`](https://en.wikipedia.org/wiki/Principal_component_analysis) | Transforms colors to PCA-aligned space before quantization. [Ref](https://github.com/scikit-learn/scikit-learn/blob/main/sklearn/decomposition/_pca.py) |
| [`BitReductionWrapper`](https://en.wikipedia.org/wiki/Bit_manipulation)             | Reduces color precision by masking off LSBs, creating posterized/retro effects and faster quantization                                                  |

#### Postprocessing Wrappers

| Wrapper                                                                         | Description                                                                                                                |
| ------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| [`KMeansRefinementWrapper`](https://en.wikipedia.org/wiki/K-means_clustering)   | Refines any quantizer's output using iterative K-means-style clustering. [Ref](https://github.com/ImageMagick/ImageMagick) |
| [`AcoRefinementWrapper`](https://en.wikipedia.org/wiki/Ant_colony_optimization) | Refines palette using Ant Colony Optimization for escaping local optima. [Ref](https://doi.org/10.1109/4235.585892)        |

#### Wrapper Parameters

| Wrapper                     | Parameter         | Type     | Default | Range   | Description                                   |
| --------------------------- | ----------------- | -------- | ------- | ------- | --------------------------------------------- |
| **BitReductionWrapper**     | `bitsToRemove`    | `int`    | `1`     | 1-7     | LSBs to mask off per component (1=128 levels) |
| **KMeansRefinementWrapper** | `iterations`      | `int`    | `10`    | 1-100   | K-means refinement iterations                 |
| **AcoRefinementWrapper**    | `antCount`        | `int`    | `20`    | 1-100   | Number of ants exploring solutions            |
|                             | `iterations`      | `int`    | `50`    | 1-500   | ACO iterations                                |
|                             | `evaporationRate` | `double` | `0.1`   | 0.0-1.0 | Pheromone evaporation rate                    |
|                             | `seed`            | `int?`   | `null`  | -       | Random seed for reproducibility               |

```csharp
// Wrap a quantizer with PCA preprocessing
var pcaEnhanced = new PcaQuantizerWrapper<WuQuantizer>(new WuQuantizer());

// Add K-means refinement to any quantizer
var refined = new KMeansRefinementWrapper<MedianCutQuantizer>(new MedianCutQuantizer(), iterations: 10);

// Use ACO for complex color distributions (slower but may escape local optima)
var acoRefined = new AcoRefinementWrapper<OctreeQuantizer>(
  new OctreeQuantizer(),
  antCount: 20,
  iterations: 50,
  seed: 42  // for reproducible results
);

// Reduce color precision for retro/posterized effects (4 bits removed = 16 levels per channel)
var posterized = new BitReductionWrapper<MedianCutQuantizer>(new MedianCutQuantizer(), bitsToRemove: 4);

// Chain wrappers for combined effects: BitReduction -> KMeans -> PCA -> Octree
var chained = new BitReductionWrapper<KMeansRefinementWrapper<PcaQuantizerWrapper<OctreeQuantizer>>>(
  new KMeansRefinementWrapper<PcaQuantizerWrapper<OctreeQuantizer>>(
    new PcaQuantizerWrapper<OctreeQuantizer>(new OctreeQuantizer()),
    iterations: 5),
  bitsToRemove: 2
);
```

### Fixed Palette Quantizers

Fixed palette quantizers use predefined color palettes for specific platforms or standards.

| Quantizer                                                                                          | Colors | Use Case     | Description                                                                                                                                                                          |
| -------------------------------------------------------------------------------------------------- | ------ | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| [`WebSafeQuantizer`](https://en.wikipedia.org/wiki/Web_colors#Web-safe_colors)                     | 216    | Web graphics | [Browser-safe web palette](https://www.color-hex.com/216-web-safe-colors/)                                                                                                           |
| [`Ega16Quantizer`](https://en.wikipedia.org/wiki/Enhanced_Graphics_Adapter#Color_palette)          | 16     | Retro DOS    | [IBM EGA 16-color palette](https://moddingwiki.shikadi.net/wiki/EGA_Palette)                                                                                                         |
| [`Vga256Quantizer`](https://en.wikipedia.org/wiki/VGA#Color_palette)                               | 256    | Retro DOS    | [VGA default 256-color palette](https://moddingwiki.shikadi.net/wiki/VGA_Palette)                                                                                                    |
| [`Cga4Quantizer`](https://en.wikipedia.org/wiki/Color_Graphics_Adapter#Color_palette)              | 4      | Retro DOS    | [6 CGA palettes](https://moddingwiki.shikadi.net/wiki/CGA_Palette): Palette0 (Green/Red/Brown), Palette1 (Cyan/Magenta/White), Mode5 (Cyan/Red/White) - each with Low/High intensity |
| [`Mac8BitQuantizer`](https://en.wikipedia.org/wiki/List_of_8-bit_computer_hardware_graphics#Apple) | 256    | Retro Mac    | [Classic Macintosh system palette](https://lospec.com/palette-list/macintosh-8-bit-system-palette)                                                                                   |
| `MonochromeQuantizer`                                                                              | 2      | B&W          | Black and white only                                                                                                                                                                 |
| [`GrayscaleQuantizer`](https://en.wikipedia.org/wiki/Grayscale)                                    | 2-256  | Grayscale    | [Grayscale ramp](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/quantize.c)                                                                                         |
| `CustomPaletteQuantizer`                                                                           | Any    | Custom       | User-defined palette                                                                                                                                                                 |

### Quantizer Usage

```csharp
using Hawkynt.ColorProcessing.Quantization;

// Generate palette from colors
var quantizer = new WuQuantizer();
Bgra8888[] palette = quantizer.GeneratePalette(colors, 16);

// Generate from histogram (weighted by frequency)
var histogram = new List<(Bgra8888 color, uint count)> { ... };
Bgra8888[] palette = quantizer.GeneratePalette(histogram, 256);

// Fixed palette
var egaPalette = new Ega16Quantizer().GetPalette();

// Reduce colors with quantization
using var reduced = bitmap.ReduceColors(new WuQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

// High-quality PngQuant-style quantization (variance median cut + K-means refinement)
var pngQuant = new PngQuantQuantizer {
  MedianCutIterations = 3,    // Iterations with weight adjustment for underrepresented colors
  KMeansIterations = 10,      // Voronoi/K-means refinement passes
  ErrorBoostFactor = 2.0f     // Boost for poorly quantized colors
};
using var pngResult = bitmap.ReduceColors(pngQuant, ErrorDiffusion.FloydSteinberg, 256);
```

### K-Means Color Metrics

The `KMeansQuantizer` supports any `IColorMetric<Bgra8888>` for clustering. See [Color Metrics](#distance-calculators-hawkyntcolorprocessingmetrics) for all available metrics.

```csharp
// Default K-Means (squared Euclidean - fastest)
var kmeans = new KMeansQuantizer();

// With perceptual Lab-based distance
var perceptual = new KMeansQuantizer<CIEDE2000>();

// With weighted RGB perception
var weighted = new KMeansQuantizer<PngQuantDistance>(default);

// With custom iterations
var custom = new KMeansQuantizer { MaxIterations = 200, ConvergenceThreshold = 0.0001f };
```

---

## Dithering

Error diffusion dithering distributes quantization error to neighboring pixels for smoother gradients.

### Error-Diffusion Ditherers

| Ditherer                                                                                      | Author                               | Year | Neighbors | Reference                                                                                                                                                           |
| --------------------------------------------------------------------------------------------- | ------------------------------------ | ---- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`Atkinson`](https://en.wikipedia.org/wiki/Atkinson_dithering)                                | Bill Atkinson                        | 1984 | 6         | [Apple MacPaint](https://en.wikipedia.org/wiki/MacPaint), 75% error diffusion. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/threshold.c)   |
| [`JarvisJudiceNinke`](<https://doi.org/10.1016/S0146-664X%2876%2980003-2>)                    | J.F. Jarvis, C.N. Judice, W.H. Ninke | 1976 | 12        | [CGIP vol. 5](https://doi.org/10.1016/S0146-664X%2876%2980003-2). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/threshold.c)                |
| [`Pigeon`](https://hbfs.wordpress.com/2013/12/31/dithering/)                                  | Steven Pigeon                        | 2013 | 7         | [Blog post with analysis](https://hbfs.wordpress.com/2013/12/31/dithering/). [Ref](https://github.com/stevenpigeon/DitherBenchmark)                                 |
| [`ShiauFan`](https://patents.google.com/patent/US5353127A)                                    | J.N. Shiau, Z. Fan                   | 1993 | 4         | [US Patent 5,353,127](https://patents.google.com/patent/US5353127A). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/threshold.c)             |
| [`ShiauFan2`](https://patents.google.com/patent/US5353127A)                                   | J.N. Shiau, Z. Fan                   | 1993 | 5         | [US Patent 5,353,127](https://patents.google.com/patent/US5353127A). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/threshold.c)             |
| [`StevensonArce`](https://opg.optica.org/josaa/abstract.cfm?uri=josaa-2-7-1009)               | R.L. Stevenson, G.R. Arce            | 1985 | 12        | [JOSA A vol. 2](https://opg.optica.org/josaa/abstract.cfm?uri=josaa-2-7-1009). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/threshold.c)   |
| [`Stucki`](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html) | P. Stucki                            | 1981 | 12        | [IBM Research RZ1060](https://en.wikipedia.org/wiki/Error_diffusion). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/threshold.c)            |
| [`Burkes`](https://en.wikipedia.org/wiki/Error_diffusion)                                     | D. Burkes                            | 1988 | 7         | CIS Graphics Support Forum, LIB 15. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/threshold.c)                                              |
| `Diagonal`                                                                                    | -                                    | -    | 1         | Single diagonal neighbor                                                                                                                                            |
| `Diamond`                                                                                     | -                                    | -    | 8         | Symmetric diamond pattern                                                                                                                                           |
| `DoubleDown`                                                                                  | -                                    | -    | 3         | Two rows down                                                                                                                                                       |
| `Down`                                                                                        | -                                    | -    | 1         | Single pixel below                                                                                                                                                  |
| `EqualFloydSteinberg`                                                                         | -                                    | -    | 4         | Equal weight distribution variant                                                                                                                                   |
| `FalseFloydSteinberg`                                                                         | -                                    | -    | 3         | Simplified 3-neighbor variant                                                                                                                                       |
| [`Fan93`](https://doi.org/10.1117/12.59413)                                                   | Z. Fan                               | 1992 | 4         | [SPIE'92](https://doi.org/10.1117/12.59413), "A Simple Modification of Error Diffusion Weights"                                                                     |
| [`FloydSteinberg`](https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering)           | R.W. Floyd, L. Steinberg             | 1976 | 4         | [Proc. SID vol. 17](https://steveomohundro.com/wp-content/uploads/2009/03/omohundro90_floyd_steinberg_dithering.pdf), "An Adaptive Algorithm for Spatial Greyscale" |
| `HorizontalDiamond`                                                                           | -                                    | -    | 6         | Diamond with horizontal bias                                                                                                                                        |
| [`Sierra`](https://en.wikipedia.org/wiki/Error_diffusion#Sierra_dithering)                    | Frankie Sierra                       | 1989 | 10        | Three-line filter, [King's Quest](https://en.wikipedia.org/wiki/King%27s_Quest) era                                                                                 |
| [`SierraLite`](https://en.wikipedia.org/wiki/Error_diffusion#Sierra_dithering)                | Frankie Sierra                       | 1990 | 3         | Filter Lite - minimal variant                                                                                                                                       |
| `Simple`                                                                                      | -                                    | -    | 1         | Single neighbor diffusion                                                                                                                                           |
| `TwoD`                                                                                        | -                                    | -    | 2         | Simple 2-neighbor                                                                                                                                                   |
| [`TwoRowSierra`](https://en.wikipedia.org/wiki/Error_diffusion#Sierra_dithering)              | Frankie Sierra                       | 1990 | 7         | Two-row variant                                                                                                                                                     |
| `VerticalDiamond`                                                                             | -                                    | -    | 8         | Diamond with vertical bias                                                                                                                                          |

### Ordered Dithering

Ordered dithering uses threshold matrices to determine pixel output. Unlike error diffusion, pixels can be processed independently (parallelizable).

| Ditherer                                                        | Author      | Year | Size  | Description                                                                                                      |
| --------------------------------------------------------------- | ----------- | ---- | ----- | ---------------------------------------------------------------------------------------------------------------- |
| [`Bayer2x2`](https://en.wikipedia.org/wiki/Ordered_dithering)   | Bryce Bayer | 1973 | 2x2   | Smallest [Bayer](https://en.wikipedia.org/wiki/Ordered_dithering#Pre-calculated_threshold_maps) threshold matrix |
| [`Bayer4x4`](https://en.wikipedia.org/wiki/Ordered_dithering)   | Bryce Bayer | 1973 | 4x4   | Standard [Bayer](https://en.wikipedia.org/wiki/Ordered_dithering#Pre-calculated_threshold_maps) threshold matrix |
| [`Bayer8x8`](https://en.wikipedia.org/wiki/Ordered_dithering)   | Bryce Bayer | 1973 | 8x8   | Large [Bayer](https://en.wikipedia.org/wiki/Ordered_dithering#Pre-calculated_threshold_maps) matrix (256 levels) |
| [`Bayer16x16`](https://en.wikipedia.org/wiki/Ordered_dithering) | Bryce Bayer | 1973 | 16x16 | Very large [Bayer](https://en.wikipedia.org/wiki/Ordered_dithering#Pre-calculated_threshold_maps) matrix         |
| [`ClusterDot4x4`](https://en.wikipedia.org/wiki/Halftone)       | -           | -    | 4x4   | Clustered dot pattern for smoother appearance                                                                    |
| [`ClusterDot8x8`](https://en.wikipedia.org/wiki/Halftone)       | -           | -    | 8x8   | Larger cluster dot pattern                                                                                       |
| `Diagonal4x4`                                                   | -           | -    | 4x4   | Diagonal line pattern                                                                                            |
| [`Halftone4x4`](https://en.wikipedia.org/wiki/Halftone)         | -           | -    | 4x4   | Simulates [halftone](https://en.wikipedia.org/wiki/Halftone) printing                                            |
| [`Halftone8x8`](https://en.wikipedia.org/wiki/Halftone)         | -           | -    | 8x8   | Larger halftone pattern                                                                                          |

```csharp
// Ordered dithering with Bayer matrix
var ditherer = OrderedDitherer.Bayer8x8;

// Adjust strength (0.0 - 1.0)
var reduced = OrderedDitherer.Bayer4x4.WithStrength(0.5f);
```

### Noise Dithering

Noise dithering adds random or pseudo-random thresholds before quantization. Can be processed in parallel.

| Ditherer                                                                                                                                   | Description                                                                                     |
| ------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------- |
| [`WhiteNoise`](https://en.wikipedia.org/wiki/White_noise)                                                                                  | Uniform random threshold with equal energy at all frequencies                                   |
| [`BlueNoise`](https://en.wikipedia.org/wiki/Colors_of_noise#Blue_noise)                                                                    | Spatially-filtered noise with reduced low-frequency content                                     |
| [`PinkNoise`](https://en.wikipedia.org/wiki/Pink_noise)                                                                                    | 1/f noise, equal energy per octave, more natural-looking                                        |
| [`BrownNoise`](https://en.wikipedia.org/wiki/Brownian_noise)                                                                               | 1/f^2 noise (Brownian motion), strong low-frequency, smooth organic                             |
| [`VioletNoise`](https://en.wikipedia.org/wiki/Colors_of_noise#Violet_noise)                                                                | f noise, high-frequency emphasis, sharp textured appearance                                     |
| [`GreyNoise`](https://en.wikipedia.org/wiki/Colors_of_noise#Gray_noise)                                                                    | Perceptually uniform noise adjusted for human vision response                                   |
| [`InterleavedGradientNoise`](https://blog.demofox.org/2022/01/01/interleaved-gradient-noise-a-different-kind-of-low-discrepancy-sequence/) | Deterministic pseudo-random noise for temporal AA. [Ref](https://www.shadertoy.com/view/4djSRW) |

```csharp
// Noise dithering
var ditherer = NoiseDitherer.BlueNoise;

// Other noise types
var pink = NoiseDitherer.PinkNoise;     // More natural than white
var brown = NoiseDitherer.BrownNoise;   // Smooth, organic appearance
var violet = NoiseDitherer.VioletNoise; // Sharp, textured
var grey = NoiseDitherer.GreyNoise;     // Perceptually uniform

// Adjust strength and seed
var custom = NoiseDitherer.WhiteNoise.WithStrength(0.8f).WithSeed(12345);
```

### Advanced Ditherers

High-quality ditherers for specialized applications with superior visual quality.

| Ditherer                                                                                   | Author              | Year  | Type           | Description                                                                                                                     |
| ------------------------------------------------------------------------------------------ | ------------------- | ----- | -------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| [`Yliluoma1`](https://bisqwit.iki.fi/story/howto/dither/jy/)                               | Joel Yliluoma       | 2011  | Pattern        | Position-dependent mixing patterns; [ref impl](https://bisqwit.iki.fi/story/howto/dither/jy/#_Appendix%201Algorithm1)           |
| [`Yliluoma2`](https://bisqwit.iki.fi/story/howto/dither/jy/)                               | Joel Yliluoma       | 2011  | Pattern        | Improved Yliluoma with candidate counting; [ref impl](https://bisqwit.iki.fi/story/howto/dither/jy/#_Appendix%202Algorithm2)    |
| [`Yliluoma3`](https://bisqwit.iki.fi/story/howto/dither/jy/)                               | Joel Yliluoma       | 2011  | Pattern        | Threshold-based mixing; [ref impl](https://bisqwit.iki.fi/story/howto/dither/jy/#_Appendix%203Algorithm3)                       |
| [`Yliluoma4`](https://bisqwit.iki.fi/story/howto/dither/jy/)                               | Joel Yliluoma       | 2011  | Pattern        | Balanced mixing strategy; [ref impl](https://bisqwit.iki.fi/story/howto/dither/jy/#_Appendix%204Algorithm4)                     |
| [`DbsDitherer`](https://doi.org/10.1145/127719.122734)                                     | Mitchel et al.      | 1987  | Iterative      | [Direct Binary Search](https://doi.org/10.1145/127719.122734) optimization                                                      |
| [`Riemersma`](https://www.compuphase.com/riemer.htm)                                       | Thiadmer Riemersma  | 1998  | Space-filling  | [Hilbert](https://en.wikipedia.org/wiki/Hilbert_curve)/[Peano](https://en.wikipedia.org/wiki/Peano_curve) curve error diffusion |
| [`Ostromoukhov`](https://perso.liris.cnrs.fr/victor.ostromoukhov/)                         | Victor Ostromoukhov | 2001  | Adaptive       | [Variable-coefficient error diffusion](https://doi.org/10.1145/383259.383326)                                                   |
| [`Knoll`](https://bisqwit.iki.fi/story/howto/dither/jy/#KnollDithering)                    | Thomas Knoll        | 1990s | Pattern        | [Adobe Photoshop](https://en.wikipedia.org/wiki/Adobe_Photoshop) pattern dithering                                              |
| `NClosestDitherer`                                                                         | -                   | -     | Pattern        | N-closest palette color mixing                                                                                                  |
| `NConvexDitherer`                                                                          | -                   | -     | Pattern        | Convex combination of palette colors                                                                                            |
| [`VoidAndClusterDitherer`](https://doi.org/10.1117/12.152707)                              | Robert Ulichney     | 1993  | Ordered        | [Blue noise](https://en.wikipedia.org/wiki/Colors_of_noise#Blue_noise) via void-and-cluster method                              |
| [`BarycentricDitherer`](https://en.wikipedia.org/wiki/Barycentric_coordinate_system)       | -                   | -     | Ordered        | Triangle-based 3-color interpolation with Bayer pattern                                                                         |
| [`TinDitherer`](https://en.wikipedia.org/wiki/Triangulated_irregular_network)              | -                   | -     | Ordered        | Tetrahedral 4-color interpolation with Bayer pattern                                                                            |
| [`NaturalNeighbourDitherer`](https://en.wikipedia.org/wiki/Natural_neighbor_interpolation) | -                   | -     | Ordered        | [Voronoi](https://en.wikipedia.org/wiki/Voronoi_diagram)-based area-weighted color interpolation                                |
| [`AverageDitherer`](https://www.graphicsacademy.com/what_dithera.php)                      | -                   | -     | Custom         | Uses local region averages as thresholds                                                                                        |
| `DizzyDitherer`                                                                            | -                   | -     | ErrorDiffusion | Spiral-based error distribution reducing directional artifacts                                                                  |

### Adaptive Ditherers

Ditherers that analyze local image content to adjust their behavior.

| Ditherer                 | Description                                                |
| ------------------------ | ---------------------------------------------------------- |
| `SmartDitherer`          | Automatically selects best ditherer based on local content |
| `AdaptiveDitherer`       | Adjusts error diffusion strength based on local contrast   |
| `AdaptiveMatrixDitherer` | Uses content-aware threshold matrices                      |
| `GradientAwareDitherer`  | Preserves gradients while dithering flat areas             |
| `StructureAwareDitherer` | Maintains structural features                              |
| `DebandingDitherer`      | Specifically designed to reduce banding artifacts          |

### Ditherer Configuration

Error diffusion ditherers support two scan modes via separate zero-cost types:
- `ErrorDiffusion` - Linear left-to-right scanning
- `ErrorDiffusionSerpentine` - Alternating direction per row (reduces directional artifacts)

```csharp
// Basic usage (linear scan)
var ditherer = ErrorDiffusion.FloydSteinberg;

// Serpentine scanning (returns ErrorDiffusionSerpentine type - zero-cost abstraction)
var serpentine = ErrorDiffusion.FloydSteinberg.Serpentine;

// Switch back to linear if needed
var linear = serpentine.Linear;

// Adjust strength (0.0 - 1.0)
var reduced = ErrorDiffusion.Atkinson.WithStrength(0.75f);

// Combine options (serpentine with reduced strength)
var custom = ErrorDiffusion.JarvisJudiceNinke.Serpentine.WithStrength(0.9f);
```

### Riemersma Ditherer (Space-Filling Curves)

The [Riemersma ditherer](https://www.compuphase.com/riemer.htm) uses space-filling curves to traverse the image, maintaining spatial locality for better error diffusion. [Interactive visualization](https://www.mathematik.ch/anwendungenmath/fractal/hilbert/)

| Curve Type                                             | Subdivision | Order Range | Coverage per Order |
| ------------------------------------------------------ | ----------- | ----------- | ------------------ |
| [Hilbert](https://en.wikipedia.org/wiki/Hilbert_curve) | 2x2         | 1-7         | 2ⁿ x 2ⁿ pixels   |
| [Peano](https://en.wikipedia.org/wiki/Peano_curve)     | 3x3         | 1-5         | 3ⁿ x 3ⁿ pixels   |
| Linear                                                 | -           | -           | Serpentine scan    |

```csharp
// Pre-configured instances
var hilbert = RiemersmaDitherer.Default;   // Hilbert curve, history size 16
var peano = RiemersmaDitherer.Peano;       // Peano curve, history size 16
var linear = RiemersmaDitherer.LinearScan; // Simple serpentine

// Different history sizes (affects error decay)
var small = RiemersmaDitherer.Small;  // History size 8 (faster)
var large = RiemersmaDitherer.Large;  // History size 32 (higher quality)

// Custom configuration with explicit curve type
var customHilbert = new RiemersmaDitherer(16, SpaceFillingCurve.Hilbert);
var customPeano = new RiemersmaDitherer(16, SpaceFillingCurve.Peano);

// Specify exact curve order (auto-calculated if omitted)
var hilbertOrder4 = new RiemersmaDitherer(16, SpaceFillingCurve.Hilbert, 4);  // 16x16 coverage
var peanoOrder3 = new RiemersmaDitherer(16, SpaceFillingCurve.Peano, 3);      // 27x27 coverage
```

---

## Image Scaling / Pixel Art Rescaling

The library provides a comprehensive collection of image scaling algorithms, from simple interpolation methods to sophisticated pixel art scalers and retro gaming effects.

### Upscaling Methods

```csharp
using var source = new Bitmap("sprite.png");

// Pixel art scalers
using var scaled2x = source.Upscale(Eagle.X2);
using var scaled3x = source.Upscale(SuperEagle.X2);
using var hq4x = source.Upscale(Hqnx.X4);

// Anti-aliased scaling
using var smaa = source.Upscale(Smaa.X2);

// Edge-preserving smoothing
using var bilateral = source.Upscale(Bilateral.X2);
```

### Available Scalers

#### Anti-Aliasing

| Scaler                                                                                                 | Author                   | Year | Scales     | Description                                                                                                                                                                                          |
| ------------------------------------------------------------------------------------------------------ | ------------------------ | ---- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`Smaa`](https://www.iryoku.com/smaa/)                                                                 | Jorge Jimenez et al.     | 2012 | 2x, 3x, 4x | Subpixel Morphological Anti-Aliasing - detects edges using luma gradients and applies weighted pixel blending. Presets: `X2`, `X3`, `X4` with `.Low`, `.Medium`, `.High`, `.Ultra` quality variants. |
| [`Fxaa`](https://en.wikipedia.org/wiki/Fast_approximate_anti-aliasing)                                 | Timothy Lottes/NVIDIA    | 2009 | 2x, 3x, 4x | Fast Approximate Anti-Aliasing using luma-based edge detection. [Ref](https://developer.download.nvidia.com/assets/gamedev/files/sdk/11/FXAA_WhitePaper.pdf)                                         |
| [`Mlaa`](https://en.wikipedia.org/wiki/Morphological_antialiasing)                                     | Alexander Reshetov       | 2009 | 2x, 3x, 4x | Morphological Anti-Aliasing using pattern detection (L, Z, U shapes). [Ref](https://software.intel.com/content/www/us/en/develop/articles/morphological-antialiasing.html)                           |
| [`ReverseAa`](https://github.com/libretro/common-shaders/tree/master/anti-aliasing/shaders/reverse-aa) | Christoph Feck / Hyllian | 2011 | 2x         | Reverse anti-aliasing using gradient-based tilt computation for smooth edges                                                                                                                         |

#### Edge-Preserving Filters

| Scaler                                                                                                     | Author            | Year | Scales     | Description                                                                                                                                                                                 |
| ---------------------------------------------------------------------------------------------------------- | ----------------- | ---- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`Bilateral`](https://homepages.inf.ed.ac.uk/rbf/CVonline/LOCAL_COPIES/MANDUCHI1/Bilateral_Filtering.html) | Tomasi & Manduchi | 1998 | 2x, 3x, 4x | Edge-preserving smoothing filter combining spatial and range weighting. Presets: `X2`, `X3`, `X4` with `.Soft` (sigma_s=2.0, sigma_r=0.3) and `.Sharp` (sigma_s=1.0, sigma_r=0.1) variants. |

#### Edge-Directed Interpolation

| Scaler                                                                                     | Author                | Year | Scales     | Description                                                                                                                                                            |
| ------------------------------------------------------------------------------------------ | --------------------- | ---- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`Nedi`](https://ieeexplore.ieee.org/document/941048)                                      | Xin Li & M.T. Orchard | 2001 | 2x, 3x, 4x | New Edge-Directed Interpolation using local autocorrelation for adaptive edge-aware upscaling. [Ref](https://www.academia.edu/8327337/New_Edge_Directed_Interpolation) |
| [`Nnedi3`](https://github.com/sekrit-twc/znedi3)                                           | tritical              | 2010 | 2x, 3x, 4x | Neural Network Edge Directed Interpolation using trained weights for high-quality edge-directed scaling                                                                |
| [`SuperXbr`](https://github.com/libretro/common-shaders/tree/master/xbr/shaders/super-xbr) | Hyllian               | 2015 | 2x         | Super-Scale2x Refinement with 2-pass edge-directed scaling and anti-ringing                                                                                            |
| `AnimeLineEnhancer`                                                                        | -                     | -    | 2x, 3x    | Gradient-based edge enhancement for anime/cartoon content with edge-directed sharpening                                                               |

#### Pixel Art Scalers

| Scaler                                                                                                | Author           | Year  | Scales     | Description                                                                                                       |
| ----------------------------------------------------------------------------------------------------- | ---------------- | ----- | ---------- | ----------------------------------------------------------------------------------------------------------------- |
| [`Eagle`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#Eagle)                           | -                | 1990s | 2x, 3x     | Classic pixel doubling with corner detection                                                                      |
| [`SuperEagle`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#2xSaI)                      | Kreed            | 2001  | 2x         | Enhanced Eagle with better diagonal handling                                                                      |
| [`Super2xSaI`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#2xSaI)                      | Kreed            | 1999  | 2x         | 2x Scale and Interpolation engine                                                                                 |
| [`Epx` / `Scale2x`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#EPX/Scale2x/AdvMAME2x) | Andrea Mazzoleni | 2001  | 2x, 3x     | Edge-preserving pixel expansion                                                                                   |
| [`Hqnx`](https://en.wikipedia.org/wiki/Hqx)                                                           | Maxim Stepin     | 2003  | 2x, 3x, 4x | High-quality magnification using YUV comparisons                                                                  |
| [`Lqnx`](https://en.wikipedia.org/wiki/Hqx)                                                           | -                | -     | 2x, 3x, 4x | Low-quality simplified variant of HQnx. [Ref](https://github.com/luckytyphlosion/vba-link/blob/master/src/lq2x.h) |
| [`Xbr`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#xBR_family)                        | Hyllian          | 2011  | 2x, 3x, 4x | xBR (scale By Rules) edge-detection scaler                                                                        |
| [`Xbrz`](https://sourceforge.net/projects/xbrz/)                                                      | Zenju            | 2012  | 2x-6x      | Enhanced xBR with improved edge handling                                                                          |
| [`Sal`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#2xSaI)                             | Kreed            | 2001  | 2x         | Simple Assembly Language scaler with anti-aliasing                                                                |
| [`Sabr`](https://github.com/libretro/common-shaders/tree/master/sabr)                       | Joshua Street    | 2012  | 2x-4x      | Scalable Bicubic Renderer with multi-angle (45/30/60) edge detection and smoothstep blending. Variants: `SabrSharp` (more edges), `SabrSmooth` (softer) |
| [`Mmpx`](https://casual-effects.com/research/McGuire2021PixelArt/)                                    | Morgan McGuire   | 2021  | 2x         | Modern AI-inspired pixel art scaling                                                                              |
| [`Omniscale`](https://github.com/libretro/common-shaders)                                             | libretro         | 2015  | 2x-6x      | Multi-method hybrid scaler                                                                                        |
| [`RotSprite`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#RotSprite)                   | Xenowhirl        | 2007  | 2x-4x      | Rotation-aware pixel art scaling                                                                                  |
| [`Clean`](https://github.com/libretro/common-shaders)                                                 | -                | -     | 2x, 3x     | Clean edge pixel art scaler                                                                                       |
| [`TriplePoint`](https://github.com/Hawkynt/2dimagefilter)                                             | Hawkynt          | 2011  | 2x, 3x     | 3x scaler using diagonal color analysis                                                                           |
| [`ScaleNxSfx`](https://github.com/libretro/common-shaders/tree/master/scalenx)                        | Sp00kyFox        | 2013  | 2x, 3x     | ScaleNx with effects (corner blending)                                                                            |
| [`ScaleNxPlus`](https://github.com/Hawkynt/2dimagefilter)                                             | Hawkynt          | 2011  | 2x, 3x     | Enhanced ScaleNx with better diagonals                                                                            |
| [`ScaleHq`](https://github.com/Hawkynt/2dimagefilter)                                                 | Hawkynt          | 2011  | 2x, 3x     | HQ-style pixel art scaler                                                                                         |
| [`EpxB`](https://www.snes9x.com/)                                                                     | SNES9x Team      | 2003  | 2x         | Enhanced EPX with complex edge detection                                                                          |
| [`EpxC`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#EPX/Scale2x/AdvMAME2x)            | -                | -     | 2x         | EPX variant with additional edge cases. [Ref](https://github.com/libretro/common-shaders)                         |
| [`Des`](https://www.reddit.com/r/emulation/wiki/index/)                                               | FNES Team        | 2000  | 1x         | Diagonal Edge Scaling filter (pre-processing)                                                                     |
| [`Des2`](https://www.reddit.com/r/emulation/wiki/index/)                                              | FNES Team        | 2000  | 2x         | DES with 2x scaling using edge-directed scaling                                                                   |
| [`Scl2x`](https://www.reddit.com/r/emulation/wiki/index/)                                             | FNES Team        | 2000  | 2x         | Simple corner-based line scaling                                                                                  |
| [`ScaleFx`](https://github.com/libretro/slang-shaders/tree/master/edge-smoothing/scalefx)             | Sp00kyFox        | 2014  | 3x         | Scale3x with enhanced edge detection                                                                              |
| [`Edge`](https://wiki.scummvm.org/index.php/Scalers)                                                  | ScummVM Team     | 2001  | 2x, 3x, 4x | Simple edge enhancement for smoother diagonals                                                                    |
| [`TwoXpm`](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms)                                | Pablo Medina     | -     | 2x         | High-quality 2x pixel-art scaler with edge morphing                                                               |
| [`Saa5050`](https://en.wikipedia.org/wiki/Mullard_SAA5050)                                            | Mullard          | 1980  | 2x3        | Teletext character smoothing (original hardware IC)                                                               |

#### Simple & Utility Scalers

| Scaler                                                                                                            | Author       | Scales     | Description                                                                                         |
| ----------------------------------------------------------------------------------------------------------------- | ------------ | ---------- | --------------------------------------------------------------------------------------------------- |
| [`NearestNeighbor`](https://en.wikipedia.org/wiki/Nearest-neighbor_interpolation)                                 | -            | Nx         | Simple pixel duplication                                                                            |
| [`Bilinear`](https://en.wikipedia.org/wiki/Bilinear_interpolation)                                                | -            | Nx         | Linear interpolation                                                                                |
| [`Bicubic`](https://en.wikipedia.org/wiki/Bicubic_interpolation)                                                  | -            | Nx         | Cubic spline interpolation                                                                          |
| [`Lanczos`](https://en.wikipedia.org/wiki/Lanczos_resampling)                                                     | -            | Nx         | Sinc-windowed interpolation                                                                         |
| [`Normal`](https://en.wikipedia.org/wiki/Nearest-neighbor_interpolation)                                          | -            | 2x, 3x, 4x | [Fixed-factor pixel duplication](https://github.com/libretro/common-shaders)                        |
| [`Pixellate`](https://en.wikipedia.org/wiki/Pixelization)                                                         | -            | Nx         | [Pixelation/mosaic effect](https://github.com/libretro/common-shaders/tree/master/misc)             |
| [`BilinearPlus`](https://vba-m.com/)                                                                              | VBA Team     | 2x         | VBA weighted bilinear interpolation (5:2:1 weighting)                                               |
| [`SharpBilinear`](https://github.com/libretro/common-shaders/blob/master/interpolation/shaders/sharp-bilinear.cg) | LibRetro     | 2x, 3x, 4x | Integer prescaling + bilinear for crisp pixels                                                      |
| [`Quilez`](https://iquilezles.org/articles/texture/)                                                              | Inigo Quilez | 2x, 3x, 4x | Quintic smoothstep interpolation for smooth gradients. [Ref](https://www.shadertoy.com/view/MllBWf) |
| [`NearestNeighborPlus`](https://github.com/libretro/common-shaders/tree/master/interpolation)                     | -            | 2x, 3x, 4x | Enhanced nearest neighbor with edge detection                                                       |
| [`Soft`](https://github.com/libretro/common-shaders/tree/master/interpolation)                                    | -            | 2x, 3x, 4x | Soft interpolation with gentle blending                                                             |
| [`SoftSmart`](https://github.com/libretro/common-shaders/tree/master/interpolation)                               | -            | 2x, 3x, 4x | Smart soft interpolation with adaptive blending                                                     |
| [`Cut`](https://github.com/libretro/common-shaders)                                                               | -            | 2x, 3x, 4x | Cut-based scaling utility                                                                           |
| [`Ddt`](https://github.com/libretro/common-shaders/tree/master/ddt)                                               | Sp00kyFox    | 2x         | Diagonal De-interpolation Technique                                                                 |
| [`CatmullRom`](https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline)                              | -            | 2x, 3x, 4x | Catmull-Rom spline interpolation (pixel-art variant)                                                |
| [`Aann`](https://github.com/libretro/glsl-shaders/tree/master/interpolation/shaders/aann)    | jimbo1qaz/wareya | 2x, 3x     | Anti-aliased nearest neighbor with gamma-corrected bilinear interpolation                                                                               |
| [`Nis`](https://github.com/NVIDIAGameWorks/NVIDIAImageScaling)                               | NVIDIA           | 2x, 3x     | Edge-adaptive spatial upscaler with directional sharpening                                                                                              |

#### Retro Display Effects

| Scaler                                                                                         | Author         | Scales     | Description                                                 |
| ---------------------------------------------------------------------------------------------- | -------------- | ---------- | ----------------------------------------------------------- |
| [`DotMatrix`](https://wiki.scummvm.org/index.php?title=Graphics_filtering)                     | ScummVM        | 2x, 3x, 4x | Dot-matrix display simulation with brightness falloff       |
| [`LcdGrid`](https://en.wikipedia.org/wiki/Subpixel_rendering)                                  | -              | 2x, 3x, 4x | LCD subpixel grid simulation                                |
| [`ScanlineHorizontal`](https://en.wikipedia.org/wiki/Scan_line)                                | Hawkynt        | 2x1        | CRT vertical scanline effect                                |
| [`ScanlineVertical`](https://en.wikipedia.org/wiki/Scan_line)                                  | Hawkynt        | 1x2        | CRT horizontal scanline effect                              |
| [`Tv2x` / `Tv3x` / `Tv4x`](https://wiki.scummvm.org/index.php?title=Graphics_filtering)        | -              | 2x, 3x, 4x | TV scanline simulation                                      |
| [`MameRgb`](https://www.mamedev.org/)                                                          | MAME Team      | 2x, 3x     | LCD RGB subpixel channel filter simulation                  |
| [`MameAdvInterp`](https://www.mamedev.org/)                                                    | MAME Team      | 2x, 3x     | MAME advanced interpolation with scanline effect            |
| [`HawkyntTv`](https://github.com/Hawkynt/2dimagefilter)                                        | Hawkynt        | 2x, 3x, 4x | TV effect with configurable scanline and phosphor patterns  |
| [`CrtEasymode`](https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-easymode) | -              | 2x, 3x, 4x | Lightweight CRT simulation with scanlines and phosphor mask |
| [`CrtHyllian`](https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-hyllian)   | Hyllian        | 2x, 3x, 4x | Sharp CRT scanlines with phosphor simulation                |
| [`CrtLottes`](https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-lottes)     | Timothy Lottes | 2x, 3x, 4x | CRT simulation with bloom and phosphor mask                 |
| [`ZfastCrt`](https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/zfast-crt)       | -              | 2x, 3x, 4x | Performance-optimized CRT simulation                        |
| [`GameBoyShader`](https://github.com/libretro/slang-shaders/tree/master/handheld)              | -              | 2x, 3x, 4x | Game Boy Color LCD simulation with pixel grid               |
| [`LcdGhosting`](https://github.com/libretro/slang-shaders/tree/master/handheld)                | -              | 2x, 3x, 4x | LCD response time blur simulation                           |
| [`Ntsc`](https://en.wikipedia.org/wiki/NTSC)                                                   | blargg         | 2x         | NTSC composite video color bleeding simulation              |
| [`CrtGeom`](https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-geom)      | libretro       | 2x, 3x     | CRT simulation with shadow mask and scanline darkening                                                                                                  |
| [`CrtCaligari`](https://github.com/libretro/slang-shaders/tree/master/crt/shaders)          | Caligari       | 2x, 3x     | Performance-focused CRT with electron beam spot simulation                                                                                              |
| [`CrtRoyale`](https://github.com/libretro/slang-shaders/tree/master/crt/shaders/crt-royale)  | TroggleMonkey  | 2x, 3x     | Advanced CRT with phosphor masks, bloom, and halation                                                                                                   |
| [`Gtu`](https://github.com/aliaspider/interpolation-shaders)                                 | Aliaspider     | 2x, 3x     | Gaussian TV upscaler with bandwidth simulation and scanlines                                                                                            |

### Downscaling

| Scaler         | Scales             | Description                                                     |
| -------------- | ------------------ | --------------------------------------------------------------- |
| `BoxDownscale` | 1/2, 1/3, 1/4, 1/5 | Box filter downscaling by averaging NxN blocks of source pixels |

### Scaler Configuration

Many scalers support configuration options:

```csharp
// SMAA quality levels
using var low = source.Upscale(Smaa.X2.Low);
using var ultra = source.Upscale(Smaa.X2.Ultra);

// Bilateral filter parameters
using var soft = source.Upscale(Bilateral.X2.Soft);    // sigma_s=2.0, sigma_r=0.3
using var sharp = source.Upscale(Bilateral.X2.Sharp);  // sigma_s=1.0, sigma_r=0.1
using var custom = source.Upscale(new Bilateral(3, 1.5f, 0.2f));

// Scanline brightness
using var scanlines = source.Upscale(new ScanlineHorizontal(brightness: 0.3f));
```

---

## Image Resamplers

High-quality interpolation filters for arbitrary scaling to any resolution. Unlike pixel art scalers that work at fixed integer factors, resamplers use mathematical kernels to compute pixel values at any scale.

### Resampling Methods

```csharp
using Hawkynt.ColorProcessing.Resizing.Resamplers;

// Generic type syntax (parameterless)
using var result = bitmap.Resample<Lanczos3>(newWidth, newHeight);
using var result = bitmap.Resample<MitchellNetravali>(newWidth, newHeight);

// Instance syntax (parameterized)
using var result = bitmap.Resample(new Bicubic(-0.75f), newWidth, newHeight);
using var result = bitmap.Resample(new Gaussian(sigma: 1.0f), newWidth, newHeight);
```

### Basic Filters

| Resampler                                                                         | Radius | Description                                                                                                    |
| --------------------------------------------------------------------------------- | ------ | -------------------------------------------------------------------------------------------------------------- |
| [`NearestNeighbor`](https://en.wikipedia.org/wiki/Nearest-neighbor_interpolation) | 1      | Point sampling, fastest. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)       |
| [`Bilinear`](https://en.wikipedia.org/wiki/Bilinear_interpolation)                | 1      | 2x2 weighted average. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)          |
| [`Box`](https://en.wikipedia.org/wiki/Box_blur)                                   | 1+     | Simple averaging. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)              |
| [`Hermite`](https://en.wikipedia.org/wiki/Cubic_Hermite_spline)                   | 1      | Smooth cubic polynomial. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)       |
| [`Cosine`](https://paulbourke.net/miscellaneous/interpolation/)                   | 1      | Cosine-weighted interpolation. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c) |
| [`Smoothstep`](https://en.wikipedia.org/wiki/Smoothstep)                          | 1      | Hermite polynomial S-curve interpolation. [Ref](https://www.shadertoy.com/view/MsS3zK)                         |

### Cubic Family

| Resampler                                                                                                        | Author              | Year | Radius | Parameters           | Reference                                                                                                  |
| ---------------------------------------------------------------------------------------------------------------- | ------------------- | ---- | ------ | -------------------- | ---------------------------------------------------------------------------------------------------------- |
| [`Bicubic`](https://en.wikipedia.org/wiki/Bicubic_interpolation)                                                 | Robert Keys         | 1981 | 2      | `a` (-0.5f)          | Keys coefficient. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)          |
| [`MitchellNetravali`](https://www.cs.utexas.edu/~fussell/courses/cs384g-fall2013/lectures/mitchell/Mitchell.pdf) | Mitchell, Netravali | 1988 | 2      | `b` (1/3), `c` (1/3) | Balanced cubic. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)            |
| [`CatmullRom`](https://en.wikipedia.org/wiki/Cubic_Hermite_spline#Catmull-Rom_spline)                            | Catmull, Rom        | 1974 | 2      | None                 | Sharp spline (B=0, C=0.5). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c) |
| [`Robidoux`](http://www.imagemagick.org/Usage/filter/#robidoux)                                                  | Nicolas Robidoux    | 2011 | 2      | None                 | Optimized for photos. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)      |
| [`RobidouxSharp`](http://www.imagemagick.org/Usage/filter/#robidoux)                                             | Nicolas Robidoux    | 2011 | 2      | None                 | Sharper variant. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)           |
| [`RobidouxSoft`](http://www.imagemagick.org/Usage/filter/#robidoux)                                              | Nicolas Robidoux    | 2011 | 2      | None                 | Smoother variant. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)          |

### Lanczos Family

| Resampler                                                      | Radius | Sharpness | Ringing     | Best For                                                                                              |
| -------------------------------------------------------------- | ------ | --------- | ----------- | ----------------------------------------------------------------------------------------------------- |
| [`Lanczos2`](https://en.wikipedia.org/wiki/Lanczos_resampling) | 2      | Good      | Low         | General use. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)          |
| [`Lanczos3`](https://en.wikipedia.org/wiki/Lanczos_resampling) | 3      | Very good | Moderate    | Photos (recommended). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c) |
| [`Lanczos4`](https://en.wikipedia.org/wiki/Lanczos_resampling) | 4      | Excellent | Higher      | Maximum sharpness. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)    |
| [`Lanczos5`](https://en.wikipedia.org/wiki/Lanczos_resampling) | 5      | Maximum   | Significant | Special cases. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)        |
| [`Jinc`](https://en.wikipedia.org/wiki/Sombrero_function)      | 3+     | Excellent | Moderate    | 2D (uses Bessel J1). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)  |

### B-Spline & O-MOMS

| Resampler                                                    | Degree | Radius | Prefilter | Description                                                                                       |
| ------------------------------------------------------------ | ------ | ------ | --------- | ------------------------------------------------------------------------------------------------- |
| [`BSpline`](https://en.wikipedia.org/wiki/B-spline)          | 3      | 2      | Required  | Cubic B-spline. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)   |
| [`BSpline2`](https://en.wikipedia.org/wiki/B-spline)         | 2      | 2      | Required  | Quadratic. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)        |
| [`BSpline4`](https://en.wikipedia.org/wiki/B-spline)         | 4      | 3      | Required  | Quartic (Parzen). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c) |
| [`BSpline5`](https://en.wikipedia.org/wiki/B-spline)         | 5      | 3      | Required  | Quintic. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)          |
| [`BSpline7`](https://en.wikipedia.org/wiki/B-spline)         | 7      | 4      | Required  | Septic. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)           |
| [`OMoms3`](https://bigwww.epfl.ch/publications/blu9901.html) | 3      | 2      | Required  | Optimal cubic. [Ref](https://bigwww.epfl.ch/thevenaz/interpolation/)                              |
| [`OMoms5`](https://bigwww.epfl.ch/publications/blu9901.html) | 5      | 3      | Required  | Optimal quintic. [Ref](https://bigwww.epfl.ch/thevenaz/interpolation/)                            |
| [`OMoms7`](https://bigwww.epfl.ch/publications/blu9901.html) | 7      | 4      | Required  | Optimal septic. [Ref](https://bigwww.epfl.ch/thevenaz/interpolation/)                             |

### Spline & Window Filters

| Resampler                                                                                | Radius | Used By     | Description                                                                                                         |
| ---------------------------------------------------------------------------------------- | ------ | ----------- | ------------------------------------------------------------------------------------------------------------------- |
| [`Spline16`](http://www.ipol.im/pub/art/2011/g_lmii/)                                    | 2      | VLC         | 4-tap spline. [Ref](https://github.com/FFmpeg/FFmpeg/blob/master/libswscale/utils.c)                                |
| [`Spline36`](http://www.ipol.im/pub/art/2011/g_lmii/)                                    | 3      | FFmpeg      | 6-tap spline. [Ref](https://github.com/FFmpeg/FFmpeg/blob/master/libswscale/utils.c)                                |
| [`Spline64`](http://www.ipol.im/pub/art/2011/g_lmii/)                                    | 4      | ImageMagick | 8-tap spline. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)                       |
| [`Blackman`](https://en.wikipedia.org/wiki/Window_function#Blackman_window)              | 3+     | -           | Very low sidelobes. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)                 |
| [`Kaiser`](https://en.wikipedia.org/wiki/Kaiser_window)                                  | 3+     | -           | Adjustable via beta parameter. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)      |
| [`Hann`](https://en.wikipedia.org/wiki/Hann_function)                                    | 3+     | -           | Raised cosine. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)                      |
| [`Hamming`](https://en.wikipedia.org/wiki/Window_function#Hann_and_Hamming_windows)      | 3+     | -           | Modified Hann. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)                      |
| [`Welch`](https://en.wikipedia.org/wiki/Window_function#Welch_window)                    | 3+     | -           | Parabolic window. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)                   |
| [`Bartlett`](https://en.wikipedia.org/wiki/Window_function#Triangular_window)            | 3+     | -           | Triangular window. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)                  |
| [`Bohman`](https://en.wikipedia.org/wiki/Window_function#Bohman_window)                  | 3+     | -           | Cosine-convolved window. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)            |
| [`Nuttal`](https://en.wikipedia.org/wiki/Window_function#Nuttall_window)                 | 3+     | -           | 4-term Blackman-Harris variant. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)     |
| [`BlackmanNuttal`](https://en.wikipedia.org/wiki/Window_function#Nuttall_window)         | 3+     | -           | Blackman-Nuttal hybrid. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)             |
| [`BlackmanHarris`](https://en.wikipedia.org/wiki/Window_function#Blackman-Harris_window) | 3+     | -           | 4-term Blackman-Harris. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)             |
| [`FlatTop`](https://en.wikipedia.org/wiki/Window_function#Flat_top_window)               | 3+     | -           | Very flat passband. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)                 |
| [`Tukey`](https://en.wikipedia.org/wiki/Window_function#Tukey_window)                    | 3+     | -           | Tapered cosine window. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)              |
| [`Poisson`](https://en.wikipedia.org/wiki/Window_function#Poisson_window)                | 3+     | -           | Exponential decay window. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)           |
| [`BartlettHann`](https://en.wikipedia.org/wiki/Window_function#Bartlett-Hann_window)     | 3+     | -           | Bartlett-Hann hybrid. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)               |
| [`Cauchy`](https://en.wikipedia.org/wiki/Cauchy_distribution)                            | 3+     | -           | Cauchy/Lorentz distribution window. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c) |
| [`Schaum2`](https://bigwww.epfl.ch/publications/thevenaz9901.pdf)                        | 2      | -           | Quadratic Schaum interpolation. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)     |
| [`Schaum3`](https://bigwww.epfl.ch/publications/thevenaz9901.pdf)                        | 3      | -           | Cubic Schaum interpolation. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)         |

### Lagrange Polynomial Interpolation

| Resampler                                                        | Degree | Radius | Description                                                                                        |
| ---------------------------------------------------------------- | ------ | ------ | -------------------------------------------------------------------------------------------------- |
| [`Lagrange3`](https://en.wikipedia.org/wiki/Lagrange_polynomial) | 3      | 2      | Cubic (4-point). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)   |
| [`Lagrange5`](https://en.wikipedia.org/wiki/Lagrange_polynomial) | 5      | 3      | Quintic (6-point). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c) |
| [`Lagrange7`](https://en.wikipedia.org/wiki/Lagrange_polynomial) | 7      | 4      | Septic (8-point). [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)  |

> **Note:** Lagrange interpolation passes exactly through all sample points, making it suitable for applications
> requiring exact value preservation. No prefiltering is required.

### Edge-Directed & Modern Upscalers

| Resampler                                                                                              | Author           | Year | Radius | Parameters                               | Description                                                                                                  |
| ------------------------------------------------------------------------------------------------------ | ---------------- | ---- | ------ | ---------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| [`Dcci`](https://ieeexplore.ieee.org/document/941048)                                                  | Li, Orchard      | 2001 | 2      | `cubicA`, `coherenceThreshold`           | Edge-directed interpolation. [Ref](https://github.com/HomeOfVapourSynthEvolution/VapourSynth-DCTFilter)      |
| [`Fsr`](https://gpuopen.com/fidelityfx-superresolution/)                                               | AMD              | 2021 | 2      | `sharpness` (0.5f)                       | FidelityFX Super Resolution. [Ref](https://github.com/GPUOpen-Effects/FidelityFX-FSR)                        |
| [`Ravu`](https://github.com/bjin/mpv-prescalers)                                                       | -                | 2017 | 2      | `sharpness`, `antiRinging`               | Robust Adaptive Video Upscaling. [Ref](https://github.com/bjin/mpv-prescalers)                               |
| [`Eedi2`](https://github.com/Asd-g/AviSynth-EEDI2)                                                     | tritical         | 2005 | 2      | -                                        | Enhanced Edge-Directed Interp. [Ref](https://github.com/HomeOfVapourSynthEvolution/VapourSynth-EEDI2)        |
| [`KrigBilateral`](https://en.wikipedia.org/wiki/Kriging)                                               | -                | -    | 3      | `sigma`, `radius`                        | Kriging-based bilateral upscaling. [Ref](https://github.com/igv/KrigBilateral)                               |
| [`KopfLischinski`](https://johanneskopf.de/publications/pixelart/)                                     | Kopf, Lischinski | 2011 | 2      | -                                        | Depixelizing pixel art (SIGGRAPH 2011). [Ref](https://johanneskopf.de/publications/pixelart/paper/pixel.pdf) |
| [`Icbi`](https://ieeexplore.ieee.org/document/977589)                                                  | Li, Orchard      | 2001 | 2      | `coherenceThreshold`, `correctionFactor` | Curvature-based interpolation with structure tensor analysis                                                 |
| [`Bedi`](https://en.wikipedia.org/wiki/Bilinear_interpolation)                                         | -                | 2010 | 2      | `edgeThreshold` (30f)                    | Bilinear edge-directed interpolation with Sobel gradient classification                                      |
| [`AdvancedAa`](https://github.com/libretro/glsl-shaders/tree/master/anti-aliasing/shaders/advanced-aa) | guest(r)         | 2006 | 2      | -                                        | Edge-weighted anti-aliasing interpolation                                                                    |

### Content-Aware Resizers

| Resampler                                                   | Author         | Year | Description                                                       |
| ----------------------------------------------------------- | -------------- | ---- | ----------------------------------------------------------------- |
| [`SeamCarving`](https://en.wikipedia.org/wiki/Seam_carving) | Avidan, Shamir | 2007 | Content-aware resizing by inserting/removing minimum-energy seams |

SeamCarving supports three energy calculation modes via `SeamCarvingEnergyMode`:
- `Gradient` - Simple gradient magnitude using absolute color differences (fastest)
- `Sobel` - Sobel operator for gradient magnitude (balanced)
- `Forward` - Forward energy considers insertion cost (best quality, slowest)

```csharp
// Content-aware resize (removes/adds low-energy seams)
using var result = bitmap.Resample(new SeamCarving(SeamCarvingEnergyMode.Sobel), newWidth, newHeight);
```

### Specialized Filters

| Resampler                                                     | Author           | Radius | Parameters               | Description                                                                                                   |
| ------------------------------------------------------------- | ---------------- | ------ | ------------------------ | ------------------------------------------------------------------------------------------------------------- |
| [`Gaussian`](https://en.wikipedia.org/wiki/Gaussian_blur)     | Gauss            | 2+     | `sigma` (0.5f), `radius` | Gaussian blur/interpolation. [Ref](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c)  |
| [`NoHalo`](https://gegl.org/operations/gegl-scale-ratio.html) | Nicolas Robidoux | 3      | None                     | Minimizes halo artifacts. [Ref](https://gitlab.gnome.org/GNOME/gegl/-/blob/master/operations/common/nohalo.c) |
| [`LoHalo`](https://gegl.org/operations/gegl-scale-ratio.html) | Nicolas Robidoux | 3      | None                     | Low-halo variant. [Ref](https://gitlab.gnome.org/GNOME/gegl/-/blob/master/operations/common/lohalo.c)         |
| [`MagicKernelSharp`](https://johncostella.com/magic/mks.pdf)  | John Costella    | 2      | `sharpness`              | 4-tap max sharpness kernel. [Ref](https://johncostella.com/magic/mks.pdf)                                     |

### Resampler Parameters

| Resampler             | Parameter            | Type    | Default    | Range   | Description                                     |
| --------------------- | -------------------- | ------- | ---------- | ------- | ----------------------------------------------- |
| **Bicubic**           | `a`                  | `float` | `-0.5f`    | -1 to 0 | Keys coefficient (-0.5=standard, -0.75=sharper) |
| **MitchellNetravali** | `b`                  | `float` | `0.333f`   | 0-1     | Blur parameter                                  |
|                       | `c`                  | `float` | `0.333f`   | 0-1     | Ringing parameter                               |
| **Kaiser**            | `radius`             | `int`   | `3`        | 1-10    | Filter radius                                   |
|                       | `beta`               | `float` | `8.6f`     | 0-20    | Shape parameter                                 |
| **Gaussian**          | `sigma`              | `float` | `0.5f`     | 0.1-5   | Standard deviation                              |
| **Fsr**               | `sharpness`          | `float` | `0.5f`     | 0-1     | Sharpness level                                 |
| **Ravu**              | `sharpness`          | `float` | `0.5f`     | 0-1     | Sharpness level                                 |
|                       | `antiRinging`        | `float` | `0.5f`     | 0-1     | Anti-ringing strength                           |
| **Dcci**              | `cubicA`             | `float` | `-0.5f`    | -1 to 0 | Cubic coefficient                               |
|                       | `coherenceThreshold` | `float` | `0.3f`     | 0-1     | Edge detection threshold                        |
| **Icbi**              | `coherenceThreshold` | `float` | `0.3f`     | 0-1     | Edge coherence threshold                        |
|                       | `correctionFactor`   | `float` | `0.2f`     | 0-1     | Curvature correction strength                   |
| **Bedi**              | `edgeThreshold`      | `float` | `30f`      | 0-255   | Edge detection threshold (lower = more edges)   |
| **SeamCarving**       | `energyMode`         | `enum`  | `Gradient` | -       | Energy calculation mode                         |

### Resampler Usage Examples

```csharp
// High-quality photo scaling
using var photo = bitmap.Resample<Lanczos3>(newWidth, newHeight);

// Balanced quality (recommended for most uses)
using var balanced = bitmap.Resample<MitchellNetravali>(newWidth, newHeight);

// Sharp results with custom bicubic
using var sharp = bitmap.Resample(new Bicubic(-0.75f), newWidth, newHeight);

// Edge-preserving upscaling
using var edges = bitmap.Resample(Dcci.Sharp, newWidth, newHeight);

// Modern AI-like upscaling
using var fsr = bitmap.Resample(Fsr.Sharp, newWidth, newHeight);

// Halo-free scaling
using var nohalo = bitmap.Resample<NoHalo>(newWidth, newHeight);

// Smooth Gaussian
using var smooth = bitmap.Resample(new Gaussian(1.0f), newWidth, newHeight);

// Depixelize pixel art
using var depixelized = bitmap.Resample<KopfLischinski>(newWidth, newHeight);

// Content-aware resize
using var contentAware = bitmap.Resample(new SeamCarving(SeamCarvingEnergyMode.Forward), newWidth, newHeight);
```

### Algorithm Coverage

This library provides comprehensive coverage of resampling algorithms from major image processing libraries:

| Source Library                                                                          | Algorithms Covered                                                                                                                                                                                 | Notes         |
| --------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- |
| [ImageMagick](https://github.com/ImageMagick/ImageMagick/blob/main/MagickCore/resize.c) | Point, Box, Triangle, Hermite, Cubic, Catrom, Mitchell, Lanczos, Spline, Gaussian, Kaiser, Blackman, Hann, Hamming, Jinc, Robidoux variants, Parzen (BSpline4), Lagrange, and all window functions | Full coverage |
| [FFmpeg libswscale](https://github.com/FFmpeg/FFmpeg/blob/master/libswscale/utils.c)    | Point, Bilinear, Bicubic, Lanczos, Spline16, Spline36, Gaussian, Sinc, Area                                                                                                                        | Full coverage |
| [GEGL](https://gegl.org/)                                                               | NoHalo, LoHalo                                                                                                                                                                                     | Full coverage |
| [bisqwit.iki.fi](https://bisqwit.iki.fi/story/howto/dither/jy/)                         | Yliluoma 1-4, Knoll pattern dithering                                                                                                                                                              | Full coverage |

---

## Runtime Discovery

The library provides registry classes for runtime enumeration of all available algorithms. These are useful for building UIs, benchmarking, or dynamically selecting algorithms.

### ScalerRegistry

Discovers all pixel art scalers and resamplers at runtime.

```csharp
using Hawkynt.ColorProcessing.Resizing;

// List all available scalers
foreach (var scaler in ScalerRegistry.PixelScalers)
  Console.WriteLine($"{scaler.Name} ({scaler.Scale})");

// List all resamplers
foreach (var resampler in ScalerRegistry.Resamplers)
  Console.WriteLine($"{resampler.Name}");
```

### QuantizerRegistry

Discovers all quantization algorithms at runtime.

```csharp
using Hawkynt.ColorProcessing.Quantization;

// List all available quantizers
foreach (var quantizer in QuantizerRegistry.All)
  Console.WriteLine($"{quantizer.Name} ({quantizer.Type})");

// Find by name
var octree = QuantizerRegistry.FindByName("Octree");

// Create an instance
var instance = QuantizerRegistry.All.First(q => q.Name == "Octree").CreateDefault();
```

### DithererRegistry

Discovers all dithering algorithms and presets at runtime.

```csharp
using Hawkynt.ColorProcessing.Dithering;

// List all available ditherers
foreach (var ditherer in DithererRegistry.All)
  Console.WriteLine($"{ditherer.Name} ({ditherer.Type})");

// Find specific ditherer
var floydSteinberg = DithererRegistry.FindByName("ErrorDiffusion_FloydSteinberg");

// Filter by type
var errorDiffusion = DithererRegistry.GetByType(DitheringType.ErrorDiffusion);
```

---

## Image Filters

The library provides a pixel filter infrastructure for 1:1 image transformations that don't change dimensions. Filters use the same high-performance kernel pipeline as scalers but produce same-size output.

### Filter Methods

```csharp
using Hawkynt.ColorProcessing.Filtering.Filters;
using Hawkynt.Drawing;

// Color corrections
using var bright = source.ApplyFilter(new Brightness(0.2f));
using var adjusted = source.ApplyFilter(new BrightnessContrast(0.1f, 0.3f));
using var warm = source.ApplyFilter(new ColorTemperature(0.5f));
using var corrected = source.ApplyFilter(new Gamma(2.2f));
using var inverted = source.ApplyFilter(Invert.Default);
using var vintage = source.ApplyFilter(new Sepia(0.8f));
using var poster = source.ApplyFilter(new Posterize(4));

// HSL adjustments
using var saturated = source.ApplyFilter(new HueSaturation(0f, 0.3f, 0f));
using var vibrant = source.ApplyFilter(new Vibrance(0.5f));

// Enhancement
using var sharp = source.ApplyFilter(Sharpen.Default);
using var blurred = source.ApplyFilter(GaussianBlur.Default);
using var blurred5x5 = source.ApplyFilter(new GaussianBlur(2, 2));
using var blurredLarge = source.ApplyFilter(new GaussianBlur(10, 10));
using var denoised = source.ApplyFilter(MedianFilter.Default);
using var denoisedLarge = source.ApplyFilter(new MedianFilter(5));
using var sharpened = source.ApplyFilter(new UnsharpMask(1.5f, 0.05f));
using var sharpenedLarge = source.ApplyFilter(new UnsharpMask(1.5f, 0.05f, 5, 5));
using var smoothed = source.ApplyFilter(new BilateralFilter(3, 3f, 0.1f));
using var dilated = source.ApplyFilter(new Dilate(3));
using var eroded = source.ApplyFilter(new Erode(3));

// Edge detection & analysis
using var edges = source.ApplyFilter(SobelEdge.Default);
using var binary = source.ApplyFilter(new Threshold(0.5f));
using var red = source.ApplyFilter(new ChannelExtraction(ColorChannel.Red));
using var morphEdges = source.ApplyFilter(new MorphologicalGradient(2));

// Artistic
using var duo = source.ApplyFilter(Duotone.Default);
using var falseCol = source.ApplyFilter(FalseColor.Default);
using var boxBlurred = source.ApplyFilter(new BoxBlur(5, 5));
using var painterly = source.ApplyFilter(new Kuwahara(3));
using var vignetted = source.ApplyFilter(new Vignette(0.5f, 0.75f));
using var chromatic = source.ApplyFilter(new ChromaticAberration(2f));
using var nightVis = source.ApplyFilter(NightVision.Default);
using var thermal = source.ApplyFilter(new Thermal(0.8f));
using var oilPaint = source.ApplyFilter(new OilPainting(3, 20));
using var sketch = source.ApplyFilter(new PencilSketch(0.8f, 1));
using var waterCol = source.ApplyFilter(new Watercolor(2, 6));
using var pixelated = source.ApplyFilter(new Pixelate(8));
using var halftoned = source.ApplyFilter(new Halftone(6, 45f));
using var selective = source.ApplyFilter(new SelectiveDesaturation(0f, 30f, 1f));
using var bloomed = source.ApplyFilter(new Bloom(0.7f, 1f, 3));
using var glowing = source.ApplyFilter(new GlowingEdges(0.8f, 2f));
using var cartoon = source.ApplyFilter(new Cartoon(6, 0.1f, 1));
using var neon = source.ApplyFilter(new Neon(1f, 2f));

// Distortion
using var twirled = source.ApplyFilter(new Twirl(45f, 0.5f));
using var rippled = source.ApplyFilter(new Ripple(5f, 20f, 0f));
using var spherized = source.ApplyFilter(new Spherize(0.5f));
using var frosted = source.ApplyFilter(FrostedGlass.Default);
using var waved = source.ApplyFilter(new Wave(5f, 30f, 5f, 30f));

// Noise
using var noisy = source.ApplyFilter(new AddNoise(0.1f));
using var despeckled = source.ApplyFilter(Despeckle.Default);
using var denoised2 = source.ApplyFilter(new ReduceNoise(0.5f, 2));

// High-quality mode uses OkLab perceptual color space
using var hqSharp = source.ApplyFilter(Sharpen.Default, ScalerQuality.HighQuality);
```

### Available Filters

#### Color Correction

| Filter | Description |
| ------ | ----------- |
| `BleachBypass` | Film look: desaturated, high-contrast, muted colors simulating skipped bleach step |
| `Brightness` | Adjust brightness by adding offset to RGB channels |
| `BrightnessContrast` | Combined brightness and contrast adjustment |
| `ChannelMixer` | Recombine R/G/B channels with arbitrary 3x3 coefficient matrix |
| `ColorBalance` | Adjust color balance independently in shadows, midtones, and highlights (9 parameters) |
| `ColorTemperature` | Adjust color temperature (warm/cool shift) |
| `ColorTint` | Adjust green-magenta color tint |
| `Contrast` | Adjust contrast by scaling around midpoint |
| `CrossProcess` | Simulates cross-processed film (E6 in C41 chemistry) with shifted curves and boosted saturation |
| `Exposure` | Adjust exposure in stops (power-of-two scaling) |
| `Gamma` | Apply gamma correction curve |
| `Grayscale` | Convert to grayscale via luminance |
| `HDRToneMap` | Reinhard tone mapping operator for compressing high dynamic range |
| `HueSaturation` | Adjust hue, saturation, and lightness in HSL space |
| `Invert` | Invert all color channels |
| `Levels` | Input/output level remapping with midtone gamma |
| `Posterize` | Reduce color levels per channel |
| `SelectiveDesaturation` | Desaturates all colors except a chosen hue range (grayscale with conditional coloring) |
| `Sepia` | Apply sepia tone using standard color matrix |
| `Solarize` | Invert channels above threshold |
| `Vibrance` | Smart saturation boost targeting desaturated pixels |
| `VonKries` | Chromatic adaptation via Bradford transform (white point correction) |
| `AutoLevels` | Local auto-levels contrast stretching based on neighborhood min/max (always uses frame access) |
| `Equalize` | Local histogram equalization mapping via CDF in neighborhood (always uses frame access) |
| `SigmoidContrast` | S-curve sigmoid contrast adjustment per channel (always uses frame access) |

#### Enhancement

| Filter | Description |
| ------ | ----------- |
| `BilateralFilter` | Edge-preserving bilateral smoothing (spatial + range Gaussian weighting; always uses frame access) |
| `Blur` | 3x3 weighted blur |
| `Clarity` | Local contrast enhancement targeting midtones (always uses frame access) |
| `Dehaze` | Remove atmospheric haze by estimating and subtracting minimum channel (always uses frame access) |
| `Dilate` | Morphological dilation (max luminance) with configurable radius (uses frame access for radius > 2) |
| `Emboss` | 3x3 emboss convolution with gray bias |
| `Erode` | Morphological erosion (min luminance) with configurable radius (uses frame access for radius > 2) |
| `GaussianBlur` | Gaussian blur with arbitrary kernel size (radiusX/radiusY; uses frame access for radius > 2) |
| `HighPass` | High-pass filter with arbitrary blur radius (original minus low-pass + 0.5 bias; uses frame access for radius > 2) |
| `MedianFilter` | Median filter for noise reduction with configurable radius (uses frame access for radius > 2) |
| `MotionBlur` | Directional motion blur along a configurable angle and length (always uses frame access) |
| `RadialBlur` | Radial blur emanating from a configurable center point (always uses frame access) |
| `Sharpen` | 3x3 unsharp mask sharpening |
| `SpinBlur` | Circular/rotational blur around a configurable center point (always uses frame access) |
| `SurfaceBlur` | Edge-preserving blur that only averages neighbors within a color threshold (always uses frame access) |
| `UnsharpMask` | Unsharp mask sharpening with threshold and arbitrary blur radius (uses frame access for radius > 2) |
| `ZoomBlur` | Zoom/radial blur along the direction from center through each pixel (always uses frame access) |
| `BokehBlur` | Polygonal disc-shaped blur with bright spot emphasis (always uses frame access) |
| `SmartBlur` | Edge-preserving blur with hard color difference threshold (always uses frame access) |
| `SmartSharpen` | Deconvolution-style sharpening with edge magnitude threshold (always uses frame access) |

#### Analysis

| Filter | Description |
| ------ | ----------- |
| `AccentedEdges` | Sobel edges with brightness and smoothness modulation (always uses frame access) |
| `Channel Extraction` | Extract single color channel as grayscale |
| `LaplacianEdge` | Laplacian edge detection |
| `MorphologicalGradient` | Edge detection via morphological gradient (dilate - erode; uses frame access for radius > 2) |
| `PrewittEdge` | Prewitt edge detection (equal-weight gradient) |
| `SobelEdge` | Sobel edge detection (gradient magnitude) |
| `Threshold` | Binary threshold based on luminance |
| `TraceContour` | Contour tracing that outputs white where gradient crosses a threshold (always uses frame access) |
| `CannyEdge` | Canny edge detection with double threshold hysteresis (always uses frame access) |
| `FindEdges` | Simple Sobel edge detection showing white edges on black background (always uses frame access) |

#### Artistic

| Filter | Description |
| ------ | ----------- |
| `Bloom` | Glow around bright areas with threshold and radial glow (always uses frame access) |
| `BoxBlur` | Uniform box blur with configurable kernel size (radiusX/radiusY; uses frame access for radius > 2) |
| `Cartoon` | Color quantization with edge darkening for cartoon/cel-shaded appearance (always uses frame access) |
| `ChromaticAberration` | Simulates lens chromatic aberration with radial RGB channel shifting (always uses frame access) |
| `ColoredPencil` | Colored pencil sketch preserving hue via HSL edge modulation (always uses frame access) |
| `ColorHalftone` | Per-CMYK-channel halftone dots at different screen angles (always uses frame access) |
| `Crosshatch` | Multi-angle line hatching where line density varies with luminance (always uses frame access) |
| `Crystallize` | Grid-based Voronoi cell tessellation with seed-point coloring (always uses frame access) |
| `Cutout` | Simplified paper-cutout effect via smoothing and posterization (always uses frame access) |
| `DiffuseGlow` | Bright area glow with optional grain for dreamy effect (always uses frame access) |
| `DryBrush` | Simplified oil-painting with configurable brush size and levels (always uses frame access) |
| `Duotone` | Map luminance to shadow/highlight color gradient |
| `Engrave` | Engraving-style line density that varies with luminance (always uses frame access) |
| `FalseColor` | Map luminance to three-stop color gradient |
| `Fragment` | Average of multiple rotated/offset copies for ghosting effect (always uses frame access) |
| `GlowingEdges` | Neon-colored edges on dark background (always uses frame access) |
| `Grain` | Deterministic photographic film grain based on position hash (always uses frame access) |
| `Halftone` | Print-style circular dot pattern simulating newspaper printing (always uses frame access) |
| `InkOutlines` | Multiply original color by edge-darkness factor for ink outline appearance (always uses frame access) |
| `Kuwahara` | Painterly filter using quadrant variance analysis (always uses frame access) |
| `LensFlare` | Procedural lens flare with radial glow and ray pattern (always uses frame access) |
| `LightingEffects` | Phong-like directional shading using luminance as height map (always uses frame access) |
| `Mosaic` | Tile mosaic with dark grout lines between blocks (always uses frame access) |
| `Neon` | Neon glow edges with color spread and additive blending (always uses frame access) |
| `NightVision` | Green monochrome with amplified brightness simulating night-vision goggles |
| `OilPainting` | Quantized luminance-binned color averaging for painterly effect (always uses frame access) |
| `OldPhoto` | Combined vintage: sepia tone + reduced contrast + warm color shift |
| `PaletteKnife` | Directional smoothing following gradient for palette knife strokes (always uses frame access) |
| `PencilSketch` | Edge detection inverted and blended with grayscale for sketch appearance (always uses frame access) |
| `Pixelate` | Mosaic effect replacing blocks of pixels with their average color (always uses frame access) |
| `Pointillize` | Circular dot pattern on paper background simulating pointillism (always uses frame access) |
| `PosterEdges` | Color posterization combined with edge darkening (always uses frame access) |
| `Relief` | 3D relief effect with directional light source from luminance heightmap |
| `StainedGlass` | Voronoi cell tessellation with dark borders between cells (always uses frame access) |
| `Thermal` | False-color thermal/infrared mapping via 5-stop color ramp |
| `TiltShift` | Position-dependent selective blur with sharp focus band (always uses frame access) |
| `Vignette` | Radial vignette darkening effect (always uses frame access) |
| `Watercolor` | Watercolor painting simulation with color quantization and soft edges (always uses frame access) |
| `AngledStrokes` | Directional brush strokes at configurable angle (always uses frame access) |
| `BasRelief` | Bas-relief sculptural shading from directional edges (always uses frame access) |
| `ChalkAndCharcoal` | Chalk and charcoal sketch effect on gray paper (always uses frame access) |
| `Charcoal` | Charcoal sketch using edge detection and threshold (always uses frame access) |
| `ConteCrayon` | Conte crayon with foreground/background color mapping (always uses frame access) |
| `Craquelure` | Surface crack texture overlay with noise displacement (always uses frame access) |
| `DarkStrokes` | Dark stroke painting emphasizing shadows (always uses frame access) |
| `Facet` | Weighted median color by similarity in neighborhood (always uses frame access) |
| `Fresco` | Fresco painting with textured surface noise (always uses frame access) |
| `GlassTile` | Glass tile distortion with lens bulge effect per tile (always uses frame access) |
| `GraphicPen` | Graphic pen with directional edge strokes (always uses frame access) |
| `HalftonePattern` | Ordered dither halftone pattern (dot/line/cross types) (always uses frame access) |
| `NotePaper` | Note paper sketch with emboss relief and grain (always uses frame access) |
| `Patchwork` | Patchwork quilt effect with emboss-style relief edges (always uses frame access) |
| `Photocopy` | Photocopy effect with adjustable detail and darkness (always uses frame access) |
| `PlasticWrap` | Plastic wrap effect with specular highlights from edges (always uses frame access) |
| `Reticulation` | Film grain reticulation modulated by luminance (always uses frame access) |
| `RoughPastels` | Rough pastel strokes with canvas texture overlay (always uses frame access) |
| `SmudgeStick` | Smudge stick painting with highlight brightening (always uses frame access) |
| `SoftGlow` | Soft glow via screen blend of blurred highlights (always uses frame access) |
| `Spatter` | Paint spatter with hash-based displacement and smoothing (always uses frame access) |
| `Sponge` | Sponge texture with high-contrast luminance bins (always uses frame access) |
| `Stamp` | Rubber stamp effect with luminance threshold (always uses frame access) |
| `Texturizer` | Procedural texture overlay (brick/burlap/canvas/sandstone) with relief (always uses frame access) |
| `Tiles` | Offset tile blocks with random displacement and dark gaps (always uses frame access) |
| `TornEdges` | Stamp effect with noisy displaced edges (always uses frame access) |
| `Underpainting` | Underpainting with large brush oil-painting and texture (always uses frame access) |
| `WaterPaper` | Water paper texture with directional blur and fiber grain (always uses frame access) |

#### Distortion

| Filter | Description |
| ------ | ----------- |
| `FrostedGlass` | Random pixel displacement via positional hash for frosted glass effect (always uses frame access) |
| `LensDistortion` | Brown-Conrady barrel/pincushion lens distortion model (always uses frame access) |
| `Pinch` | Radial compress/expand within configurable radius (always uses frame access) |
| `PolarCoordinates` | Convert between rectangular and polar coordinate systems (always uses frame access) |
| `Ripple` | Sinusoidal displacement perpendicular to a configurable angle (always uses frame access) |
| `Spherize` | Spherical refraction distortion (always uses frame access) |
| `Spread` | Random neighbor displacement for scatter effect (always uses frame access) |
| `Turbulence` | Fractal noise displacement using value noise with octave summation (always uses frame access) |
| `Twirl` | Spiral rotation decreasing from center to edge (always uses frame access) |
| `Wave` | Independent X/Y sinusoidal displacement with configurable amplitude and wavelength (always uses frame access) |
| `Wind` | Directional pixel streaks at bright edges simulating wind (always uses frame access) |
| `Bulge` | Radial bulge/pinch power-law distortion from center (always uses frame access) |
| `Dents` | Noise-based surface dent displacement distortion (always uses frame access) |
| `OceanRipple` | Dual-axis sinusoidal ocean ripple distortion (always uses frame access) |
| `Offset` | Wrap-around pixel offset displacement (always uses frame access) |
| `Shear` | Linear horizontal or vertical shear displacement (always uses frame access) |
| `ZigZag` | Radial zigzag sinusoidal displacement from center (always uses frame access) |

#### Noise

| Filter | Description |
| ------ | ----------- |
| `AddNoise` | Deterministic positional hash noise, monochromatic or per-channel (always uses frame access) |
| `Despeckle` | Hybrid median filter combining horizontal, vertical, and diagonal medians (always uses frame access) |
| `DustAndScratches` | Replace outlier pixels with neighborhood median (always uses frame access) |
| `Mezzotint` | Random dot pattern where density matches luminance (always uses frame access) |
| `ReduceNoise` | Non-local means approximation weighting neighbors by color similarity (always uses frame access) |
| `Diffuse` | Random positional hash displacement noise (always uses frame access) |

#### Render

| Filter | Description |
| ------ | ----------- |
| `Clouds` | Procedural multi-octave value noise clouds blended with source (always uses frame access) |
| `DifferenceClouds` | Difference blend of procedural noise with source image (always uses frame access) |
| `Fibers` | Vertical fiber texture with noise modulation blended with source (always uses frame access) |
| `PlasmaTexture` | Multi-octave plasma noise with per-channel phase offset (always uses frame access) |
| `Supernova` | Radial starburst light rays with spoke pattern added to source (always uses frame access) |

#### Morphology

| Filter | Description |
| ------ | ----------- |
| `BottomHat` | Black bottom-hat: closing minus original highlights dark details (always uses frame access) |
| `Closing` | Morphological closing: dilation then erosion (always uses frame access) |
| `Opening` | Morphological opening: erosion then dilation (always uses frame access) |
| `TopHat` | White top-hat: original minus opening highlights bright details (always uses frame access) |

### Filter Discovery

```csharp
using Hawkynt.ColorProcessing.Filtering;

// Enumerate all available filters
foreach (var filter in FilterRegistry.All)
  Console.WriteLine($"{filter.Name} ({filter.Category})");

// Find by name or category
var vonKries = FilterRegistry.FindByName("VonKries");
var analysisFilters = FilterRegistry.GetByCategory(FilterCategory.Analysis);

// Apply via descriptor (reflection-based, useful for UI/tooling)
var desc = FilterRegistry.FindByName("Blur");
using var result = desc.Apply(source);
```

---

## Blend Modes

The library provides 27 blend modes for compositing bitmaps, covering all standard categories from image editing applications. Blend modes operate on normalized [0,1] float channel values and support alpha compositing with adjustable strength.

### Usage

```csharp
using Hawkynt.ColorProcessing.Blending.BlendModes;
using Hawkynt.Drawing;

using var background = new Bitmap("base.png");
using var overlay = new Bitmap("overlay.png");

// Blend with specific mode — returns new bitmap
using var multiplied = background.BlendWith<Multiply>(overlay);
using var screened = background.BlendWith<Screen>(overlay, 0.5f);  // 50% strength

// In-place blending — modifies background directly
background.BlendInto<Overlay>(overlay);

// Blend filter result with adjustable opacity
using var softGlow = source.BlendWith(new GaussianBlur(3, 3), 0.4f);  // Normal blend
using var sharpened = source.BlendWith<Screen, UnsharpMask>(new UnsharpMask(1.5f, 0.05f), 0.7f);

// HSL component modes
using var recolored = background.BlendWith<HueBlend>(overlay);
using var luminosity = background.BlendWith<LuminosityBlend>(overlay);
```

### Available Blend Modes

#### Normal

| Mode | Description |
| ---- | ----------- |
| `Normal` | Replaces background with foreground |

#### Darken

| Mode | Description |
| ---- | ----------- |
| `Multiply` | Multiplies channels, darkening the image |
| `ColorBurn` | Darkens by dividing inverted background by foreground |
| `Darken` | Takes the darker of two channels |
| `LinearBurn` | Adds channels and subtracts 1, darkening the image |

#### Lighten

| Mode | Description |
| ---- | ----------- |
| `Screen` | Inverts, multiplies, and inverts again, lightening the image |
| `ColorDodge` | Brightens by dividing background by inverted foreground |
| `Lighten` | Takes the lighter of two channels |
| `Add` | Adds channels, clamping at 1 |
| `LinearDodge` | Adds channels, clamping at 1 (alias of Add) |

#### Contrast

| Mode | Description |
| ---- | ----------- |
| `Overlay` | Combines Multiply and Screen based on background luminance |
| `SoftLight` | Soft contrast adjustment using W3C formula |
| `HardLight` | Overlay with foreground and background swapped |
| `VividLight` | Combines Color Burn and Color Dodge based on foreground |
| `LinearLight` | Combines Linear Burn and Linear Dodge based on foreground |
| `PinLight` | Replaces dark or light halves based on foreground |
| `HardMix` | Posterizes to black or white based on channel sum |

#### Inversion

| Mode | Description |
| ---- | ----------- |
| `Difference` | Absolute difference between channels |
| `Exclusion` | Similar to Difference but with lower contrast |
| `Subtract` | Subtracts foreground from background, clamping at 0 |
| `Divide` | Divides background by foreground |

#### Component (HSL-based)

| Mode | Description |
| ---- | ----------- |
| `HueBlend` | Takes hue from foreground, saturation and lightness from background |
| `SaturationBlend` | Takes saturation from foreground, hue and lightness from background |
| `ColorBlend` | Takes hue and saturation from foreground, lightness from background |
| `LuminosityBlend` | Takes lightness from foreground, hue and saturation from background |

#### Other

| Mode | Description |
| ---- | ----------- |
| `GrainExtract` | Extracts film grain texture from the image |
| `GrainMerge` | Merges film grain texture back into the image |

### Blend Mode Discovery

```csharp
using Hawkynt.ColorProcessing.Blending;

// Enumerate all available blend modes
foreach (var mode in BlendModeRegistry.All)
  Console.WriteLine($"{mode.Name} ({mode.Category})");

// Find by name or category
var multiply = BlendModeRegistry.FindByName("Multiply");
var darkenModes = BlendModeRegistry.GetByCategory(BlendModeCategory.Darken);
```

---

## Frequency Domain

The library provides FFT (Fast Fourier Transform) and DCT (Discrete Cosine Transform) operations for frequency-domain image analysis and manipulation.

### Types

| Type | Description |
| ---- | ----------- |
| `Complex` | Lightweight complex number (`float Real, float Imaginary`) with arithmetic operators, magnitude, phase, conjugate, and polar conversion |
| `Fft1D` | 1D FFT using Cooley-Tukey radix-2 algorithm (in-place, power-of-2 length) |
| `Fft2D` | 2D FFT via row-wise then column-wise 1D FFT |
| `Dct1D` | 1D Discrete Cosine Transform (Type-II forward, Type-III inverse) |
| `Dct2D` | 2D DCT via row-wise then column-wise 1D DCT |

### Bitmap Extension Methods

```csharp
using Hawkynt.ColorProcessing.FrequencyDomain;
using Hawkynt.Drawing;

// FFT operations
var spectrum = source.ToFrequencyDomain();           // Bitmap → Complex[,] (grayscale FFT)
using var reconstructed = BitmapFrequencyDomainExtensions
    .FromFrequencyDomain(spectrum, width, height);   // Complex[,] → Bitmap

// Spectrum visualization
using var magnitude = source.GetMagnitudeSpectrum(); // Log-scaled magnitude (zero-freq centered)
using var phase = source.GetPhaseSpectrum();         // Phase spectrum visualization

// DCT operations
var coefficients = source.ToDctDomain();             // Bitmap → float[,] (grayscale DCT)
using var fromDct = BitmapFrequencyDomainExtensions
    .FromDctDomain(coefficients, width, height);     // float[,] → Bitmap
```

---

## Known Issues

- **Bedi resampler**: Produces out-of-range color values on some inputs, causing `IndexOutOfRangeException` in the sRGB gamma LUT. Tests skip gracefully when this occurs.

---

## Installation

```bash
dotnet add package FrameworkExtensions.System.Drawing
```

### Supported Target Frameworks

| Framework      | Version                      |
| -------------- | ---------------------------- |
| .NET Framework | 3.5, 4.0, 4.5, 4.8           |
| .NET Core      | 3.1                          |
| .NET           | 6.0, 7.0, 8.0, 9.0 (Windows) |

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
