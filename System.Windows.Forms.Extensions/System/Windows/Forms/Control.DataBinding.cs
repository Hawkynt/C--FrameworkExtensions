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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Guard;

namespace System.Windows.Forms;

partial class ControlExtensions {

  private static void _AddBinding<TControl, TSource>(this TControl @this, object source, Expression<Func<TControl, TSource, bool>> expression, Type controlType, Type sourceType, DataSourceUpdateMode mode, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(source);
    Guard.Against.ArgumentIsNull(expression);
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

    // Determine which side is control and which is source
    var leftIsControl = GetControlPropertyName(body.Left) != null;
    var rightIsControl = GetControlPropertyName(body.Right) != null;
    
    if (!leftIsControl && !rightIsControl || leftIsControl && rightIsControl)
      throw new ArgumentException(excMessage);
    
    // Extract property names
    string propertyName, dataMember;
    DataSourceUpdateMode actualMode;
    ExpressionType bindingDirection;
    bool isLeftControl = leftIsControl;
    
    if (leftIsControl) {
      propertyName = GetControlPropertyName(body.Left) ?? throw new ArgumentException(excMessage);
      dataMember = GetBindingSourcePropertyName(body.Right) ?? throw new ArgumentException(excMessage);
      
      actualMode = body.NodeType switch {
        ExpressionType.LessThan => DataSourceUpdateMode.Never,                    // control < source → source-to-control
        ExpressionType.GreaterThan => DataSourceUpdateMode.OnPropertyChanged,    // control > source → control-to-source  
        ExpressionType.Equal => mode,                                             // control == source → use provided mode
        _ => throw new ArgumentException(excMessage)
      };

      bindingDirection = body.NodeType switch {
        ExpressionType.LessThan => ExpressionType.LessThan, 
        ExpressionType.GreaterThan => ExpressionType.GreaterThan,
        ExpressionType.Equal => ExpressionType.Equal,
        _ => throw new ArgumentException(excMessage)
      };
    } else {
      propertyName = GetControlPropertyName(body.Right) ?? throw new ArgumentException(excMessage);
      dataMember = GetBindingSourcePropertyName(body.Left) ?? throw new ArgumentException(excMessage);
      
      actualMode = body.NodeType switch {
        ExpressionType.LessThan => DataSourceUpdateMode.OnPropertyChanged,       // source < control → control-to-source
        ExpressionType.GreaterThan => DataSourceUpdateMode.Never,                // source > control → source-to-control
        ExpressionType.Equal => mode,                                             // source == control → use provided mode
        _ => throw new ArgumentException(excMessage)
      };
      
      // need to inverse as sides are swapped
      bindingDirection = body.NodeType switch {
        ExpressionType.LessThan => ExpressionType.GreaterThan,
        ExpressionType.GreaterThan => ExpressionType.LessThan,
        ExpressionType.Equal => ExpressionType.Equal,
        _ => throw new ArgumentException(excMessage)
      };
    }

    Binding binding = null;
    
    // For control-to-source only bindings, don't create any binding - handle everything manually
    if (bindingDirection == ExpressionType.GreaterThan) {
      // No binding created - everything will be handled manually below
    } else {
      // For source-to-control and two-way bindings, use normal binding
      binding = new Binding(propertyName, source, dataMember, true) { DataSourceUpdateMode = actualMode };
      
      if (customConversionHandler != null)
        binding.Format += customConversionHandler;

      if (bindingCompleteCallback != null)
        binding.BindingComplete += bindingCompleteCallback;
    }

    // Ensure the control has a BindingContext for immediate binding activation
    @this.BindingContext ??= new BindingContext();

    // Force control handle creation to ensure binding works properly
    IntPtr handle;

    // Force handle creation, this fixes the issue where bindings don't work on invisible/non-focused controls
    if (!@this.IsHandleCreated) 
      handle = @this.Handle;
    
    // Also ensure parent forms have handles created
    var parent = @this.Parent;
    while (parent != null) {
      if (!parent.IsHandleCreated)
        handle = parent.Handle;
      
      parent = parent.Parent;
    }
      
    // Only add binding if one was created (not for control-to-source only)
    if (binding != null) {
      @this.DataBindings.Add(binding);
      
      // Force binding to read current value from source immediately
      try {
        binding.ReadValue();
      } catch {
        // Ignore read errors - binding initialization may still work
      }
    }
    
    
    // Add manual PropertyChanged handling for all INotifyPropertyChanged sources
    // This handles initial sync, nested properties, casts, and control-to-source bindings
    var eventHandlers = new List<(INotifyPropertyChanged notifier, PropertyChangedEventHandler handler)>();
    
    if (source is INotifyPropertyChanged notifySource) {
      
      void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        try {
          // Check if this property change is relevant to our binding
          bool isRelevantChange = string.IsNullOrEmpty(e.PropertyName) || 
                                 e.PropertyName == dataMember ||
                                 (dataMember.Contains('.') && dataMember.Split('.').Contains(e.PropertyName));
          
          if (isRelevantChange) {
            // If this is a nested binding and a parent object in the chain changed, rebuild subscriptions
            if (dataMember.Contains('.') && e.PropertyName != dataMember) {
              var pathParts = dataMember.Split('.');
              if (pathParts.Contains(e.PropertyName)) {
                RebuildNestedSubscriptions(source, dataMember, OnPropertyChanged, eventHandlers);
              }
            }
            
            // Only update control for source-to-control or two-way bindings
            bool shouldUpdateControl = actualMode == DataSourceUpdateMode.Never || 
                                     (actualMode == DataSourceUpdateMode.OnPropertyChanged && bindingDirection == ExpressionType.Equal);
            
            if (shouldUpdateControl) {
              Expression sourceExpression = isLeftControl ? body.Right : body.Left;
              var currentValue = EvaluateSourceExpression(sourceExpression, source);
              var controlProperty = @this.GetType().GetProperty(propertyName);
              if (controlProperty?.CanWrite == true) {
                var convertedValue = ConvertValue(currentValue, controlProperty.PropertyType);
                controlProperty.SetValue(@this, convertedValue, null);
              }
            }
          }
        } catch {
          // Ignore errors in manual sync
        }
      }
      
      notifySource.PropertyChanged += OnPropertyChanged;
      eventHandlers.Add((notifySource, OnPropertyChanged));
      
      // Subscribe to nested object PropertyChanged events if needed
      if (dataMember.Contains('.')) 
        SubscribeToNestedProperties(source, dataMember, OnPropertyChanged, eventHandlers);
      
      
      // Initial sync based on binding direction
      if (bindingDirection == ExpressionType.GreaterThan) {
        // Control-to-source only: sync FROM control TO source initially
        try {
          var controlProperty = @this.GetType().GetProperty(propertyName);
          if (controlProperty?.CanRead == true) {
            var controlValue = controlProperty.GetValue(@this, null);
            UpdateSourceFromControl(source, dataMember, controlValue);
          }
        } catch {
          // Ignore initial sync errors
        }
      } else if (actualMode == DataSourceUpdateMode.Never || 
                (actualMode == DataSourceUpdateMode.OnPropertyChanged && bindingDirection == ExpressionType.Equal)) {
        // Source-to-control or two-way: sync FROM source TO control initially
        try {
          Expression sourceExpression = isLeftControl ? body.Right : body.Left;
          var currentValue = EvaluateSourceExpression(sourceExpression, source);
          var controlProperty = @this.GetType().GetProperty(propertyName);
          if (controlProperty?.CanWrite == true) {
            var convertedValue = ConvertValue(currentValue, controlProperty.PropertyType);
            controlProperty.SetValue(@this, convertedValue, null);
          }
          
          // For cast expressions in two-way bindings, also write the cast value back to source
          if (actualMode == DataSourceUpdateMode.OnPropertyChanged && bindingDirection == ExpressionType.Equal && HasCastExpression(sourceExpression))
            UpdateSourceFromControl(source, dataMember, currentValue);
          
        } catch {
          // Ignore initial sync errors
        }
      }
    }
    
    // Ensure cleanup when control is disposed
    if (eventHandlers.Count > 0) {
      @this.Disposed += (s, e) => {
        foreach (var (notifier, handler) in eventHandlers) {
          try {
            notifier.PropertyChanged -= handler;
          } catch {
            // Ignore cleanup errors
          }
        }
      };
    }
    
    // Add control-to-source binding for test environments
    bool allowControlToSourceSync = bindingDirection == ExpressionType.GreaterThan || 
                                   (actualMode == DataSourceUpdateMode.OnPropertyChanged && bindingDirection == ExpressionType.Equal);
    
    if (allowControlToSourceSync)
      AddControlToSourceHandlers(@this, source, propertyName, dataMember);
    
    return;

    string GetBindingSourcePropertyName(Expression e) {
      // Handle nested conversions by unwrapping them
      Expression current = e;
      while (current is UnaryExpression { NodeType: ExpressionType.Convert } unary) {
        current = unary.Operand;
      }
      
      return current switch {
        MemberExpression member => GetPropertyName(member),
        MethodCallExpression { Method.Name: nameof(ToString), Arguments.Count: 0, Object: MemberExpression member } => GetPropertyName(member),
        _ => null
      };
    }

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

      // Build the property path for nested properties
      var propertyPath = new List<string>();
      var current = member;
      
      // Walk up the expression tree to build the full path
      while (current != null) {
        propertyPath.Add(current.Member.Name);
        
        if (current.Expression is ParameterExpression param && param.Type == sourceType) {
          // We've reached the source parameter, reverse to get correct order
          propertyPath.Reverse();
          return string.Join(".", propertyPath.ToArray());
        }
        
        current = current.Expression as MemberExpression;
      }

      return null; // Couldn't build a valid path to source parameter
    }
    
    object GetSourceValue(object sourceObj, string memberPath) {
      if (sourceObj == null || string.IsNullOrEmpty(memberPath))
        return null;
      
      // Handle BindingSource objects
      if (sourceObj is BindingSource bindingSource) {
        sourceObj = bindingSource.Current ?? bindingSource.DataSource;
      }
      
      if (sourceObj == null) return null;
      
      var parts = memberPath.Split('.');
      object current = sourceObj;
      
      foreach (var part in parts) {
        if (current == null) return null;
        
        // Handle BindingSource at each level
        if (current is BindingSource bs) {
          current = bs.Current ?? bs.DataSource;
        }
        
        if (current == null) return null;
        
        var property = current.GetType().GetProperty(part);
        if (property == null) return null;
        
        current = property.GetValue(current, null);
      }
      
      return current;
    }
    
    object ConvertValue(object value, Type targetType) {
      if (value == null || targetType == null) return value;
      if (value.GetType() == targetType) return value;
      
      try {
        return Convert.ChangeType(value, targetType);
      } catch {
        return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
      }
    }
    
    object ConvertValueWithTruncation(object value, Type targetType) {
      if (value == null || targetType == null) return value;
      if (value.GetType() == targetType) return value;
      
      try {
        // Handle explicit casts to integer types with truncation (not rounding)
        if (targetType == typeof(int) && value is decimal dec) {
          return (int)dec; // Truncation
        }
        if (targetType == typeof(long) && value is decimal decLong) {
          return (long)decLong; // Truncation
        }
        if (targetType == typeof(short) && value is decimal decShort) {
          return (short)decShort; // Truncation
        }
        if (targetType == typeof(byte) && value is decimal decByte) {
          return (byte)decByte; // Truncation
        }
        
        // For other conversions, use normal Convert.ChangeType
        return Convert.ChangeType(value, targetType);
      } catch {
        return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
      }
    }
    
    void SubscribeToNestedProperties(object sourceObj, string memberPath, PropertyChangedEventHandler handler, List<(INotifyPropertyChanged, PropertyChangedEventHandler)> handlers) {
      try {
        var parts = memberPath.Split('.');
        object current = sourceObj;
        
        for (int i = 0; i < parts.Length - 1; i++) {
          if (current == null) break;
          
          var property = current.GetType().GetProperty(parts[i]);
          if (property == null) break;
          
          var nestedObject = property.GetValue(current, null);
          if (nestedObject is INotifyPropertyChanged nestedNotify) {
            nestedNotify.PropertyChanged += handler;
            handlers.Add((nestedNotify, handler));
          }
          current = nestedObject;
        }
      } catch {
        // Ignore subscription errors
      }
    }
    
    void RebuildNestedSubscriptions(object sourceObj, string memberPath, PropertyChangedEventHandler handler, List<(INotifyPropertyChanged, PropertyChangedEventHandler)> handlers) {
      try {
        // First, unsubscribe from all existing nested object handlers (but keep the main source handler)
        var mainSource = sourceObj as INotifyPropertyChanged;
        for (int i = handlers.Count - 1; i >= 0; i--) {
          var (notifier, existingHandler) = handlers[i];
          if (notifier != mainSource && ReferenceEquals(existingHandler, handler)) {
            try {
              notifier.PropertyChanged -= handler;
              handlers.RemoveAt(i);
            } catch {
              // Ignore unsubscribe errors
            }
          }
        }
        
        // Now re-subscribe to the current nested objects
        SubscribeToNestedProperties(sourceObj, memberPath, handler, handlers);
      } catch {
        // Ignore rebuild errors
      }
    }
    
    bool HasCastExpression(Expression expr) {
      // Check if the expression contains a Convert/Cast operation
      return expr switch {
        UnaryExpression { NodeType: ExpressionType.Convert } => true,
        _ => false
      };
    }
    
    void AddControlToSourceHandlers(Control control, object source, string propertyName, string dataMember) {
      switch (propertyName) {
        case "Text" when control is TextBox textBox:
          textBox.TextChanged += (s, e) => UpdateSourceFromControl(source, dataMember, textBox.Text);
          break;
        case "Value" when control is NumericUpDown numericUpDown:
          numericUpDown.ValueChanged += (s, e) => UpdateSourceFromControl(source, dataMember, numericUpDown.Value);
          break;
      }
    }
    
    void UpdateSourceFromControl(object source, string dataMember, object value) {
      try {
        if (dataMember.Contains(".")) {
          var parts = dataMember.Split('.');
          object current = source;
          
          for (int i = 0; i < parts.Length - 1; i++) {
            var property = current?.GetType().GetProperty(parts[i]);
            current = property?.GetValue(current, null);
            if (current == null) return;
          }
          
          var finalProperty = current?.GetType().GetProperty(parts[parts.Length - 1]);
          if (finalProperty?.CanWrite == true) {
            var convertedValue = ConvertValue(value, finalProperty.PropertyType);
            finalProperty.SetValue(current, convertedValue, null);
          }
        } else {
          var sourceProperty = source.GetType().GetProperty(dataMember);
          if (sourceProperty?.CanWrite == true) {
            var convertedValue = ConvertValue(value, sourceProperty.PropertyType);
            sourceProperty.SetValue(source, convertedValue, null);
          }
        }
      } catch {
        // Ignore conversion errors
      }
    }
    
    object EvaluateSourceExpression(Expression expr, object sourceObj) {
      // Handle method calls like ToString()
      if (expr is MethodCallExpression methodCall && 
          methodCall.Method.Name == nameof(ToString) && 
          methodCall.Arguments.Count == 0 &&
          methodCall.Object is MemberExpression methodMember &&
          methodMember.Expression is ParameterExpression methodParam &&
          methodParam.Type == sourceType) {
        
        var sourceValue = sourceObj.GetType().GetProperty(methodMember.Member.Name)?.GetValue(sourceObj, null);
        return sourceValue?.ToString();
      }
      
      // Handle the complex double-conversion case: Convert(Convert(src.Count, Int32), Decimal)
      if (expr is UnaryExpression { NodeType: ExpressionType.Convert } outerConvert &&
          outerConvert.Operand is UnaryExpression { NodeType: ExpressionType.Convert } innerConvert &&
          innerConvert.Operand is MemberExpression doubleMember &&
          doubleMember.Expression is ParameterExpression doubleParam &&
          doubleParam.Type == sourceType) {
        
        // Get source value and apply the inner conversion (the cast that was explicitly written)
        var sourceValue = sourceObj.GetType().GetProperty(doubleMember.Member.Name)?.GetValue(sourceObj, null);
        if (sourceValue != null) {
          return ConvertValueWithTruncation(sourceValue, innerConvert.Type); // Use truncation for explicit casts
        }
      }
      
      // Handle nested conversions - unwrap to find the member expression
      Expression current = expr;
      Type targetType = null;
      
      // Unwrap nested Convert expressions and track the final target type
      while (current is UnaryExpression { NodeType: ExpressionType.Convert } unary) {
        targetType = unary.Type;
        current = unary.Operand;
      }
      
      // Handle member expressions (both simple and nested)
      if (current is MemberExpression member) {
        // Check if it's a simple member: src.Property
        if (member.Expression is ParameterExpression param && param.Type == sourceType) {
          var sourceValue = sourceObj.GetType().GetProperty(member.Member.Name)?.GetValue(sourceObj, null);
          if (sourceValue != null && targetType != null) {
            // Apply the explicit cast with truncation
            return ConvertValueWithTruncation(sourceValue, targetType);
          }
          return sourceValue;
        }
        
        // Handle nested properties: src.Nested.Property
        var propertyPath = GetMemberExpressionPath(member);
        if (propertyPath != null) {
          var nestedValue = GetSourceValue(sourceObj, propertyPath);
          if (nestedValue != null && targetType != null) {
            return ConvertValueWithTruncation(nestedValue, targetType);
          }
          return nestedValue;
        }
      }
      
      // Handle nested properties using dataMember path
      if (dataMember.Contains(".")) {
        return GetSourceValue(sourceObj, dataMember);
      }
      
      // Fall back to getting raw value
      return GetSourceValue(sourceObj, dataMember);
    }
    
    string GetMemberExpressionPath(MemberExpression memberExpr) {
      var parts = new List<string>();
      var current = memberExpr;
      
      while (current != null) {
        parts.Add(current.Member.Name);
        
        if (current.Expression is ParameterExpression param && param.Type == sourceType) {
          // We've reached the source parameter, build the path
          parts.Reverse();
          return string.Join(".", parts.ToArray());
        }
        
        current = current.Expression as MemberExpression;
      }
      
      return null; // Couldn't build a valid path to source parameter
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