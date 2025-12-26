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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Provides exact equality comparison for colors.
/// </summary>
/// <remarks>
/// Uses <see cref="EqualityComparer{T}.Default"/> which avoids boxing for types
/// implementing <see cref="System.IEquatable{T}"/>.
/// This is the default equality strategy for pixel-art scalers.
/// </remarks>
public readonly struct ExactEquality<TKey> : IColorEquality<TKey> where TKey : unmanaged, IColorSpace {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b) => EqualityComparer<TKey>.Default.Equals(a, b);
}
