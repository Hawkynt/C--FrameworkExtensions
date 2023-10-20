#region (c)2010-2020 Hawkynt
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

namespace System.IO {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  partial class FastFileOperations {

    private const long KB = 1024;
    private const long MB = KB * 1024;
    private const long GB = MB * 1024;
    /// <summary>
    /// The default size of the buffer used for reading/writing file chunks.
    /// </summary>
    private const int _DEFAULT_BUFFER_SIZE = (int)(512 * KB);
    /// <summary>
    /// The default size of the buffer used for reading/writing large file chunks.
    /// </summary>
    private const int _DEFAULT_LARGE_BUFFER_SIZE = (int)(8 * MB);
    /// <summary>
    /// The maximum file size for using the small buffer size.
    /// </summary>
    private const long _MAX_SMALL_FILESIZE = 1 * GB;
    /// <summary>
    /// The maximum file size to preallocate for a file.
    /// Note: Larger values could tend to slow down the start of a copy operation if OS is too slow to allocate a continous block.
    /// </summary>
    private const long _MAX_PREALLOCATION_SIZE = 8 * GB;
    /// <summary>
    /// The maximum size for a file that allows OS caching. 
    /// Note: If the file is larger, RAM could be eaten too much and forces unneccessary swapping, thus slowing down the PC.
    ///       Without caching, copying of small files would be way slow, but copying large files isn't really influenced much.
    /// </summary>
    private const long _MAX_CACHED_SIZE = 2 * GB;
    /// <summary>
    /// The maximum number of chunks to read ahead.
    /// </summary>
    private const long _MAX_READ_AHEAD = 512 * MB;

    private const string _EX_USER_ABORT = "User aborted operation";
    private const string _EX_APP_UNLOAD = "Process was killed";

    private const string _EX_SOURCE_FILE_DOES_NOT_EXIST = "Source file \"{0}\" does not exist.";
    private const string _EX_TARGET_FILE_ALREADY_EXISTS = "Target file \"{0}\" already exists.";
    private const string _EX_SOURCE_DIRECTORY_DOES_NOT_EXIST = "Source directory \"{0}\" does not exist.";
    private const string _EX_TARGET_DIRECTORY_ALREADY_EXISTS = "Target directory \"{0}\" already exists.";
    private const string _EX_COULD_NOT_CREATE_TARGET_DIR = "Could not create target directory \"{0}\".";
  }
}
