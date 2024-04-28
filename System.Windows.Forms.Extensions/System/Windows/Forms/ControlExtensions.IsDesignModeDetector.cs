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

using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  private class IsDesignModeDetector {
    
    public static IsDesignModeDetector Instance { get; } = new();

    private readonly Dictionary<Type, Func<Control, bool>> _methodCache = new();

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
