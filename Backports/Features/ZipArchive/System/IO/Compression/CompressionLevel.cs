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
/// Specifies values that indicate whether a compression operation emphasizes speed or compression size.
/// </summary>
public enum CompressionLevel {
  /// <summary>
  /// The compression operation should optimally balance compression speed and output size.
  /// </summary>
  Optimal = 0,

  /// <summary>
  /// The compression operation should complete as quickly as possible, even if the resulting file is not optimally compressed.
  /// </summary>
  Fastest = 1,

  /// <summary>
  /// No compression should be performed on the file.
  /// </summary>
  NoCompression = 2,

  /// <summary>
  /// The compression operation should create output as small as possible, even if the operation takes a longer time to complete.
  /// </summary>
  SmallestSize = 3
}

#endif
