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

#if !SUPPORTS_ARRAY_MAXLENGTH

namespace System;

public static partial class ArrayPolyfills {

  extension(Array) {

    /// <summary>
    /// Gets the maximum number of elements that may be contained in an array.
    /// </summary>
    /// <value>The maximum count of elements that may be contained in an array.</value>
    /// <remarks>
    /// This property returns the maximum possible value that can be used as the length of an array.
    /// On 64-bit systems, this is 0x7FFFFFC7 (2,147,483,591) to account for object header and alignment.
    /// </remarks>
    public static int MaxLength => 0x7FFFFFC7;

  }

}

#endif
