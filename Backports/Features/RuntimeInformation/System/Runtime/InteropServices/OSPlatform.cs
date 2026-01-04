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

// OSPlatform was added in .NET Standard 1.1 / .NET Core 1.0
// Available via NuGet package for net45+
#if !SUPPORTS_RUNTIME_INFORMATION && !OFFICIAL_RUNTIME_INFORMATION

namespace System.Runtime.InteropServices;

/// <summary>
/// Represents an operating system platform.
/// </summary>
public readonly struct OSPlatform : IEquatable<OSPlatform> {

  private readonly string _osPlatform;

  private OSPlatform(string osPlatform) => this._osPlatform = osPlatform ?? throw new ArgumentNullException(nameof(osPlatform));

  /// <summary>
  /// Gets an object that represents the FreeBSD operating system.
  /// </summary>
  public static OSPlatform FreeBSD { get; } = new("FREEBSD");

  /// <summary>
  /// Gets an object that represents the Linux operating system.
  /// </summary>
  public static OSPlatform Linux { get; } = new("LINUX");

  /// <summary>
  /// Gets an object that represents the macOS operating system.
  /// </summary>
  public static OSPlatform OSX { get; } = new("OSX");

  /// <summary>
  /// Gets an object that represents the Windows operating system.
  /// </summary>
  public static OSPlatform Windows { get; } = new("WINDOWS");

  /// <summary>
  /// Creates a new OSPlatform instance.
  /// </summary>
  /// <param name="osPlatform">The name of the platform.</param>
  /// <returns>An object that represents the <paramref name="osPlatform"/> operating system.</returns>
  public static OSPlatform Create(string osPlatform) {
    ArgumentNullException.ThrowIfNull(osPlatform);
    ArgumentException.ThrowIfNullOrEmpty(osPlatform);
    
    return new(osPlatform);
  }

  /// <inheritdoc />
  public bool Equals(OSPlatform other) => string.Equals(this._osPlatform, other._osPlatform, StringComparison.OrdinalIgnoreCase);

  /// <inheritdoc />
  public override bool Equals(object obj) => obj is OSPlatform other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() => this._osPlatform?.ToUpperInvariant().GetHashCode() ?? 0;

  /// <inheritdoc />
  public override string ToString() => this._osPlatform ?? string.Empty;

  public static bool operator ==(OSPlatform left, OSPlatform right) => left.Equals(right);

  public static bool operator !=(OSPlatform left, OSPlatform right) => !left.Equals(right);

}

#endif
