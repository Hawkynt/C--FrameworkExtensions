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

      while (repeatCount-- > 0) {
        try {
          This();
          return (true);
        } catch (Exception) {

        }
      }
      return (false);
    }

  }
}