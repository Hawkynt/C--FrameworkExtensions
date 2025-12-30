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

using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.InteropServices;
using Hawkynt.ColorProcessing.Codecs;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("NeighborWindow")]
public class NeighborWindowTests {

  #region Test Helpers

  /// <summary>
  /// Identity decoder: TPixel == TWork, just pass through.
  /// </summary>
  private readonly struct IdentityDecode : IDecode<TestInt32, TestInt32> {
    public TestInt32 Decode(in TestInt32 pixel) => pixel;
  }

  /// <summary>
  /// Identity projector: TWork == TKey, just pass through.
  /// </summary>
  private readonly struct IdentityProject : IProject<TestInt32, TestInt32> {
    public TestInt32 Project(in TestInt32 color) => color;
  }

  /// <summary>
  /// Creates a test image with sequential values.
  /// </summary>
  private static TestInt32[] CreateSequentialImage(int width, int height) {
    var result = new TestInt32[width * height];
    for (var i = 0; i < result.Length; ++i)
      result[i] = i;
    return result;
  }

  #endregion

  #region MoveRight Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void MoveRight_MovesWindowByOnePixel() {
    const int width = 10;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      var window = frame.GetWindow();
      var firstCenter = (int)window.P0P0.Work;

      window.MoveRight();
      var secondCenter = (int)window.P0P0.Work;

      Assert.That(secondCenter, Is.EqualTo(firstCenter + 1));
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void MoveRight_MultipleMoves_TraversesRow() {
    const int width = 10;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      var window = frame.GetWindow();

      for (var x = 0; x < width; ++x) {
        Assert.That((int)window.P0P0.Work, Is.EqualTo(x), $"Center pixel at x={x}");
        if (x < width - 1)
          window.MoveRight();
      }
    }
  }

  #endregion

  #region 5x5 Accessor Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void All25Accessors_ReturnCorrectNeighbors() {
    const int width = 10;
    const int height = 10;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      var window = frame.GetWindow();

      // Move to position (3, 2) - away from edges to avoid OOB clamping
      frame.MoveDown();
      frame.MoveDown();
      window = frame.GetWindow();
      window.MoveRight();
      window.MoveRight();
      window.MoveRight();

      var centerX = 3;
      var centerY = 2;
      var center = centerY * width + centerX;

      // Row -2
      Assert.That((int)window.M2M2.Work, Is.EqualTo((centerY - 2) * width + (centerX - 2)));
      Assert.That((int)window.M2M1.Work, Is.EqualTo((centerY - 2) * width + (centerX - 1)));
      Assert.That((int)window.M2P0.Work, Is.EqualTo((centerY - 2) * width + centerX));
      Assert.That((int)window.M2P1.Work, Is.EqualTo((centerY - 2) * width + (centerX + 1)));
      Assert.That((int)window.M2P2.Work, Is.EqualTo((centerY - 2) * width + (centerX + 2)));

      // Row -1
      Assert.That((int)window.M1M2.Work, Is.EqualTo((centerY - 1) * width + (centerX - 2)));
      Assert.That((int)window.M1M1.Work, Is.EqualTo((centerY - 1) * width + (centerX - 1)));
      Assert.That((int)window.M1P0.Work, Is.EqualTo((centerY - 1) * width + centerX));
      Assert.That((int)window.M1P1.Work, Is.EqualTo((centerY - 1) * width + (centerX + 1)));
      Assert.That((int)window.M1P2.Work, Is.EqualTo((centerY - 1) * width + (centerX + 2)));

      // Row 0 (center)
      Assert.That((int)window.P0M2.Work, Is.EqualTo(centerY * width + (centerX - 2)));
      Assert.That((int)window.P0M1.Work, Is.EqualTo(centerY * width + (centerX - 1)));
      Assert.That((int)window.P0P0.Work, Is.EqualTo(center));
      Assert.That((int)window.P0P1.Work, Is.EqualTo(centerY * width + (centerX + 1)));
      Assert.That((int)window.P0P2.Work, Is.EqualTo(centerY * width + (centerX + 2)));

      // Row +1
      Assert.That((int)window.P1M2.Work, Is.EqualTo((centerY + 1) * width + (centerX - 2)));
      Assert.That((int)window.P1M1.Work, Is.EqualTo((centerY + 1) * width + (centerX - 1)));
      Assert.That((int)window.P1P0.Work, Is.EqualTo((centerY + 1) * width + centerX));
      Assert.That((int)window.P1P1.Work, Is.EqualTo((centerY + 1) * width + (centerX + 1)));
      Assert.That((int)window.P1P2.Work, Is.EqualTo((centerY + 1) * width + (centerX + 2)));

      // Row +2
      Assert.That((int)window.P2M2.Work, Is.EqualTo((centerY + 2) * width + (centerX - 2)));
      Assert.That((int)window.P2M1.Work, Is.EqualTo((centerY + 2) * width + (centerX - 1)));
      Assert.That((int)window.P2P0.Work, Is.EqualTo((centerY + 2) * width + centerX));
      Assert.That((int)window.P2P1.Work, Is.EqualTo((centerY + 2) * width + (centerX + 1)));
      Assert.That((int)window.P2P2.Work, Is.EqualTo((centerY + 2) * width + (centerX + 2)));
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void WorkAndKey_BothAccessible() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      var window = frame.GetWindow();
      var pixel = window.P0P0;

      Assert.That((int)pixel.Work, Is.EqualTo(0));
      Assert.That((int)pixel.Key, Is.EqualTo(0));
    }
  }

  #endregion

  #region ResetX Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void ResetX_ResetsWindowToStart() {
    const int width = 10;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      var window = frame.GetWindow();

      // Move to middle of row
      for (var i = 0; i < 5; ++i)
        window.MoveRight();

      Assert.That((int)window.P0P0.Work, Is.EqualTo(5));

      // Reset to start
      window.ResetX(2); // 2 is the OOB padding offset

      Assert.That((int)window.P0P0.Work, Is.EqualTo(0));
    }
  }

  #endregion

}
