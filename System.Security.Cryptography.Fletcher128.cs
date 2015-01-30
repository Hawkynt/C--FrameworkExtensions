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

using System.Linq;

namespace System.Security.Cryptography {
  /// <summary>
  /// </summary>
  public class Fletcher128 : HashAlgorithm {

    public Fletcher128() {
      this.Initialize();
    }

    private ulong _state;
    private ulong _sum;

    #region Overrides of HashAlgorithm

    public override void Initialize() {
      this._state = 0; 
      this._sum = 0;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
      for (var i = ibStart; i < ibStart + cbSize; ++i)
        _sum += (_state += array[i]);
    }

    protected override byte[] HashFinal() {
      return (
        BitConverter.GetBytes(_state)
        .Concat(
          BitConverter.GetBytes(_sum)
        )
        .ToArray()
      );
    }

    #endregion
  }
}
