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
using System.Numerics.Tensors;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("TensorPrimitives")]
public class TensorPrimitivesTests {

  private const float FloatTolerance = 0.0001f;
  private const double DoubleTolerance = 0.00001;

  #region Add Tests

  [Test]
  [Category("HappyPath")]
  public void Add_TwoSpans_ComputesElementWiseSum() {
    var x = new float[] { 1f, 2f, 3f, 4f };
    var y = new float[] { 5f, 6f, 7f, 8f };
    var destination = new float[4];

    TensorPrimitives.Add<float>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(6f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(8f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(12f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Add_SpanAndScalar_ComputesElementWiseSum() {
    var x = new float[] { 1f, 2f, 3f, 4f };
    var destination = new float[4];

    TensorPrimitives.Add(x, 10f, destination);

    Assert.That(destination[0], Is.EqualTo(11f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(12f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(13f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(14f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Add_IntegerSpans_ComputesCorrectly() {
    var x = new int[] { 1, 2, 3, 4 };
    var y = new int[] { 5, 6, 7, 8 };
    var destination = new int[4];

    TensorPrimitives.Add<int>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(6));
    Assert.That(destination[1], Is.EqualTo(8));
    Assert.That(destination[2], Is.EqualTo(10));
    Assert.That(destination[3], Is.EqualTo(12));
  }

  #endregion

  #region Subtract Tests

  [Test]
  [Category("HappyPath")]
  public void Subtract_TwoSpans_ComputesElementWiseDifference() {
    var x = new float[] { 10f, 20f, 30f, 40f };
    var y = new float[] { 1f, 2f, 3f, 4f };
    var destination = new float[4];

    TensorPrimitives.Subtract<float>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(9f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(18f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(27f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(36f).Within(FloatTolerance));
  }

  #endregion

  #region Multiply Tests

  [Test]
  [Category("HappyPath")]
  public void Multiply_TwoSpans_ComputesElementWiseProduct() {
    var x = new float[] { 2f, 3f, 4f, 5f };
    var y = new float[] { 1f, 2f, 3f, 4f };
    var destination = new float[4];

    TensorPrimitives.Multiply<float>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(6f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(12f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(20f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Multiply_SpanAndScalar_ComputesElementWiseProduct() {
    var x = new float[] { 1f, 2f, 3f, 4f };
    var destination = new float[4];

    TensorPrimitives.Multiply(x, 2f, destination);

    Assert.That(destination[0], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(6f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(8f).Within(FloatTolerance));
  }

  #endregion

  #region Divide Tests

  [Test]
  [Category("HappyPath")]
  public void Divide_TwoSpans_ComputesElementWiseQuotient() {
    var x = new float[] { 10f, 20f, 30f, 40f };
    var y = new float[] { 2f, 4f, 5f, 8f };
    var destination = new float[4];

    TensorPrimitives.Divide<float>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(6f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(5f).Within(FloatTolerance));
  }

  #endregion

  #region Sum Tests

  [Test]
  [Category("HappyPath")]
  public void Sum_FloatSpan_ComputesCorrectSum() {
    var x = new float[] { 1f, 2f, 3f, 4f, 5f };

    var result = TensorPrimitives.Sum<float>(x);

    Assert.That(result, Is.EqualTo(15f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Sum_IntegerSpan_ComputesCorrectSum() {
    var x = new int[] { 1, 2, 3, 4, 5 };

    var result = TensorPrimitives.Sum<int>(x);

    Assert.That(result, Is.EqualTo(15));
  }

  [Test]
  [Category("EdgeCase")]
  public void Sum_EmptySpan_ReturnsZero() {
    var x = Array.Empty<float>();

    var result = TensorPrimitives.Sum<float>(x);

    Assert.That(result, Is.EqualTo(0f).Within(FloatTolerance));
  }

  #endregion

  #region Product Tests

  [Test]
  [Category("HappyPath")]
  public void Product_FloatSpan_ComputesCorrectProduct() {
    var x = new float[] { 1f, 2f, 3f, 4f };

    var result = TensorPrimitives.Product<float>(x);

    Assert.That(result, Is.EqualTo(24f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Product_IntegerSpan_ComputesCorrectProduct() {
    var x = new int[] { 2, 3, 4 };

    var result = TensorPrimitives.Product<int>(x);

    Assert.That(result, Is.EqualTo(24));
  }

  #endregion

  #region Dot Product Tests

  [Test]
  [Category("HappyPath")]
  public void Dot_TwoFloatSpans_ComputesCorrectDotProduct() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 4f, 5f, 6f };

    var result = TensorPrimitives.Dot<float>(x, y);

    Assert.That(result, Is.EqualTo(32f).Within(FloatTolerance)); // 1*4 + 2*5 + 3*6 = 32
  }

  [Test]
  [Category("HappyPath")]
  public void Dot_TwoIntegerSpans_ComputesCorrectDotProduct() {
    var x = new int[] { 1, 2, 3 };
    var y = new int[] { 4, 5, 6 };

    var result = TensorPrimitives.Dot<int>(x, y);

    Assert.That(result, Is.EqualTo(32));
  }

  #endregion

  #region Norm Tests

  [Test]
  [Category("HappyPath")]
  public void Norm_FloatSpan_ComputesL2Norm() {
    var x = new float[] { 3f, 4f };

    var result = TensorPrimitives.Norm<float>(x);

    Assert.That(result, Is.EqualTo(5f).Within(FloatTolerance)); // sqrt(9 + 16) = 5
  }

  [Test]
  [Category("HappyPath")]
  public void SumOfSquares_FloatSpan_ComputesCorrectly() {
    var x = new float[] { 1f, 2f, 3f };

    var result = TensorPrimitives.SumOfSquares<float>(x);

    Assert.That(result, Is.EqualTo(14f).Within(FloatTolerance)); // 1 + 4 + 9 = 14
  }

  #endregion

  #region Min/Max Tests

  [Test]
  [Category("HappyPath")]
  public void Max_FloatSpan_ReturnsMaxValue() {
    var x = new float[] { 3f, 1f, 4f, 1f, 5f, 9f, 2f, 6f };

    var result = TensorPrimitives.Max<float>(x);

    Assert.That(result, Is.EqualTo(9f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Min_FloatSpan_ReturnsMinValue() {
    var x = new float[] { 3f, 1f, 4f, 1f, 5f, 9f, 2f, 6f };

    var result = TensorPrimitives.Min<float>(x);

    Assert.That(result, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfMax_FloatSpan_ReturnsCorrectIndex() {
    var x = new float[] { 3f, 1f, 9f, 5f };

    var result = TensorPrimitives.IndexOfMax<float>(x);

    Assert.That(result, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfMin_FloatSpan_ReturnsCorrectIndex() {
    var x = new float[] { 3f, 1f, 9f, 5f };

    var result = TensorPrimitives.IndexOfMin<float>(x);

    Assert.That(result, Is.EqualTo(1));
  }

  #endregion

  #region Abs/Negate Tests

  [Test]
  [Category("HappyPath")]
  public void Abs_FloatSpan_ComputesAbsoluteValues() {
    var x = new float[] { -1f, 2f, -3f, 4f };
    var destination = new float[4];

    TensorPrimitives.Abs(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Negate_FloatSpan_NegatesAllElements() {
    var x = new float[] { 1f, -2f, 3f, -4f };
    var destination = new float[4];

    TensorPrimitives.Negate(x, destination);

    Assert.That(destination[0], Is.EqualTo(-1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-3f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(4f).Within(FloatTolerance));
  }

  #endregion

  #region Math Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Sqrt_FloatSpan_ComputesSquareRoots() {
    var x = new float[] { 1f, 4f, 9f, 16f };
    var destination = new float[4];

    TensorPrimitives.Sqrt(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Exp_FloatSpan_ComputesExponentials() {
    var x = new float[] { 0f, 1f };
    var destination = new float[2];

    TensorPrimitives.Exp(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance)); // e^0 = 1
    Assert.That(destination[1], Is.EqualTo(MathF.E).Within(FloatTolerance)); // e^1 = e
  }

  [Test]
  [Category("HappyPath")]
  public void Log_FloatSpan_ComputesNaturalLogarithms() {
    var x = new float[] { 1f, MathF.E };
    var destination = new float[2];

    TensorPrimitives.Log(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance)); // ln(1) = 0
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance)); // ln(e) = 1
  }

  #endregion

  #region Distance Tests

  [Test]
  [Category("HappyPath")]
  public void Distance_TwoFloatSpans_ComputesEuclideanDistance() {
    var x = new float[] { 0f, 0f };
    var y = new float[] { 3f, 4f };

    var result = TensorPrimitives.Distance<float>(x, y);

    Assert.That(result, Is.EqualTo(5f).Within(FloatTolerance)); // sqrt(9 + 16) = 5
  }

  #endregion

  #region CosineSimilarity Tests

  [Test]
  [Category("HappyPath")]
  public void CosineSimilarity_ParallelVectors_ReturnsOne() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 2f, 4f, 6f };

    var result = TensorPrimitives.CosineSimilarity<float>(x, y);

    Assert.That(result, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CosineSimilarity_PerpendicularVectors_ReturnsZero() {
    var x = new float[] { 1f, 0f };
    var y = new float[] { 0f, 1f };

    var result = TensorPrimitives.CosineSimilarity<float>(x, y);

    Assert.That(result, Is.EqualTo(0f).Within(FloatTolerance));
  }

  #endregion

  #region Sigmoid Tests

  [Test]
  [Category("HappyPath")]
  public void Sigmoid_FloatSpan_ComputesSigmoid() {
    var x = new float[] { 0f };
    var destination = new float[1];

    TensorPrimitives.Sigmoid(x, destination);

    Assert.That(destination[0], Is.EqualTo(0.5f).Within(FloatTolerance)); // sigmoid(0) = 0.5
  }

  #endregion

  #region Tanh Tests

  [Test]
  [Category("HappyPath")]
  public void Tanh_FloatSpan_ComputesTanh() {
    var x = new float[] { 0f };
    var destination = new float[1];

    TensorPrimitives.Tanh(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance)); // tanh(0) = 0
  }

  #endregion

  #region SoftMax Tests

  [Test]
  [Category("HappyPath")]
  public void SoftMax_FloatSpan_SumsToOne() {
    var x = new float[] { 1f, 2f, 3f };
    var destination = new float[3];

    TensorPrimitives.SoftMax(x, destination);

    var sum = destination[0] + destination[1] + destination[2];
    Assert.That(sum, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void SoftMax_FloatSpan_LargerInputsHaveLargerOutputs() {
    var x = new float[] { 1f, 2f, 3f };
    var destination = new float[3];

    TensorPrimitives.SoftMax(x, destination);

    Assert.That(destination[2], Is.GreaterThan(destination[1]));
    Assert.That(destination[1], Is.GreaterThan(destination[0]));
  }

  #endregion

  #region Double Precision Tests

  [Test]
  [Category("HappyPath")]
  public void Add_DoubleSpans_ComputesCorrectly() {
    var x = new double[] { 1.0, 2.0, 3.0 };
    var y = new double[] { 4.0, 5.0, 6.0 };
    var destination = new double[3];

    TensorPrimitives.Add<double>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(5.0).Within(DoubleTolerance));
    Assert.That(destination[1], Is.EqualTo(7.0).Within(DoubleTolerance));
    Assert.That(destination[2], Is.EqualTo(9.0).Within(DoubleTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Sum_DoubleSpan_ComputesCorrectly() {
    var x = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

    var result = TensorPrimitives.Sum<double>(x);

    Assert.That(result, Is.EqualTo(15.0).Within(DoubleTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Dot_DoubleSpans_ComputesCorrectly() {
    var x = new double[] { 1.0, 2.0, 3.0 };
    var y = new double[] { 4.0, 5.0, 6.0 };

    var result = TensorPrimitives.Dot<double>(x, y);

    Assert.That(result, Is.EqualTo(32.0).Within(DoubleTolerance));
  }

  #endregion

  #region Exception Tests

  [Test]
  [Category("Exception")]
  public void Add_MismatchedLengths_ThrowsArgumentException() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 4f, 5f };
    var destination = new float[3];

    Assert.Throws<ArgumentException>(() => TensorPrimitives.Add<float>(x, y, destination));
  }

  [Test]
  [Category("Exception")]
  public void Max_EmptySpan_ThrowsArgumentException() {
    var x = Array.Empty<float>();

    Assert.Throws<ArgumentException>(() => TensorPrimitives.Max<float>(x));
  }

  [Test]
  [Category("Exception")]
  public void Min_EmptySpan_ThrowsArgumentException() {
    var x = Array.Empty<float>();

    Assert.Throws<ArgumentException>(() => TensorPrimitives.Min<float>(x));
  }

  [Test]
  [Category("Exception")]
  public void Dot_MismatchedLengths_ThrowsArgumentException() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 4f, 5f };

    Assert.Throws<ArgumentException>(() => TensorPrimitives.Dot<float>(x, y));
  }

  #endregion

  #region Additional Math Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Log2_FloatSpan_ComputesBase2Logarithms() {
    var x = new float[] { 1f, 2f, 4f, 8f };
    var destination = new float[4];

    TensorPrimitives.Log2<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(3f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Floor_FloatSpan_ComputesFloorValues() {
    var x = new float[] { 1.7f, 2.3f, -1.5f, 3.0f };
    var destination = new float[4];

    TensorPrimitives.Floor<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-2f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(3f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Ceiling_FloatSpan_ComputesCeilingValues() {
    var x = new float[] { 1.1f, 2.9f, -1.5f, 3.0f };
    var destination = new float[4];

    TensorPrimitives.Ceiling<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-1f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(3f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Truncate_FloatSpan_ComputesTruncatedValues() {
    var x = new float[] { 1.7f, 2.9f, -1.5f, -2.9f };
    var destination = new float[4];

    TensorPrimitives.Truncate<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-1f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(-2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Round_FloatSpan_ComputesRoundedValues() {
    var x = new float[] { 1.4f, 1.5f, 2.5f, -1.5f };
    var destination = new float[4];

    TensorPrimitives.Round<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(-2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Hypot_FloatSpans_ComputesHypotenuse() {
    var x = new float[] { 3f, 5f, 8f };
    var y = new float[] { 4f, 12f, 15f };
    var destination = new float[3];

    TensorPrimitives.Hypot<float>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(13f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(17f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Reciprocal_FloatSpan_ComputesReciprocals() {
    var x = new float[] { 1f, 2f, 4f, 0.5f };
    var destination = new float[4];

    TensorPrimitives.Reciprocal<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(0.25f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void ReciprocalSqrt_FloatSpan_ComputesReciprocalSqrts() {
    var x = new float[] { 1f, 4f, 16f, 25f };
    var destination = new float[4];

    TensorPrimitives.ReciprocalSqrt<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(0.25f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(0.2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void SumOfMagnitudes_FloatSpan_ComputesSumOfAbsoluteValues() {
    var x = new float[] { 1f, -2f, 3f, -4f };

    var result = TensorPrimitives.SumOfMagnitudes<float>(x);

    Assert.That(result, Is.EqualTo(10f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Average_FloatSpan_ComputesAverage() {
    var x = new float[] { 1f, 2f, 3f, 4f, 5f };

    var result = TensorPrimitives.Average<float>(x);

    Assert.That(result, Is.EqualTo(3f).Within(FloatTolerance));
  }

  [Test]
  [Category("Exception")]
  public void Average_EmptySpan_ThrowsArgumentException() {
    var x = Array.Empty<float>();

    Assert.Throws<ArgumentException>(() => TensorPrimitives.Average<float>(x));
  }

  #endregion

  #region AddMultiply and MultiplyAdd Tests

  [Test]
  [Category("HappyPath")]
  public void AddMultiply_ThreeSpans_ComputesCorrectly() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 2f, 3f, 4f };
    var multiplier = new float[] { 2f, 2f, 2f };
    var destination = new float[3];

    TensorPrimitives.AddMultiply<float>(x, y, multiplier, destination);

    Assert.That(destination[0], Is.EqualTo(6f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(14f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void AddMultiply_TwoSpansScalar_ComputesCorrectly() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 2f, 3f, 4f };
    var destination = new float[3];

    TensorPrimitives.AddMultiply<float>(x, y, 2f, destination);

    Assert.That(destination[0], Is.EqualTo(6f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(14f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MultiplyAdd_ThreeSpans_ComputesCorrectly() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 2f, 3f, 4f };
    var addend = new float[] { 1f, 1f, 1f };
    var destination = new float[3];

    TensorPrimitives.MultiplyAdd<float>(x, y, addend, destination);

    Assert.That(destination[0], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(7f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(13f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MultiplyAdd_TwoSpansScalar_ComputesCorrectly() {
    var x = new float[] { 1f, 2f, 3f };
    var y = new float[] { 2f, 3f, 4f };
    var destination = new float[3];

    TensorPrimitives.MultiplyAdd<float>(x, y, 10f, destination);

    Assert.That(destination[0], Is.EqualTo(12f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(16f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(22f).Within(FloatTolerance));
  }

  #endregion

  #region Element-wise Max/Min and Magnitude Tests

  [Test]
  [Category("HappyPath")]
  public void Max_ElementWise_ComputesMaxOfEachPair() {
    var x = new float[] { 1f, 5f, 3f };
    var y = new float[] { 2f, 3f, 4f };
    var destination = new float[3];

    TensorPrimitives.Max<float>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Min_ElementWise_ComputesMinOfEachPair() {
    var x = new float[] { 1f, 5f, 3f };
    var y = new float[] { 2f, 3f, 4f };
    var destination = new float[3];

    TensorPrimitives.Min<float>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxMagnitude_FloatSpan_ReturnsValueWithMaxAbsValue() {
    var x = new float[] { 1f, -5f, 3f, -2f };

    var result = TensorPrimitives.MaxMagnitude<float>(x);

    Assert.That(result, Is.EqualTo(-5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MinMagnitude_FloatSpan_ReturnsValueWithMinAbsValue() {
    var x = new float[] { 3f, -5f, -1f, 2f };

    var result = TensorPrimitives.MinMagnitude<float>(x);

    Assert.That(result, Is.EqualTo(-1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfMaxMagnitude_FloatSpan_ReturnsCorrectIndex() {
    var x = new float[] { 1f, -5f, 3f, -2f };

    var result = TensorPrimitives.IndexOfMaxMagnitude<float>(x);

    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfMinMagnitude_FloatSpan_ReturnsCorrectIndex() {
    var x = new float[] { 3f, -5f, -1f, 2f };

    var result = TensorPrimitives.IndexOfMinMagnitude<float>(x);

    Assert.That(result, Is.EqualTo(2));
  }

  #endregion

  #region Conversion Tests

  [Test]
  [Category("HappyPath")]
  public void ConvertChecked_IntToFloat_ConvertsCorrectly() {
    var source = new int[] { 1, 2, 3, 4 };
    var destination = new float[4];

    TensorPrimitives.ConvertChecked<int, float>(source, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void ConvertTruncating_FloatToInt_TruncatesCorrectly() {
    var source = new float[] { 1.7f, 2.3f, 3.9f, 4.1f };
    var destination = new int[4];

    TensorPrimitives.ConvertTruncating<float, int>(source, destination);

    Assert.That(destination[0], Is.EqualTo(1));
    Assert.That(destination[1], Is.EqualTo(2));
    Assert.That(destination[2], Is.EqualTo(3));
    Assert.That(destination[3], Is.EqualTo(4));
  }

  #endregion

  #region Bitwise Operations Tests

  [Test]
  [Category("HappyPath")]
  public void BitwiseAnd_IntSpans_ComputesBitwiseAnd() {
    var x = new int[] { 0b1100, 0b1010, 0b1111 };
    var y = new int[] { 0b1010, 0b1100, 0b0000 };
    var destination = new int[3];

    TensorPrimitives.BitwiseAnd<int>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(0b1000));
    Assert.That(destination[1], Is.EqualTo(0b1000));
    Assert.That(destination[2], Is.EqualTo(0b0000));
  }

  [Test]
  [Category("HappyPath")]
  public void BitwiseOr_IntSpans_ComputesBitwiseOr() {
    var x = new int[] { 0b1100, 0b1010, 0b0000 };
    var y = new int[] { 0b1010, 0b1100, 0b1111 };
    var destination = new int[3];

    TensorPrimitives.BitwiseOr<int>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(0b1110));
    Assert.That(destination[1], Is.EqualTo(0b1110));
    Assert.That(destination[2], Is.EqualTo(0b1111));
  }

  [Test]
  [Category("HappyPath")]
  public void Xor_IntSpans_ComputesXor() {
    var x = new int[] { 0b1100, 0b1010, 0b1111 };
    var y = new int[] { 0b1010, 0b1010, 0b1111 };
    var destination = new int[3];

    TensorPrimitives.Xor<int>(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(0b0110));
    Assert.That(destination[1], Is.EqualTo(0b0000));
    Assert.That(destination[2], Is.EqualTo(0b0000));
  }

  [Test]
  [Category("HappyPath")]
  public void OnesComplement_ByteSpan_ComputesComplement() {
    var x = new byte[] { 0b00001111, 0b11110000, 0b10101010 };
    var destination = new byte[3];

    TensorPrimitives.OnesComplement<byte>(x, destination);

    Assert.That(destination[0], Is.EqualTo((byte)0b11110000));
    Assert.That(destination[1], Is.EqualTo((byte)0b00001111));
    Assert.That(destination[2], Is.EqualTo((byte)0b01010101));
  }

  #endregion

  #region Shift Operations Tests

  [Test]
  [Category("HappyPath")]
  public void ShiftLeft_IntSpan_ShiftsLeftCorrectly() {
    var x = new int[] { 1, 2, 4 };
    var destination = new int[3];

    TensorPrimitives.ShiftLeft<int>(x, 2, destination);

    Assert.That(destination[0], Is.EqualTo(4));
    Assert.That(destination[1], Is.EqualTo(8));
    Assert.That(destination[2], Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void ShiftRightArithmetic_IntSpan_ShiftsRightPreservingSign() {
    var x = new int[] { 8, 16, -8 };
    var destination = new int[3];

    TensorPrimitives.ShiftRightArithmetic<int>(x, 2, destination);

    Assert.That(destination[0], Is.EqualTo(2));
    Assert.That(destination[1], Is.EqualTo(4));
    Assert.That(destination[2], Is.EqualTo(-2));
  }

  [Test]
  [Category("HappyPath")]
  public void ShiftRightLogical_IntSpan_ShiftsRightWithZeroFill() {
    var x = new int[] { 8, 16 };
    var destination = new int[2];

    TensorPrimitives.ShiftRightLogical<int>(x, 2, destination);

    Assert.That(destination[0], Is.EqualTo(2));
    Assert.That(destination[1], Is.EqualTo(4));
  }

  #endregion

  #region Trigonometric Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Sin_FloatSpan_ComputesSineValues() {
    var x = new float[] { 0f, MathF.PI / 2f, MathF.PI };
    var destination = new float[3];

    TensorPrimitives.Sin(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Cos_FloatSpan_ComputesCosineValues() {
    var x = new float[] { 0f, MathF.PI / 2f, MathF.PI };
    var destination = new float[3];

    TensorPrimitives.Cos(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Tan_FloatSpan_ComputesTangentValues() {
    var x = new float[] { 0f, MathF.PI / 4f };
    var destination = new float[2];

    TensorPrimitives.Tan(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Sinh_FloatSpan_ComputesHyperbolicSine() {
    var x = new float[] { 0f };
    var destination = new float[1];

    TensorPrimitives.Sinh(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Cosh_FloatSpan_ComputesHyperbolicCosine() {
    var x = new float[] { 0f };
    var destination = new float[1];

    TensorPrimitives.Cosh(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Asin_FloatSpan_ComputesArcSine() {
    var x = new float[] { 0f, 1f };
    var destination = new float[2];

    TensorPrimitives.Asin(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(MathF.PI / 2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Acos_FloatSpan_ComputesArcCosine() {
    var x = new float[] { 1f, 0f };
    var destination = new float[2];

    TensorPrimitives.Acos(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(MathF.PI / 2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Atan_FloatSpan_ComputesArcTangent() {
    var x = new float[] { 0f, 1f };
    var destination = new float[2];

    TensorPrimitives.Atan(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(MathF.PI / 4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Atan2_FloatSpans_ComputesAtan2() {
    var y = new float[] { 1f, 0f };
    var x = new float[] { 0f, 1f };
    var destination = new float[2];

    TensorPrimitives.Atan2(y, x, destination);

    Assert.That(destination[0], Is.EqualTo(MathF.PI / 2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Asinh_FloatSpan_ComputesInverseHyperbolicSine() {
    var x = new float[] { 0f };
    var destination = new float[1];

    TensorPrimitives.Asinh(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Acosh_FloatSpan_ComputesInverseHyperbolicCosine() {
    var x = new float[] { 1f };
    var destination = new float[1];

    TensorPrimitives.Acosh(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Atanh_FloatSpan_ComputesInverseHyperbolicTangent() {
    var x = new float[] { 0f };
    var destination = new float[1];

    TensorPrimitives.Atanh(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void SinCos_FloatSpan_ComputesBothSineAndCosine() {
    var x = new float[] { 0f, MathF.PI / 2f };
    var sinDestination = new float[2];
    var cosDestination = new float[2];

    TensorPrimitives.SinCos(x, sinDestination, cosDestination);

    Assert.That(sinDestination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(sinDestination[1], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(cosDestination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(cosDestination[1], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void SinPi_FloatSpan_ComputesSineOfPiMultiple() {
    var x = new float[] { 0f, 0.5f, 1f };
    var destination = new float[3];

    TensorPrimitives.SinPi(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CosPi_FloatSpan_ComputesCosineOfPiMultiple() {
    var x = new float[] { 0f, 0.5f, 1f };
    var destination = new float[3];

    TensorPrimitives.CosPi(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void TanPi_FloatSpan_ComputesTangentOfPiMultiple() {
    var x = new float[] { 0f, 0.25f };
    var destination = new float[2];

    TensorPrimitives.TanPi(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void DegreesToRadians_FloatSpan_ConvertsCorrectly() {
    var x = new float[] { 0f, 90f, 180f, 360f };
    var destination = new float[4];

    TensorPrimitives.DegreesToRadians(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(MathF.PI / 2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(MathF.PI).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(2f * MathF.PI).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void RadiansToDegrees_FloatSpan_ConvertsCorrectly() {
    var x = new float[] { 0f, MathF.PI / 2f, MathF.PI, 2f * MathF.PI };
    var destination = new float[4];

    TensorPrimitives.RadiansToDegrees(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(90f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(180f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(360f).Within(FloatTolerance));
  }

  #endregion

  #region Additional Math Operations Tests (New)

  [Test]
  [Category("HappyPath")]
  public void Log10_FloatSpan_ComputesBase10Logarithms() {
    var x = new float[] { 1f, 10f, 100f, 1000f };
    var destination = new float[4];

    TensorPrimitives.Log10(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(3f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Cbrt_FloatSpan_ComputesCubeRoots() {
    var x = new float[] { 1f, 8f, 27f, -8f };
    var destination = new float[4];

    TensorPrimitives.Cbrt(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(-2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Pow_FloatSpans_ComputesPowers() {
    var x = new float[] { 2f, 3f, 4f };
    var y = new float[] { 2f, 2f, 2f };
    var destination = new float[3];

    TensorPrimitives.Pow(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(9f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(16f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Pow_FloatSpanScalar_ComputesPowers() {
    var x = new float[] { 2f, 3f, 4f };
    var destination = new float[3];

    TensorPrimitives.Pow(x, 2f, destination);

    Assert.That(destination[0], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(9f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(16f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CopySign_FloatSpans_CopiesSignCorrectly() {
    var x = new float[] { 1f, -2f, 3f, -4f };
    var y = new float[] { -1f, 1f, -1f, 1f };
    var destination = new float[4];

    TensorPrimitives.CopySign(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(-1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-3f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void FusedMultiplyAdd_FloatSpans_ComputesFMA() {
    var x = new float[] { 2f, 3f, 4f };
    var y = new float[] { 3f, 4f, 5f };
    var addend = new float[] { 1f, 2f, 3f };
    var destination = new float[3];

    TensorPrimitives.FusedMultiplyAdd(x, y, addend, destination);

    Assert.That(destination[0], Is.EqualTo(7f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(14f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(23f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Exp2_FloatSpan_ComputesPowersOf2() {
    var x = new float[] { 0f, 1f, 2f, 3f };
    var destination = new float[4];

    TensorPrimitives.Exp2(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(8f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Exp10_FloatSpan_ComputesPowersOf10() {
    var x = new float[] { 0f, 1f, 2f };
    var destination = new float[3];

    TensorPrimitives.Exp10(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(100f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void LogP1_FloatSpan_ComputesLogOfOnePlusX() {
    var x = new float[] { 0f, MathF.E - 1f };
    var destination = new float[2];

    TensorPrimitives.LogP1(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Log2P1_FloatSpan_ComputesLog2OfOnePlusX() {
    var x = new float[] { 0f, 1f, 3f };
    var destination = new float[3];

    TensorPrimitives.Log2P1(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpM1_FloatSpan_ComputesExpMinusOne() {
    var x = new float[] { 0f };
    var destination = new float[1];

    TensorPrimitives.ExpM1(x, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Log_FloatSpanWithBase_ComputesLogarithmWithBase() {
    var x = new float[] { 1f, 8f, 27f };
    var baseVal = new float[] { 2f, 2f, 3f };
    var destination = new float[3];

    TensorPrimitives.Log(x, baseVal, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3f).Within(FloatTolerance));
  }

  #endregion

  #region Comparison Operations Tests (New)

  [Test]
  [Category("HappyPath")]
  public void Max_SpanAndScalar_ComputesMaxWithScalar() {
    var x = new float[] { 1f, 5f, 3f, 7f };
    var destination = new float[4];

    TensorPrimitives.Max(x, 4f, destination);

    Assert.That(destination[0], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(7f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Min_SpanAndScalar_ComputesMinWithScalar() {
    var x = new float[] { 1f, 5f, 3f, 7f };
    var destination = new float[4];

    TensorPrimitives.Min(x, 4f, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxMagnitude_ElementWise_ComputesMaxMagnitudeOfEachPair() {
    var x = new float[] { -3f, 2f, -5f };
    var y = new float[] { 2f, -4f, 4f };
    var destination = new float[3];

    TensorPrimitives.MaxMagnitude(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(-3f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(-4f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MinMagnitude_ElementWise_ComputesMinMagnitudeOfEachPair() {
    var x = new float[] { -3f, 2f, -5f };
    var y = new float[] { 2f, -4f, 4f };
    var destination = new float[3];

    TensorPrimitives.MinMagnitude(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(4f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxNumber_FloatSpan_ReturnsMax() {
    var x = new float[] { 3f, 1f, 5f, 2f };

    var result = TensorPrimitives.MaxNumber<float>(x);

    Assert.That(result, Is.EqualTo(5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MinNumber_FloatSpan_ReturnsMin() {
    var x = new float[] { 3f, 1f, 5f, 2f };

    var result = TensorPrimitives.MinNumber<float>(x);

    Assert.That(result, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxNumber_ElementWise_ComputesMax() {
    var x = new float[] { 3f, 1f, 2f };
    var y = new float[] { 2f, 4f, 1f };
    var destination = new float[3];

    TensorPrimitives.MaxNumber(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(4f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(2f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MinNumber_ElementWise_ComputesMin() {
    var x = new float[] { 3f, 1f, 2f };
    var y = new float[] { 2f, 4f, 1f };
    var destination = new float[3];

    TensorPrimitives.MinNumber(x, y, destination);

    Assert.That(destination[0], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_SpanWithBounds_ClampsToRange() {
    var x = new float[] { -5f, 0f, 5f, 10f, 15f };
    var min = new float[] { 0f, 0f, 0f, 0f, 0f };
    var max = new float[] { 10f, 10f, 10f, 10f, 10f };
    var destination = new float[5];

    TensorPrimitives.Clamp(x, min, max, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(destination[4], Is.EqualTo(10f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_SpanWithScalarBounds_ClampsToRange() {
    var x = new float[] { -5f, 0f, 5f, 10f, 15f };
    var destination = new float[5];

    TensorPrimitives.Clamp(x, 0f, 10f, destination);

    Assert.That(destination[0], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(5f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(10f).Within(FloatTolerance));
    Assert.That(destination[4], Is.EqualTo(10f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxMagnitudeNumber_FloatSpan_ReturnsMaxMagnitude() {
    var x = new float[] { -3f, 1f, 2f, -5f };

    var result = TensorPrimitives.MaxMagnitudeNumber<float>(x);

    Assert.That(result, Is.EqualTo(-5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MinMagnitudeNumber_FloatSpan_ReturnsMinMagnitude() {
    var x = new float[] { -3f, 1f, 2f, -5f };

    var result = TensorPrimitives.MinMagnitudeNumber<float>(x);

    Assert.That(result, Is.EqualTo(1f).Within(FloatTolerance));
  }

  #endregion

  #region Round Overloads Tests

  [Test]
  [Category("HappyPath")]
  public void Round_FloatSpanWithDigits_RoundsToSpecifiedDecimalPlaces() {
    var x = new float[] { 1.234f, 2.567f, 3.891f };
    var destination = new float[3];

    TensorPrimitives.Round(x, 2, destination);

    Assert.That(destination[0], Is.EqualTo(1.23f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(2.57f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(3.89f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Round_FloatSpanWithMidpointRounding_RoundsAwayFromZero() {
    var x = new float[] { 1.5f, 2.5f, -1.5f, -2.5f };
    var destination = new float[4];

    TensorPrimitives.Round(x, MidpointRounding.AwayFromZero, destination);

    Assert.That(destination[0], Is.EqualTo(2f).Within(FloatTolerance));
    Assert.That(destination[1], Is.EqualTo(3f).Within(FloatTolerance));
    Assert.That(destination[2], Is.EqualTo(-2f).Within(FloatTolerance));
    Assert.That(destination[3], Is.EqualTo(-3f).Within(FloatTolerance));
  }

  #endregion

  #region ReciprocalEstimate Tests

  [Test]
  [Category("HappyPath")]
  public void ReciprocalEstimate_FloatSpan_ComputesApproximateReciprocals() {
    var x = new float[] { 1f, 2f, 4f };
    var destination = new float[3];

    TensorPrimitives.ReciprocalEstimate<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(0.01f));
    Assert.That(destination[1], Is.EqualTo(0.5f).Within(0.01f));
    Assert.That(destination[2], Is.EqualTo(0.25f).Within(0.01f));
  }

  [Test]
  [Category("HappyPath")]
  public void ReciprocalSqrtEstimate_FloatSpan_ComputesApproximateReciprocalSqrts() {
    var x = new float[] { 1f, 4f, 16f };
    var destination = new float[3];

    TensorPrimitives.ReciprocalSqrtEstimate<float>(x, destination);

    Assert.That(destination[0], Is.EqualTo(1f).Within(0.01f));
    Assert.That(destination[1], Is.EqualTo(0.5f).Within(0.01f));
    Assert.That(destination[2], Is.EqualTo(0.25f).Within(0.01f));
  }

  #endregion

}
