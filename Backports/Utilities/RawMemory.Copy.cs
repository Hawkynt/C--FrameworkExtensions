﻿// This file is part of Hawkynt's .NET Framework extensions.
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
//

namespace Utilities;

unsafe partial class RawMemory {
  public static void CopyWithoutChecks(byte* source, byte* target, uint bytes) {
#if SUPPORTS_BUFFER_MEMORYCOPY
    System.Buffer.MemoryCopy(source, target, bytes, bytes);
#else
    CopyBytes(source, target, bytes);
#endif

    return;
    
#pragma warning disable CS8321 // Local function is declared but never used
    static void CopyBytes(byte* source, byte* target, uint totalBytes) {
#pragma warning restore CS8321 // Local function is declared but never used

      // Copy chunks of 64 * 8 = 512 bytes at a time
      for (; totalBytes >= 512; source += 512, target += 512, totalBytes -= 512) {
        *(Block64*)target = *(Block64*)source;
        ((Block64*)target)[1] = ((Block64*)source)[1];
        ((Block64*)target)[2] = ((Block64*)source)[2];
        ((Block64*)target)[3] = ((Block64*)source)[3];
        ((Block64*)target)[4] = ((Block64*)source)[4];
        ((Block64*)target)[5] = ((Block64*)source)[5];
        ((Block64*)target)[6] = ((Block64*)source)[6];
        ((Block64*)target)[7] = ((Block64*)source)[7];
      }

      // count is < 512 from here on
      var iterations64 = totalBytes / 64;
      switch (iterations64) {
        case 0:
          goto CopyLessThan64;
        case 1:
          goto Copy64;
        case 2:
          goto Copy128;
        case 3:
          goto Copy192;
        case 4:
          goto Copy256;
        case 5:
          goto Copy320;
        case 6:
          goto Copy384;
        case 7:
          goto Copy448;
        default:
          goto CopyLessThan64; // Avoid compiler warning and trigger optimization - Never gonna get here
      }

Copy448:
      ((Block64*)target)[6] = ((Block64*)source)[6];
Copy384:
      ((Block64*)target)[5] = ((Block64*)source)[5];
Copy320:
      ((Block64*)target)[4] = ((Block64*)source)[4];
Copy256:
      ((Block64*)target)[3] = ((Block64*)source)[3];
Copy192:
      ((Block64*)target)[2] = ((Block64*)source)[2];
Copy128:
      ((Block64*)target)[1] = ((Block64*)source)[1];
Copy64:
      *(Block64*)target = *(Block64*)source;

      iterations64 *= 64;
      source += iterations64;
      target += iterations64;
      totalBytes -= iterations64;

CopyLessThan64:
// count is < 64 from here on
      switch (totalBytes) {
        case 0:
          goto CopyDone;
        case 1:
          goto Copy1;
        case 2:
          goto Copy2;
        case 3:
          goto Copy3;
        case 4:
          goto Copy4;
        case 5:
          goto Copy5;
        case 6:
          goto Copy6;
        case 7:
          goto Copy7;
        case 8:
          goto Copy8;
        case 9:
          goto Copy9;
        case 10:
          goto Copy10;
        case 11:
          goto Copy11;
        case 12:
          goto Copy12;
        case 13:
          goto Copy13;
        case 14:
          goto Copy14;
        case 15:
          goto Copy15;
        case 16:
          goto Copy16;
        case 17:
          goto Copy17;
        case 18:
          goto Copy18;
        case 19:
          goto Copy19;
        case 20:
          goto Copy20;
        case 21:
          goto Copy21;
        case 22:
          goto Copy22;
        case 23:
          goto Copy23;
        case 24:
          goto Copy24;
        case 25:
          goto Copy25;
        case 26:
          goto Copy26;
        case 27:
          goto Copy27;
        case 28:
          goto Copy28;
        case 29:
          goto Copy29;
        case 30:
          goto Copy30;
        case 31:
          goto Copy31;
        case 32:
          goto Copy32;
        case 33:
          goto Copy33;
        case 34:
          goto Copy34;
        case 35:
          goto Copy35;
        case 36:
          goto Copy36;
        case 37:
          goto Copy37;
        case 38:
          goto Copy38;
        case 39:
          goto Copy39;
        case 40:
          goto Copy40;
        case 41:
          goto Copy41;
        case 42:
          goto Copy42;
        case 43:
          goto Copy43;
        case 44:
          goto Copy44;
        case 45:
          goto Copy45;
        case 46:
          goto Copy46;
        case 47:
          goto Copy47;
        case 48:
          goto Copy48;
        case 49:
          goto Copy49;
        case 50:
          goto Copy50;
        case 51:
          goto Copy51;
        case 52:
          goto Copy52;
        case 53:
          goto Copy53;
        case 54:
          goto Copy54;
        case 55:
          goto Copy55;
        case 56:
          goto Copy56;
        case 57:
          goto Copy57;
        case 58:
          goto Copy58;
        case 59:
          goto Copy59;
        case 60:
          goto Copy60;
        case 61:
          goto Copy61;
        case 62:
          goto Copy62;
        case 63:
          goto Copy63;
        default:
          goto CopyDone;
      }

Copy63:
      target[62] = source[62];
Copy62:
      *(Block2*)(target + 60) = *(Block2*)(source + 60);
      goto Copy60;
Copy61:
      target[60] = source[60];
Copy60:
      *(Block4*)(target + 56) = *(Block4*)(source + 56);
      goto Copy56;
Copy59:
      target[58] = source[58];
Copy58:
      *(Block2*)(target + 56) = *(Block2*)(source + 56);
      goto Copy56;
Copy57:
      target[56] = source[56];
Copy56:
      *(Block8*)(target + 48) = *(Block8*)(source + 48);
      goto Copy48;
Copy55:
      target[54] = source[54];
Copy54:
      *(Block2*)(target + 52) = *(Block2*)(source + 52);
      goto Copy52;
Copy53:
      target[52] = source[52];
Copy52:
      *(Block4*)(target + 48) = *(Block4*)(source + 48);
      goto Copy48;
Copy51:
      target[50] = source[50];
Copy50:
      *(Block2*)(target + 48) = *(Block2*)(source + 48);
      goto Copy48;
Copy49:
      target[48] = source[48];
Copy48:
      *(Block16*)(target + 32) = *(Block16*)(source + 32);
      goto Copy32;
Copy47:
      target[46] = source[46];
Copy46:
      *(Block2*)(target + 44) = *(Block2*)(source + 44);
      goto Copy44;
Copy45:
      target[44] = source[44];
Copy44:
      *(Block4*)(target + 40) = *(Block4*)(source + 40);
      goto Copy40;
Copy43:
      target[42] = source[42];
Copy42:
      *(Block2*)(target + 40) = *(Block2*)(source + 40);
      goto Copy40;
Copy41:
      target[40] = source[40];
Copy40:
      *(Block8*)(target + 32) = *(Block8*)(source + 32);
      goto Copy32;
Copy39:
      target[38] = source[38];
Copy38:
      *(Block2*)(target + 36) = *(Block2*)(source + 36);
      goto Copy36;
Copy37:
      target[36] = source[36];
Copy36:
      *(Block4*)(target + 32) = *(Block4*)(source + 32);
      goto Copy32;
Copy35:
      target[34] = source[34];
Copy34:
      *(Block2*)(target + 32) = *(Block2*)(source + 32);
      goto Copy32;
Copy33:
      target[32] = source[32];
Copy32:
      *(Block32*)target = *(Block32*)source;
      goto CopyDone;
Copy31:
      target[30] = source[30];
Copy30:
      *(Block2*)(target + 28) = *(Block2*)(source + 28);
      goto Copy28;
Copy29:
      target[28] = source[28];
Copy28:
      *(Block4*)(target + 24) = *(Block4*)(source + 24);
      goto Copy24;
Copy27:
      target[26] = source[26];
Copy26:
      *(Block2*)(target + 24) = *(Block2*)(source + 24);
      goto Copy24;
Copy25:
      target[24] = source[24];
Copy24:
      *(Block8*)(target + 16) = *(Block8*)(source + 16);
      goto Copy16;
Copy23:
      target[22] = source[22];
Copy22:
      *(Block2*)(target + 20) = *(Block2*)(source + 20);
      goto Copy20;
Copy21:
      target[20] = source[20];
Copy20:
      *(Block4*)(target + 16) = *(Block4*)(source + 16);
      goto Copy16;
Copy19:
      target[18] = source[18];
Copy18:
      *(Block2*)(target + 16) = *(Block2*)(source + 16);
      goto Copy16;
Copy17:
      target[16] = source[16];
Copy16:
      *(Block16*)target = *(Block16*)source;
      goto CopyDone;
Copy15:
      target[14] = source[14];
Copy14:
      *(Block2*)(target + 12) = *(Block2*)(source + 12);
      goto Copy12;
Copy13:
      target[12] = source[12];
Copy12:
      *(Block4*)(target + 8) = *(Block4*)(source + 8);
      goto Copy8;
Copy11:
      target[10] = source[10];
Copy10:
      *(Block2*)(target + 8) = *(Block2*)(source + 8);
      goto Copy8;
Copy9:
      target[8] = source[8];
Copy8:
      *(Block8*)target = *(Block8*)source;
      goto CopyDone;
Copy7:
      target[6] = source[6];
Copy6:
      *(Block2*)(target + 4) = *(Block2*)(source + 4);
      goto Copy4;
Copy5:
      target[4] = source[4];
Copy4:
      *(Block4*)target = *(Block4*)source;
      goto CopyDone;
Copy3:
      target[2] = source[2];
Copy2:
      *(Block2*)target = *(Block2*)source;
      goto CopyDone;
Copy1:
      *target = *source;
CopyDone:
      ;
    }


  }
}
