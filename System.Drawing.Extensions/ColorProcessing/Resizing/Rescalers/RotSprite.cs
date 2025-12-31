#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

#region RotSprite

/// <summary>
/// RotSprite - pixel art scaling with rotation support (2x, 3x, 4x).
/// </summary>
/// <remarks>
/// <para>Uses modified Scale2x with color similarity instead of exact matching.</para>
/// <para>Designed for high-quality pixel art scaling with optional rotation.</para>
/// <para>Reference: Xenowhirl, 2007</para>
/// </remarks>
[ScalerInfo("RotSprite", Author = "Xenowhirl", Year = 2007,
  Description = "RotSprite pixel art scaling with rotation support", Category = ScalerCategory.PixelArt)]
public readonly struct RotSprite : IPixelScaler {
  private readonly int _scale;
  private readonly float _rotationRadians;

  /// <summary>
  /// Creates a new RotSprite scaler with the specified scale factor and rotation.
  /// </summary>
  /// <param name="scale">The scale factor (2, 3, or 4).</param>
  /// <param name="rotationDegrees">The rotation angle in degrees (0-360).</param>
  public RotSprite(int scale = 2, float rotationDegrees = 0f) {
    if (scale is not (2 or 3 or 4))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "RotSprite supports 2x, 3x, 4x scaling");
    this._scale = scale;
    this._rotationRadians = rotationDegrees * (MathF.PI / 180f);
  }

  /// <summary>
  /// Gets or sets the rotation angle in degrees (0-360).
  /// </summary>
  public float RotationDegrees => this._rotationRadians * (180f / MathF.PI);

  /// <summary>
  /// Gets the rotation angle in radians.
  /// </summary>
  public float RotationRadians => this._rotationRadians;

  /// <summary>
  /// Creates a new RotSprite with the specified rotation angle.
  /// </summary>
  /// <param name="degrees">The rotation angle in degrees.</param>
  /// <returns>A new RotSprite with the specified rotation.</returns>
  public RotSprite WithRotation(float degrees) => new(this._scale, degrees);

  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._scale switch {
      0 or 2 => callback.Invoke(new RotSprite2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp, this._rotationRadians)),
      3 => callback.Invoke(new RotSprite3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp, this._rotationRadians)),
      4 => callback.Invoke(new RotSprite4xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp, this._rotationRadians)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  public static RotSprite Scale2x => new(2);
  public static RotSprite Scale3x => new(3);
  public static RotSprite Scale4x => new(4);
  public static RotSprite Default => new(2);
}

#endregion

#region RotSprite 2x Kernel

file readonly struct RotSprite2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default, float rotationRadians = 0f)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  // Precompute sin/cos to avoid trig in hot loop
  private readonly float _cos = MathF.Cos(rotationRadians);
  private readonly float _sin = MathF.Sin(rotationRadians);

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // Get rotation-adjusted neighbor sampling
    // For rotation, we rotate which neighbors correspond to which directions
    var cos = this._cos;
    var sin = this._sin;

    // Get base neighbors - we'll use rotated sampling for direction detection
    var c4 = window.P0P0; // center (always center)

    // Rotate cardinal direction sampling based on rotation angle
    // Each quadrant maps to different physical neighbors
    var rotatedN = this._GetRotatedNeighbor(window, 0, -1, cos, sin, lerp);
    var rotatedE = this._GetRotatedNeighbor(window, 1, 0, cos, sin, lerp);
    var rotatedS = this._GetRotatedNeighbor(window, 0, 1, cos, sin, lerp);
    var rotatedW = this._GetRotatedNeighbor(window, -1, 0, cos, sin, lerp);

    var w4 = c4.Work;

    var e00 = w4;
    var e01 = w4;
    var e10 = w4;
    var e11 = w4;

    // Modified Scale2x with rotated sampling
    // Check if diagonals are different (edge detection)
    var diagDiff1 = !equality.Equals(rotatedW.Key, rotatedE.Key);
    var diagDiff2 = !equality.Equals(rotatedN.Key, rotatedS.Key);

    if (diagDiff1 && diagDiff2) {
      // Top-left: N matches W
      if (equality.Equals(rotatedN.Key, rotatedW.Key))
        e00 = lerp.Lerp(rotatedN.Work, rotatedW.Work);

      // Top-right: N matches E
      if (equality.Equals(rotatedN.Key, rotatedE.Key))
        e01 = lerp.Lerp(rotatedN.Work, rotatedE.Work);

      // Bottom-left: S matches W
      if (equality.Equals(rotatedS.Key, rotatedW.Key))
        e10 = lerp.Lerp(rotatedS.Work, rotatedW.Work);

      // Bottom-right: S matches E
      if (equality.Equals(rotatedS.Key, rotatedE.Key))
        e11 = lerp.Lerp(rotatedS.Work, rotatedE.Work);
    }

    // Apply rotation to output positions
    this._WriteRotatedOutput(dest, destStride, encoder, e00, e01, e10, e11, cos, sin, lerp);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private NeighborPixel<TWork, TKey> _GetRotatedNeighbor(
    in NeighborWindow<TWork, TKey> window,
    int dx, int dy,
    float cos, float sin,
    TLerp lerp) {
    // Apply inverse rotation to find source position
    var srcX = dx * cos + dy * sin;
    var srcY = -dx * sin + dy * cos;

    // Round to nearest neighbor
    var nx = (int)MathF.Round(srcX);
    var ny = (int)MathF.Round(srcY);

    // Clamp to valid neighbor range
    nx = nx < -1 ? -1 : nx > 1 ? 1 : nx;
    ny = ny < -1 ? -1 : ny > 1 ? 1 : ny;

    // Map to NeighborWindow property (row=Y, col=X)
    return (ny, nx) switch {
      (-1, -1) => window.M1M1,
      (-1,  0) => window.M1P0,
      (-1,  1) => window.M1P1,
      ( 0, -1) => window.P0M1,
      ( 0,  0) => window.P0P0,
      ( 0,  1) => window.P0P1,
      ( 1, -1) => window.P1M1,
      ( 1,  0) => window.P1P0,
      ( 1,  1) => window.P1P1,
      _ => window.P0P0
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe void _WriteRotatedOutput(
    TPixel* dest, int destStride, in TEncode encoder,
    TWork e00, TWork e01, TWork e10, TWork e11,
    float cos, float sin, TLerp lerp) {
    // For small rotations, output directly
    // For larger rotations, rotate the output positions
    var absAngle = MathF.Abs(rotationRadians);
    if (absAngle < 0.01f) {
      // No significant rotation - write normally
      dest[0] = encoder.Encode(e00);
      dest[1] = encoder.Encode(e01);
      dest[destStride] = encoder.Encode(e10);
      dest[destStride + 1] = encoder.Encode(e11);
    } else {
      // Apply rotation by swapping output positions based on rotation quadrant
      var quadrant = (int)(rotationRadians / (MathF.PI / 2)) % 4;
      if (quadrant < 0) quadrant += 4;

      switch (quadrant) {
        case 0: // 0-90°
          dest[0] = encoder.Encode(e00);
          dest[1] = encoder.Encode(e01);
          dest[destStride] = encoder.Encode(e10);
          dest[destStride + 1] = encoder.Encode(e11);
          break;
        case 1: // 90-180°
          dest[0] = encoder.Encode(e01);
          dest[1] = encoder.Encode(e11);
          dest[destStride] = encoder.Encode(e00);
          dest[destStride + 1] = encoder.Encode(e10);
          break;
        case 2: // 180-270°
          dest[0] = encoder.Encode(e11);
          dest[1] = encoder.Encode(e10);
          dest[destStride] = encoder.Encode(e01);
          dest[destStride + 1] = encoder.Encode(e00);
          break;
        case 3: // 270-360°
          dest[0] = encoder.Encode(e10);
          dest[1] = encoder.Encode(e00);
          dest[destStride] = encoder.Encode(e11);
          dest[destStride + 1] = encoder.Encode(e01);
          break;
      }
    }
  }
}

#endregion

#region RotSprite 3x Kernel

file readonly struct RotSprite3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default, float rotationRadians = 0f)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  // Precompute sin/cos to avoid trig in hot loop
  private readonly float _cos = MathF.Cos(rotationRadians);
  private readonly float _sin = MathF.Sin(rotationRadians);

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var cos = this._cos;
    var sin = this._sin;

    var c4 = window.P0P0; // center
    var rotatedN = this._GetRotatedNeighbor(window, 0, -1, cos, sin);
    var rotatedE = this._GetRotatedNeighbor(window, 1, 0, cos, sin);
    var rotatedS = this._GetRotatedNeighbor(window, 0, 1, cos, sin);
    var rotatedW = this._GetRotatedNeighbor(window, -1, 0, cos, sin);

    var w4 = c4.Work;

    // Build 3x3 output block - all default to center
    var e = stackalloc TWork[9];
    for (var i = 0; i < 9; ++i)
      e[i] = w4;

    // Modified Scale3x with rotated sampling
    if (!equality.Equals(rotatedW.Key, rotatedE.Key) && !equality.Equals(rotatedN.Key, rotatedS.Key)) {
      // Top-left corner
      if (equality.Equals(rotatedN.Key, rotatedW.Key))
        e[0] = lerp.Lerp(rotatedN.Work, rotatedW.Work);

      // Top-right corner
      if (equality.Equals(rotatedN.Key, rotatedE.Key))
        e[2] = lerp.Lerp(rotatedN.Work, rotatedE.Work);

      // Bottom-left corner
      if (equality.Equals(rotatedS.Key, rotatedW.Key))
        e[6] = lerp.Lerp(rotatedS.Work, rotatedW.Work);

      // Bottom-right corner
      if (equality.Equals(rotatedS.Key, rotatedE.Key))
        e[8] = lerp.Lerp(rotatedS.Work, rotatedE.Work);
    }

    // Apply rotation to output positions
    this._WriteRotatedOutput(dest, destStride, encoder, e);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private NeighborPixel<TWork, TKey> _GetRotatedNeighbor(
    in NeighborWindow<TWork, TKey> window,
    int dx, int dy,
    float cos, float sin) {
    var srcX = dx * cos + dy * sin;
    var srcY = -dx * sin + dy * cos;
    var nx = (int)MathF.Round(srcX);
    var ny = (int)MathF.Round(srcY);
    nx = nx < -1 ? -1 : nx > 1 ? 1 : nx;
    ny = ny < -1 ? -1 : ny > 1 ? 1 : ny;
    return (ny, nx) switch {
      (-1, -1) => window.M1M1,
      (-1,  0) => window.M1P0,
      (-1,  1) => window.M1P1,
      ( 0, -1) => window.P0M1,
      ( 0,  0) => window.P0P0,
      ( 0,  1) => window.P0P1,
      ( 1, -1) => window.P1M1,
      ( 1,  0) => window.P1P0,
      ( 1,  1) => window.P1P1,
      _ => window.P0P0
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe void _WriteRotatedOutput(TPixel* dest, int destStride, in TEncode encoder, TWork* e) {
    var absAngle = MathF.Abs(rotationRadians);
    if (absAngle < 0.01f) {
      // No significant rotation - write normally
      dest[0] = encoder.Encode(e[0]); dest[1] = encoder.Encode(e[1]); dest[2] = encoder.Encode(e[2]);
      dest[destStride] = encoder.Encode(e[3]); dest[destStride + 1] = encoder.Encode(e[4]); dest[destStride + 2] = encoder.Encode(e[5]);
      dest[destStride * 2] = encoder.Encode(e[6]); dest[destStride * 2 + 1] = encoder.Encode(e[7]); dest[destStride * 2 + 2] = encoder.Encode(e[8]);
    } else {
      // Rotate output based on quadrant (90° increments)
      var quadrant = (int)(rotationRadians / (MathF.PI / 2)) % 4;
      if (quadrant < 0) quadrant += 4;

      switch (quadrant) {
        case 0: // 0-90°
          dest[0] = encoder.Encode(e[0]); dest[1] = encoder.Encode(e[1]); dest[2] = encoder.Encode(e[2]);
          dest[destStride] = encoder.Encode(e[3]); dest[destStride + 1] = encoder.Encode(e[4]); dest[destStride + 2] = encoder.Encode(e[5]);
          dest[destStride * 2] = encoder.Encode(e[6]); dest[destStride * 2 + 1] = encoder.Encode(e[7]); dest[destStride * 2 + 2] = encoder.Encode(e[8]);
          break;
        case 1: // 90-180° - rotate CW 90°
          dest[0] = encoder.Encode(e[6]); dest[1] = encoder.Encode(e[3]); dest[2] = encoder.Encode(e[0]);
          dest[destStride] = encoder.Encode(e[7]); dest[destStride + 1] = encoder.Encode(e[4]); dest[destStride + 2] = encoder.Encode(e[1]);
          dest[destStride * 2] = encoder.Encode(e[8]); dest[destStride * 2 + 1] = encoder.Encode(e[5]); dest[destStride * 2 + 2] = encoder.Encode(e[2]);
          break;
        case 2: // 180-270° - rotate 180°
          dest[0] = encoder.Encode(e[8]); dest[1] = encoder.Encode(e[7]); dest[2] = encoder.Encode(e[6]);
          dest[destStride] = encoder.Encode(e[5]); dest[destStride + 1] = encoder.Encode(e[4]); dest[destStride + 2] = encoder.Encode(e[3]);
          dest[destStride * 2] = encoder.Encode(e[2]); dest[destStride * 2 + 1] = encoder.Encode(e[1]); dest[destStride * 2 + 2] = encoder.Encode(e[0]);
          break;
        case 3: // 270-360° - rotate CW 270° (CCW 90°)
          dest[0] = encoder.Encode(e[2]); dest[1] = encoder.Encode(e[5]); dest[2] = encoder.Encode(e[8]);
          dest[destStride] = encoder.Encode(e[1]); dest[destStride + 1] = encoder.Encode(e[4]); dest[destStride + 2] = encoder.Encode(e[7]);
          dest[destStride * 2] = encoder.Encode(e[0]); dest[destStride * 2 + 1] = encoder.Encode(e[3]); dest[destStride * 2 + 2] = encoder.Encode(e[6]);
          break;
      }
    }
  }
}

#endregion

#region RotSprite 4x Kernel

file readonly struct RotSprite4xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default, float rotationRadians = 0f)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  // Precompute sin/cos to avoid trig in hot loop
  private readonly float _cos = MathF.Cos(rotationRadians);
  private readonly float _sin = MathF.Sin(rotationRadians);

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var cos = this._cos;
    var sin = this._sin;

    var c4 = window.P0P0; // center
    var rotatedN = this._GetRotatedNeighbor(window, 0, -1, cos, sin);
    var rotatedE = this._GetRotatedNeighbor(window, 1, 0, cos, sin);
    var rotatedS = this._GetRotatedNeighbor(window, 0, 1, cos, sin);
    var rotatedW = this._GetRotatedNeighbor(window, -1, 0, cos, sin);

    var w4 = c4.Work;

    // Build 4x4 output block - all default to center
    var e = stackalloc TWork[16];
    for (var i = 0; i < 16; ++i)
      e[i] = w4;

    // Modified Scale4x with rotated sampling (corners only)
    if (!equality.Equals(rotatedW.Key, rotatedE.Key) && !equality.Equals(rotatedN.Key, rotatedS.Key)) {
      // Top-left corner
      if (equality.Equals(rotatedN.Key, rotatedW.Key))
        e[0] = lerp.Lerp(rotatedN.Work, rotatedW.Work);

      // Top-right corner
      if (equality.Equals(rotatedN.Key, rotatedE.Key))
        e[3] = lerp.Lerp(rotatedN.Work, rotatedE.Work);

      // Bottom-left corner
      if (equality.Equals(rotatedS.Key, rotatedW.Key))
        e[12] = lerp.Lerp(rotatedS.Work, rotatedW.Work);

      // Bottom-right corner
      if (equality.Equals(rotatedS.Key, rotatedE.Key))
        e[15] = lerp.Lerp(rotatedS.Work, rotatedE.Work);
    }

    // Apply rotation to output positions
    this._WriteRotatedOutput(dest, destStride, encoder, e);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private NeighborPixel<TWork, TKey> _GetRotatedNeighbor(
    in NeighborWindow<TWork, TKey> window,
    int dx, int dy,
    float cos, float sin) {
    var srcX = dx * cos + dy * sin;
    var srcY = -dx * sin + dy * cos;
    var nx = (int)MathF.Round(srcX);
    var ny = (int)MathF.Round(srcY);
    nx = nx < -1 ? -1 : nx > 1 ? 1 : nx;
    ny = ny < -1 ? -1 : ny > 1 ? 1 : ny;
    return (ny, nx) switch {
      (-1, -1) => window.M1M1,
      (-1,  0) => window.M1P0,
      (-1,  1) => window.M1P1,
      ( 0, -1) => window.P0M1,
      ( 0,  0) => window.P0P0,
      ( 0,  1) => window.P0P1,
      ( 1, -1) => window.P1M1,
      ( 1,  0) => window.P1P0,
      ( 1,  1) => window.P1P1,
      _ => window.P0P0
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe void _WriteRotatedOutput(TPixel* dest, int destStride, in TEncode encoder, TWork* e) {
    var absAngle = MathF.Abs(rotationRadians);
    if (absAngle < 0.01f) {
      // No significant rotation - write normally
      for (var y = 0; y < 4; ++y)
        for (var x = 0; x < 4; ++x)
          dest[y * destStride + x] = encoder.Encode(e[y * 4 + x]);
    } else {
      // Rotate output based on quadrant (90° increments)
      var quadrant = (int)(rotationRadians / (MathF.PI / 2)) % 4;
      if (quadrant < 0) quadrant += 4;

      switch (quadrant) {
        case 0: // 0-90°
          for (var y = 0; y < 4; ++y)
            for (var x = 0; x < 4; ++x)
              dest[y * destStride + x] = encoder.Encode(e[y * 4 + x]);
          break;
        case 1: // 90-180° - rotate CW 90°
          for (var y = 0; y < 4; ++y)
            for (var x = 0; x < 4; ++x)
              dest[y * destStride + x] = encoder.Encode(e[(3 - x) * 4 + y]);
          break;
        case 2: // 180-270° - rotate 180°
          for (var y = 0; y < 4; ++y)
            for (var x = 0; x < 4; ++x)
              dest[y * destStride + x] = encoder.Encode(e[(3 - y) * 4 + (3 - x)]);
          break;
        case 3: // 270-360° - rotate CW 270° (CCW 90°)
          for (var y = 0; y < 4; ++y)
            for (var x = 0; x < 4; ++x)
              dest[y * destStride + x] = encoder.Encode(e[x * 4 + (3 - y)]);
          break;
      }
    }
  }
}

#endregion
