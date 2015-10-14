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
  public class Adler32 : HashAlgorithm {

    private const ushort _DIVISOR = 65521;

    public Adler32() {
      this.Initialize();
    }

    private ushort _state;
    private ushort _sum;

    #region Overrides of HashAlgorithm

    public override sealed void Initialize() {
      this._state = 1;
      this._sum = 0;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
      for (var i = ibStart; i < ibStart + cbSize; ++i)
        _sum = (ushort)((_sum + (_state = (ushort)((_state + array[i]) % _DIVISOR))) % _DIVISOR);
    }

    protected override byte[] HashFinal() {
      return (BitConverter.GetBytes(_sum << 16 | _state));
    }

    #endregion
  }
}
