using System;
using System.IO;
using System.Linq;
using Corlib.Tests.NUnit;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;

using global::System.Collections.Generic;
using static TestUtilities;

[TestFixture]
internal class DirectoryInfoTests {

  private static string Combine(string arg, string[] args) => Combine(new []{arg}.Concat(args).ToArray());

  private static string Combine(string[] args) => string.Join(Path.DirectorySeparatorChar.ToString(), args.Select(i => i.TrimEnd(Path.DirectorySeparatorChar)).ToArray());

  // Directory(string subdirectory) Tests
  [Test]
  public void Directory_NullDirectory_ThrowsNullReferenceException() {
    DirectoryInfo? directoryInfo = null;
    Assert.That(() => directoryInfo.Directory("subdir"), Throws.TypeOf<NullReferenceException>());
  }

  [Test]
  public void Directory_NullSubdirectory_ThrowsArgumentNullException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    string? subdirectory = null;
    Assert.That(() => directoryInfo.Directory(subdirectory), Throws.ArgumentNullException);
  }

  [Test]
  [TestCase("subdir")]
  [TestCase("anotherSubdir")]
  public void Directory_ValidSubdirectory_ReturnsCorrectDirectoryInfo(string subdirectory) {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    var result = directoryInfo.Directory(subdirectory);

    Assert.That(result.FullName, Is.EqualTo(Path.Combine(directoryInfo.FullName, subdirectory)));
  }

  // Directory(params string[] subdirectories) Tests
  [Test]
  public void Directory_WithSubdirectoriesArray_NullDirectory_ThrowsNullReferenceException() {
    DirectoryInfo? directoryInfo = null;
    Assert.That(() => directoryInfo.Directory("subdir1", "subdir2"), Throws.TypeOf<NullReferenceException>());
  }

  [Test]
  public void Directory_WithSubdirectoriesArray_NullSubdirectories_ThrowsArgumentNullException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    string[]? subdirectories = null;
    Assert.That(() => directoryInfo.Directory(subdirectories), Throws.ArgumentNullException);
  }

  [Test]
  public void Directory_WithSubdirectoriesArray_EmptySubdirectories_ThrowsArgumentException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    Assert.That(() => directoryInfo.Directory(), Throws.ArgumentException);
  }

  [Test]
  [TestCase(new [] { "subdir1", "subdir2" }, 0)]
  [TestCase(new [] { "dir1", "dir2", "dir3" }, 0)]
  public void Directory_WithSubdirectoriesArray_ValidSubdirectories_ReturnsCorrectDirectoryInfo(string[] subdirectories, int _) {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    var result = directoryInfo.Directory(subdirectories);

    var expectedPath = Combine(directoryInfo.FullName, subdirectories);
    Assert.That(result.FullName, Is.EqualTo(expectedPath));
  }

  // Directory(bool ignoreCase, params string[] subdirectories) Tests
  [Test]
  public void Directory_WithExactCase_NullDirectory_ThrowsNullReferenceException() {
    DirectoryInfo? directoryInfo = null;
    Assert.That(() => directoryInfo.Directory(false, "subdir1", "subdir2"), Throws.TypeOf<NullReferenceException>());
  }

  [Test]
  public void Directory_WithExactCase_NullSubdirectories_ThrowsArgumentNullException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    string[]? subdirectories = null;
    Assert.That(() => directoryInfo.Directory(false, subdirectories), Throws.ArgumentNullException);
  }

  [Test]
  public void Directory_WithExactCase_EmptySubdirectories_ThrowsArgumentException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    Assert.That(() => directoryInfo.Directory(false), Throws.ArgumentException);
  }

  [Test]
  [TestCase(new [] { "subdir1", "subdir2" }, false)]
  [TestCase(new [] { "dir1", "dir2", "dir3" }, true)]
  public void Directory_WithExactCase_ValidSubdirectories_ReturnsCorrectDirectoryInfo(string[] subdirectories, bool ignoreCase) {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    var result = directoryInfo.Directory(ignoreCase, subdirectories);

    var expectedPath = Combine(directoryInfo.FullName, subdirectories);
    Assert.That(result.FullName, Is.EqualTo(expectedPath));
  }

  // File(string filePath) Tests
  [Test]
  public void File_NullDirectory_ThrowsNullReferenceException() {
    DirectoryInfo? directoryInfo = null;
    Assert.That(() => directoryInfo.File("file.txt"), Throws.TypeOf<NullReferenceException>());
  }

  [Test]
  public void File_NullFilePath_ThrowsArgumentNullException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    string? filePath = null;
    Assert.That(() => directoryInfo.File(filePath), Throws.ArgumentNullException);
  }

  [Test]
  public void File_EmptyFilePath_ThrowsArgumentException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    Assert.That(() => directoryInfo.File(string.Empty), Throws.ArgumentException);
  }

  [Test]
  [TestCase("file.txt")]
  [TestCase("anotherfile.doc")]
  public void File_ValidFilePath_ReturnsCorrectFileInfo(string filePath) {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    var result = directoryInfo.File(filePath);

    Assert.That(result.FullName, Is.EqualTo(Path.Combine(directoryInfo.FullName, filePath)));
  }

  // File(params string[] filePath) Tests
  [Test]
  public void File_WithFilePathArray_NullDirectory_ThrowsNullReferenceException() {
    DirectoryInfo? directoryInfo = null;
    Assert.That(() => directoryInfo.File("dir1", "file.txt"), Throws.TypeOf<NullReferenceException>());
  }

  [Test]
  public void File_WithFilePathArray_NullFilePath_ThrowsArgumentNullException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    string[]? filePath = null;
    Assert.That(() => directoryInfo.File(filePath), Throws.ArgumentNullException);
  }

  [Test]
  public void File_WithFilePathArray_EmptyFilePath_ThrowsArgumentException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    Assert.That(() => directoryInfo.File(), Throws.ArgumentException);
  }

  [Test]
  [TestCase(new [] { "dir1", "file.txt" }, 0)]
  [TestCase(new [] { "dir1", "dir2", "file.doc" }, 0)]
  public void File_WithFilePathArray_ValidFilePath_ReturnsCorrectFileInfo(string[] filePath, int _) {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    var result = directoryInfo.File(filePath);

    var expectedPath = Combine(directoryInfo.FullName, filePath);
    Assert.That(result.FullName, Is.EqualTo(expectedPath));
  }

  // File(bool ignoreCase, params string[] filePath) Tests
  [Test]
  public void File_WithExactCase_NullDirectory_ThrowsNullReferenceException() {
    DirectoryInfo? directoryInfo = null;
    Assert.That(() => directoryInfo.File(false, "dir1", "file.txt"), Throws.TypeOf<NullReferenceException>());
  }

  [Test]
  public void File_WithExactCase_NullFilePath_ThrowsArgumentNullException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    string[]? filePath = null;
    Assert.That(() => directoryInfo.File(false, filePath), Throws.ArgumentNullException);
  }

  [Test]
  public void File_WithExactCase_EmptyFilePath_ThrowsArgumentException() {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    Assert.That(() => directoryInfo.File(false), Throws.ArgumentException);
  }

  [Test]
  [TestCase(new [] { "dir1", "file.txt" }, false)]
  [TestCase(new [] { "dir1", "dir2", "file.doc" }, true)]
  public void File_WithExactCase_ValidFilePath_ReturnsCorrectFileInfo(string[] filePath, bool ignoreCase) {
    var directoryInfo = new DirectoryInfo(Path.GetTempPath());
    var result = directoryInfo.File(ignoreCase, filePath);

    var expectedPath = Combine(directoryInfo.FullName, filePath);
    Assert.That(result.FullName, Is.EqualTo(expectedPath));
  }

  [TestFixture]
  public class CasingTests {
    private DirectoryInfo _testDirectory;

    [SetUp]
    public void SetUp() {
      _testDirectory = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
      _testDirectory.Create();
    }

    [TearDown]
    public void TearDown() {
      if (_testDirectory.Exists) {
        _testDirectory.Delete(true);
      }
    }

    [Test]
    public void Directory_IgnoreCase_ExistingSubdirectory_ReturnsCorrectCasing() {
      var subDirName = "TestSubDir";
      var subDirPath = Path.Combine(_testDirectory.FullName, subDirName);
      Directory.CreateDirectory(subDirPath);

      var result = _testDirectory.Directory(true, "testsubdir");

      Assert.That(result.FullName, Is.EqualTo(subDirPath));
    }

    [Test]
    public void Directory_IgnoreCase_NonExistingSubdirectory_ReturnsGivenCasing() {
      var result = _testDirectory.Directory(true, "testsubdir");

      var expectedPath = Path.Combine(_testDirectory.FullName, "testsubdir");
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public void Directory_CaseSensitive_ExistingSubdirectory_ReturnsCorrectCasing() {
      var subDirName = "TestSubDir";
      var subDirPath = Path.Combine(_testDirectory.FullName, subDirName);
      Directory.CreateDirectory(subDirPath);

      var result = _testDirectory.Directory(false, "TestSubDir");

      Assert.That(result.FullName, Is.EqualTo(subDirPath));
    }

    [Test]
    public void Directory_CaseSensitive_NonExistingSubdirectory_ReturnsGivenCasing() {
      var result = _testDirectory.Directory(false, "testsubdir");

      var expectedPath = Path.Combine(_testDirectory.FullName, "testsubdir");
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public void File_IgnoreCase_ExistingFile_ReturnsCorrectCasing() {
      var fileName = "TestFile.txt";
      var filePath = Path.Combine(_testDirectory.FullName, fileName);
      File.WriteAllText(filePath, "Test content");

      var result = _testDirectory.File(true, "testfile.txt");

      Assert.That(result.FullName, Is.EqualTo(filePath));
    }

    [Test]
    public void File_IgnoreCase_NonExistingFile_ReturnsGivenCasing() {
      var result = _testDirectory.File(true, "testfile.txt");

      var expectedPath = Path.Combine(_testDirectory.FullName, "testfile.txt");
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public void File_CaseSensitive_ExistingFile_ReturnsCorrectCasing() {
      var fileName = "TestFile.txt";
      var filePath = Path.Combine(_testDirectory.FullName, fileName);
      File.WriteAllText(filePath, "Test content");

      var result = _testDirectory.File(false, "TestFile.txt");

      Assert.That(result.FullName, Is.EqualTo(filePath));
    }

    [Test]
    public void File_CaseSensitive_NonExistingFile_ReturnsGivenCasing() {
      var result = _testDirectory.File(false, "testfile.txt");

      var expectedPath = Path.Combine(_testDirectory.FullName, "testfile.txt");
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public void Directory_IgnoreCase_ExistingNestedSubdirectories_ReturnsCorrectCasing() {
      var subDirs = new[] { "Dir1", "Dir2" };
      var nestedDirPath = Combine(_testDirectory.FullName, subDirs);
      Directory.CreateDirectory(nestedDirPath);

      var result = _testDirectory.Directory(true, "dir1", "dir2");

      Assert.That(result.FullName, Is.EqualTo(nestedDirPath));
    }

    [Test]
    public void Directory_CaseSensitive_ExistingNestedSubdirectories_ReturnsCorrectCasing() {
      var subDirs = new[] { "Dir1", "Dir2" };
      var nestedDirPath = Combine(_testDirectory.FullName, subDirs);
      Directory.CreateDirectory(nestedDirPath);

      var result = _testDirectory.Directory(false, "Dir1", "Dir2");

      Assert.That(result.FullName, Is.EqualTo(nestedDirPath));
    }

    [Test]
    public void Directory_IgnoreCase_NonExistingNestedSubdirectories_ReturnsGivenCasing() {
      var subDirs = new[] { "dir1", "dir2" };
      var result = _testDirectory.Directory(true, subDirs);

      var expectedPath = Combine(_testDirectory.FullName, subDirs);
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public void Directory_CaseSensitive_NonExistingNestedSubdirectories_ReturnsGivenCasing() {
      var subDirs = new[] { "dir1", "dir2" };
      var result = _testDirectory.Directory(false, subDirs);

      var expectedPath = Combine(_testDirectory.FullName, subDirs);
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public void File_IgnoreCase_ExistingNestedFile_ReturnsCorrectCasing() {
      var subDirs = new[] { "Dir1", "Dir2" };
      var fileName = "TestFile.txt";
      var nestedFilePath = Path.Combine(Combine(_testDirectory.FullName, subDirs), fileName);
      Directory.CreateDirectory(Path.GetDirectoryName(nestedFilePath)!);
      File.WriteAllText(nestedFilePath, "Test content");

      var result = _testDirectory.File(true, "dir1", "dir2", "testfile.txt");

      Assert.That(result.FullName, Is.EqualTo(nestedFilePath));
    }

    [Test]
    public void File_CaseSensitive_ExistingNestedFile_ReturnsCorrectCasing() {
      var subDirs = new[] { "Dir1", "Dir2" };
      var fileName = "TestFile.txt";
      var nestedFilePath = Path.Combine(Combine(_testDirectory.FullName, subDirs), fileName);
      Directory.CreateDirectory(Path.GetDirectoryName(nestedFilePath)!);
      File.WriteAllText(nestedFilePath, "Test content");

      var result = _testDirectory.File(false, "Dir1", "Dir2", "TestFile.txt");

      Assert.That(result.FullName, Is.EqualTo(nestedFilePath));
    }

    [Test]
    public void File_IgnoreCase_NonExistingNestedFile_ReturnsGivenCasing() {
      var subDirs = new[] { "dir1", "dir2" };
      var fileName = "testfile.txt";
      var result = _testDirectory.File(true, subDirs.Append(fileName).ToArray());

      var expectedPath = Path.Combine(Combine(_testDirectory.FullName, subDirs), fileName);
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public void File_CaseSensitive_NonExistingNestedFile_ReturnsGivenCasing() {
      var subDirs = new[] { "dir1", "dir2" };
      var fileName = "testfile.txt";
      var result = _testDirectory.File(false, subDirs.Append(fileName).ToArray());

      var expectedPath = Path.Combine(Combine(_testDirectory.FullName, subDirs), fileName);
      Assert.That(result.FullName, Is.EqualTo(expectedPath));
    }
  }

}