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

// ExpressionVisitor was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a visitor or rewriter for expression trees.
/// </summary>
/// <remarks>
/// This class is designed to be inherited to create more specialized classes whose functionality
/// requires traversing, examining, or copying an expression tree.
/// </remarks>
public abstract class ExpressionVisitor {

  /// <summary>
  /// Initializes a new instance of the <see cref="ExpressionVisitor"/> class.
  /// </summary>
  protected ExpressionVisitor() { }

  /// <summary>
  /// Dispatches the expression to one of the more specialized visit methods in this class.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  public virtual Expression? Visit(Expression? node) {
    if (node == null)
      return null;

    return node.Accept(this);
  }

  /// <summary>
  /// Dispatches the list of expressions to one of the more specialized visit methods in this class.
  /// </summary>
  /// <param name="nodes">The expressions to visit.</param>
  /// <returns>The modified expression list, if any of the elements were modified; otherwise, returns the original expression list.</returns>
  public ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes) {
    Expression[]? newNodes = null;
    for (int i = 0, n = nodes.Count; i < n; ++i) {
      var node = this.Visit(nodes[i]);
      if (newNodes != null) {
        newNodes[i] = node!;
      } else if (node != nodes[i]) {
        newNodes = new Expression[n];
        for (var j = 0; j < i; ++j)
          newNodes[j] = nodes[j];
        newNodes[i] = node!;
      }
    }

    return newNodes == null ? nodes : new ReadOnlyCollection<Expression>(newNodes);
  }

  /// <summary>
  /// Visits the children of the <see cref="BinaryExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitBinary(BinaryExpression node) {
    var left = this.Visit(node.Left);
    var right = this.Visit(node.Right);
    var conversion = this.VisitAndConvert(node.Conversion, nameof(VisitBinary));
    return node.Update(left!, conversion, right!);
  }

  /// <summary>
  /// Visits the children of the <see cref="ConditionalExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitConditional(ConditionalExpression node) {
    var test = this.Visit(node.Test);
    var ifTrue = this.Visit(node.IfTrue);
    var ifFalse = this.Visit(node.IfFalse);
    return node.Update(test!, ifTrue!, ifFalse!);
  }

  /// <summary>
  /// Visits the <see cref="ConstantExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitConstant(ConstantExpression node) => node;

  /// <summary>
  /// Visits the <see cref="DefaultExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitDefault(DefaultExpression node) => node;

  /// <summary>
  /// Visits the children of the extension expression.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitExtension(Expression node) =>
    node.CanReduce ? this.Visit(node.ReduceAndCheck())! : node;

  /// <summary>
  /// Visits the children of the <see cref="InvocationExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitInvocation(InvocationExpression node) {
    var expression = this.Visit(node.Expression);
    var arguments = this.VisitArguments(node.Arguments);
    return node.Update(expression!, arguments);
  }

  /// <summary>
  /// Visits the children of the <see cref="LambdaExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitLambda(LambdaExpression node) {
    var body = this.Visit(node.Body);
    var parameters = this.VisitAndConvert(node.Parameters, nameof(VisitLambda));
    if (body == node.Body && parameters == node.Parameters)
      return node;

    return Expression.Lambda(body!, parameters);
  }

  /// <summary>
  /// Visits the children of the <see cref="MemberExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitMember(MemberExpression node) {
    var expression = this.Visit(node.Expression);
    return node.Update(expression);
  }

  /// <summary>
  /// Visits the children of the <see cref="MethodCallExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitMethodCall(MethodCallExpression node) {
    var @object = this.Visit(node.Object);
    var arguments = this.VisitArguments(node.Arguments);
    return node.Update(@object, arguments);
  }

  /// <summary>
  /// Visits the children of the <see cref="NewExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitNew(NewExpression node) {
    var arguments = this.VisitArguments(node.Arguments);
    return node.Update(arguments);
  }

  /// <summary>
  /// Visits the children of the <see cref="NewArrayExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitNewArray(NewArrayExpression node) {
    var expressions = this.VisitArguments(node.Expressions);
    return node.Update(expressions);
  }

  /// <summary>
  /// Visits the <see cref="ParameterExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitParameter(ParameterExpression node) => node;

  /// <summary>
  /// Visits the children of the <see cref="TypeBinaryExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitTypeBinary(TypeBinaryExpression node) {
    var expression = this.Visit(node.Expression);
    return node.Update(expression!);
  }

  /// <summary>
  /// Visits the children of the <see cref="UnaryExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitUnary(UnaryExpression node) {
    var operand = this.Visit(node.Operand);
    return node.Update(operand!);
  }

  /// <summary>
  /// Visits the <see cref="LabelExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitLabel(LabelExpression node) {
    var defaultValue = this.Visit(node.DefaultValue);
    return node.Update(node.Target, defaultValue);
  }

  /// <summary>
  /// Visits the <see cref="GotoExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitGoto(GotoExpression node) {
    var value = this.Visit(node.Value);
    return node.Update(node.Target, value);
  }

  /// <summary>
  /// Visits the children of the <see cref="BlockExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitBlock(BlockExpression node) {
    var variables = this.VisitAndConvert(node.Variables, nameof(VisitBlock));
    var expressions = this.Visit(node.Expressions);
    return node.Update(variables, expressions);
  }

  /// <summary>
  /// Visits the children of the <see cref="LoopExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitLoop(LoopExpression node) {
    var body = this.Visit(node.Body);
    return node.Update(node.BreakLabel, node.ContinueLabel, body!);
  }

  /// <summary>
  /// Visits the children of the <see cref="SwitchExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitSwitch(SwitchExpression node) {
    var switchValue = this.Visit(node.SwitchValue);
    var cases = this.VisitSwitchCases(node.Cases);
    var defaultBody = this.Visit(node.DefaultBody);
    return node.Update(switchValue!, cases, defaultBody);
  }

  /// <summary>
  /// Visits the <see cref="SwitchCase"/>.
  /// </summary>
  /// <param name="node">The switch case to visit.</param>
  /// <returns>The modified switch case, if it or any subexpression was modified; otherwise, returns the original switch case.</returns>
  protected virtual SwitchCase VisitSwitchCase(SwitchCase node) {
    var testValues = this.Visit(node.TestValues);
    var body = this.Visit(node.Body);
    return node.Update(testValues, body!);
  }

  private IEnumerable<SwitchCase> VisitSwitchCases(ReadOnlyCollection<SwitchCase> cases) {
    var result = new List<SwitchCase>(cases.Count);
    foreach (var switchCase in cases)
      result.Add(this.VisitSwitchCase(switchCase));
    return result;
  }

  /// <summary>
  /// Visits the children of the <see cref="TryExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitTry(TryExpression node) {
    var body = this.Visit(node.Body);
    var handlers = this.VisitCatchBlocks(node.Handlers);
    var @finally = this.Visit(node.Finally);
    var fault = this.Visit(node.Fault);
    return node.Update(body!, handlers, @finally, fault);
  }

  /// <summary>
  /// Visits the <see cref="CatchBlock"/>.
  /// </summary>
  /// <param name="node">The catch block to visit.</param>
  /// <returns>The modified catch block, if it or any subexpression was modified; otherwise, returns the original catch block.</returns>
  protected virtual CatchBlock VisitCatchBlock(CatchBlock node) {
    var variable = this.VisitAndConvert(node.Variable, nameof(VisitCatchBlock));
    var filter = this.Visit(node.Filter);
    var body = this.Visit(node.Body);
    return node.Update(variable, filter, body!);
  }

  private IEnumerable<CatchBlock> VisitCatchBlocks(ReadOnlyCollection<CatchBlock> handlers) {
    var result = new List<CatchBlock>(handlers.Count);
    foreach (var handler in handlers)
      result.Add(this.VisitCatchBlock(handler));
    return result;
  }

  /// <summary>
  /// Visits the children of the <see cref="IndexExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitIndex(IndexExpression node) {
    var @object = this.Visit(node.Object);
    var arguments = this.VisitArguments(node.Arguments);
    return node.Update(@object, arguments);
  }

  /// <summary>
  /// Visits the children of the <see cref="ListInitExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitListInit(ListInitExpression node) {
    var newExpression = (NewExpression)this.Visit(node.NewExpression)!;
    var initializers = this.VisitElementInits(node.Initializers);
    return node.Update(newExpression, initializers);
  }

  /// <summary>
  /// Visits the <see cref="ElementInit"/>.
  /// </summary>
  /// <param name="node">The element initializer to visit.</param>
  /// <returns>The modified element initializer, if it or any subexpression was modified; otherwise, returns the original element initializer.</returns>
  protected virtual ElementInit VisitElementInit(ElementInit node) {
    var arguments = this.VisitArguments(node.Arguments);
    return node.Update(arguments);
  }

  private IEnumerable<ElementInit> VisitElementInits(ReadOnlyCollection<ElementInit> initializers) {
    var result = new List<ElementInit>(initializers.Count);
    foreach (var init in initializers)
      result.Add(this.VisitElementInit(init));
    return result;
  }

  /// <summary>
  /// Visits the children of the <see cref="MemberInitExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitMemberInit(MemberInitExpression node) {
    var newExpression = (NewExpression)this.Visit(node.NewExpression)!;
    var bindings = this.VisitMemberBindings(node.Bindings);
    return node.Update(newExpression, bindings);
  }

  /// <summary>
  /// Visits the children of the <see cref="DynamicExpression"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected internal virtual Expression VisitDynamic(DynamicExpression node) {
    var arguments = this.VisitArguments(node.Arguments);
    return node.Update(arguments);
  }

  /// <summary>
  /// Visits the <see cref="MemberBinding"/>.
  /// </summary>
  /// <param name="node">The member binding to visit.</param>
  /// <returns>The modified member binding, if it or any subexpression was modified; otherwise, returns the original member binding.</returns>
  protected virtual MemberBinding VisitMemberBinding(MemberBinding node) =>
    node.BindingType switch {
      MemberBindingType.Assignment => this.VisitMemberAssignment((MemberAssignment)node),
      MemberBindingType.MemberBinding => this.VisitMemberMemberBinding((MemberMemberBinding)node),
      MemberBindingType.ListBinding => this.VisitMemberListBinding((MemberListBinding)node),
      _ => throw new InvalidOperationException($"Unknown member binding type: {node.BindingType}")
    };

  private IEnumerable<MemberBinding> VisitMemberBindings(ReadOnlyCollection<MemberBinding> bindings) {
    var result = new List<MemberBinding>(bindings.Count);
    foreach (var binding in bindings)
      result.Add(this.VisitMemberBinding(binding));
    return result;
  }

  /// <summary>
  /// Visits the children of the <see cref="MemberAssignment"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment node) {
    var expression = this.Visit(node.Expression);
    return node.Update(expression!);
  }

  /// <summary>
  /// Visits the children of the <see cref="MemberMemberBinding"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
    var bindings = this.VisitMemberBindings(node.Bindings);
    return node.Update(bindings);
  }

  /// <summary>
  /// Visits the children of the <see cref="MemberListBinding"/>.
  /// </summary>
  /// <param name="node">The expression to visit.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding node) {
    var initializers = this.VisitElementInits(node.Initializers);
    return node.Update(initializers);
  }

  /// <summary>
  /// Visits an expression, casting the result back to the original expression type.
  /// </summary>
  /// <typeparam name="T">The type of the expression.</typeparam>
  /// <param name="node">The expression to visit.</param>
  /// <param name="callerName">The name of the calling method; used to report to report better error message.</param>
  /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
  public T? VisitAndConvert<T>(T? node, string? callerName) where T : Expression {
    if (node == null)
      return null;

    var result = this.Visit(node);
    if (result is not T converted)
      throw new InvalidOperationException($"When called from '{callerName}', Visit must return the same type.");

    return converted;
  }

  /// <summary>
  /// Visits all nodes in the collection, casting the results back to the original types.
  /// </summary>
  public ReadOnlyCollection<T> VisitAndConvert<T>(ReadOnlyCollection<T> nodes, string? callerName) where T : Expression {
    T[]? newNodes = null;
    for (int i = 0, n = nodes.Count; i < n; ++i) {
      var node = this.VisitAndConvert(nodes[i], callerName);
      if (newNodes != null) {
        newNodes[i] = node!;
      } else if (node != nodes[i]) {
        newNodes = new T[n];
        for (var j = 0; j < i; ++j)
          newNodes[j] = nodes[j];
        newNodes[i] = node!;
      }
    }

    return newNodes == null ? nodes : new ReadOnlyCollection<T>(newNodes);
  }

  private IEnumerable<Expression> VisitArguments(ReadOnlyCollection<Expression> arguments) {
    var result = new List<Expression>(arguments.Count);
    foreach (var arg in arguments)
      result.Add(this.Visit(arg)!);
    return result;
  }

}

#endif
