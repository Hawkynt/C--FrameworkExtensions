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

#if !SUPPORTS_MEMORY && !OFFICIAL_MEMORY

namespace System;

/// <summary>
/// Represents a position in a non-contiguous set of memory.
/// </summary>
public readonly struct SequencePosition : IEquatable<SequencePosition> {

  private readonly object _object;
  private readonly int _integer;

  /// <summary>
  /// Creates a new <see cref="SequencePosition"/> at the specified object and integer position.
  /// </summary>
  public SequencePosition(object @object, int integer) {
    this._object = @object;
    this._integer = integer;
  }

  /// <summary>
  /// Gets the object part of this position.
  /// </summary>
  public object GetObject() => this._object;

  /// <summary>
  /// Gets the integer part of this position.
  /// </summary>
  public int GetInteger() => this._integer;

  /// <inheritdoc />
  public bool Equals(SequencePosition other) =>
    this._integer == other._integer && object.Equals(this._object, other._object);

  /// <inheritdoc />
  public override bool Equals(object obj) => obj is SequencePosition other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() {
    unchecked {
      var hash = this._object?.GetHashCode() ?? 0;
      hash = (hash * 397) ^ this._integer;
      return hash;
    }
  }

  public static bool operator ==(SequencePosition left, SequencePosition right) => left.Equals(right);
  public static bool operator !=(SequencePosition left, SequencePosition right) => !left.Equals(right);
}

#endif
