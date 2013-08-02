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
using System.Text;

namespace System.IO {
  internal static partial class PathExtensions {
    /// <summary>
    /// Generates a temporary filename which is most like the given one in the temporary folder.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
    /// <returns>The full path of the created temporary directory.</returns>
    public static string GetTempFileName(string name = null, string baseDirectory = null) {

      // use fully random name if none is given
      if (name == null)
        return (Path.GetTempFileName());

      var path = baseDirectory ?? Path.GetTempPath();
      name = Path.GetFileName(name);
      Contract.Assert(name != null, "Filename went <null>");
      var fullName = Path.Combine(path, name);

      // if we could use the given name
      if (TryCreateFile(fullName))
        return (fullName);

      // otherwise, count
      var i = 1;
      var fileName = Path.GetFileNameWithoutExtension(name);
      var ext = Path.GetExtension(name);
      while (!TryCreateFile(fullName = Path.Combine(path, string.Format("{0}.{1}{2}", fileName, ++i, ext)))) { }
      return (fullName);
    }

    /// <summary>
    /// Tries to create a new file.
    /// </summary>
    /// <param name="fileName">The file to create.</param>
    /// <returns><c>true</c> if the file didn't exist and was successfully created; otherwise, <c>false</c>.</returns>
    public static bool TryCreateFile(string fileName) {
      Contract.Requires(fileName != null);
      try {
        var fileHandle = File.Open(fileName, FileMode.CreateNew, FileAccess.Write);
        fileHandle.Close();
        return (true);
      } catch (IOException) {
        return (false);
      }
    }

    /// <summary>
    /// Generates a temporary directory which is most like the given one in the temporary folder.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
    /// <returns>The full path of the created temporary directory.</returns>
    public static string GetTempDirectoryName(string name = null, string baseDirectory = null) {
      var path = baseDirectory ?? Path.GetTempPath();

      // use a temp name if none given
      if (name == null) {
        const string PREFIX = "tmp";
        const int LENGTH = 4;
        const string SUFFIX = ".tmp";
        string result;
        var random = new Random();

        // loop until the temporarely generated name does not exist
        do {

          // generate a temporary name
          var tempName = new StringBuilder(PREFIX, LENGTH + PREFIX.Length);
          for (var j = LENGTH; j > 0; --j)
            tempName.Append(random.Next(0, 16).ToString("X"));

          tempName.Append(SUFFIX);
          result = Path.Combine(path, tempName.ToString());
          Contract.Assume(!string.IsNullOrEmpty(result));
        } while (!TryCreateDirectory(result));

        return (result);
      }

      // a name is given, so try to accommodate this
      name = Path.GetFileName(name);
      Contract.Assert(name != null, "DirectoryName went <null>");
      var fullName = Path.Combine(path, name);

      // if we could use the given name, return it
      Contract.Assume(!string.IsNullOrEmpty(fullName));
      if (TryCreateDirectory(fullName))
        return (fullName);

      // otherwise count up
      var i = 1;
      while (!TryCreateDirectory(fullName = Path.Combine(path, string.Format("{0}{1}", name, ++i)))) { }
      return (fullName);
    }

    /// <summary>
    /// Tries to create a new folder.
    /// </summary>
    /// <param name="pathName">The directory name.</param>
    /// <returns><c>true</c> when the folder didn't exist and was successfully created; otherwise, <c>false</c>.</returns>
    public static bool TryCreateDirectory(string pathName) {
      Contract.Requires(!string.IsNullOrEmpty(pathName));
      try {
        if (Directory.Exists(pathName))
          return (false);
        Directory.CreateDirectory(pathName);
        return (true);
      } catch (IOException) {
        return (false);
      }
    }

    /// <summary>
    /// This could contain a full network path eg. user:password@\\server\share\folder\filename.extension
    /// </summary>
    public struct NetworkPath {
      private const char _pathSeparator = '\\';
      private const char _userSeparator = '@';
      private const char _passSeparator = ':';

      private string _username;
      private string _password;
      private string _server;
      private string _share;
      private string _fullPath;
      private string _directory;
      private string _uncPath;

      public string Username {
        get { return (this._username); }
        set { this._username = value; this._InvalidateUnc(); }
      }
      public string Password {
        get { return (this._password); }
        set { this._password = value; this._InvalidateUnc(); }
      }
      public string Server { get { return (this._server); } set { this._server = value; this._InvalidateFullPath(); } }
      public string Share { get { return (this._share); } set { this._share = value; this._InvalidateFullPath(); } }
      public string DirectoryAndOrFileName { get { return (this._directory); } set { this._directory = value; this._InvalidateFullPath(); } }
      public string FullPath {
        get { return (this._fullPath); }
        set {
          this._fullPath = value;
          this._SplitPath();
          this._InvalidateUnc();
        }
      }
      public string UncPath {
        get { return (this._uncPath); }
        set {
          this._uncPath = value;
          this._SplitUnc();
        }
      }

      public NetworkPath(string uncPath) {
        this._username = this._password = this._server = this._share = this._fullPath = this._directory = this._uncPath = null;
        this.UncPath = uncPath;
      }

      private void _SplitPath() {
        var value = this._fullPath;
        // extract server
        if (value != null && value.StartsWith(_pathSeparator + string.Empty + _pathSeparator)) {
          Contract.Assume(value.Length > 2);
          var idx = value.IndexOf(_pathSeparator, 2);
          if (idx < 0) {
            this._server = value.Substring(2);
            value = null;
          } else {
            this._server = value.Substring(2, idx - 2);
            value = value.Substring(idx);
          }
        } else {
          this._server = null;
        }

        // extract share
        if (!string.IsNullOrEmpty(value) && value[0] == _pathSeparator) {
          Contract.Assume(value.Length > 1);
          var idx = value.IndexOf(_pathSeparator, 1);
          if (idx < 0) {
            this._share = value.Substring(1);
            value = null;
          } else {
            this._share = value.Substring(1, idx - 1);
            value = value.Substring(idx + 1);
          }
        } else {
          this._share = null;
        }

        this._directory = string.IsNullOrEmpty(value) ? null : value;
      }

      private void _SplitUnc() {
        var value = this._uncPath;
        string password;
        var user = password = null;

        var idx = value.IndexOf(_userSeparator);
        if (idx >= 0) {
          var userAndOrPassword = value.Substring(0, idx);
          value = value.Substring(idx + 1);
          idx = userAndOrPassword.IndexOf(_passSeparator);
          if (idx >= 0) {
            user = userAndOrPassword.Substring(0, idx);
            password = userAndOrPassword.Substring(idx + 1);
          } else {
            user = userAndOrPassword;
          }
        }

        this._username = string.IsNullOrEmpty(user) ? null : user;
        this._password = string.IsNullOrEmpty(password) ? null : password;
        this._fullPath = value;
        this._SplitPath();
      }

      private void _InvalidateFullPath() {
        var result = (this._server == null ? string.Empty : _pathSeparator + _pathSeparator + this._server);
        if (this._share != null)
          result += _pathSeparator + this._share;
        this._fullPath = result + (this._directory == null ? string.Empty : _pathSeparator + this._directory);
        this._InvalidateUnc();
      }

      private void _InvalidateUnc() {
        var result = string.Empty;
        if (this._username != null) {
          result += this._username;
          if (this._password != null)
            result += _passSeparator + this._password;
          result += _userSeparator;
        }

        this.UncPath = result + this._fullPath;
      }

    }
  }
}