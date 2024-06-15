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
using Guard;

namespace System.Text;

// ReSharper disable once PartialTypeWithSinglePart
public static partial class StringBuilderExtensions {
  /// <summary>
  ///   Appends a collection of lines to the end of the current <see cref="StringBuilder" /> object.
  ///   Each line is appended as a new line.
  /// </summary>
  /// <param name="this">The <see cref="StringBuilder" /> to which the lines should be appended.</param>
  /// <param name="lines">An <see cref="IEnumerable{T}" /> of lines to append to the <see cref="StringBuilder" />.</param>
  public static void AppendLines(this StringBuilder @this, IEnumerable<string> lines) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(lines);

    foreach (var line in lines)
      @this.AppendLine(line);
  }
}
