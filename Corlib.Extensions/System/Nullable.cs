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

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class NullableExtensions {

  /// <summary>
  /// Detects whether the given <see cref="Nullable{T}"/> is <see langword="null"/>.
  /// </summary>
  /// <typeparam name="T">The item type</typeparam>
  /// <param name="this">This <see cref="Nullable{T}"/></param>
  /// <returns><see langword="true"/> when the given reference is <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNull<T>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
  [NotNullWhen(false)] 
#endif
    this T? @this) where T:struct => @this is null;

  /// <summary>
  /// Detects whether the given <see cref="Nullable{T}"/> is <see langword="null"/>.
  /// </summary>
  /// <typeparam name="T">The item type</typeparam>
  /// <param name="this">This <see cref="Nullable{T}"/></param>
  /// <returns><see langword="true"/> when the given reference is not <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotNull<T>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
  [NotNullWhen(true)] 
#endif
    this T? @this) where T : struct => @this is not null;
  
}

/// <summary>
/// Extended Nullable type, that allows reference types to be wrapped.
/// </summary>
/// <typeparam name="TType">The type of the type.</typeparam>

#if COMPILE_TO_EXTENSION_DLL
public 
#else
internal
#endif
readonly struct NullableEx<TType> {
  private readonly TType _value;
  private readonly bool _hasValue;

  public bool HasValue => this._hasValue;

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
  /// true, wenn der <paramref name="other"/>-Parameter gleich dem aktuellen <see cref="T:System.Nullable`1"/>-Objekt ist, andernfalls false. Diese Tabelle beschreibt, wie Gleichheit für die verglichenen Werte definiert wird: RückgabewertBeschreibungtrueDie <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft ist false, und der <paramref name="other"/>-Parameter ist null.Das heißt, zwei NULL-Werte sind per Definition gleich.- oder -Die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft ist true, und der von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zurückgegebene Wert ist gleich dem <paramref name="other"/>-Parameter.falseDie <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft für die aktuelle <see cref="T:System.Nullable`1"/>-Struktur ist true, und der <paramref name="other"/>-Parameter ist null.- oder -Die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft für die aktuelle <see cref="T:System.Nullable`1"/>-Struktur ist false, und der <paramref name="other"/>-Parameter ist nicht null.- oder -Die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft der aktuellen <see cref="T:System.Nullable`1"/>-Struktur ist true, und der von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zurückgegebene Wert ist ungleich dem <paramref name="other"/>-Parameter.
  /// </returns>
  /// <param name="other">Ein Objekt.</param><filterpriority>1</filterpriority>
  public override bool Equals(object other) => this.HasValue ? other != null && this._value.Equals(other) : other == null;

  /// <summary>
  /// Ruft den Hashcode des Objekts ab, das von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zurückgegeben wird.
  /// </summary>
  /// 
  /// <returns>
  /// Der Hashcode des Objekts, das von der <see cref="P:System.Nullable`1.Value"/>-Eigenschaft zurückgegeben wird, wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft true ist, oder 0 (null), wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft false ist.
  /// </returns>
  /// <filterpriority>1</filterpriority>
  public override int GetHashCode() => this.HasValue ? this._value.GetHashCode() : 0;

  /// <summary>
  /// Gibt die Textdarstellung des Werts des aktuellen <see cref="T:System.Nullable`1"/>-Objekts zurück.
  /// </summary>
  /// 
  /// <returns>
  /// Die Textdarstellung des Werts des aktuellen <see cref="T:System.Nullable`1"/>-Objekts, wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft true ist, oder eine leere Zeichenfolge (""), wenn die <see cref="P:System.Nullable`1.HasValue"/>-Eigenschaft false ist.
  /// </returns>
  /// <filterpriority>1</filterpriority>
  public override string ToString() => this.HasValue ? this._value.ToString() : "";
}
