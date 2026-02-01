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

namespace System.IO;

/// <summary>
///   Specifies how concurrent file modifications should be handled when working with files in progress.
/// </summary>
public enum ConflictResolutionMode {
  /// <summary>
  ///   No conflict detection - last write wins (current behavior).
  /// </summary>
  None = 0,

  /// <summary>
  ///   Lock the original file exclusively but allow concurrent readers.
  ///   Other processes can read the file but cannot write or delete it.
  /// </summary>
  LockWithReadShare,

  /// <summary>
  ///   Lock the original file exclusively - no readers or writers allowed.
  ///   Provides complete isolation during the work-in-progress operation.
  /// </summary>
  LockExclusive,

  /// <summary>
  ///   Verify LastWriteTimeUtc is unchanged before applying changes; throw <see cref="FileConflictException" /> on conflict.
  /// </summary>
  CheckLastWriteTimeAndThrow,

  /// <summary>
  ///   Verify LastWriteTimeUtc is unchanged before applying changes; silently discard changes on conflict.
  /// </summary>
  CheckLastWriteTimeAndIgnoreUpdate,

  /// <summary>
  ///   Verify SHA256 hash is unchanged before applying changes; throw <see cref="FileConflictException" /> on conflict.
  /// </summary>
  CheckChecksumAndThrow,

  /// <summary>
  ///   Verify SHA256 hash is unchanged before applying changes; silently discard changes on conflict.
  /// </summary>
  CheckChecksumAndIgnoreUpdate
}
