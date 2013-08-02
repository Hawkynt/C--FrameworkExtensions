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
    private readonly PropertyDescriptor _propertyDescriptor = null;
    private readonly ListSortDirection _sortDirection = ListSortDirection.Ascending;

    public SortComparer(PropertyDescriptor propertyDescriptor, ListSortDirection listSortDirection) {
      _propertyDescriptor = propertyDescriptor;
      _sortDirection = listSortDirection;
    }

    int IComparer<TValue>.Compare(TValue x, TValue y) {
      var xValue = _propertyDescriptor.GetValue(x);
      var yValue = _propertyDescriptor.GetValue(y);
      return CompareValues(xValue, yValue, _sortDirection);
    }

    private static int CompareValues(object xValue, object yValue, ListSortDirection direction) {
      var retValue = 0;

      if (xValue == null && yValue == null)
        return (0);

      if (xValue is IComparable) {

        //can ask the x value
        retValue = ((IComparable)xValue).CompareTo(yValue);
      } else if (yValue is IComparable) {

        //can ask the y value
        retValue = -((IComparable)yValue).CompareTo(xValue);
      } else if (!xValue.Equals(yValue)) {

        //not comparable, compare string representations
        retValue = xValue.ToString().CompareTo(yValue.ToString());
      }
      return direction == ListSortDirection.Ascending ? retValue : -retValue;
    }
  }
}
