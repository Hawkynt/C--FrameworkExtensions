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

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Marker interface for color metrics that guarantee normalized distance output (0.0-1.0).
/// </summary>
/// <remarks>
/// <para>
/// Metrics implementing this interface guarantee that <c>Distance()</c> returns
/// values in the range [0.0, 1.0], where:
/// <list type="bullet">
///   <item><description>0.0 = identical colors</description></item>
///   <item><description>1.0 = maximum possible difference</description></item>
/// </list>
/// </para>
/// <para>
/// This enables rescalers to use consistent threshold values regardless of
/// the underlying metric implementation. For example, a threshold of 0.1
/// means "10% of maximum possible difference" for any normalized metric.
/// </para>
/// <para>
/// Scalers requiring normalized input should add this as a generic constraint:
/// <code>where TMetric : struct, IColorMetric&lt;TKey&gt;, INormalizedMetric</code>
/// </para>
/// </remarks>
public interface INormalizedMetric;
