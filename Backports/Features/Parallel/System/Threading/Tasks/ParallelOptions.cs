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

#if !SUPPORTS_PARALLEL

namespace System.Threading.Tasks;

/// <summary>
/// Stores options that configure the operation of methods on the <see cref="Parallel"/> class.
/// </summary>
public class ParallelOptions {

  /// <summary>
  /// Gets or sets the <see cref="CancellationToken"/> associated with this <see cref="ParallelOptions"/> instance.
  /// </summary>
  public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

  /// <summary>
  /// Gets or sets the maximum number of concurrent tasks enabled by this <see cref="ParallelOptions"/> instance.
  /// </summary>
  public int MaxDegreeOfParallelism { get; set; } = -1;

  /// <summary>
  /// Gets or sets the <see cref="TaskScheduler"/> associated with this <see cref="ParallelOptions"/> instance.
  /// </summary>
  public TaskScheduler TaskScheduler { get; set; } = TaskScheduler.Default;

}

#endif
