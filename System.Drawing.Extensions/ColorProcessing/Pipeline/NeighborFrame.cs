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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Hawkynt.ColorProcessing.Codecs;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Pipeline;

/// <summary>
/// Manages a sliding 5x5 pixel window over a frame with dual-type projections.
/// </summary>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
/// <typeparam name="TKey">The key color type (for pattern matching).</typeparam>
/// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
/// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
/// <remarks>
/// <para>
/// Uses a single pinned buffer of 5 rows with pre-computed OOB padding.
/// Movement is O(1) horizontally (pointer offset) and O(width) vertically (1 row load + 5 pointer rotations).
/// </para>
/// <para>
/// Supports parallel processing via the <paramref name="startY"/> constructor parameter.
/// Each partition can create its own independent NeighborFrame initialized at its starting row.
/// </para>
/// </remarks>
public sealed unsafe class NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> : IDisposable
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey> {

  private readonly NeighborPixel<TWork, TKey>[] _buffer;
  private readonly GCHandle _handle;
  private readonly int _rowWidth;
  private readonly int _sourceWidth;
  private readonly int _sourceHeight;
  private readonly int _sourceStride;
  private readonly TPixel* _sourcePtr;
  private readonly TDecode _decoder;
  private readonly TProject _projector;
  private readonly OutOfBoundsMode _horizontalMode;
  private readonly OutOfBoundsMode _verticalMode;

  private NeighborPixel<TWork, TKey>* _ptrM2;
  private NeighborPixel<TWork, TKey>* _ptrM1;
  private NeighborPixel<TWork, TKey>* _ptrP0;
  private NeighborPixel<TWork, TKey>* _ptrP1;
  private NeighborPixel<TWork, TKey>* _ptrP2;
  private int _currentY;
  private bool _disposed;

  /// <summary>
  /// Gets the width of the source frame.
  /// </summary>
  public int Width => this._sourceWidth;

  /// <summary>
  /// Gets the height of the source frame.
  /// </summary>
  public int Height => this._sourceHeight;

  /// <summary>
  /// Gets the current Y position.
  /// </summary>
  public int CurrentY => this._currentY;

  /// <summary>
  /// Gets a pixel at the specified coordinates with full decode/project.
  /// Use this for random access outside the optimized 5x5 window.
  /// </summary>
  /// <param name="x">The X coordinate (can be out of bounds).</param>
  /// <param name="y">The Y coordinate (can be out of bounds).</param>
  /// <returns>The decoded and projected pixel.</returns>
  /// <remarks>
  /// This bypasses the sliding window buffer and reads directly from the source.
  /// Use sparingly as it incurs the full decode/project cost per access.
  /// </remarks>
  public NeighborPixel<TWork, TKey> this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var clampedX = this._ClampX(x);
      var clampedY = this._ClampY(y);
      var pixel = this._sourcePtr[clampedY * this._sourceStride + clampedX];
      return this._DecodeAndProject(pixel);
    }
  }

  /// <summary>
  /// Creates a new NeighborFrame over the specified source.
  /// </summary>
  /// <param name="sourcePtr">Pointer to the source pixel data (must remain valid for the lifetime of this frame).</param>
  /// <param name="width">Width of the source image.</param>
  /// <param name="height">Height of the source image.</param>
  /// <param name="stride">Stride of the source image in pixels.</param>
  /// <param name="decoder">The decoder instance.</param>
  /// <param name="projector">The projector instance.</param>
  /// <param name="horizontalMode">How to handle horizontal out-of-bounds access.</param>
  /// <param name="verticalMode">How to handle vertical out-of-bounds access.</param>
  /// <param name="startY">The starting Y row (for parallel processing).</param>
  public NeighborFrame(
    TPixel* sourcePtr,
    int width,
    int height,
    int stride,
    TDecode decoder,
    TProject projector,
    OutOfBoundsMode horizontalMode = OutOfBoundsMode.Const,
    OutOfBoundsMode verticalMode = OutOfBoundsMode.Const,
    int startY = 0
  ) {
    this._sourcePtr = sourcePtr;
    this._sourceWidth = width;
    this._sourceHeight = height;
    this._sourceStride = stride;
    this._decoder = decoder;
    this._projector = projector;
    this._horizontalMode = horizontalMode;
    this._verticalMode = verticalMode;
    this._rowWidth = width + 4; // +2 left, +2 right for OOB padding

    // Allocate and pin buffer
    this._buffer = new NeighborPixel<TWork, TKey>[5 * this._rowWidth];
    this._handle = GCHandle.Alloc(this._buffer, GCHandleType.Pinned);
    var basePtr = (NeighborPixel<TWork, TKey>*)this._handle.AddrOfPinnedObject();

    // Initialize row pointers
    this._ptrM2 = basePtr;
    this._ptrM1 = basePtr + this._rowWidth;
    this._ptrP0 = basePtr + 2 * this._rowWidth;
    this._ptrP1 = basePtr + 3 * this._rowWidth;
    this._ptrP2 = basePtr + 4 * this._rowWidth;

    // Load initial 5 rows centered at startY
    this.SeekToRow(startY);
  }

  /// <summary>
  /// Gets a window positioned at X=0 for the current row.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public NeighborWindow<TWork, TKey> GetWindow() => new(
    this._ptrM2,
    this._ptrM1,
    this._ptrP0,
    this._ptrP1,
    this._ptrP2,
    startX: 2 // +2 for OOB padding
  );

  /// <summary>
  /// Advances to the next row. O(width) operation: 5 pointer rotations + 1 row load.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MoveDown() {
    // Rotate pointers: old top (M2) becomes new bottom (P2)
    var oldTop = this._ptrM2;
    this._ptrM2 = this._ptrM1;
    this._ptrM1 = this._ptrP0;
    this._ptrP0 = this._ptrP1;
    this._ptrP1 = this._ptrP2;
    this._ptrP2 = oldTop;

    // Load new bottom row into recycled memory
    ++this._currentY;
    this._LoadRow(this._ptrP2, this._currentY + 2);
  }

  /// <summary>
  /// Seeks to a specific row, reloading all 5 buffer rows. O(5 * width) operation.
  /// </summary>
  /// <param name="y">The target row to center the window on.</param>
  /// <remarks>
  /// Use this for random row access or large jumps (e.g., downscaling by ratio 3+).
  /// For sequential access, use <see cref="MoveDown"/> which is more cache-efficient.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SeekToRow(int y) {
    this._currentY = y;
    this._LoadRow(this._ptrM2, y - 2);
    this._LoadRow(this._ptrM1, y - 1);
    this._LoadRow(this._ptrP0, y);
    this._LoadRow(this._ptrP1, y + 1);
    this._LoadRow(this._ptrP2, y + 2);
  }

  /// <summary>
  /// Loads a row into the buffer with OOB handling and decode/project.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRow(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var clampedY = this._ClampY(y);

    // Fast path for Const-Const mode (most common)
    if (this._horizontalMode == OutOfBoundsMode.Const && this._verticalMode == OutOfBoundsMode.Const)
      this._LoadRowConstConst(rowPtr, clampedY);
    else
      this._LoadRowGeneric(rowPtr, clampedY);
  }

  /// <summary>
  /// Optimized row loader for Const-Const OOB mode with JIT-eliminated type dispatch.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRowConstConst(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    // JIT eliminates these branches at compile time based on type parameters
    if (typeof(TPixel) == typeof(TWork) && typeof(TWork) == typeof(TKey)) {
      // Fast path: Direct copy with SIMD (TPixel == TWork == TKey)
      var size = sizeof(TPixel);
      switch (size) {
        case 4:
          this._LoadRowDirectCopy4(rowPtr, y);
          break;
        case 8:
          this._LoadRowDirectCopy8(rowPtr, y);
          break;
        case 16:
          this._LoadRowDirectCopy16(rowPtr, y);
          break;
        default:
          this._LoadRowDirectCopyScalar(rowPtr, y);
          break;
      }
    } else if (typeof(TWork) == typeof(TKey))
      // Medium path: Decode only, no projection needed
      this._LoadRowDecodeOnly(rowPtr, y);
    else
      // Slow path: Full decode + project
      this._LoadRowFullTransform(rowPtr, y);
  }

  /// <summary>
  /// Direct copy for 4-byte pixels using SIMD (Vector512 → Vector256 → Vector128 → ulong cascade).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRowDirectCopy4(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var src = (uint*)(this._sourcePtr + y * this._sourceStride);
    var dst = (ulong*)(rowPtr + 2); // Skip left OOB padding
    var count = this._sourceWidth;

    // Vector512: 16 source pixels → 16 NeighborPixels (64 bytes in, 128 bytes out)
    if (Vector512.IsHardwareAccelerated && count >= 16) {
      while (count >= 16) {
        // Load 16 pixels
        var v = Vector512.Load(src);

        // Extract 256-bit halves
        var lo256 = v.GetLower();
        var hi256 = v.GetUpper();

        // Convert uint to ulong by duplicating: [A,B,C,D] → [A|A, B|B, C|C, D|D]
        // Process each 128-bit quarter separately
        var q0 = lo256.GetLower(); // [A,B,C,D]
        var q1 = lo256.GetUpper(); // [E,F,G,H]
        var q2 = hi256.GetLower(); // [I,J,K,L]
        var q3 = hi256.GetUpper(); // [M,N,O,P]

        // Widen each 128-bit vector of uint to 256-bit vector of ulong with duplication
        _DuplicateAndStore4(q0, dst);
        _DuplicateAndStore4(q1, dst + 4);
        _DuplicateAndStore4(q2, dst + 8);
        _DuplicateAndStore4(q3, dst + 12);

        src += 16;
        dst += 16;
        count -= 16;
      }
    }

    // Vector256: 8 source pixels → 8 NeighborPixels
    if (Vector256.IsHardwareAccelerated && count >= 8) {
      while (count >= 8) {
        var v = Vector256.Load(src);
        var lo = v.GetLower();
        var hi = v.GetUpper();

        _DuplicateAndStore4(lo, dst);
        _DuplicateAndStore4(hi, dst + 4);

        src += 8;
        dst += 8;
        count -= 8;
      }
    }

    // Vector128: 4 source pixels → 4 NeighborPixels
    if (Vector128.IsHardwareAccelerated && count >= 4) {
      while (count >= 4) {
        var v = Vector128.Load(src);
        _DuplicateAndStore4(v, dst);

        src += 4;
        dst += 4;
        count -= 4;
      }
    }

    // ulong: 1 source pixel → 1 NeighborPixel (duplicate uint into ulong)
    while (count > 0) {
      var pixel = *src;
      *dst = ((ulong)pixel << 32) | pixel;
      ++src;
      ++dst;
      --count;
    }

    // Copy edge pixels using tuple assignment
    var leftEdge = rowPtr[2];
    var w = this._sourceWidth;
    var rightEdge = rowPtr[w + 1];
    (rowPtr[0], rowPtr[1]) = (leftEdge, leftEdge);
    (rowPtr[w + 2], rowPtr[w + 3]) = (rightEdge, rightEdge);
  }

  /// <summary>
  /// Duplicates 4 uint values from Vector128 to 4 ulong values and stores them.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _DuplicateAndStore4(Vector128<uint> src, ulong* dst) {
    // Extract each uint and write as duplicated ulong
    var a = src.GetElement(0);
    var b = src.GetElement(1);
    var c = src.GetElement(2);
    var d = src.GetElement(3);

    dst[0] = ((ulong)a << 32) | a;
    dst[1] = ((ulong)b << 32) | b;
    dst[2] = ((ulong)c << 32) | c;
    dst[3] = ((ulong)d << 32) | d;
  }

  /// <summary>
  /// Direct copy for 8-byte pixels using SIMD (Vector256 → scalar cascade).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRowDirectCopy8(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var src = (ulong*)(this._sourcePtr + y * this._sourceStride);
    var dst = rowPtr + 2; // Skip left OOB padding
    var count = this._sourceWidth;

    // Vector256: 4 source pixels → 4 NeighborPixels (32 bytes in, 64 bytes out)
    if (Vector256.IsHardwareAccelerated && count >= 4) {
      while (count >= 4) {
        var v = Vector256.Load(src);

        // Duplicate each ulong: [A,B,C,D] → store [A,A], [B,B], [C,C], [D,D]
        var a = v.GetElement(0);
        var b = v.GetElement(1);
        var c = v.GetElement(2);
        var d = v.GetElement(3);

        var dup01 = Vector256.Create(a, a, b, b);
        var dup23 = Vector256.Create(c, c, d, d);

        Vector256.Store(dup01, (ulong*)dst);
        Vector256.Store(dup23, (ulong*)(dst + 2));

        src += 4;
        dst += 4;
        count -= 4;
      }
    }

    // Scalar: duplicate ulong
    while (count > 0) {
      var pixel = *src;
      var dstPtr = (ulong*)dst;
      (dstPtr[0], dstPtr[1]) = (pixel, pixel);
      ++src;
      ++dst;
      --count;
    }

    // Copy edge pixels using tuple assignment
    var leftEdge = rowPtr[2];
    var w = this._sourceWidth;
    var rightEdge = rowPtr[w + 1];
    (rowPtr[0], rowPtr[1]) = (leftEdge, leftEdge);
    (rowPtr[w + 2], rowPtr[w + 3]) = (rightEdge, rightEdge);
  }

  /// <summary>
  /// Direct copy for 16-byte pixels using Vector128.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRowDirectCopy16(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var src = (byte*)(this._sourcePtr + y * this._sourceStride);
    var dst = rowPtr + 2; // Skip left OOB padding
    var count = this._sourceWidth;

    // Each pixel is 16 bytes, NeighborPixel is 32 bytes (duplicate)
    if (Vector128.IsHardwareAccelerated) {
      while (count > 0) {
        var pixel = Vector128.Load(src);
        var dstBytes = (byte*)dst;
        Vector128.Store(pixel, dstBytes);
        Vector128.Store(pixel, dstBytes + 16);

        src += 16;
        ++dst;
        --count;
      }
    } else {
      // Scalar fallback for 16-byte
      var srcPtr = (this._sourcePtr + y * this._sourceStride);
      for (var x = 0; x < count; ++x) {
        var pixel = srcPtr[x];
        var work = Unsafe.As<TPixel, TWork>(ref pixel);
        var key = Unsafe.As<TPixel, TKey>(ref pixel);
        dst[x] = new NeighborPixel<TWork, TKey>(work, key);
      }
    }

    // Copy edge pixels using tuple assignment
    var leftEdge = rowPtr[2];
    var w = this._sourceWidth;
    var rightEdge = rowPtr[w + 1];
    (rowPtr[0], rowPtr[1]) = (leftEdge, leftEdge);
    (rowPtr[w + 2], rowPtr[w + 3]) = (rightEdge, rightEdge);
  }

  /// <summary>
  /// Scalar direct copy fallback for non-standard pixel sizes.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRowDirectCopyScalar(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var srcPtr = this._sourcePtr + y * this._sourceStride;
    var dst = rowPtr + 2;
    var count = this._sourceWidth;

    for (var x = 0; x < count; ++x) {
      var pixel = srcPtr[x];
      var work = Unsafe.As<TPixel, TWork>(ref pixel);
      var key = Unsafe.As<TPixel, TKey>(ref pixel);
      dst[x] = new NeighborPixel<TWork, TKey>(work, key);
    }

    // Copy edge pixels using tuple assignment
    var leftEdge = rowPtr[2];
    var w = this._sourceWidth;
    var rightEdge = rowPtr[w + 1];
    (rowPtr[0], rowPtr[1]) = (leftEdge, leftEdge);
    (rowPtr[w + 2], rowPtr[w + 3]) = (rightEdge, rightEdge);
  }

  /// <summary>
  /// Decode only path when TWork == TKey (no projection needed).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRowDecodeOnly(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var srcRow = this._sourcePtr + y * this._sourceStride;
    var width = this._sourceWidth;
    var dst = rowPtr + 2;

    // Process main pixels - decode and duplicate as Work+Key
    for (var x = 0; x < width; ++x) {
      var work = this._decoder.Decode(in srcRow[x]);
      dst[x] = new NeighborPixel<TWork, TKey>(work, Unsafe.As<TWork, TKey>(ref work));
    }

    // Copy edge pixels using tuple assignment
    var leftEdge = rowPtr[2];
    var rightEdge = rowPtr[width + 1];
    (rowPtr[0], rowPtr[1]) = (leftEdge, leftEdge);
    (rowPtr[width + 2], rowPtr[width + 3]) = (rightEdge, rightEdge);
  }

  /// <summary>
  /// Full transform path with decode + project.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LoadRowFullTransform(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var srcRow = this._sourcePtr + y * this._sourceStride;
    var width = this._sourceWidth;
    var dst = rowPtr + 2;

    // Process main pixels with full decode + project
    for (var x = 0; x < width; ++x)
      dst[x] = this._DecodeAndProject(srcRow[x]);

    // Copy edge pixels using tuple assignment
    var leftEdge = rowPtr[2];
    var rightEdge = rowPtr[width + 1];
    (rowPtr[0], rowPtr[1]) = (leftEdge, leftEdge);
    (rowPtr[width + 2], rowPtr[width + 3]) = (rightEdge, rightEdge);
  }

  /// <summary>
  /// Generic row loader for all OOB modes.
  /// </summary>
  private void _LoadRowGeneric(NeighborPixel<TWork, TKey>* rowPtr, int y) {
    var srcRow = this._sourcePtr + y * this._sourceStride;
    var width = this._sourceWidth;

    for (var bufferX = 0; bufferX < this._rowWidth; ++bufferX) {
      var srcX = this._ClampX(bufferX - 2);
      rowPtr[bufferX] = this._DecodeAndProject(srcRow[srcX]);
    }
  }

  /// <summary>
  /// Decodes a pixel to work space and projects to key space.
  /// Uses JIT-eliminated type checks for fast paths.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private NeighborPixel<TWork, TKey> _DecodeAndProject(TPixel pixel) {
    TWork work;
    TKey key;

    // Fast path: TWork == TPixel (skip decode)
    if (typeof(TWork) == typeof(TPixel))
      work = Unsafe.As<TPixel, TWork>(ref pixel);
    else
      work = this._decoder.Decode(in pixel);

    // Fast path: TKey == TPixel (skip project, use original pixel)
    if (typeof(TKey) == typeof(TPixel))
      key = Unsafe.As<TPixel, TKey>(ref pixel);
    // Fast path: TKey == TWork (skip project, reuse decoded work)
    else if (typeof(TKey) == typeof(TWork))
      key = Unsafe.As<TWork, TKey>(ref work);
    else
      key = this._projector.Project(in work);

    return new(work, key);
  }

  /// <summary>
  /// Clamps Y coordinate according to vertical OOB mode.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _ClampY(int y) {
    if (y >= 0 && y < this._sourceHeight)
      return y;

    return this._verticalMode switch {
      OutOfBoundsMode.Const => y < 0 ? 0 : this._sourceHeight - 1,
      OutOfBoundsMode.Half => this._MirrorHalf(y, this._sourceHeight),
      OutOfBoundsMode.Whole => this._MirrorWhole(y, this._sourceHeight),
      OutOfBoundsMode.Wrap => this._Wrap(y, this._sourceHeight),
      OutOfBoundsMode.Transparent => 0, // Will be overridden with transparent value
      _ => y < 0 ? 0 : this._sourceHeight - 1
    };
  }

  /// <summary>
  /// Clamps X coordinate according to horizontal OOB mode.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _ClampX(int x) {
    if (x >= 0 && x < this._sourceWidth)
      return x;

    return this._horizontalMode switch {
      OutOfBoundsMode.Const => x < 0 ? 0 : this._sourceWidth - 1,
      OutOfBoundsMode.Half => this._MirrorHalf(x, this._sourceWidth),
      OutOfBoundsMode.Whole => this._MirrorWhole(x, this._sourceWidth),
      OutOfBoundsMode.Wrap => this._Wrap(x, this._sourceWidth),
      OutOfBoundsMode.Transparent => 0, // Will be overridden with transparent value
      _ => x < 0 ? 0 : this._sourceWidth - 1
    };
  }

  /// <summary>
  /// Mirrors at half-pixel positions: cba|abcde|edc
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _MirrorHalf(int coord, int size) {
    if (coord < 0)
      return -coord - 1;
    if (coord >= size)
      return 2 * size - coord - 1;
    return coord;
  }

  /// <summary>
  /// Mirrors at pixel centers: dcb|abcde|dcb
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _MirrorWhole(int coord, int size) {
    if (coord < 0)
      return -coord;
    if (coord >= size)
      return 2 * size - coord - 2;
    return coord;
  }

  /// <summary>
  /// Wraps around: cde|abcde|abc
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _Wrap(int coord, int size) {
    var result = coord % size;
    return result < 0 ? result + size : result;
  }

  /// <inheritdoc/>
  public void Dispose() {
    if (this._disposed)
      return;

    this._disposed = true;
    if (this._handle.IsAllocated)
      this._handle.Free();
  }
}
