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

namespace System {
  internal static partial class ActivatorExtensions {
    /// <summary>
    /// Creates an instance of the given type by calling the parameterless ctor.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <returns>The instance of the given type.</returns>
    public static TType FromConstructor<TType>() {
      return typeof(TType).CreateInstance<TType>();
    }

    /// <summary>
    /// Creates an instance of the given type by calling the ctor with the given parameter type.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <typeparam name="TParam0">The type of the 1st parameter.</typeparam>
    /// <param name="param0">The 1st parameter.</param>
    /// <returns>The instance of the given type.</returns>
    public static TType FromConstructor<TType, TParam0>(TParam0 param0) {
      return typeof(TType).FromConstructor<TType, TParam0>(param0);
    }

    /// <summary>
    /// Creates an instance of the given type by calling the ctor with the given parameter type.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <typeparam name="TParam0">The type of the 1st parameter.</typeparam>
    /// <typeparam name="TParam1">The type of the 2nd parameter.</typeparam>
    /// <param name="param0">The 1st parameter.</param>
    /// <param name="param1">The 2nd parameter.</param>
    /// <returns>
    /// The instance of the given type.
    /// </returns>
    public static TType FromConstructor<TType, TParam0, TParam1>(TParam0 param0, TParam1 param1) {
      return typeof(TType).FromConstructor<TType, TParam0, TParam1>(param0, param1);
    }

    /// <summary>
    /// Creates an instance of the given type by calling the ctor with the given parameter type.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <typeparam name="TParam0">The type of the 1st parameter.</typeparam>
    /// <typeparam name="TParam1">The type of the 2nd parameter.</typeparam>
    /// <typeparam name="TParam2">The type of the 3rd parameter.</typeparam>
    /// <param name="param0">The 1st parameter.</param>
    /// <param name="param1">The 2nd parameter.</param>
    /// <param name="param2">The 3rd parameter.</param>
    /// <returns>
    /// The instance of the given type.
    /// </returns>
    public static TType FromConstructor<TType, TParam0, TParam1, TParam2>(TParam0 param0, TParam1 param1, TParam2 param2) {
      return typeof(TType).FromConstructor<TType, TParam0, TParam1, TParam2>(param0, param1, param2);
    }
  }
}
