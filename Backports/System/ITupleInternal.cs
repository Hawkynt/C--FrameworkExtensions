#if !SUPPORTS_VALUE_TUPLE
using System.Collections;

namespace System;

/// <summary>
/// Helper so we can call some tuple methods recursively without knowing the underlying types.
/// </summary>
internal interface ITupleInternal {
  int Size { get; }

  int GetHashCode(IEqualityComparer comparer);

  string ToStringEnd();
}
#endif