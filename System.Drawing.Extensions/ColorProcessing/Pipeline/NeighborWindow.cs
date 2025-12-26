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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Pipeline;

/// <summary>
/// Provides O(1) access to a 5x5 pixel neighborhood for image scaling algorithms.
/// </summary>
/// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
/// <typeparam name="TKey">The key color type (for pattern matching).</typeparam>
/// <remarks>
/// <para>
/// This ref struct provides 25 combined-pixel accessors (Work + Key in single struct).
/// Naming convention: M = minus, P = plus, followed by row then column offset.
/// Example: M2M1 = row -2, column -1 relative to center (P0P0).
/// </para>
/// <para>
/// Movement is O(1): MoveRight increments a single pointer offset.
/// The 5 row pointers remain fixed; only the X offset changes.
/// </para>
/// </remarks>
public unsafe ref struct NeighborWindow<TWork, TKey>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace {

  private readonly NeighborPixel<TWork, TKey>* _ptrM2;
  private readonly NeighborPixel<TWork, TKey>* _ptrM1;
  private readonly NeighborPixel<TWork, TKey>* _ptrP0;
  private readonly NeighborPixel<TWork, TKey>* _ptrP1;
  private readonly NeighborPixel<TWork, TKey>* _ptrP2;
  private int _currentX;

  /// <summary>
  /// Creates a new window positioned at the specified X offset.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal NeighborWindow(
    NeighborPixel<TWork, TKey>* ptrM2,
    NeighborPixel<TWork, TKey>* ptrM1,
    NeighborPixel<TWork, TKey>* ptrP0,
    NeighborPixel<TWork, TKey>* ptrP1,
    NeighborPixel<TWork, TKey>* ptrP2,
    int startX
  ) {
    this._ptrM2 = ptrM2;
    this._ptrM1 = ptrM1;
    this._ptrP0 = ptrP0;
    this._ptrP1 = ptrP1;
    this._ptrP2 = ptrP2;
    this._currentX = startX;
  }

  #region Row -2 (M2)

  /// <summary>Row -2, Column -2</summary>
  public NeighborPixel<TWork, TKey> M2M2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM2[this._currentX - 2];
  }

  /// <summary>Row -2, Column -1</summary>
  public NeighborPixel<TWork, TKey> M2M1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM2[this._currentX - 1];
  }

  /// <summary>Row -2, Column 0</summary>
  public NeighborPixel<TWork, TKey> M2P0 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM2[this._currentX];
  }

  /// <summary>Row -2, Column +1</summary>
  public NeighborPixel<TWork, TKey> M2P1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM2[this._currentX + 1];
  }

  /// <summary>Row -2, Column +2</summary>
  public NeighborPixel<TWork, TKey> M2P2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM2[this._currentX + 2];
  }

  #endregion

  #region Row -1 (M1)

  /// <summary>Row -1, Column -2</summary>
  public NeighborPixel<TWork, TKey> M1M2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM1[this._currentX - 2];
  }

  /// <summary>Row -1, Column -1</summary>
  public NeighborPixel<TWork, TKey> M1M1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM1[this._currentX - 1];
  }

  /// <summary>Row -1, Column 0</summary>
  public NeighborPixel<TWork, TKey> M1P0 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM1[this._currentX];
  }

  /// <summary>Row -1, Column +1</summary>
  public NeighborPixel<TWork, TKey> M1P1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM1[this._currentX + 1];
  }

  /// <summary>Row -1, Column +2</summary>
  public NeighborPixel<TWork, TKey> M1P2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrM1[this._currentX + 2];
  }

  #endregion

  #region Row 0 (P0) - Center Row

  /// <summary>Row 0, Column -2</summary>
  public NeighborPixel<TWork, TKey> P0M2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP0[this._currentX - 2];
  }

  /// <summary>Row 0, Column -1</summary>
  public NeighborPixel<TWork, TKey> P0M1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP0[this._currentX - 1];
  }

  /// <summary>Row 0, Column 0 - Center pixel</summary>
  public NeighborPixel<TWork, TKey> P0P0 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP0[this._currentX];
  }

  /// <summary>Row 0, Column +1</summary>
  public NeighborPixel<TWork, TKey> P0P1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP0[this._currentX + 1];
  }

  /// <summary>Row 0, Column +2</summary>
  public NeighborPixel<TWork, TKey> P0P2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP0[this._currentX + 2];
  }

  #endregion

  #region Row +1 (P1)

  /// <summary>Row +1, Column -2</summary>
  public NeighborPixel<TWork, TKey> P1M2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP1[this._currentX - 2];
  }

  /// <summary>Row +1, Column -1</summary>
  public NeighborPixel<TWork, TKey> P1M1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP1[this._currentX - 1];
  }

  /// <summary>Row +1, Column 0</summary>
  public NeighborPixel<TWork, TKey> P1P0 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP1[this._currentX];
  }

  /// <summary>Row +1, Column +1</summary>
  public NeighborPixel<TWork, TKey> P1P1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP1[this._currentX + 1];
  }

  /// <summary>Row +1, Column +2</summary>
  public NeighborPixel<TWork, TKey> P1P2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP1[this._currentX + 2];
  }

  #endregion

  #region Row +2 (P2)

  /// <summary>Row +2, Column -2</summary>
  public NeighborPixel<TWork, TKey> P2M2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP2[this._currentX - 2];
  }

  /// <summary>Row +2, Column -1</summary>
  public NeighborPixel<TWork, TKey> P2M1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP2[this._currentX - 1];
  }

  /// <summary>Row +2, Column 0</summary>
  public NeighborPixel<TWork, TKey> P2P0 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP2[this._currentX];
  }

  /// <summary>Row +2, Column +1</summary>
  public NeighborPixel<TWork, TKey> P2P1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP2[this._currentX + 1];
  }

  /// <summary>Row +2, Column +2</summary>
  public NeighborPixel<TWork, TKey> P2P2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ptrP2[this._currentX + 2];
  }

  #endregion

  #region Movement

  /// <summary>
  /// Moves the window one pixel to the right. O(1) operation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MoveRight() => ++this._currentX;

  /// <summary>
  /// Moves the window by the specified number of pixels. O(1) operation.
  /// </summary>
  /// <param name="delta">The number of pixels to move (positive = right, negative = left).</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MoveBy(int delta) => this._currentX += delta;

  /// <summary>
  /// Resets the X position to the start of the row.
  /// </summary>
  /// <param name="startX">The starting X offset (typically 2 for OOB padding).</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ResetX(int startX) => this._currentX = startX;

  #endregion
}
