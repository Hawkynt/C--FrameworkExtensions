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

namespace System.ComponentModel {
  public sealed class SortComparer<TValue> : IComparer<TValue> {
    private readonly PropertyDescriptor _propertyDescriptor;
    private readonly ListSortDirection _sortDirection;

    public SortComparer(PropertyDescriptor propertyDescriptor, ListSortDirection listSortDirection) {
      this._propertyDescriptor = propertyDescriptor;
      this._sortDirection = listSortDirection;
    }

    int IComparer<TValue>.Compare(TValue x, TValue y) => CompareValues(this._propertyDescriptor.GetValue(x), this._propertyDescriptor.GetValue(y), this._sortDirection);

    private static int CompareValues(object xValue, object yValue, ListSortDirection direction) {

      if (ReferenceEquals(xValue, yValue))
        return (0);

      var factor = direction == ListSortDirection.Ascending ? 1 : -1;

      if (ReferenceEquals(xValue, null))
        return (factor);

      if (ReferenceEquals(yValue, null))
        return (-factor);

      var comparable = xValue as IComparable;

      //can ask the x value
      if (comparable != null)
        return (factor * comparable.CompareTo(yValue));

      comparable = yValue as IComparable;

      //can ask the y value
      if (comparable != null)
        return (-factor * comparable.CompareTo(xValue));

      if (xValue.Equals(yValue))
        return (0);

      if (yValue.Equals(xValue))
        return (0);

      //not comparable, compare string representations
      return (factor * string.Compare(xValue.ToString(), yValue.ToString(), StringComparison.Ordinal));

    }
  }
}
