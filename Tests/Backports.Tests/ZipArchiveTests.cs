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
using System.Text;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ZipArchive")]
public class ZipArchiveTests {

  #region ZipArchive Creation

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_CreateMode_CreatesEmptyArchive() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("create_empty");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      Assert.That(archive.Mode, Is.EqualTo(ZipArchiveMode.Create));
    }

    Assert.That(File.Exists(zipPath), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_CreateEntry_AddsEntryToArchive() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("with_entry");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("test.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Hello, World!");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(1));
      Assert.That(archive.Entries[0].FullName, Is.EqualTo("test.txt"));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_CreateEntryWithCompressionLevel_RespectsLevel() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("with_compression");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("test.txt", CompressionLevel.Fastest);
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Hello, World!");
    }

    Assert.That(File.Exists(zipPath), Is.True);
    Assert.That(new FileInfo(zipPath).Length, Is.GreaterThan(0));
  }

  #endregion

  #region ZipArchive Reading

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_ReadMode_ReadsExistingArchive() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("read_test");
    const string expectedContent = "Test content for reading";

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("readme.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write(expectedContent);
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      Assert.That(archive.Mode, Is.EqualTo(ZipArchiveMode.Read));
      Assert.That(archive.Entries.Count, Is.EqualTo(1));

      var entry = archive.GetEntry("readme.txt");
      Assert.That(entry, Is.Not.Null);

      using (var reader = new StreamReader(entry.Open())) {
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo(expectedContent));
      }
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_GetEntry_ReturnsNullForNonexistent() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("getentry_null");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntry("exists.txt");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry("doesnotexist.txt");
      Assert.That(entry, Is.Null);
    }
  }

  #endregion

  #region ZipArchive Update Mode

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_UpdateMode_CanAddAndReadEntries() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("update_test");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("original.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Original content");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.ReadWrite))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Update)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(1));

      var newEntry = archive.CreateEntry("added.txt");
      using (var writer = new StreamWriter(newEntry.Open()))
        writer.Write("Added content");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(2));
    }
  }

  #endregion

  #region ZipArchiveEntry Properties

  [Test]
  [Category("HappyPath")]
  public void ZipArchiveEntry_Name_ReturnsFileNameOnly() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("entry_name");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("folder/subfolder/file.txt");
      Assert.That(entry.Name, Is.EqualTo("file.txt"));
      Assert.That(entry.FullName, Is.EqualTo("folder/subfolder/file.txt"));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ZipArchiveEntry_Length_ReturnsUncompressedSize() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("entry_length");
    var content = new string('A', 1000);

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("test.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write(content);
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry("test.txt");
      Assert.That(entry.Length, Is.EqualTo(content.Length));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ZipArchiveEntry_LastWriteTime_IsPreserved() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("entry_time");
    // Use a date-time without timezone complications to avoid DST issues
    // Simply verify that the DateTime portion (ignoring offset) is preserved
    var testDateTime = new DateTime(2020, 6, 15, 10, 30, 0, DateTimeKind.Unspecified);
    var expectedTime = new DateTimeOffset(testDateTime, TimeSpan.Zero);

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("test.txt");
      entry.LastWriteTime = expectedTime;
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Content");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry("test.txt");
      // DOS time has 2-second precision and doesn't store timezone
      // Different implementations may interpret timezone differently, so we just verify
      // that the date portion (year, month, day) matches
      Assert.That(entry.LastWriteTime.Year, Is.EqualTo(testDateTime.Year));
      Assert.That(entry.LastWriteTime.Month, Is.EqualTo(testDateTime.Month));
      Assert.That(entry.LastWriteTime.Day, Is.EqualTo(testDateTime.Day));
    }
  }

  #endregion

  #region ZipArchiveEntry Delete

  [Test]
  [Category("HappyPath")]
  public void ZipArchiveEntry_Delete_RemovesEntry() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("entry_delete");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntry("keep.txt");
      archive.CreateEntry("delete.txt");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.ReadWrite))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Update)) {
      var entryToDelete = archive.GetEntry("delete.txt");
      entryToDelete.Delete();
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(1));
      Assert.That(archive.GetEntry("keep.txt"), Is.Not.Null);
      Assert.That(archive.GetEntry("delete.txt"), Is.Null);
    }
  }

  [Test]
  [Category("Exception")]
  public void ZipArchiveEntry_Delete_ThrowsInReadMode() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("delete_readonly");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntry("test.txt");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry("test.txt");
      Assert.Throws<NotSupportedException>(() => entry.Delete());
    }
  }

  #endregion

  #region Binary Content

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_BinaryContent_IsPreserved() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("binary_content");
    var binaryData = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD };

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("binary.bin");
      using (var stream = entry.Open())
        stream.Write(binaryData, 0, binaryData.Length);
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry("binary.bin");
      using (var stream = entry.Open()) {
        var buffer = new byte[binaryData.Length];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);
        Assert.That(bytesRead, Is.EqualTo(binaryData.Length));
        Assert.That(buffer, Is.EqualTo(binaryData));
      }
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_LargeFile_CompressesAndDecompresses() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("large_file");

    // Create a 100KB file with repeating pattern (should compress well)
    var largeData = new byte[100 * 1024];
    for (var i = 0; i < largeData.Length; ++i)
      largeData[i] = (byte)(i % 256);

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry("large.bin");
      using (var stream = entry.Open())
        stream.Write(largeData, 0, largeData.Length);
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry("large.bin");
      Assert.That(entry.Length, Is.EqualTo(largeData.Length));
      Assert.That(entry.CompressedLength, Is.LessThan(largeData.Length)); // Should compress

      using (var stream = entry.Open())
      using (var ms = new MemoryStream()) {
        var buffer = new byte[4096];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
          ms.Write(buffer, 0, read);

        Assert.That(ms.ToArray(), Is.EqualTo(largeData));
      }
    }
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void ZipArchive_EmptyEntry_CreatesZeroLengthEntry() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("empty_entry");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntry("empty.txt");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry("empty.txt");
      Assert.That(entry.Length, Is.EqualTo(0));
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void ZipArchive_UnicodeEntryName_IsPreserved() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("unicode_name");
    const string unicodeName = "日本語ファイル.txt";

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      var entry = archive.CreateEntry(unicodeName);
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Unicode content: 你好世界");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      var entry = archive.GetEntry(unicodeName);
      Assert.That(entry, Is.Not.Null);
      Assert.That(entry.FullName, Is.EqualTo(unicodeName));
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void ZipArchive_MultipleEntries_AllPreserved() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("multiple_entries");
    const int entryCount = 50;

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      for (var i = 0; i < entryCount; ++i) {
        var entry = archive.CreateEntry($"file{i:D3}.txt");
        using (var writer = new StreamWriter(entry.Open()))
          writer.Write($"Content of file {i}");
      }
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(entryCount));
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void ZipArchive_DirectoryEntry_CreatesEmptyEntry() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("directory_entry");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntry("mydir/");
      var fileEntry = archive.CreateEntry("mydir/file.txt");
      using (var writer = new StreamWriter(fileEntry.Open()))
        writer.Write("File in directory");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(2));
    }
  }

  #endregion

  #region Exception Cases

  [Test]
  [Category("Exception")]
  public void ZipArchive_NullStream_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => new ZipArchive(null));
  }

  [Test]
  [Category("Exception")]
  public void ZipArchive_CreateEntry_InReadMode_ThrowsNotSupportedException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("create_in_read");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntry("test.txt");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read)) {
      Assert.Throws<NotSupportedException>(() => archive.CreateEntry("new.txt"));
    }
  }

  [Test]
  [Category("Exception")]
  public void ZipArchive_GetEntry_InCreateMode_ThrowsNotSupportedException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("get_in_create");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      Assert.Throws<NotSupportedException>(() => archive.GetEntry("test.txt"));
    }
  }

  [Test]
  [Category("Exception")]
  public void ZipArchive_CreateEntry_NullEntryName_ThrowsArgumentNullException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("null_entryname");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      Assert.Throws<ArgumentNullException>(() => archive.CreateEntry(null));
    }
  }

  [Test]
  [Category("Exception")]
  public void ZipArchive_Disposed_ThrowsObjectDisposedException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("disposed");

    ZipArchive archive;
    // Use ReadWrite access because the official implementation may need to seek during disposal
    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite)) {
      archive = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: true);
      archive.Dispose();
    }

    // After dispose, trying to create an entry should throw ObjectDisposedException
    Assert.Throws<ObjectDisposedException>(() => archive.CreateEntry("test.txt"));
  }

  [Test]
  [Category("Exception")]
  public void ZipArchive_ReadMode_NonReadableStream_ThrowsArgumentException() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("non_readable");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
      archive.CreateEntry("test.txt");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Write)) {
      Assert.Throws<ArgumentException>(() => new ZipArchive(fs, ZipArchiveMode.Read));
    }
  }

  #endregion

  #region Encoding

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_CustomEncoding_IsRespected() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("custom_encoding");
    var encoding = Encoding.UTF8;

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false, encoding)) {
      var entry = archive.CreateEntry("test.txt");
      using (var writer = new StreamWriter(entry.Open()))
        writer.Write("Test content");
    }

    using (var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false, encoding)) {
      Assert.That(archive.Entries.Count, Is.EqualTo(1));
    }
  }

  #endregion

  #region LeaveOpen

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_LeaveOpenTrue_DoesNotDisposeStream() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("leave_open");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write)) {
      using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: true)) {
        var entry = archive.CreateEntry("test.txt");
        using (var writer = new StreamWriter(entry.Open()))
          writer.Write("Content");
      }

      // Stream should still be open after archive is disposed
      Assert.That(fs.CanWrite, Is.True);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ZipArchive_LeaveOpenFalse_DisposesStream() {
    using var helper = new ZipTestHelpers();
    var zipPath = helper.GetZipPath("dispose_stream");
    FileStream fs = null;

    try {
      fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write);
      using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false)) {
        var entry = archive.CreateEntry("test.txt");
        using (var writer = new StreamWriter(entry.Open()))
          writer.Write("Content");
      }

      // Stream should be closed after archive is disposed
      // Verify that CanWrite is false or that attempting to write throws
      var streamIsDisposed = !fs.CanWrite;
      if (!streamIsDisposed) {
        try {
          fs.WriteByte(0);
        } catch (ObjectDisposedException) {
          streamIsDisposed = true;
        } catch (NotSupportedException) {
          streamIsDisposed = true;
        } catch (IOException) {
          // Some implementations may throw IOException on closed stream
          streamIsDisposed = true;
        }
      }
      Assert.That(streamIsDisposed, Is.True, "Stream should be disposed when leaveOpen is false");
      fs = null; // Already disposed
    } finally {
      fs?.Dispose();
    }
  }

  #endregion

}
