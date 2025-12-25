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

#if !SUPPORTS_ZIPARCHIVE

namespace System.IO.Compression;

/// <summary>
/// Specifies values for interacting with zip archive entries.
/// </summary>
public enum ZipArchiveMode {
  /// <summary>
  /// Only reading archive entries is permitted.
  /// </summary>
  Read = 0,

  /// <summary>
  /// Only creating new archive entries is permitted.
  /// </summary>
  Create = 1,

  /// <summary>
  /// Both read and write operations are permitted for archive entries.
  /// </summary>
  Update = 2
}

#endif
