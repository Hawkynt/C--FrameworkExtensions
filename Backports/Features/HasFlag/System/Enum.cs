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

#if !SUPPORTS_HAS_FLAG

using Guard;
using System.Reflection.Emit;

namespace System;

public static partial class EnumPolyfills {
  public static bool HasFlag<T>(this T @this, T flag) where T : unmanaged, Enum => HasFlagHelper<T>.HasFlag(@this, flag);

  private static class HasFlagHelper<T> where T : unmanaged, Enum {
    public static readonly Func<T, T, bool> HasFlag = _CreateImplementation();

    private static Func<T, T, bool> _CreateImplementation() {
      var underlyingType = Enum.GetUnderlyingType(typeof(T));
      var dm = new DynamicMethod(string.Empty, typeof(bool), [typeof(T), typeof(T)], true);
      var il = dm.GetILGenerator();
      il.Emit(OpCodes.Ldarg_1);
      EmitConversion(il, underlyingType);
      il.Emit(OpCodes.Dup);
      il.Emit(OpCodes.Ldarg_0);
      EmitConversion(il, underlyingType);
      il.Emit(OpCodes.And);
      il.Emit(OpCodes.Ceq);
      il.Emit(OpCodes.Ret);
      return (Func<T, T, bool>)dm.CreateDelegate(typeof(Func<T, T, bool>));

      static void EmitConversion(ILGenerator il, Type underlyingType) {
        if (underlyingType == typeof(long))
          il.Emit(OpCodes.Conv_I8);
        else if (underlyingType == typeof(ulong))
          il.Emit(OpCodes.Conv_U8);
        else if(underlyingType == typeof(int))
          il.Emit(OpCodes.Conv_I4);
        else if (underlyingType == typeof(uint))
          il.Emit(OpCodes.Conv_U4);
        else if (underlyingType == typeof(short))
          il.Emit(OpCodes.Conv_I2);
        else if (underlyingType == typeof(ushort))
          il.Emit(OpCodes.Conv_U2);
        else if (underlyingType == typeof(sbyte))
          il.Emit(OpCodes.Conv_I1);
        else if (underlyingType == typeof(byte))
          il.Emit(OpCodes.Conv_U1);
        else
          AlwaysThrow.ArgumentException("Unsupported underlying enum type", nameof(underlyingType));
      }
    }
  }

}

#endif
