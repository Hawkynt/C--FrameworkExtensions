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

#if !SUPPORTS_TUPLES

namespace System.Collections;

public interface IStructuralEquatable {
  bool Equals(object other, IEqualityComparer comparer);

  int GetHashCode(IEqualityComparer comparer);
}

#endif
