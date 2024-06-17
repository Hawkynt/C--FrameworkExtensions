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

namespace System.Security.Cryptography;

public interface IAdvancedHashAlgorithm {
  string Name { get; }
  int OutputBits { get; set; }
  byte[] IV { get; set; }

#if SUPPORTS_STATIC_IN_INTERFACES
  static abstract int MinOutputBits { get; }
  static abstract int MaxOutputBits { get; }
  static abstract int[] SupportedOutputBits { get; }

  static abstract bool SupportsIV { get; }
  static abstract int MinIVBits { get; }
  static abstract int MaxIVBits { get; }
  static abstract int[] SupportedIVBits { get; }

#endif
}
