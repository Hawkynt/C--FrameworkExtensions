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
[Category("MemoryComplex")]
public class MemoryComplexTests {

  #region ReadOnlySequence<T>

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_FromArray_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var sequence = new ReadOnlySequence<int>(array);

    Assert.That(sequence.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_FromArraySlice_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    var sequence = new ReadOnlySequence<int>(array, 2, 5);

    Assert.That(sequence.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_IsSingleSegment_ReturnsTrue() {
    var array = new[] { 1, 2, 3 };
    var sequence = new ReadOnlySequence<int>(array);

    Assert.That(sequence.IsSingleSegment, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_First_ReturnsFirstSegment() {
    var array = new[] { 10, 20, 30 };
    var sequence = new ReadOnlySequence<int>(array);

    var first = sequence.First;
    Assert.That(first.Length, Is.EqualTo(3));
    Assert.That(first.Span[0], Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_ToArray_ReturnsCorrectData() {
    var array = new[] { 5, 10, 15, 20 };
    var sequence = new ReadOnlySequence<int>(array);

    var result = sequence.ToArray();
    Assert.That(result, Is.EqualTo(array));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_Slice_ReturnsSubsequence() {
    var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    var sequence = new ReadOnlySequence<int>(array);

    var sliced = sequence.Slice(2, 5);
    Assert.That(sliced.Length, Is.EqualTo(5));
    Assert.That(sliced.ToArray(), Is.EqualTo(new[] { 3, 4, 5, 6, 7 }));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_SliceFromStart_ReturnsCorrectSubsequence() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var sequence = new ReadOnlySequence<int>(array);

    var sliced = sequence.Slice(3);
    Assert.That(sliced.Length, Is.EqualTo(2));
    Assert.That(sliced.ToArray(), Is.EqualTo(new[] { 4, 5 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadOnlySequence_Empty_HasZeroLength() {
    var sequence = ReadOnlySequence<int>.Empty;

    Assert.That(sequence.Length, Is.EqualTo(0));
    Assert.That(sequence.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySequence_Enumerator_IteratesAllSegments() {
    var array = new[] { 1, 2, 3 };
    var sequence = new ReadOnlySequence<int>(array);

    var count = 0;
    foreach (var segment in sequence)
      count += segment.Length;

    Assert.That(count, Is.EqualTo(3));
  }

  #endregion

  #region MemoryPool<T>

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_Shared_ReturnsSingletonInstance() {
    var pool1 = MemoryPool<byte>.Shared;
    var pool2 = MemoryPool<byte>.Shared;

    Assert.That(pool1, Is.SameAs(pool2));
  }

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_Rent_ReturnsMemoryOfRequestedSize() {
    using var pool = MemoryPool<byte>.Shared;
    using var owner = pool.Rent(100);

    Assert.That(owner.Memory.Length, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_Rent_DefaultSize_ReturnsNonEmptyMemory() {
    using var pool = MemoryPool<byte>.Shared;
    using var owner = pool.Rent();

    Assert.That(owner.Memory.Length, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void MemoryPool_RentedMemory_CanWriteAndRead() {
    using var pool = MemoryPool<int>.Shared;
    using var owner = pool.Rent(10);

    var memory = owner.Memory;
    memory.Span[0] = 42;
    memory.Span[5] = 100;

    Assert.That(memory.Span[0], Is.EqualTo(42));
    Assert.That(memory.Span[5], Is.EqualTo(100));
  }

  #endregion

  #region MemoryExtensions CommonPrefixLength

  [Test]
  [Category("HappyPath")]
  public void CommonPrefixLength_MatchingPrefix_ReturnsLength() {
    var array1 = new[] { 1, 2, 3, 4, 5 };
    var array2 = new[] { 1, 2, 3, 8, 9 };

    var prefixLength = array1.AsSpan().CommonPrefixLength(array2.AsSpan());
    Assert.That(prefixLength, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void CommonPrefixLength_FullMatch_ReturnsMinLength() {
    var array1 = new[] { 1, 2, 3 };
    var array2 = new[] { 1, 2, 3, 4, 5 };

    var prefixLength = array1.AsSpan().CommonPrefixLength(array2.AsSpan());
    Assert.That(prefixLength, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void CommonPrefixLength_NoMatch_ReturnsZero() {
    var array1 = new[] { 1, 2, 3 };
    var array2 = new[] { 4, 5, 6 };

    var prefixLength = array1.AsSpan().CommonPrefixLength(array2.AsSpan());
    Assert.That(prefixLength, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void CommonPrefixLength_EmptySpan_ReturnsZero() {
    var array1 = new[] { 1, 2, 3 };
    var array2 = Array.Empty<int>();

    var prefixLength = array1.AsSpan().CommonPrefixLength(array2.AsSpan());
    Assert.That(prefixLength, Is.EqualTo(0));
  }

  #endregion

  #region MemoryExtensions Count

  [Test]
  [Category("HappyPath")]
  public void Count_Element_ReturnsOccurrences() {
    var array = new[] { 1, 2, 3, 2, 4, 2, 5 };

    var count = array.AsSpan().Count(2);
    Assert.That(count, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void Count_NotFound_ReturnsZero() {
    var array = new[] { 1, 2, 3, 4, 5 };

    var count = array.AsSpan().Count(99);
    Assert.That(count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Count_Sequence_ReturnsOccurrences() {
    var array = new[] { 1, 2, 1, 2, 1, 2, 3 };
    var pattern = new[] { 1, 2 };

    var count = array.AsSpan().Count(pattern.AsSpan());
    Assert.That(count, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void Count_EmptySequence_ReturnsZero() {
    var array = new[] { 1, 2, 3 };
    var pattern = Array.Empty<int>();

    var count = array.AsSpan().Count(pattern.AsSpan());
    Assert.That(count, Is.EqualTo(0));
  }

  #endregion

  #region MemoryExtensions Reverse

  [Test]
  [Category("HappyPath")]
  public void Reverse_Span_ReversesElements() {
    var array = new[] { 1, 2, 3, 4, 5 };

    MemoryExtensions.Reverse(array.AsSpan());

    Assert.That(array, Is.EqualTo(new[] { 5, 4, 3, 2, 1 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void Reverse_SingleElement_NoChange() {
    var array = new[] { 42 };

    MemoryExtensions.Reverse(array.AsSpan());

    Assert.That(array[0], Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void Reverse_Empty_NoException() {
    var array = Array.Empty<int>();
    MemoryExtensions.Reverse(array.AsSpan());
    Assert.Pass();
  }

  [Test]
  [Category("HappyPath")]
  public void Reverse_EvenCount_Correct() {
    var array = new[] { 1, 2, 3, 4 };

    MemoryExtensions.Reverse(array.AsSpan());

    Assert.That(array, Is.EqualTo(new[] { 4, 3, 2, 1 }));
  }

  #endregion

  #region Memory<T> and ReadOnlyMemory<T>

  [Test]
  [Category("HappyPath")]
  public void Memory_FromArray_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new Memory<int>(array);

    Assert.That(memory.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Memory_Slice_ReturnsSubset() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new Memory<int>(array);

    var sliced = memory.Slice(1, 3);
    Assert.That(sliced.Length, Is.EqualTo(3));
    Assert.That(sliced.ToArray(), Is.EqualTo(new[] { 2, 3, 4 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Memory_Span_CanModifyData() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new Memory<int>(array);

    memory.Span[2] = 99;
    Assert.That(array[2], Is.EqualTo(99));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_FromArray_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new ReadOnlyMemory<int>(array);

    Assert.That(memory.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyMemory_Slice_ReturnsSubset() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = new ReadOnlyMemory<int>(array);

    var sliced = memory.Slice(2, 2);
    Assert.That(sliced.Length, Is.EqualTo(2));
    Assert.That(sliced.ToArray(), Is.EqualTo(new[] { 3, 4 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void Memory_Empty_HasZeroLength() {
    var memory = Memory<int>.Empty;

    Assert.That(memory.Length, Is.EqualTo(0));
    Assert.That(memory.IsEmpty, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadOnlyMemory_Empty_HasZeroLength() {
    var memory = ReadOnlyMemory<int>.Empty;

    Assert.That(memory.Length, Is.EqualTo(0));
    Assert.That(memory.IsEmpty, Is.True);
  }

  #endregion

}
