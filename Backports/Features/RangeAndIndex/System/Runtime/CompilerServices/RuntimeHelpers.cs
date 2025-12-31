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
//

#if !SUPPORTS_RUNTIMEHELPERS_GETSUBARRAY

namespace System.Runtime.CompilerServices;

public static partial class RuntimeHelpers {
  public static T[] GetSubArray<T>(T[] array, Range range) {
    ArgumentNullException.ThrowIfNull(array);

    var (offset, length) = range.GetOffsetAndLength(array.Length);
    var result = new T[length];
    Array.Copy(array, offset, result, 0, length);
    return result;
  }

  [Obsolete("Dummy für Compiler – nicht aufrufen!", true)]
  public static int OffsetToStringData => Utilities.Runtime.Is64BitArchitecture ? 12 : 8;

}

#endif
