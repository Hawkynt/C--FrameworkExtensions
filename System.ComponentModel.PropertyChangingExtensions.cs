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

/* ATTENTION: This file is generated by a tool. All changes will be reverted upon next run ! */

using System.Runtime.CompilerServices;
#if !NETFX_45
using System.Diagnostics;
using System.Linq;
#endif

// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.ComponentModel {
  internal static partial class PropertyChangingExtensions {

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref bool backingField, 
      bool value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref byte backingField, 
      byte value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref sbyte backingField, 
      sbyte value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref char backingField, 
      char value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref short backingField, 
      short value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref ushort backingField, 
      ushort value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref int backingField, 
      int value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref uint backingField, 
      uint value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref long backingField, 
      long value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref ulong backingField, 
      ulong value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref float backingField, 
      float value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref double backingField, 
      double value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref decimal backingField, 
      decimal value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref string backingField, 
      string value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (backingField == value)
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }

  
    /// <summary>
    /// Checks if a property already matches a desired value.  Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="This">Type of the object.</typeparam>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="onPropertyChanging">The property changing event invocator.</param>
    /// <param name="onPropertyChanged">The property changed event invocator.</param>
    /// <param name="backingField">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.  This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>
    /// <c>True</c> if the value was changed, <c>False</c> if the existing value matched the
    /// desired value.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SetProperty<This,T>(
      this This @this, 
      Action<string> onPropertyChanging, 
      Action<string> onPropertyChanged, 
      ref T backingField, 
      T value, 
#if NETFX_45
      [CallerMemberName]string propertyName = null
#else
      string propertyName = null
#endif
    ) where This:INotifyPropertyChanged,INotifyPropertyChanging {
      if (Equals(backingField, value))
        return (false);

#if !NETFX_45
      if(propertyName==null){
        var method=new StackTrace().GetFrame(1).GetMethod();
        var name = method.Name;
        if (method.IsSpecialName) {
          var declaringType = method.DeclaringType;
          if (
            declaringType != null && (
              (name.StartsWith("get_") && declaringType.GetProperties().Any(p => p.GetGetMethod() == method)) ||
               (name.StartsWith("set_") && declaringType.GetProperties().Any(p => p.GetSetMethod() == method))
            )
          )
            name = name.Substring(4);
        }
        propertyName = name;
      }
#endif
      onPropertyChanging(propertyName);
      backingField = value;
      onPropertyChanged(propertyName);
      return (true);
    }
  }
}