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

| Type                        | Components | Description                        |
| --------------------------- | ---------- | ---------------------------------- |
| `Cmyk` / `CmykNormalized`   | C, M, Y, K | Subtractive printing model         |
| `Din99` / `Din99Normalized` | L, a, b    | DIN 6176 perceptual space          |
| `Hsl` / `HslNormalized`     | H, S, L    | Hue, Saturation, Lightness         |
| `Hsv` / `HsvNormalized`     | H, S, V    | Hue, Saturation, Value             |
| `Hwb` / `HwbNormalized`     | H, W, B    | Hue, Whiteness, Blackness          |
| `Lab` / `LabNormalized`     | L, a, b    | CIE L*a*b\* perceptual color space |
| `Rgb` / `RgbNormalized`     | R, G, B    | Standard RGB color model           |
| `Xyz` / `XyzNormalized`     | X, Y, Z    | CIE XYZ tristimulus values         |
| `YCbCr` / `YCbCrNormalized` | Y, Cb, Cr  | Digital video color encoding       |
| `Yuv` / `YuvNormalized`     | Y, U, V    | Luma + chrominance (PAL/NTSC)      |

### Distance Calculators (`System.Drawing.ColorSpaces.Distances`)

| Calculator                     | Color Space     | Description                      |
| ------------------------------ | --------------- | -------------------------------- |
| `ChebyshevDistance<T>`         | Any 3-component | max(\|Δc1\|, \|Δc2\|, \|Δc3\|)   |
| `CIE76Distance`                | Lab             | ΔE\*ab (basic Lab distance)      |
| `CIE94Distance`                | Lab             | Improved perceptual formula      |
| `CIEDE2000Distance`            | Lab             | Most accurate perceptual ΔE      |
| `CMCAcceptabilityDistance`     | Lab             | Textile acceptability (l=2, c=1) |
| `CMCDistance`                  | Lab             | Textile industry (l=1, c=1)      |
| `DIN99Distance`                | DIN99           | German standard DIN 6176         |
| `EuclideanDistance<T>`         | Any 3-component | √(Δc1² + Δc2² + Δc3²)            |
| `EuclideanDistance4<T>`        | Any 4-component | For CMYK and similar             |
| `ManhattanDistance<T>`         | Any 3-component | \|Δc1\| + \|Δc2\| + \|Δc3\|      |
| `RedmeanDistance`              | RGB             | Compuserve algorithm             |
| `WeightedEuclideanRgbDistance` | RGB             | Perception-weighted RGB          |

All calculators also have `*Squared` variants for faster comparisons.

### Palette Lookup

```csharp
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Working;

// Convert palette to working color space
LinearRgbaF[] workPalette = palette.Select(c => /* decode to LinearRgbaF */).ToArray();

// Create lookup with metric (auto-caches results)
var lookup = new PaletteLookup<LinearRgbaF, EuclideanSquared4F<LinearRgbaF>>(
    workPalette,
    default);

// Find nearest palette color
int index = lookup.FindNearest(targetColor);
LinearRgbaF nearest = lookup.FindNearestColor(targetColor);

// Repeated lookups are O(1) due to built-in caching
foreach (var pixel in imagePixels) {
  var paletteIndex = lookup.FindNearest(pixel);  // Cached after first lookup
}
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

Quantizers reduce the number of colors in an image to generate optimized palettes.

### Available Quantizers

| Quantizer                                                                                                    | Author                      | Year | Type       | Description                                                                                        |
| ------------------------------------------------------------------------------------------------------------ | --------------------------- | ---- | ---------- | -------------------------------------------------------------------------------------------------- |
| [`KMeansQuantizer`](https://en.wikipedia.org/wiki/K-means_clustering)                                        | J. MacQueen                 | 1967 | Clustering | K-means++ iterative clustering, "Some Methods for Classification and Analysis"                     |
| [`MedianCutQuantizer`](https://dl.acm.org/doi/10.1145/965145.801294)                                         | Paul Heckbert               | 1982 | Splitting  | "Color Image Quantization for Frame Buffer Display", SIGGRAPH '82, pp. 297-307                     |
| [`NeuquantQuantizer`](https://scientificgems.wordpress.com/stuff/neuquant-fast-high-quality-image-quantization/) | Anthony Dekker              | 1994 | Neural     | "Kohonen Neural Networks for Optimal Colour Quantization", Network: Computing, vol. 73, pp. 351-367 |
| [`OctreeQuantizer`](https://www.cubic.org/docs/octree.htm)                                                   | M. Gervautz, W. Purgathofer | 1988 | Tree       | "A Simple Method for Color Quantization: Octree Quantization", Graphics Gems, pp. 219-231          |
| `PopularityQuantizer`                                                                                        | -                           | -    | Fixed      | Selects most frequently occurring colors from histogram                                            |
| `UniformQuantizer`                                                                                           | -                           | -    | Fixed      | Divides RGB color space into uniform grid                                                          |
| [`WuQuantizer`](http://www.ece.mcmaster.ca/~xwu/cq.c)                                                        | Xiaolin Wu                  | 1991 | Splitting  | "Efficient Statistical Computations for Optimal Color Quantization", Graphics Gems II, pp. 126-133 |

```csharp
// Generate palette from colors
var quantizer = new WuQuantizer();
Bgra8888[] palette = quantizer.GeneratePalette(colors, 16);

// Generate from histogram (weighted by frequency)
var histogram = new List<(Bgra8888 color, uint count)> { ... };
Bgra8888[] palette = quantizer.GeneratePalette(histogram, 256);
```

### K-Means Color Metrics

The `KMeansQuantizer` supports any `IColorMetric<Bgra8888>` for clustering. See [Color Metrics](#color-metrics) for all available metrics.

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

### Available Ditherers

| Ditherer                                                                              | Author                               | Year | Neighbors | Reference                                                                                                                                           |
| ------------------------------------------------------------------------------------- | ------------------------------------ | ---- | --------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`Atkinson`](https://en.wikipedia.org/wiki/Atkinson_dithering)                        | Bill Atkinson                        | 1984 | 6         | Apple Macintosh MacPaint, 75% error diffusion                                                                                                       |
| [`JarvisJudiceNinke`](<https://doi.org/10.1016/S0146-664X(76)80003-2>)                | J.F. Jarvis, C.N. Judice, W.H. Ninke | 1976 | 12        | "A Survey of Techniques for the Display of Continuous Tone Pictures on Bilevel Displays", Computer Graphics and Image Processing, vol. 5, pp. 13-40 |
| [`Pigeon`](https://hbfs.wordpress.com/2013/12/31/dithering/)                          | Steven Pigeon                        | 2013 | 7         | Blog post with analysis                                                                                                                             |
| [`ShiauFan`](https://patents.google.com/patent/US5353127A)                            | J.N. Shiau, Z. Fan                   | 1993 | 4         | "Set of Symmetrical Halftone Dot Patterns for Error Diffusion", US Patent 5,353,127                                                                 |
| [`ShiauFan2`](https://patents.google.com/patent/US5353127A)                           | J.N. Shiau, Z. Fan                   | 1993 | 5         | Extended Shiau-Fan variant, US Patent 5,353,127                                                                                                     |
| [`StevensonArce`](https://opg.optica.org/josaa/abstract.cfm?uri=josaa-2-7-1009)       | R.L. Stevenson, G.R. Arce            | 1985 | 12        | "Binary Display of Hexagonally Sampled Continuous-Tone Images", J. Optical Society of America A, vol. 2, no. 7, pp. 1009-1013                       |
| [`Stucki`](https://dominoweb.draco.res.ibm.com/1319c04d395da62c85257568004f2ab3.html) | P. Stucki                            | 1981 | 12        | "MECCA - A Multiple-Error Correcting Computation Algorithm for Bi-Level Image Hardcopy Reproduction", IBM Research Report RZ1060, Zurich            |
| `Burkes`                                                                              | D. Burkes                            | 1988 | 7         | "Presentation of the Burkes error filter", CIS Graphics Support Forum, LIB 15 (unpublished)                                                         |
| `Diagonal`                                                                            | -                                    | -    | 1         | Single diagonal neighbor                                                                                                                            |
| `Diamond`                                                                             | -                                    | -    | 8         | Symmetric diamond pattern                                                                                                                           |
| `DoubleDown`                                                                          | -                                    | -    | 3         | Two rows down                                                                                                                                       |
| `Down`                                                                                | -                                    | -    | 1         | Single pixel below                                                                                                                                  |
| `EqualFloydSteinberg`                                                                 | -                                    | -    | 4         | Equal weight distribution variant                                                                                                                   |
| `FalseFloydSteinberg`                                                                 | -                                    | -    | 3         | Simplified 3-neighbor variant                                                                                                                       |
| `Fan93`                                                                               | Z. Fan                               | 1992 | 4         | "A Simple Modification of Error Diffusion Weights", SPIE'92                                                                                         |
| `FloydSteinberg`                                                                      | R.W. Floyd, L. Steinberg             | 1976 | 4         | "An Adaptive Algorithm for Spatial Greyscale", Proc. SID, vol. 17, no. 2, pp. 75-77                                                                 |
| `HorizontalDiamond`                                                                   | -                                    | -    | 6         | Diamond with horizontal bias                                                                                                                        |
| `Sierra`                                                                              | Frankie Sierra                       | 1989 | 10        | Three-line filter, King's Quest era                                                                                                                 |
| `SierraLite`                                                                          | Frankie Sierra                       | 1990 | 3         | Filter Lite - minimal variant                                                                                                                       |
| `Simple`                                                                              | -                                    | -    | 1         | Single neighbor diffusion                                                                                                                           |
| `TwoD`                                                                                | -                                    | -    | 2         | Simple 2-neighbor                                                                                                                                   |
| `TwoRowSierra`                                                                        | Frankie Sierra                       | 1990 | 7         | Two-row variant                                                                                                                                     |
| `VerticalDiamond`                                                                     | -                                    | -    | 8         | Diamond with vertical bias                                                                                                                          |

### Ordered Dithering

Ordered dithering uses threshold matrices to determine pixel output. Unlike error diffusion, pixels can be processed independently (parallelizable).

| Ditherer                                                                   | Author      | Year | Size  | Description                                            |
| -------------------------------------------------------------------------- | ----------- | ---- | ----- | ------------------------------------------------------ |
| [`Bayer2x2`](https://en.wikipedia.org/wiki/Ordered_dithering)              | Bryce Bayer | 1973 | 2×2   | Smallest Bayer threshold matrix                        |
| [`Bayer4x4`](https://en.wikipedia.org/wiki/Ordered_dithering)              | Bryce Bayer | 1973 | 4×4   | Standard Bayer threshold matrix                        |
| [`Bayer8x8`](https://en.wikipedia.org/wiki/Ordered_dithering)              | Bryce Bayer | 1973 | 8×8   | Large Bayer matrix with more gradation levels          |
| [`Bayer16x16`](https://en.wikipedia.org/wiki/Ordered_dithering)            | Bryce Bayer | 1973 | 16×16 | Very large Bayer matrix for high quality               |
| `ClusterDot4x4`                                                            | -           | -    | 4×4   | Clustered dot pattern for smoother appearance          |
| `ClusterDot8x8`                                                            | -           | -    | 8×8   | Larger cluster dot pattern                             |
| `Diagonal4x4`                                                              | -           | -    | 4×4   | Diagonal line pattern                                  |
| `Halftone4x4`                                                              | -           | -    | 4×4   | Simulates halftone printing pattern                    |
| `Halftone8x8`                                                              | -           | -    | 8×8   | Larger halftone pattern                                |

```csharp
// Ordered dithering with Bayer matrix
var ditherer = OrderedDitherer.Bayer8x8;

// Adjust strength (0.0 - 1.0)
var reduced = OrderedDitherer.Bayer4x4.WithStrength(0.5f);
```

### Noise Dithering

Noise dithering adds random or pseudo-random thresholds before quantization. Can be processed in parallel.

| Ditherer      | Description                                                          |
| ------------- | -------------------------------------------------------------------- |
| `WhiteNoise`  | Uniform random threshold with equal energy at all frequencies        |
| `BlueNoise`   | Spatially-filtered noise with reduced low-frequency content          |
| `PinkNoise`   | 1/f noise, equal energy per octave, more natural-looking             |
| `BrownNoise`  | 1/f² noise (Brownian motion), strong low-frequency, smooth organic   |
| `VioletNoise` | f noise, high-frequency emphasis, sharp textured appearance          |
| `GreyNoise`   | Perceptually uniform noise adjusted for human vision response        |

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

### Ditherer Configuration

```csharp
// Basic usage
var ditherer = ErrorDiffusion.FloydSteinberg;

// Enable serpentine scanning (reduces artifacts)
var serpentine = ErrorDiffusion.FloydSteinberg.Serpentine;

// Adjust strength (0.0 - 1.0)
var reduced = ErrorDiffusion.Atkinson.WithStrength(0.75f);

// Combine options
var custom = ErrorDiffusion.JarvisJudiceNinke.Serpentine.WithStrength(0.9f);
```

---

## Installation

```bash
dotnet add package FrameworkExtensions.System.Drawing
```

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
