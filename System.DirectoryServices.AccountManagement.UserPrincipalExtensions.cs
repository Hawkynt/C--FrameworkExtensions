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
namespace System.DirectoryServices.AccountManagement {
  /// <summary>
  /// Extensions for the UserPrincipal objects from System.DirectoryServices.AccountManagement
  /// </summary>
  internal static partial class UserPrincipalExtensions {
    /// <summary>
    /// Gets the full name.
    /// </summary>
    /// <param name="This">This UserPrincipal.</param>
    /// <returns>The full name.</returns>
    public static string GetFullName(this UserPrincipal This) {
      Contract.Requires(This != null);
      return (new[] { This.GivenName, This.MiddleName, This.Surname }.Join(" ", true));
    }
  }
}
