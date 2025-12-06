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
using System.Buffers;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Memory")]
public class MemoryTests {

  #region Memory<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void Memory_FromArray_CreatesMemory() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new Memory<int>(array);

    Assert.That(memory.Length, Is.EqualTo(5));
    Assert.That(memory.IsEmpty, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Memory_FromArraySlice_CreatesSlicedMemory() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new Memory<int>(array, 1, 3);

    Assert.That(memory.Length, Is.EqualTo(3));
    Assert.That(memory.Span[0], Is.EqualTo(2));
    Assert.That(memory.Span[2], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Memory_Empty_HasZeroLength() {
    var memory = Memory<int>.Empty;

    Assert.That(memory.Length, Is.EqualTo(0));
    Assert.That(memory.IsEmpty, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Memory_NullArray_CreatesEmptyMemory() {
    var memory = new Memory<int>(null);

    Assert.That(memory.Length, Is.EqualTo(0));
    Assert.That(memory.IsEmpty, Is.True);
  }

  #endregion

  #region Memory<T> - Slice

  [Test]
  [Category("HappyPath")]
  public void Memory_Slice_Start_ReturnsSlicedMemory() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new Memory<int>(array);
    var sliced = memory.Slice(2);

    Assert.That(sliced.Length, Is.EqualTo(3));
    Assert.That(sliced.Span[0], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Memory_Slice_StartAndLength_ReturnsSlicedMemory() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new Memory<int>(array);
    var sliced = memory.Slice(1, 2);

    Assert.That(sliced.Length, Is.EqualTo(2));
    Assert.That(sliced.Span[0], Is.EqualTo(2));
    Assert.That(sliced.Span[1], Is.EqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void Memory_Slice_InvalidStart_ThrowsArgumentOutOfRange() {
    var array = new[] { 1, 2, 3 };
    var memory = new Memory<int>(array);

    Assert.Throws<ArgumentOutOfRangeException>(() => memory.Slice(5));
  }

  #endregion

  #region Memory<T> - Span Access

  [Test]
  [Category("HappyPath")]
  public void Memory_Span_AllowsReadWrite() {
    var array = new[] { 1, 2, 3 };
    var memory = new Memory<int>(array);

    memory.Span[1] = 42;

    Assert.That(array[1], Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Memory_ToArray_CreatesNewArray() {
    var array = new[] { 1, 2, 3 };
    var memory = new Memory<int>(array);
    var copy = memory.ToArray();

    Assert.That(copy, Is.EqualTo(array));
    Assert.That(copy, Is.Not.SameAs(array));
  }

  #endregion

  #region Memory<T> - Equality

  [Test]
  [Category("HappyPath")]
  public void Memory_Equals_SameMemory_ReturnsTrue() {
    var array = new[] { 1, 2, 3 };
    var memory1 = new Memory<int>(array);
    var memory2 = new Memory<int>(array);

    Assert.That(memory1.Equals(memory2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Memory_Equals_DifferentMemory_ReturnsFalse() {
    var array1 = new[] { 1, 2, 3 };
    var array2 = new[] { 1, 2, 3 };
    var memory1 = new Memory<int>(array1);
    var memory2 = new Memory<int>(array2);

    Assert.That(memory1.Equals(memory2), Is.False);
  }

  #endregion

  #region ReadOnlyMemory<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_FromArray_CreatesMemory() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new ReadOnlyMemory<int>(array);

    Assert.That(memory.Length, Is.EqualTo(5));
    Assert.That(memory.IsEmpty, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_FromCharArray_CreatesCharMemory() {
    var chars = new[] { 'H', 'e', 'l', 'l', 'o' };
    var memory = new ReadOnlyMemory<char>(chars);

    Assert.That(memory.Length, Is.EqualTo(5));
    Assert.That(memory.Span[0], Is.EqualTo('H'));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_Empty_HasZeroLength() {
    var memory = ReadOnlyMemory<int>.Empty;

    Assert.That(memory.Length, Is.EqualTo(0));
    Assert.That(memory.IsEmpty, Is.True);
  }

  #endregion

  #region ReadOnlyMemory<T> - Slice

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_Slice_Start_ReturnsSlicedMemory() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new ReadOnlyMemory<int>(array);
    var sliced = memory.Slice(2);

    Assert.That(sliced.Length, Is.EqualTo(3));
    Assert.That(sliced.Span[0], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_Slice_StartAndLength_ReturnsSlicedMemory() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new ReadOnlyMemory<int>(array);
    var sliced = memory.Slice(1, 2);

    Assert.That(sliced.Length, Is.EqualTo(2));
    Assert.That(sliced.Span[0], Is.EqualTo(2));
    Assert.That(sliced.Span[1], Is.EqualTo(3));
  }

  #endregion

  #region ReadOnlyMemory<T> - ToArray

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_ToArray_CreatesNewArray() {
    var array = new[] { 1, 2, 3 };
    var memory = new ReadOnlyMemory<int>(array);
    var copy = memory.ToArray();

    Assert.That(copy, Is.EqualTo(array));
    Assert.That(copy, Is.Not.SameAs(array));
  }

  #endregion

  #region MemoryPool<T>

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_Shared_ReturnsInstance() {
    var pool = MemoryPool<byte>.Shared;

    Assert.That(pool, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_Rent_ReturnsMemory() {
    var pool = MemoryPool<int>.Shared;
    using var owner = pool.Rent(100);

    Assert.That(owner.Memory.Length, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_Rent_Default_ReturnsPositiveSizeMemory() {
    var pool = MemoryPool<byte>.Shared;
    using var owner = pool.Rent();

    Assert.That(owner.Memory.Length, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_Rent_MemoryCanBeWrittenTo() {
    var pool = MemoryPool<int>.Shared;
    using var owner = pool.Rent(10);

    owner.Memory.Span[0] = 42;
    owner.Memory.Span[9] = 100;

    Assert.That(owner.Memory.Span[0], Is.EqualTo(42));
    Assert.That(owner.Memory.Span[9], Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_MaxBufferSize_IsPositive() {
    var pool = MemoryPool<byte>.Shared;

    Assert.That(pool.MaxBufferSize, Is.GreaterThan(0));
  }

  #endregion

  #region SequencePosition

  [Test]
  [Category("HappyPath")]
  public void SequencePosition_Default_IsEmpty() {
    var position = default(SequencePosition);

    Assert.That(position.GetObject(), Is.Null);
    Assert.That(position.GetInteger(), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SequencePosition_WithValues_ReturnsValues() {
    var obj = new object();
    var position = new SequencePosition(obj, 42);

    Assert.That(position.GetObject(), Is.SameAs(obj));
    Assert.That(position.GetInteger(), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void SequencePosition_Equals_SameValues_ReturnsTrue() {
    var obj = new object();
    var pos1 = new SequencePosition(obj, 42);
    var pos2 = new SequencePosition(obj, 42);

    Assert.That(pos1.Equals(pos2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SequencePosition_Equals_DifferentValues_ReturnsFalse() {
    var pos1 = new SequencePosition(new object(), 42);
    var pos2 = new SequencePosition(new object(), 42);

    Assert.That(pos1.Equals(pos2), Is.False);
  }

  #endregion

}
