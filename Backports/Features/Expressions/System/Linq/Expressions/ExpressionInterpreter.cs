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

// ExpressionInterpreter is internal infrastructure for our expression polyfill
// Only needed for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Interprets expression trees and creates delegates from them.
/// </summary>
internal sealed class ExpressionInterpreter {

  private readonly LambdaExpression _lambda;
  private readonly Dictionary<ParameterExpression, int> _parameterIndices = [];
  private readonly Dictionary<LabelTarget, object?> _labelValues = [];
  private LabelTarget? _gotoTarget;

  public ExpressionInterpreter(LambdaExpression lambda) {
    this._lambda = lambda;
    for (var i = 0; i < lambda.Parameters.Count; ++i)
      this._parameterIndices[lambda.Parameters[i]] = i;
  }

  public Delegate CreateDelegate() {
    var paramCount = this._lambda.Parameters.Count;
    var returnType = this._lambda.ReturnType;

    // Create appropriate delegate based on signature
    if (returnType == typeof(void)) {
      return paramCount switch {
        0 => new Action(() => Interpret([])),
        1 => CreateAction1(),
        2 => CreateAction2(),
        3 => CreateAction3(),
        4 => CreateAction4(),
        _ => throw new NotSupportedException($"Lambda with {paramCount} parameters is not supported.")
      };
    }

    return paramCount switch {
      0 => CreateFunc0(),
      1 => CreateFunc1(),
      2 => CreateFunc2(),
      3 => CreateFunc3(),
      4 => CreateFunc4(),
      _ => throw new NotSupportedException($"Lambda with {paramCount} parameters is not supported.")
    };
  }

  private object? Interpret(object?[] args) => Evaluate(this._lambda.Body, args);

  private object? Evaluate(Expression expr, object?[] args) {
    return expr switch {
      ConstantExpression constant => constant.Value,
      ParameterExpression param => args[this._parameterIndices[param]],
      BinaryExpression binary => EvaluateBinary(binary, args),
      UnaryExpression unary => EvaluateUnary(unary, args),
      MethodCallExpression call => EvaluateMethodCall(call, args),
      MemberExpression member => EvaluateMember(member, args),
      ConditionalExpression cond => EvaluateConditional(cond, args),
      NewExpression newExpr => EvaluateNew(newExpr, args),
      NewArrayExpression newArray => EvaluateNewArray(newArray, args),
      InvocationExpression invocation => EvaluateInvocation(invocation, args),
      TypeBinaryExpression typeBinary => EvaluateTypeBinary(typeBinary, args),
      DefaultExpression => GetDefaultValue(expr.Type),
      LambdaExpression lambda => new ExpressionInterpreter(lambda).CreateDelegate(),
      BlockExpression block => EvaluateBlock(block, args),
      IndexExpression index => EvaluateIndex(index, args),
      ListInitExpression listInit => EvaluateListInit(listInit, args),
      MemberInitExpression memberInit => EvaluateMemberInit(memberInit, args),
      SwitchExpression switchExpr => EvaluateSwitch(switchExpr, args),
      LoopExpression loop => EvaluateLoop(loop, args),
      LabelExpression label => EvaluateLabel(label, args),
      GotoExpression gotoExpr => EvaluateGoto(gotoExpr, args),
      TryExpression tryExpr => EvaluateTry(tryExpr, args),
      _ => throw new NotSupportedException($"Expression type {expr.NodeType} is not supported for interpretation.")
    };
  }

  private object? EvaluateBinary(BinaryExpression binary, object?[] args) {
    // Handle assignment expressions
    if (binary.NodeType == ExpressionType.Assign)
      return EvaluateAssign(binary.Left, Evaluate(binary.Right, args), args);

    // Handle compound assignment expressions
    if (IsCompoundAssignment(binary.NodeType)) {
      var leftValue = Evaluate(binary.Left, args);
      var rightValue = Evaluate(binary.Right, args);
      var result = EvaluateCompoundAssignment(binary.NodeType, leftValue, rightValue);
      return EvaluateAssign(binary.Left, result, args);
    }

    var left = Evaluate(binary.Left, args);

    // Short-circuit evaluation for AndAlso and OrElse
    if (binary.NodeType == ExpressionType.AndAlso) {
      if (left is false)
        return false;
      return Evaluate(binary.Right, args);
    }

    if (binary.NodeType == ExpressionType.OrElse) {
      if (left is true)
        return true;
      return Evaluate(binary.Right, args);
    }

    // Coalesce
    if (binary.NodeType == ExpressionType.Coalesce)
      return left ?? Evaluate(binary.Right, args);

    var right = Evaluate(binary.Right, args);

    // Array index
    if (binary.NodeType == ExpressionType.ArrayIndex) {
      var array = (Array)left!;
      var index = System.Convert.ToInt32(right);
      return array.GetValue(index);
    }

    // Use method if provided
    if (binary.Method != null)
      return binary.Method.Invoke(null, [left, right]);

    return binary.NodeType switch {
      ExpressionType.Add => Add(left, right),
      ExpressionType.AddChecked => checked(Add(left, right)),
      ExpressionType.Subtract => Subtract(left, right),
      ExpressionType.SubtractChecked => checked(Subtract(left, right)),
      ExpressionType.Multiply => Multiply(left, right),
      ExpressionType.MultiplyChecked => checked(Multiply(left, right)),
      ExpressionType.Divide => Divide(left, right),
      ExpressionType.Modulo => Modulo(left, right),
      ExpressionType.And => And(left, right),
      ExpressionType.Or => Or(left, right),
      ExpressionType.ExclusiveOr => Xor(left, right),
      ExpressionType.LeftShift => LeftShift(left, right),
      ExpressionType.RightShift => RightShift(left, right),
      ExpressionType.Equal => Equals(left, right),
      ExpressionType.NotEqual => !Equals(left, right),
      ExpressionType.LessThan => Compare(left, right) < 0,
      ExpressionType.LessThanOrEqual => Compare(left, right) <= 0,
      ExpressionType.GreaterThan => Compare(left, right) > 0,
      ExpressionType.GreaterThanOrEqual => Compare(left, right) >= 0,
      ExpressionType.Power => Math.Pow(System.Convert.ToDouble(left), System.Convert.ToDouble(right)),
      _ => throw new NotSupportedException($"Binary operator {binary.NodeType} is not supported.")
    };
  }

  private object? EvaluateUnary(UnaryExpression unary, object?[] args) {
    // Handle throw specially
    if (unary.NodeType == ExpressionType.Throw) {
      var exception = Evaluate(unary.Operand, args) as Exception;
      throw exception ?? new InvalidOperationException("Throw expression evaluated to null.");
    }

    var operand = Evaluate(unary.Operand, args);

    // Use method if provided
    if (unary.Method != null)
      return unary.Method.Invoke(null, [operand]);

    return unary.NodeType switch {
      ExpressionType.Negate or ExpressionType.NegateChecked => Negate(operand),
      ExpressionType.UnaryPlus => operand,
      ExpressionType.Not => Not(operand),
      ExpressionType.OnesComplement => OnesComplement(operand),
      ExpressionType.Convert or ExpressionType.ConvertChecked => Convert(operand, unary.Type),
      ExpressionType.TypeAs => operand != null && unary.Type.IsInstanceOfType(operand) ? operand : null,
      ExpressionType.ArrayLength => ((Array)operand!).Length,
      ExpressionType.Quote => unary.Operand,
      ExpressionType.Increment => Increment(operand),
      ExpressionType.Decrement => Decrement(operand),
      ExpressionType.IsTrue => IsTrue(operand),
      ExpressionType.IsFalse => IsFalse(operand),
      ExpressionType.Unbox => operand,
      ExpressionType.PreIncrementAssign => EvaluatePreIncrement(unary.Operand, args),
      ExpressionType.PreDecrementAssign => EvaluatePreDecrement(unary.Operand, args),
      ExpressionType.PostIncrementAssign => EvaluatePostIncrement(unary.Operand, args),
      ExpressionType.PostDecrementAssign => EvaluatePostDecrement(unary.Operand, args),
      _ => throw new NotSupportedException($"Unary operator {unary.NodeType} is not supported.")
    };
  }

  private object? EvaluateMethodCall(MethodCallExpression call, object?[] args) {
    var instance = call.Object != null ? Evaluate(call.Object, args) : null;
    var arguments = new object?[call.Arguments.Count];
    for (var i = 0; i < call.Arguments.Count; ++i)
      arguments[i] = Evaluate(call.Arguments[i], args);

    return call.Method.Invoke(instance, arguments);
  }

  private object? EvaluateMember(MemberExpression member, object?[] args) {
    var instance = member.Expression != null ? Evaluate(member.Expression, args) : null;

    return member.Member switch {
      PropertyInfo property => property.GetValue(instance, null),
      FieldInfo field => field.GetValue(instance),
      _ => throw new NotSupportedException($"Member type {member.Member.MemberType} is not supported.")
    };
  }

  private object? EvaluateConditional(ConditionalExpression cond, object?[] args) {
    var test = Evaluate(cond.Test, args);
    return (bool)test! ? Evaluate(cond.IfTrue, args) : Evaluate(cond.IfFalse, args);
  }

  private object? EvaluateNew(NewExpression newExpr, object?[] args) {
    var arguments = new object?[newExpr.Arguments.Count];
    for (var i = 0; i < newExpr.Arguments.Count; ++i)
      arguments[i] = Evaluate(newExpr.Arguments[i], args);

    return newExpr.Constructor!.Invoke(arguments);
  }

  private object? EvaluateNewArray(NewArrayExpression newArray, object?[] args) {
    var elementType = newArray.Type.GetElementType()!;

    if (newArray.NodeType == ExpressionType.NewArrayInit) {
      var array = Array.CreateInstance(elementType, newArray.Expressions.Count);
      for (var i = 0; i < newArray.Expressions.Count; ++i)
        array.SetValue(Evaluate(newArray.Expressions[i], args), i);
      return array;
    }

    // NewArrayBounds
    var bounds = new int[newArray.Expressions.Count];
    for (var i = 0; i < newArray.Expressions.Count; ++i)
      bounds[i] = System.Convert.ToInt32(Evaluate(newArray.Expressions[i], args));

    return Array.CreateInstance(elementType, bounds);
  }

  private object? EvaluateInvocation(InvocationExpression invocation, object?[] args) {
    var target = Evaluate(invocation.Expression, args);
    var arguments = new object?[invocation.Arguments.Count];
    for (var i = 0; i < invocation.Arguments.Count; ++i)
      arguments[i] = Evaluate(invocation.Arguments[i], args);

    return ((Delegate)target!).DynamicInvoke(arguments);
  }

  private object EvaluateTypeBinary(TypeBinaryExpression typeBinary, object?[] args) {
    var value = Evaluate(typeBinary.Expression, args);
    if (typeBinary.NodeType == ExpressionType.TypeIs)
      return value != null && typeBinary.TypeOperand.IsInstanceOfType(value);

    // TypeEqual
    return value != null && value.GetType() == typeBinary.TypeOperand;
  }

  private object? EvaluateBlock(BlockExpression block, object?[] args) {
    object? result = null;
    foreach (var expr in block.Expressions)
      result = Evaluate(expr, args);
    return result;
  }

  private object? EvaluateIndex(IndexExpression index, object?[] args) {
    var instance = Evaluate(index.Object!, args);
    var indexArgs = new object?[index.Arguments.Count];
    for (var i = 0; i < index.Arguments.Count; ++i)
      indexArgs[i] = Evaluate(index.Arguments[i], args);

    if (index.Indexer != null)
      return index.Indexer.GetValue(instance, indexArgs);

    // Array access
    var array = (Array)instance!;
    if (indexArgs.Length == 1)
      return array.GetValue(System.Convert.ToInt32(indexArgs[0]));

    var indices = new int[indexArgs.Length];
    for (var i = 0; i < indexArgs.Length; ++i)
      indices[i] = System.Convert.ToInt32(indexArgs[i]);
    return array.GetValue(indices);
  }

  private object? EvaluateListInit(ListInitExpression listInit, object?[] args) {
    var instance = EvaluateNew(listInit.NewExpression, args);
    foreach (var initializer in listInit.Initializers) {
      var initArgs = new object?[initializer.Arguments.Count];
      for (var i = 0; i < initializer.Arguments.Count; ++i)
        initArgs[i] = Evaluate(initializer.Arguments[i], args);
      initializer.AddMethod.Invoke(instance, initArgs);
    }
    return instance;
  }

  private object? EvaluateMemberInit(MemberInitExpression memberInit, object?[] args) {
    var instance = EvaluateNew(memberInit.NewExpression, args);
    foreach (var binding in memberInit.Bindings)
      ApplyBinding(binding, instance, args);
    return instance;
  }

  private void ApplyBinding(MemberBinding binding, object? instance, object?[] args) {
    switch (binding) {
      case MemberAssignment assignment:
        var value = Evaluate(assignment.Expression, args);
        SetMemberValue(assignment.Member, instance, value);
        break;
      case MemberMemberBinding memberBinding:
        var nestedInstance = GetMemberValue(memberBinding.Member, instance);
        foreach (var nestedBinding in memberBinding.Bindings)
          ApplyBinding(nestedBinding, nestedInstance, args);
        break;
      case MemberListBinding listBinding:
        var listInstance = GetMemberValue(listBinding.Member, instance);
        foreach (var initializer in listBinding.Initializers) {
          var initArgs = new object?[initializer.Arguments.Count];
          for (var i = 0; i < initializer.Arguments.Count; ++i)
            initArgs[i] = Evaluate(initializer.Arguments[i], args);
          initializer.AddMethod.Invoke(listInstance, initArgs);
        }
        break;
    }
  }

  private static void SetMemberValue(MemberInfo member, object? instance, object? value) {
    switch (member) {
      case PropertyInfo property:
        property.SetValue(instance, value, null);
        break;
      case FieldInfo field:
        field.SetValue(instance, value);
        break;
    }
  }

  private static object? GetMemberValue(MemberInfo member, object? instance) {
    return member switch {
      PropertyInfo property => property.GetValue(instance, null),
      FieldInfo field => field.GetValue(instance),
      _ => throw new NotSupportedException($"Member type {member.MemberType} is not supported.")
    };
  }

  #region Control Flow

  private object? EvaluateSwitch(SwitchExpression switchExpr, object?[] args) {
    var switchValue = Evaluate(switchExpr.SwitchValue, args);

    foreach (var @case in switchExpr.Cases) {
      foreach (var testValue in @case.TestValues) {
        var test = Evaluate(testValue, args);
        var isMatch = switchExpr.Comparison != null
          ? (bool)switchExpr.Comparison.Invoke(null, [switchValue, test])!
          : Equals(switchValue, test);

        if (isMatch)
          return Evaluate(@case.Body, args);
      }
    }

    return switchExpr.DefaultBody != null ? Evaluate(switchExpr.DefaultBody, args) : null;
  }

  private object? EvaluateLoop(LoopExpression loop, object?[] args) {
    while (true) {
      this._gotoTarget = null;
      var result = Evaluate(loop.Body, args);

      if (this._gotoTarget != null) {
        if (loop.BreakLabel != null && this._gotoTarget == loop.BreakLabel) {
          this._gotoTarget = null;
          return this._labelValues.TryGetValue(loop.BreakLabel, out var breakValue) ? breakValue : null;
        }

        if (loop.ContinueLabel != null && this._gotoTarget == loop.ContinueLabel) {
          this._gotoTarget = null;
          continue;
        }

        // Propagate to outer scope
        return result;
      }
    }
  }

  private object? EvaluateLabel(LabelExpression label, object?[] args) {
    if (this._gotoTarget == label.Target) {
      this._gotoTarget = null;
      return this._labelValues.TryGetValue(label.Target, out var value) ? value : null;
    }

    return label.DefaultValue != null ? Evaluate(label.DefaultValue, args) : GetDefaultValue(label.Type);
  }

  private object? EvaluateGoto(GotoExpression gotoExpr, object?[] args) {
    var value = gotoExpr.Value != null ? Evaluate(gotoExpr.Value, args) : null;
    this._labelValues[gotoExpr.Target] = value;
    this._gotoTarget = gotoExpr.Target;
    return value;
  }

  private object? EvaluateTry(TryExpression tryExpr, object?[] args) {
    object? result = null;
    Exception? caughtException = null;

    try {
      result = Evaluate(tryExpr.Body, args);
    } catch (Exception ex) {
      caughtException = ex;

      foreach (var handler in tryExpr.Handlers) {
        if (!handler.Test.IsInstanceOfType(ex))
          continue;

        if (handler.Filter != null) {
          if (handler.Variable != null)
            this._parameterIndices[handler.Variable] = args.Length;

          var filterResult = Evaluate(handler.Filter, handler.Variable != null ? [..args, ex] : args);
          if (filterResult is not true)
            continue;
        }

        if (handler.Variable != null)
          this._parameterIndices[handler.Variable] = args.Length;

        result = Evaluate(handler.Body, handler.Variable != null ? [..args, ex] : args);
        caughtException = null;
        break;
      }

      if (caughtException != null && tryExpr.Fault != null)
        Evaluate(tryExpr.Fault, args);

      if (caughtException != null)
        throw;
    } finally {
      if (tryExpr.Finally != null)
        Evaluate(tryExpr.Finally, args);
    }

    return result;
  }

  #endregion

  #region Assignment Operations

  private object? EvaluateAssign(Expression target, object? value, object?[] args) {
    switch (target) {
      case ParameterExpression param:
        args[this._parameterIndices[param]] = value;
        return value;
      case MemberExpression member:
        var instance = member.Expression != null ? Evaluate(member.Expression, args) : null;
        SetMemberValue(member.Member, instance, value);
        return value;
      case IndexExpression index:
        var indexInstance = Evaluate(index.Object!, args);
        var indexArgs = new object?[index.Arguments.Count];
        for (var i = 0; i < index.Arguments.Count; ++i)
          indexArgs[i] = Evaluate(index.Arguments[i], args);
        if (index.Indexer != null)
          index.Indexer.SetValue(indexInstance, value, indexArgs);
        else {
          var array = (Array)indexInstance!;
          if (indexArgs.Length == 1)
            array.SetValue(value, System.Convert.ToInt32(indexArgs[0]));
          else {
            var indices = new int[indexArgs.Length];
            for (var i = 0; i < indexArgs.Length; ++i)
              indices[i] = System.Convert.ToInt32(indexArgs[i]);
            array.SetValue(value, indices);
          }
        }
        return value;
      default:
        throw new NotSupportedException($"Cannot assign to expression of type {target.NodeType}.");
    }
  }

  private static bool IsCompoundAssignment(ExpressionType nodeType) =>
    nodeType is ExpressionType.AddAssign or ExpressionType.AddAssignChecked
      or ExpressionType.SubtractAssign or ExpressionType.SubtractAssignChecked
      or ExpressionType.MultiplyAssign or ExpressionType.MultiplyAssignChecked
      or ExpressionType.DivideAssign or ExpressionType.ModuloAssign
      or ExpressionType.AndAssign or ExpressionType.OrAssign or ExpressionType.ExclusiveOrAssign
      or ExpressionType.LeftShiftAssign or ExpressionType.RightShiftAssign
      or ExpressionType.PowerAssign;

  private static object? EvaluateCompoundAssignment(ExpressionType nodeType, object? left, object? right) =>
    nodeType switch {
      ExpressionType.AddAssign or ExpressionType.AddAssignChecked => Add(left, right),
      ExpressionType.SubtractAssign or ExpressionType.SubtractAssignChecked => Subtract(left, right),
      ExpressionType.MultiplyAssign or ExpressionType.MultiplyAssignChecked => Multiply(left, right),
      ExpressionType.DivideAssign => Divide(left, right),
      ExpressionType.ModuloAssign => Modulo(left, right),
      ExpressionType.AndAssign => And(left, right),
      ExpressionType.OrAssign => Or(left, right),
      ExpressionType.ExclusiveOrAssign => Xor(left, right),
      ExpressionType.LeftShiftAssign => LeftShift(left, right),
      ExpressionType.RightShiftAssign => RightShift(left, right),
      ExpressionType.PowerAssign => Math.Pow(System.Convert.ToDouble(left), System.Convert.ToDouble(right)),
      _ => throw new NotSupportedException($"Compound assignment {nodeType} is not supported.")
    };

  private object? EvaluatePreIncrement(Expression target, object?[] args) {
    var value = Evaluate(target, args);
    var incremented = Increment(value);
    return EvaluateAssign(target, incremented, args);
  }

  private object? EvaluatePreDecrement(Expression target, object?[] args) {
    var value = Evaluate(target, args);
    var decremented = Decrement(value);
    return EvaluateAssign(target, decremented, args);
  }

  private object? EvaluatePostIncrement(Expression target, object?[] args) {
    var value = Evaluate(target, args);
    var incremented = Increment(value);
    EvaluateAssign(target, incremented, args);
    return value;
  }

  private object? EvaluatePostDecrement(Expression target, object?[] args) {
    var value = Evaluate(target, args);
    var decremented = Decrement(value);
    EvaluateAssign(target, decremented, args);
    return value;
  }

  #endregion

  #region Arithmetic Operations

  private static object Add(object? left, object? right) {
    if (left is string || right is string)
      return string.Concat(left, right);

    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! + (int)right!,
      TypeCode.Int64 => (long)left! + (long)right!,
      TypeCode.Double => (double)left! + (double)right!,
      TypeCode.Single => (float)left! + (float)right!,
      TypeCode.Decimal => (decimal)left! + (decimal)right!,
      TypeCode.Int16 => (short)left! + (short)right!,
      TypeCode.Byte => (byte)left! + (byte)right!,
      TypeCode.UInt32 => (uint)left! + (uint)right!,
      TypeCode.UInt64 => (ulong)left! + (ulong)right!,
      _ => throw new NotSupportedException($"Addition is not supported for type {left?.GetType()}.")
    };
  }

  private static object Subtract(object? left, object? right) {
    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! - (int)right!,
      TypeCode.Int64 => (long)left! - (long)right!,
      TypeCode.Double => (double)left! - (double)right!,
      TypeCode.Single => (float)left! - (float)right!,
      TypeCode.Decimal => (decimal)left! - (decimal)right!,
      _ => throw new NotSupportedException($"Subtraction is not supported for type {left?.GetType()}.")
    };
  }

  private static object Multiply(object? left, object? right) {
    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! * (int)right!,
      TypeCode.Int64 => (long)left! * (long)right!,
      TypeCode.Double => (double)left! * (double)right!,
      TypeCode.Single => (float)left! * (float)right!,
      TypeCode.Decimal => (decimal)left! * (decimal)right!,
      _ => throw new NotSupportedException($"Multiplication is not supported for type {left?.GetType()}.")
    };
  }

  private static object Divide(object? left, object? right) {
    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! / (int)right!,
      TypeCode.Int64 => (long)left! / (long)right!,
      TypeCode.Double => (double)left! / (double)right!,
      TypeCode.Single => (float)left! / (float)right!,
      TypeCode.Decimal => (decimal)left! / (decimal)right!,
      _ => throw new NotSupportedException($"Division is not supported for type {left?.GetType()}.")
    };
  }

  private static object Modulo(object? left, object? right) {
    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! % (int)right!,
      TypeCode.Int64 => (long)left! % (long)right!,
      TypeCode.Double => (double)left! % (double)right!,
      _ => throw new NotSupportedException($"Modulo is not supported for type {left?.GetType()}.")
    };
  }

  private static object And(object? left, object? right) {
    if (left is bool boolLeft && right is bool boolRight)
      return boolLeft & boolRight;

    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! & (int)right!,
      TypeCode.Int64 => (long)left! & (long)right!,
      TypeCode.UInt32 => (uint)left! & (uint)right!,
      TypeCode.UInt64 => (ulong)left! & (ulong)right!,
      _ => throw new NotSupportedException($"Bitwise AND is not supported for type {left?.GetType()}.")
    };
  }

  private static object Or(object? left, object? right) {
    if (left is bool boolLeft && right is bool boolRight)
      return boolLeft | boolRight;

    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! | (int)right!,
      TypeCode.Int64 => (long)left! | (long)right!,
      TypeCode.UInt32 => (uint)left! | (uint)right!,
      TypeCode.UInt64 => (ulong)left! | (ulong)right!,
      _ => throw new NotSupportedException($"Bitwise OR is not supported for type {left?.GetType()}.")
    };
  }

  private static object Xor(object? left, object? right) {
    if (left is bool boolLeft && right is bool boolRight)
      return boolLeft ^ boolRight;

    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! ^ (int)right!,
      TypeCode.Int64 => (long)left! ^ (long)right!,
      TypeCode.UInt32 => (uint)left! ^ (uint)right!,
      TypeCode.UInt64 => (ulong)left! ^ (ulong)right!,
      _ => throw new NotSupportedException($"Bitwise XOR is not supported for type {left?.GetType()}.")
    };
  }

  private static object LeftShift(object? left, object? right) {
    var shift = System.Convert.ToInt32(right);
    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! << shift,
      TypeCode.Int64 => (long)left! << shift,
      TypeCode.UInt32 => (uint)left! << shift,
      TypeCode.UInt64 => (ulong)left! << shift,
      _ => throw new NotSupportedException($"Left shift is not supported for type {left?.GetType()}.")
    };
  }

  private static object RightShift(object? left, object? right) {
    var shift = System.Convert.ToInt32(right);
    return Type.GetTypeCode(left?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)left! >> shift,
      TypeCode.Int64 => (long)left! >> shift,
      TypeCode.UInt32 => (uint)left! >> shift,
      TypeCode.UInt64 => (ulong)left! >> shift,
      _ => throw new NotSupportedException($"Right shift is not supported for type {left?.GetType()}.")
    };
  }

  private static int Compare(object? left, object? right) {
    if (left is IComparable comparable)
      return comparable.CompareTo(right);
    throw new NotSupportedException($"Comparison is not supported for type {left?.GetType()}.");
  }

  private static object Negate(object? operand) {
    return Type.GetTypeCode(operand?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => -(int)operand!,
      TypeCode.Int64 => -(long)operand!,
      TypeCode.Double => -(double)operand!,
      TypeCode.Single => -(float)operand!,
      TypeCode.Decimal => -(decimal)operand!,
      _ => throw new NotSupportedException($"Negation is not supported for type {operand?.GetType()}.")
    };
  }

  private static object Not(object? operand) {
    if (operand is bool b)
      return !b;

    return Type.GetTypeCode(operand?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => ~(int)operand!,
      TypeCode.Int64 => ~(long)operand!,
      TypeCode.UInt32 => ~(uint)operand!,
      TypeCode.UInt64 => ~(ulong)operand!,
      _ => throw new NotSupportedException($"Bitwise NOT is not supported for type {operand?.GetType()}.")
    };
  }

  private static object OnesComplement(object? operand) =>
    Type.GetTypeCode(operand?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => ~(int)operand!,
      TypeCode.Int64 => ~(long)operand!,
      TypeCode.UInt32 => ~(uint)operand!,
      TypeCode.UInt64 => ~(ulong)operand!,
      TypeCode.Int16 => (short)~(int)(short)operand!,
      TypeCode.Byte => (byte)~(int)(byte)operand!,
      TypeCode.SByte => (sbyte)~(int)(sbyte)operand!,
      TypeCode.UInt16 => (ushort)~(int)(ushort)operand!,
      _ => throw new NotSupportedException($"Ones complement is not supported for type {operand?.GetType()}.")
    };

  private static object Increment(object? operand) =>
    Type.GetTypeCode(operand?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)operand! + 1,
      TypeCode.Int64 => (long)operand! + 1,
      TypeCode.Double => (double)operand! + 1,
      TypeCode.Single => (float)operand! + 1,
      TypeCode.Decimal => (decimal)operand! + 1,
      TypeCode.Int16 => (short)((short)operand! + 1),
      TypeCode.Byte => (byte)((byte)operand! + 1),
      TypeCode.UInt32 => (uint)operand! + 1,
      TypeCode.UInt64 => (ulong)operand! + 1,
      _ => throw new NotSupportedException($"Increment is not supported for type {operand?.GetType()}.")
    };

  private static object Decrement(object? operand) =>
    Type.GetTypeCode(operand?.GetType() ?? typeof(object)) switch {
      TypeCode.Int32 => (int)operand! - 1,
      TypeCode.Int64 => (long)operand! - 1,
      TypeCode.Double => (double)operand! - 1,
      TypeCode.Single => (float)operand! - 1,
      TypeCode.Decimal => (decimal)operand! - 1,
      TypeCode.Int16 => (short)((short)operand! - 1),
      TypeCode.Byte => (byte)((byte)operand! - 1),
      TypeCode.UInt32 => (uint)operand! - 1,
      TypeCode.UInt64 => (ulong)operand! - 1,
      _ => throw new NotSupportedException($"Decrement is not supported for type {operand?.GetType()}.")
    };

  private static bool IsTrue(object? operand) => operand is true;

  private static bool IsFalse(object? operand) => operand is false;

  private static object? Convert(object? value, Type type) {
    if (value == null)
      return type.IsValueType ? GetDefaultValue(type) : null;

    if (type.IsInstanceOfType(value))
      return value;

    // Handle nullable types
    var underlyingType = Nullable.GetUnderlyingType(type);
    if (underlyingType != null)
      type = underlyingType;

    return System.Convert.ChangeType(value, type);
  }

  private static object? GetDefaultValue(Type type) {
    if (!type.IsValueType || type == typeof(void))
      return null;

    return Activator.CreateInstance(type);
  }

  #endregion

  #region Delegate Creation Helpers

  private Delegate CreateFunc0() {
    var returnType = this._lambda.ReturnType;
    var funcType = typeof(Func<>).MakeGenericType(returnType);
    var method = GetType().GetMethod(nameof(InvokeFunc0), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(returnType);
    return Delegate.CreateDelegate(funcType, this, method);
  }

  private TResult InvokeFunc0<TResult>() => (TResult)Interpret([])!;

  private Delegate CreateFunc1() {
    var p0 = this._lambda.Parameters[0].Type;
    var returnType = this._lambda.ReturnType;
    var funcType = typeof(Func<,>).MakeGenericType(p0, returnType);
    var method = GetType().GetMethod(nameof(InvokeFunc1), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0, returnType);
    return Delegate.CreateDelegate(funcType, this, method);
  }

  private TResult InvokeFunc1<T1, TResult>(T1 arg1) => (TResult)Interpret([arg1])!;

  private Delegate CreateFunc2() {
    var p0 = this._lambda.Parameters[0].Type;
    var p1 = this._lambda.Parameters[1].Type;
    var returnType = this._lambda.ReturnType;
    var funcType = typeof(Func<,,>).MakeGenericType(p0, p1, returnType);
    var method = GetType().GetMethod(nameof(InvokeFunc2), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0, p1, returnType);
    return Delegate.CreateDelegate(funcType, this, method);
  }

  private TResult InvokeFunc2<T1, T2, TResult>(T1 arg1, T2 arg2) => (TResult)Interpret([arg1, arg2])!;

  private Delegate CreateFunc3() {
    var p0 = this._lambda.Parameters[0].Type;
    var p1 = this._lambda.Parameters[1].Type;
    var p2 = this._lambda.Parameters[2].Type;
    var returnType = this._lambda.ReturnType;
    var funcType = typeof(Func<,,,>).MakeGenericType(p0, p1, p2, returnType);
    var method = GetType().GetMethod(nameof(InvokeFunc3), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0, p1, p2, returnType);
    return Delegate.CreateDelegate(funcType, this, method);
  }

  private TResult InvokeFunc3<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3) =>
    (TResult)Interpret([arg1, arg2, arg3])!;

  private Delegate CreateFunc4() {
    var p0 = this._lambda.Parameters[0].Type;
    var p1 = this._lambda.Parameters[1].Type;
    var p2 = this._lambda.Parameters[2].Type;
    var p3 = this._lambda.Parameters[3].Type;
    var returnType = this._lambda.ReturnType;
    var funcType = typeof(Func<,,,,>).MakeGenericType(p0, p1, p2, p3, returnType);
    var method = GetType().GetMethod(nameof(InvokeFunc4), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0, p1, p2, p3, returnType);
    return Delegate.CreateDelegate(funcType, this, method);
  }

  private TResult InvokeFunc4<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) =>
    (TResult)Interpret([arg1, arg2, arg3, arg4])!;

  private Delegate CreateAction1() {
    var p0 = this._lambda.Parameters[0].Type;
    var actionType = typeof(Action<>).MakeGenericType(p0);
    var method = GetType().GetMethod(nameof(InvokeAction1), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0);
    return Delegate.CreateDelegate(actionType, this, method);
  }

  private void InvokeAction1<T1>(T1 arg1) => Interpret([arg1]);

  private Delegate CreateAction2() {
    var p0 = this._lambda.Parameters[0].Type;
    var p1 = this._lambda.Parameters[1].Type;
    var actionType = typeof(Action<,>).MakeGenericType(p0, p1);
    var method = GetType().GetMethod(nameof(InvokeAction2), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0, p1);
    return Delegate.CreateDelegate(actionType, this, method);
  }

  private void InvokeAction2<T1, T2>(T1 arg1, T2 arg2) => Interpret([arg1, arg2]);

  private Delegate CreateAction3() {
    var p0 = this._lambda.Parameters[0].Type;
    var p1 = this._lambda.Parameters[1].Type;
    var p2 = this._lambda.Parameters[2].Type;
    var actionType = typeof(Action<,,>).MakeGenericType(p0, p1, p2);
    var method = GetType().GetMethod(nameof(InvokeAction3), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0, p1, p2);
    return Delegate.CreateDelegate(actionType, this, method);
  }

  private void InvokeAction3<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) => Interpret([arg1, arg2, arg3]);

  private Delegate CreateAction4() {
    var p0 = this._lambda.Parameters[0].Type;
    var p1 = this._lambda.Parameters[1].Type;
    var p2 = this._lambda.Parameters[2].Type;
    var p3 = this._lambda.Parameters[3].Type;
    var actionType = typeof(Action<,,,>).MakeGenericType(p0, p1, p2, p3);
    var method = GetType().GetMethod(nameof(InvokeAction4), BindingFlags.NonPublic | BindingFlags.Instance)!
      .MakeGenericMethod(p0, p1, p2, p3);
    return Delegate.CreateDelegate(actionType, this, method);
  }

  private void InvokeAction4<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) =>
    Interpret([arg1, arg2, arg3, arg4]);

  #endregion

}

#endif
