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

#if !SUPPORTS_MOVETO_OVERWRITE

using System.Reflection;
using System.Threading;

namespace System.IO;

public static partial class FileInfoPolyfills {
  /// <summary>
  ///   Moves the specified <see cref="FileInfo" /> instance to a new location with an option to overwrite an existing file,
  ///   using a default timeout period for retrying the deletion of the source file if it is locked or cannot be deleted
  ///   immediately.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to move.</param>
  /// <param name="destFileName">The path to the destination file. This cannot be a directory.</param>
  /// <param name="overwrite">
  ///   A <see langword="bool" /> indicating whether to overwrite an existing file at the destination.
  ///   If <see langword="true" />, the file will be overwritten; if <see langword="false" />, an <see cref="IOException" />
  ///   will be thrown
  ///   if a file with the same name already exists at the destination.
  /// </param>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// string destinationPath = @"D:\destination\example.txt";
  /// sourceFile.MoveTo(destinationPath, true);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  ///   This example demonstrates moving a file from one location to another, with the option to overwrite an existing file
  ///   at the destination.
  /// </example>
  public static void MoveTo(this FileInfo @this, string destFileName, bool overwrite) {
    destFileName = Path.GetFullPath(destFileName);
    if (Directory.Exists(destFileName))
      throw new UnauthorizedAccessException("Target directory already exists");

    if (!File.Exists(destFileName)) {
      @this.MoveTo(destFileName);
      return;
    }

    if (string.Equals(destFileName, @this.FullName,StringComparison.OrdinalIgnoreCase))
      return;

    const int ERROR_FILE_EXISTS = -2147024816;
    if (!overwrite)
      throw new IOException("Target already exists", ERROR_FILE_EXISTS);

    if (File.GetAttributes(destFileName).HasFlag(FileAttributes.ReadOnly))
      throw new UnauthorizedAccessException("Target file is read-only");

    try {
      using (new FileStream(destFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
      }
    } catch (IOException) {
      throw new UnauthorizedAccessException("Target file is locked by another process");
    }

    var sourceDirectoryPath = @this.Directory?.FullName;
    var targetDirectoryPath = Path.GetDirectoryName(destFileName);

    var sourceFileName = @this.FullName;
    
    // we gonna do the following to get an "atomic" file-operation:
    // create a temp in source calling stemp (we need this for rollbacks)
    // copy source to stemp
    // create a temp in target calling ttemp (we want to make sure that slow I/O doesn't kill our routine)
    // move source to ttemp (now we have neither source nor target, but we gonna fix this)
    // move stemp to source (now we have our source back in case something goes wrong later)
    // create a temp in target calling ttemp2 (we need this to rollback changes in the existing target file)
    // move target to ttemp2 (now we have the original target no longer, but we could restore from our temp if needed)
    // move ttemp to target (now this hopefully goes quick because we already are inside the target directory)
    // delete ttemp2 (we no longer need the original target content)
    // delete source (we no longer need the source)
    
    string sourceCopyPath = null;
    try {
      sourceCopyPath = CreateTempFile(sourceDirectoryPath, "temp");
      // Race-Case: source exists, empty temp file in srcdir -> removed by finally
      
      @this.CopyTo(sourceCopyPath, true);
      // Race-Case: source exists, temp file filled in srcdir -> removed by finally

      string targetCopyPath = null;
      try {
        targetCopyPath = CreateTempFile(targetDirectoryPath, "temp");
        // Race-Case: temp file filled in srcdir, empty temp file in tgtdir -> removed by finally

        Thread.BeginCriticalRegion();
        TryFileDelete(targetCopyPath);
        @this.MoveTo(targetCopyPath);
        File.Move(sourceCopyPath,sourceFileName);
        sourceCopyPath = null;
        Thread.EndCriticalRegion();
        // Race-Case: source exists, temp file filled in tgtdir -> removed by finally

        string targetCopyPath2 = null;
        try {
          targetCopyPath2 = CreateTempFile(targetDirectoryPath, "temp");
          // Race-Case: source exists, temp file filled in tgtdir, empty temp file in tgtdir -> removed by finally

          Thread.BeginCriticalRegion();
          
          // here we copy the original tgt file content to a temp location
          TryFileDelete(targetCopyPath2);
          File.Move(destFileName,targetCopyPath2);

          try {
            @this.MoveTo(destFileName);
          } catch {
            File.Move(targetCopyPath2,destFileName);
            targetCopyPath2 = null;
          }
          Thread.EndCriticalRegion();
          
          // worst case that could happen from here on: target and source existing at the same time -> we don't care
          TryFileDelete(sourceFileName);

        } finally {
          TryFileDelete(targetCopyPath2);
        }
      } finally {
        TryFileDelete(targetCopyPath);
      }
    } finally {
      TryFileDelete(sourceCopyPath);
    }

    return;

    static void TryFileDelete(string fileName) {
      if (fileName == null || !File.Exists(fileName))
        return;
      try {
        File.Delete(fileName);
      } catch {
        ;
      }
    }

    static string CreateTempFile(string directory, string prefix) {
      for (;;) {
        var result = Path.Combine(directory, $"{prefix}_{Path.GetRandomFileName()}");
        try {
          using (new FileStream(result, FileMode.CreateNew)) { }
          return result;
        } catch (IOException e) when (GetHResult(e) == ERROR_FILE_EXISTS) {
        }
      }
    }

#if NET20_OR_GREATER && !NET45_OR_GREATER
    static int GetHResult(IOException e) {
      if (e == null)
        return 0;

      var property = typeof(IOException).GetProperty("HResult", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
      return (int)property.GetValue(e,null);
    }
#else
    static int GetHResult(IOException e) => e?.HResult ?? 0;
#endif
  }
}

#endif
