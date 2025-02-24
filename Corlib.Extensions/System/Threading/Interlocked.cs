#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

// ReSharper disable CompareOfFloatsByEqualityOperator
namespace System.Threading;

public static partial class InterlockedEx {

  #region nested types

  private static class HelperForEnum<TValue> {
    public delegate TValue CompareExchangeDelegateType(ref TValue source, TValue value, TValue comparand);
    public delegate TValue ExchangeDelegateType(ref TValue source, TValue value);
    public delegate bool HasFlagDelegateType(ref TValue source, TValue value);
    public delegate TValue SetFlagDelegateType(ref TValue source, TValue value);
    public delegate TValue ClearFlagDelegateType(ref TValue source, TValue value);
    public delegate TValue ToggleFlagDelegateType(ref TValue source, TValue value);
    public static readonly CompareExchangeDelegateType CompareExchangeImplementation = _CreateCompareExchangeImplementation();
    public static readonly ExchangeDelegateType ExchangeImplementation = _CreateExchangeImplementation();
    public static readonly HasFlagDelegateType HasFlagImplementation = _CreateHasFlagImplementation();
    public static readonly SetFlagDelegateType SetFlagImplementation = _CreateSetFlagImplementation();
    public static readonly ClearFlagDelegateType ClearFlagImplementation = _CreateClearFlagImplementation();
    public static readonly ToggleFlagDelegateType ToggleFlagImplementation = _CreateToggleFlagImplementation();

    // ReSharper disable once StaticMemberInGenericType
    private static Type _originalType;
    // ReSharper disable once StaticMemberInGenericType
    private static Type _signedType;
    // ReSharper disable once StaticMemberInGenericType
    private static MethodInfo _usedMethodForCompareExchange;

    private static void _InitIfNeeded() {
      if (_originalType != null)
        return;

      var originalType = typeof(TValue);
      if (!originalType.IsEnum)
        throw new ArgumentException("Must be enum!");

      var underlyingType = Enum.GetUnderlyingType(originalType);
      var signedType =
        underlyingType == typeof(int) || underlyingType == typeof(uint) ? typeof(int)
        : underlyingType == typeof(long) || underlyingType == typeof(ulong) ? typeof(long)
        : null
        ;

      _originalType = originalType;
      _signedType = signedType ?? throw new NotSupportedException("Enum type must be int, uint, long or ulong!");
      _usedMethodForCompareExchange = typeof(Interlocked).GetMethod(
        nameof(Interlocked.CompareExchange),
        BindingFlags.Static | BindingFlags.Public,
        null,
        new[] { signedType.MakeByRefType(), signedType, signedType },
        null
      );
    }

    private static CompareExchangeDelegateType _CreateCompareExchangeImplementation() {
      _InitIfNeeded();
      DynamicMethod dynamicMethod = new(string.Empty, _originalType, new[] { _originalType.MakeByRefType(), _originalType, _originalType }, true);
      var ilGenerator = dynamicMethod.GetILGenerator();
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldarg_1);
      ilGenerator.Emit(OpCodes.Ldarg_2);
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);
      ilGenerator.Emit(OpCodes.Ret);
      return (CompareExchangeDelegateType)dynamicMethod.CreateDelegate(typeof(CompareExchangeDelegateType));
    }

    private static ExchangeDelegateType _CreateExchangeImplementation() {
      _InitIfNeeded();
      DynamicMethod dynamicMethod = new(string.Empty, _originalType, new[] { _originalType.MakeByRefType(), _originalType }, true);
      var ilGenerator = dynamicMethod.GetILGenerator();
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldarg_1);
      ilGenerator.Emit(OpCodes.Call, typeof(Interlocked).GetMethod(
        nameof(Interlocked.Exchange),
        BindingFlags.Static | BindingFlags.Public,
        null,
        new[] { _signedType.MakeByRefType(), _signedType },
        null
      ));
      ilGenerator.Emit(OpCodes.Ret);
      return (ExchangeDelegateType)dynamicMethod.CreateDelegate(typeof(ExchangeDelegateType));
    }

    private static HasFlagDelegateType _CreateHasFlagImplementation() {
      _InitIfNeeded();
      DynamicMethod dynamicMethod = new(string.Empty, typeof(bool), new[] { _originalType.MakeByRefType(), _originalType }, true);
      var ilGenerator = dynamicMethod.GetILGenerator();

      ilGenerator.Emit(OpCodes.Ldarg_1); /* 2nd argument for ceq */               /* Stack: arg1 */
      ilGenerator.Emit(OpCodes.Dup);     /* 2nd argument for and */               /* Stack: arg1, arg1 */
      ilGenerator.Emit(OpCodes.Ldarg_0);                                          /* Stack: arg1, arg1, arg0 */
      ilGenerator.Emit(OpCodes.Ldc_I4_0);                                         /* Stack: arg1, arg1, arg0, 0 */
      ilGenerator.Emit(OpCodes.Dup);                                              /* Stack: arg1, arg1, arg0, 0, 0 */
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);              /* Stack: arg1, arg1, value */
      ilGenerator.Emit(OpCodes.And);                                              /* Stack: arg1, and */
      ilGenerator.Emit(OpCodes.Ceq);                                              /* Stack: result */
      ilGenerator.Emit(OpCodes.Ret);
      return (HasFlagDelegateType)dynamicMethod.CreateDelegate(typeof(HasFlagDelegateType));
    }

    private static SetFlagDelegateType _CreateSetFlagImplementation() {
      _InitIfNeeded();
      DynamicMethod dynamicMethod = new(string.Empty, _originalType, new[] { _originalType.MakeByRefType(), _originalType }, true);
      var ilGenerator = dynamicMethod.GetILGenerator();
      var loopLabel = ilGenerator.DefineLabel();
      var oldValue = ilGenerator.DeclareLocal(_signedType);
      var newValue = ilGenerator.DeclareLocal(_signedType);

      ilGenerator.MarkLabel(loopLabel);                                           /* Stack: */

      // read value
      ilGenerator.Emit(OpCodes.Ldarg_0); /* 1st argument for call */              /* Stack: arg0 */
      ilGenerator.Emit(OpCodes.Ldarg_1); /* 2nd argument for or */                /* Stack: arg0, arg1 */

      ilGenerator.Emit(OpCodes.Ldarg_0);                                          /* Stack: arg0, arg1, arg0 */
      ilGenerator.Emit(OpCodes.Ldc_I4_0);                                         /* Stack: arg0, arg1, arg0, 0 */
      ilGenerator.Emit(OpCodes.Dup);                                              /* Stack: arg0, arg1, arg0, 0, 0 */
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);              /* Stack: arg0, arg1, oldValue */

      // write value to variable
      ilGenerator.Emit(OpCodes.Dup);                                              /* Stack: arg0, arg1, oldValue, oldValue */
      ilGenerator.Emit(OpCodes.Stloc, oldValue);                                  /* Stack: arg0, arg1, oldValue */

      // set flags
      ilGenerator.Emit(OpCodes.Or);                                               /* Stack: arg0, newValue */
      ilGenerator.Emit(OpCodes.Dup); /* 2nd argument for call */                  /* Stack: arg0, newValue, newValue */
      ilGenerator.Emit(OpCodes.Stloc, newValue);                                  /* Stack: arg0, newValue */

      // compare exchange
      ilGenerator.Emit(OpCodes.Ldloc, oldValue);                                  /* Stack: arg0, newValue, oldValue */
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);              /* Stack: result */

      // if changed, loop
      ilGenerator.Emit(OpCodes.Ldloc, oldValue);                                  /* Stack: result, oldValue */
      ilGenerator.Emit(OpCodes.Ceq);                                              /* Stack: success */
      ilGenerator.Emit(OpCodes.Brfalse_S, loopLabel);                             /* Stack: */

      // return changed value
      ilGenerator.Emit(OpCodes.Ldloc, newValue);                                  /* Stack: newValue */
      ilGenerator.Emit(OpCodes.Ret);
      return (SetFlagDelegateType)dynamicMethod.CreateDelegate(typeof(SetFlagDelegateType));
    }

    private static ClearFlagDelegateType _CreateClearFlagImplementation() {
      _InitIfNeeded();
      DynamicMethod dynamicMethod = new(string.Empty, _originalType, new[] { _originalType.MakeByRefType(), _originalType }, true);
      var ilGenerator = dynamicMethod.GetILGenerator();
      var loopLabel = ilGenerator.DefineLabel();
      var maskValue = ilGenerator.DeclareLocal(_signedType);
      var oldValue = ilGenerator.DeclareLocal(_signedType);
      var newValue = ilGenerator.DeclareLocal(_signedType);

      // create mask
      ilGenerator.Emit(OpCodes.Ldc_I4_M1);
      if (_signedType == typeof(long))
        ilGenerator.Emit(OpCodes.Conv_I8);

      ilGenerator.Emit(OpCodes.Ldarg_1);
      ilGenerator.Emit(OpCodes.Xor);
      ilGenerator.Emit(OpCodes.Stloc, maskValue);

      ilGenerator.MarkLabel(loopLabel);

      // read value
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldc_I4_0);
      ilGenerator.Emit(OpCodes.Dup);
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);

      // write value to variable
      ilGenerator.Emit(OpCodes.Dup);
      ilGenerator.Emit(OpCodes.Stloc, oldValue);

      // set flags
      ilGenerator.Emit(OpCodes.Ldloc, maskValue);
      ilGenerator.Emit(OpCodes.And);
      ilGenerator.Emit(OpCodes.Stloc, newValue);

      // compare exchange
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldloc, newValue);
      ilGenerator.Emit(OpCodes.Ldloc, oldValue);
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);

      // if changed, loop
      ilGenerator.Emit(OpCodes.Ldloc, oldValue);
      ilGenerator.Emit(OpCodes.Ceq);
      ilGenerator.Emit(OpCodes.Brfalse_S, loopLabel);

      // return changed value
      ilGenerator.Emit(OpCodes.Ldloc, newValue);
      ilGenerator.Emit(OpCodes.Ret);
      return (ClearFlagDelegateType)dynamicMethod.CreateDelegate(typeof(ClearFlagDelegateType));
    }

    private static ToggleFlagDelegateType _CreateToggleFlagImplementation() {
      _InitIfNeeded();
      DynamicMethod dynamicMethod = new(string.Empty, _originalType, new[] { _originalType.MakeByRefType(), _originalType }, true);
      var ilGenerator = dynamicMethod.GetILGenerator();
      var loopLabel = ilGenerator.DefineLabel();
      var oldValue = ilGenerator.DeclareLocal(_signedType);
      var newValue = ilGenerator.DeclareLocal(_signedType);

      ilGenerator.MarkLabel(loopLabel);

      // read value
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldc_I4_0);
      ilGenerator.Emit(OpCodes.Dup);
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);

      // write value to variable
      ilGenerator.Emit(OpCodes.Dup);
      ilGenerator.Emit(OpCodes.Stloc, oldValue);

      // toggle flags
      ilGenerator.Emit(OpCodes.Ldarg_1);
      ilGenerator.Emit(OpCodes.Xor);
      ilGenerator.Emit(OpCodes.Stloc, newValue);

      // compare exchange
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldloc, newValue);
      ilGenerator.Emit(OpCodes.Ldloc, oldValue);
      ilGenerator.Emit(OpCodes.Call, _usedMethodForCompareExchange);

      // if changed, loop
      ilGenerator.Emit(OpCodes.Ldloc, oldValue);
      ilGenerator.Emit(OpCodes.Ceq);
      ilGenerator.Emit(OpCodes.Brfalse_S, loopLabel);

      // return changed value
      ilGenerator.Emit(OpCodes.Ldloc, newValue);
      ilGenerator.Emit(OpCodes.Ret);
      return (ToggleFlagDelegateType)dynamicMethod.CreateDelegate(typeof(ToggleFlagDelegateType));
    }

  }

  #endregion

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue CompareExchange<TValue>(ref TValue source, TValue value, TValue comparand, __StructForcingTag<TValue> _ = null) where TValue : struct
    => HelperForEnum<TValue>.CompareExchangeImplementation(ref source, value, comparand)
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue Exchange<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct
    => HelperForEnum<TValue>.ExchangeImplementation(ref source, value)
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue Read<TValue>(ref TValue source, __StructForcingTag<TValue> _ = null) where TValue : struct
    => HelperForEnum<TValue>.CompareExchangeImplementation(ref source, default(TValue), default(TValue))
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool HasFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct
    => HelperForEnum<TValue>.HasFlagImplementation(ref source, value)
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue SetFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct
    => HelperForEnum<TValue>.SetFlagImplementation(ref source, value)
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue ClearFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct
    => HelperForEnum<TValue>.ClearFlagImplementation(ref source, value)
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue ToggleFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct
    => HelperForEnum<TValue>.ToggleFlagImplementation(ref source, value)
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue CompareExchange<TValue>(ref TValue source, TValue value, TValue comparand, __ClassForcingTag<TValue> _ = null) where TValue : class
    => Interlocked.CompareExchange(ref source, value, comparand)
    ;

  // ReSharper disable once UnusedParameter.Global
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TValue Exchange<TValue>(ref TValue source, TValue value, __ClassForcingTag<TValue> _ = null) where TValue : class
    => Interlocked.Exchange(ref source, value)
    ;

}