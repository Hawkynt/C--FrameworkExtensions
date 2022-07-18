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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETSTANDARD || NETCOREAPP

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Collections.Concurrent {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class ConcurrentQueueExtensions {
    /// <summary>
    /// Clears the specified Queue.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This ConcurrentQueue.</param>
    public static void Clear<TValue>(this ConcurrentQueue<TValue> This) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      TValue dummy;
      while (This.TryDequeue(out dummy))
        ;
    }
  }
}

#endif