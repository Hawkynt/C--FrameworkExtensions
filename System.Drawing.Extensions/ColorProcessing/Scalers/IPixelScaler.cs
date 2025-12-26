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

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Interface for pixel-art scalers with discrete scale factors.
/// </summary>
/// <remarks>
/// <para>
/// Implemented by public scaler structs (Scale2x, Scale3x, Epx, Hq, etc.).
/// Used for generic dispatch in Upscale methods.
/// </para>
/// <para>
/// Pixel-art scalers operate on discrete scale factors.
/// Each concrete scaler type should provide static members:
/// <list type="bullet">
/// <item><c>SupportedScales</c> - Array of supported scale factors</item>
/// <item><c>SupportsScale(ScaleFactor)</c> - Check if a scale is supported</item>
/// <item><c>GetPossibleTargets(int, int)</c> - Enumerate valid target dimensions</item>
/// </list>
/// </para>
/// </remarks>
public interface IPixelScaler : IScalerInfo;
