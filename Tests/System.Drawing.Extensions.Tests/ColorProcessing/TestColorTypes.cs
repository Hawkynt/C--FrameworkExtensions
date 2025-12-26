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

using Hawkynt.ColorProcessing;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Test wrapper for int that implements IStorageSpace.
/// </summary>
public readonly struct TestInt32 : IStorageSpace, IEquatable<TestInt32> {
  public readonly int Value;
  public TestInt32(int value) => this.Value = value;
  public bool Equals(TestInt32 other) => this.Value == other.Value;
  public override bool Equals(object? obj) => obj is TestInt32 other && this.Equals(other);
  public override int GetHashCode() => this.Value;
  public static bool operator ==(TestInt32 left, TestInt32 right) => left.Equals(right);
  public static bool operator !=(TestInt32 left, TestInt32 right) => !left.Equals(right);
  public static implicit operator int(TestInt32 t) => t.Value;
  public static implicit operator TestInt32(int v) => new(v);
}

/// <summary>
/// Test wrapper for uint that implements IStorageSpace.
/// </summary>
public readonly struct TestUInt32 : IStorageSpace, IEquatable<TestUInt32> {
  public readonly uint Value;
  public TestUInt32(uint value) => this.Value = value;
  public bool Equals(TestUInt32 other) => this.Value == other.Value;
  public override bool Equals(object? obj) => obj is TestUInt32 other && this.Equals(other);
  public override int GetHashCode() => (int)this.Value;
  public static bool operator ==(TestUInt32 left, TestUInt32 right) => left.Equals(right);
  public static bool operator !=(TestUInt32 left, TestUInt32 right) => !left.Equals(right);
  public static implicit operator uint(TestUInt32 t) => t.Value;
  public static implicit operator TestUInt32(uint v) => new(v);
}

/// <summary>
/// Test wrapper for ulong that implements IStorageSpace.
/// </summary>
public readonly struct TestUInt64 : IStorageSpace, IEquatable<TestUInt64> {
  public readonly ulong Value;
  public TestUInt64(ulong value) => this.Value = value;
  public bool Equals(TestUInt64 other) => this.Value == other.Value;
  public override bool Equals(object? obj) => obj is TestUInt64 other && this.Equals(other);
  public override int GetHashCode() => (int)this.Value;
  public static bool operator ==(TestUInt64 left, TestUInt64 right) => left.Equals(right);
  public static bool operator !=(TestUInt64 left, TestUInt64 right) => !left.Equals(right);
  public static implicit operator ulong(TestUInt64 t) => t.Value;
  public static implicit operator TestUInt64(ulong v) => new(v);
}
