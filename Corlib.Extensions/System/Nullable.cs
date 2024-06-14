#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif

namespace System;

public static partial class NullableExtensions {
  /// <summary>
  ///   Detects whether the given <see cref="Nullable{T}" /> is <see langword="null" />.
  /// </summary>
  /// <typeparam name="T">The item type</typeparam>
  /// <param name="this">This <see cref="Nullable{T}" /></param>
  /// <returns>
  ///   <see langword="true" /> when the given reference is <see langword="null" />; otherwise,
  ///   <see langword="false" />.
  /// </returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNull<T>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
  [NotNullWhen(false)]
#endif
    this T? @this
  ) where T : struct => @this is null;

  /// <summary>
  ///   Detects whether the given <see cref="Nullable{T}" /> is <see langword="null" />.
  /// </summary>
  /// <typeparam name="T">The item type</typeparam>
  /// <param name="this">This <see cref="Nullable{T}" /></param>
  /// <returns>
  ///   <see langword="true" /> when the given reference is not <see langword="null" />; otherwise,
  ///   <see langword="false" />.
  /// </returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotNull<T>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
  [NotNullWhen(true)]
#endif
    this T? @this
  ) where T : struct => @this is not null;
}

/// <summary>
///   Extended Nullable type, that allows reference types to be wrapped.
/// </summary>
/// <typeparam name="TType">The type of the type.</typeparam>
public readonly struct NullableEx<TType>(TType value) {
  public bool HasValue { get; } = true;

  public TType Value {
    get {
      if (!this.HasValue)
        throw new InvalidOperationException("No Value");
      return value;
    }
  }

  public static implicit operator NullableEx<TType>(TType value) => new(value);

  public static explicit operator TType(NullableEx<TType> This) => This.Value;

  public TType GetValueOrDefault() => value;

  public TType GetValueOrDefault(TType defaultValue) => this.HasValue ? value : defaultValue;

  /// <summary>
  ///   Indicates whether the current <see cref="T:System.Nullable`1" /> object is equal to a specified object.
  /// </summary>
  /// <returns>
  ///   true, wenn der <paramref name="other" />-Parameter gleich dem aktuellen <see cref="T:System.Nullable`1" />-Objekt
  ///   ist, andernfalls false. Diese Tabelle beschreibt, wie Gleichheit für die verglichenen Werte definiert wird:
  ///   RückgabewertBeschreibungtrueDie <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft ist false, und der
  ///   <paramref name="other" />-Parameter ist null.Das heißt, zwei NULL-Werte sind per Definition gleich.- oder -Die
  ///   <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft ist true, und der von der
  ///   <see cref="P:System.Nullable`1.Value" />-Eigenschaft zurückgegebene Wert ist gleich dem <paramref name="other" />
  ///   -Parameter.falseDie <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft für die aktuelle
  ///   <see cref="T:System.Nullable`1" />-Struktur ist true, und der <paramref name="other" />-Parameter ist null.- oder
  ///   -Die <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft für die aktuelle <see cref="T:System.Nullable`1" />
  ///   -Struktur ist false, und der <paramref name="other" />-Parameter ist nicht null.- oder -Die
  ///   <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft der aktuellen <see cref="T:System.Nullable`1" />-Struktur ist
  ///   true, und der von der <see cref="P:System.Nullable`1.Value" />-Eigenschaft zurückgegebene Wert ist ungleich dem
  ///   <paramref name="other" />-Parameter.
  /// </returns>
  /// <param name="other">Ein Objekt.</param>
  /// <filterpriority>1</filterpriority>
  public override bool Equals(object other) => this.HasValue ? other != null && value.Equals(other) : other == null;

  /// <summary>
  ///   Ruft den Hashcode des Objekts ab, das von der <see cref="P:System.Nullable`1.Value" />-Eigenschaft zurückgegeben
  ///   wird.
  /// </summary>
  /// <returns>
  ///   Der Hashcode des Objekts, das von der <see cref="P:System.Nullable`1.Value" />-Eigenschaft zurückgegeben wird, wenn
  ///   die <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft true ist, oder 0 (null), wenn die
  ///   <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft false ist.
  /// </returns>
  /// <filterpriority>1</filterpriority>
  public override int GetHashCode() => this.HasValue ? value.GetHashCode() : 0;

  /// <summary>
  ///   Gibt die Textdarstellung des Werts des aktuellen <see cref="T:System.Nullable`1" />-Objekts zurück.
  /// </summary>
  /// <returns>
  ///   Die Textdarstellung des Werts des aktuellen <see cref="T:System.Nullable`1" />-Objekts, wenn die
  ///   <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft true ist, oder eine leere Zeichenfolge (""), wenn die
  ///   <see cref="P:System.Nullable`1.HasValue" />-Eigenschaft false ist.
  /// </returns>
  /// <filterpriority>1</filterpriority>
  public override string ToString() => this.HasValue ? value.ToString() : "";
}
