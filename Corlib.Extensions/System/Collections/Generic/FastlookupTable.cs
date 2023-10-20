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
namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class FastLookupTable<TItem> : ICollection<TItem>, ICloneable {
    private readonly Dictionary<TItem, bool> _table = new();

    #region Implementation of IEnumerable<TItem>
    public IEnumerator<TItem> GetEnumerator() {
      return (this._table.Keys.GetEnumerator());
    }
    #endregion

    #region Implementation of IEnumerable
    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
    #endregion

    #region Implementation of ICollection<TItem>
    /// <summary>
    /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
    public void Add(TItem item) {
      if (this._table.ContainsKey(item))
        throw new NotSupportedException("Item already exists");
      this._table.Add(item, true);
    }

    /// <summary>
    /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
    public void Clear() {
      this._table.Clear();
    }

    /// <summary>
    /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
    /// </summary>
    /// <returns>
    /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
    /// </returns>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
    public bool Contains(TItem item) {
      return (this._table.ContainsKey(item));
    }

    /// <summary>
    /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
    public void CopyTo(TItem[] array, int arrayIndex) {
      this._table.Keys.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <returns>
    /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </returns>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
    public bool Remove(TItem item) {
      return (this._table.Remove(item));
    }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </returns>
    public int Count {
      get {
        return (this._table.Count);
      }
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
    /// </summary>
    /// <returns>
    /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
    /// </returns>
    public bool IsReadOnly {
      get {
        return (false);
      }
    }
    #endregion

    #region
    /// <summary>
    /// Tries to add the given element.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public bool TryAdd(TItem item) {
      if (this._table.ContainsKey(item))
        return (false);
      this.Add(item);
      return (true);
    }
    #endregion

    #region Implementation of ICloneable
    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public object Clone() {
      FastLookupTable<TItem> result = new();
      foreach (var kvp in this._table)
        result._table.Add(kvp.Key, kvp.Value);
      return (result);
    }
    #endregion
  }
}