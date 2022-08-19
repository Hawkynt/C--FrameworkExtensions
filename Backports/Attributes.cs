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

/*
 *  This file provides attributes, which are introduced in certain framework 
 *  version thus allowing a modern compiler to produce code targeting older
 *  framework version.
 *
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAttributeUsageProperty
namespace System.Runtime.CompilerServices {

#if !NET45_OR_GREATER && !NET5_0_OR_GREATER && !NETCOREAPP && !NETSTANDARD
  [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  sealed class CallerMemberNameAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  sealed class CallerFilePathAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  sealed class CallerLineNumberAttribute : Attribute { }
#endif

#if !NET35_OR_GREATER && !NET5_0_OR_GREATER && !NETCOREAPP && !NETSTANDARD
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  sealed class ExtensionAttribute : Attribute { }
#endif

}
