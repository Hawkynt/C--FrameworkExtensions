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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace System {
  internal static partial class ActivatorExtensions {
    /// <summary>
    /// Creates an instance of the given type by calling the parameterless ctor.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <returns>The instance of the given type.</returns>
    public static TType FromConstructor<TType>() {
      return (TType)Activator.CreateInstance(typeof(TType));
    }

    /// <summary>
    /// Compares two arrays of types for complete equality.
    /// </summary>
    /// <param name="array1">The 1st array.</param>
    /// <param name="array2">The 2nd array.</param>
    /// <param name="allowImplicitConversion">if set to <c>true</c> [allow implicit conversion].</param>
    /// <returns>
    ///   <c>true</c> if both arrays are equal; otherwise, <c>false</c>.
    /// </returns>
    private static bool _TypeArrayEquals(Type[] array1, ParameterInfo[] array2, bool allowImplicitConversion = false) {

      // if only one of the arrays is null, return false
      if ((array1 == null || array2 == null) && !(array1 == null && array2 == null))
        return (false);

      // both arrays are null, return true
      if (array1 == null)
        return (true);

      // no array is null, compare
      if (array1.Length != array2.Length)
        return (false);

      // compare elements
      if (allowImplicitConversion)
        return (array1.All((t, i) => array2[i].ParameterType.IsAssignableFrom(t)));

      return (array1.All((t, i) => array2[i].ParameterType == t));
    }

    /// <summary>
    /// Creates an instance from a ctor matching the given parameter types.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <param name="parameters">The parameters.</param>
    /// <returns>Anew types' instance.</returns>
    private static TType _FromConstructor<TType>(IEnumerable<Tuple<Type, object>> parameters) {
      Contract.Requires(parameters != null);
      var pars = parameters.ToArray();

      var typeToCreate = typeof(TType);
      var typeOfParams = (
        from i in pars
        select i.Item1
        ).ToArray();

      var ctors = typeToCreate.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

      var matchingCtor = (

        // try to find an exact matching ctor first
        from i in ctors
        let par = i.GetParameters()
        where par != null && par.Length == typeOfParams.Length && _TypeArrayEquals(typeOfParams, par)
        select i
      ).FirstOrDefault() ?? (

        // if none found, try to get a ctor that could be filled by implicit parameter conversions
        from i in ctors
        let par = i.GetParameters()
        where par != null && par.Length == typeOfParams.Length && _TypeArrayEquals(typeOfParams, par, true)
        select i
        ).FirstOrDefault();

      if (matchingCtor == null)
        throw new NotSupportedException("No matching ctor found");

      return (TType)matchingCtor.Invoke((from i in pars
                                         select i.Item2).ToArray());

    }

    /// <summary>
    /// Creates an instance of the given type by calling the ctor with the given parameter type.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <typeparam name="TParam0">The type of the 1st parameter.</typeparam>
    /// <param name="param0">The 1st parameter.</param>
    /// <returns>The instance of the given type.</returns>
    public static TType FromConstructor<TType, TParam0>(TParam0 param0) {
      return _FromConstructor<TType>(new[] {
        Tuple.Create(typeof (TParam0), (object)param0)
      });
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
      return _FromConstructor<TType>(new[] {
        Tuple.Create(typeof (TParam0), (object)param0),
        Tuple.Create(typeof (TParam1), (object)param1)
      });
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
      return _FromConstructor<TType>(new[] {
        Tuple.Create(typeof (TParam0), (object)param0),
        Tuple.Create(typeof (TParam1), (object)param1),
        Tuple.Create(typeof (TParam2), (object)param2)
      });
    }
  }
}
