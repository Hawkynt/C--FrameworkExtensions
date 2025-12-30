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

using System.Collections.Concurrent;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Threading.Tasks;
using Hawkynt.ColorProcessing.Codecs;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Integration")]
[Category("ColorProcessing")]
[Category("ParallelProcessing")]
public class ParallelProcessingTests {

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

  #region Independent Partition Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void MultiplePartitions_IndependentBuffers_NoDataCorruption() {
    const int width = 100;
    const int height = 100;
    const int numPartitions = 4;
    var image = CreateSequentialImage(width, height);
    var partitionSize = height / numPartitions;

    var results = new ConcurrentDictionary<int, int[]>();

    fixed (TestInt32* ptr = image) {
      var ptrCopy = ptr; // Capture for closure

      Parallel.For(0, numPartitions, partitionId => {
        var startY = partitionId * partitionSize;
        var endY = partitionId == numPartitions - 1 ? height : (partitionId + 1) * partitionSize;
        var partitionResults = new int[(endY - startY) * width];

        using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
          ptrCopy, width, height, width,
          default, default,
          OutOfBoundsMode.Const, OutOfBoundsMode.Const,
          startY
        );

        for (var y = startY; y < endY; ++y) {
          var window = frame.GetWindow();
          for (var x = 0; x < width; ++x) {
            partitionResults[(y - startY) * width + x] = window.P0P0.Work;
            if (x < width - 1)
              window.MoveRight();
          }

          if (y < endY - 1)
            frame.MoveDown();
        }

        results[partitionId] = partitionResults;
      });
    }

    // Verify all partitions processed correctly
    for (var partitionId = 0; partitionId < numPartitions; ++partitionId) {
      Assert.That(results.ContainsKey(partitionId), Is.True, $"Partition {partitionId} missing");

      var startY = partitionId * partitionSize;
      var endY = partitionId == numPartitions - 1 ? height : (partitionId + 1) * partitionSize;
      var partitionResults = results[partitionId];

      for (var y = startY; y < endY; ++y)
      for (var x = 0; x < width; ++x) {
        var expected = y * width + x;
        var actual = partitionResults[(y - startY) * width + x];
        Assert.That(actual, Is.EqualTo(expected), $"Mismatch at ({x}, {y}) in partition {partitionId}");
      }
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void PartitionsWithDifferentStartY_ProduceCorrectResults() {
    const int width = 20;
    const int height = 30;
    var image = CreateSequentialImage(width, height);

    // Test specific start rows
    var startRows = new[] { 0, 5, 15, 25 };
    var results = new ConcurrentDictionary<int, int>();

    fixed (TestInt32* ptr = image) {
      var ptrCopy = ptr;

      Parallel.ForEach(startRows, startY => {
        using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
          ptrCopy, width, height, width,
          default, default,
          OutOfBoundsMode.Const, OutOfBoundsMode.Const,
          startY
        );

        var window = frame.GetWindow();
        results[startY] = window.P0P0.Work;
      });
    }

    foreach (var startY in startRows)
      Assert.That(results[startY], Is.EqualTo(startY * width), $"Start row {startY}");
  }

  #endregion

  #region Thread Safety Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void ConcurrentAccess_NoRaceConditions() {
    const int width = 50;
    const int height = 200;
    const int iterations = 10;
    var image = CreateSequentialImage(width, height);

    for (var iter = 0; iter < iterations; ++iter) {
      var errors = new ConcurrentBag<string>();

      fixed (TestInt32* ptr = image) {
        var ptrCopy = ptr;

        Parallel.For(0, height, y => {
          using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
            ptrCopy, width, height, width,
            default, default,
            OutOfBoundsMode.Const, OutOfBoundsMode.Const,
            y
          );

          var window = frame.GetWindow();
          for (var x = 0; x < width; ++x) {
            var expected = y * width + x;
            var actual = (int)window.P0P0.Work;
            if (actual != expected)
              errors.Add($"Iteration {iter}: ({x}, {y}) expected {expected} got {actual}");

            if (x < width - 1)
              window.MoveRight();
          }
        });
      }

      Assert.That(errors.ToArray().Length, Is.EqualTo(0), string.Join("\n", errors));
    }
  }

  #endregion

  #region Edge Case Partition Tests

  [Test]
  [Category("EdgeCase")]
  public unsafe void SingleRowPartitions_WorkCorrectly() {
    const int width = 20;
    const int height = 10;
    var image = CreateSequentialImage(width, height);
    var results = new int[height * width];

    fixed (TestInt32* ptr = image) {
      var ptrCopy = ptr;

      Parallel.For(0, height, y => {
        using var frame = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
          ptrCopy, width, height, width,
          default, default,
          OutOfBoundsMode.Const, OutOfBoundsMode.Const,
          y
        );

        var window = frame.GetWindow();
        for (var x = 0; x < width; ++x) {
          results[y * width + x] = window.P0P0.Work;
          if (x < width - 1)
            window.MoveRight();
        }
      });
    }

    for (var i = 0; i < image.Length; ++i)
      Assert.That(results[i], Is.EqualTo((int)image[i]), $"Index {i}");
  }

  [Test]
  [Category("EdgeCase")]
  public unsafe void OverlappingNeighborhood_AcrossPartitions_ReadsCorrectly() {
    // Test that partition boundaries don't affect neighbor access
    const int width = 10;
    const int height = 20;
    var image = CreateSequentialImage(width, height);

    // Two partitions: 0-9 and 10-19
    // Row 9 should correctly see rows 7,8,9,10,11
    // Row 10 should correctly see rows 8,9,10,11,12

    fixed (TestInt32* ptr = image) {
      var ptrCopy = ptr;

      var partition1Neighbors = new int[5]; // M2, M1, P0, P1, P2 at boundary row
      var partition2Neighbors = new int[5];

      // First partition ends at row 9
      using (var frame1 = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
               ptrCopy, width, height, width,
               default, default,
               OutOfBoundsMode.Const, OutOfBoundsMode.Const,
               0)) {
        for (var y = 0; y < 9; ++y)
          frame1.MoveDown();

        var window = frame1.GetWindow();
        partition1Neighbors[0] = window.M2P0.Work;
        partition1Neighbors[1] = window.M1P0.Work;
        partition1Neighbors[2] = window.P0P0.Work;
        partition1Neighbors[3] = window.P1P0.Work;
        partition1Neighbors[4] = window.P2P0.Work;
      }

      // Second partition starts at row 10
      using (var frame2 = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
               ptrCopy, width, height, width,
               default, default,
               OutOfBoundsMode.Const, OutOfBoundsMode.Const,
               10)) {
        var window = frame2.GetWindow();
        partition2Neighbors[0] = window.M2P0.Work;
        partition2Neighbors[1] = window.M1P0.Work;
        partition2Neighbors[2] = window.P0P0.Work;
        partition2Neighbors[3] = window.P1P0.Work;
        partition2Neighbors[4] = window.P2P0.Work;
      }

      // Verify partition 1 (row 9) neighbors
      Assert.That(partition1Neighbors[0], Is.EqualTo(7 * width)); // M2: row 7
      Assert.That(partition1Neighbors[1], Is.EqualTo(8 * width)); // M1: row 8
      Assert.That(partition1Neighbors[2], Is.EqualTo(9 * width)); // P0: row 9
      Assert.That(partition1Neighbors[3], Is.EqualTo(10 * width)); // P1: row 10
      Assert.That(partition1Neighbors[4], Is.EqualTo(11 * width)); // P2: row 11

      // Verify partition 2 (row 10) neighbors
      Assert.That(partition2Neighbors[0], Is.EqualTo(8 * width)); // M2: row 8
      Assert.That(partition2Neighbors[1], Is.EqualTo(9 * width)); // M1: row 9
      Assert.That(partition2Neighbors[2], Is.EqualTo(10 * width)); // P0: row 10
      Assert.That(partition2Neighbors[3], Is.EqualTo(11 * width)); // P1: row 11
      Assert.That(partition2Neighbors[4], Is.EqualTo(12 * width)); // P2: row 12
    }
  }

  #endregion

  #region Memory Independence Tests

  [Test]
  [Category("HappyPath")]
  public unsafe void EachPartition_HasIndependentBuffer() {
    const int width = 10;
    const int height = 20;
    var image = CreateSequentialImage(width, height);

    fixed (TestInt32* ptr = image) {
      using var frame1 = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const,
        0
      );

      using var frame2 = new NeighborFrame<TestInt32, TestInt32, TestInt32, IdentityDecode, IdentityProject>(
        ptr, width, height, width,
        default, default,
        OutOfBoundsMode.Const, OutOfBoundsMode.Const,
        10
      );

      var window1 = frame1.GetWindow();
      var window2 = frame2.GetWindow();

      // Both frames should return their respective positions
      Assert.That((int)window1.P0P0.Work, Is.EqualTo(0 * width));
      Assert.That((int)window2.P0P0.Work, Is.EqualTo(10 * width));

      // Moving one frame shouldn't affect the other
      frame1.MoveDown();
      window1 = frame1.GetWindow();

      Assert.That((int)window1.P0P0.Work, Is.EqualTo(1 * width));
      Assert.That((int)window2.P0P0.Work, Is.EqualTo(10 * width)); // Unchanged
    }
  }

  #endregion

}
