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

#if !SUPPORTS_RANGE_AND_INDEX

namespace System.Runtime.CompilerServices;

public static class RuntimeHelpers {
  public static T[] GetSubArray<T>(T[] array, Range range) {
    if (array is null)
      throw new ArgumentNullException(nameof(array));

    var (offset, length) = range.GetOffsetAndLength(array.Length);
    var result = new T[length];
    Array.Copy(array, offset, result, 0, length);
    return result;
  }

  private static int? _offsetToStringData;

  [Obsolete("Dummy für Compiler – nicht aufrufen!", true)]
  public static int OffsetToStringData {
    get {
      return _offsetToStringData ??= Invoke();

      static unsafe int Invoke() {
        var runtimeString = DateTime.UtcNow.ToString("O");

        fixed (char* charPtr = runtimeString) {
          var stringPtr = GetObjectAddress(runtimeString);
          var dataPtr = new IntPtr(charPtr);
          return (int)(dataPtr.ToInt64() - stringPtr.ToInt64());
        }

        static IntPtr GetObjectAddress(object obj) {
          var tr = __makeref(obj);
          return **(IntPtr**)&tr;
        }
      }

    }
  }

}

#endif
