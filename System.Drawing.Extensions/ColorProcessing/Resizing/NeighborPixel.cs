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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hawkynt.ColorProcessing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// A pixel with both work-space and key-space representations.
/// </summary>
/// <typeparam name="TWork">The working color type (for interpolation/accumulation).</typeparam>
/// <typeparam name="TKey">The key color type (for pattern matching/comparison).</typeparam>
/// <param name="Work">The work-space color value.</param>
/// <param name="Key">The key-space color value.</param>
[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct NeighborPixel<TWork, TKey>(TWork Work, TKey Key)
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace;
