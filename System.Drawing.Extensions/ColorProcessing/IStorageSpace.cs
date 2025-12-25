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

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Marker interface for color spaces that can be directly serialized to storage.
/// </summary>
/// <remarks>
/// <para>
/// Storage spaces (like <c>Bgra8888</c>, <c>Rgb24</c>, <c>Rgb565</c>) represent packed, byte-oriented
/// pixel formats that can be written directly to files or memory buffers without conversion.
/// </para>
/// <para>
/// Non-storage color spaces (like <c>LinearRgbaF</c>, <c>OklabF</c>) are working or perceptual
/// spaces that require encoding through an <c>IEncode</c> implementation before they can be saved.
/// </para>
/// <para>
/// This distinction enables compile-time safety for operations that require serializable formats:
/// <code>
/// public void SaveToFile&lt;T&gt;(Frame&lt;T&gt; frame, string path) where T : unmanaged, IStorageSpace
/// {
///   // Only accepts storage-compatible types
/// }
/// </code>
/// </para>
/// </remarks>
public interface IStorageSpace : IColorSpace;
