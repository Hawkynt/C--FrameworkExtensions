﻿// This file is part of Hawkynt's .NET Framework extensions.
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
//

#if !SUPPORTS_PARSABLE

namespace System {

  /// <summary>
  /// You need to implement:
  ///   * public static abstract TSelf Parse(ReadOnlySpan&lt;char&gt; s, IFormatProvider? provider);
  ///   * public static abstract bool TryParse(ReadOnlySpan&lt;char&gt; s, IFormatProvider? provider, out TSelf result);
  /// </summary>
  /// <typeparam name="TSelf"></typeparam>
  public interface ISpanParsable<TSelf> : IParsable<TSelf> where TSelf : ISpanParsable<TSelf>;

}

#endif
