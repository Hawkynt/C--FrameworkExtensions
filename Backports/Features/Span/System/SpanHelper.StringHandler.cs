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

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

using System.Runtime.CompilerServices;
using Guard;

namespace System;

partial class SpanHelper {
  
  public sealed class StringHandler<T> : MemoryHandlerBase<T> {

    private readonly SharedPin<char> _pin;
    public readonly string source;
    public readonly int start;

    private StringHandler(string source, int start, SharedPin<char> pin) {
      this.source = source;
      this.start = start;
      this._pin = pin;
    }

    public StringHandler(string source, int start):this(source,start,new (source)) {
      this.source = source;
      this.start = start;
    }
    
    #region Implementation of IMemoryHandler<char>

    /// <inheritdoc />
    public override ref T GetRef(int index) {
      unsafe {
        return ref Unsafe.AsRef<T>(this._pin.Pointer + this.start + index);
      }
    }

    /// <inheritdoc />
    public override T GetValue(int index) {
      this._pin.TrackAccess();
      if (!this._pin.IsPinned)
        return (T)(object)this.source[this.start + index];
      unsafe {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        return *(T*)(this._pin.Pointer + this.start + index);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
      }
    }

    /// <inheritdoc />
    public override void SetValue(int index, T value) => AlwaysThrow.InvalidOperationException("Strings are immutable");

    /// <inheritdoc />
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public override unsafe T* Pointer => (T*)(this._pin.Pointer + this.start);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

    /// <inheritdoc />
    public override MemoryHandlerBase<T> SliceFrom(int offset) => new StringHandler<T>(this.source, this.start + offset, this._pin);

    #endregion

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString() => this.start == 0 ? this.source : this.source[this.start..];

    #endregion

    public string ToString(int length) => this.start == 0 && length == this.source.Length ? this.source : this.source.Substring(this.start, length);

  }
}

#endif