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


namespace System.Security.Cryptography {
  /// <summary>
  /// </summary>
  public class JavaHash32 : HashAlgorithm {

    public JavaHash32() {
      this.Initialize();
    }

    private uint _state;

    #region Overrides of HashAlgorithm

    public override void Initialize() {
      this._state = 0;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
      for (var i = ibStart; i < ibStart + cbSize; ++i)
        this._state = this._state * 0x1f + array[i];
    }

    protected override byte[] HashFinal() {
      return BitConverter.GetBytes(this._state);
    }

    #endregion
  }
}
