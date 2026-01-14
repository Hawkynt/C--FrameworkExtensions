using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace System.IO;

[TestFixture]
internal class PathTest {
  [Test]
  public static void GetUsableSystemTempDirectoryName_Should_Exists() {
    var arrange = PathExtensions.GetUsableSystemTempDirectoryName();
    Assert.That(Directory.Exists(arrange), Is.EqualTo(true));
  }

  #region Regression tests for atomic temp directory creation

  [Test]
  [Category("Regression")]
  [Category("Integration")]
  public void GetTempDirectoryName_ConcurrentCreation_NoDuplicates() {
    const int THREAD_COUNT = 8;
    const int DIRECTORIES_PER_THREAD = 50;

    var createdDirectories = new ConcurrentBag<string>();
    var errors = new ConcurrentBag<Exception>();
    var baseDir = PathExtensions.GetUsableSystemTempDirectoryName();

    // Create a unique subdirectory for this test run
    var testBaseDir = Path.Combine(baseDir, $"concurrent_test_{Guid.NewGuid():N}");
    Directory.CreateDirectory(testBaseDir);

    try {
      var threads = new Thread[THREAD_COUNT];

      for (var i = 0; i < THREAD_COUNT; ++i)
        threads[i] = new Thread(() => {
          for (var j = 0; j < DIRECTORIES_PER_THREAD; ++j)
            try {
              var dir = PathExtensions.GetTempDirectoryName(baseDirectory: testBaseDir);
              createdDirectories.Add(dir);
            } catch (Exception ex) {
              errors.Add(ex);
            }
        }) { IsBackground = true };

      // Start all threads simultaneously
      foreach (var thread in threads)
        thread.Start();

      // Wait for all threads to complete
      foreach (var thread in threads)
        thread.Join();

      // Verify results
      var dirArray = createdDirectories.ToArray();
      var dirList = new List<string>(dirArray);
      var uniqueDirs = new HashSet<string>(dirArray);
      var errorArray = errors.ToArray();
      var errorList = new List<Exception>(errorArray);

      Assert.That(errorList, Is.Empty, $"Errors occurred during creation: {string.Join(", ", errorList.Select(e => e.Message))}");
      Assert.That(dirList.Count, Is.EqualTo(THREAD_COUNT * DIRECTORIES_PER_THREAD), "Not all directories were created");
      Assert.That(uniqueDirs.Count, Is.EqualTo(dirList.Count), "Duplicate directories were created - atomicity violated!");

      // Verify all directories actually exist
      foreach (var dir in dirList)
        Assert.That(Directory.Exists(dir), Is.True, $"Directory {dir} does not exist");
    } finally {
      // Cleanup
      try {
        if (Directory.Exists(testBaseDir))
          Directory.Delete(testBaseDir, true);
      } catch {
        // Ignore cleanup errors
      }
    }
  }

  [Test]
  [Category("Regression")]
  [Category("Integration")]
  public void TryCreateDirectory_ConcurrentSameName_OnlyOneSucceeds() {
    const int THREAD_COUNT = 16;
    const int ATTEMPTS_PER_THREAD = 10;

    var baseDir = PathExtensions.GetUsableSystemTempDirectoryName();
    var testBaseDir = Path.Combine(baseDir, $"same_name_test_{Guid.NewGuid():N}");
    Directory.CreateDirectory(testBaseDir);

    try {
      for (var attempt = 0; attempt < ATTEMPTS_PER_THREAD; ++attempt) {
        var targetDir = Path.Combine(testBaseDir, $"contested_dir_{attempt}");
        var successCount = 0;
        var threads = new Thread[THREAD_COUNT];

        for (var i = 0; i < THREAD_COUNT; ++i)
          threads[i] = new Thread(() => {
            if (PathExtensions.TryCreateDirectory(targetDir))
              Interlocked.Increment(ref successCount);
          }) { IsBackground = true };

        // Start all threads simultaneously
        foreach (var thread in threads)
          thread.Start();

        // Wait for all threads to complete
        foreach (var thread in threads)
          thread.Join();

        Assert.That(successCount, Is.EqualTo(1), $"Expected exactly 1 thread to succeed creating {targetDir}, but {successCount} succeeded");
        Assert.That(Directory.Exists(targetDir), Is.True, $"Directory {targetDir} should exist");
      }
    } finally {
      // Cleanup
      try {
        if (Directory.Exists(testBaseDir))
          Directory.Delete(testBaseDir, true);
      } catch {
        // Ignore cleanup errors
      }
    }
  }

  [Test]
  [Category("Regression")]
  public void GetTempDirectoryName_GeneratesUniqueNames() {
    const int COUNT = 100;
    var baseDir = PathExtensions.GetUsableSystemTempDirectoryName();
    var testBaseDir = Path.Combine(baseDir, $"unique_test_{Guid.NewGuid():N}");
    Directory.CreateDirectory(testBaseDir);

    try {
      var directories = new string[COUNT];

      for (var i = 0; i < COUNT; ++i)
        directories[i] = PathExtensions.GetTempDirectoryName(baseDirectory: testBaseDir);

      var uniqueCount = directories.Distinct().Count();
      Assert.That(uniqueCount, Is.EqualTo(COUNT), "Generated directory names are not unique");

      // Verify names have expected format (tmp + 12 chars + .tmp)
      foreach (var dir in directories) {
        var name = Path.GetFileName(dir);
        Assert.That(name, Does.StartWith("tmp"), $"Directory name should start with 'tmp': {name}");
        Assert.That(name, Does.EndWith(".tmp"), $"Directory name should end with '.tmp': {name}");
        Assert.That(name.Length, Is.EqualTo(3 + 12 + 4), $"Directory name should be 19 chars (tmp + 12 + .tmp): {name}");
      }
    } finally {
      // Cleanup
      try {
        if (Directory.Exists(testBaseDir))
          Directory.Delete(testBaseDir, true);
      } catch {
        // Ignore cleanup errors
      }
    }
  }

  #endregion
}
