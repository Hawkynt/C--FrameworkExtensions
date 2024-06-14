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

#if !SUPPORTS_FIRSTLASTSINGLE_PREDICATE

using System.Collections.Generic;

namespace System.Linq;

public static partial class EnumerablePolyfills {
  public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate)
    => FirstOrDefault(@this, predicate, default);

  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate)
    => SingleOrDefault(@this, predicate, default);

  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate)
    => LastOrDefault(@this, predicate, default);
}

#endif
