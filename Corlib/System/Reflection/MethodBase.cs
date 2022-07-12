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

using System.Collections.Generic;
#if NET40_OR_GREATER
using System.Diagnostics.Contracts;
#endif
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Reflection {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static class MethodBaseExtensions {
    #region nested types

    public class ILInstruction {
      // Fields
      private OpCode code;
      private object operand;
      private int offset;

      // Properties
      public OpCode Code {
        get { return this.code; }
        set { this.code = value; }
      }

      public object Operand {
        get { return this.operand; }
        set { this.operand = value; }
      }

      public byte[] OperandData { get; set; }

      public int Offset {
        get { return this.offset; }
        set { this.offset = value; }
      }

      /// <summary>
      /// Returns a friendly strign representation of this instruction
      /// </summary>
      /// <returns></returns>
      public string GetCode() {
        var result = "";
        result += this.GetExpandedOffset(this.offset) + " : " + this.code;
        if (this.operand != null) {
          result += " ";
          switch (this.code.OperandType) {
            case OperandType.InlineField: {
                var fOperand = ((FieldInfo)this.operand);
                result += fOperand.FieldType + " " + fOperand.ReflectedType + "::" + fOperand.Name + "";
                break;
              }
            case OperandType.InlineMethod: {
                var mOperand = this.operand as MethodInfo;
                if (mOperand != null) {
                  if (!mOperand.IsStatic)
                    result += "instance ";

                  result += mOperand.ReturnType + " " + mOperand.ReflectedType + "::" + mOperand.Name + "()";
                } else {
                  var cOperand = (ConstructorInfo)this.operand;
                  result += " ";
                  if (!cOperand.IsStatic)
                    result += "instance ";
                  result += "void " + cOperand.ReflectedType + "::" + cOperand.Name + "()";
                }
                break;
              }
            case OperandType.ShortInlineBrTarget:
            case OperandType.InlineBrTarget: {
                result += this.GetExpandedOffset((int)this.operand);
                break;
              }
            case OperandType.InlineType: {
                result += this.operand;
                break;
              }
            case OperandType.InlineString: {
                if (this.operand.ToString() == "\r\n")
                  result += "\"\\r\\n\"";
                else
                  result += "\"" + this.operand + "\"";
                break;
              }
            case OperandType.ShortInlineVar: {
                result += this.operand.ToString();
                break;
              }
            case OperandType.InlineI:
            case OperandType.InlineI8:
            case OperandType.InlineR:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineR: {
                result += this.operand.ToString();
                break;
              }
            case OperandType.InlineTok: {
                if (this.operand is Type)
                  result += ((Type)this.operand).FullName;
                else
                  result += "not supported";
                break;
              }
            default: {
                result += "not supported";
                break;
              }
          }
        }
        return result;
      }

      /// <summary>
      /// Add enough zeros to a number as to be represented on 4 characters
      /// </summary>
      /// <param name="offset">
      /// The number that must be represented on 4 characters
      /// </param>
      /// <returns>
      /// </returns>
      private string GetExpandedOffset(long offset) {
        var result = offset.ToString();
        for (var i = 0; result.Length < 4; i++)
          result = "0" + result;
        return result;
      }

      public override string ToString() {
        return this.GetCode();
      }
    }

    #endregion

    #region opcode init

    private static OpCode[] _MULTI_BYTE_OP_CODES;
    private static OpCode[] _SINGLE_BYTE_OP_CODES;
    private static bool _isOpcodeListInited;

    /// <summary>
    /// Checks whether opcodes are loaded and loads them if needed.
    /// </summary>
    private static void _CheckOpcodeInit() {
      if (_isOpcodeListInited)
        return;

      _InitOpcodeList();
      _isOpcodeListInited = true;
    }

    /// <summary>
    /// Loads the opcode list on demand.
    /// </summary>
    private static void _InitOpcodeList() {
      _SINGLE_BYTE_OP_CODES = new OpCode[0x100];
      _MULTI_BYTE_OP_CODES = new OpCode[0x100];
      var fields = typeof(OpCodes).GetFields();
      foreach (var info1 in fields) {
        if (info1.FieldType != typeof(OpCode))
          continue;

        var code1 = (OpCode)info1.GetValue(null);
        var num2 = (ushort)code1.Value;
        if (num2 < 0x100)
          _SINGLE_BYTE_OP_CODES[num2] = code1;
        else {
          if ((num2 & 0xff00) != 0xfe00)
            throw new Exception("Invalid OpCode.");

          _MULTI_BYTE_OP_CODES[num2 & 0xff] = code1;
        }
      }
    }

    #endregion

    /// <summary>
    /// Gets the il instructions.
    /// </summary>
    /// <param name="This">This MethodBase.</param>
    /// <returns>A list of instructions.</returns>
    public static ILInstruction[] GetInstructions(this MethodBase This) {
#if NET40_OR_GREATER
      Contract.Requires(This != null);
#endif
      var body = This.GetMethodBody();
      if (body == null)
        return (null);

      // make sure opcodes are loaded
      _CheckOpcodeInit();

      var module = This.Module;
      var il = body.GetILAsByteArray();
      var position = 0;
      var result = new List<ILInstruction>();
      while (position < il.Length) {
        var instruction = new ILInstruction();

        // get the operation code of the current instruction
        OpCode code;
        ushort value = il[position++];
        if (value != 0xfe)
          code = _SINGLE_BYTE_OP_CODES[value];
        else {
          value = il[position++];
          code = _MULTI_BYTE_OP_CODES[value];
          value = (ushort)(value | 0xfe00);
        }
        instruction.Code = code;
        instruction.Offset = position - 1;

        // get the operand of the current operation
        position = _ReadOperand(This, code, position, il, module, instruction);
        result.Add(instruction);
      }
      return (result.ToArray());
    }

    private static int _ReadOperand(MethodBase This, OpCode code, int position, byte[] il, Module module, ILInstruction instruction) {
      int metadataToken;
      switch (code.OperandType) {
        case OperandType.InlineBrTarget: {
            metadataToken = ReadInt32(il, ref position);
            metadataToken += position;
            instruction.Operand = metadataToken;
            break;
          }
        case OperandType.InlineField: {
            metadataToken = ReadInt32(il, ref position);
            instruction.Operand = module.ResolveField(metadataToken);
            break;
          }
        case OperandType.InlineMethod: {
            metadataToken = ReadInt32(il, ref position);
            try {
              instruction.Operand = module.ResolveMethod(metadataToken);
            } catch {
              instruction.Operand = module.ResolveMember(metadataToken);
            }
            break;
          }
        case OperandType.InlineSig: {
            metadataToken = ReadInt32(il, ref position);
            instruction.Operand = module.ResolveSignature(metadataToken);
            break;
          }
        case OperandType.InlineTok: {
            metadataToken = ReadInt32(il, ref position);
            try {
              instruction.Operand = module.ResolveType(metadataToken);
            } catch { }
            // SSS : see what to do here
            break;
          }
        case OperandType.InlineType: {
            metadataToken = ReadInt32(il, ref position);
            // now we call the ResolveType always using the generic attributes type in order
            // to support decompilation of generic methods and classes

            // thanks to the guys from code project who commented on this missing feature

            instruction.Operand = module.ResolveType(metadataToken, This.DeclaringType.GetGenericArguments(), This.GetGenericArguments());
            break;
          }
        case OperandType.InlineI: {
            instruction.Operand = ReadInt32(il, ref position);
            break;
          }
        case OperandType.InlineI8: {
            instruction.Operand = ReadInt64(il, ref position);
            break;
          }
        case OperandType.InlineNone: {
            instruction.Operand = null;
            break;
          }
        case OperandType.InlineR: {
            instruction.Operand = ReadDouble(il, ref position);
            break;
          }
        case OperandType.InlineString: {
            metadataToken = ReadInt32(il, ref position);
            instruction.Operand = module.ResolveString(metadataToken);
            break;
          }
        case OperandType.InlineSwitch: {
            var count = ReadInt32(il, ref position);
            var casesAddresses = new int[count];
            for (var i = 0; i < count; i++)
              casesAddresses[i] = ReadInt32(il, ref position);

            var cases = new int[count];
            for (var i = 0; i < count; i++)
              cases[i] = position + casesAddresses[i];

            break;
          }
        case OperandType.InlineVar: {
            instruction.Operand = ReadUInt16(il, ref position);
            break;
          }
        case OperandType.ShortInlineBrTarget: {
            instruction.Operand = ReadSByte(il, ref position) + position;
            break;
          }
        case OperandType.ShortInlineI: {
            if (instruction.Code == OpCodes.Ldc_I4_S)
              instruction.Operand = ReadSByte(il, ref position);
            else
              instruction.Operand = ReadByte(il, ref position);
            break;
          }
        case OperandType.ShortInlineR: {
            instruction.Operand = ReadSingle(il, ref position);
            break;
          }
        case OperandType.ShortInlineVar: {
            instruction.Operand = ReadByte(il, ref position);
            break;
          }
        default: {
            throw new Exception("Unknown operand type.");
          }
      }
      return position;
    }

    #region il read methods

    private static ushort ReadUInt16(byte[] il, ref int position) {
      var result = BitConverter.ToUInt16(il, position);
      position += 2;
      return (result);
    }

    private static int ReadInt32(byte[] il, ref int position) {
      var result = BitConverter.ToInt32(il, position);
      position += 4;
      return (result);
    }

    private static long ReadInt64(byte[] il, ref int position) {
      var result = BitConverter.ToInt64(il, position);
      position += 8;
      return (result);
    }

    private static double ReadDouble(byte[] il, ref int position) {
      var result = BitConverter.ToDouble(il, position);
      position += 8;
      return (result);
    }

    private static sbyte ReadSByte(byte[] il, ref int position) {
      return (sbyte)il[position++];
    }

    private static byte ReadByte(byte[] il, ref int position) {
      return il[position++];
    }

    private static Single ReadSingle(byte[] il, ref int position) {
      var result = BitConverter.ToSingle(il, position);
      position += 4;
      return (result);
    }

    #endregion

    /// <summary>
    /// Determines whether this method is compiler generated or not.
    /// </summary>
    /// <param name="This">This MethodBase.</param>
    /// <returns>
    ///   <c>true</c> if the given method was compiler generated; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsCompilerGenerated(this MethodBase This) {
#if NET40_OR_GREATER
      Contract.Requires(This != null);
#endif
      var customAttributes = This.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
      return (customAttributes.Length > 0);
    }

    /// <summary>
    /// Determines whether this method is a getter or setter.
    /// </summary>
    /// <param name="This">This MethodBase.</param>
    /// <returns>
    ///   <c>true</c> if this is a getter or setter; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsGetterOrSetter(this MethodBase This) {
#if NET40_OR_GREATER
      Contract.Requires(This != null);
#endif
      var name = This.Name;
      return ((name.StartsWith("get_") || name.StartsWith("set_")) && (This.IsCompilerGenerated() || This.IsSpecialName));
    }
  }
}