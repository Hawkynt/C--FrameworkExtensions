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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

using System.Text;
using word = System.UInt16;
using dword = System.UInt32;
using qword = System.UInt64;
namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class Int32Extensions {
    public static void Times(this int This,Action action) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(action!=null);
#endif
      while(This-->0)
        action();
    }
    
    public static void Times(this int This,Action<int> action) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(action!=null);
#endif
      for(var i=0;i<This;i++)
        action(i);
    }
    
    public static string Times(this int This,string text) {
      if(text==null) 
        return(text);
        
      var result=new StringBuilder(text.Length*This);
      for(var i=0;i<This;i++)
        result.Append(text);
        
      return(result.ToString());
    }
  }
}