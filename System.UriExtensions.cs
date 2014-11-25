﻿#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System {
  internal static partial class UriExtensions {

    private static readonly Dictionary<HttpRequestHeader, string> _DEFAULT_HEADERS = new Dictionary<HttpRequestHeader, string> {
      {HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12"},
      {HttpRequestHeader.Accept, "*/*"},
      {HttpRequestHeader.AcceptLanguage, "en-gb,en;q=0.5"},
      {HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7"},
    };

    /// <summary>
    /// Reads all text.
    /// </summary>
    /// <param name="This">This Uri.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="postValues">The post values.</param>
    /// <returns>
    /// The text of the target url
    /// </returns>
    public static string ReadAllText(this Uri This, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
      Contract.Requires(This != null);
      Trace.WriteLine(This.AbsoluteUri);

      if (This.IsFile)
        return (encoding == null ? File.ReadAllText(This.AbsolutePath) : File.ReadAllText(This.AbsolutePath, encoding));

      using (var webClient = new WebClient()) {
        webClient.Headers.AddRange(headers ?? _DEFAULT_HEADERS);
        if (encoding != null)
          webClient.Encoding = encoding;

        Exception ex = null;
        while (retryCount-- >= 0) {
          try {
            if (postValues == null)
              return (webClient.DownloadString(This));

            var nameValCollection = new NameValueCollection();
            foreach (var kvp in postValues)
              nameValCollection.Add(kvp.Key, kvp.Value);

            return (encoding ?? Encoding.Default).GetString(webClient.UploadValues(This, nameValCollection));
          } catch (Exception e) {
            ex = e;
          }
        }
        throw (ex);
      }
    }

    /// <summary>
    /// Reads all text.
    /// </summary>
    /// <param name="This">This Uri.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <returns>The text of the target url</returns>
    public static async Task<string> ReadAllTextTaskAsync(this Uri This, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null) {
      Contract.Requires(This != null);

      if (This.IsFile)
        return await (new Task<string>(() => encoding == null ? File.ReadAllText(This.AbsolutePath) : File.ReadAllText(This.AbsolutePath, encoding)));

      using (var webClient = new WebClient()) {
        webClient.Headers.AddRange(headers ?? _DEFAULT_HEADERS);
        if (encoding != null)
          webClient.Encoding = encoding;

        Exception ex = null;
        while (retryCount-- >= 0) {
          try {
            return await (webClient.DownloadStringTaskAsync(This));
          } catch (Exception e) {
            ex = e;
          }
        }
        throw (ex);
      }
    }

    /// <summary>
    /// Reads all bytes.
    /// </summary>
    /// <param name="This">This Uri.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="postValues">The post values.</param>
    /// <returns>
    /// The bytes of the target url
    /// </returns>
    public static byte[] ReadAllBytes(this Uri This, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
      if (This.IsFile)
        return (File.ReadAllBytes(This.AbsolutePath));

      using (var webClient = new WebClient()) {
        webClient.Headers.AddRange(headers ?? _DEFAULT_HEADERS);

        Exception ex = null;
        while (retryCount-- >= 0) {
          try {
            if (postValues == null)
              return (webClient.DownloadData(This));

            var nameValCollection = new NameValueCollection();
            foreach (var kvp in postValues)
              nameValCollection.Add(kvp.Key, kvp.Value);

            return webClient.UploadValues(This, nameValCollection);
          } catch (Exception e) {
            ex = e;
          }
        }
        throw (ex);
      }
    }

    /// <summary>
    /// Reads all bytes.
    /// </summary>
    /// <param name="This">This Uri.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>
    /// The bytes of the target url
    /// </returns>
    public static async Task<byte[]> ReadAllBytesTaskAsync(
      this Uri This,
      int retryCount = 0,
      IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null) {
      if (This.IsFile)
        return await (new Task<byte[]>(() => File.ReadAllBytes(This.AbsolutePath)));

      using (var webClient = new WebClient()) {
        webClient.Headers.AddRange(headers ?? _DEFAULT_HEADERS);

        Exception ex = null;
        while (retryCount-- >= 0) {
          try {
            return await webClient.DownloadDataTaskAsync(This);
          } catch (Exception e) {
            ex = e;
          }
        }
        throw (ex);
      }
    }

    /// <summary>
    /// Gets the base part of the uri.
    /// </summary>
    /// <param name="This">This Uri.</param>
    /// <returns></returns>
    public static Uri BaseUri(this Uri This) {
      var result = new Uri(This.Scheme + "://" + This.Host + ":" + This.Port);
      return (result);
    }

    /// <summary>
    /// Gets a new uri from this one using a relative path.
    /// </summary>
    /// <param name="This">This Uri.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static Uri Path(this Uri This, string path) {
      return (
        path.IsNullOrWhiteSpace()
        ? This
        : new Uri(This.AbsoluteUri + (path.StartsWith("/") ? path.Substring(1) : path)))
        ;
    }
  }
}
