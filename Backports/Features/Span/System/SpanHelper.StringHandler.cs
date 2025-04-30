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

#if !SUPPORTS_SPAN

using Guard;

namespace System;

partial class SpanHelper {
  
  public class StringHandler : MemoryHandlerBase<char> {

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
    public override ref char GetRef(int index) {
      unsafe {
        return ref *(this._pin.Pointer + this.start + index);
      }
    }

    /// <inheritdoc />
    public override char GetValue(int index) {
      this._pin.TrackAccess();
      if (!this._pin.IsPinned)
        return this.source[this.start + index];
      unsafe {
        return *(this._pin.Pointer + this.start + index);
      }
    }

    /// <inheritdoc />
    public override void SetValue(int index, char value) => AlwaysThrow.InvalidOperationException("Strings are immutable");

    /// <inheritdoc />
    public override unsafe char* Pointer => this._pin.Pointer + this.start;

    /// <inheritdoc />
    public override MemoryHandlerBase<char> SliceFrom(int offset) => new StringHandler(this.source, this.start + offset, this._pin);

    #endregion

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString() => this.start == 0 ? this.source : this.source[this.start..];

    #endregion

    public string ToString(int length) => this.start == 0 && length == this.source.Length ? this.source : this.source.Substring(this.start, length);

  }
}

#endif