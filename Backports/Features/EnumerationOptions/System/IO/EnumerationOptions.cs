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

// EnumerationOptions was added in .NET Core 2.1
#if !SUPPORTS_ENUMERATION_OPTIONS

namespace System.IO;

/// <summary>
/// Provides file and directory enumeration options.
/// </summary>
public class EnumerationOptions {

  /// <summary>
  /// Gets or sets a value that indicates whether to recurse into subdirectories while enumerating.
  /// The default is <see langword="false"/>.
  /// </summary>
  public bool RecurseSubdirectories { get; set; }

  /// <summary>
  /// Gets or sets a value that indicates whether to skip files or directories when access is denied
  /// (for example, <see cref="UnauthorizedAccessException"/> or <see cref="Security.SecurityException"/>).
  /// The default is <see langword="true"/>.
  /// </summary>
  public bool IgnoreInaccessible { get; set; } = true;

  /// <summary>
  /// Gets or sets the attributes to skip. The default is <c>FileAttributes.Hidden | FileAttributes.System</c>.
  /// </summary>
  public FileAttributes AttributesToSkip { get; set; } = FileAttributes.Hidden | FileAttributes.System;

  /// <summary>
  /// Gets or sets the match type. The default is <see cref="IO.MatchType.Simple"/>.
  /// </summary>
  public MatchType MatchType { get; set; } = MatchType.Simple;

  /// <summary>
  /// Gets or sets the case matching behavior. The default is <see cref="IO.MatchCasing.PlatformDefault"/>.
  /// </summary>
  public MatchCasing MatchCasing { get; set; } = MatchCasing.PlatformDefault;

  /// <summary>
  /// Gets or sets the maximum recursion depth. The default is <see cref="int.MaxValue"/>.
  /// </summary>
  public int MaxRecursionDepth { get; set; } = int.MaxValue;

  /// <summary>
  /// Gets or sets a value that indicates whether the search operation should return
  /// special directory entries "." and "..". The default is <see langword="false"/>.
  /// </summary>
  public bool ReturnSpecialDirectories { get; set; }

  /// <summary>
  /// Gets or sets the suggested buffer size, in bytes. The default is 0 (no suggestion).
  /// </summary>
  public int BufferSize { get; set; }

  /// <summary>
  /// Gets the default enumeration options.
  /// </summary>
  public static EnumerationOptions Default { get; } = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="EnumerationOptions"/> class with the default options.
  /// </summary>
  public EnumerationOptions() { }

}

/// <summary>
/// Specifies the type of wildcard matching to use.
/// </summary>
public enum MatchType {
  /// <summary>
  /// Match using simple matching rules. '*' matches any sequence of characters. '?' matches any single character.
  /// </summary>
  Simple = 0,

  /// <summary>
  /// Match using Win32 DOS-style matching semantics.
  /// '*' matches any sequence of characters. '?' matches any single character.
  /// '*.*' matches any name with a period in it (or that ends with '*').
  /// </summary>
  Win32 = 1
}

/// <summary>
/// Specifies the type of character casing to match.
/// </summary>
public enum MatchCasing {
  /// <summary>
  /// Match using the default casing for the platform.
  /// </summary>
  PlatformDefault = 0,

  /// <summary>
  /// Match case-sensitively.
  /// </summary>
  CaseSensitive = 1,

  /// <summary>
  /// Match case-insensitively.
  /// </summary>
  CaseInsensitive = 2
}

#endif
