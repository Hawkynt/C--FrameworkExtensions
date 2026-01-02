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

// LambdaExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions;

/// <summary>
/// Describes a lambda expression. This captures a block of code that is similar to a .NET method body.
/// </summary>
public class LambdaExpression : Expression {

  private readonly IList<ParameterExpression> _parameters;

  /// <summary>
  /// Initializes a new instance of the <see cref="LambdaExpression"/> class.
  /// </summary>
  internal LambdaExpression(Type delegateType, Expression body, IList<ParameterExpression> parameters)
    : base(ExpressionType.Lambda, delegateType) {
    this.Body = body;
    this._parameters = parameters;
  }

  /// <summary>
  /// Gets the body of the lambda expression.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the body of the lambda expression.</value>
  public Expression Body { get; }

  /// <summary>
  /// Gets the parameters of the lambda expression.
  /// </summary>
  /// <value>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ParameterExpression"/> objects that represent the parameters of the lambda expression.</value>
  public ReadOnlyCollection<ParameterExpression> Parameters => new(this._parameters);

  /// <summary>
  /// Gets the return type of the lambda expression.
  /// </summary>
  /// <value>The <see cref="System.Type"/> object representing the type of the lambda.</value>
  public Type ReturnType => this.Body.Type;

  /// <summary>
  /// Gets the name of the lambda expression.
  /// </summary>
  /// <value>The name of the lambda expression.</value>
  public string? Name => null;

  /// <summary>
  /// Gets the value that indicates if the lambda expression will be compiled with tail call optimization.
  /// </summary>
  /// <value><c>true</c> if the lambda expression will be compiled with tail call optimization; otherwise, <c>false</c>.</value>
  public bool TailCall => false;

  /// <summary>
  /// Compiles the lambda expression described by the expression tree into executable code.
  /// </summary>
  /// <returns>A delegate of type TDelegate that represents the lambda expression described by the <see cref="LambdaExpression"/>.</returns>
  public Delegate Compile() => this.Compile(false);

  /// <summary>
  /// Produces a delegate that represents the lambda expression.
  /// </summary>
  /// <param name="preferInterpretation">
  /// <c>true</c> to indicate that the expression should be compiled to an interpreted form, if it is available;
  /// <c>false</c> otherwise.
  /// </param>
  /// <returns>A delegate containing the compiled version of the lambda.</returns>
  public Delegate Compile(bool preferInterpretation) {
    if (preferInterpretation)
      return CompileInterpreted();

    return CompileToIL();
  }

  private Delegate CompileInterpreted() {
    var interpreter = new ExpressionInterpreter(this);
    return interpreter.CreateDelegate();
  }

  private Delegate CompileToIL() {
    // For net20, we use interpretation since DynamicMethod may have limitations
    // This is a simplified implementation - full IL compilation is complex
    return CompileInterpreted();
  }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitLambda(this);

  /// <summary>
  /// Returns a textual representation of the <see cref="LambdaExpression"/>.
  /// </summary>
  public override string ToString() {
    var parameters = JoinStrings(this._parameters);
    return $"({parameters}) => {this.Body}";
  }

  private static string JoinStrings<T>(IList<T> items) {
    if (items.Count == 0)
      return string.Empty;
    var result = new System.Text.StringBuilder();
    for (var i = 0; i < items.Count; ++i) {
      if (i > 0)
        result.Append(", ");
      result.Append(items[i]);
    }
    return result.ToString();
  }

}

/// <summary>
/// Represents a strongly typed lambda expression as a data structure in the form of an expression tree.
/// </summary>
/// <typeparam name="TDelegate">The type of the delegate that the <see cref="Expression{TDelegate}"/> represents.</typeparam>
public sealed class Expression<TDelegate> : LambdaExpression {

  /// <summary>
  /// Initializes a new instance of the <see cref="Expression{TDelegate}"/> class.
  /// </summary>
  internal Expression(Expression body, IList<ParameterExpression> parameters)
    : base(typeof(TDelegate), body, parameters) { }

  /// <summary>
  /// Compiles the lambda expression described by the expression tree into executable code and produces a delegate that represents the lambda expression.
  /// </summary>
  /// <returns>A delegate of type <typeparamref name="TDelegate"/> that represents the compiled lambda expression described by the <see cref="Expression{TDelegate}"/>.</returns>
  public new TDelegate Compile() => (TDelegate)(object)base.Compile();

  /// <summary>
  /// Produces a delegate that represents the lambda expression.
  /// </summary>
  /// <param name="preferInterpretation">
  /// <c>true</c> to indicate that the expression should be compiled to an interpreted form, if it is available;
  /// <c>false</c> otherwise.
  /// </param>
  /// <returns>A delegate containing the compiled version of the lambda.</returns>
  public new TDelegate Compile(bool preferInterpretation) => (TDelegate)(object)base.Compile(preferInterpretation);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public Expression<TDelegate> Update(Expression body, IEnumerable<ParameterExpression> parameters) {
    var paramList = parameters as IList<ParameterExpression> ?? new List<ParameterExpression>(parameters);
    if (body == this.Body && ReferenceEquals(paramList, this.Parameters))
      return this;

    return new Expression<TDelegate>(body, paramList);
  }

}

#endif
