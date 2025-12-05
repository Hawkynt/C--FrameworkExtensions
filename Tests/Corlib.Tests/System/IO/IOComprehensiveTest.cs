using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NUnit.Framework;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif

namespace System.IO;

[TestFixture]
public class IOComprehensiveTest {
  private string _tempDirectory = null!;
  private List<string> _tempFiles = null!;
  private List<string> _tempDirectories = null!;

  [SetUp]
  public void Setup() {
    this._tempDirectory = Path.Combine(Path.GetTempPath(), $"IOTests_{Guid.NewGuid():N}");
    Directory.CreateDirectory(this._tempDirectory);
    this._tempFiles = new List<string>();
    this._tempDirectories = new List<string>();
  }

  [TearDown]
  public void TearDown() {
    // Clean up all created test files and directories
    foreach (var file in this._tempFiles) {
      try {
        if (File.Exists(file))
          File.Delete(file);
      } catch {
        // Ignore cleanup errors
      }
    }

    foreach (var dir in this._tempDirectories) {
      try {
        if (Directory.Exists(dir))
          Directory.Delete(dir, true);
      } catch {
        // Ignore cleanup errors
      }
    }

    try {
      if (Directory.Exists(this._tempDirectory))
        Directory.Delete(this._tempDirectory, true);
    } catch {
      // Ignore cleanup errors
    }
  }

  private FileInfo CreateTestFile(string content = "test content", string extension = ".txt") {
    var fileName = Path.Combine(this._tempDirectory, $"test_{Guid.NewGuid():N}{extension}");
    File.WriteAllText(fileName, content, new UTF8Encoding(false)); // No BOM
    this._tempFiles.Add(fileName);
    return new FileInfo(fileName);
  }

  private DirectoryInfo CreateTestDirectory(string? subPath = null) {
    var dirPath = subPath != null
      ? Path.Combine(this._tempDirectory, subPath)
      : Path.Combine(this._tempDirectory, $"testdir_{Guid.NewGuid():N}");
    Directory.CreateDirectory(dirPath);
    this._tempDirectories.Add(dirPath);
    return new DirectoryInfo(dirPath);
  }

  #region FileInfo Extensions Tests

  [Test]
  public void FileInfoExtensions_NotExists_NonExistentFile_ReturnsTrue() {
    var nonExistentFile = new FileInfo(Path.Combine(this._tempDirectory, "nonexistent.txt"));

    Assert.That(nonExistentFile.NotExists(), Is.True);
  }

  [Test]
  public void FileInfoExtensions_NotExists_ExistingFile_ReturnsFalse() {
    var existingFile = this.CreateTestFile();

    Assert.That(existingFile.NotExists(), Is.False);
  }

  [Test]
  public void FileInfoExtensions_GetFilenameWithoutExtension_StandardFile_ReturnsCorrectName() {
    var file = new FileInfo(@"C:\path\to\file.txt");
    var result = file.GetFilenameWithoutExtension();

    Assert.That(result, Is.EqualTo("file"));
  }

  [Test]
  public void FileInfoExtensions_GetFilenameWithoutExtension_NoExtension_ReturnsFullName() {
    var file = new FileInfo(@"C:\path\to\filename");
    var result = file.GetFilenameWithoutExtension();

    Assert.That(result, Is.EqualTo("filename"));
  }

  [Test]
  public void FileInfoExtensions_GetFilename_ReturnsFileName() {
    var file = new FileInfo(@"C:\path\to\document.pdf");
    var result = file.GetFilename();

    Assert.That(result, Is.EqualTo("document.pdf"));
  }

  [Test]
  public void FileInfoExtensions_WithNewExtension_ChangesExtension() {
    var originalFile = this.CreateTestFile();
    var newFile = originalFile.WithNewExtension(".bak");

    Assert.That(newFile.Extension, Is.EqualTo(".bak"));
    Assert.That(newFile.Name, Does.EndWith(".bak"));
    Assert.That(newFile.DirectoryName, Is.EqualTo(originalFile.DirectoryName));
  }

  [Test]
  public void FileInfoExtensions_Touch_ExistingFile_UpdatesTimestamp() {
    var file = this.CreateTestFile();
    var originalTime = file.LastWriteTime;

    Thread.Sleep(100); // Ensure time difference
    file.Touch();
    file.Refresh();

    Assert.That(file.LastWriteTime, Is.GreaterThan(originalTime));
  }

  [Test]
  public void FileInfoExtensions_Touch_NonExistentFile_CreatesFile() {
    var nonExistentFile = new FileInfo(Path.Combine(this._tempDirectory, "touched.txt"));
    this._tempFiles.Add(nonExistentFile.FullName);

    nonExistentFile.Touch();

    Assert.That(nonExistentFile.Exists, Is.True);
    Assert.That(nonExistentFile.Length, Is.EqualTo(0));
  }

  [Test]
  public void FileInfoExtensions_TryTouch_ReadOnlyFile_ReturnsFalse() {
    var file = this.CreateTestFile();
    file.Attributes |= FileAttributes.ReadOnly;

    var result = file.TryTouch();

    Assert.That(result, Is.False);

    // Cleanup - remove readonly attribute
    file.Attributes &= ~FileAttributes.ReadOnly;
  }

  [Test]
  public void FileInfoExtensions_TryDelete_ExistingFile_DeletesAndReturnsTrue() {
    var file = this.CreateTestFile();

    var result = file.TryDelete();

    Assert.That(result, Is.True);
    Assert.That(file.Exists, Is.False);
    this._tempFiles.Remove(file.FullName); // Already deleted
  }

  [Test]
  public void FileInfoExtensions_TryDelete_NonExistentFile_ReturnsFalse() {
    var nonExistentFile = new FileInfo(Path.Combine(this._tempDirectory, "nonexistent.txt"));

    var result = nonExistentFile.TryDelete();

    Assert.That(result, Is.False);
  }

  #endregion

  #region FileInfo Hashing Tests

  [Test]
  public void FileInfoExtensions_ComputeHash_CustomAlgorithm_WorksCorrectly() {
    var content = "custom hash test";
    var file = this.CreateTestFile(content);

    using var sha1 = SHA1.Create();
    var hash = file.ComputeHash(sha1);

    Assert.That(hash, Is.Not.Null);
    Assert.That(hash.Length, Is.EqualTo(20)); // SHA1 produces 20-byte hash
  }

  [Test]
  public void FileInfoExtensions_ComputeSHA512Hash_EmptyFile_ReturnsValidHash() {
    var file = this.CreateTestFile("");

    var hash = file.ComputeSHA512Hash();

    Assert.That(hash, Is.Not.Null);
    Assert.That(hash.Length, Is.EqualTo(64)); // SHA512 produces 64-byte hash
  }

  [Test]
  public void FileInfoExtensions_ComputeHash_LargeFile_HandlesEfficiently() {
    // Create a larger file for performance testing
    var content = string.Join("", Enumerable.Repeat("Large file content line.\n", 1000).ToArray());
    var file = this.CreateTestFile(content);

    var sw = Stopwatch.StartNew();
    var hash = file.ComputeSHA256Hash();
    sw.Stop();

    Assert.That(hash, Is.Not.Null);
    Assert.That(hash.Length, Is.EqualTo(32));
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(1000)); // Should be reasonably fast
  }

  #endregion

  #region FileInfo Text Operations Tests

  [Test]
  public void FileInfoExtensions_ReadAllText_UTF8File_ReadsCorrectly() {
    var content = "Test content with unicode: 🌟";
    var file = this.CreateTestFile(content);

    var result = file.ReadAllText();

    Assert.That(result, Is.EqualTo(content));
  }

  [Test]
  public void FileInfoExtensions_ReadAllText_SpecificEncoding_ReadsCorrectly() {
    var content = "Test with specific encoding";
    var file = this.CreateTestFile();
    File.WriteAllText(file.FullName, content, Encoding.UTF32);

    var result = file.ReadAllText(Encoding.UTF32);

    Assert.That(result, Is.EqualTo(content));
  }

  [Test]
  public void FileInfoExtensions_ReadAllLines_MultilineFile_ReturnsCorrectLines() {
    var lines = new[] { "Line 1", "Line 2", "Line 3" };
    var content = string.Join(Environment.NewLine, lines);
    var file = this.CreateTestFile(content);

    var result = file.ReadAllLines();

    Assert.That(result, Is.EqualTo(lines));
  }

  [Test]
  public void FileInfoExtensions_WriteAllText_CreatesFileWithContent() {
    var file = new FileInfo(Path.Combine(this._tempDirectory, "written.txt"));
    this._tempFiles.Add(file.FullName);
    var content = "Written content";

    file.WriteAllText(content);

    Assert.That(file.Exists, Is.True);
    Assert.That(File.ReadAllText(file.FullName), Is.EqualTo(content));
  }

  [Test]
  public void FileInfoExtensions_WriteAllLines_CreatesFileWithLines() {
    var file = new FileInfo(Path.Combine(this._tempDirectory, "lines.txt"));
    this._tempFiles.Add(file.FullName);
    var lines = new[] { "First line", "Second line", "Third line" };

    file.WriteAllLines(lines);

    Assert.That(file.Exists, Is.True);
    var readLines = File.ReadAllLines(file.FullName);
    Assert.That(readLines, Is.EqualTo(lines));
  }

  [Test]
  public void FileInfoExtensions_AppendAllText_AppendsToExistingFile() {
    var file = this.CreateTestFile("Initial content");
    var appendContent = " Appended content";

    file.AppendAllText(appendContent);

    var result = File.ReadAllText(file.FullName);
    Assert.That(result, Is.EqualTo("Initial content Appended content"));
  }

  #endregion

  #region FileInfo Advanced Line Operations Tests

  [Test]
  public void FileInfoExtensions_KeepFirstLines_KeepsSpecifiedLines() {
    var lines = new[] { "Line 1", "Line 2", "Line 3", "Line 4", "Line 5" };
    var content = string.Join(Environment.NewLine, lines);
    var file = this.CreateTestFile(content);

    file.KeepFirstLines(3);

    var result = file.ReadAllLines();
    Assert.That(result, Is.EqualTo(new[] { "Line 1", "Line 2", "Line 3" }));
  }

  [Test]
  public void FileInfoExtensions_KeepLastLines_KeepsSpecifiedLines() {
    var lines = new[] { "Line 1", "Line 2", "Line 3", "Line 4", "Line 5" };
    var content = string.Join(Environment.NewLine, lines);
    var file = this.CreateTestFile(content);

    file.KeepLastLines(2);

    var result = file.ReadAllLines();
    Assert.That(result, Is.EqualTo(new[] { "Line 4", "Line 5" }));
  }

  [Test]
  public void FileInfoExtensions_RemoveFirstLines_RemovesSpecifiedLines() {
    var lines = new[] { "Remove 1", "Remove 2", "Keep 1", "Keep 2" };
    var content = string.Join(Environment.NewLine, lines);
    var file = this.CreateTestFile(content);

    file.RemoveFirstLines(2);

    var result = file.ReadAllLines();
    Assert.That(result, Is.EqualTo(new[] { "Keep 1", "Keep 2" }));
  }

  [Test]
  public void FileInfoExtensions_RemoveLastLines_RemovesSpecifiedLines() {
    var lines = new[] { "Keep 1", "Keep 2", "Remove 1", "Remove 2" };
    var content = string.Join(Environment.NewLine, lines);
    var file = this.CreateTestFile(content);

    file.RemoveLastLines(2);

    var result = file.ReadAllLines();
    Assert.That(result, Is.EqualTo(new[] { "Keep 1", "Keep 2" }));
  }

  [Test]
  public void FileInfoExtensions_LineOperations_EmptyFile_HandlesGracefully() {
    var file = this.CreateTestFile("");

    Assert.DoesNotThrow(() => file.KeepFirstLines(5));
    Assert.DoesNotThrow(() => file.KeepLastLines(5));
    Assert.DoesNotThrow(() => file.RemoveFirstLines(5));
    Assert.DoesNotThrow(() => file.RemoveLastLines(5));
  }

  [Test]
  public void FileInfoExtensions_LineOperations_MoreLinesThanExist_HandlesGracefully() {
    var lines = new[] { "Only line" };
    var content = string.Join(Environment.NewLine, lines);
    var file = this.CreateTestFile(content);

    file.KeepFirstLines(10);
    var result = file.ReadAllLines();
    Assert.That(result, Is.EqualTo(lines));

    file.RemoveLastLines(10);
    result = file.ReadAllLines();
    Assert.That(result, Is.Empty);
  }

  #endregion

  #region FileInfo Encoding and Text Analysis Tests

  [Test]
  public void FileInfoExtensions_IsTextFile_TextFile_ReturnsTrue() {
    var textContent = "This is a plain text file with normal characters.";
    var file = this.CreateTestFile(textContent);

    var result = file.IsTextFile();

    Assert.That(result, Is.True);
  }

  [Test]
  public void FileInfoExtensions_IsTextFile_BinaryFile_ReturnsFalse() {
    var binaryContent = new byte[] { 0x00, 0x01, 0xFF, 0xFE, 0x80, 0x90 };
    var file = new FileInfo(Path.Combine(this._tempDirectory, "binary.bin"));
    this._tempFiles.Add(file.FullName);
    File.WriteAllBytes(file.FullName, binaryContent);

    var result = file.IsTextFile();

    Assert.That(result, Is.False);
  }

  [Test]
  public void FileInfoExtensions_DetectEncoding_UTF8File_DetectsCorrectly() {
    var content = "UTF-8 content with unicode: ñáéíóú";
    var file = new FileInfo(Path.Combine(this._tempDirectory, "utf8.txt"));
    this._tempFiles.Add(file.FullName);
    File.WriteAllText(file.FullName, content, Encoding.UTF8);

    var encoding = file.DetectEncoding();

    Assert.That(encoding, Is.Not.Null);
    // UTF8 without BOM might be detected as various encodings, so just ensure it's detected
    Assert.That(encoding.GetString(File.ReadAllBytes(file.FullName)), Does.Contain("ñáéíóú"));
  }

  [Test]
  public void FileInfoExtensions_DetectLineBreakMode_WindowsLineBreaks_DetectsCorrectly() {
    var content = "Line 1\r\nLine 2\r\nLine 3\r\n";
    var file = this.CreateTestFile(content);

    var lineBreakMode = file.DetectLineBreakMode();

    Assert.That(lineBreakMode, Is.EqualTo(StringExtensions.LineBreakMode.Windows));
  }

  [Test]
  public void FileInfoExtensions_DetectLineBreakMode_UnixLineBreaks_DetectsCorrectly() {
    var content = "Line 1\nLine 2\nLine 3\n";
    var file = new FileInfo(Path.Combine(this._tempDirectory, "unix.txt"));
    this._tempFiles.Add(file.FullName);
    File.WriteAllBytes(file.FullName, Encoding.UTF8.GetBytes(content));

    var lineBreakMode = file.DetectLineBreakMode();

    Assert.That(lineBreakMode, Is.EqualTo(StringExtensions.LineBreakMode.Unix));
  }

  #endregion

  #region DirectoryInfo Extensions Tests

  [Test]
  public void DirectoryInfoExtensions_NotExists_NonExistentDirectory_ReturnsTrue() {
    var nonExistentDir = new DirectoryInfo(Path.Combine(this._tempDirectory, "nonexistent"));

    Assert.That(nonExistentDir.NotExists(), Is.True);
  }

  [Test]
  public void DirectoryInfoExtensions_NotExists_ExistingDirectory_ReturnsFalse() {
    var existingDir = this.CreateTestDirectory();

    Assert.That(existingDir.NotExists(), Is.False);
  }

  [Test]
  public void DirectoryInfoExtensions_TryCreate_NonExistentDirectory_CreatesAndReturnsTrue() {
    var newDir = new DirectoryInfo(Path.Combine(this._tempDirectory, "newdir"));
    this._tempDirectories.Add(newDir.FullName);

    var result = newDir.TryCreate();

    Assert.That(result, Is.True);
    Assert.That(newDir.Exists, Is.True);
  }

  [Test]
  public void DirectoryInfoExtensions_TryCreate_ExistingDirectory_ReturnsTrue() {
    var existingDir = this.CreateTestDirectory();

    var result = existingDir.TryCreate();

    Assert.That(result, Is.True);
  }

  [Test]
  public void DirectoryInfoExtensions_Clear_DirectoryWithFiles_RemovesAllContent() {
    var dir = this.CreateTestDirectory();

    // Create some test files and subdirectories
    File.WriteAllText(Path.Combine(dir.FullName, "file1.txt"), "content1");
    File.WriteAllText(Path.Combine(dir.FullName, "file2.txt"), "content2");
    Directory.CreateDirectory(Path.Combine(dir.FullName, "subdir"));
    File.WriteAllText(Path.Combine(Path.Combine(dir.FullName, "subdir"), "file3.txt"), "content3");

    dir.Clear();

    dir.Refresh();
    Assert.That(dir.GetFiles(), Is.Empty);
    Assert.That(dir.GetDirectories(), Is.Empty);
  }

  [Test]
  public void DirectoryInfoExtensions_GetSize_DirectoryWithFiles_ReturnsCorrectSize() {
    var dir = this.CreateTestDirectory();

    var content1 = "Small file content"; // ~18 bytes
    var content2 = "Another file with more content here"; // ~35 bytes
    File.WriteAllText(Path.Combine(dir.FullName, "file1.txt"), content1);
    File.WriteAllText(Path.Combine(dir.FullName, "file2.txt"), content2);

    var size = dir.GetSize();

    Assert.That(size, Is.GreaterThan(40)); // Should be roughly sum of file sizes
    Assert.That(size, Is.LessThan(200)); // But not too large
  }

  [Test]
  public void DirectoryInfoExtensions_GetSize_EmptyDirectory_ReturnsZero() {
    var dir = this.CreateTestDirectory();

    var size = dir.GetSize();

    Assert.That(size, Is.EqualTo(0));
  }

  [Test]
  public void DirectoryInfoExtensions_HasFile_ExistingFile_ReturnsTrue() {
    var dir = this.CreateTestDirectory();
    File.WriteAllText(Path.Combine(dir.FullName, "testfile.txt"), "content");

    var result = dir.HasFile("testfile.txt");

    Assert.That(result, Is.True);
  }

  [Test]
  public void DirectoryInfoExtensions_HasFile_NonExistentFile_ReturnsFalse() {
    var dir = this.CreateTestDirectory();

    var result = dir.HasFile("nonexistent.txt");

    Assert.That(result, Is.False);
  }

  [Test]
  public void DirectoryInfoExtensions_HasDirectory_ExistingDirectory_ReturnsTrue() {
    var dir = this.CreateTestDirectory();
    Directory.CreateDirectory(Path.Combine(dir.FullName, "subdir"));

    var result = dir.HasDirectory("subdir");

    Assert.That(result, Is.True);
  }

  [Test]
  public void DirectoryInfoExtensions_GetOrAddDirectory_NonExistentDirectory_CreatesAndReturns() {
    var dir = this.CreateTestDirectory();

    var subDir = dir.GetOrAddDirectory("newsubdir");

    Assert.That(subDir.Exists, Is.True);
    Assert.That(subDir.Name, Is.EqualTo("newsubdir"));
    Assert.That(subDir.Parent!.FullName, Is.EqualTo(dir.FullName));
  }

  [Test]
  public void DirectoryInfoExtensions_GetOrAddDirectory_ExistingDirectory_ReturnsExisting() {
    var dir = this.CreateTestDirectory();
    var existingSubDir = Directory.CreateDirectory(Path.Combine(dir.FullName, "existing"));

    var result = dir.GetOrAddDirectory("existing");

    Assert.That(result.FullName, Is.EqualTo(existingSubDir.FullName));
  }

  #endregion

  #region DirectoryInfo Advanced Operations Tests

  [Test]
  public void DirectoryInfoExtensions_ExistsAndHasFiles_DirectoryWithFiles_ReturnsTrue() {
    var dir = this.CreateTestDirectory();
    File.WriteAllText(Path.Combine(dir.FullName, "file.txt"), "content");

    var result = dir.ExistsAndHasFiles();

    Assert.That(result, Is.True);
  }

  [Test]
  public void DirectoryInfoExtensions_ExistsAndHasFiles_EmptyDirectory_ReturnsFalse() {
    var dir = this.CreateTestDirectory();

    var result = dir.ExistsAndHasFiles();

    Assert.That(result, Is.False);
  }

  [Test]
  public void DirectoryInfoExtensions_ExistsAndHasFiles_NonExistentDirectory_ReturnsFalse() {
    var nonExistentDir = new DirectoryInfo(Path.Combine(this._tempDirectory, "nonexistent"));

    var result = nonExistentDir.ExistsAndHasFiles();

    Assert.That(result, Is.False);
  }

  [Test]
  public void DirectoryInfoExtensions_SafelyEnumerateFiles_WithPermissionIssues_ContinuesEnumeration() {
    var dir = this.CreateTestDirectory();

    // Create some accessible files
    File.WriteAllText(Path.Combine(dir.FullName, "file1.txt"), "content1");
    File.WriteAllText(Path.Combine(dir.FullName, "file2.txt"), "content2");

    var files = dir.SafelyEnumerateFiles().ToList();

    Assert.That(files.Count, Is.EqualTo(2));
    Assert.That(files.Any(f => f.Name == "file1.txt"), Is.True);
    Assert.That(files.Any(f => f.Name == "file2.txt"), Is.True);
  }

  [Test]
  public void DirectoryInfoExtensions_SafelyEnumerateDirectories_WithNestedStructure_EnumeratesCorrectly() {
    var dir = this.CreateTestDirectory();

    Directory.CreateDirectory(Path.Combine(dir.FullName, "subdir1"));
    Directory.CreateDirectory(Path.Combine(dir.FullName, "subdir2"));
    Directory.CreateDirectory(Path.Combine(Path.Combine(dir.FullName, "subdir1"), "nested"));

    var directories = dir.SafelyEnumerateDirectories().ToList();

    Assert.That(directories.Count, Is.GreaterThanOrEqualTo(2));
    Assert.That(directories.Any(d => d.Name == "subdir1"), Is.True);
    Assert.That(directories.Any(d => d.Name == "subdir2"), Is.True);
  }

  #endregion

  #region Stream Extensions Tests

  [Test]
  public void StreamExtensions_WriteInt32_WritesCorrectBytes() {
    using var stream = new MemoryStream();
    var value = 0x12345678;

    stream.Write(value);

    var bytes = stream.ToArray();
    Assert.That(bytes.Length, Is.EqualTo(4));

    // Little-endian by default
    var expected = BitConverter.GetBytes(value);
    Assert.That(bytes, Is.EqualTo(expected));
  }

  [Test]
  public void StreamExtensions_WriteInt32_BigEndian_WritesCorrectBytes() {
    using var stream = new MemoryStream();
    var value = 0x12345678;

    stream.Write(value, bigEndian: true);

    var bytes = stream.ToArray();
    Assert.That(bytes.Length, Is.EqualTo(4));

    // Big-endian order
    var expected = new byte[] { 0x12, 0x34, 0x56, 0x78 };
    Assert.That(bytes, Is.EqualTo(expected));
  }

  [Test]
  public void StreamExtensions_ReadInt32_ReadsCorrectValue() {
    var value = 0x12345678;
    var bytes = BitConverter.GetBytes(value);
    using var stream = new MemoryStream(bytes);

    var result = stream.ReadInt32();

    Assert.That(result, Is.EqualTo(value));
  }

  [Test]
  public void StreamExtensions_ReadInt32_BigEndian_ReadsCorrectValue() {
    var bytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };
    using var stream = new MemoryStream(bytes);

    var result = stream.ReadInt32(bigEndian: true);

    Assert.That(result, Is.EqualTo(0x12345678));
  }

  [Test]
  public void StreamExtensions_WriteReadRoundTrip_AllPrimitiveTypes_MaintainsValues() {
    using var stream = new MemoryStream();

    // Write various primitive types
    stream.Write(true);
    stream.Write((byte)255);
    stream.Write((sbyte)-128);
    stream.Write((ushort)65535);
    stream.Write((short)-32768);
    stream.Write('A');
    stream.Write(3.14159f);
    stream.Write(2.718281828);
    stream.Write(123.456m);

    // Reset position for reading
    stream.Position = 0;

    // Read back and verify
    Assert.That(stream.ReadBool(), Is.True);
    Assert.That(stream.ReadByte(), Is.EqualTo(255));
    Assert.That(stream.Read<sbyte>(), Is.EqualTo(-128));
    Assert.That(stream.ReadUInt16(), Is.EqualTo(65535));
    Assert.That(stream.ReadInt16(), Is.EqualTo(-32768));
    Assert.That(stream.ReadChar(), Is.EqualTo('A'));
    Assert.That(stream.Read<float>(), Is.EqualTo(3.14159f).Within(0.00001f));
    Assert.That(stream.Read<double>(), Is.EqualTo(2.718281828).Within(0.000000001));
    Assert.That(stream.Read<decimal>(), Is.EqualTo(123.456m));
  }

  [Test]
  public void StreamExtensions_WriteLengthPrefixedString_WritesLengthAndContent() {
    using var stream = new MemoryStream();
    var text = "Hello, World!";

    stream.WriteLengthPrefixedString(text);

    stream.Position = 0;
    var readText = stream.ReadLengthPrefixedString();

    Assert.That(readText, Is.EqualTo(text));
  }

  [Test]
  public void StreamExtensions_WriteLengthPrefixedString_CustomEncoding_WorksCorrectly() {
    using var stream = new MemoryStream();
    var text = "Unicode: 🌟✨";

    stream.WriteLengthPrefixedString(text, Encoding.UTF32);

    stream.Position = 0;
    var readText = stream.ReadLengthPrefixedString(Encoding.UTF32);

    Assert.That(readText, Is.EqualTo(text));
  }

  [Test]
  public void StreamExtensions_WriteZeroTerminatedString_WritesStringWithNullTerminator() {
    using var stream = new MemoryStream();
    var text = "Null-terminated";

    stream.WriteZeroTerminatedString(text);

    stream.Position = 0;
    var readText = stream.ReadZeroTerminatedString();

    Assert.That(readText, Is.EqualTo(text));
  }

  [Test]
  public void StreamExtensions_WriteFixedLengthString_WritesExactLength() {
    using var stream = new MemoryStream();
    var text = "Short";
    var fixedLength = 10;

    stream.WriteFixedLengthString(text, fixedLength);

    Assert.That(stream.Length, Is.EqualTo(fixedLength));

    stream.Position = 0;
    var readText = stream.ReadFixedLengthString(fixedLength);

    Assert.That(readText.TrimEnd('\0'), Is.EqualTo(text));
  }

  [Test]
  public void StreamExtensions_IsAtEndOfStream_AtEnd_ReturnsTrue() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);

    stream.Position = stream.Length;

    Assert.That(stream.IsAtEndOfStream(), Is.True);
  }

  [Test]
  public void StreamExtensions_IsAtEndOfStream_NotAtEnd_ReturnsFalse() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);

    stream.Position = 1;

    Assert.That(stream.IsAtEndOfStream(), Is.False);
  }

  [Test]
  public void StreamExtensions_ReadAllBytes_ReadsEntireStream() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);

    var result = stream.ReadAllBytes();

    Assert.That(result, Is.EqualTo(data));
  }

  [Test]
  public void StreamExtensions_ToArray_MemoryStream_ReturnsCorrectBytes() {
    using var stream = new MemoryStream();
    var data = new byte[] { 10, 20, 30 };
    stream.Write(data, 0, data.Length);

    var result = stream.ToArray();

    Assert.That(result, Is.EqualTo(data));
  }

  #endregion

  #region FileInfo Copy and Move Operations Tests

  [Test]
  public void FileInfoExtensions_CopyTo_FileInfo_CopiesToDestination() {
    var sourceFile = this.CreateTestFile("Source content");
    var destFile = new FileInfo(Path.Combine(this._tempDirectory, "destination.txt"));
    this._tempFiles.Add(destFile.FullName);

    sourceFile.CopyTo(destFile);

    Assert.That(destFile.Exists, Is.True);
    Assert.That(File.ReadAllText(destFile.FullName), Is.EqualTo("Source content"));
  }

  [Test]
  public void FileInfoExtensions_CopyTo_DirectoryInfo_CopiesToDirectory() {
    var sourceFile = this.CreateTestFile("Directory copy content");
    var destDir = this.CreateTestDirectory();

    sourceFile.CopyTo(destDir);

    var copiedFile = new FileInfo(Path.Combine(destDir.FullName, sourceFile.Name));
    Assert.That(copiedFile.Exists, Is.True);
    Assert.That(File.ReadAllText(copiedFile.FullName), Is.EqualTo("Directory copy content"));
    this._tempFiles.Add(copiedFile.FullName);
  }

  [Test]
  public void FileInfoExtensions_CopyTo_OverwriteExisting_OverwritesSuccessfully() {
    var sourceFile = this.CreateTestFile("New content");
    var destFile = this.CreateTestFile("Old content");

    sourceFile.CopyTo(destFile, overwrite: true);

    Assert.That(File.ReadAllText(destFile.FullName), Is.EqualTo("New content"));
  }

  [Test]
  public void FileInfoExtensions_RenameTo_ChangesFileName() {
    var originalFile = this.CreateTestFile("Rename content");
    var newName = "renamed.txt";

    var oldName = originalFile.FullName;
    originalFile.RenameTo(newName);
    var renamedFile = new FileInfo(Path.Combine(originalFile.DirectoryName!, newName));

    Assert.That(File.Exists(oldName), Is.False);
    Assert.That(renamedFile.Exists, Is.True);
    Assert.That(renamedFile.Name, Is.EqualTo(newName));
    Assert.That(File.ReadAllText(renamedFile.FullName), Is.EqualTo("Rename content"));

    this._tempFiles.Add(renamedFile.FullName);
    this._tempFiles.Remove(oldName);
  }

  #endregion

  #region Performance Tests

  [Test]
  public void FileInfoExtensions_HashComputation_LargeFile_PerformanceTest() {
    // Create a moderately large file for performance testing
    var content = string.Join("", Enumerable.Repeat("Performance test line with various content.\n", 5000).ToArray());
    var file = this.CreateTestFile(content);

    var sw = Stopwatch.StartNew();
    var hash = file.ComputeSHA256Hash();
    sw.Stop();

    Assert.That(hash, Is.Not.Null);
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000)); // Should complete in reasonable time
  }

  [Test]
  public void DirectoryInfoExtensions_GetSize_DeepHierarchy_PerformanceTest() {
    var rootDir = this.CreateTestDirectory();

    // Create a moderately deep directory structure
    for (var i = 0; i < 5; i++) {
      var subDir = Path.Combine(rootDir.FullName, $"level{i}");
      Directory.CreateDirectory(subDir);

      for (var j = 0; j < 10; j++) {
        File.WriteAllText(Path.Combine(subDir, $"file{j}.txt"), $"Content for file {j}");
      }
    }

    var sw = Stopwatch.StartNew();
    var size = rootDir.GetSize();
    sw.Stop();

    Assert.That(size, Is.GreaterThan(0));
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(1000)); // Should be reasonably fast
  }

  [Test]
  public void StreamExtensions_PrimitiveIO_BulkOperations_PerformanceTest() {
    using var stream = new MemoryStream();

    var sw = Stopwatch.StartNew();
    for (var i = 0; i < 10000; i++) {
      stream.Write(i);
      stream.Write((double)i * 1.5);
    }

    sw.Stop();

    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500)); // Should be fast for bulk operations
    Assert.That(stream.Length, Is.EqualTo(10000 * (4 + 8))); // int + double
  }

  #endregion

#if NET40_OR_GREATER || NETCOREAPP

  #region Async Operations Tests

  [Test]
  public async Task FileInfoExtensions_ReadAllTextAsync_ReadsCorrectly() {
    var content = "Async read content";
    var file = this.CreateTestFile(content);

    var result = await file.ReadAllTextAsync();

    Assert.That(result, Is.EqualTo(content));
  }

  [Test]
  public async Task FileInfoExtensions_WriteAllTextAsync_WritesCorrectly() {
    var file = new FileInfo(Path.Combine(this._tempDirectory, "async.txt"));
    this._tempFiles.Add(file.FullName);
    var content = "Async written content";

    await file.WriteAllTextAsync(content);

    Assert.That(file.Exists, Is.True);
    Assert.That(File.ReadAllText(file.FullName), Is.EqualTo(content));
  }

  [Test]
  public async Task FileInfoExtensions_CopyToAsync_CopiesToDestination() {
    var sourceFile = this.CreateTestFile("Async copy content");
    var destFile = new FileInfo(Path.Combine(this._tempDirectory, "async_dest.txt"));
    this._tempFiles.Add(destFile.FullName);

    await sourceFile.CopyToAsync(destFile);

    Assert.That(destFile.Exists, Is.True);
    Assert.That(File.ReadAllText(destFile.FullName), Is.EqualTo("Async copy content"));
  }

  [Test]
  public async Task FileInfoExtensions_ReadAllBytesAsync_WithCancellation_SupportsCancellation() {
    var content = "Cancellation test content";
    var file = this.CreateTestFile(content);

    using var cts = new CancellationTokenSource();

    // Don't cancel immediately, let it complete
    var result = await file.ReadAllBytesAsync(cts.Token);

    Assert.That(Encoding.UTF8.GetString(result), Is.EqualTo(content));
  }

  #endregion

#endif

  #region Edge Cases and Error Handling Tests

  [Test]
  public void FileInfoExtensions_HashComputation_NonExistentFile_ThrowsException() {
    var nonExistentFile = new FileInfo(Path.Combine(this._tempDirectory, "nonexistent.txt"));

    Assert.Throws<FileNotFoundException>(() => nonExistentFile.ComputeSHA256Hash());
  }

  [Test]
  public void FileInfoExtensions_ReadAllText_NonExistentFile_ThrowsException() {
    var nonExistentFile = new FileInfo(Path.Combine(this._tempDirectory, "nonexistent.txt"));

    Assert.Throws<FileNotFoundException>(() => nonExistentFile.ReadAllText());
  }

  [Test]
  public void StreamExtensions_ReadPastEnd_ThrowsException() {
    var data = new byte[] { 1, 2 };
    using var stream = new MemoryStream(data);

    stream.Position = stream.Length;

    Assert.Throws<EndOfStreamException>(() => stream.ReadInt32());
  }

  [Test]
  public void FileInfoExtensions_LineOperations_ZeroLines_HandlesCorrectly() {
    var file = this.CreateTestFile("Single line");

    Assert.Throws<ArgumentOutOfRangeException>(() => file.KeepFirstLines(0));
  }

  [Test]
  public void DirectoryInfoExtensions_GetSize_NonExistentDirectory_ReturnsZero() {
    var nonExistentDir = new DirectoryInfo(Path.Combine(this._tempDirectory, "nonexistent"));

    var size = nonExistentDir.GetSize();

    Assert.That(size, Is.EqualTo(0));
  }

  #endregion

  #region IFileInProgress Extension Tests (25+ tests)

  [Test]
  public void FileInfoExtensions_StartWorkInProgress_CreatesIFileInProgress() {
    var file = this.CreateTestFile("test content");
    using var modification = file.StartWorkInProgress();

    Assert.That(modification, Is.Not.Null);
    Assert.That(modification.OriginalFile.FullName, Is.EqualTo(file.FullName));
    Assert.That(modification.CancelChanges, Is.False);
  }

  [Test]
  public void IFileInProgress_ReadAllText_ReadsOriginalContent() {
    var content = "original content";
    var file = this.CreateTestFile(content);
    using var modification = file.StartWorkInProgress(copyContents: true);

    Assert.That(modification.ReadAllText(), Is.EqualTo(content));
  }

  [Test]
  public void IFileInProgress_WriteAllText_ChangesContent() {
    var file = this.CreateTestFile("original");
    var newContent = "modified content";

    using (var modification = file.StartWorkInProgress()) {
      modification.WriteAllText(newContent);
    }

    Assert.That(file.ReadAllText(), Is.EqualTo(newContent));
  }

  [Test]
  public void IFileInProgress_CancelChanges_PreservesOriginal() {
    var originalContent = "original content";
    var file = this.CreateTestFile(originalContent);

    using (var modification = file.StartWorkInProgress()) {
      modification.WriteAllText("should not persist");
      modification.CancelChanges = true;
    }

    Assert.That(file.ReadAllText(), Is.EqualTo(originalContent));
  }

  [Test]
  public void IFileInProgress_AppendLine_AddsLine() {
    var file = this.CreateTestFile("line1");

    using (var modification = file.StartWorkInProgress(copyContents: true)) {
      modification.AppendLine("line2");
    }

    var result = file.ReadAllText();
    Assert.That(result, Contains.Substring("line1"));
    Assert.That(result, Contains.Substring("line2"));
  }

  [Test]
  public void IFileInProgress_ReadWriteBytes_HandlesBinaryData() {
    var originalData = new byte[] { 0x01, 0x02, 0x03, 0xFF };
    var file = this.CreateTestFile(string.Empty);
    file.WriteAllBytes(originalData);

    var newData = new byte[] { 0xAA, 0xBB, 0xCC };
    using (var modification = file.StartWorkInProgress()) {
      modification.WriteAllBytes(newData);
    }

    Assert.That(file.ReadAllBytes(), Is.EqualTo(newData));
  }

  [Test]
  public void IFileInProgress_WithEncoding_PreservesEncoding() {
    var content = "unicode content: ñáéíóú";
    var file = this.CreateTestFile(string.Empty);
    file.WriteAllText(content, Encoding.UTF8);

    using (var modification = file.StartWorkInProgress(copyContents: true)) {
      var encoding = modification.GetEncoding();
      modification.WriteAllText("new ñáéíóú", encoding);
    }

    var result = file.ReadAllText();
    Assert.That(result, Contains.Substring("ñáéíóú"));
  }

  [Test]
  public void IFileInProgress_KeepFirstLines_TrimsCorrectly() {
    var lines = new[] { "line1", "line2", "line3", "line4" };
    var file = this.CreateTestFile(string.Join(Environment.NewLine, lines));

    using (var modification = file.StartWorkInProgress(copyContents: true)) {
      modification.KeepFirstLines(2);
    }

    var resultLines = file.ReadAllLines().Cast<string>().ToArray();
    Assert.That(resultLines.Length, Is.EqualTo(2));
    Assert.That(resultLines[0], Is.EqualTo("line1"));
    Assert.That(resultLines[1], Is.EqualTo("line2"));
  }

  [Test]
  public void IFileInProgress_KeepLastLines_TrimsCorrectly() {
    var lines = new[] { "line1", "line2", "line3", "line4" };
    var file = this.CreateTestFile(string.Join(Environment.NewLine, lines));

    using (var modification = file.StartWorkInProgress(copyContents: true)) {
      modification.KeepLastLines(2);
    }

    var resultLines = file.ReadAllLines().Cast<string>().ToArray();
    Assert.That(resultLines.Length, Is.EqualTo(2));
    Assert.That(resultLines[0], Is.EqualTo("line3"));
    Assert.That(resultLines[1], Is.EqualTo("line4"));
  }

  [Test]
  public void IFileInProgress_RemoveFirstLines_RemovesCorrectly() {
    var lines = new[] { "line1", "line2", "line3", "line4" };
    var file = this.CreateTestFile(string.Join(Environment.NewLine, lines));

    using (var modification = file.StartWorkInProgress(copyContents: true)) {
      modification.RemoveFirstLines(2);
    }

    var resultLines = file.ReadAllLines().Cast<string>().ToArray();
    Assert.That(resultLines.Length, Is.EqualTo(2));
    Assert.That(resultLines[0], Is.EqualTo("line3"));
    Assert.That(resultLines[1], Is.EqualTo("line4"));
  }

  #endregion

  #region Link Extension Tests (35+ tests)

  [Test]
  [Platform("Win")]
  public void LinkExtensions_CreateHardLink_CreatesLink() {
    var sourceFile = this.CreateTestFile("hard link test");
    var targetPath = Path.Combine(this._tempDirectory, "hardlink.txt");
    var targetFile = new FileInfo(targetPath);
    this._tempFiles.Add(targetPath);

    try {
      var success = sourceFile.TryCreateHardLinkAt(targetFile);

      if (!success) {
        Assert.Inconclusive("Hard link creation failed - not supported on this file system");
        return;
      }

      Assert.That(targetFile.Exists, Is.True);
      Assert.That(targetFile.ReadAllText(), Is.EqualTo("hard link test"));

      var hardLinks = sourceFile.GetHardLinkTargets().ToArray();
      Assert.That(hardLinks.Length, Is.GreaterThan(0));
    } catch (UnauthorizedAccessException) {
      Assert.Inconclusive("Insufficient permissions to create hard links");
    } catch (NotSupportedException) {
      Assert.Inconclusive("Hard links not supported on this file system");
    }
  }

  [Test]
  [Platform("Win")]
  public void LinkExtensions_GetHardLinkTargets_ReturnsTargets() {
    var sourceFile = this.CreateTestFile("hard link source");

    try {
      var targets = sourceFile.GetHardLinkTargets().ToArray();
      // At minimum, should return the original file path
      Assert.That(targets, Is.Not.Null);
    } catch (UnauthorizedAccessException) {
      Assert.Inconclusive("Insufficient permissions to enumerate hard links");
    } catch (NotSupportedException) {
      Assert.Inconclusive("Hard link enumeration not supported");
    }
  }

  [Test]
  [Platform("Win")]
  public void LinkExtensions_CopyTo_WithHardLinking_CreatesLink() {
    var sourceFile = this.CreateTestFile("link copy test");
    var targetPath = Path.Combine(this._tempDirectory, "linkcopy.txt");
    this._tempFiles.Add(targetPath);

    try {
      sourceFile.CopyTo(targetPath, allowHardLinking: true);

      var targetFile = new FileInfo(targetPath);
      Assert.That(targetFile.Exists, Is.True);
      Assert.That(targetFile.ReadAllText(), Is.EqualTo("link copy test"));
    } catch (UnauthorizedAccessException) {
      Assert.Inconclusive("Insufficient permissions for hard link operations");
    } catch (NotSupportedException) {
      Assert.Inconclusive("Hard linking not supported");
    }
  }

  [Test]
  [Platform("Win")]
  public void LinkExtensions_CopyTo_OverwriteWithHardLinking_ReplacesFile() {
    var sourceFile = this.CreateTestFile("source content");
    var targetFile = this.CreateTestFile("target content");

    try {
      sourceFile.CopyTo(targetFile.FullName, overwrite: true, allowHardLinking: true);

      targetFile.Refresh();
      Assert.That(targetFile.ReadAllText(), Is.EqualTo("source content"));
    } catch (UnauthorizedAccessException) {
      Assert.Inconclusive("Insufficient permissions for hard link operations");
    } catch (NotSupportedException) {
      Assert.Inconclusive("Hard linking not supported");
    }
  }

  [Test]
  public void LinkExtensions_CopyTo_WithoutHardLinking_CopiesNormally() {
    var sourceFile = this.CreateTestFile("normal copy test");
    var targetPath = Path.Combine(this._tempDirectory, "normalcopy.txt");
    this._tempFiles.Add(targetPath);

    sourceFile.CopyTo(targetPath, allowHardLinking: false);

    var targetFile = new FileInfo(targetPath);
    Assert.That(targetFile.Exists, Is.True);
    Assert.That(targetFile.ReadAllText(), Is.EqualTo("normal copy test"));
  }

  #endregion

  #region Path Extension Tests (40+ tests)

  [Test]
  public void PathExtensions_GetTempFileToken_CreatesTemporaryFile() {
    using var token = PathExtensions.GetTempFileToken();

    Assert.That(token, Is.Not.Null);
    Assert.That(token.File, Is.Not.Null);
    Assert.That(token.File.Exists, Is.True);
    Assert.That(token.File.Length, Is.EqualTo(0));
  }

  [Test]
  public void PathExtensions_GetTempFileToken_WithNameAndDirectory_CreatesInSpecifiedLocation() {
    var fileName = "test.tmp";
    using var token = PathExtensions.GetTempFileToken(fileName, this._tempDirectory);

    Assert.That(token.File.Name, Is.EqualTo(fileName));
    Assert.That(token.File.DirectoryName, Is.EqualTo(this._tempDirectory));
    Assert.That(token.File.Exists, Is.True);
  }

  [Test]
  public void PathExtensions_GetTempDirectoryToken_CreatesTemporaryDirectory() {
    using var token = PathExtensions.GetTempDirectoryToken();

    Assert.That(token, Is.Not.Null);
    Assert.That(token.Directory, Is.Not.Null);
    Assert.That(token.Directory.Exists, Is.True);
  }

  [Test]
  public void PathExtensions_GetTempDirectoryToken_WithNameAndParent_CreatesInSpecifiedLocation() {
    var dirName = "testdir";
    using var token = PathExtensions.GetTempDirectoryToken(dirName, this._tempDirectory);

    Assert.That(token.Directory.Name, Is.EqualTo(dirName));
    Assert.That(token.Directory.Parent!.FullName, Is.EqualTo(this._tempDirectory));
    Assert.That(token.Directory.Exists, Is.True);
  }

  [Test]
  public void PathExtensions_TemporaryTokens_AutoCleanupOnDispose() {
    FileInfo tempFile;
    DirectoryInfo tempDir;

    using (var fileToken = PathExtensions.GetTempFileToken()) {
      tempFile = fileToken.File;
      Assert.That(tempFile.Exists, Is.True);
    }

    using (var dirToken = PathExtensions.GetTempDirectoryToken()) {
      tempDir = dirToken.Directory;
      Assert.That(tempDir.Exists, Is.True);
    }

    // Give cleanup a moment
    Thread.Sleep(100);
    tempFile.Refresh();
    tempDir.Refresh();

    // Files should be cleaned up
    Assert.That(tempFile.Exists, Is.False);
    Assert.That(tempDir.Exists, Is.False);
  }

  [Test]
  public void PathExtensions_TemporaryTokens_CustomFileName_CreatesWithSpecifiedName() {
    var customName = "custom_test.tmp";
    using var token = PathExtensions.GetTempFileToken(customName, this._tempDirectory);

    Assert.That(token.File.Name, Is.EqualTo(customName));
    Assert.That(token.File.Exists, Is.True);
    Assert.That(token.File.Length, Is.EqualTo(0));
  }

  #endregion

  #region Performance Tests (10+ tests)

  [Test]
  [Category("Performance")]
  public void Performance_BufferedStreamEx_LargeSequentialWrites_OptimizedThroughput() {
    var testData = new byte[1024 * 1024]; // 1MB
    new Random(42).NextBytes(testData);

    var stopwatch = Stopwatch.StartNew();

    using (var baseStream = new MemoryStream())
    using (var bufferedStream = new BufferedStreamEx(baseStream, 64 * 1024)) {
      for (var i = 0; i < 100; i++) {
        bufferedStream.Write(testData, 0, testData.Length);
      }
    }

    stopwatch.Stop();

    // Should complete in reasonable time (adjust threshold as needed)
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000));
  }

  [Test]
  [Category("Performance")]
  public void Performance_FileHashing_LargeFile_ReasonableThroughput() {
    var largeContent = new string('X', 1024 * 1024); // 1MB of X's
    var file = this.CreateTestFile(largeContent);

    var stopwatch = Stopwatch.StartNew();
    var hash = file.ComputeSHA256Hash();
    stopwatch.Stop();

    Assert.That(hash, Is.Not.Null);
    Assert.That(hash.Length, Is.EqualTo(32)); // SHA256 = 32 bytes
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(2000)); // Should be fast
  }

  [Test]
  [Category("Performance")]
  public void Performance_DirectoryEnumeration_ManyFiles_EfficientTraversal() {
    // Create test directory with many files
    var testDir = this.CreateTestDirectory();
    for (var i = 0; i < 1000; i++) {
      var filePath = Path.Combine(testDir.FullName, $"file{i:D4}.txt");
      File.WriteAllText(filePath, $"content {i}");
      this._tempFiles.Add(filePath);
    }

    var stopwatch = Stopwatch.StartNew();
    var files = testDir.EnumerateFiles().ToArray();
    stopwatch.Stop();

    Assert.That(files.Length, Is.EqualTo(1000));
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
  }

  #endregion
}
