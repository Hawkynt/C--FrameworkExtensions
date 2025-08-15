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
//

using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Windows.Forms;

partial class ControlExtensions {

  private static void _AddBinding<TControl, TSource>(this TControl @this, object bindingSource, Expression<Func<TControl, TSource, bool>> expression, Type controlType, Type sourceType, DataSourceUpdateMode mode, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control {
    const string excMessage = @"Must be an expression like :
(control, source) => control.propertyName == source.dataMember    // Two-way binding

// One-way source to control (both forms equivalent):
(control, source) => control.propertyName < source.dataMember     // control < source
(control, source) => source.dataMember > control.propertyName     // source > control

// One-way control to source (both forms equivalent):
(control, source) => control.propertyName > source.dataMember     // control > source  
(control, source) => source.dataMember < control.propertyName     // source < control

(control, source) => control.propertyName == (type)source.dataMember
(control, source) => control.propertyName == source.dataMember.ToString()
(control, source) => control.propertyName == source.subMember.dataMember
(control, source) => control.propertyName == (type)source.subMember.dataMember
(control, source) => control.propertyName == source.subMember.dataMember.ToString()
(control, source) => control.propertyName == source.....dataMember
(control, source) => control.propertyName == (type)source.....dataMember
(control, source) => control.propertyName == source.....dataMember.ToString()";

    if (expression.Body is not BinaryExpression { NodeType: ExpressionType.Equal or ExpressionType.LessThan or ExpressionType.GreaterThan } body)
      throw new ArgumentException(excMessage);

    // Determine which side is control and which is source, then determine binding direction
    var leftIsControl = GetControlPropertyName(body.Left) != null;
    var rightIsControl = GetControlPropertyName(body.Right) != null;
    
    string propertyName;
    string dataMember;
    DataSourceUpdateMode actualMode;
    ExpressionType bindingDirection;
    
    if (leftIsControl && !rightIsControl) {
      // Left = control, Right = source
      propertyName = GetControlPropertyName(body.Left) ?? throw new ArgumentException(excMessage);
      dataMember = GetBindingSourcePropertyName(body.Right) ?? throw new ArgumentException(excMessage);
      bindingDirection = body.NodeType;
      
      // Determine binding direction based on logical relationship
      actualMode = body.NodeType switch {
        ExpressionType.LessThan => DataSourceUpdateMode.Never,           // control < source → source-to-control
        ExpressionType.GreaterThan => DataSourceUpdateMode.OnPropertyChanged, // control > source → control-to-source  
        ExpressionType.Equal => mode,                                    // control == source → use provided mode
        _ => throw new ArgumentException(excMessage)
      };
    } else if (!leftIsControl && rightIsControl) {
      // Left = source, Right = control  
      propertyName = GetControlPropertyName(body.Right) ?? throw new ArgumentException(excMessage);
      
      dataMember = GetBindingSourcePropertyName(body.Left) ?? throw new ArgumentException(excMessage);
      
      // Determine binding direction based on logical relationship (swapped sides)
      actualMode = body.NodeType switch {
        ExpressionType.LessThan => DataSourceUpdateMode.OnPropertyChanged, // source < control → control-to-source
        ExpressionType.GreaterThan => DataSourceUpdateMode.Never,           // source > control → source-to-control
        ExpressionType.Equal => mode,                                       // source == control → use provided mode
        _ => throw new ArgumentException(excMessage)
      };
      
      // Adjust bindingDirection for the FormattingEnabled logic below
      bindingDirection = body.NodeType switch {
        ExpressionType.LessThan => ExpressionType.GreaterThan,  // Treat as control-to-source
        ExpressionType.GreaterThan => ExpressionType.LessThan,  // Treat as source-to-control
        ExpressionType.Equal => ExpressionType.Equal,
        _ => throw new ArgumentException(excMessage)
      };
    } else {
      throw new ArgumentException(excMessage);
    }

    var binding = new Binding(propertyName, bindingSource, dataMember, true) { DataSourceUpdateMode = actualMode };
    
    // For one-way control-to-source binding, we need to prevent initial source-to-control update
    if (bindingDirection == ExpressionType.GreaterThan)
      binding.FormattingEnabled = false;

    if (customConversionHandler != null)
      binding.Format += customConversionHandler;

    if (bindingCompleteCallback != null)
      binding.BindingComplete += bindingCompleteCallback;

    @this.DataBindings.Add(binding);
    return;

    string GetBindingSourcePropertyName(Expression e)
      => e switch {
        MemberExpression member => GetPropertyName(member), // controlProperty = bs.PropertyName
        UnaryExpression { NodeType: ExpressionType.Convert, Operand: MemberExpression member } => GetPropertyName(member),
        MethodCallExpression { Method.Name: nameof(ToString), Arguments.Count: 0, Object: MemberExpression member } => GetPropertyName(member),
        _ => null
      }
    ;

    string GetControlPropertyName(Expression e)
      => e switch {
        MemberExpression { Expression: ParameterExpression parameter } member when parameter.Type == controlType => member.Member.Name,
        UnaryExpression {
          NodeType: ExpressionType.Convert,
          Operand: MemberExpression { Expression: ParameterExpression parameter } member
        } when parameter.Type == controlType => member.Member.Name,
        MethodCallExpression { 
          Method.Name: nameof(ToString), 
          Arguments.Count: 0, 
          Object: MemberExpression { Expression: ParameterExpression parameter } member 
        } when parameter.Type == controlType => member.Member.Name,
        _ => null
      }
    ;

    string GetPropertyName(MemberExpression member) {
      if (
        member.Expression is ParameterExpression parameter
        && parameter.Type == sourceType
      )
        return member.Member.Name;

      if (member.Expression is not MemberExpression parent)
        return null;

      // walk the expression
      var propertyNameChain = new Stack<string>();
      do {
        propertyNameChain.Push(parent.Member.Name);
        parent = parent.Expression as MemberExpression;
      } while (parent != null);

      // create sources for members
      do {
        var propName = propertyNameChain.Pop();
        bindingSource = new BindingSource(bindingSource, propName);
      } while (propertyNameChain.Count > 0);

      return member.Member.Name;
    }
  }

  /// <summary>
  /// Adds a data binding to the control based on the specified expression, data source, and optional custom handlers.
  /// Supports directional binding using comparison operators.
  /// </summary>
  /// <typeparam name="TControl">The type of the control.</typeparam>
  /// <typeparam name="TSource">The type of the data source.</typeparam>
  /// <param name="this">The control instance.</param>
  /// <param name="source">The data source object.</param>
  /// <param name="expression">
  /// The binding expression, which must be of the form:
  /// <code>
  /// (control, source) => control.propertyName == source.dataMember  // Two-way binding
  /// (control, source) => control.propertyName &lt; source.dataMember   // One-way: source to control (read-only)
  /// (control, source) => control.propertyName &gt; source.dataMember   // One-way: control to source (write-only)
  /// (control, source) => control.propertyName == (type)source.dataMember
  /// (control, source) => control.propertyName == source.dataMember.ToString()
  /// (control, source) => control.propertyName == source.subMember.dataMember
  /// (control, source) => control.propertyName == (type)source.subMember.dataMember
  /// (control, source) => control.propertyName == source.subMember.dataMember.ToString()
  /// (control, source) => control.propertyName == source.....dataMember
  /// (control, source) => control.propertyName == (type)source.....dataMember
  /// (control, source) => control.propertyName == source.....dataMember.ToString()
  /// </code>
  /// </param>
  /// <param name="mode">(Optional: defaults to <see cref="DataSourceUpdateMode.OnPropertyChanged"/>) The data source update mode. Ignored for directional bindings (&lt; and &gt;).</param>
  /// <param name="customConversionHandler">(Optional) A custom conversion handler for the binding.</param>
  /// <param name="bindingCompleteCallback">(Optional) A callback to handle the binding complete event.</param>
  /// <exception cref="System.ArgumentException">Thrown if the expression does not match the expected format.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// MyDataSource dataSource = new MyDataSource();
  /// 
  /// // Two-way binding
  /// textBox.AddBinding(dataSource, (ctrl, src) => ctrl.Text == src.MyProperty);
  /// 
  /// // One-way: source updates control only
  /// textBox.AddBinding(dataSource, (ctrl, src) => ctrl.Text &lt; src.MyProperty);
  /// 
  /// // One-way: control updates source only
  /// textBox.AddBinding(dataSource, (ctrl, src) => ctrl.Text &gt; src.MyProperty);
  /// </code>
  /// </example>
  public static void AddBinding<TControl, TSource>(this TControl @this, TSource source, Expression<Func<TControl, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control
    => _AddBinding(@this, source, expression, @this.GetType(), source.GetType(), mode, customConversionHandler, bindingCompleteCallback);

  /// <summary>
  /// Adds a data binding to the control based on the specified expression, data source, and optional custom handlers.
  /// Supports directional binding using comparison operators.
  /// </summary>
  /// <typeparam name="TControl">The type of the control.</typeparam>
  /// <typeparam name="TSource">The type of the data source.</typeparam>
  /// <param name="this">The control instance.</param>
  /// <param name="source">The data source object.</param>
  /// <param name="expression">
  /// The binding expression, which must be of the form:
  /// <code>
  /// (control, source) => control.propertyName == source.dataMember  // Two-way binding
  /// (control, source) => control.propertyName &lt; source.dataMember   // One-way: source to control (read-only)
  /// (control, source) => control.propertyName &gt; source.dataMember   // One-way: control to source (write-only)
  /// (control, source) => control.propertyName == (type)source.dataMember
  /// (control, source) => control.propertyName == source.dataMember.ToString()
  /// (control, source) => control.propertyName == source.subMember.dataMember
  /// (control, source) => control.propertyName == (type)source.subMember.dataMember
  /// (control, source) => control.propertyName == source.subMember.dataMember.ToString()
  /// (control, source) => control.propertyName == source.....dataMember
  /// (control, source) => control.propertyName == (type)source.....dataMember
  /// (control, source) => control.propertyName == source.....dataMember.ToString()
  /// </code>
  /// </param>
  /// <param name="mode">(Optional: defaults to <see cref="DataSourceUpdateMode.OnPropertyChanged"/>) The data source update mode. Ignored for directional bindings (&lt; and &gt;).</param>
  /// <param name="customConversionHandler">(Optional) A custom conversion handler for the binding.</param>
  /// <param name="bindingCompleteCallback">(Optional) A callback to handle the binding complete event.</param>
  /// <exception cref="System.ArgumentException">Thrown if the expression does not match the expected format.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// object dataSource = new MyDataSource();
  /// 
  /// // Two-way binding
  /// textBox.AddBinding&lt;TextBox, MyDataSource&gt;(dataSource, (ctrl, src) => ctrl.Text == src.MyProperty);
  /// 
  /// // One-way: source updates control only
  /// textBox.AddBinding&lt;TextBox, MyDataSource&gt;(dataSource, (ctrl, src) => ctrl.Text &lt; src.MyProperty);
  /// 
  /// // One-way: control updates source only
  /// textBox.AddBinding&lt;TextBox, MyDataSource&gt;(dataSource, (ctrl, src) => ctrl.Text &gt; src.MyProperty);
  /// </code>
  /// </example>
  public static void AddBinding<TControl, TSource>(this TControl @this, object source, Expression<Func<TControl, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control
    => _AddBinding(@this, source, expression, typeof(TControl), typeof(TSource), mode, customConversionHandler, bindingCompleteCallback);

  #region to make life easier

  public static void AddBinding<TSource>(
    this Label @this,
    object source,
    Expression<Func<Label, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<Label, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this CheckBox @this,
    object source,
    Expression<Func<CheckBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<CheckBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this TextBox @this,
    object source,
    Expression<Func<TextBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<TextBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this NumericUpDown @this,
    object source,
    Expression<Func<NumericUpDown, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<NumericUpDown, TSource>(
      @this,
      source,
      expression,
      mode,
      customConversionHandler,
      bindingCompleteCallback
    );

  public static void AddBinding<TSource>(
    this RadioButton @this,
    object source,
    Expression<Func<RadioButton, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<RadioButton, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this Button @this,
    object source,
    Expression<Func<Button, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<Button, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this GroupBox @this,
    object source,
    Expression<Func<GroupBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<GroupBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this ComboBox @this,
    object source,
    Expression<Func<ComboBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<ComboBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this DateTimePicker @this,
    object source,
    Expression<Func<DateTimePicker, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<DateTimePicker, TSource>(
      @this,
      source,
      expression,
      mode,
      customConversionHandler,
      bindingCompleteCallback
    );

  #endregion

}