#region (c)2010-2020 Hawkynt
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

using System.Diagnostics.Contracts;

namespace System {
  internal static partial class ActionExtensions {
    /// <summary>
    /// Tries to invoke the given delegate.
    /// </summary>
    /// <param name="This">This Action.</param>
    /// <param name="repeatCount">The repeat count, until execution is aborted.</param>
    /// <returns>
    ///   <c>true</c> on success; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryInvoke(this Action This, int repeatCount = 1) {
      Contract.Requires(This != null);
      Contract.Requires(repeatCount > 0);

      while (--repeatCount >= 0) {
        try {
          This();
          return (true);
        } catch (Exception) {

        }
      }
      return (false);
    }

    #region Async action calls
    
    public static IAsyncResult Async(this Action @this, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(@this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke(this Action @this, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(callback, null));
    }

    
    public static IAsyncResult Async<T1>(this Action<T1> @this, T1 arg1, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1>(this Action<T1> @this, T1 arg1, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2>(this Action<T1, T2> @this, T1 arg1, T2 arg2, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2>(this Action<T1, T2> @this, T1 arg1, T2 arg2, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3>(this Action<T1, T2, T3> @this, T1 arg1, T2 arg2, T3 arg3, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3>(this Action<T1, T2, T3> @this, T1 arg1, T2 arg2, T3 arg3, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, callback, null));
    }

    
    public static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, object state = null) {
      Contract.Requires(@this!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16, @this.EndInvoke, state));
    }

    public static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, AsyncCallback callback) {
      Contract.Requires(@this!=null);
      Contract.Requires(callback!=null);
      return (@this.BeginInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16, callback, null));
    }

    
    #endregion
  }
}