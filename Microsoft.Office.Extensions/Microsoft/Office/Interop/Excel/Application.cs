#region (c)2010-2042 Hawkynt

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
#if !NETSTANDARD

using System.IO;

namespace Microsoft.Office.Interop.Excel {
  /// <summary>
  ///   This class contains all constants, that are needed to access Excel Files.
  /// </summary>
  public static partial class ApplicationExtensions {

    /// <summary>
    ///   True to open the workbook in read-only mode.
    /// </summary>

    public enum Mode { ReadOnly = 0, ReadWrite = 1 }

    /// <summary>
    ///   True to have Microsoft Excel not display the read-only
    ///   recommended message (if the workbook was saved with the
    ///   Read-Only Recommended option).
    /// </summary>

    public enum IgnoreReadOnly { IgnoreReadOnlyRecommended, DontIgnoreReadOnlyRecommended }

    /// <summary>
    ///   If the file is a Microsoft Excel 4.0 add-in, this argument
    ///   is True to open the add-in so that its a visible window.
    ///   If this argument is False or omitted, the add-in is opened
    ///   as hidden, and it cannot be unhidden. This option doesn't
    ///   apply to add-ins created in Microsoft Excel 5.0 or later.
    ///   If the file is an Excel template, True to open the specified
    ///   template for editing. False to open a new workbook based on
    ///   the specified template. The default value is False.
    /// </summary>

    public enum Edit { NotEditable, Editable }

    /// <summary>
    ///   If the file cannot be opened in read/write mode, this
    ///   argument is True to add the file to the file notification
    ///   list. Microsoft Excel will open the file as read-only, poll
    ///   the file notification list, and then notify the user when
    ///   the file becomes available. If this argument is False or
    ///   omitted, no notification is requested, and any attempts to
    ///   open an unavailable file will fail.
    /// </summary>

    public enum Notify {
      DontNotify = 0,
      Notify = 1
    }

    /// <summary>
    ///   True to add this workbook to the list of recently used files.
    ///   The default value is False.
    /// </summary>

    public enum AddToMru {DontAddToMru, AddToMru}

  /// <summary>
    ///   True saves files against the language of Microsoft Excel
    ///   (including control panel settings). False (default) saves
    ///   files against the language of Visual Basic for Applications
    ///   (VBA) (which is typically US English unless the VBA project
    ///   where Workbooks.Open is run from is an old internationalized
    ///   XL5/95 VBA project).
    /// </summary>

    public enum Local { NotLocal, Local}

    public enum CorruptLoad { NormalLoad = 0, RepairFile = 1, ExtractData = 2 };

    /// <summary>
    ///   Specifies the way links in the file are updated. If this
    ///   argument is omitted, the user is prompted to specify how
    ///   links will be updated. Otherwise, this argument is one of
    ///   the values listed in the following table.
    /// </summary>
    public enum UpdateLinks { DontUpdate = 0, ExternalOnly = 1, RemoteOnly = 2, ExternalAndRemote = 3 }


    /// <summary>
    ///   If Microsoft Excel is opening a text file, this argument
    ///   specifies the delimiter character, as shown in the following
    ///   table. If this argument is omitted, the current delimiter
    ///   is used.
    /// </summary>
    public enum Format { Tabs = 1, Commas = 2, Spaces = 3, Semicolons = 4, Nothing = 5, CustomCharacter = 6 }

    /// <summary>
    ///   The index of the first file converter to try when opening
    ///   the file. The specified file converter is tried first; if
    ///   this converter doesnt recognize the file, all other converters
    ///   are tried. The converter index consists of the row numbers
    ///   of the converters returned by the FileConverters property.
    /// </summary>
    public enum Converter { Default = 0 };

    public static Workbook OpenWorkbook(
      this Application @this,
      FileInfo file,
      UpdateLinks updateLinks = UpdateLinks.DontUpdate,
      Mode openMode = Mode.ReadWrite,
      Format format = Format.Nothing,
      string openPassword = null,
      string writePassword = null,
      IgnoreReadOnly ignoreReadOnly = IgnoreReadOnly.IgnoreReadOnlyRecommended,
      XlPlatform xlPlatform = XlPlatform.xlWindows,
      char delimiter = ',',
      Edit editable = Edit.NotEditable,
      Notify notify = Notify.DontNotify,
      Converter converter = Converter.Default,
      AddToMru addToMru = AddToMru.DontAddToMru,
      Local local = Local.Local,
      CorruptLoad corruptLoad = CorruptLoad.NormalLoad)

      =>@this.Workbooks.Open(
          file.FullName, 
          updateLinks, 
          openMode, 
          format, 
          openPassword??string.Empty, 
          writePassword??string.Empty, 
          ignoreReadOnly, 
          xlPlatform,
          delimiter, 
          editable, 
          notify, 
          converter, 
          addToMru, 
          local, 
          corruptLoad
        );
  }
}
#endif