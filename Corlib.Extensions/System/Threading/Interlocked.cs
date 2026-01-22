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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

// ReSharper disable CompareOfFloatsByEqualityOperator
namespace System.Threading;

public static partial class InterlockedEx {

  extension(Interlocked) {

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue CompareExchange<TValue>(ref TValue source, TValue value, TValue comparand, __StructForcingTag<TValue> _ = null) where TValue : struct
      => HelperForEnum<TValue>.CompareExchangeImplementation(ref source, value, comparand);

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue Exchange<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct
      => HelperForEnum<TValue>.ExchangeImplementation(ref source, value);

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue Read<TValue>(ref TValue source, __StructForcingTag<TValue> _ = null) where TValue : struct
      => HelperForEnum<TValue>.CompareExchangeImplementation(ref source, default(TValue), default(TValue));

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct, Enum
      => HelperForEnum<TValue>.HasFlagImplementation(ref source, value);

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue SetFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct, Enum
      => HelperForEnum<TValue>.SetFlagImplementation(ref source, value);

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue ClearFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct, Enum
      => HelperForEnum<TValue>.ClearFlagImplementation(ref source, value);

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue ToggleFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null) where TValue : struct, Enum
      => HelperForEnum<TValue>.ToggleFlagImplementation(ref source, value);

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue CompareExchange<TValue>(ref TValue source, TValue value, TValue comparand, __ClassForcingTag<TValue> _ = null) where TValue : class
      => Interlocked.CompareExchange(ref source, value, comparand);

    // ReSharper disable once UnusedParameter.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue Exchange<TValue>(ref TValue source, TValue value, __ClassForcingTag<TValue> _ = null) where TValue : class
      => Interlocked.Exchange(ref source, value);

  }
}