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

using System.Reflection.Emit;
using System.Collections.Concurrent;
namespace System.Runtime {
  internal static class DynamicObjectFactory {
    /// <summary>
    /// Creates an instance of a class decided at runtime
    /// </summary>
    /// <typeparam name="TClass">The type of the class.</typeparam>
    /// <param name="arrParams">The parameters for the constructor.</param>
    /// <returns>An new instance of the class</returns>
    public static TClass CreateInstance<TClass>(params object[] arrParams) where TClass : class {
#if !DEBUG
      try {
#endif
      return (TClass)((arrParams == null || arrParams.Length == 0)
                ? _varCreateInstanceRaw<TClass>()
                : (Activator.CreateInstance(typeof(TClass), arrParams)));
#if !DEBUG
      } catch {
        return (null);
      }
#endif
    }

    private delegate object delCreateObject();
    private static readonly Type tpDelegate=typeof(delCreateObject);
    private static readonly ConcurrentDictionary<Type, delCreateObject> _hashILCache = new ConcurrentDictionary<Type, delCreateObject>();
    // create a class by calling an empty constructur
    private static TClass _varCreateInstanceRaw<TClass>() {
      // this is fast for up to 6 different classes
      //return (Activator.CreateInstance<TClass>());
      // after that this is faster because it caches
      // first call is 5 times slower than the above 
      // but every call after that is 3 times faster
      Type tpClass = typeof(TClass);
      string strCacheName = "varObjFactory$$_" + tpClass.FullName;
      delCreateObject ptrCall;
      if (!_hashILCache.TryGetValue(tpClass, out ptrCall)) {
        DynamicMethod objDynamicMethod = new DynamicMethod(strCacheName, typeof(object), null, tpClass);
        ILGenerator objILGenerator = objDynamicMethod.GetILGenerator();
        objILGenerator.Emit(OpCodes.Newobj, tpClass.GetConstructor(Type.EmptyTypes));
        objILGenerator.Emit(OpCodes.Ret);
        ptrCall=(delCreateObject)objDynamicMethod.CreateDelegate(tpDelegate);
        _hashILCache.TryAdd(tpClass, ptrCall);
      } else {
        // successfully read cache entry
      }
      return ((TClass)ptrCall());
    }
    /// <summary>
    /// Creates an instance of a class decided at runtime
    /// </summary>
    /// <param name="strType">Type of the class.</param>
    /// <param name="arrParams">The parameters for the constructor.</param>
    /// <returns>A new instance of the class</returns>
    public static object CreateInstance(string strType, params object[] arrParams) {
      try {
        return (Activator.CreateInstance(Type.GetType(strType), arrParams));
      } catch {
        return (null);
      }
    }
  }
}
