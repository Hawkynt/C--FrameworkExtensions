#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
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

#endregion

using ET = System.Linq.Expressions.ExpressionType;

namespace Utilities;

/// <summary>
/// ExpressionType bridge that provides all enum values across all target frameworks.
/// - net20: Uses our full polyfill directly
/// - net35: Uses BCL for 0-45, raw integer casts for 46-84
/// - net40+: Uses BCL for all values
/// </summary>
internal static class ExpressionType {

  // All frameworks have these (BCL for net35+, our polyfill for net20)
  public const ET Add = ET.Add;                                     // 0
  public const ET AddChecked = ET.AddChecked;                       // 1
  public const ET And = ET.And;                                     // 2
  public const ET AndAlso = ET.AndAlso;                             // 3
  public const ET ArrayLength = ET.ArrayLength;                     // 4
  public const ET ArrayIndex = ET.ArrayIndex;                       // 5
  public const ET Call = ET.Call;                                   // 6
  public const ET Coalesce = ET.Coalesce;                           // 7
  public const ET Conditional = ET.Conditional;                     // 8
  public const ET Constant = ET.Constant;                           // 9
  public const ET Convert = ET.Convert;                             // 10
  public const ET ConvertChecked = ET.ConvertChecked;               // 11
  public const ET Divide = ET.Divide;                               // 12
  public const ET Equal = ET.Equal;                                 // 13
  public const ET ExclusiveOr = ET.ExclusiveOr;                     // 14
  public const ET GreaterThan = ET.GreaterThan;                     // 15
  public const ET GreaterThanOrEqual = ET.GreaterThanOrEqual;       // 16
  public const ET Invoke = ET.Invoke;                               // 17
  public const ET Lambda = ET.Lambda;                               // 18
  public const ET LeftShift = ET.LeftShift;                         // 19
  public const ET LessThan = ET.LessThan;                           // 20
  public const ET LessThanOrEqual = ET.LessThanOrEqual;             // 21
  public const ET ListInit = ET.ListInit;                           // 22
  public const ET MemberAccess = ET.MemberAccess;                   // 23
  public const ET MemberInit = ET.MemberInit;                       // 24
  public const ET Modulo = ET.Modulo;                               // 25
  public const ET Multiply = ET.Multiply;                           // 26
  public const ET MultiplyChecked = ET.MultiplyChecked;             // 27
  public const ET Negate = ET.Negate;                               // 28
  public const ET UnaryPlus = ET.UnaryPlus;                         // 29
  public const ET NegateChecked = ET.NegateChecked;                 // 30
  public const ET New = ET.New;                                     // 31
  public const ET NewArrayInit = ET.NewArrayInit;                   // 32
  public const ET NewArrayBounds = ET.NewArrayBounds;               // 33
  public const ET Not = ET.Not;                                     // 34
  public const ET NotEqual = ET.NotEqual;                           // 35
  public const ET Or = ET.Or;                                       // 36
  public const ET OrElse = ET.OrElse;                               // 37
  public const ET Parameter = ET.Parameter;                         // 38
  public const ET Power = ET.Power;                                 // 39
  public const ET Quote = ET.Quote;                                 // 40
  public const ET RightShift = ET.RightShift;                       // 41
  public const ET Subtract = ET.Subtract;                           // 42
  public const ET SubtractChecked = ET.SubtractChecked;             // 43
  public const ET TypeAs = ET.TypeAs;                               // 44
  public const ET TypeIs = ET.TypeIs;                               // 45

  // net40+ values (46-84)
  // - net20: Our polyfill has them
  // - net40+: BCL has them
  // - net35: Only framework needing raw casts
#if !SUPPORTS_LINQ || SUPPORTS_EXPRESSION_VISITOR
  // net20 (polyfill) or net40+ (BCL) - enum values exist
  public const ET Assign = ET.Assign;                               // 46
  public const ET Block = ET.Block;                                 // 47
  public const ET DebugInfo = ET.DebugInfo;                         // 48
  public const ET Decrement = ET.Decrement;                         // 49
  public const ET Dynamic = ET.Dynamic;                             // 50
  public const ET Default = ET.Default;                             // 51
  public const ET Extension = ET.Extension;                         // 52
  public const ET Goto = ET.Goto;                                   // 53
  public const ET Increment = ET.Increment;                         // 54
  public const ET Index = ET.Index;                                 // 55
  public const ET Label = ET.Label;                                 // 56
  public const ET RuntimeVariables = ET.RuntimeVariables;           // 57
  public const ET Loop = ET.Loop;                                   // 58
  public const ET Switch = ET.Switch;                               // 59
  public const ET Throw = ET.Throw;                                 // 60
  public const ET Try = ET.Try;                                     // 61
  public const ET Unbox = ET.Unbox;                                 // 62
  public const ET AddAssign = ET.AddAssign;                         // 63
  public const ET AndAssign = ET.AndAssign;                         // 64
  public const ET DivideAssign = ET.DivideAssign;                   // 65
  public const ET ExclusiveOrAssign = ET.ExclusiveOrAssign;         // 66
  public const ET LeftShiftAssign = ET.LeftShiftAssign;             // 67
  public const ET ModuloAssign = ET.ModuloAssign;                   // 68
  public const ET MultiplyAssign = ET.MultiplyAssign;               // 69
  public const ET OrAssign = ET.OrAssign;                           // 70
  public const ET PowerAssign = ET.PowerAssign;                     // 71
  public const ET RightShiftAssign = ET.RightShiftAssign;           // 72
  public const ET SubtractAssign = ET.SubtractAssign;               // 73
  public const ET AddAssignChecked = ET.AddAssignChecked;           // 74
  public const ET MultiplyAssignChecked = ET.MultiplyAssignChecked; // 75
  public const ET SubtractAssignChecked = ET.SubtractAssignChecked; // 76
  public const ET PreIncrementAssign = ET.PreIncrementAssign;       // 77
  public const ET PreDecrementAssign = ET.PreDecrementAssign;       // 78
  public const ET PostIncrementAssign = ET.PostIncrementAssign;     // 79
  public const ET PostDecrementAssign = ET.PostDecrementAssign;     // 80
  public const ET TypeEqual = ET.TypeEqual;                         // 81
  public const ET OnesComplement = ET.OnesComplement;               // 82
  public const ET IsTrue = ET.IsTrue;                               // 83
  public const ET IsFalse = ET.IsFalse;                             // 84
#else
  // net35 only - BCL lacks these values, use raw casts
  public const ET Assign = (ET)46;
  public const ET Block = (ET)47;
  public const ET DebugInfo = (ET)48;
  public const ET Decrement = (ET)49;
  public const ET Dynamic = (ET)50;
  public const ET Default = (ET)51;
  public const ET Extension = (ET)52;
  public const ET Goto = (ET)53;
  public const ET Increment = (ET)54;
  public const ET Index = (ET)55;
  public const ET Label = (ET)56;
  public const ET RuntimeVariables = (ET)57;
  public const ET Loop = (ET)58;
  public const ET Switch = (ET)59;
  public const ET Throw = (ET)60;
  public const ET Try = (ET)61;
  public const ET Unbox = (ET)62;
  public const ET AddAssign = (ET)63;
  public const ET AndAssign = (ET)64;
  public const ET DivideAssign = (ET)65;
  public const ET ExclusiveOrAssign = (ET)66;
  public const ET LeftShiftAssign = (ET)67;
  public const ET ModuloAssign = (ET)68;
  public const ET MultiplyAssign = (ET)69;
  public const ET OrAssign = (ET)70;
  public const ET PowerAssign = (ET)71;
  public const ET RightShiftAssign = (ET)72;
  public const ET SubtractAssign = (ET)73;
  public const ET AddAssignChecked = (ET)74;
  public const ET MultiplyAssignChecked = (ET)75;
  public const ET SubtractAssignChecked = (ET)76;
  public const ET PreIncrementAssign = (ET)77;
  public const ET PreDecrementAssign = (ET)78;
  public const ET PostIncrementAssign = (ET)79;
  public const ET PostDecrementAssign = (ET)80;
  public const ET TypeEqual = (ET)81;
  public const ET OnesComplement = (ET)82;
  public const ET IsTrue = (ET)83;
  public const ET IsFalse = (ET)84;
#endif

}
