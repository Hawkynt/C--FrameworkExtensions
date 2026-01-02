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
using System.Linq;
using System.Numerics.Tensors;
using System.Buffers;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Tensor")]
public class TensorTests {

  private const float FloatTolerance = 0.0001f;

  // Helper methods to create nint/NIndex/NRange arrays for shape parameters.
  // Note: This file is excluded from older frameworks (net35-net48, netstandard2.0-2.1) due to
  // Roslyn SDK 10.0 compiler bug - see Backports.Tests.csproj for details.
  private static nint[] I() => [];
  private static nint[] I(nint a) => [a];
  private static nint[] I(nint a, nint b) => [a, b];
  private static nint[] I(nint a, nint b, nint c) => [a, b, c];
  private static nint[] I(nint a, nint b, nint c, nint d) => [a, b, c, d];

  private static NIndex[] NI(NIndex a) => [a];
  private static NIndex[] NI(NIndex a, NIndex b) => [a, b];

  private static NRange[] NR(NRange a, NRange b) => [a, b];

  #region Tensor<T> Construction Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromArray_HasCorrectDimensions() {
    var data = new float[] { 1f, 2f, 3f, 4f, 5f, 6f };
    var tensor = Tensor.Create(data, I(3, 2));

    Assert.That(tensor.Rank, Is.EqualTo(2));
    Assert.That((int)tensor.Lengths[0], Is.EqualTo(3));
    Assert.That((int)tensor.Lengths[1], Is.EqualTo(2));
    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromArrayWithLengths_ContainsCorrectData() {
    var data = new float[] { 1f, 2f, 3f, 4f, 5f, 6f };
    var tensor = Tensor.Create(data, I(2, 3));

    Assert.That(tensor.Rank, Is.EqualTo(2));
    Assert.That(tensor[I(0, 0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(tensor[I(1, 2)], Is.EqualTo(6f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Create1D_HasRankOne() {
    var data = new int[] { 1, 2, 3, 4, 5 };
    var tensor = Tensor.Create(data);

    Assert.That(tensor.Rank, Is.EqualTo(1));
    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(5));
  }

  #endregion

  #region Tensor<T> Indexing Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_IndexByMultiDimensionalIndex_ReturnsCorrectValue() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    Assert.That(tensor[I(0, 0)], Is.EqualTo(1));
    Assert.That(tensor[I(0, 2)], Is.EqualTo(3));
    Assert.That(tensor[I(1, 0)], Is.EqualTo(4));
    Assert.That(tensor[I(1, 2)], Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_SetByIndex_UpdatesValue() {
    var data = new int[] { 0, 0, 0, 0 };
    var tensor = Tensor.Create(data, I(2, 2));

    tensor[I(0, 1)] = 42;
    tensor[I(1, 0)] = 99;

    Assert.That(tensor[I(0, 1)], Is.EqualTo(42));
    Assert.That(tensor[I(1, 0)], Is.EqualTo(99));
  }

  #endregion

  #region Tensor<T> Methods Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Fill_SetsAllElements() {
    var data = new int[9];
    var tensor = Tensor.Create(data, I(3, 3));

    tensor.Fill(7);

    foreach (var value in tensor)
      Assert.That(value, Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Clear_SetsAllElementsToDefault() {
    var data = new int[] { 42, 42, 42, 42 };
    var tensor = Tensor.Create(data, I(2, 2));

    tensor.Clear();

    foreach (var value in tensor)
      Assert.That(value, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_FlattenTo_CopiesDataToSpan() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);
    var destination = new int[3];

    tensor.FlattenTo(destination);

    Assert.That(destination, Is.EqualTo(new int[] { 1, 2, 3 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryFlattenTo_ReturnsTrue_WhenDestinationLargeEnough() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);
    var destination = new int[5];

    var result = tensor.TryFlattenTo(destination);

    Assert.That(result, Is.True);
    Assert.That(destination[0], Is.EqualTo(1));
    Assert.That(destination[1], Is.EqualTo(2));
    Assert.That(destination[2], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryFlattenTo_ReturnsFalse_WhenDestinationTooSmall() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);
    var destination = new int[2];

    var result = tensor.TryFlattenTo(destination);

    Assert.That(result, Is.False);
  }

  #endregion

  #region Tensor<T> Properties Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_IsDense_ReturnsTrue_ForContiguousTensor() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    Assert.That(tensor.IsDense, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_HasAnyDenseDimensions_ReturnsTrue_WhenLastStrideIsOne() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    Assert.That(tensor.HasAnyDenseDimensions, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_IsPinned_ReturnsFalse_ByDefault() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    Assert.That(tensor.IsPinned, Is.False);
  }

  #endregion

  #region Tensor<T> Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Tensor_Empty_HasZeroLength() {
    var tensor = Tensor<int>.Empty;

    Assert.That(tensor.IsEmpty, Is.True);
    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void Tensor_CreateFromData_WrongLength_ThrowsException() {
    var data = new int[] { 1, 2, 3 };

    // Official API throws ArgumentOutOfRangeException, polyfill throws ArgumentException
    Assert.That(() => Tensor.Create(data, I(2, 2)), Throws.InstanceOf<ArgumentException>());
  }

  #endregion

  #region TensorSpan<T> Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_FromArray_HasCorrectShape() {
    var array = new float[] { 1f, 2f, 3f, 4f, 5f, 6f };
    var span = new TensorSpan<float>(array, I(2, 3));

    Assert.That(span.Rank, Is.EqualTo(2));
    Assert.That((int)span.Lengths[0], Is.EqualTo(2));
    Assert.That((int)span.Lengths[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_IndexAccess_ReturnsCorrectValue() {
    var array = new int[] { 10, 20, 30, 40, 50, 60 };
    var span = new TensorSpan<int>(array, I(2, 3));

    Assert.That(span[I(0, 0)], Is.EqualTo(10));
    Assert.That(span[I(1, 2)], Is.EqualTo(60));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_SetValue_ModifiesUnderlyingArray() {
    var array = new int[] { 1, 2, 3, 4 };
    var span = new TensorSpan<int>(array);

    span[I(2)] = 99;

    Assert.That(array[2], Is.EqualTo(99));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_Fill_FillsUnderlyingArray() {
    var array = new int[4];
    var span = new TensorSpan<int>(array);

    span.Fill(42);

    Assert.That(array, Is.EqualTo(new int[] { 42, 42, 42, 42 }));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_Clear_ClearsUnderlyingArray() {
    var array = new int[] { 1, 2, 3, 4 };
    var span = new TensorSpan<int>(array);

    span.Clear();

    Assert.That(array, Is.EqualTo(new int[] { 0, 0, 0, 0 }));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_IsDense_ReturnsTrue_ForContiguousSpan() {
    var array = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(array, I(2, 3));

    Assert.That(span.IsDense, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_FlattenTo_CopiesData() {
    var array = new int[] { 1, 2, 3 };
    var span = new TensorSpan<int>(array);
    var destination = new int[3];

    span.FlattenTo(destination);

    Assert.That(destination, Is.EqualTo(new int[] { 1, 2, 3 }));
  }

  #endregion

  #region ReadOnlyTensorSpan<T> Tests

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_FromArray_HasCorrectShape() {
    var array = new float[] { 1f, 2f, 3f, 4f };
    var span = new ReadOnlyTensorSpan<float>(array, I(2, 2));

    Assert.That(span.Rank, Is.EqualTo(2));
    Assert.That((int)span.FlattenedLength, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_IndexAccess_ReturnsCorrectValue() {
    var array = new int[] { 5, 10, 15, 20 };
    var span = new ReadOnlyTensorSpan<int>(array, I(2, 2));

    Assert.That(span[I(0, 0)], Is.EqualTo(5));
    Assert.That(span[I(0, 1)], Is.EqualTo(10));
    Assert.That(span[I(1, 0)], Is.EqualTo(15));
    Assert.That(span[I(1, 1)], Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_Enumerate_IteratesAllElements() {
    var array = new int[] { 1, 2, 3, 4 };
    var span = new ReadOnlyTensorSpan<int>(array);
    var sum = 0;

    foreach (var value in span)
      sum += value;

    Assert.That(sum, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_IsDense_ReturnsTrue_ForContiguousSpan() {
    var array = new int[] { 1, 2, 3, 4 };
    var span = new ReadOnlyTensorSpan<int>(array, I(2, 2));

    Assert.That(span.IsDense, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_FlattenTo_CopiesData() {
    var array = new int[] { 1, 2, 3 };
    var span = new ReadOnlyTensorSpan<int>(array);
    var destination = new int[3];

    span.FlattenTo(destination);

    Assert.That(destination, Is.EqualTo(new int[] { 1, 2, 3 }));
  }

  #endregion

  #region TensorSpan and Tensor Interop Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsTensorSpan_ReturnsModifiableView() {
    var data = new int[] { 1, 2, 3, 4 };
    var tensor = Tensor.Create(data);
    var span = tensor.AsTensorSpan();

    span[I(0)] = 100;

    Assert.That(tensor[I(0)], Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsReadOnlyTensorSpan_ReturnsReadOnlyView() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);
    var readOnlySpan = tensor.AsReadOnlyTensorSpan();

    Assert.That((int)readOnlySpan.FlattenedLength, Is.EqualTo(3));
    Assert.That(readOnlySpan[I(0)], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_ImplicitConversionToTensorSpan_Works() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    TensorSpan<int> span = tensor;

    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
    Assert.That(span[I(0)], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_ImplicitConversionToReadOnlyTensorSpan_Works() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    ReadOnlyTensorSpan<int> span = tensor;

    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
    Assert.That(span[I(0)], Is.EqualTo(1));
  }

  #endregion

  #region 3D Tensor Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_3D_HasCorrectStrides() {
    var data = new int[24];
    var tensor = Tensor.Create(data, I(2, 3, 4));

    Assert.That(tensor.Rank, Is.EqualTo(3));
    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(24));
    Assert.That((int)tensor.Strides[0], Is.EqualTo(12)); // 3*4
    Assert.That((int)tensor.Strides[1], Is.EqualTo(4));  // 4
    Assert.That((int)tensor.Strides[2], Is.EqualTo(1));  // 1
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_3D_IndexingWorks() {
    var data = Enumerable.Range(0, 24).ToArray();
    var tensor = Tensor.Create(data, I(2, 3, 4));

    Assert.That(tensor[I(1, 2, 3)], Is.EqualTo(23));
    Assert.That(tensor[I(0, 0, 0)], Is.EqualTo(0));
  }

  #endregion

  #region Slice Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Slice_ReturnsCorrectSubTensor() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var slice = tensor.Slice(I(1, 0));

    Assert.That((int)slice.Lengths[0], Is.EqualTo(1));
    Assert.That((int)slice.Lengths[1], Is.EqualTo(3));
  }

  #endregion

  #region GetSpan Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetSpan_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var span = tensor.GetSpan(I(0, 0), 3);

    Assert.That(span.Length, Is.EqualTo(3));
    Assert.That(span[0], Is.EqualTo(1));
    Assert.That(span[1], Is.EqualTo(2));
    Assert.That(span[2], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryGetSpan_ReturnsTrue_WhenValid() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    var result = tensor.TryGetSpan(I(0), 2, out Span<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(2));
  }

  #endregion

  #region ToString Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_ToString_ReturnsDescriptiveString() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var result = tensor.ToString();

    Assert.That(result, Does.Contain("Tensor"));
    Assert.That(result, Does.Contain("Int32"));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_ToString_ReturnsDescriptiveString() {
    var array = new int[] { 1, 2, 3 };
    var span = new TensorSpan<int>(array);

    var result = span.ToString();

    Assert.That(result, Does.Contain("TensorSpan"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_ToString_ReturnsDescriptiveString() {
    var array = new int[] { 1, 2, 3 };
    var span = new ReadOnlyTensorSpan<int>(array);

    var result = span.ToString();

    Assert.That(result, Does.Contain("ReadOnlyTensorSpan"));
  }

  #endregion

  #region Equality Operator Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_EqualityOperator_ReturnsTrueForSameSpan() {
    var array = new int[] { 1, 2, 3 };
    var span1 = new TensorSpan<int>(array);
    var span2 = new TensorSpan<int>(array);

    Assert.That(span1 == span2, Is.True);
    Assert.That(span1 != span2, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_EqualityOperator_ReturnsTrueForSameSpan() {
    var array = new int[] { 1, 2, 3 };
    var span1 = new ReadOnlyTensorSpan<int>(array);
    var span2 = new ReadOnlyTensorSpan<int>(array);

    Assert.That(span1 == span2, Is.True);
    Assert.That(span1 != span2, Is.False);
  }

  #endregion

  #region ToDenseTensor Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_ToDenseTensor_ReturnsSameTensor_WhenAlreadyDense() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    var dense = tensor.ToDenseTensor();

    Assert.That(dense, Is.SameAs(tensor));
  }

  #endregion

  #region Tensor<T> CopyTo and TryCopyTo Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_CopyTo_CopiesDataToTensorSpan() {
    var source = new int[] { 1, 2, 3, 4 };
    var tensor = Tensor.Create(source, I(2, 2));
    var destination = new int[4];
    var destSpan = new TensorSpan<int>(destination, I(2, 2));

    tensor.CopyTo(destSpan);

    Assert.That(destination, Is.EqualTo(new int[] { 1, 2, 3, 4 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryCopyTo_ReturnsTrue_WhenDestinationMatchesShape() {
    var source = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(source);
    var destination = new int[3];
    var destSpan = new TensorSpan<int>(destination);

    var result = tensor.TryCopyTo(destSpan);

    Assert.That(result, Is.True);
    Assert.That(destination[0], Is.EqualTo(1));
    Assert.That(destination[1], Is.EqualTo(2));
    Assert.That(destination[2], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryCopyTo_ReturnsFalse_WhenDestinationTooSmall() {
    var source = new int[] { 1, 2, 3, 4 };
    var tensor = Tensor.Create(source);
    var destination = new int[2];
    var destSpan = new TensorSpan<int>(destination);

    var result = tensor.TryCopyTo(destSpan);

    Assert.That(result, Is.False);
  }

  #endregion

  #region Tensor<T> GetPinnableReference and GetPinnedHandle Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetPinnableReference_ReturnsRefToFirstElement() {
    var data = new int[] { 42, 2, 3 };
    var tensor = Tensor.Create(data);

    ref var first = ref tensor.GetPinnableReference();

    Assert.That(first, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetPinnableReference_AllowsModification() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    ref var first = ref tensor.GetPinnableReference();
    first = 100;

    Assert.That(tensor[I(0)], Is.EqualTo(100));
    Assert.That(data[0], Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetPinnedHandle_ReturnsValidHandle() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    using var handle = tensor.GetPinnedHandle();

    // Just verify handle is valid and pointer is not null
    // IsPinned behavior varies between polyfill and official API
    unsafe {
      Assert.That((IntPtr)handle.Pointer, Is.Not.EqualTo(IntPtr.Zero));
    }
  }

  #endregion

  #region Tensor<T> Implicit Conversion Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_ImplicitFromArray_CreatesTensor() {
    var data = new int[] { 1, 2, 3, 4, 5 };

    Tensor<int> tensor = data;

    Assert.That(tensor.Rank, Is.EqualTo(1));
    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(5));
    Assert.That(tensor[I(0)], Is.EqualTo(1));
    Assert.That(tensor[I(4)], Is.EqualTo(5));
  }

  #endregion

  #region Tensor<T> AsTensorSpan Overload Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsTensorSpan_WithNintStartIndices_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var span = tensor.AsTensorSpan(I(1, 0));

    Assert.That(span[I(0, 0)], Is.EqualTo(4));
  }

  #endregion

  #region Tensor<T> AsReadOnlyTensorSpan Overload Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsReadOnlyTensorSpan_WithNintStartIndices_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var span = tensor.AsReadOnlyTensorSpan(I(1, 0));

    Assert.That(span[I(0, 0)], Is.EqualTo(4));
  }

  #endregion

  #region Tensor<T> GetSpan Overload Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryGetSpan_ReadOnlySpan_WithNint_ReturnsTrue() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    var result = tensor.TryGetSpan(I(0), 2, out ReadOnlySpan<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(2));
  }

  #endregion

  #region TensorSpan<T> Empty and Constructor Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_Empty_HasZeroLength() {
    var span = TensorSpan<int>.Empty;

    Assert.That(span.IsEmpty, Is.True);
    Assert.That((int)span.FlattenedLength, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_ConstructFromSpan_HasCorrectShape() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data.AsSpan(), I(2, 3));

    Assert.That(span.Rank, Is.EqualTo(2));
    Assert.That((int)span.Lengths[0], Is.EqualTo(2));
    Assert.That((int)span.Lengths[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_ConstructFromSpanWithStrides_HasCorrectStrides() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data.AsSpan(), I(2, 3), I(3, 1));

    Assert.That((int)span.Strides[0], Is.EqualTo(3));
    Assert.That((int)span.Strides[1], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_ConstructFromArray_Works() {
    var data = new int[] { 1, 2, 3 };
    var span = new TensorSpan<int>(data);

    Assert.That(span.Rank, Is.EqualTo(1));
    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_ConstructFromArrayWithStartAndLengths_Works() {
    var data = new int[] { 0, 1, 2, 3, 4, 5 };
    var span = new TensorSpan<int>(data, 1, I(2, 2), I());

    Assert.That(span[I(0, 0)], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_IsPinned_ReturnsFalseByDefault() {
    var data = new int[] { 1, 2, 3 };
    var span = new TensorSpan<int>(data);

    Assert.That(span.IsPinned, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_HasAnyDenseDimensions_ReturnsTrueForContiguous() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    Assert.That(span.HasAnyDenseDimensions, Is.True);
  }

  #endregion

  #region TensorSpan<T> CopyTo and TryCopyTo Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_CopyTo_CopiesDataToDestination() {
    var source = new int[] { 1, 2, 3, 4 };
    var sourceSpan = new TensorSpan<int>(source);
    var destination = new int[4];
    var destSpan = new TensorSpan<int>(destination);

    sourceSpan.CopyTo(destSpan);

    Assert.That(destination, Is.EqualTo(new int[] { 1, 2, 3, 4 }));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_TryCopyTo_ReturnsTrue_WhenDestinationMatchesShape() {
    var source = new int[] { 1, 2, 3 };
    var sourceSpan = new TensorSpan<int>(source);
    var destination = new int[3];
    var destSpan = new TensorSpan<int>(destination);

    var result = sourceSpan.TryCopyTo(destSpan);

    Assert.That(result, Is.True);
    Assert.That(destination[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_TryCopyTo_ReturnsFalse_WhenDestinationTooSmall() {
    var source = new int[] { 1, 2, 3, 4 };
    var sourceSpan = new TensorSpan<int>(source);
    var destination = new int[2];
    var destSpan = new TensorSpan<int>(destination);

    var result = sourceSpan.TryCopyTo(destSpan);

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_TryFlattenTo_ReturnsTrue_WhenDestinationLargeEnough() {
    var source = new int[] { 1, 2, 3 };
    var sourceSpan = new TensorSpan<int>(source);
    var destination = new int[5];

    var result = sourceSpan.TryFlattenTo(destination);

    Assert.That(result, Is.True);
    Assert.That(destination[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_TryFlattenTo_ReturnsFalse_WhenDestinationTooSmall() {
    var source = new int[] { 1, 2, 3, 4 };
    var sourceSpan = new TensorSpan<int>(source);
    var destination = new int[2];

    var result = sourceSpan.TryFlattenTo(destination);

    Assert.That(result, Is.False);
  }

  #endregion

  #region TensorSpan<T> GetSpan and TryGetSpan Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_GetSpan_WithNint_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensorSpan = new TensorSpan<int>(data, I(2, 3));

    var span = tensorSpan.GetSpan(I(0, 0), 3);

    Assert.That(span.Length, Is.EqualTo(3));
    Assert.That(span[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_TryGetSpan_ReturnsTrue_WhenValid() {
    var data = new int[] { 1, 2, 3 };
    var tensorSpan = new TensorSpan<int>(data);

    var result = tensorSpan.TryGetSpan(I(0), 2, out Span<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(2));
  }

  #endregion

  #region TensorSpan<T> AsReadOnlyTensorSpan Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_AsReadOnlyTensorSpan_ReturnsReadOnlyView() {
    var data = new int[] { 1, 2, 3 };
    var span = new TensorSpan<int>(data);

    var readOnly = span.AsReadOnlyTensorSpan();

    Assert.That((int)readOnly.FlattenedLength, Is.EqualTo(3));
    Assert.That(readOnly[I(0)], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_AsReadOnlyTensorSpan_WithStartIndices_ReturnsCorrectView() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    var readOnly = span.AsReadOnlyTensorSpan(I(1, 0));

    Assert.That(readOnly[I(0, 0)], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_ImplicitConversionFromArray_Works() {
    var data = new int[] { 1, 2, 3 };

    TensorSpan<int> span = data;

    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_ImplicitConversionToReadOnlyTensorSpan_Works() {
    var data = new int[] { 1, 2, 3 };
    var span = new TensorSpan<int>(data);

    var readOnly = span.AsReadOnlyTensorSpan();

    Assert.That((int)readOnly.FlattenedLength, Is.EqualTo(3));
  }

  #endregion

  #region ReadOnlyTensorSpan<T> Empty and Constructor Tests

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_Empty_HasZeroLength() {
    var span = ReadOnlyTensorSpan<int>.Empty;

    Assert.That(span.IsEmpty, Is.True);
    Assert.That((int)span.FlattenedLength, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_ConstructFromArray_Works() {
    var data = new int[] { 1, 2, 3 };
    var span = new ReadOnlyTensorSpan<int>(data);

    Assert.That(span.Rank, Is.EqualTo(1));
    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_ConstructFromArrayWithLengths_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    Assert.That(span.Rank, Is.EqualTo(2));
    Assert.That((int)span.Lengths[0], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_ConstructFromArrayWithStrides_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3), I(3, 1));

    Assert.That((int)span.Strides[0], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_IsPinned_ReturnsFalseByDefault() {
    var data = new int[] { 1, 2, 3 };
    var span = new ReadOnlyTensorSpan<int>(data);

    Assert.That(span.IsPinned, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_HasAnyDenseDimensions_ReturnsTrueForContiguous() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    Assert.That(span.HasAnyDenseDimensions, Is.True);
  }

  #endregion

  #region ReadOnlyTensorSpan<T> CopyTo and TryCopyTo Tests

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_CopyTo_CopiesDataToDestination() {
    var source = new int[] { 1, 2, 3, 4 };
    var sourceSpan = new ReadOnlyTensorSpan<int>(source);
    var destination = new int[4];
    var destSpan = new TensorSpan<int>(destination);

    sourceSpan.CopyTo(destSpan);

    Assert.That(destination, Is.EqualTo(new int[] { 1, 2, 3, 4 }));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_TryCopyTo_ReturnsTrue_WhenDestinationMatchesShape() {
    var source = new int[] { 1, 2, 3 };
    var sourceSpan = new ReadOnlyTensorSpan<int>(source);
    var destination = new int[3];
    var destSpan = new TensorSpan<int>(destination);

    var result = sourceSpan.TryCopyTo(destSpan);

    Assert.That(result, Is.True);
    Assert.That(destination[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_TryCopyTo_ReturnsFalse_WhenDestinationTooSmall() {
    var source = new int[] { 1, 2, 3, 4 };
    var sourceSpan = new ReadOnlyTensorSpan<int>(source);
    var destination = new int[2];
    var destSpan = new TensorSpan<int>(destination);

    var result = sourceSpan.TryCopyTo(destSpan);

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_TryFlattenTo_ReturnsTrue_WhenDestinationLargeEnough() {
    var source = new int[] { 1, 2, 3 };
    var sourceSpan = new ReadOnlyTensorSpan<int>(source);
    var destination = new int[5];

    var result = sourceSpan.TryFlattenTo(destination);

    Assert.That(result, Is.True);
    Assert.That(destination[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_TryFlattenTo_ReturnsFalse_WhenDestinationTooSmall() {
    var source = new int[] { 1, 2, 3, 4 };
    var sourceSpan = new ReadOnlyTensorSpan<int>(source);
    var destination = new int[2];

    var result = sourceSpan.TryFlattenTo(destination);

    Assert.That(result, Is.False);
  }

  #endregion

  #region ReadOnlyTensorSpan<T> Slice and GetSpan Tests

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_GetSpan_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensorSpan = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var span = tensorSpan.GetSpan(I(0, 0), 3);

    Assert.That(span.Length, Is.EqualTo(3));
    Assert.That(span[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_TryGetSpan_ReturnsTrue_WhenValid() {
    var data = new int[] { 1, 2, 3 };
    var tensorSpan = new ReadOnlyTensorSpan<int>(data);

    var result = tensorSpan.TryGetSpan(I(0), 2, out ReadOnlySpan<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(2));
  }

  #endregion

  #region ReadOnlyTensorSpan<T> Conversion Tests

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_ImplicitConversionFromArray_Works() {
    var data = new int[] { 1, 2, 3 };

    ReadOnlyTensorSpan<int> span = data;

    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
  }

  #endregion

  #region Factory Method Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Create_WithStartAndStrides_Works() {
    var data = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    var tensor = Tensor.Create(data, 1, I(2, 3), I(3, 1));

    Assert.That(tensor[I(0, 0)], Is.EqualTo(1));
    Assert.That(tensor[I(1, 0)], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Create_WithCustomStrides_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3), I(3, 1));

    Assert.That((int)tensor.Strides[0], Is.EqualTo(3));
    Assert.That((int)tensor.Strides[1], Is.EqualTo(1));
  }

  #endregion

  #region Exception Tests

  [Test]
  [Category("Exception")]
  public void Tensor_IndexOutOfRange_ThrowsException() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    Assert.That(() => _ = tensor[I(5)], Throws.InstanceOf<Exception>());
  }

  [Test]
  [Category("Exception")]
  public void Tensor_WrongRankIndex_ThrowsException() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    Assert.That(() => _ = tensor[I(0)], Throws.InstanceOf<ArgumentException>());
  }

  [Test]
  [Category("Exception")]
  public void TensorSpan_WrongRankIndex_ThrowsException() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };

    Assert.That(() => {
      var span = new TensorSpan<int>(data, I(2, 3));
      _ = span[I(0)];
    }, Throws.InstanceOf<ArgumentException>());
  }

  [Test]
  [Category("Exception")]
  public void ReadOnlyTensorSpan_WrongRankIndex_ThrowsException() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };

    Assert.That(() => {
      var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));
      _ = span[I(0)];
    }, Throws.InstanceOf<ArgumentException>());
  }

  #endregion

  #region NIndex Tests

  [Test]
  [Category("HappyPath")]
  public void NIndex_Constructor_WithValue_CreatesFromStart() {
    var index = new NIndex(5, fromEnd: false);

    Assert.That(index.Value, Is.EqualTo((nint)5));
    Assert.That(index.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_Constructor_WithValueFromEnd_CreatesFromEnd() {
    var index = new NIndex(3, fromEnd: true);

    Assert.That(index.Value, Is.EqualTo((nint)3));
    Assert.That(index.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_Constructor_FromIndex_ConvertsCorrectly() {
    var systemIndex = new Index(2, fromEnd: false);
    var nindex = new NIndex(systemIndex);

    Assert.That(nindex.Value, Is.EqualTo((nint)2));
    Assert.That(nindex.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_Constructor_FromEndIndex_ConvertsCorrectly() {
    var systemIndex = new Index(1, fromEnd: true);
    var nindex = new NIndex(systemIndex);

    Assert.That(nindex.Value, Is.EqualTo((nint)1));
    Assert.That(nindex.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_Start_ReturnsZeroIndex() {
    var start = NIndex.Start;

    Assert.That(start.Value, Is.EqualTo((nint)0));
    Assert.That(start.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_End_ReturnsFromEndZeroIndex() {
    var end = NIndex.End;

    Assert.That(end.Value, Is.EqualTo((nint)0));
    Assert.That(end.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_FromStart_CreatesCorrectIndex() {
    var index = NIndex.FromStart(7);

    Assert.That(index.Value, Is.EqualTo((nint)7));
    Assert.That(index.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_FromEnd_CreatesCorrectIndex() {
    var index = NIndex.FromEnd(2);

    Assert.That(index.Value, Is.EqualTo((nint)2));
    Assert.That(index.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_GetOffset_FromStart_CalculatesCorrectly() {
    var index = new NIndex(3, fromEnd: false);

    var offset = index.GetOffset(10);

    Assert.That(offset, Is.EqualTo((nint)3));
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_GetOffset_FromEnd_CalculatesCorrectly() {
    var index = new NIndex(2, fromEnd: true);

    var offset = index.GetOffset(10);

    Assert.That(offset, Is.EqualTo((nint)8));
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_ToIndex_ConvertsCorrectly() {
    var nindex = new NIndex(5, fromEnd: false);

    var index = nindex.ToIndex();

    Assert.That(index.Value, Is.EqualTo(5));
    Assert.That(index.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_ToIndex_FromEnd_ConvertsCorrectly() {
    var nindex = new NIndex(3, fromEnd: true);

    var index = nindex.ToIndex();

    Assert.That(index.Value, Is.EqualTo(3));
    Assert.That(index.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_ToIndexUnchecked_ConvertsCorrectly() {
    var nindex = new NIndex(5, fromEnd: false);

    var index = nindex.ToIndexUnchecked();

    Assert.That(index.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_ImplicitFromNint_Works() {
    NIndex index = (nint)5;

    Assert.That(index.Value, Is.EqualTo((nint)5));
    Assert.That(index.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_ImplicitFromIndex_Works() {
    NIndex nindex = new Index(3);

    Assert.That(nindex.Value, Is.EqualTo((nint)3));
    Assert.That(nindex.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_ExplicitToIndex_Works() {
    var nindex = new NIndex(4, fromEnd: false);

    var index = (Index)nindex;

    Assert.That(index.Value, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_Equals_ReturnsTrueForSameValue() {
    var index1 = new NIndex(5, fromEnd: false);
    var index2 = new NIndex(5, fromEnd: false);

    Assert.That(index1.Equals(index2), Is.True);
    Assert.That(index1.Equals((object)index2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_Equals_ReturnsFalseForDifferentValue() {
    var index1 = new NIndex(5, fromEnd: false);
    var index2 = new NIndex(5, fromEnd: true);

    Assert.That(index1.Equals(index2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_GetHashCode_SameForEqualValues() {
    var index1 = new NIndex(5, fromEnd: false);
    var index2 = new NIndex(5, fromEnd: false);

    Assert.That(index1.GetHashCode(), Is.EqualTo(index2.GetHashCode()));
  }

  [Test]
  [Category("HappyPath")]
  public void NIndex_ToString_ReturnsExpectedFormat() {
    var indexFromStart = new NIndex(5, fromEnd: false);
    var indexFromEnd = new NIndex(3, fromEnd: true);

    var strFromStart = indexFromStart.ToString();
    var strFromEnd = indexFromEnd.ToString();

    Assert.That(strFromStart, Does.Contain("5"));
    Assert.That(strFromEnd, Does.Contain("3"));
  }

  #endregion

  #region NRange Tests

  [Test]
  [Category("HappyPath")]
  public void NRange_Constructor_WithStartEnd_CreatesRange() {
    var start = new NIndex(2, fromEnd: false);
    var end = new NIndex(5, fromEnd: false);
    var range = new NRange(start, end);

    Assert.That(range.Start.Value, Is.EqualTo((nint)2));
    Assert.That(range.End.Value, Is.EqualTo((nint)5));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_Constructor_FromRange_ConvertsCorrectly() {
    var systemRange = new Range(1, 4);
    var nrange = new NRange(systemRange);

    Assert.That(nrange.Start.Value, Is.EqualTo((nint)1));
    Assert.That(nrange.End.Value, Is.EqualTo((nint)4));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_All_ReturnsFullRange() {
    var all = NRange.All;

    Assert.That(all.Start.Value, Is.EqualTo((nint)0));
    Assert.That(all.Start.IsFromEnd, Is.False);
    Assert.That(all.End.Value, Is.EqualTo((nint)0));
    Assert.That(all.End.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_StartAt_CreatesCorrectRange() {
    var range = NRange.StartAt(new NIndex(3, fromEnd: false));

    Assert.That(range.Start.Value, Is.EqualTo((nint)3));
    Assert.That(range.End.IsFromEnd, Is.True);
    Assert.That(range.End.Value, Is.EqualTo((nint)0));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_EndAt_CreatesCorrectRange() {
    var range = NRange.EndAt(new NIndex(5, fromEnd: false));

    Assert.That(range.Start.Value, Is.EqualTo((nint)0));
    Assert.That(range.End.Value, Is.EqualTo((nint)5));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_GetOffsetAndLength_CalculatesCorrectly() {
    var range = new NRange(new NIndex(2, fromEnd: false), new NIndex(7, fromEnd: false));

    var (offset, length) = range.GetOffsetAndLength(10);

    Assert.That(offset, Is.EqualTo((nint)2));
    Assert.That(length, Is.EqualTo((nint)5));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_GetOffsetAndLength_WithFromEnd_CalculatesCorrectly() {
    var range = new NRange(new NIndex(2, fromEnd: true), new NIndex(1, fromEnd: true));

    var (offset, length) = range.GetOffsetAndLength(10);

    Assert.That(offset, Is.EqualTo((nint)8));
    Assert.That(length, Is.EqualTo((nint)1));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_ToRange_ConvertsCorrectly() {
    var nrange = new NRange(new NIndex(1, fromEnd: false), new NIndex(4, fromEnd: false));

    var range = nrange.ToRange();

    Assert.That(range.Start.Value, Is.EqualTo(1));
    Assert.That(range.End.Value, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_ToRangeUnchecked_ConvertsCorrectly() {
    var nrange = new NRange(new NIndex(1, fromEnd: false), new NIndex(4, fromEnd: false));

    var range = nrange.ToRangeUnchecked();

    Assert.That(range.Start.Value, Is.EqualTo(1));
    Assert.That(range.End.Value, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_ImplicitFromRange_Works() {
    NRange nrange = new Range(2, 6);

    Assert.That(nrange.Start.Value, Is.EqualTo((nint)2));
    Assert.That(nrange.End.Value, Is.EqualTo((nint)6));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_ExplicitToRange_Works() {
    var nrange = new NRange(new NIndex(1, fromEnd: false), new NIndex(5, fromEnd: false));

    var range = (Range)nrange;

    Assert.That(range.Start.Value, Is.EqualTo(1));
    Assert.That(range.End.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_Equals_ReturnsTrueForSameValues() {
    var range1 = new NRange(new NIndex(1, fromEnd: false), new NIndex(5, fromEnd: false));
    var range2 = new NRange(new NIndex(1, fromEnd: false), new NIndex(5, fromEnd: false));

    Assert.That(range1.Equals(range2), Is.True);
    Assert.That(range1.Equals((object)range2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_Equals_ReturnsFalseForDifferentValues() {
    var range1 = new NRange(new NIndex(1, fromEnd: false), new NIndex(5, fromEnd: false));
    var range2 = new NRange(new NIndex(2, fromEnd: false), new NIndex(5, fromEnd: false));

    Assert.That(range1.Equals(range2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_GetHashCode_SameForEqualValues() {
    var range1 = new NRange(new NIndex(1, fromEnd: false), new NIndex(5, fromEnd: false));
    var range2 = new NRange(new NIndex(1, fromEnd: false), new NIndex(5, fromEnd: false));

    Assert.That(range1.GetHashCode(), Is.EqualTo(range2.GetHashCode()));
  }

  [Test]
  [Category("HappyPath")]
  public void NRange_ToString_ReturnsExpectedFormat() {
    var range = new NRange(new NIndex(2, fromEnd: false), new NIndex(5, fromEnd: false));

    var str = range.ToString();

    Assert.That(str, Is.Not.Null.And.Not.Empty);
  }

  #endregion

  #region Tensor Factory - CreateFromShape Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromShape_CreatesWithDefaultValues() {
    var tensor = Tensor.CreateFromShape<int>(I(2, 3));

    Assert.That(tensor.Rank, Is.EqualTo(2));
    Assert.That((int)tensor.Lengths[0], Is.EqualTo(2));
    Assert.That((int)tensor.Lengths[1], Is.EqualTo(3));
    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(6));
    foreach (var value in tensor)
      Assert.That(value, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromShape_WithStrides_CreatesCorrectly() {
    var tensor = Tensor.CreateFromShape<int>(I(2, 3), I(3, 1));

    Assert.That((int)tensor.Strides[0], Is.EqualTo(3));
    Assert.That((int)tensor.Strides[1], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromShape_Pinned_IsPinned() {
    var tensor = Tensor.CreateFromShape<int>(I(2, 3), pinned: true);

    Assert.That(tensor.IsPinned, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromShape_3D_CreatesCorrectShape() {
    var tensor = Tensor.CreateFromShape<float>(I(2, 3, 4));

    Assert.That(tensor.Rank, Is.EqualTo(3));
    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(24));
  }

  #endregion

  #region Tensor Factory - CreateFromShapeUninitialized Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromShapeUninitialized_CreatesWithShape() {
    var tensor = Tensor.CreateFromShapeUninitialized<int>(I(2, 3));

    Assert.That(tensor.Rank, Is.EqualTo(2));
    Assert.That((int)tensor.Lengths[0], Is.EqualTo(2));
    Assert.That((int)tensor.Lengths[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromShapeUninitialized_WithStrides_CreatesCorrectly() {
    var tensor = Tensor.CreateFromShapeUninitialized<int>(I(2, 3), I(3, 1));

    Assert.That((int)tensor.Strides[0], Is.EqualTo(3));
    Assert.That((int)tensor.Strides[1], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CreateFromShapeUninitialized_Pinned_IsPinned() {
    var tensor = Tensor.CreateFromShapeUninitialized<int>(I(2, 3), pinned: true);

    Assert.That(tensor.IsPinned, Is.True);
  }

  #endregion

  #region Tensor Factory - AsTensorSpan Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsTensorSpan_FromArray_Works() {
    var data = new int[] { 1, 2, 3 };
    var span = Tensor.AsTensorSpan(data);

    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
    Assert.That(span[I(0)], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsTensorSpan_WithLengths_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = Tensor.AsTensorSpan(data, I(2, 3));

    Assert.That(span.Rank, Is.EqualTo(2));
    Assert.That(span[I(0, 0)], Is.EqualTo(1));
    Assert.That(span[I(1, 2)], Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsTensorSpan_WithStrides_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = Tensor.AsTensorSpan(data, I(2, 3), I(3, 1));

    Assert.That((int)span.Strides[0], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsTensorSpan_WithStart_Works() {
    var data = new int[] { 0, 1, 2, 3, 4, 5, 6 };
    var span = Tensor.AsTensorSpan(data, 1, I(2, 3), I(3, 1));

    Assert.That(span[I(0, 0)], Is.EqualTo(1));
  }

  #endregion

  #region Tensor Factory - AsReadOnlyTensorSpan Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsReadOnlyTensorSpan_FromArray_Works() {
    var data = new int[] { 1, 2, 3 };
    var span = Tensor.AsReadOnlyTensorSpan(data);

    Assert.That((int)span.FlattenedLength, Is.EqualTo(3));
    Assert.That(span[I(0)], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsReadOnlyTensorSpan_WithLengths_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = Tensor.AsReadOnlyTensorSpan(data, I(2, 3));

    Assert.That(span.Rank, Is.EqualTo(2));
    Assert.That(span[I(1, 2)], Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsReadOnlyTensorSpan_WithStrides_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = Tensor.AsReadOnlyTensorSpan(data, I(2, 3), I(3, 1));

    Assert.That((int)span.Strides[0], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Static_AsReadOnlyTensorSpan_WithStart_Works() {
    var data = new int[] { 0, 1, 2, 3, 4, 5, 6 };
    var span = Tensor.AsReadOnlyTensorSpan(data, 1, I(2, 3), I(3, 1));

    Assert.That(span[I(0, 0)], Is.EqualTo(1));
  }

  #endregion

  #region GetDimensionSpan Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetDimensionSpan_Dimension0_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var dimensionSpan = tensor.GetDimensionSpan(0);

    Assert.That((int)dimensionSpan.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetDimensionSpan_Dimension1_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var dimensionSpan = tensor.GetDimensionSpan(1);

    // GetDimensionSpan(1) on a [2,3] tensor returns product of lengths[0..1] = 2*3 = 6
    Assert.That((int)dimensionSpan.Length, Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetDimensionSpan_IsDense_ReturnsTrue() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var dimensionSpan = tensor.GetDimensionSpan(0);

    Assert.That(dimensionSpan.IsDense, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetDimensionSpan_Indexer_ReturnsSlice() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var dimensionSpan = tensor.GetDimensionSpan(0);
    var slice = dimensionSpan[1];

    Assert.That(slice[I(0)], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_GetDimensionSpan_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    var dimensionSpan = span.GetDimensionSpan(0);

    Assert.That((int)dimensionSpan.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_GetDimensionSpan_CanEnumerate() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    var dimensionSpan = span.GetDimensionSpan(0);
    var count = 0;
    foreach (var slice in dimensionSpan)
      ++count;

    Assert.That(count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_GetDimensionSpan_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var dimensionSpan = span.GetDimensionSpan(0);

    Assert.That((int)dimensionSpan.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_GetDimensionSpan_CanEnumerate() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var dimensionSpan = span.GetDimensionSpan(0);
    var count = 0;
    foreach (var slice in dimensionSpan)
      ++count;

    Assert.That(count, Is.EqualTo(2));
  }

  [Test]
  [Category("Exception")]
  public void Tensor_GetDimensionSpan_InvalidDimension_Throws() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    Assert.That(() => tensor.GetDimensionSpan(5), Throws.InstanceOf<Exception>());
  }

  [Test]
  [Category("Exception")]
  public void Tensor_GetDimensionSpan_NegativeDimension_Throws() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    Assert.That(() => tensor.GetDimensionSpan(-1), Throws.InstanceOf<Exception>());
  }

  #endregion

  #region Tensor NIndex Indexer Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_NIndexIndexer_ReturnsCorrectElement() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var indices = NI(new NIndex(0, fromEnd: false), new NIndex(1, fromEnd: false));
    var value = tensor[indices];

    Assert.That(value, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_NIndexIndexer_FromEnd_ReturnsCorrectElement() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    // For tensor [2,3] with data [1,2,3,4,5,6]:
    // Row 0: [1, 2, 3], Row 1: [4, 5, 6]
    // ^1 for dim 0 (length 2): 2-1=1 → row 1
    // ^1 for dim 1 (length 3): 3-1=2 → column 2
    // tensor[1, 2] = 6
    var indices = NI(new NIndex(1, fromEnd: true), new NIndex(1, fromEnd: true));
    var value = tensor[indices];

    Assert.That(value, Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_NIndexIndexer_ReturnsCorrectElement() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    var indices = NI(new NIndex(1, fromEnd: false), new NIndex(0, fromEnd: false));
    var value = span[indices];

    Assert.That(value, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_NIndexIndexer_ReturnsCorrectElement() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var indices = NI(new NIndex(0, fromEnd: false), new NIndex(2, fromEnd: false));
    var value = span[indices];

    Assert.That(value, Is.EqualTo(3));
  }

  #endregion

  #region Tensor NRange Indexer Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_NRangeIndexer_ReturnsSlice() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var ranges = NR(NRange.All, new NRange(new NIndex(0, fromEnd: false), new NIndex(2, fromEnd: false)));
    var slice = tensor[ranges];

    Assert.That((int)slice.Lengths[0], Is.EqualTo(2));
    Assert.That((int)slice.Lengths[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_NRangeIndexer_ReturnsSlice() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    var ranges = NR(new NRange(new NIndex(0, fromEnd: false), new NIndex(1, fromEnd: false)), NRange.All);
    var slice = span[ranges];

    Assert.That((int)slice.Lengths[0], Is.EqualTo(1));
    Assert.That((int)slice.Lengths[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_NRangeIndexer_ReturnsSlice() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var ranges = NR(NRange.All, NRange.All);
    var slice = span[ranges];

    Assert.That((int)slice.FlattenedLength, Is.EqualTo(6));
  }

  #endregion

  #region Slice with NIndex/NRange Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Slice_WithNIndex_ReturnsCorrectSlice() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var startIndices = NI(new NIndex(1, fromEnd: false), new NIndex(0, fromEnd: false));
    var slice = tensor.Slice(startIndices);

    Assert.That(slice[I(0, 0)], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Slice_WithNRange_ReturnsCorrectSlice() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var ranges = NR(NRange.All, new NRange(new NIndex(1, fromEnd: false), new NIndex(3, fromEnd: false)));
    var slice = tensor.Slice(ranges);

    Assert.That((int)slice.Lengths[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_Slice_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    var startIndices = NI(new NIndex(0, fromEnd: false), new NIndex(1, fromEnd: false));
    var slice = span.Slice(startIndices);

    Assert.That(slice[I(0, 0)], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_Slice_WithNRange_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new TensorSpan<int>(data, I(2, 3));

    var ranges = NR(new NRange(new NIndex(1, fromEnd: false), new NIndex(2, fromEnd: false)), NRange.All);
    var slice = span.Slice(ranges);

    Assert.That((int)slice.Lengths[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_Slice_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var startIndices = NI(new NIndex(1, fromEnd: false), new NIndex(2, fromEnd: false));
    var slice = span.Slice(startIndices);

    Assert.That(slice[I(0, 0)], Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_Slice_WithNRange_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var span = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var ranges = NR(NRange.All, new NRange(new NIndex(0, fromEnd: false), new NIndex(2, fromEnd: false)));
    var slice = span.Slice(ranges);

    Assert.That((int)slice.Lengths[1], Is.EqualTo(2));
  }

  #endregion

  #region AsTensorSpan/AsReadOnlyTensorSpan with NIndex/NRange Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsTensorSpan_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var startIndices = NI(new NIndex(1, fromEnd: false), new NIndex(0, fromEnd: false));
    var span = tensor.AsTensorSpan(startIndices);

    Assert.That(span[I(0, 0)], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsTensorSpan_WithNRange_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var ranges = NR(NRange.All, new NRange(new NIndex(1, fromEnd: false), new NIndex(3, fromEnd: false)));
    var span = tensor.AsTensorSpan(ranges);

    Assert.That((int)span.Lengths[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsReadOnlyTensorSpan_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var startIndices = NI(new NIndex(0, fromEnd: false), new NIndex(1, fromEnd: false));
    var span = tensor.AsReadOnlyTensorSpan(startIndices);

    Assert.That(span[I(0, 0)], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsReadOnlyTensorSpan_WithNRange_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var ranges = NR(new NRange(new NIndex(1, fromEnd: false), new NIndex(2, fromEnd: false)), NRange.All);
    var span = tensor.AsReadOnlyTensorSpan(ranges);

    Assert.That((int)span.Lengths[0], Is.EqualTo(1));
  }

  #endregion

  #region TensorSpan AsReadOnlyTensorSpan with NIndex/NRange Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_AsReadOnlyTensorSpan_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensorSpan = new TensorSpan<int>(data, I(2, 3));

    var startIndices = NI(new NIndex(1, fromEnd: false), new NIndex(1, fromEnd: false));
    var span = tensorSpan.AsReadOnlyTensorSpan(startIndices);

    Assert.That(span[I(0, 0)], Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_AsReadOnlyTensorSpan_WithNRange_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensorSpan = new TensorSpan<int>(data, I(2, 3));

    var ranges = NR(NRange.All, new NRange(new NIndex(0, fromEnd: false), new NIndex(1, fromEnd: false)));
    var span = tensorSpan.AsReadOnlyTensorSpan(ranges);

    Assert.That((int)span.Lengths[1], Is.EqualTo(1));
  }

  #endregion

  #region GetSpan/TryGetSpan with NIndex Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_GetSpan_WithNIndex_ReturnsCorrectSpan() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensor = Tensor.Create(data, I(2, 3));

    var indices = NI(new NIndex(0, fromEnd: false), new NIndex(0, fromEnd: false));
    var span = tensor.GetSpan(indices, 3);

    Assert.That(span.Length, Is.EqualTo(3));
    Assert.That(span[0], Is.EqualTo(1));
    Assert.That(span[1], Is.EqualTo(2));
    Assert.That(span[2], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryGetSpan_WithNIndex_ReturnsTrue() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    var indices = NI(new NIndex(0, fromEnd: false));
    var result = tensor.TryGetSpan(indices, 2, out Span<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryGetSpan_ReadOnly_WithNIndex_ReturnsTrue() {
    var data = new int[] { 1, 2, 3 };
    var tensor = Tensor.Create(data);

    var indices = NI(new NIndex(1, fromEnd: false));
    var result = tensor.TryGetSpan(indices, 2, out ReadOnlySpan<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(2));
    Assert.That(span[0], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_GetSpan_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensorSpan = new TensorSpan<int>(data, I(2, 3));

    var indices = NI(new NIndex(1, fromEnd: false), new NIndex(0, fromEnd: false));
    var span = tensorSpan.GetSpan(indices, 2);

    Assert.That(span.Length, Is.EqualTo(2));
    Assert.That(span[0], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_TryGetSpan_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3 };
    var tensorSpan = new TensorSpan<int>(data);

    var indices = NI(new NIndex(0, fromEnd: false));
    var result = tensorSpan.TryGetSpan(indices, 2, out Span<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_GetSpan_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3, 4, 5, 6 };
    var tensorSpan = new ReadOnlyTensorSpan<int>(data, I(2, 3));

    var indices = NI(new NIndex(0, fromEnd: false), new NIndex(1, fromEnd: false));
    var span = tensorSpan.GetSpan(indices, 2);

    Assert.That(span.Length, Is.EqualTo(2));
    Assert.That(span[0], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyTensorSpan_TryGetSpan_WithNIndex_Works() {
    var data = new int[] { 1, 2, 3 };
    var tensorSpan = new ReadOnlyTensorSpan<int>(data);

    var indices = NI(new NIndex(1, fromEnd: false));
    var result = tensorSpan.TryGetSpan(indices, 1, out ReadOnlySpan<int> span);

    Assert.That(result, Is.True);
    Assert.That(span.Length, Is.EqualTo(1));
    Assert.That(span[0], Is.EqualTo(2));
  }

  #endregion

  #region Tensor Mathematical Operations - Add Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Add_TwoTensors_ReturnsSum() {
    var a = Tensor.Create(new float[] { 1, 2, 3 });
    var b = Tensor.Create(new float[] { 4, 5, 6 });

    var result = Tensor.Add<float>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(7f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(9f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Add_TensorAndScalar_ReturnsSum() {
    var tensor = Tensor.Create(new float[] { 1, 2, 3 });

    var result = Tensor.Add<float>(tensor, 10f);

    Assert.That(result[I(0)], Is.EqualTo(11f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(12f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(13f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Add_WithDestination_WritesToDestination() {
    var a = Tensor.Create(new float[] { 1, 2, 3 });
    var b = Tensor.Create(new float[] { 4, 5, 6 });
    var dest = new float[3];
    var destSpan = new TensorSpan<float>(dest);

    Tensor.Add<float>(a, b, destSpan);

    Assert.That(dest[0], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(dest[1], Is.EqualTo(7f).Within(FloatTolerance));
    Assert.That(dest[2], Is.EqualTo(9f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Add_2DTensors_Works() {
    var a = Tensor.Create([1, 2, 3, 4], I(2, 2));
    var b = Tensor.Create([10, 20, 30, 40], I(2, 2));

    var result = Tensor.Add<int>(a, b);

    Assert.That(result[I(0, 0)], Is.EqualTo(11));
    Assert.That(result[I(1, 1)], Is.EqualTo(44));
  }

  #endregion

  #region Tensor Mathematical Operations - Subtract Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Subtract_TwoTensors_ReturnsDifference() {
    var a = Tensor.Create(new float[] { 10, 20, 30 });
    var b = Tensor.Create(new float[] { 1, 2, 3 });

    var result = Tensor.Subtract<float>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(9f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(18f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(27f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Subtract_TensorAndScalar_ReturnsDifference() {
    var tensor = Tensor.Create(new float[] { 10, 20, 30 });

    var result = Tensor.Subtract<float>(tensor, 5f);

    Assert.That(result[I(0)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(15f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(25f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Subtract_ScalarMinusTensor_ReturnsDifference() {
    var tensor = Tensor.Create(new float[] { 1, 2, 3 });

    var result = Tensor.Subtract<float>(10f, tensor);

    Assert.That(result[I(0)], Is.EqualTo(9f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(8f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(7f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Mathematical Operations - Multiply Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Multiply_TwoTensors_ReturnsProduct() {
    var a = Tensor.Create(new float[] { 2, 3, 4 });
    var b = Tensor.Create(new float[] { 5, 6, 7 });

    var result = Tensor.Multiply<float>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(18f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(28f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Multiply_TensorAndScalar_ReturnsProduct() {
    var tensor = Tensor.Create(new float[] { 1, 2, 3 });

    var result = Tensor.Multiply<float>(tensor, 3f);

    Assert.That(result[I(0)], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(6f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(9f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Mathematical Operations - Divide Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Divide_TwoTensors_ReturnsQuotient() {
    var a = Tensor.Create(new float[] { 10, 20, 30 });
    var b = Tensor.Create(new float[] { 2, 4, 5 });

    var result = Tensor.Divide<float>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(6f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Divide_TensorByScalar_ReturnsQuotient() {
    var tensor = Tensor.Create(new float[] { 10, 20, 30 });

    var result = Tensor.Divide<float>(tensor, 2f);

    Assert.That(result[I(0)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(15f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Divide_ScalarByTensor_ReturnsQuotient() {
    var tensor = Tensor.Create(new float[] { 2, 4, 5 });

    var result = Tensor.Divide<float>(100f, tensor);

    Assert.That(result[I(0)], Is.EqualTo(50f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(25f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(20f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Mathematical Operations - Negate Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Negate_ReturnsNegatedValues() {
    var tensor = Tensor.Create(new float[] { 1, -2, 3, -4 });

    var result = Tensor.Negate<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(-1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(-3f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(4f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Mathematical Operations - Abs Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Abs_ReturnsAbsoluteValues() {
    var tensor = Tensor.Create(new float[] { -1, 2, -3, 4 });

    var result = Tensor.Abs<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(4f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Mathematical Operations - Sqrt Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Sqrt_ReturnsSquareRoots() {
    var tensor = Tensor.Create(new float[] { 4, 9, 16, 25 });

    var result = Tensor.Sqrt<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(5f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Mathematical Operations - Exp/Log Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Exp_ReturnsExponentialValues() {
    var tensor = Tensor.Create(new float[] { 0, 1, 2 });

    var result = Tensor.Exp<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)Math.E).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo((float)(Math.E * Math.E)).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Log_ReturnsNaturalLogarithms() {
    var tensor = Tensor.Create([1, (float)Math.E, (float)(Math.E * Math.E)]);

    var result = Tensor.Log<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(2f).Within(0.01f));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Log2_ReturnsLog2Values() {
    var tensor = Tensor.Create(new float[] { 1, 2, 4, 8 });

    var result = Tensor.Log2<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(3f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Log10_ReturnsLog10Values() {
    var tensor = Tensor.Create(new float[] { 1, 10, 100, 1000 });

    var result = Tensor.Log10<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(3f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Mathematical Operations - Pow Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Pow_TensorByScalar_ReturnsPower() {
    var tensor = Tensor.Create(new float[] { 2, 3, 4 });

    var result = Tensor.Pow<float>(tensor, 2f);

    Assert.That(result[I(0)], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(9f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(16f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Pow_TensorByTensor_ReturnsPower() {
    var bases = Tensor.Create(new float[] { 2, 3, 4 });
    var exponents = Tensor.Create([3, 2, 0.5f]);

    var result = Tensor.Pow<float>(bases, exponents);

    Assert.That(result[I(0)], Is.EqualTo(8f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(9f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(2f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Reduction Operations - Sum Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Sum_ReturnsTotal() {
    var tensor = Tensor.Create(new float[] { 1, 2, 3, 4, 5 });

    var result = Tensor.Sum<float>(tensor);

    Assert.That(result, Is.EqualTo(15f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Sum_2D_ReturnsTotal() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6], I(2, 3));

    var result = Tensor.Sum<int>(tensor);

    Assert.That(result, Is.EqualTo(21));
  }

  #endregion

  #region Tensor Reduction Operations - Average Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Average_ReturnsAverage() {
    var tensor = Tensor.Create(new float[] { 2, 4, 6, 8, 10 });

    var result = Tensor.Average<float>(tensor);

    Assert.That(result, Is.EqualTo(6f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Reduction Operations - Min/Max Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Min_ReturnsMinimumValue() {
    var tensor = Tensor.Create(new float[] { 5, 2, 8, 1, 9 });

    var result = Tensor.Min<float>(tensor);

    Assert.That(result, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Max_ReturnsMaximumValue() {
    var tensor = Tensor.Create(new float[] { 5, 2, 8, 1, 9 });

    var result = Tensor.Max<float>(tensor);

    Assert.That(result, Is.EqualTo(9f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_MinNumber_ReturnsMinimumValue() {
    var tensor = Tensor.Create(new float[] { 5, 2, 8, 1, 9 });

    var result = Tensor.MinNumber<float>(tensor);

    Assert.That(result, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_MaxNumber_ReturnsMaximumValue() {
    var tensor = Tensor.Create(new float[] { 5, 2, 8, 1, 9 });

    var result = Tensor.MaxNumber<float>(tensor);

    Assert.That(result, Is.EqualTo(9f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_MinMagnitude_ReturnsSmallestMagnitude() {
    var tensor = Tensor.Create(new float[] { -5, 2, -1, 3 });

    var result = Tensor.MinMagnitude<float>(tensor);

    Assert.That(result, Is.EqualTo(-1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_MaxMagnitude_ReturnsLargestMagnitude() {
    var tensor = Tensor.Create(new float[] { -5, 2, -1, 3 });

    var result = Tensor.MaxMagnitude<float>(tensor);

    Assert.That(result, Is.EqualTo(-5f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Reduction Operations - Product Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Product_ReturnsProduct() {
    var tensor = Tensor.Create(new float[] { 2, 3, 4 });

    var result = Tensor.Product<float>(tensor);

    Assert.That(result, Is.EqualTo(24f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Reduction Operations - Dot/Inner Product Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Dot_ReturnsDotProduct() {
    var a = Tensor.Create(new float[] { 1, 2, 3 });
    var b = Tensor.Create(new float[] { 4, 5, 6 });

    var result = Tensor.Dot<float>(a, b);

    // 1*4 + 2*5 + 3*6 = 4 + 10 + 18 = 32
    Assert.That(result, Is.EqualTo(32f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Reduction Operations - IndexOf Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_IndexOfMin_ReturnsCorrectIndex() {
    var tensor = Tensor.Create(new float[] { 5, 2, 8, 1, 9 });

    var index = Tensor.IndexOfMin<float>(tensor);

    Assert.That(index, Is.EqualTo((nint)3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_IndexOfMax_ReturnsCorrectIndex() {
    var tensor = Tensor.Create(new float[] { 5, 2, 8, 1, 9 });

    var index = Tensor.IndexOfMax<float>(tensor);

    Assert.That(index, Is.EqualTo((nint)4));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_IndexOfMinMagnitude_ReturnsCorrectIndex() {
    var tensor = Tensor.Create(new float[] { -5, 2, -1, 3 });

    var index = Tensor.IndexOfMinMagnitude<float>(tensor);

    Assert.That(index, Is.EqualTo((nint)2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_IndexOfMaxMagnitude_ReturnsCorrectIndex() {
    var tensor = Tensor.Create(new float[] { -5, 2, -1, 3 });

    var index = Tensor.IndexOfMaxMagnitude<float>(tensor);

    Assert.That(index, Is.EqualTo((nint)0));
  }

  #endregion

  #region Tensor Comparison Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_EqualsAll_ReturnsTrueForEqualTensors() {
    var a = Tensor.Create([1, 2, 3]);
    var b = Tensor.Create([1, 2, 3]);

    var result = Tensor.EqualsAll<int>(a, b);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_EqualsAll_ReturnsFalseForUnequalTensors() {
    var a = Tensor.Create([1, 2, 3]);
    var b = Tensor.Create([1, 2, 4]);

    var result = Tensor.EqualsAll<int>(a, b);

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_EqualsAny_ReturnsTrueIfAnyElementsEqual() {
    var a = Tensor.Create([1, 2, 3]);
    var b = Tensor.Create([4, 2, 5]);

    var result = Tensor.EqualsAny<int>(a, b);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_EqualsAny_ReturnsFalseIfNoElementsEqual() {
    var a = Tensor.Create([1, 2, 3]);
    var b = Tensor.Create([4, 5, 6]);

    var result = Tensor.EqualsAny<int>(a, b);

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GreaterThan_ReturnsComparisonResult() {
    var a = Tensor.Create([5, 2, 8]);
    var b = Tensor.Create([3, 4, 6]);

    var result = Tensor.GreaterThan<int>(a, b);

    Assert.That(result[I(0)], Is.True);
    Assert.That(result[I(1)], Is.False);
    Assert.That(result[I(2)], Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GreaterThanOrEqual_ReturnsComparisonResult() {
    var a = Tensor.Create([5, 4, 8]);
    var b = Tensor.Create([3, 4, 9]);

    var result = Tensor.GreaterThanOrEqual<int>(a, b);

    Assert.That(result[I(0)], Is.True);
    Assert.That(result[I(1)], Is.True);
    Assert.That(result[I(2)], Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_LessThan_ReturnsComparisonResult() {
    var a = Tensor.Create([2, 5, 3]);
    var b = Tensor.Create([3, 4, 6]);

    var result = Tensor.LessThan<int>(a, b);

    Assert.That(result[I(0)], Is.True);
    Assert.That(result[I(1)], Is.False);
    Assert.That(result[I(2)], Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_LessThanOrEqual_ReturnsComparisonResult() {
    var a = Tensor.Create([2, 4, 7]);
    var b = Tensor.Create([3, 4, 6]);

    var result = Tensor.LessThanOrEqual<int>(a, b);

    Assert.That(result[I(0)], Is.True);
    Assert.That(result[I(1)], Is.True);
    Assert.That(result[I(2)], Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GreaterThanAll_ReturnsTrueIfAllGreater() {
    var a = Tensor.Create([5, 6, 7]);
    var b = Tensor.Create([1, 2, 3]);

    var result = Tensor.GreaterThanAll<int>(a, b);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_GreaterThanAny_ReturnsTrueIfAnyGreater() {
    var a = Tensor.Create([1, 6, 2]);
    var b = Tensor.Create([5, 2, 3]);

    var result = Tensor.GreaterThanAny<int>(a, b);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_LessThanAll_ReturnsTrueIfAllLess() {
    var a = Tensor.Create([1, 2, 3]);
    var b = Tensor.Create([5, 6, 7]);

    var result = Tensor.LessThanAll<int>(a, b);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_LessThanAny_ReturnsTrueIfAnyLess() {
    var a = Tensor.Create([5, 1, 7]);
    var b = Tensor.Create([1, 6, 3]);

    var result = Tensor.LessThanAny<int>(a, b);

    Assert.That(result, Is.True);
  }

  #endregion

  #region Tensor Trigonometric Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Sin_ReturnsSineValues() {
    var tensor = Tensor.Create([0, (float)(Math.PI / 2), (float)Math.PI]);

    var result = Tensor.Sin<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Cos_ReturnsCosineValues() {
    var tensor = Tensor.Create([0, (float)(Math.PI / 2), (float)Math.PI]);

    var result = Tensor.Cos<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(-1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Tan_ReturnsTangentValues() {
    var tensor = Tensor.Create([0, (float)(Math.PI / 4)]);

    var result = Tensor.Tan<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Asin_ReturnsArcSineValues() {
    var tensor = Tensor.Create([0, 0.5f, 1]);

    var result = Tensor.Asin<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)(Math.PI / 6)).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo((float)(Math.PI / 2)).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Acos_ReturnsArcCosineValues() {
    var tensor = Tensor.Create([1, 0.5f, 0]);

    var result = Tensor.Acos<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)(Math.PI / 3)).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo((float)(Math.PI / 2)).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Atan_ReturnsArcTangentValues() {
    var tensor = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.Atan<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)(Math.PI / 4)).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Atan2_ReturnsArcTangent2Values() {
    var y = Tensor.Create(new float[] { 1, 1 });
    var x = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.Atan2<float>(y, x);

    Assert.That(result[I(0)], Is.EqualTo((float)(Math.PI / 2)).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)(Math.PI / 4)).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Hyperbolic Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Sinh_ReturnsHyperbolicSineValues() {
    var tensor = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.Sinh<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)Math.Sinh(1)).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Cosh_ReturnsHyperbolicCosineValues() {
    var tensor = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.Cosh<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)Math.Cosh(1)).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Tanh_ReturnsHyperbolicTangentValues() {
    var tensor = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.Tanh<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)Math.Tanh(1)).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Shape Manipulation Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Reshape_ChangesShape() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6], I(2, 3));

    var reshaped = Tensor.Reshape<int>(tensor, I(3, 2));

    Assert.That(reshaped.Rank, Is.EqualTo(2));
    Assert.That((int)reshaped.Lengths[0], Is.EqualTo(3));
    Assert.That((int)reshaped.Lengths[1], Is.EqualTo(2));
    Assert.That((int)reshaped.FlattenedLength, Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Reshape_To1D_Works() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6], I(2, 3));

    var reshaped = Tensor.Reshape<int>(tensor, I(6));

    Assert.That(reshaped.Rank, Is.EqualTo(1));
    Assert.That((int)reshaped.FlattenedLength, Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Squeeze_RemovesSingleDimensions() {
    var tensor = Tensor.Create([1, 2, 3], I(1, 3, 1));

    var squeezed = Tensor.Squeeze<int>(tensor);

    Assert.That(squeezed.Rank, Is.EqualTo(1));
    Assert.That((int)squeezed.Lengths[0], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_SqueezeDimension_RemovesSpecificDimension() {
    var tensor = Tensor.Create([1, 2, 3], I(1, 3));

    var squeezed = Tensor.SqueezeDimension<int>(tensor, 0);

    Assert.That(squeezed.Rank, Is.EqualTo(1));
    Assert.That((int)squeezed.Lengths[0], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Unsqueeze_AddsDimension() {
    var tensor = Tensor.Create([1, 2, 3]);

    var unsqueezed = Tensor.Unsqueeze<int>(tensor, 0);

    Assert.That(unsqueezed.Rank, Is.EqualTo(2));
    Assert.That((int)unsqueezed.Lengths[0], Is.EqualTo(1));
    Assert.That((int)unsqueezed.Lengths[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_PermuteDimensions_ReordersDimensions() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6], I(2, 3));

    var permuted = Tensor.PermuteDimensions<int>(tensor, [1, 0]);

    Assert.That((int)permuted.Lengths[0], Is.EqualTo(3));
    Assert.That((int)permuted.Lengths[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Reverse_ReversesAllDimensions() {
    var tensor = Tensor.Create([1, 2, 3, 4], I(2, 2));

    var reversed = Tensor.Reverse<int>(tensor);

    Assert.That(reversed[I(0, 0)], Is.EqualTo(4));
    Assert.That(reversed[I(1, 1)], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_ReverseDimension_ReversesSpecificDimension() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6], I(2, 3));

    var reversed = Tensor.ReverseDimension<int>(tensor, 1);

    Assert.That(reversed[I(0, 0)], Is.EqualTo(3));
    Assert.That(reversed[I(0, 2)], Is.EqualTo(1));
  }

  #endregion

  #region Tensor Broadcast Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Broadcast_ExpandsTensorToShape() {
    var tensor = Tensor.Create([1, 2, 3]);

    var broadcasted = Tensor.Broadcast<int>(tensor, I(2, 3));

    Assert.That((int)broadcasted.Lengths[0], Is.EqualTo(2));
    Assert.That((int)broadcasted.Lengths[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_BroadcastTo_ExpandsCorrectly() {
    var tensor = Tensor.Create([1, 2, 3]);
    var dest = new int[6];
    var destSpan = new TensorSpan<int>(dest, I(2, 3));

    Tensor.BroadcastTo(tensor, destSpan);

    Assert.That(dest[0], Is.EqualTo(1));
    Assert.That(dest[3], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryBroadcastTo_ReturnsTrueOnSuccess() {
    var tensor = Tensor.Create([1, 2, 3]);
    var dest = new int[6];
    var destSpan = new TensorSpan<int>(dest, I(2, 3));

    var result = Tensor.TryBroadcastTo(tensor, destSpan);

    Assert.That(result, Is.True);
  }

  #endregion

  #region Tensor Bitwise Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_BitwiseAnd_ReturnsAndResult() {
    var a = Tensor.Create([0b1100, 0b1010]);
    var b = Tensor.Create([0b1010, 0b0110]);

    var result = Tensor.BitwiseAnd<int>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(0b1000));
    Assert.That(result[I(1)], Is.EqualTo(0b0010));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_BitwiseOr_ReturnsOrResult() {
    var a = Tensor.Create([0b1100, 0b1010]);
    var b = Tensor.Create([0b1010, 0b0110]);

    var result = Tensor.BitwiseOr<int>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(0b1110));
    Assert.That(result[I(1)], Is.EqualTo(0b1110));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Xor_ReturnsXorResult() {
    var a = Tensor.Create([0b1100, 0b1010]);
    var b = Tensor.Create([0b1010, 0b0110]);

    var result = Tensor.Xor<int>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(0b0110));
    Assert.That(result[I(1)], Is.EqualTo(0b1100));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_OnesComplement_ReturnsComplementResult() {
    var tensor = Tensor.Create([0, -1]);

    var result = Tensor.OnesComplement<int>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(-1));
    Assert.That(result[I(1)], Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_PopCount_ReturnsPopCountResult() {
    var tensor = Tensor.Create([0b1111, 0b1010, 0]);

    var result = Tensor.PopCount<int>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(4));
    Assert.That(result[I(1)], Is.EqualTo(2));
    Assert.That(result[I(2)], Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_LeadingZeroCount_ReturnsLeadingZeros() {
    var tensor = Tensor.Create([1, 256, 0]);

    var result = Tensor.LeadingZeroCount<int>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(31));
    Assert.That(result[I(1)], Is.EqualTo(23));
    Assert.That(result[I(2)], Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TrailingZeroCount_ReturnsTrailingZeros() {
    var tensor = Tensor.Create([8, 12, 1]);

    var result = Tensor.TrailingZeroCount<int>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(3));
    Assert.That(result[I(1)], Is.EqualTo(2));
    Assert.That(result[I(2)], Is.EqualTo(0));
  }

  #endregion

  #region Tensor Rounding Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Ceiling_ReturnsCeilingValues() {
    var tensor = Tensor.Create([1.1f, 2.5f, 3.9f, -1.5f]);

    var result = Tensor.Ceiling<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(-1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Floor_ReturnsFloorValues() {
    var tensor = Tensor.Create([1.1f, 2.5f, 3.9f, -1.5f]);

    var result = Tensor.Floor<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(-2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Round_ReturnsRoundedValues() {
    var tensor = Tensor.Create([1.4f, 2.5f, 3.6f]);

    var result = Tensor.Round<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Truncate_ReturnsTruncatedValues() {
    var tensor = Tensor.Create([1.9f, -2.9f, 3.1f]);

    var result = Tensor.Truncate<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(-2f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(3f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Copy/Clone Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_CopyTo_CreatesCopy() {
    var source = Tensor.Create([1, 2, 3, 4, 5, 6], I(2, 3));
    var dest = new int[6];
    var destSpan = new TensorSpan<int>(dest, I(2, 3));

    source.CopyTo(destSpan);

    Assert.That(dest, Is.EqualTo(new int[] { 1, 2, 3, 4, 5, 6 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryCopyTo_ReturnsTrueOnSuccess() {
    var source = Tensor.Create([1, 2, 3]);
    var dest = new int[3];
    var destSpan = new TensorSpan<int>(dest);

    var result = source.TryCopyTo(destSpan);

    Assert.That(result, Is.True);
    Assert.That(dest, Is.EqualTo(new int[] { 1, 2, 3 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TryCopyTo_ReturnsFalseWhenDestinationTooSmall() {
    var source = Tensor.Create([1, 2, 3, 4]);
    var dest = new int[2];
    var destSpan = new TensorSpan<int>(dest);

    var result = source.TryCopyTo(destSpan);

    Assert.That(result, Is.False);
  }

  #endregion

  #region Tensor Fill/Clear Operations Tests

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_Fill_FillsWithValue() {
    var dest = new int[6];
    var destSpan = new TensorSpan<int>(dest, I(2, 3));

    destSpan.Fill(42);

    Assert.That(dest.All(x => x == 42), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TensorSpan_Clear_ClearsToDefault() {
    var dest = new int[] { 1, 2, 3, 4 };
    var destSpan = new TensorSpan<int>(dest);

    destSpan.Clear();

    Assert.That(dest.All(x => x == 0), Is.True);
  }

  #endregion

  #region Tensor CosineSimilarity Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_CosineSimilarity_ReturnsCorrectValue() {
    var a = Tensor.Create(new float[] { 1, 0, 0 });
    var b = Tensor.Create(new float[] { 1, 0, 0 });

    var result = Tensor.CosineSimilarity<float>(a, b);

    Assert.That(result, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CosineSimilarity_OrthogonalVectors_ReturnsZero() {
    var a = Tensor.Create(new float[] { 1, 0, 0 });
    var b = Tensor.Create(new float[] { 0, 1, 0 });

    var result = Tensor.CosineSimilarity<float>(a, b);

    Assert.That(result, Is.EqualTo(0f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Distance Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Distance_ReturnsEuclideanDistance() {
    var a = Tensor.Create(new float[] { 0, 0, 0 });
    var b = Tensor.Create(new float[] { 3, 4, 0 });

    var result = Tensor.Distance<float>(a, b);

    Assert.That(result, Is.EqualTo(5f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Norm Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Norm_ReturnsL2Norm() {
    var tensor = Tensor.Create(new float[] { 3, 4 });

    var result = Tensor.Norm<float>(tensor);

    Assert.That(result, Is.EqualTo(5f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor SoftMax Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_SoftMax_ReturnsSoftMaxValues() {
    var tensor = Tensor.Create(new float[] { 1, 2, 3 });
    var dest = new float[3];
    var destSpan = new TensorSpan<float>(dest);

    Tensor.SoftMax(tensor, destSpan);

    // Softmax values should sum to 1
    var sum = dest.Sum();
    Assert.That(sum, Is.EqualTo(1f).Within(FloatTolerance));
    // Values should be in ascending order
    Assert.That(dest[0], Is.LessThan(dest[1]));
    Assert.That(dest[1], Is.LessThan(dest[2]));
  }

  #endregion

  #region Tensor Sigmoid Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Sigmoid_ReturnsSigmoidValues() {
    var tensor = Tensor.Create(new float[] { 0, 10, -10 });
    var dest = new float[3];
    var destSpan = new TensorSpan<float>(dest);

    Tensor.Sigmoid(tensor, destSpan);

    Assert.That(dest[0], Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(dest[1], Is.EqualTo(1f).Within(0.001f));
    Assert.That(dest[2], Is.EqualTo(0f).Within(0.001f));
  }

  #endregion

  #region Tensor Cbrt Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Cbrt_ReturnsCubeRoots() {
    var tensor = Tensor.Create(new float[] { 8, 27, 64 });

    var result = Tensor.Cbrt<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(4f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Hypot Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Hypot_ReturnsHypotenuse() {
    var a = Tensor.Create(new float[] { 3, 5, 8 });
    var b = Tensor.Create(new float[] { 4, 12, 15 });

    var result = Tensor.Hypot<float>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(13f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(17f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor IEEE Remainder Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Ieee754Remainder_ReturnsRemainder() {
    var a = Tensor.Create(new float[] { 10, 15, 20 });
    var b = Tensor.Create(new float[] { 3, 4, 7 });

    var result = Tensor.Ieee754Remainder<float>(a, b);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(-1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(-1f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor CopySign Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_CopySign_CopiesSign() {
    var magnitude = Tensor.Create(new float[] { 5, -5, 5 });
    var sign = Tensor.Create(new float[] { 1, 1, -1 });

    var result = Tensor.CopySign<float>(magnitude, sign);

    Assert.That(result[I(0)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(-5f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor DegreesToRadians/RadiansToDegrees Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_DegreesToRadians_ConvertsCorrectly() {
    var tensor = Tensor.Create(new float[] { 0, 90, 180, 360 });

    var result = Tensor.DegreesToRadians<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)(Math.PI / 2)).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo((float)Math.PI).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo((float)(Math.PI * 2)).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_RadiansToDegrees_ConvertsCorrectly() {
    var tensor = Tensor.Create([0, (float)(Math.PI / 2), (float)Math.PI]);

    var result = Tensor.RadiansToDegrees<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(90f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(180f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor ILogB Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_ILogB_ReturnsIntegerLogarithm() {
    var tensor = Tensor.Create(new float[] { 1, 2, 4, 8 });

    var result = Tensor.ILogB<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0));
    Assert.That(result[I(1)], Is.EqualTo(1));
    Assert.That(result[I(2)], Is.EqualTo(2));
    Assert.That(result[I(3)], Is.EqualTo(3));
  }

  #endregion

  #region Tensor Exp Special Functions Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Exp10_ReturnsExp10Values() {
    var tensor = Tensor.Create(new float[] { 0, 1, 2 });

    var result = Tensor.Exp10<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(100f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Exp2_ReturnsExp2Values() {
    var tensor = Tensor.Create(new float[] { 0, 1, 2, 3 });

    var result = Tensor.Exp2<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(result[I(3)], Is.EqualTo(8f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_ExpM1_ReturnsExpM1Values() {
    var tensor = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.ExpM1<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo((float)(Math.E - 1)).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Log Special Functions Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_LogP1_ReturnsLogP1Values() {
    var tensor = Tensor.Create([0, (float)(Math.E - 1)]);

    var result = Tensor.LogP1<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Log2P1_ReturnsLog2P1Values() {
    var tensor = Tensor.Create(new float[] { 0, 1, 3 });

    var result = Tensor.Log2P1<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Log10P1_ReturnsLog10P1Values() {
    var tensor = Tensor.Create(new float[] { 0, 9, 99 });

    var result = Tensor.Log10P1<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(2f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Reciprocal Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Reciprocal_ReturnsReciprocalValues() {
    var tensor = Tensor.Create(new float[] { 2, 4, 5 });

    var result = Tensor.Reciprocal<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(0.25f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(0.2f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor RootN Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_RootN_ReturnsNthRoots() {
    var tensor = Tensor.Create(new float[] { 8, 27, 16 });

    var result2 = Tensor.RootN<float>(tensor, 2);
    var result3 = Tensor.RootN<float>(tensor, 3);

    Assert.That(result3[I(0)], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(result3[I(1)], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(result2[I(2)], Is.EqualTo(4f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Fill Distribution Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_FillGaussianNormalDistribution_FillsTensor() {
    var tensor = Tensor.Create(new float[10]);
    var span = tensor.AsTensorSpan();

    Tensor.FillGaussianNormalDistribution(span, new Random(42));

    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_FillUniformDistribution_FillsTensor() {
    var tensor = Tensor.Create(new float[10]);
    var span = tensor.AsTensorSpan();

    Tensor.FillUniformDistribution(span, new Random(42));

    Assert.That((int)tensor.FlattenedLength, Is.EqualTo(10));
  }

  #endregion

  #region Tensor SinPi/CosPi/TanPi Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_SinPi_ReturnsSinPiValues() {
    var tensor = Tensor.Create([0, 0.5f, 1]);

    var result = Tensor.SinPi<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_CosPi_ReturnsCosPiValues() {
    var tensor = Tensor.Create([0, 0.5f, 1]);

    var result = Tensor.CosPi<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(2)], Is.EqualTo(-1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_TanPi_ReturnsTanPiValues() {
    var tensor = Tensor.Create([0, 0.25f]);

    var result = Tensor.TanPi<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor AsinPi/AcosPi/AtanPi/Atan2Pi Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_AsinPi_ReturnsAsinPiValues() {
    var tensor = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.AsinPi<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(0.5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_AcosPi_ReturnsAcosPiValues() {
    var tensor = Tensor.Create(new float[] { 1, 0 });

    var result = Tensor.AcosPi<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(0.5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_AtanPi_ReturnsAtanPiValues() {
    var tensor = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.AtanPi<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(0.25f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Atan2Pi_ReturnsAtan2PiValues() {
    var y = Tensor.Create(new float[] { 1, 0 });
    var x = Tensor.Create(new float[] { 0, 1 });

    var result = Tensor.Atan2Pi<float>(y, x);

    Assert.That(result[I(0)], Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(0f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Asinh/Acosh/Atanh Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Asinh_ReturnsAsinhValues() {
    var tensor = Tensor.Create([0, (float)Math.Sinh(1)]);

    var result = Tensor.Asinh<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Acosh_ReturnsAcoshValues() {
    var tensor = Tensor.Create([1, (float)Math.Cosh(1)]);

    var result = Tensor.Acosh<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Atanh_ReturnsAtanhValues() {
    var tensor = Tensor.Create([0, (float)Math.Tanh(1)]);

    var result = Tensor.Atanh<float>(tensor);

    Assert.That(result[I(0)], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(result[I(1)], Is.EqualTo(1f).Within(FloatTolerance));
  }

  #endregion

  #region Tensor Concatenate/Stack Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Concatenate_ConcatenatesAlongAxis() {
    var a = Tensor.Create([1, 2]);
    var b = Tensor.Create([3, 4]);

    var result = Tensor.Concatenate<int>([a, b]);

    Assert.That((int)result.FlattenedLength, Is.EqualTo(4));
    Assert.That(result[I(0)], Is.EqualTo(1));
    Assert.That(result[I(2)], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_ConcatenateOnDimension_Works() {
    var a = Tensor.Create([1, 2, 3, 4], I(2, 2));
    var b = Tensor.Create([5, 6, 7, 8], I(2, 2));

    var result = Tensor.ConcatenateOnDimension<int>(0, a, b);

    Assert.That((int)result.Lengths[0], Is.EqualTo(4));
    Assert.That((int)result.Lengths[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Tensor_Stack_StacksAlongNewAxis() {
    var a = Tensor.Create([1, 2]);
    var b = Tensor.Create([3, 4]);

    var result = Tensor.StackAlongDimension<int>(0, a, b);

    Assert.That(result.Rank, Is.EqualTo(2));
    Assert.That((int)result.Lengths[0], Is.EqualTo(2));
    Assert.That((int)result.Lengths[1], Is.EqualTo(2));
  }

  #endregion

  #region Tensor Split Tests

  [Test]
  [Category("HappyPath")]
  public void Tensor_Split_SplitsIntoEqualParts() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6]);

    var parts = Tensor.Split<int>(tensor, 3, 0);

    Assert.That(parts.Length, Is.EqualTo(3));
    Assert.That((int)parts[0].FlattenedLength, Is.EqualTo(2));
  }

  #endregion

  #region Tensor Edge Cases and Exception Tests

  [Test]
  [Category("EdgeCase")]
  public void Tensor_Add_EmptyTensors_ReturnsEmpty() {
    var a = Tensor<float>.Empty;
    var b = Tensor<float>.Empty;

    var result = Tensor.Add<float>(a, b);

    Assert.That(result.IsEmpty, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Tensor_Negate_2D_Works() {
    var tensor = Tensor.Create([1, -2, 3, -4], I(2, 2));

    var result = Tensor.Negate<int>(tensor);

    Assert.That(result[I(0, 0)], Is.EqualTo(-1));
    Assert.That(result[I(0, 1)], Is.EqualTo(2));
    Assert.That(result[I(1, 0)], Is.EqualTo(-3));
    Assert.That(result[I(1, 1)], Is.EqualTo(4));
  }

  [Test]
  [Category("Exception")]
  public void Tensor_Add_MismatchedShapes_Throws() {
    var a = Tensor.Create([1, 2, 3]);
    var b = Tensor.Create([1, 2]);

    Assert.That(() => Tensor.Add<int>(a, b), Throws.InstanceOf<Exception>());
  }

  [Test]
  [Category("Exception")]
  public void Tensor_Reshape_WrongTotalSize_Throws() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6]);

    Assert.That(() => Tensor.Reshape<int>(tensor, I(2, 4)), Throws.InstanceOf<Exception>());
  }

  [Test]
  [Category("Exception")]
  public void Tensor_SqueezeDimension_NonSingleDimension_Throws() {
    var tensor = Tensor.Create([1, 2, 3, 4, 5, 6], I(2, 3));

    Assert.That(() => Tensor.SqueezeDimension<int>(tensor, 0), Throws.InstanceOf<Exception>());
  }

  #endregion

}
