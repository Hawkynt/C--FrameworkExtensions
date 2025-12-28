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
using System.IO;
using System.IO.Compression;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ZipFile")]
public class ZipFileTests {

  #region CreateFromDirectory

  [Test]
  [Category("HappyPath")]
  public void CreateFromDirectory_SimpleDirectory_CreatesArchive() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSampleDirectoryStructure();
    var zipPath = helper.GetZipPath("from_dir");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    Assert.That(File.Exists(zipPath), Is.True);

    using var archive = ZipFile.OpenRead(zipPath);
    Assert.That(archive.Entries.Count, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CreateFromDirectory_IncludeBaseDirectory_IncludesRootFolder() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSampleDirectoryStructure("myroot");
    var zipPath = helper.GetZipPath("with_base");

    ZipFile.CreateFromDirectory(sourceDir, zipPath, CompressionLevel.Optimal, includeBaseDirectory: true);

    using var archive = ZipFile.OpenRead(zipPath);
    foreach (var entry in archive.Entries) {
      // Normalize path separators - some implementations use \ on Windows
      var normalizedName = entry.FullName.Replace('\\', '/');
      Assert.That(normalizedName, Does.StartWith("myroot/"));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void CreateFromDirectory_ExcludeBaseDirectory_StartsWithContents() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSampleDirectoryStructure("myroot");
    var zipPath = helper.GetZipPath("without_base");

    ZipFile.CreateFromDirectory(sourceDir, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);

    using var archive = ZipFile.OpenRead(zipPath);
    var hasRootPrefix = false;
    foreach (var entry in archive.Entries) {
      // Normalize path separators
      var normalizedName = entry.FullName.Replace('\\', '/');
      if (normalizedName.StartsWith("myroot/"))
        hasRootPrefix = true;
    }
    Assert.That(hasRootPrefix, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CreateFromDirectory_WithCompressionLevel_RespectsLevel() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("compress_test");
    File.WriteAllText(Path.Combine(sourceDir, "data.txt"), new string('A', 10000));

    var zipOptimal = helper.GetZipPath("optimal");
    var zipNoCompression = helper.GetZipPath("nocompression");

    ZipFile.CreateFromDirectory(sourceDir, zipOptimal, CompressionLevel.Optimal, false);
    ZipFile.CreateFromDirectory(sourceDir, zipNoCompression, CompressionLevel.NoCompression, false);

    var optimalSize = new FileInfo(zipOptimal).Length;
    var noCompressionSize = new FileInfo(zipNoCompression).Length;

    Assert.That(optimalSize, Is.LessThan(noCompressionSize));
  }

  [Test]
  [Category("HappyPath")]
  public void CreateFromDirectory_NestedDirectories_PreservesStructure() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSampleDirectoryStructure();
    var zipPath = helper.GetZipPath("nested");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    using var archive = ZipFile.OpenRead(zipPath);
    var hasNestedFile = false;
    foreach (var entry in archive.Entries) {
      // Check for either path separator style
      if (entry.FullName.Contains("/") || entry.FullName.Contains("\\"))
        hasNestedFile = true;
    }
    Assert.That(hasNestedFile, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void CreateFromDirectory_NullSource_ThrowsArgumentNullException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("null_source");

    Assert.Throws<ArgumentNullException>(() => ZipFile.CreateFromDirectory(null, zipPath));
  }

  [Test]
  [Category("Exception")]
  public void CreateFromDirectory_NullDestination_ThrowsArgumentNullException() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");

    Assert.Throws<ArgumentNullException>(() => ZipFile.CreateFromDirectory(sourceDir, (string)null));
  }

  [Test]
  [Category("Exception")]
  public void CreateFromDirectory_NonexistentSource_ThrowsDirectoryNotFoundException() {
    using var helper = new ZipTestHelpers();
    var nonexistentDir = Path.Combine(helper.TestDirectory, "doesnotexist");
    var zipPath = helper.GetZipPath("nonexistent");

    Assert.Throws<DirectoryNotFoundException>(() => ZipFile.CreateFromDirectory(nonexistentDir, zipPath));
  }

  #endregion

  #region ExtractToDirectory

  [Test]
  [Category("HappyPath")]
  public void ExtractToDirectory_ValidArchive_ExtractsAllFiles() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSampleDirectoryStructure();
    var zipPath = helper.GetZipPath("extract_test");
    var extractDir = helper.CreateSubDirectory("extracted");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);
    ZipFile.ExtractToDirectory(zipPath, extractDir);

    Assert.That(File.Exists(Path.Combine(extractDir, "file1.txt")), Is.True);
    Assert.That(File.Exists(Path.Combine(extractDir, "file2.txt")), Is.True);
    Assert.That(File.Exists(Path.Combine(Path.Combine(extractDir, "subdir"), "nested.txt")), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ExtractToDirectory_PreservesContent() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    const string expectedContent = "Hello, World!";
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), expectedContent);

    var zipPath = helper.GetZipPath("content_test");
    var extractDir = helper.CreateSubDirectory("extracted");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);
    ZipFile.ExtractToDirectory(zipPath, extractDir);

    var actualContent = File.ReadAllText(Path.Combine(extractDir, "test.txt"));
    Assert.That(actualContent, Is.EqualTo(expectedContent));
  }

#if !SUPPORTS_ZIPARCHIVE

  [Test]
  [Category("HappyPath")]
  public void ExtractToDirectory_OverwriteTrue_OverwritesExisting() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "New content");

    var zipPath = helper.GetZipPath("overwrite_test");
    var extractDir = helper.CreateSubDirectory("extracted");

    // Create existing file with different content
    File.WriteAllText(Path.Combine(extractDir, "test.txt"), "Old content");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);
    ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);

    var content = File.ReadAllText(Path.Combine(extractDir, "test.txt"));
    Assert.That(content, Is.EqualTo("New content"));
  }

  [Test]
  [Category("Exception")]
  public void ExtractToDirectory_OverwriteFalse_ThrowsOnExisting() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    var zipPath = helper.GetZipPath("no_overwrite");
    var extractDir = helper.CreateSubDirectory("extracted");

    // Create existing file
    File.WriteAllText(Path.Combine(extractDir, "test.txt"), "Existing");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    Assert.Throws<IOException>(() => ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: false));
  }

#endif

  [Test]
  [Category("Exception")]
  public void ExtractToDirectory_NullSource_ThrowsArgumentNullException() {
    using var helper = new ZipTestHelpers();
    var extractDir = helper.CreateSubDirectory("extract");

    Assert.Throws<ArgumentNullException>(() => ZipFile.ExtractToDirectory((string)null, extractDir));
  }

  [Test]
  [Category("Exception")]
  public void ExtractToDirectory_NullDestination_ThrowsArgumentNullException() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    var zipPath = helper.GetZipPath("null_dest");
    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    Assert.Throws<ArgumentNullException>(() => ZipFile.ExtractToDirectory(zipPath, null));
  }

  [Test]
  [Category("Exception")]
  public void ExtractToDirectory_NonexistentArchive_ThrowsFileNotFoundException() {
    using var helper = new ZipTestHelpers();
    var extractDir = helper.CreateSubDirectory("extract");
    var nonexistentZip = helper.GetZipPath("doesnotexist");

    Assert.Throws<FileNotFoundException>(() => ZipFile.ExtractToDirectory(nonexistentZip, extractDir));
  }

  #endregion

  #region Open and OpenRead

  [Test]
  [Category("HappyPath")]
  public void Open_ReadMode_OpensForReading() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    var zipPath = helper.GetZipPath("open_read");
    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Read);
    Assert.That(archive.Mode, Is.EqualTo(ZipArchiveMode.Read));
    Assert.That(archive.Entries.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Open_CreateMode_CreatesNewArchive() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("open_create");

    using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
      Assert.That(archive.Mode, Is.EqualTo(ZipArchiveMode.Create));
      var entry = archive.CreateEntry("new.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Created content");
    }

    Assert.That(File.Exists(zipPath), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Open_UpdateMode_AllowsModification() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "original.txt"), "Original");

    var zipPath = helper.GetZipPath("open_update");
    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update)) {
      Assert.That(archive.Mode, Is.EqualTo(ZipArchiveMode.Update));
      var entry = archive.CreateEntry("added.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Added");
    }

    using (var archive = ZipFile.OpenRead(zipPath)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(2));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void OpenRead_ValidArchive_OpensInReadMode() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    var zipPath = helper.GetZipPath("openread");
    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    using var archive = ZipFile.OpenRead(zipPath);
    Assert.That(archive.Mode, Is.EqualTo(ZipArchiveMode.Read));
  }

  [Test]
  [Category("Exception")]
  public void Open_NullPath_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => ZipFile.Open(null, ZipArchiveMode.Read));
  }

  #endregion

  #region ZipFileExtensions

  [Test]
  [Category("HappyPath")]
  public void CreateEntryFromFile_AddsFileToArchive() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("entry_from_file");
    var sourceFile = helper.CreateTestFile("source.txt", "File content to add");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntryFromFile(sourceFile, "archived.txt");
    }

    using (var archive = ZipFile.OpenRead(zipPath)) {
      var entry = archive.GetEntry("archived.txt");
      Assert.That(entry, Is.Not.Null);

      using (var reader = new StreamReader(entry.Open())) {
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("File content to add"));
      }
    }
  }

  [Test]
  [Category("HappyPath")]
  public void CreateEntryFromFile_WithCompressionLevel_RespectsLevel() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("entry_compression");
    var sourceFile = helper.CreateTestFile("source.txt", new string('A', 10000));

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntryFromFile(sourceFile, "compressed.txt", CompressionLevel.Optimal);
    }

    using (var archive = ZipFile.OpenRead(zipPath)) {
      var entry = archive.GetEntry("compressed.txt");
      Assert.That(entry.CompressedLength, Is.LessThan(entry.Length));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ExtractToFile_ExtractsEntryToFile() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    const string expectedContent = "Content to extract";
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), expectedContent);

    var zipPath = helper.GetZipPath("extract_entry");
    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    var extractPath = Path.Combine(helper.TestDirectory, "extracted.txt");

    using (var archive = ZipFile.OpenRead(zipPath)) {
      var entry = archive.GetEntry("test.txt");
      entry.ExtractToFile(extractPath);
    }

    Assert.That(File.Exists(extractPath), Is.True);
    Assert.That(File.ReadAllText(extractPath), Is.EqualTo(expectedContent));
  }

  [Test]
  [Category("HappyPath")]
  public void ExtractToFile_OverwriteTrue_OverwritesExisting() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "New content");

    var zipPath = helper.GetZipPath("extract_overwrite");
    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    var extractPath = Path.Combine(helper.TestDirectory, "extracted.txt");
    File.WriteAllText(extractPath, "Old content");

    using (var archive = ZipFile.OpenRead(zipPath)) {
      var entry = archive.GetEntry("test.txt");
      entry.ExtractToFile(extractPath, overwrite: true);
    }

    Assert.That(File.ReadAllText(extractPath), Is.EqualTo("New content"));
  }

  [Test]
  [Category("Exception")]
  public void ExtractToFile_OverwriteFalse_ThrowsOnExisting() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    var zipPath = helper.GetZipPath("no_overwrite_entry");
    ZipFile.CreateFromDirectory(sourceDir, zipPath);

    var extractPath = Path.Combine(helper.TestDirectory, "extracted.txt");
    File.WriteAllText(extractPath, "Existing");

    using var archive = ZipFile.OpenRead(zipPath);
    var entry = archive.GetEntry("test.txt");
    Assert.Throws<IOException>(() => entry.ExtractToFile(extractPath, overwrite: false));
  }

  [Test]
  [Category("Exception")]
  public void CreateEntryFromFile_NonexistentFile_ThrowsFileNotFoundException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("nonexistent_source");
    var nonexistentFile = Path.Combine(helper.TestDirectory, "doesnotexist.txt");

    using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write);
    using var archive = new ZipArchive(fs, ZipArchiveMode.Create);
    Assert.Throws<FileNotFoundException>(() => archive.CreateEntryFromFile(nonexistentFile, "test.txt"));
  }

  #endregion

  #region Zip Slip Protection

#if !SUPPORTS_ZIPARCHIVE

  [Test]
  [Category("Exception")]
  [Category("Security")]
  public void ExtractToDirectory_ZipSlipAttempt_ThrowsIOException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("zipslip");
    var extractDir = helper.CreateSubDirectory("extract");

    // Create a malicious archive with path traversal
    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("../../../evil.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Malicious content");
    }

    Assert.Throws<IOException>(() => ZipFile.ExtractToDirectory(zipPath, extractDir));
  }

#endif

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void CreateFromDirectory_EmptyDirectory_CreatesEmptyArchive() {
    using var helper = new ZipTestHelpers();
    var emptyDir = helper.CreateSubDirectory("empty");
    var zipPath = helper.GetZipPath("empty_dir");

    ZipFile.CreateFromDirectory(emptyDir, zipPath);

    Assert.That(File.Exists(zipPath), Is.True);

    using var archive = ZipFile.OpenRead(zipPath);
    Assert.That(archive.Entries.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void ExtractToDirectory_CreatesDestinationIfNotExists() {
    using var helper = new ZipTestHelpers();
    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    var zipPath = helper.GetZipPath("create_dest");
    var extractDir = Path.Combine(Path.Combine(helper.TestDirectory, "newdir"), "nested");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);
    ZipFile.ExtractToDirectory(zipPath, extractDir);

    Assert.That(Directory.Exists(extractDir), Is.True);
    Assert.That(File.Exists(Path.Combine(extractDir, "test.txt")), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void RoundTrip_BinaryFile_PreservesContent() {
    using var helper = new ZipTestHelpers();
    var binaryData = new byte[1024];
    new Random(42).NextBytes(binaryData);

    var sourceDir = helper.CreateSubDirectory("source");
    File.WriteAllBytes(Path.Combine(sourceDir, "binary.bin"), binaryData);

    var zipPath = helper.GetZipPath("binary_roundtrip");
    var extractDir = helper.CreateSubDirectory("extracted");

    ZipFile.CreateFromDirectory(sourceDir, zipPath);
    ZipFile.ExtractToDirectory(zipPath, extractDir);

    var extractedData = File.ReadAllBytes(Path.Combine(extractDir, "binary.bin"));
    Assert.That(extractedData, Is.EqualTo(binaryData));
  }

  #endregion
}
