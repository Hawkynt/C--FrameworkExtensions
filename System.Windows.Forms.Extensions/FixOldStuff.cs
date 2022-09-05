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

#if NET45_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_INLINING
#endif

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Form.Extensions;
internal static class FixOldStuff {

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool _FOS_IsNullOrWhiteSpace(this string @this) =>
#if NET35_OR_GREATER && !NET40_OR_GREATER
    @this == null || @this.Trim().Length < 1
#else
    string.IsNullOrWhiteSpace(@this)
#endif
  ;

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static string _FOS_Join(this IEnumerable<string> @this,string separator) =>
#if NET35_OR_GREATER && !NET40_OR_GREATER
    string.Join(separator, @this.ToArray())
#else
    string.Join(separator, @this)
#endif
  ;

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool _FOS_HasFlag<TEnum>(this TEnum @this, TEnum flag) where TEnum : Enum =>
#if NET35_OR_GREATER && !NET40_OR_GREATER
    ((ulong)(object)@this & (ulong)(object)flag) == (ulong)(object)flag
#else
    @this.HasFlag(flag)
#endif
  ;

}
