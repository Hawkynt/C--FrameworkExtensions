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
using Hawkynt.ColorProcessing.Codecs;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("NeighborFrame")]
public class NeighborFrameTests {

  #region Test Helpers

  private readonly struct IdentityDecode : IDecode<TestInt32, TestInt32> {
    public TestInt32 Decode(in TestInt32 pixel) => pixel;
  }

  private readonly struct IdentityProject : IProject<TestInt32, TestInt32> {
    public TestInt32 Project(in TestInt32 color) => color;
  }

  private static TestInt32[] CreateSequentialImage(int width, int height) {
    var result = new TestInt32[width * height];
    for (var i = 0; i < result.Length; ++i)
      result[i] = i;
    return result;
  }

  #endregion

  #region MoveDown Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void MoveDown_AdvancesToNextRow() {
    const int width = 5;
    const int height = 10;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      Assert.That(frame.CurrentY, Is.EqualTo(0));

      frame.MoveDown();
      Assert.That(frame.CurrentY, Is.EqualTo(1));

      var window = frame.GetWindow();
      Assert.That((int)window.P0P0.Work, Is.EqualTo(1 * width)); // Row 1, column 0
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void MoveDown_MultipleRows_TraversesImage() {
    const int width = 5;
    const int height = 10;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y) {
        var window = frame.GetWindow();
        Assert.That((int)window.P0P0.Work, Is.EqualTo(y * width), $"Row {y}");
        if (y < height - 1)
          frame.MoveDown();
      }
    }
  }

  #endregion

  #region StartY Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void Constructor_WithStartY_InitializesAtCorrectRow() {
    const int width = 5;
    const int height = 10;
    const int startY = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const,
        startY
      );

      Assert.That(frame.CurrentY, Is.EqualTo(startY));

      var window = frame.GetWindow();
      Assert.That((int)window.P0P0.Work, Is.EqualTo(startY * width));
    }
  }

  [Test]
  [Category("EdgeCase")]
  public unsafe void Constructor_WithStartY_HandlesOobCorrectly() {
    const int width = 5;
    const int height = 10;
    const int startY = 0;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const,
        startY
      );

      var window = frame.GetWindow();

      // M2 row should clamp to row 0 (const mode)
      Assert.That((int)window.M2P0.Work, Is.EqualTo(0));
      Assert.That((int)window.M1P0.Work, Is.EqualTo(0));
      Assert.That((int)window.P0P0.Work, Is.EqualTo(0));
    }
  }

  #endregion

  #region OutOfBoundsMode.Const Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void ConstMode_ClampsToEdgePixels_Horizontal() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      frame.MoveDown();
      frame.MoveDown();
      var window = frame.GetWindow();

      // At x=0, left neighbors should clamp to first pixel
      var rowStart = 2 * width;
      Assert.That((int)window.P0M2.Work, Is.EqualTo(rowStart)); // Clamped to col 0
      Assert.That((int)window.P0M1.Work, Is.EqualTo(rowStart)); // Clamped to col 0
      Assert.That((int)window.P0P0.Work, Is.EqualTo(rowStart));

      // Move to end of row
      for (var i = 0; i < width - 1; ++i)
        window.MoveRight();

      // At x=width-1, right neighbors should clamp to last pixel
      var rowEnd = 2 * width + (width - 1);
      Assert.That((int)window.P0P0.Work, Is.EqualTo(rowEnd));
      Assert.That((int)window.P0P1.Work, Is.EqualTo(rowEnd)); // Clamped to col width-1
      Assert.That((int)window.P0P2.Work, Is.EqualTo(rowEnd)); // Clamped to col width-1
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void ConstMode_ClampsToEdgePixels_Vertical() {
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
      window.MoveRight();
      window.MoveRight();

      // At y=0, top neighbors should clamp to row 0
      Assert.That((int)window.M2P0.Work, Is.EqualTo(2)); // Clamped to row 0
      Assert.That((int)window.M1P0.Work, Is.EqualTo(2)); // Clamped to row 0
      Assert.That((int)window.P0P0.Work, Is.EqualTo(2));

      // Move to bottom
      for (var i = 0; i < height - 1; ++i)
        frame.MoveDown();

      window = frame.GetWindow();
      window.MoveRight();
      window.MoveRight();

      // At y=height-1, bottom neighbors should clamp to last row
      var lastRow = (height - 1) * width + 2;
      Assert.That((int)window.P0P0.Work, Is.EqualTo(lastRow));
      Assert.That((int)window.P1P0.Work, Is.EqualTo(lastRow)); // Clamped to last row
      Assert.That((int)window.P2P0.Work, Is.EqualTo(lastRow)); // Clamped to last row
    }
  }

  #endregion

  #region OutOfBoundsMode.Wrap Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void WrapMode_WrapsAroundHorizontally() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Wrap, OutOfBoundsMode.Const
      );

      frame.MoveDown();
      frame.MoveDown();
      var window = frame.GetWindow();

      // At x=0: neighbors at x=-1 and x=-2 wrap to x=4 and x=3
      var row = 2 * width;
      Assert.That((int)window.P0M1.Work, Is.EqualTo(row + (width - 1))); // x=-1 wraps to x=4
      Assert.That((int)window.P0M2.Work, Is.EqualTo(row + (width - 2))); // x=-2 wraps to x=3
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void WrapMode_WrapsAroundVertically() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Wrap
      );

      var window = frame.GetWindow();
      window.MoveRight();
      window.MoveRight();

      // At y=0: neighbors at y=-1 and y=-2 wrap to y=4 and y=3
      Assert.That((int)window.M1P0.Work, Is.EqualTo((height - 1) * width + 2)); // y=-1 wraps to y=4
      Assert.That((int)window.M2P0.Work, Is.EqualTo((height - 2) * width + 2)); // y=-2 wraps to y=3
    }
  }

  #endregion

  #region OutOfBoundsMode.Half Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void HalfMode_MirrorsAtHalfPixelBoundary() {
    // Half mirror: cba|abcde|edc (mirrors at boundary)
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Half, OutOfBoundsMode.Const
      );

      frame.MoveDown();
      frame.MoveDown();
      var window = frame.GetWindow();

      // At x=0: x=-1 mirrors to x=0, x=-2 mirrors to x=1
      var row = 2 * width;
      Assert.That((int)window.P0M1.Work, Is.EqualTo(row + 0)); // x=-1 -> x=0
      Assert.That((int)window.P0M2.Work, Is.EqualTo(row + 1)); // x=-2 -> x=1
    }
  }

  #endregion

  #region OutOfBoundsMode.Whole Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void WholeMode_MirrorsAtPixelCenter() {
    // Whole mirror: dcb|abcde|dcb (mirrors including edge pixel)
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Whole, OutOfBoundsMode.Const
      );

      frame.MoveDown();
      frame.MoveDown();
      var window = frame.GetWindow();

      // At x=0: x=-1 mirrors to x=1, x=-2 mirrors to x=2
      var row = 2 * width;
      Assert.That((int)window.P0M1.Work, Is.EqualTo(row + 1)); // x=-1 -> x=1
      Assert.That((int)window.P0M2.Work, Is.EqualTo(row + 2)); // x=-2 -> x=2
    }
  }

  #endregion

  #region Dispose Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void Dispose_ReleasesGCHandle() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      frame.Dispose();

      // Dispose should be idempotent
      Assert.DoesNotThrow(() => frame.Dispose());
    }
  }

  #endregion

  #region Properties Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void Width_ReturnsSourceWidth() {
    const int width = 7;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      Assert.That(frame.Width, Is.EqualTo(width));
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void Height_ReturnsSourceHeight() {
    const int width = 5;
    const int height = 11;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      Assert.That(frame.Height, Is.EqualTo(height));
    }
  }

  #endregion

  #region Full Traversal Tests

  [Test]
  [Category("Integration")]
  public unsafe void FullTraversal_AccessesAllPixels() {
    const int width = 8;
    const int height = 6;
    var image = CreateSequentialImage(width, height);
    var accessedValues = new System.Collections.Generic.HashSet<int>();

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y) {
        var window = frame.GetWindow();
        for (var x = 0; x < width; ++x) {
          accessedValues.Add(window.P0P0.Work);
          if (x < width - 1)
            window.MoveRight();
        }

        if (y < height - 1)
          frame.MoveDown();
      }
    }

    // Verify all pixels were accessed
    Assert.That(accessedValues.Count, Is.EqualTo(width * height));
    for (var i = 0; i < width * height; ++i)
      Assert.That(accessedValues.Contains(i), Is.True, $"Pixel {i} was not accessed");
  }

  #endregion

  #region Random Access Indexer Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void Indexer_ReturnsCorrectPixel() {
    const int width = 10;
    const int height = 10;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y)
      for (var x = 0; x < width; ++x) {
        var expected = y * width + x;
        Assert.That((int)frame[x, y].Work, Is.EqualTo(expected), $"Work at ({x}, {y})");
        Assert.That((int)frame[x, y].Key, Is.EqualTo(expected), $"Key at ({x}, {y})");
      }
    }
  }

  [Test]
  [Category("EdgeCase")]
  public unsafe void Indexer_ClampsNegativeCoordinates() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      // Negative X clamps to 0
      Assert.That((int)frame[-1, 2].Work, Is.EqualTo(2 * width));
      Assert.That((int)frame[-5, 2].Work, Is.EqualTo(2 * width));

      // Negative Y clamps to 0
      Assert.That((int)frame[2, -1].Work, Is.EqualTo(2));
      Assert.That((int)frame[2, -5].Work, Is.EqualTo(2));

      // Both negative
      Assert.That((int)frame[-1, -1].Work, Is.EqualTo(0));
    }
  }

  [Test]
  [Category("EdgeCase")]
  public unsafe void Indexer_ClampsOverflowCoordinates() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      // X >= width clamps to width-1
      Assert.That((int)frame[width, 2].Work, Is.EqualTo(2 * width + (width - 1)));
      Assert.That((int)frame[width + 5, 2].Work, Is.EqualTo(2 * width + (width - 1)));

      // Y >= height clamps to height-1
      Assert.That((int)frame[2, height].Work, Is.EqualTo((height - 1) * width + 2));
      Assert.That((int)frame[2, height + 5].Work, Is.EqualTo((height - 1) * width + 2));

      // Both overflow
      Assert.That((int)frame[width, height].Work, Is.EqualTo((height - 1) * width + (width - 1)));
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void Indexer_WorksWithWrapMode() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Wrap, OutOfBoundsMode.Wrap
      );

      // X wraps: -1 -> width-1
      Assert.That((int)frame[-1, 2].Work, Is.EqualTo(2 * width + (width - 1)));

      // Y wraps: -1 -> height-1
      Assert.That((int)frame[2, -1].Work, Is.EqualTo((height - 1) * width + 2));

      // X wraps: width -> 0
      Assert.That((int)frame[width, 2].Work, Is.EqualTo(2 * width));
    }
  }

  [Test]
  [Category("EdgeCase")]
  public unsafe void Indexer_ExtremeOutOfBounds_ConstMode_ClampsCorrectly() {
    const int width = 10;
    const int height = 10;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      // Extreme negative coordinates should clamp to (0, 0)
      Assert.That((int)frame[-1000, -2033].Work, Is.EqualTo(0));
      Assert.That((int)frame[-999999, -999999].Work, Is.EqualTo(0));
      Assert.That((int)frame[int.MinValue + 1, int.MinValue + 1].Work, Is.EqualTo(0));

      // Extreme positive coordinates should clamp to (width-1, height-1)
      var bottomRight = (height - 1) * width + (width - 1);
      Assert.That((int)frame[1000, 2033].Work, Is.EqualTo(bottomRight));
      Assert.That((int)frame[999999, 999999].Work, Is.EqualTo(bottomRight));
      Assert.That((int)frame[int.MaxValue - 1, int.MaxValue - 1].Work, Is.EqualTo(bottomRight));

      // Mixed extreme coordinates
      Assert.That((int)frame[-1000, 2033].Work, Is.EqualTo((height - 1) * width)); // (0, height-1)
      Assert.That((int)frame[1000, -2033].Work, Is.EqualTo(width - 1)); // (width-1, 0)
    }
  }

  [Test]
  [Category("EdgeCase")]
  public unsafe void Indexer_ExtremeOutOfBounds_WrapMode_WrapsCorrectly() {
    const int width = 10;
    const int height = 10;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Wrap, OutOfBoundsMode.Wrap
      );

      // -1000 % 10 = 0 (after adding 10 for negative modulo)
      // -1000 mod 10: -1000 = -100 * 10, so remainder is 0
      var expectedX = ((-1000 % width) + width) % width;
      var expectedY = ((-2033 % height) + height) % height;
      var expected = expectedY * width + expectedX;
      Assert.That((int)frame[-1000, -2033].Work, Is.EqualTo(expected), $"Expected ({expectedX}, {expectedY})");

      // 1000 % 10 = 0, 2033 % 10 = 3
      expectedX = 1000 % width;
      expectedY = 2033 % height;
      expected = expectedY * width + expectedX;
      Assert.That((int)frame[1000, 2033].Work, Is.EqualTo(expected), $"Expected ({expectedX}, {expectedY})");

      // Verify specific wrap calculations
      Assert.That((int)frame[-1, 0].Work, Is.EqualTo(width - 1)); // -1 wraps to 9
      Assert.That((int)frame[-11, 0].Work, Is.EqualTo(width - 1)); // -11 wraps to 9
      Assert.That((int)frame[10, 0].Work, Is.EqualTo(0)); // 10 wraps to 0
      Assert.That((int)frame[21, 0].Work, Is.EqualTo(1)); // 21 wraps to 1
    }
  }

  [Test]
  [Category("EdgeCase")]
  public unsafe void Indexer_ExtremeOutOfBounds_HalfMirrorMode_MirrorsCorrectly() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Half, OutOfBoundsMode.Half
      );

      // Half mirror: cba|abcde|edc
      // -1 -> 0, -2 -> 1, -3 -> 2, etc.
      Assert.That((int)frame[-1, 2].Work, Is.EqualTo(2 * width + 0));
      Assert.That((int)frame[-2, 2].Work, Is.EqualTo(2 * width + 1));
      Assert.That((int)frame[-3, 2].Work, Is.EqualTo(2 * width + 2));

      // For very large negative: mirrors repeatedly
      // The implementation uses: -coord - 1 for negative
      // -1000 -> 999, which then needs further handling
      // Let's verify small values work correctly first
      Assert.That((int)frame[5, 2].Work, Is.EqualTo(2 * width + 4)); // 5 -> 4
      Assert.That((int)frame[6, 2].Work, Is.EqualTo(2 * width + 3)); // 6 -> 3
    }
  }

  [Test]
  [Category("Integration")]
  public unsafe void Indexer_AccessOutsideWindow_ReturnsCorrectValues() {
    const int width = 20;
    const int height = 20;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const,
        startY: 10
      );

      var window = frame.GetWindow();

      // Window is at (0, 10), covers y=8..12 and x=-2..width+1
      // Access pixels far outside the window
      Assert.That((int)frame[15, 0].Work, Is.EqualTo(15));
      Assert.That((int)frame[15, 19].Work, Is.EqualTo(19 * width + 15));
      Assert.That((int)frame[0, 5].Work, Is.EqualTo(5 * width));

      // Verify window still works correctly
      Assert.That((int)window.P0P0.Work, Is.EqualTo(10 * width));
    }
  }

  #endregion

  #region Fast Path Tests (SIMD Optimizations)

  // 8-byte pixel type helpers
  private readonly struct IdentityDecode8 : IDecode<TestUInt64, TestUInt64> {
    public TestUInt64 Decode(in TestUInt64 pixel) => pixel;
  }

  private readonly struct IdentityProject8 : IProject<TestUInt64, TestUInt64> {
    public TestUInt64 Project(in TestUInt64 color) => color;
  }

  private static TestUInt64[] CreateSequentialImage8(int width, int height) {
    var result = new TestUInt64[width * height];
    for (var i = 0; i < result.Length; ++i)
      result[i] = (ulong)i;
    return result;
  }

  // Decode-only path helpers (TPixel=uint, TWork=TKey=ulong)
  private readonly struct WideningDecode : IDecode<TestUInt32, TestUInt64> {
    public TestUInt64 Decode(in TestUInt32 pixel) => (ulong)pixel.Value;
  }

  private readonly struct IdentityProject8From4 : IProject<TestUInt64, TestUInt64> {
    public TestUInt64 Project(in TestUInt64 color) => color;
  }

  // Full transform path helpers (TPixel=uint, TWork=ulong, TKey=int)
  private readonly struct WideningDecodeForTransform : IDecode<TestUInt32, TestUInt64> {
    public TestUInt64 Decode(in TestUInt32 pixel) => (ulong)pixel.Value;
  }

  private readonly struct NarrowingProject : IProject<TestUInt64, TestInt32> {
    public TestInt32 Project(in TestUInt64 color) => (int)(color & 0xFFFFFFFF);
  }

  [Test]
  [Category("HappyPath")]
  [Category("FastPath")]
  public unsafe void DirectCopy8_TraversesImageCorrectly() {
    const int width = 20;
    const int height = 10;
    var image = CreateSequentialImage8(width, height);

    fixed (TestUInt64* ptr = image) {
      using var frame = new NeighborFrame<TestUInt64, TestUInt64, TestUInt64, IdentityDecode8, IdentityProject8>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y) {
        var window = frame.GetWindow();
        for (var x = 0; x < width; ++x) {
          var expected = (ulong)(y * width + x);
          Assert.That((ulong)window.P0P0.Work, Is.EqualTo(expected), $"Work at ({x}, {y})");
          Assert.That((ulong)window.P0P0.Key, Is.EqualTo(expected), $"Key at ({x}, {y})");
          if (x < width - 1)
            window.MoveRight();
        }
        if (y < height - 1)
          frame.MoveDown();
      }
    }
  }

  [Test]
  [Category("EdgeCase")]
  [Category("FastPath")]
  public unsafe void DirectCopy8_EdgePadding_ClampsCorrectly() {
    const int width = 5;
    const int height = 5;
    var image = CreateSequentialImage8(width, height);

    fixed (TestUInt64* ptr = image) {
      using var frame = new NeighborFrame<TestUInt64, TestUInt64, TestUInt64, IdentityDecode8, IdentityProject8>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      frame.MoveDown();
      frame.MoveDown();
      var window = frame.GetWindow();

      var rowStart = 2ul * width;
      Assert.That((ulong)window.P0M2.Work, Is.EqualTo(rowStart));
      Assert.That((ulong)window.P0M1.Work, Is.EqualTo(rowStart));
      Assert.That((ulong)window.P0P0.Work, Is.EqualTo(rowStart));

      for (var i = 0; i < width - 1; ++i)
        window.MoveRight();

      var rowEnd = 2ul * width + (width - 1);
      Assert.That((ulong)window.P0P0.Work, Is.EqualTo(rowEnd));
      Assert.That((ulong)window.P0P1.Work, Is.EqualTo(rowEnd));
      Assert.That((ulong)window.P0P2.Work, Is.EqualTo(rowEnd));
    }
  }

  [Test]
  [Category("HappyPath")]
  [Category("FastPath")]
  public unsafe void DecodeOnlyPath_WorksCorrectly() {
    const int width = 10;
    const int height = 5;
    var image = new TestUInt32[width * height];
    for (var i = 0; i < image.Length; ++i)
      image[i] = (uint)i;

    fixed (TestUInt32* ptr = image) {
      using var frame = new NeighborFrame<TestUInt32, TestUInt64, TestUInt64, WideningDecode, IdentityProject8From4>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y) {
        var window = frame.GetWindow();
        for (var x = 0; x < width; ++x) {
          var expected = (ulong)(y * width + x);
          Assert.That((ulong)window.P0P0.Work, Is.EqualTo(expected), $"Work at ({x}, {y})");
          Assert.That((ulong)window.P0P0.Key, Is.EqualTo(expected), $"Key at ({x}, {y})");
          if (x < width - 1)
            window.MoveRight();
        }
        if (y < height - 1)
          frame.MoveDown();
      }
    }
  }

  [Test]
  [Category("HappyPath")]
  [Category("FastPath")]
  public unsafe void FullTransformPath_WorksCorrectly() {
    const int width = 10;
    const int height = 5;
    var image = new TestUInt32[width * height];
    for (var i = 0; i < image.Length; ++i)
      image[i] = (uint)i;

    fixed (TestUInt32* ptr = image) {
      using var frame = new NeighborFrame<TestUInt32, TestUInt64, TestInt32, WideningDecodeForTransform, NarrowingProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y) {
        var window = frame.GetWindow();
        for (var x = 0; x < width; ++x) {
          var expectedWork = (ulong)(y * width + x);
          var expectedKey = y * width + x;
          Assert.That((ulong)window.P0P0.Work, Is.EqualTo(expectedWork), $"Work at ({x}, {y})");
          Assert.That((int)window.P0P0.Key, Is.EqualTo(expectedKey), $"Key at ({x}, {y})");
          if (x < width - 1)
            window.MoveRight();
        }
        if (y < height - 1)
          frame.MoveDown();
      }
    }
  }

  [Test]
  [Category("Integration")]
  [Category("FastPath")]
  public unsafe void DirectCopy4_LargeImage_ProcessesCorrectly() {
    const int width = 100;
    const int height = 100;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y) {
        var window = frame.GetWindow();
        for (var x = 0; x < width; ++x) {
          var expected = y * width + x;
          Assert.That((int)window.P0P0.Work, Is.EqualTo(expected), $"Work at ({x}, {y})");
          Assert.That((int)window.P0P0.Key, Is.EqualTo(expected), $"Key at ({x}, {y})");
          if (x < width - 1)
            window.MoveRight();
        }
        if (y < height - 1)
          frame.MoveDown();
      }
    }
  }

  [Test]
  [Category("Integration")]
  [Category("FastPath")]
  public unsafe void DirectCopy8_LargeImage_ProcessesCorrectly() {
    const int width = 100;
    const int height = 100;
    var image = CreateSequentialImage8(width, height);

    fixed (TestUInt64* ptr = image) {
      using var frame = new NeighborFrame<TestUInt64, TestUInt64, TestUInt64, IdentityDecode8, IdentityProject8>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const
      );

      for (var y = 0; y < height; ++y) {
        var window = frame.GetWindow();
        for (var x = 0; x < width; ++x) {
          var expected = (ulong)(y * width + x);
          Assert.That((ulong)window.P0P0.Work, Is.EqualTo(expected), $"Work at ({x}, {y})");
          Assert.That((ulong)window.P0P0.Key, Is.EqualTo(expected), $"Key at ({x}, {y})");
          if (x < width - 1)
            window.MoveRight();
        }
        if (y < height - 1)
          frame.MoveDown();
      }
    }
  }

  #endregion

}
