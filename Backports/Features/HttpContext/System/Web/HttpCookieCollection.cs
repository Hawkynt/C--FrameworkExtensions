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

// HttpCookieCollection is only available in .NET Framework 4.0+ via System.Web.dll
#if !SUPPORTS_HTTPCONTEXT

using System.Collections;
using System.Collections.Specialized;

namespace System.Web;

/// <summary>
/// Provides a type-safe way to manipulate HTTP cookies.
/// </summary>
public sealed class HttpCookieCollection : NameObjectCollectionBase {

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpCookieCollection"/> class.
  /// </summary>
  public HttpCookieCollection() { }

  /// <summary>
  /// Adds the specified cookie to the cookie collection.
  /// </summary>
  /// <param name="cookie">The <see cref="HttpCookie"/> to add to the collection.</param>
  public void Add(HttpCookie cookie) {
    ArgumentNullException.ThrowIfNull(cookie);
    this.BaseAdd(cookie.Name, cookie);
  }

  /// <summary>
  /// Updates the value of an existing cookie in a cookie collection.
  /// </summary>
  /// <param name="cookie">The <see cref="HttpCookie"/> object to update.</param>
  public void Set(HttpCookie cookie) {
    ArgumentNullException.ThrowIfNull(cookie);
    this.BaseSet(cookie.Name, cookie);
  }

  /// <summary>
  /// Removes the cookie with the specified name from the collection.
  /// </summary>
  /// <param name="name">The name of the cookie to remove from the collection.</param>
  public void Remove(string name) => this.BaseRemove(name);

  /// <summary>
  /// Clears all cookies from the cookie collection.
  /// </summary>
  public void Clear() => this.BaseClear();

  /// <summary>
  /// Returns the cookie with the specified name from the cookie collection.
  /// </summary>
  /// <param name="name">The name of the cookie to retrieve from the collection.</param>
  /// <returns>The <see cref="HttpCookie"/> specified by <paramref name="name"/>.</returns>
  public HttpCookie? Get(string name) => (HttpCookie?)this.BaseGet(name);

  /// <summary>
  /// Returns the cookie with the specified index from the cookie collection.
  /// </summary>
  /// <param name="index">The index of the cookie to return from the collection.</param>
  /// <returns>The <see cref="HttpCookie"/> specified by <paramref name="index"/>.</returns>
  public HttpCookie? Get(int index) => (HttpCookie?)this.BaseGet(index);

  /// <summary>
  /// Returns the key (name) of the cookie at the specified numerical index.
  /// </summary>
  /// <param name="index">The index of the key to retrieve from the collection.</param>
  /// <returns>The name of the cookie specified by <paramref name="index"/>.</returns>
  public string? GetKey(int index) => this.BaseGetKey(index);

  /// <summary>
  /// Gets the cookie with the specified name from the cookie collection.
  /// </summary>
  /// <param name="name">The name of the cookie to retrieve.</param>
  /// <returns>The <see cref="HttpCookie"/> specified by <paramref name="name"/>.</returns>
  public HttpCookie? this[string name] => this.Get(name);

  /// <summary>
  /// Gets the cookie with the specified numerical index from the cookie collection.
  /// </summary>
  /// <param name="index">The index of the cookie to retrieve from the collection.</param>
  /// <returns>The <see cref="HttpCookie"/> specified by <paramref name="index"/>.</returns>
  public HttpCookie? this[int index] => this.Get(index);

  /// <summary>
  /// Gets a string array containing all the keys (cookie names) in the cookie collection.
  /// </summary>
  public string?[] AllKeys => this.BaseGetAllKeys();

}

#endif
