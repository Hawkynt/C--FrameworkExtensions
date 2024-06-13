#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using System.Linq;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Guard;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.IO;

/// <summary>
/// Extensions for the DirectoryInfo type.
/// </summary>
public static partial class DirectoryInfoExtensions {

  #region nested types

  private readonly struct SubdirectoryInfo(DirectoryInfo directory, string relativeToRoot) {
    public void Deconstruct(out DirectoryInfo directory1, out string pathRelativeToRoot) {
      directory1 = directory;
      pathRelativeToRoot = relativeToRoot;
    }
  }

  private static class NativeMethods {
    [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int WNetGetConnection(
      [MarshalAs(UnmanagedType.LPTStr)] string localName,
      [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
      ref int length);
  }

  /// <summary>
  /// Determines the order in which sub-items will be returned.
  /// </summary>
  public enum RecursionMode {
    /// <summary>
    /// The toplevel items only (eg. /a, /b)
    /// </summary>
    ToplevelOnly,
    /// <summary>
    /// The shortest path first (eg. /a, /b, /a/c, /b/d)
    /// </summary>
    ShortestPathFirst,
    /// <summary>
    /// The deepest path first (eg. /a , /a/c, /b, /b/d)
    /// </summary>
    DeepestPathFirst,
  }
  #endregion

  /// <summary>
  /// Renames the directory represented by this <see cref="DirectoryInfo"/> instance to a new name in the same parent directory.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance to rename.</param>
  /// <param name="newName">The new name for the directory. This should not include the path, only the new directory name.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="newName"/> is null.</exception>
  /// <exception cref="ArgumentException">Thrown if <paramref name="newName"/> is empty or contains invalid characters.</exception>
  /// <exception cref="IOException">Thrown if a directory with the new name already exists, or if any other I/O error occurs during the renaming.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo directoryInfo = new DirectoryInfo("C:\\Example");
  /// directoryInfo.RenameTo("NewExample");
  /// Console.WriteLine("Directory renamed to: " + directoryInfo.FullName);
  /// </code>
  /// This example renames the directory 'Example' to 'NewExample'.
  /// </example>
  public static void RenameTo(this DirectoryInfo @this, string newName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(newName);
      
    if (newName.Contains(Path.DirectorySeparatorChar) || newName.Contains(Path.AltDirectorySeparatorChar) || newName.Contains(Path.VolumeSeparatorChar))
      throw new ArgumentException("No support for new directory structures", nameof(newName));

    // nothing to do on same name
    if (@this.Name == newName)
      return;

    var parent = @this.Parent;
    var fullTargetName = parent == null ? newName : Path.Combine(parent.FullName, newName);

    // only case has changed, so rename using a temporary intermediate
    if (string.Equals(@this.Name, newName, StringComparison.OrdinalIgnoreCase)) {
      var temporaryName = @this.FullName + "$";
      while (IO.Directory.Exists(temporaryName) || IO.File.Exists(temporaryName))
        temporaryName += "$";

      @this.MoveTo(temporaryName);
    }

    @this.MoveTo(fullTargetName);
  }

  /// <summary>
  /// Deletes all files and subdirectories within the directory represented by this <see cref="DirectoryInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance representing the directory to clear.</param>
  /// <exception cref="System.IO.IOException">Thrown if the directory does not exist or an error occurs when trying to delete the files or subdirectories.</exception>
  /// <exception cref="System.Security.SecurityException">Thrown if the caller does not have the required permission to delete files or directories.</exception>
  /// <exception cref="System.NotSupportedException">Thrown if the operation is attempted on a directory with a read-only file system.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo directoryInfo = new DirectoryInfo("C:\\MyDirectory");
  /// directoryInfo.Clear();
  /// Console.WriteLine("All files and subdirectories have been deleted.");
  /// </code>
  /// This example demonstrates how to delete all files and subdirectories within 'MyDirectory'.
  /// </example>
  /// <remarks>
  /// This method is destructive and irreversible. It will delete all contents within the directory but not the directory itself.
  /// Ensure that you have appropriate backups or safeguards before using this method to prevent data loss.
  /// </remarks>
  public static void Clear(this DirectoryInfo @this) {
    Against.ThisIsNull(@this);
    
    foreach (var item in @this.EnumerateFileSystemInfos())
      switch (item) {
        case FileInfo file:
          file.Delete();
          continue;
        case DirectoryInfo directory:
          directory.Delete(true);
          continue;
        default:
          throw new NotSupportedException("Unknown FileSystem item");
      }
  }

  /// <summary>
  /// Gets the size.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <returns>The number of bytes in this directory</returns>
  public static long GetSize(this DirectoryInfo @this) {
    Against.ThisIsNull(@this);
    
    // if less than 4 cores, use sequential approach
    if (Environment.ProcessorCount < 4)
      return @this.EnumerateFiles("*", SearchOption.AllDirectories).Select(f => f.Length).Sum();

    // otherwise, use MT approach
    long[] itemsLeftAndResult = [1L, 0L];
    using AutoResetEvent pushNotification = new(false);

    ExecuteAsync(@this);
    pushNotification.WaitOne();

    return itemsLeftAndResult[1];

    void WorkOnDirectory(DirectoryInfo directory) {
      try {
        foreach (var item in directory.EnumerateFileSystemInfos()) {
          switch (item) {
            case FileInfo file:
              Interlocked.Add(ref itemsLeftAndResult[1], file.Length);
              continue;
            case DirectoryInfo folder: {
              Interlocked.Increment(ref itemsLeftAndResult[0]);
              ExecuteAsync(folder);
              continue;
            }
            default:
              throw new NotSupportedException("Unknown FileSystemInfo item");
          }
        }
      } finally {
        if(Interlocked.Decrement(ref itemsLeftAndResult[0])<=0)
          // ReSharper disable once AccessToDisposedClosure
          pushNotification.Set();
      }
    }

    void ExecuteAsync(DirectoryInfo directory) {
      if (ThreadPool.QueueUserWorkItem(_ => WorkOnDirectory(directory)))
        return;

      var call = WorkOnDirectory;
      call.BeginInvoke(directory, call.EndInvoke, null);
    }
  }

  /// <summary>
  /// Given a path, returns the UNC path or the original. (No exceptions
  /// are raised by this function directly). For example, "P:\2008-02-29"
  /// might return: "\\networkserver\Shares\Photos\2008-02-09"
  /// </summary>
  /// <param name="this">The path to convert to a UNC Path</param>
  /// <returns>A UNC path. If a network drive letter is specified, the
  /// drive letter is converted to a UNC or network path. If the
  /// originalPath cannot be converted, it is returned unchanged.</returns>
  public static DirectoryInfo GetRealPath(this DirectoryInfo @this) {
    Against.ThisIsNull(@this);
    
    var originalPath = @this.FullName;

    // look for the {LETTER}: combination ...
    if (originalPath.Length < 2 || originalPath[1] != ':')
      return @this;

    // don't use char.IsLetter here - as that can be misleading
    // the only valid drive letters are a-z && A-Z.
    var c = originalPath[0];
    if (c is (< 'a' or > 'z') and (< 'A' or > 'Z'))
      return @this;

    StringBuilder sb = new(32768);
    var size = sb.Capacity;
    var error = NativeMethods.WNetGetConnection(originalPath[..2], sb, ref size);
    if (error != 0)
      return @this;

    var path = originalPath[@this.Root.FullName.Length..];
    return new(Path.Combine(sb.ToString().TrimEnd(), path));
  }

  /// <summary>
  /// Gets all sub-directories.
  /// </summary>
  /// <param name="this">This DirectoryInfo</param>
  /// <param name="searchOption">Whether to get all directories recursively or not.</param>
  /// <returns>An enumeration of DirectoryInfos</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<DirectoryInfo> GetDirectories(this DirectoryInfo @this, SearchOption searchOption) {
    Against.ThisIsNull(@this);
    
    return @this.GetDirectories("*.*", searchOption);
  }

  /// <summary>
  /// Enumerates the file system infos.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="mode">The recursion mode.</param>
  /// <param name="recursionFilter">The filter to use for recursing into sub-directories (Walks on <c>true</c>; otherwise, skips recursion).</param>
  /// <returns>
  /// The FileSystemInfos
  /// </returns>
  /// <exception cref="System.NotSupportedException">RecursionMode</exception>
  public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo @this, RecursionMode mode, Func<DirectoryInfo, bool> recursionFilter = null) {
    Against.ThisIsNull(@this);

    return mode switch {
      RecursionMode.ToplevelOnly => InvokeTopLevelOnly(@this),
      RecursionMode.ShortestPathFirst => InvokeShortestPathFirst(@this, recursionFilter),
      RecursionMode.DeepestPathFirst => InvokeDeepestPathFirst(@this, recursionFilter),
      _ => throw new NotSupportedException(nameof(RecursionMode))
    };

    static IEnumerable<FileSystemInfo> InvokeTopLevelOnly(DirectoryInfo @this) {
      foreach (var result in @this.EnumerateFileSystemInfos())
        yield return result;
    }

    static IEnumerable<FileSystemInfo> InvokeShortestPathFirst(DirectoryInfo @this, Func<DirectoryInfo, bool> recursionFilter) {
      LinkedList<DirectoryInfo> results = [];
      results.Enqueue(@this);
      while (results.Any()) {
        var result = results.Dequeue();
        foreach (var fsi in result.EnumerateFileSystemInfos()) {
          yield return fsi;
          if (fsi is not DirectoryInfo di)
            continue;

          if (recursionFilter == null || recursionFilter(di))
            results.Enqueue(di);
        }
      }
    }

    static IEnumerable<FileSystemInfo> InvokeDeepestPathFirst(DirectoryInfo @this, Func<DirectoryInfo, bool> recursionFilter) {
      LinkedList<DirectoryInfo> results = [];
      results.Push(@this);
      while (results.Any()) {
        var result = results.Pop();
        foreach (var fsi in result.EnumerateFileSystemInfos()) {
          yield return fsi;
          if (fsi is not DirectoryInfo di)
            continue;

          if (recursionFilter == null || recursionFilter(di))
            results.Push(di);
        }
      }
    }
  }

  /// <summary>
  /// Tries to set the last write time.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="lastWriteTimeUtc">The date&amp;time.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TrySetLastWriteTimeUtc(this DirectoryInfo @this, DateTime lastWriteTimeUtc) {
    Against.ThisIsNull(@this);

    @this.Refresh();

    if (!@this.Exists)
      return false;

    if (@this.LastWriteTimeUtc == lastWriteTimeUtc)
      return true;

    try {
      @this.LastWriteTimeUtc = lastWriteTimeUtc;
      return true;
    } catch (Exception) {
      return @this.LastWriteTimeUtc == lastWriteTimeUtc;
    }
  }

  /// <summary>
  /// Tries to set the creation time.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="creationTimeUtc">The date&amp;time.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TrySetCreationTimeUtc(this DirectoryInfo @this, DateTime creationTimeUtc) {
    Against.ThisIsNull(@this);

    @this.Refresh();

    if (!@this.Exists)
      return false;

    if (@this.CreationTimeUtc == creationTimeUtc)
      return true;

    try {
      @this.CreationTimeUtc = creationTimeUtc;
      return true;
    } catch (Exception) {
      return @this.CreationTimeUtc == creationTimeUtc;
    }
  }

  /// <summary>
  /// Tries to set the attributes.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="attributes">The attributes.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TrySetAttributes(this DirectoryInfo @this, FileAttributes attributes) {
    Against.ThisIsNull(@this);

    @this.Refresh();

    if (!@this.Exists)
      return false;

    if (@this.Attributes == attributes)
      return true;

    try {
      @this.Attributes = attributes;
      return true;
    } catch (Exception) {
      return @this.Attributes == attributes;
    }
  }

  public static bool TryCreate(this DirectoryInfo @this) => TryCreate(@this, false);

  /// <summary>
  /// Tries to create the given directory.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="recursive">if set to <c>true</c> recursively tries to create all subdirectories.</param>
  /// <returns>
  ///   <c>true</c> on success; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryCreate(this DirectoryInfo @this, bool recursive) {
    Against.ThisIsNull(@this);

    if (@this.Exists)
      return true;

    try {
      if (recursive) {
        IO.Directory.CreateDirectory(@this.FullName);
        @this.Refresh();
      } else
        @this.Create();
      return true;
    } catch (Exception) {
      @this.Refresh();
      return @this.Exists;
    }
  }

  /// <summary>
  /// Tries to delete the given directory.
  /// </summary>
  /// <param name="this">This DirectoyInfo.</param>
  /// <param name="recursive">if set to <c>true</c> deletes recursive.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryDelete(this DirectoryInfo @this, bool recursive = false) {
    Against.ThisIsNull(@this);

    if (!@this.Exists)
      return true;

    try {
      @this.Delete(recursive);
      @this.Refresh();
      return true;
    } catch (Exception) {
      @this.Refresh();
      return !@this.Exists;
    }
  }

  /// <summary>
  /// Checks whether the given directory does not exist.
  /// </summary>
  /// <param name="This">This DirectoryInfo.</param>
  /// <returns><c>true</c> if it does not exist; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool NotExists(this DirectoryInfo This) => !This.Exists;

  /// <summary>
  /// Gets a subdirectory under the current directory.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance representing the current directory.</param>
  /// <param name="subdirectory">The relative path to the subdirectory.</param>
  /// <returns>A <see cref="DirectoryInfo"/> instance pointing to the specified subdirectory.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="subdirectory"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo currentDir = new DirectoryInfo("C:\\CurrentDirectory");
  /// DirectoryInfo subDir = currentDir.Directory("SubDirectory");
  /// Console.WriteLine(subDir.FullName); // Outputs: "C:\\CurrentDirectory\\SubDirectory"
  /// </code>
  /// This example demonstrates how to get a subdirectory under the current directory.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static DirectoryInfo Directory(this DirectoryInfo @this, string subdirectory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(subdirectory);

    return new(Path.Combine(@this.FullName, subdirectory));
  }

  /// <summary>
  /// Gets a subdirectory under the current directory.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance representing the current directory.</param>
  /// <param name="subdirectories">An array of relative paths to the subdirectory.</param>
  /// <returns>A <see cref="DirectoryInfo"/> instance pointing to the specified subdirectory.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="subdirectories"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown if <paramref name="subdirectories"/> is empty.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo currentDir = new DirectoryInfo("C:\\CurrentDirectory");
  /// DirectoryInfo subDir = currentDir.Directory("SubDir1", "SubDir2");
  /// Console.WriteLine(subDir.FullName); // Outputs: "C:\\CurrentDirectory\\SubDir1\\SubDir2"
  /// </code>
  /// This example demonstrates how to get a nested subdirectory under the current directory.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static DirectoryInfo Directory(this DirectoryInfo @this, params string[] subdirectories) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(subdirectories);

#if SUPPORTS_PATH_COMBINE_ARRAYS
    return new(Path.Combine(new[] { @this.FullName }.Concat(subdirectories).ToArray()));
#else
    return new(string.Join(Path.DirectorySeparatorChar + string.Empty, new[] { @this.FullName }.Concat(subdirectories).ToArray()));
#endif
  }

  /// <summary>
  /// Gets a subdirectory under the current directory, querying the filesystem to return the exact case.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance representing the current directory.</param>
  /// <param name="ignoreCase">If <see langword="true"/>, the case will be ignored; otherwise, the filesystem will be queried for the exact casing.</param>
  /// <param name="subdirectories">An array of relative paths to the subdirectory.</param>
  /// <returns>A <see cref="DirectoryInfo"/> instance pointing to the specified subdirectory.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="subdirectories"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown if <paramref name="subdirectories"/> is empty.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo currentDir = new DirectoryInfo("C:\\CurrentDirectory");
  /// DirectoryInfo subDir = currentDir.Directory(false, "SubDir1", "SubDir2");
  /// Console.WriteLine(subDir.FullName); // Outputs the exact case path if it exists
  /// </code>
  /// This example demonstrates how to get a nested subdirectory under the current directory with exact casing.
  /// </example>
  /// <remarks>
  /// This method queries the filesystem to find the exact casing of the directory names which could case heavy I/O.
  /// </remarks>
  public static DirectoryInfo Directory(this DirectoryInfo @this, bool ignoreCase, params string[] subdirectories) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(subdirectories);

    var comparisonMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    
    var result = @this;
    foreach (var subdir in subdirectories) {
      var current = result.Directory(subdir);
      if (result.Exists) {
        foreach (var existing in result.EnumerateDirectories()) {
          if (!string.Equals(existing.Name, subdir, comparisonMode))
            continue;

          current = existing;
          break;
        }
      }

      result = current;
    }

    return result;
  }

  /// <summary>
  /// Gets a file under the current directory.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance representing the current directory.</param>
  /// <param name="filePath">The relative path to the file.</param>
  /// <returns>A <see cref="FileInfo"/> instance pointing to the specified file path.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> is empty.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo currentDir = new DirectoryInfo("C:\\CurrentDirectory");
  /// FileInfo file = currentDir.File("example.txt");
  /// Console.WriteLine(file.FullName); // Outputs: "C:\\CurrentDirectory\\example.txt"
  /// </code>
  /// This example demonstrates how to get a file under the current directory.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static FileInfo File(this DirectoryInfo @this, string filePath) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(filePath);

    return new(Path.Combine(@this.FullName, filePath));
  }

  /// <summary>
  /// Gets a file under the current directory.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance representing the current directory.</param>
  /// <param name="filePath">An array of relative paths to the file.</param>
  /// <returns>A <see cref="FileInfo"/> instance pointing to the specified file path.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> is empty.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo currentDir = new DirectoryInfo("C:\\CurrentDirectory");
  /// FileInfo file = currentDir.File("SubDir", "example.txt");
  /// Console.WriteLine(file.FullName); // Outputs: "C:\\CurrentDirectory\\SubDir\\example.txt"
  /// </code>
  /// This example demonstrates how to get a nested file under the current directory.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static FileInfo File(this DirectoryInfo @this, params string[] filePath) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(filePath);

#if SUPPORTS_PATH_COMBINE_ARRAYS
    return new(Path.Combine(new[] { @this.FullName }.Concat(filePath).ToArray()));
#else
    return new(string.Join(Path.DirectorySeparatorChar + string.Empty, new[] { @this.FullName }.Concat(filePath).ToArray()));
#endif
  }

  /// <summary>
  /// Gets a file under the current directory, querying the filesystem to return the exact case.
  /// </summary>
  /// <param name="this">The <see cref="DirectoryInfo"/> instance representing the current directory.</param>
  /// <param name="ignoreCase">If <see langword="true"/>, the case will be ignored; otherwise, the filesystem will be queried for the exact casing.</param>
  /// <param name="filePath">An array of relative paths to the file.</param>
  /// <returns>A <see cref="FileInfo"/> instance pointing to the specified file path.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> is empty.</exception>
  /// <example>
  /// <code>
  /// DirectoryInfo currentDir = new DirectoryInfo("C:\\CurrentDirectory");
  /// FileInfo file = currentDir.File(false, "SubDir", "example.txt");
  /// Console.WriteLine(file.FullName); // Outputs the exact case path if it exists
  /// </code>
  /// This example demonstrates how to get a nested file under the current directory with exact casing.
  /// </example>
  /// <remarks>
  /// This method queries the filesystem to find the exact casing of the directory/file names which could case heavy I/O.
  /// </remarks>
  public static FileInfo File(this DirectoryInfo @this, bool ignoreCase, params string[] filePath) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(filePath);

    var comparisonMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    var parent = @this;
    for (var i = 0; i < filePath.Length - 1; ++i) {
      var subdir = filePath[i];
      var current = parent.Directory(subdir);
      if (parent.Exists) {
        foreach (var existing in parent.EnumerateDirectories()) {
          if (!string.Equals(existing.Name, subdir, comparisonMode))
            continue;

          current = existing;
          break;
        }
      }

      parent = current;
    }

    var fileName = filePath[^1];
    var result = parent.File(fileName);
    if (!parent.Exists)
      return result;

    foreach (var existing in parent.EnumerateFiles()) {
      if (!string.Equals(existing.Name, fileName, comparisonMode))
        continue;

      result = existing;
      break;
    }

    return result;
  }

  /// <summary>
  /// Determines whether the specified subdirectory exists.
  /// </summary>
  /// <param name="This">This DirectoryInfo.</param>
  /// <param name="searchPattern">The search pattern.</param>
  /// <param name="searchOption">The search option.</param>
  /// <returns><c>true</c> if at least one match was found; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool HasDirectory(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) => This.EnumerateDirectories(searchPattern, searchOption).Any();

  /// <summary>
  /// Determines whether the specified file exists.
  /// </summary>
  /// <param name="This">This DirectoryInfo.</param>
  /// <param name="searchPattern">The search pattern.</param>
  /// <param name="searchOption">The search option.</param>
  /// <returns><c>true</c> if at least one match was found; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool HasFile(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) => This.EnumerateFiles(searchPattern, searchOption).Any();

  /// <summary>
  /// Creates the directory recursively.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  public static void CreateDirectory(this DirectoryInfo @this) {
    Against.ThisIsNull(@this);

    if (@this.Parent != null && !@this.Exists)
      CreateDirectory(@this.Parent);

    if (@this.Exists)
      return;

    @this.Create();
    @this.Refresh();
  }

  /// <summary>
  /// Copies the specified directorys' contents to the target directory.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="target">The target directory to place files.</param>
  public static void CopyTo(this DirectoryInfo @this, DirectoryInfo target) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);

    Stack<SubdirectoryInfo> stack = new();
    stack.Push(new(@this, "."));
    while (stack.Count > 0) {
      var (directory, relativePath) = stack.Pop();
      var targetPath = Path.Combine(target.FullName, relativePath);

      // create directory if it does not exist
      if (!IO.Directory.Exists(targetPath))
        IO.Directory.CreateDirectory(targetPath);

      foreach (var fileSystemInfo in directory.GetFileSystemInfos()) {
        if (fileSystemInfo is FileInfo fileInfo) {
          fileInfo.CopyTo(Path.Combine(targetPath, fileInfo.Name));
          continue;
        }

        var directoryInfo = fileSystemInfo as DirectoryInfo;
        stack.Push(new(directoryInfo, Path.Combine(relativePath, directoryInfo.Name)));
      }
    }
  }

  /// <summary>
  /// Gets the or adds a subdirectory.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="name">The name.</param>
  /// <returns></returns>
  public static DirectoryInfo GetOrAddDirectory(this DirectoryInfo @this, string name) {
    Against.ThisIsNull(@this);

    var fullPath = Path.Combine(@this.FullName, name);
    return IO.Directory.Exists(fullPath) ? new(fullPath) : @this.CreateSubdirectory(name);
  }

  /// <summary>
  /// Determines whether the specified directory contains file.
  /// </summary>
  /// <param name="This">This DirectoryInfo.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="option">The option.</param>
  /// <returns><c>true</c> if there is a matching file; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool ContainsFile(this DirectoryInfo This, string fileName, SearchOption option = SearchOption.TopDirectoryOnly) => This.EnumerateFiles(fileName, option).Any();

  /// <summary>
  /// Determines whether the specified directory contains directory.
  /// </summary>
  /// <param name="This">This DirectoryInfo.</param>
  /// <param name="directoryName">Name of the directory.</param>
  /// <param name="option">The option.</param>
  /// <returns>
  ///   <c>true</c> if there is a matching directory; otherwise, <c>false</c>.
  /// </returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool ContainsDirectory(this DirectoryInfo This, string directoryName, SearchOption option = SearchOption.TopDirectoryOnly) => This.EnumerateDirectories(directoryName, option).Any();

  /// <summary>
  /// Checks whether the given directory exists and contains at least one file.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="fileMask">The file mask; defaults to '*.*'.</param>
  /// <returns><c>true</c> if it exists and has matching files; otherwise, <c>false</c>.</returns>
  public static bool ExistsAndHasFiles(this DirectoryInfo @this, string fileMask = "*.*") {
    Against.ThisIsNull(@this);
    
    if (!@this.Exists)
      return false;

    try {
      if (@this.HasFile(fileMask, SearchOption.AllDirectories))
        return true;

    } catch (IOException) {
      return false;
    }
    return false;
  }

  /// <summary>
  /// Tries to create a temporary file.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="extension">The extension; defaults to '.tmp'.</param>
  /// <returns>A temporary file</returns>
  public static FileInfo GetTempFile(this DirectoryInfo @this, string extension = null) {
    Against.ThisIsNull(@this);

    extension = extension == null ? ".tmp" : '.' + extension.TrimStart('.');

    static string Generator(Random random,string ext) {
      const int LENGTH = 4;
      const string PREFIX = "tmp";
      StringBuilder result = new(16);
      result.Append(PREFIX);
      for (var i = 0; i < LENGTH; ++i)
        result.Append(random.Next(0, 16).ToString("X"));

      result.Append(ext);
      return result.ToString();
    }

    Random random = new();

    while (true) {
      var result = @this.TryCreateFile(Generator(random, extension), FileAttributes.NotContentIndexed | FileAttributes.Temporary);
      if (result != null)
        return result;
    }

  }

  /// <summary>
  /// Tries to create file.
  /// </summary>
  /// <param name="this">This DirectoryInfo.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="attributes">The attributes; defaults to FileAttributes.Normal.</param>
  /// <returns>A FileInfo instance or <c>null</c> on error.</returns>
  public static FileInfo TryCreateFile(this DirectoryInfo @this, string fileName, FileAttributes attributes = FileAttributes.Normal) {
    Against.ThisIsNull(@this);

    var fullFileName = Path.Combine(@this.FullName, fileName);
    if (IO.File.Exists(fullFileName))
      return null;

    try {
      var fileHandle = IO.File.Open(fullFileName, FileMode.CreateNew, FileAccess.Write);
      fileHandle.Close();
      IO.File.SetAttributes(fullFileName, attributes);
      return new(fullFileName);
    } catch (UnauthorizedAccessException) {

      // in case multiple threads try to create the same file, this gets fired
      return null;
    } catch (IOException) {

      // file already exists
      return null;
    }
  }

  /// <summary>Safely enumerates through a directory even if some entries throw exceptions</summary>
  /// <param name="this">This <see cref="DirectoryInfo"/></param>
  public static IEnumerable<DirectoryInfo> SafelyEnumerateDirectories(this DirectoryInfo @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);
      
    static IEnumerable<DirectoryInfo> Invoke(DirectoryInfo @this) {
      IEnumerator<DirectoryInfo> enumerator = null;
      try {
        enumerator = @this.EnumerateDirectories().GetEnumerator();
      } catch {
        ;
      }

      if (enumerator == null)
        yield break;

      for (;;) {
        try {
          if (!enumerator.MoveNext())
            break;
        } catch {
          continue;
        }

        DirectoryInfo result = null;
        try {
          result = enumerator.Current;
        } catch {
          ;
        }

        if (result != null)
          yield return result;
      }

      enumerator.Dispose();
    }
  }

  /// <summary>Safely enumerates through a directory even if some entries throw exceptions</summary>
  /// <param name="this">This <see cref="DirectoryInfo"/></param>
  public static IEnumerable<FileInfo> SafelyEnumerateFiles(this DirectoryInfo @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);
      
    static IEnumerable<FileInfo> Invoke(DirectoryInfo @this) {
      IEnumerator<FileInfo> enumerator = null;
      try {
        enumerator = @this.EnumerateFiles().GetEnumerator();
      } catch {
        ;
      }

      if (enumerator == null)
        yield break;

      for (;;) {
        try {
          if (!enumerator.MoveNext())
            break;
        } catch {
          continue;
        }

        FileInfo result = null;
        try {
          result = enumerator.Current;
        } catch {
          ;
        }

        if (result != null)
          yield return result;
      }

      enumerator.Dispose();
    }
  }

}