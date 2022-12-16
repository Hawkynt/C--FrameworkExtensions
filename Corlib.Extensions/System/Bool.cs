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

// ReSharper disable UnusedMember.Global
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System {
  // ReSharper disable once PartialTypeWithSinglePart
  // ReSharper disable once UnusedMember.Global

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class BoolExtensions {

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ToOneOrZeroString(this bool @this) => _ConvertToString(@this, "1", "0", false);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ToYesOrNoString(this bool @this, bool useLowerCaseOnly = false) => _ConvertToString(@this, "Yes", "No", useLowerCaseOnly);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ToTrueOrFalseString(this bool @this, bool useLowerCaseOnly = false) => _ConvertToString(@this, "True", "False", useLowerCaseOnly);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static string _ConvertToString(bool value, string trueValue, string falseValue, bool useLowerCaseOnly) {
      var result = value ? trueValue : falseValue;
      if (useLowerCaseOnly)
        result = result.ToLower();

      return result;
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void When(this bool @this, Action @true = null, Action @false = null) {
      if (@this)
        @true?.Invoke();
      else
        @false?.Invoke();
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void When(this bool @this, Action<bool> @true = null, Action<bool> @false = null) {
      if (@this)
        @true?.Invoke(true);
      else
        @false?.Invoke(false);
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool WhenTrue(this bool @this, Action callback) {
      if (@this)
        callback();
      return @this;
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool WhenTrue(this bool @this, Action<bool> callback) {
      if (@this)
        callback(true);
      return @this;
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool WhenFalse(this bool @this, Action callback) {
      if (!@this)
        callback();
      return @this;
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool WhenFalse(this bool @this, Action<bool> callback) {
      if (!@this)
        callback(false);
      return @this;
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TResult WhenTrue<TResult>(this bool @this, Func<TResult> callback) => @this ? callback() : default(TResult);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TResult WhenFalse<TResult>(this bool @this, Func<TResult> callback) => @this ? default(TResult) : callback();

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TResult When<TResult>(this bool @this, Func<TResult> @true = null, Func<TResult> @false = null) => @this ? @true == null ? default(TResult) : @true() : @false == null ? default(TResult) : @false();

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TResult WhenTrue<TResult>(this bool @this, Func<bool, TResult> callback) => @this ? callback(true) : default(TResult);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TResult WhenFalse<TResult>(this bool @this, Func<bool, TResult> callback) => @this ? default(TResult) : callback(false);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TResult When<TResult>(this bool @this, Func<bool, TResult> @true = null, Func<bool, TResult> @false = null) => @this ? @true == null ? default(TResult) : @true(true) : @false == null ? default(TResult) : @false(false);

  }
}