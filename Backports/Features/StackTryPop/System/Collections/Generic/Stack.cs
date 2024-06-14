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

#if !SUPPORTS_STACK_TRYPOP

namespace System.Collections.Generic;

public static partial class StackPolyfills {
  /// <summary>
  ///   Returns a value that indicates whether there is an object at the top of the <see cref="Stack{T}" />, and if one is
  ///   present, copies it to the <paramref name="result" /> parameter, and removes it from the <see cref="Stack{T}" />.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This <see cref="Stack{T}" /></param>
  /// <param name="result">
  ///   If present, the object at the top of the <see cref="Stack{T}" />; otherwise, the default value of
  ///   T.
  /// </param>
  /// <returns>
  ///   <see langword="true" /> if there is an object at the top of the <see cref="Stack{T}" />;
  ///   <see langword="false" /> if the <see cref="Stack{T}" /> is empty.
  /// </returns>
  public static bool TryPop<TItem>(this Stack<TItem> @this, out TItem result) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    if (@this.Count < 1) {
      result = default;
      return false;
    }

    result = @this.Pop();
    return true;
  }
}

#endif
