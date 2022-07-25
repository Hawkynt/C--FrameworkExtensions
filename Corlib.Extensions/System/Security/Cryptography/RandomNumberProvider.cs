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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Security.Cryptography {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class RandomNumberGeneratorExtenions {
    /// <summary>
    /// Gets a new random number.
    /// </summary>
    /// <param name="This">This RandomNumberGenerator.</param>
    /// <param name="maxValue">The maximum exclusive value.</param>
    /// <returns></returns>
    public static int Next(this RandomNumberGenerator This, int maxValue) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      var data = new byte[4];
      This.GetBytes(data);
      var result = BitConverter.ToInt32(data, 0);
      return (Math.Abs(result) % maxValue);
    }
  }
}
