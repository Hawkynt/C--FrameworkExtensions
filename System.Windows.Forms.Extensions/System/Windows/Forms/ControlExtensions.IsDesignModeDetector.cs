#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  private sealed class IsDesignModeDetector {
    public static IsDesignModeDetector Instance { get; } = new();

    private readonly Dictionary<Type, Func<Control, bool>> _methodCache = [];

    private IsDesignModeDetector() { }

    public bool GetDesignModePropertyValue(Control c) {
      Func<Control, bool> getter;
      var controlType = c.GetType();
      lock (this._methodCache)
        if (!this._methodCache.TryGetValue(controlType, out getter))
          this._methodCache.Add(controlType, getter = CreateGetDesignModePropertyGetter(controlType));

      return getter(c);
    }

    private static Func<Control, bool> CreateGetDesignModePropertyGetter(Type controlType) {
      var propertyInfo = controlType.GetProperty("DesignMode", BindingFlags.Instance | BindingFlags.NonPublic);

      var getMethod = propertyInfo?.GetGetMethod(true);
      if (getMethod == null)
        return _ => false;

      var call = (Func<Control, bool>)Delegate.CreateDelegate(typeof(Func<Control, bool>), getMethod);
      return call;
    }
  }
}
