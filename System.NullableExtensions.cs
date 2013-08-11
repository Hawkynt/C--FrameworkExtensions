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

using System.Diagnostics.Contracts;
using word = System.UInt16;
using dword = System.UInt32;
using qword = System.UInt64;

namespace System {
  internal static partial class NullableExtensions {

  }

  /// <summary>
  /// Extended Nullable type, that allows reference types to be wrapped.
  /// </summary>
  /// <typeparam name="TType">The type of the type.</typeparam>
  internal struct NullableEx<TType> {
    private readonly TType _value;
    private readonly bool _hasValue;

    public bool HasValue { get { return (this._hasValue); } }

    public TType Value {
      get {
        if (!this._hasValue)
          throw new InvalidOperationException("No Value");
        return (this._value);
      }
    }

    public NullableEx(TType value)
      : this() {
      this._value = value;
      this._hasValue = true;
    }

    public static implicit operator NullableEx<TType>(TType value) {
      return (new NullableEx<TType>(value));
    }

    public static explicit operator TType(NullableEx<TType> This) {
      return (This.Value);
    }

    public TType GetValueOrDefault() {
      return (this._value);
    }

    public TType GetValueOrDefault(TType defaultValue) {
      return (this._hasValue ? this._value : defaultValue);
    }

    /// <summary>
    /// Indicates whether the current <see cref="T:System.Nullable`1"/> object is equal to a specified object.
    /// </summary>
    /// 
    /// <returns>
    /// true, wenn der <paramref name="other"/>-Parameter gleich dem aktuellen <see cref="T:System.Nullable`1"/>-Objekt ist, andernfalls false. Diese Tabelle beschreibt, wie Gleichheit f�r die verglichenen Werte definiert wird: R�ckgabewertBeschreibungtrueDie <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft ist false, und der <paramref name="other"/>-Parameter ist null.Das hei�t, zwei NULL-Werte sind per Definition gleich.-�oder�-Die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft ist true, und der von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zur�ckgegebene Wert ist gleich dem <paramref name="other"/>-Parameter.falseDie <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft f�r die aktuelle <see cref="T:System.Nullable`1"/>-Struktur ist true, und der <paramref name="other"/>-Parameter ist null.-�oder�-Die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft f�r die aktuelle <see cref="T:System.Nullable`1"/>-Struktur ist false, und der <paramref name="other"/>-Parameter ist nicht null.-�oder�-Die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft der aktuellen <see cref="T:System.Nullable`1"/>-Struktur ist true, und der von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zur�ckgegebene Wert ist ungleich dem <paramref name="other"/>-Parameter.
    /// </returns>
    /// <param name="other">Ein Objekt.</param><filterpriority>1</filterpriority>
    public override bool Equals(object other) {
      return this.HasValue ? other != null && this._value.Equals(other) : other == null;
    }

    /// <summary>
    /// Ruft den Hashcode des Objekts ab, das von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zur�ckgegeben wird.
    /// </summary>
    /// 
    /// <returns>
    /// Der Hashcode des Objekts, das von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zur�ckgegeben wird, wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft true ist, oder 0 (null), wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft false ist.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public override int GetHashCode() {
      return this.HasValue ? this._value.GetHashCode() : 0;
    }

    /// <summary>
    /// Gibt die Textdarstellung des Werts des aktuellen <see cref="T:System.Nullable`1"/>-Objekts zur�ck.
    /// </summary>
    /// 
    /// <returns>
    /// Die Textdarstellung des Werts des aktuellen <see cref="T:System.Nullable`1"/>-Objekts, wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft true ist, oder eine leere Zeichenfolge (""), wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft false ist.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public override string ToString() {
      return this.HasValue ? this._value.ToString() : "";
    }
  }

}