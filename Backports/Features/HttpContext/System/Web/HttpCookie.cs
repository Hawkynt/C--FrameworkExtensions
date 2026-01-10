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

// HttpCookie is only available in .NET Framework 4.0+ via System.Web.dll
#if !SUPPORTS_HTTPCONTEXT

using System.Collections.Specialized;

namespace System.Web;

/// <summary>
/// Provides a type-safe way to create and manipulate individual HTTP cookies.
/// </summary>
public sealed class HttpCookie {

  /// <summary>
  /// Creates and names a new cookie.
  /// </summary>
  /// <param name="name">The name of the new cookie.</param>
  public HttpCookie(string name) {
    this.Name = name ?? throw new ArgumentNullException(nameof(name));
    this.Values = new();
  }

  /// <summary>
  /// Creates, names, and assigns a value to a new cookie.
  /// </summary>
  /// <param name="name">The name of the new cookie.</param>
  /// <param name="value">The value of the new cookie.</param>
  public HttpCookie(string name, string? value) : this(name) => this.Value = value;

  /// <summary>
  /// Gets or sets the name of the cookie.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Gets or sets the value of the cookie.
  /// </summary>
  public string? Value {
    get => this.Values.Count > 0 ? this.Values[null] : null;
    set {
      if (this.Values.Count > 0)
        this.Values[null] = value;
      else
        this.Values.Add(null, value);
    }
  }

  /// <summary>
  /// Gets a collection of key/value pairs that are contained within a single cookie object.
  /// </summary>
  public NameValueCollection Values { get; }

  /// <summary>
  /// Gets or sets the virtual path to transmit with the current cookie.
  /// </summary>
  public string Path { get; set; } = "/";

  /// <summary>
  /// Gets or sets the domain to associate the cookie with.
  /// </summary>
  public string? Domain { get; set; }

  /// <summary>
  /// Gets or sets the expiration date and time for the cookie.
  /// </summary>
  public DateTime Expires { get; set; }

  /// <summary>
  /// Gets or sets a value that specifies whether a cookie is accessible by client-side script.
  /// </summary>
  public bool HttpOnly { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether to transmit the cookie using Secure Sockets Layer (SSL).
  /// </summary>
  public bool Secure { get; set; }

  /// <summary>
  /// Gets a value indicating whether the cookie has subkeys.
  /// </summary>
  public bool HasKeys => this.Values.Count > 1 || (this.Values.Count == 1 && this.Values.GetKey(0) != null);

  /// <summary>
  /// Gets a shortcut to the <see cref="Values"/> property.
  /// </summary>
  /// <param name="key">The key for the cookie value.</param>
  /// <returns>The cookie value associated with the specified key.</returns>
  public string? this[string key] {
    get => this.Values[key];
    set => this.Values[key] = value;
  }

}

#endif
