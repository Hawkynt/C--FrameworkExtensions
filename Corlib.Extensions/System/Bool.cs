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

using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class BoolExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToOneOrZeroString(this bool @this) => _ConvertToString(@this, "1", "0", false);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToYesOrNoString(this bool @this, bool useLowerCaseOnly = false) => _ConvertToString(@this, "Yes", "No", useLowerCaseOnly);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToTrueOrFalseString(this bool @this, bool useLowerCaseOnly = false) => _ConvertToString(@this, "True", "False", useLowerCaseOnly);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static string _ConvertToString(bool value, string trueValue, string falseValue, bool useLowerCaseOnly) {
    var result = value ? trueValue : falseValue;
    if (useLowerCaseOnly)
      result = result.ToLower();

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void When(this bool @this, Action @true = null, Action @false = null) {
    Against.True(@true == null && @false == null);

    var action = @this ? @true : @false;
    action?.Invoke();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void When(this bool @this, Action<bool> @true = null, Action<bool> @false = null) {
    Against.True(@true == null && @false == null);

    var action = @this ? @true : @false;
    action?.Invoke(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool WhenTrue(this bool @this, Action callback) {
    Against.ArgumentIsNull(callback);

    if (@this)
      callback();
    return @this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool WhenTrue(this bool @this, Action<bool> callback) {
    Against.ArgumentIsNull(callback);

    if (@this)
      callback(true);
    return @this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool WhenFalse(this bool @this, Action callback) {
    Against.ArgumentIsNull(callback);

    if (!@this)
      callback();
    return @this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool WhenFalse(this bool @this, Action<bool> callback) {
    Against.ArgumentIsNull(callback);

    if (!@this)
      callback(false);
    return @this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TResult WhenTrue<TResult>(this bool @this, Func<TResult> callback) {
    Against.ArgumentIsNull(callback);

    return @this ? callback() : default;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TResult WhenFalse<TResult>(this bool @this, Func<TResult> callback) {
    Against.ArgumentIsNull(callback);

    return @this ? default : callback();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TResult When<TResult>(this bool @this, Func<TResult> @true = null, Func<TResult> @false = null) {
    Against.True(@true == null && @false == null);

    var function = @this ? @true : @false;
    return function == null ? default : function();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TResult WhenTrue<TResult>(this bool @this, Func<bool, TResult> callback) {
    Against.ArgumentIsNull(callback);

    return @this ? callback(true) : default;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TResult WhenFalse<TResult>(this bool @this, Func<bool, TResult> callback) {
    Against.ArgumentIsNull(callback);

    return @this ? default : callback(false);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TResult When<TResult>(this bool @this, Func<bool, TResult> @true = null, Func<bool, TResult> @false = null) {
    Against.True(@true == null && @false == null);

    var function = @this ? @true : @false;
    return function == null ? default : function(@this);
  }
}
