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

// System.Numerics types require SUPPORTS_VECTOR (netstandard2.0+)
#if SUPPORTS_VECTOR

using System;
using System.Numerics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Numerics")]
public class NumericsPolyfillTests {

#if !SUPPORTS_MATRIX3X2_INDEXER

  #region Matrix3x2 - Indexer (get_Item)

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_Indexer_Row0Col0_ReturnsM11() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    Assert.That(matrix.get_Item(0, 0), Is.EqualTo(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_Indexer_Row0Col1_ReturnsM12() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    Assert.That(matrix.get_Item(0, 1), Is.EqualTo(2f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_Indexer_Row1Col0_ReturnsM21() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    Assert.That(matrix.get_Item(1, 0), Is.EqualTo(3f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_Indexer_Row2Col1_ReturnsM32() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    Assert.That(matrix.get_Item(2, 1), Is.EqualTo(6f));
  }

  [Test]
  [Category("Exception")]
  public void Matrix3x2_Indexer_RowOutOfRange_ThrowsArgumentOutOfRangeException() {
    var matrix = Matrix3x2.Identity;
    Assert.Throws<ArgumentOutOfRangeException>(() => _ = matrix.get_Item(3, 0));
  }

  [Test]
  [Category("Exception")]
  public void Matrix3x2_Indexer_ColumnOutOfRange_ThrowsArgumentOutOfRangeException() {
    var matrix = Matrix3x2.Identity;
    Assert.Throws<ArgumentOutOfRangeException>(() => _ = matrix.get_Item(0, 2));
  }

  #endregion

#endif

#if !SUPPORTS_MATRIX3X2_CREATE

  #region Matrix3x2 - Create Factory Methods

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_Create_Float_SetsAllElements() {
    var matrix = Matrix3x2Polyfills.Create(5f);
    Assert.That(matrix.M11, Is.EqualTo(5f));
    Assert.That(matrix.M32, Is.EqualTo(5f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_Create_Vector2_BroadcastsToAllRows() {
    var matrix = Matrix3x2Polyfills.Create(new Vector2(1, 2));
    Assert.That(matrix.M11, Is.EqualTo(1f));
    Assert.That(matrix.M12, Is.EqualTo(2f));
    Assert.That(matrix.M21, Is.EqualTo(1f));
    Assert.That(matrix.M32, Is.EqualTo(2f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_Create_ThreeVector2s_SetsRows() {
    var matrix = Matrix3x2Polyfills.Create(new Vector2(1, 2), new Vector2(3, 4), new Vector2(5, 6));
    Assert.That(matrix.M11, Is.EqualTo(1f));
    Assert.That(matrix.M22, Is.EqualTo(4f));
    Assert.That(matrix.M31, Is.EqualTo(5f));
  }

  #endregion

  #region Matrix3x2 - GetRow / WithRow

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_GetRow_Row0_ReturnsFirstRow() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    var row = matrix.GetRow(0);
    Assert.That(row.X, Is.EqualTo(1f));
    Assert.That(row.Y, Is.EqualTo(2f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_GetRow_Row2_ReturnsThirdRow() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    var row = matrix.GetRow(2);
    Assert.That(row.X, Is.EqualTo(5f));
    Assert.That(row.Y, Is.EqualTo(6f));
  }

  [Test]
  [Category("Exception")]
  public void Matrix3x2_GetRow_InvalidIndex_ThrowsArgumentOutOfRangeException() {
    var matrix = Matrix3x2.Identity;
    Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetRow(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_WithRow_Row0_SetsFirstRow() {
    var matrix = Matrix3x2.Identity;
    var result = matrix.WithRow(0, new Vector2(7, 8));
    Assert.That(result.M11, Is.EqualTo(7f));
    Assert.That(result.M12, Is.EqualTo(8f));
    Assert.That(result.M21, Is.EqualTo(0f));
  }

  #endregion

  #region Matrix3x2 - Row Properties (GetX/Y/Z, WithX/Y/Z)

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_GetX_ReturnsFirstRow() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    var row = matrix.X;
    Assert.That(row.X, Is.EqualTo(1f));
    Assert.That(row.Y, Is.EqualTo(2f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_GetY_ReturnsSecondRow() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    var row = matrix.Y;
    Assert.That(row.X, Is.EqualTo(3f));
    Assert.That(row.Y, Is.EqualTo(4f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_GetZ_ReturnsThirdRow() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    var row = matrix.Z;
    Assert.That(row.X, Is.EqualTo(5f));
    Assert.That(row.Y, Is.EqualTo(6f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix3x2_WithX_SetsFirstRow() {
    var matrix = new Matrix3x2(1, 2, 3, 4, 5, 6);
    var result = matrix.WithX(new Vector2(10, 20));
    Assert.That(result.M11, Is.EqualTo(10f));
    Assert.That(result.M12, Is.EqualTo(20f));
    Assert.That(result.M21, Is.EqualTo(3f));
  }

  #endregion

#endif

#if !SUPPORTS_MATRIX4X4_CREATE

  #region Matrix4x4 - GetRow / WithRow

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_GetRow_Row0_ReturnsFirstRow() {
    var matrix = new Matrix4x4(
      1, 2, 3, 4,
      5, 6, 7, 8,
      9, 10, 11, 12,
      13, 14, 15, 16
    );
    var row = matrix.GetRow(0);
    Assert.That(row.X, Is.EqualTo(1f));
    Assert.That(row.W, Is.EqualTo(4f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_GetRow_Row3_ReturnsFourthRow() {
    var matrix = new Matrix4x4(
      1, 2, 3, 4,
      5, 6, 7, 8,
      9, 10, 11, 12,
      13, 14, 15, 16
    );
    var row = matrix.GetRow(3);
    Assert.That(row.X, Is.EqualTo(13f));
    Assert.That(row.W, Is.EqualTo(16f));
  }

  [Test]
  [Category("Exception")]
  public void Matrix4x4_GetRow_InvalidIndex_ThrowsArgumentOutOfRangeException() {
    var matrix = Matrix4x4.Identity;
    Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetRow(4));
  }

  #endregion

  #region Matrix4x4 - Row Properties

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_GetX_ReturnsFirstRow() {
    var matrix = new Matrix4x4(
      1, 2, 3, 4,
      5, 6, 7, 8,
      9, 10, 11, 12,
      13, 14, 15, 16
    );
    var row = matrix.X;
    Assert.That(row, Is.EqualTo(new Vector4(1, 2, 3, 4)));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_GetW_ReturnsFourthRow() {
    var matrix = new Matrix4x4(
      1, 2, 3, 4,
      5, 6, 7, 8,
      9, 10, 11, 12,
      13, 14, 15, 16
    );
    var row = matrix.W;
    Assert.That(row, Is.EqualTo(new Vector4(13, 14, 15, 16)));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_WithX_SetsFirstRow() {
    var matrix = Matrix4x4.Identity;
    var result = matrix.WithX(new Vector4(1, 2, 3, 4));
    Assert.That(result.M11, Is.EqualTo(1f));
    Assert.That(result.M14, Is.EqualTo(4f));
    Assert.That(result.M21, Is.EqualTo(0f));
  }

  #endregion

  #region Matrix4x4 - Create Factory Methods

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_Create_Float_SetsAllElements() {
    var matrix = Matrix4x4Polyfills.Create(5f);
    Assert.That(matrix.M11, Is.EqualTo(5f));
    Assert.That(matrix.M44, Is.EqualTo(5f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_Create_Vector4_BroadcastsToAllRows() {
    var matrix = Matrix4x4Polyfills.Create(new Vector4(1, 2, 3, 4));
    Assert.That(matrix.M11, Is.EqualTo(1f));
    Assert.That(matrix.M14, Is.EqualTo(4f));
    Assert.That(matrix.M21, Is.EqualTo(1f));
    Assert.That(matrix.M41, Is.EqualTo(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_Create_FromMatrix3x2_SetsCorrectElements() {
    var source = new Matrix3x2(1, 2, 3, 4, 5, 6);
    var matrix = Matrix4x4Polyfills.Create(source);
    Assert.That(matrix.M11, Is.EqualTo(1f));
    Assert.That(matrix.M12, Is.EqualTo(2f));
    Assert.That(matrix.M33, Is.EqualTo(1f));
    Assert.That(matrix.M44, Is.EqualTo(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_CreateBillboardLeftHanded_ReturnsValidMatrix() {
    var objectPos = new Vector3(0, 0, 0);
    var cameraPos = new Vector3(0, 0, -5);
    var cameraUp = new Vector3(0, 1, 0);
    var cameraForward = new Vector3(0, 0, 1);

    var matrix = Matrix4x4Polyfills.CreateBillboardLeftHanded(objectPos, cameraPos, cameraUp, cameraForward);

    Assert.That(matrix.M44, Is.EqualTo(1f));
    Assert.That(float.IsNaN(matrix.M11), Is.False);
  }

  #endregion

#endif

#if !SUPPORTS_MATRIX4X4_LEFTHANDED

  #region Matrix4x4 - Left-Handed Methods

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_CreateLookAtLeftHanded_ReturnsValidMatrix() {
    var cameraPos = new Vector3(0, 0, -5);
    var target = new Vector3(0, 0, 0);
    var up = new Vector3(0, 1, 0);
    var matrix = Matrix4x4Polyfills.CreateLookAtLeftHanded(cameraPos, target, up);

    Assert.That(matrix.M44, Is.EqualTo(1f));
    Assert.That(float.IsNaN(matrix.M11), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_CreateLookToLeftHanded_ReturnsValidMatrix() {
    var cameraPos = new Vector3(0, 0, -5);
    var direction = new Vector3(0, 0, 1);
    var up = new Vector3(0, 1, 0);

    var matrix = Matrix4x4Polyfills.CreateLookToLeftHanded(cameraPos, direction, up);

    Assert.That(matrix.M44, Is.EqualTo(1f));
    Assert.That(float.IsNaN(matrix.M11), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_CreateOrthographicLeftHanded_ReturnsValidMatrix() {
    var matrix = Matrix4x4Polyfills.CreateOrthographicLeftHanded(800, 600, 0.1f, 1000f);

    Assert.That(matrix.M44, Is.EqualTo(1f));
    Assert.That(matrix.M11, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_CreatePerspectiveFieldOfViewLeftHanded_ReturnsValidMatrix() {
    var fov = MathF.PI / 4;
    var matrix = Matrix4x4Polyfills.CreatePerspectiveFieldOfViewLeftHanded(fov, 16f / 9f, 0.1f, 1000f);

    Assert.That(matrix.M11, Is.GreaterThan(0f));
    Assert.That(matrix.M34, Is.EqualTo(1f));
  }

  [Test]
  [Category("Exception")]
  public void Matrix4x4_CreatePerspectiveFieldOfViewLeftHanded_InvalidFov_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      Matrix4x4Polyfills.CreatePerspectiveFieldOfViewLeftHanded(0, 1, 0.1f, 1000f));
  }

  [Test]
  [Category("HappyPath")]
  public void Matrix4x4_CreateViewport_ReturnsValidMatrix() {
    var matrix = Matrix4x4Polyfills.CreateViewport(0, 0, 800, 600, 0, 1);

    Assert.That(matrix.M11, Is.EqualTo(400f));
    Assert.That(matrix.M22, Is.EqualTo(-300f));
  }

  #endregion

#endif

#if !SUPPORTS_PLANE_CREATE

  #region Plane - Create Factory Methods

  [Test]
  [Category("HappyPath")]
  public void Plane_Create_Vector4_SetsCorrectValues() {
    var plane = PlanePolyfills.Create(new Vector4(1, 0, 0, 5));
    Assert.That(plane.Normal.X, Is.EqualTo(1f));
    Assert.That(plane.D, Is.EqualTo(5f));
  }

  [Test]
  [Category("HappyPath")]
  public void Plane_Create_NormalAndD_SetsCorrectValues() {
    var plane = PlanePolyfills.Create(new Vector3(0, 1, 0), 10f);
    Assert.That(plane.Normal.Y, Is.EqualTo(1f));
    Assert.That(plane.D, Is.EqualTo(10f));
  }

  [Test]
  [Category("HappyPath")]
  public void Plane_Create_FourFloats_SetsCorrectValues() {
    var plane = PlanePolyfills.Create(1, 2, 3, 4);
    Assert.That(plane.Normal.X, Is.EqualTo(1f));
    Assert.That(plane.Normal.Z, Is.EqualTo(3f));
    Assert.That(plane.D, Is.EqualTo(4f));
  }

  #endregion

#endif

#if !SUPPORTS_QUATERNION_ZERO_INDEXER

  #region Quaternion - Zero Property

  [Test]
  [Category("HappyPath")]
  public void Quaternion_Zero_AllComponentsZero() {
    var zero = QuaternionPolyfills.Zero;
    Assert.That(zero.X, Is.EqualTo(0f));
    Assert.That(zero.Y, Is.EqualTo(0f));
    Assert.That(zero.Z, Is.EqualTo(0f));
    Assert.That(zero.W, Is.EqualTo(0f));
  }

  #endregion

  #region Quaternion - GetElement / WithElement

  [Test]
  [Category("HappyPath")]
  public void Quaternion_GetElement_Index0_ReturnsX() {
    var quat = new Quaternion(1, 2, 3, 4);
    Assert.That(quat.GetElement(0), Is.EqualTo(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Quaternion_GetElement_Index3_ReturnsW() {
    var quat = new Quaternion(1, 2, 3, 4);
    Assert.That(quat.GetElement(3), Is.EqualTo(4f));
  }

  [Test]
  [Category("Exception")]
  public void Quaternion_GetElement_InvalidIndex_ThrowsArgumentOutOfRangeException() {
    var quat = Quaternion.Identity;
    Assert.Throws<ArgumentOutOfRangeException>(() => quat.GetElement(4));
    Assert.Throws<ArgumentOutOfRangeException>(() => quat.GetElement(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Quaternion_WithElement_Index0_SetsX() {
    var quat = Quaternion.Identity;
    var result = quat.WithElement(0, 99f);
    Assert.That(result.X, Is.EqualTo(99f));
    Assert.That(result.W, Is.EqualTo(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Quaternion_WithElement_DoesNotModifyOriginal() {
    var original = Quaternion.Identity;
    _ = original.WithElement(0, 99f);
    Assert.That(original.X, Is.EqualTo(0f));
  }

  [Test]
  [Category("Exception")]
  public void Quaternion_WithElement_InvalidIndex_ThrowsArgumentOutOfRangeException() {
    var quat = Quaternion.Identity;
    Assert.Throws<ArgumentOutOfRangeException>(() => quat.WithElement(4, 1f));
  }

  #endregion

#endif

#if !SUPPORTS_QUATERNION_CREATE

  #region Quaternion - Create Factory Methods

  [Test]
  [Category("HappyPath")]
  public void Quaternion_Create_VectorAndScalar_SetsCorrectValues() {
    var quat = QuaternionPolyfills.Create(new Vector3(1, 2, 3), 4);
    Assert.That(quat.X, Is.EqualTo(1f));
    Assert.That(quat.Y, Is.EqualTo(2f));
    Assert.That(quat.Z, Is.EqualTo(3f));
    Assert.That(quat.W, Is.EqualTo(4f));
  }

  [Test]
  [Category("HappyPath")]
  public void Quaternion_Create_FourFloats_SetsCorrectValues() {
    var quat = QuaternionPolyfills.Create(1, 2, 3, 4);
    Assert.That(quat.X, Is.EqualTo(1f));
    Assert.That(quat.W, Is.EqualTo(4f));
  }

  #endregion

#endif

}

#endif
