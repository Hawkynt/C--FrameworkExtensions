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

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

namespace System;

partial class SpanHelper {
  /// <summary>
  ///   Provides a managed array implementation of <see cref="MemoryHandlerBase{T}" />, allowing for array operations and
  ///   manipulations based on the <see cref="MemoryHandlerBase{T}" /> interface.
  /// </summary>
  /// <typeparam name="T">The type of elements stored in the managed array.</typeparam>
  /// <remarks>
  ///   This class manages an array segment by providing direct access and manipulation capabilities over a portion of an
  ///   array, beginning at a specified index.
  /// </remarks>
  public class ManagedArrayHandler<T> : MemoryHandlerBase<T> {
    private readonly SharedPin<T> _pin;
    public readonly T[] source;
    public readonly int start;

    /// <summary>
    ///   Provides a managed array implementation of <see cref="MemoryHandlerBase{T}" />, allowing for array operations and
    ///   manipulations based on the <see cref="MemoryHandlerBase{T}" /> interface.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the managed array.</typeparam>
    /// <remarks>
    ///   This class manages an array segment by providing direct access and manipulation capabilities over a portion of an
    ///   array, beginning at a specified index.
    /// </remarks>
    public ManagedArrayHandler(T[] source, int start):this(source,start,new(source)){ }

    private ManagedArrayHandler(T[] source, int start, SharedPin<T> pin) {
      this.source = source;
      this.start = start;
      this._pin = pin;
    }

    #region Implementation of IMemoryHandler<T>

    /// <inheritdoc />
    public override ref T GetRef(int index) {
      this._pin.TrackAccess();
      if (!this._pin.IsPinned)
        return ref this.source[this.start+index];
      unsafe {
        return ref *(this._pin.Pointer + this.start + index);
      }
    }

    /// <inheritdoc />
    public override T GetValue(int index) {
      this._pin.TrackAccess();
      if (!this._pin.IsPinned)
        return this.source[this.start+index];
      unsafe {
        return *(this._pin.Pointer+ this.start+index);
      }
    }

    /// <inheritdoc />
    public override void SetValue(int index, T value) {
      this._pin.TrackAccess();
      if (!this._pin.IsPinned)
        this.source[this.start + index] = value;
      else unsafe {
        *(this._pin.Pointer + this.start + index) = value;
      }
    }

    /// <inheritdoc />
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public override unsafe T* Pointer => this._pin.Pointer + this.start;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    
    /// <inheritdoc />
    public override MemoryHandlerBase<T> SliceFrom(int offset) => new ManagedArrayHandler<T>(this.source, this.start + offset, this._pin);

    #endregion
  }
}

#endif
