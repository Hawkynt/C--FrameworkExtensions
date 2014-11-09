#region (c)2010-2020 Hawkynt
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

using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Text;

namespace System {
  internal static partial class UriExtensions {

    /// <summary>
    /// Reads all text.
    /// </summary>
    /// <param name="This">This Uri.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <returns></returns>
    public static string ReadAllText(this Uri This, Encoding encoding = null, int retryCount = 0) {
      Contract.Requires(This != null);

      if (This.IsFile)
        return (encoding == null ? File.ReadAllText(This.AbsolutePath) : File.ReadAllText(This.AbsolutePath, encoding));

      using (var webClient = new WebClient()) {
        webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12");
        webClient.Headers.Add("Accept", "*/*");
        webClient.Headers.Add("Accept-Language", "en-gb,en;q=0.5");
        webClient.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
        if (encoding != null)
          webClient.Encoding = encoding;

        Exception ex = null;
        while (retryCount-- >= 0) {
          try {
            return (webClient.DownloadString(This));
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
    /// <returns></returns>
    public static byte[] ReadAllBytes(this Uri This, int retryCount = 0) {
      if (This.IsFile)
        return (File.ReadAllBytes(This.AbsolutePath));

      using (var webClient = new WebClient()) {
        webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12");
        webClient.Headers.Add("Accept", "*/*");
        webClient.Headers.Add("Accept-Language", "en-gb,en;q=0.5");
        webClient.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");

        Exception ex = null;
        while (retryCount-- >= 0) {
          try {
            return (webClient.DownloadData(This));
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
    public static Uri GetByRelativePath(this Uri This, string path) {
      if (path.IsNullOrWhiteSpace())
        return (This);

      return (new Uri(This.AbsoluteUri + (path.StartsWith("/") ? path.Substring(1) : path)));
    }

  }
}
