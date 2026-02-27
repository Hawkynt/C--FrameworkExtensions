
# Extensions to WindowsForms

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/System.Windows.Forms.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.System.Windows.Forms)](https://www.nuget.org/packages/FrameworkExtensions.System.Windows.Forms/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

A comprehensive C# library of extension methods, custom controls, attribute-based rendering systems, and charting/diagramming components for Windows Forms. Part of [Hawkynt's .NET Framework Extensions](https://github.com/Hawkynt/C--FrameworkExtensions).

**Target Frameworks:** .NET Framework 3.5, 4.0, 4.5, 4.8 | .NET Core 3.1 | .NET 6.0 (Windows)

## How It Works

This library enhances Windows Forms development in four main ways:

1. **Extension methods** on standard WinForms types (`Control`, `DataGridView`, `ComboBox`, `ListView`, `TextBox`, `RichTextBox`, `TreeView`, `ProgressBar`, `NumericUpDown`, `TabControl`, `TableLayoutPanel`, `ToolStrip*`, `ErrorProvider`, `ImageList`, `Binding`, `ToolTip`) providing commonly needed functionality.
2. **An attribute-based rendering system** for `DataGridView`, `ListView`, `ListBox`, and `ComboBox` enabling declarative column configuration, cell styling, progress bars, images, and more -- driven by attributes on data model classes.
3. **custom controls** implementing modern UI patterns (toggle switches, ribbon controls, breadcrumbs, toast notifications, carousels, gauges, schedulers, wizards, etc.).
4. **A full charting and diagramming framework** (`AdvancedChart` with different chart types, `DiagramControl` with various diagram types, `SimpleChart`) rendered natively in GDI+.

## Build

```bash
dotnet build System.Windows.Forms.Extensions.csproj
```

Depends on sibling projects `Backports` and `Corlib.Extensions`. On non-Windows platforms, `<EnableWindowsTargeting>true</EnableWindowsTargeting>` is set in the `.csproj`.

# Features

## Control Extensions

- **IsDesignMode**: Detects if a control is in design mode (even within the constructor), useful for conditionally executing code only at runtime. (`Control.IsDesignMode`)

  ```cs
  if (this.IsDesignMode()) {
    // Execute design-time logic
  }
  ```

- **ISuspendedLayoutToken**: Facilitates the suspension and resumption of layout logic on controls, improving performance during batch updates. (`Control.PauseLayout`)

  WinForms recalculates layout (size, position, anchoring, docking) every time a child control is added, removed, or resized. When adding many controls in a loop, this triggers hundreds of redundant layout passes, causing visible flickering and degraded performance. `PauseLayout` returns a RAII token that calls `SuspendLayout()` on creation and `ResumeLayout(true)` on disposal, guaranteeing layout is always resumed -- even if an exception is thrown.

  ```csharp
  // Without the token: layout recalculates after EVERY Add() call - causes flickering
  // With the token: layout is suspended, all controls are added, then layout runs once
  using (panel.PauseLayout()) {
    for (var i = 0; i < 100; ++i)
      panel.Controls.Add(new Label { Text = $"Item {i}", Dock = DockStyle.Top });
  } // layout resumes here automatically, computing positions only once

  // Also useful when changing multiple layout-affecting properties at once
  using (form.PauseLayout()) {
    form.Size = new Size(800, 600);
    form.Controls.Add(toolbar);
    form.Controls.Add(statusBar);
    form.Controls.Add(mainPanel);
  }
  ```

- **ISuspendedRedrawToken**: Similar to `ISuspendedLayoutToken`, but suspends and resumes redraw operations to avoid flickering during updates. (`Control.PauseRedraw`)

  Even after layout is suspended, controls may still repaint on each change. `PauseRedraw` sends the Win32 `WM_SETREDRAW(false)` message to the control's window handle, which tells the OS to suppress all visual updates. On disposal it sends `WM_SETREDRAW(true)` to re-enable painting. This is the most effective way to eliminate flickering when performing bulk modifications to any control, and it works at a lower level than `SuspendLayout` -- it prevents the control from painting entirely.

  ```csharp
  // Prevents the control from repainting until all changes are done
  using (listView.PauseRedraw()) {
    listView.Items.Clear();
    foreach (var item in newItems)
      listView.Items.Add(item);
  } // control repaints once here with all changes visible

  // Combine with PauseLayout for maximum performance on heavy updates
  using (panel.PauseRedraw())
  using (panel.PauseLayout()) {
    panel.Controls.Clear();
    foreach (var widget in BuildDashboardWidgets())
      panel.Controls.Add(widget);
  } // layout recalculates once, then a single repaint occurs
  ```

- **Bindings**: Allows using Lambdas to add bindings for easier Model-View-ViewModel architecture (MVVM). This feature facilitates the binding of control properties to model properties using lambda expressions, ensuring a cleaner and more maintainable codebase. Ensure to use the equality (`==`) operator as assignments (`=`) are not supported in expression trees. (`Control.AddBinding`)

  ```csharp
  // Bind the model's LabelText property to the label's Text property.
  label.AddBinding(model, (c, m) => c.Text == m.LabelText);
  ```

  **Complete Real-World Example**:

  Traditional WinForms data binding requires string-based property names that are fragile and hard to refactor. `AddBinding` uses lambda expressions so the compiler catches typos, and renaming a property with F2 updates bindings automatically. Comparison operators control the data-flow direction, similar to WPF's `BindingMode`.

  ```csharp
  // Model class
  public class SettingsModel : INotifyPropertyChanged {
    public string UserName { get; set; }
    public int RefreshInterval { get; set; }
    public bool IsAutoSaveEnabled { get; set; }
    public string StatusMessage { get; set; }
    // INotifyPropertyChanged implementation omitted for brevity
  }

  // In Form_Load or InitializeComponent:
  var model = new SettingsModel();

  // Two-way: edits in the TextBox update the model and vice versa
  txtUserName.AddBinding(model, (c, m) => c.Text == m.UserName);

  // Two-way with type conversion: decimal control property <-> int model property
  nudInterval.AddBinding(model, (c, m) => c.Value == (decimal)m.RefreshInterval);

  // Two-way: CheckBox.Checked <-> bool property
  chkAutoSave.AddBinding(model, (c, m) => c.Checked == m.IsAutoSaveEnabled);

  // One-way source-to-control (read-only label): model updates the label, not the reverse
  lblStatus.AddBinding(model, (c, m) => c.Text < m.StatusMessage);
  ```

  **Supported Binding Expressions**:

  The `AddBinding` method supports a variety of expressions for binding control properties to model data members. Here are some examples:
  
  - **Direct Property Binding**:

    ```csharp
    (control, source) => control.propertyName == source.dataMember
    ```

    Example:

    ```csharp
    label.AddBinding(model, (c, m) => c.Text == m.LabelText);
    ```

  - **Type Conversion Binding**:

    ```csharp
    (control, source) => control.propertyName == (type)source.dataMember
    ```

    Example:

    ```csharp
    numericUpDown.AddBinding(model, (c, m) => c.Value == (decimal)m.NumericValue);
    ```

  - **String Conversion Binding**:

    ```csharp
    (control, source) => control.propertyName == source.dataMember.ToString()
    ```

    Example:

    ```csharp
    textBox.AddBinding(model, (c, m) => c.Text == m.IntegerValue.ToString());
    ```

  - **Nested Property Binding**:

    ```csharp
    (control, source) => control.propertyName == source.subMember.dataMember
    ```

    Example:

    ```csharp
    label.AddBinding(model, (c, m) => c.Text == m.SubModel.SubLabelText);
    ```

  - **Nested Type Conversion Binding**:

    ```csharp
    (control, source) => control.propertyName == (type)source.subMember.dataMember
    ```

    Example:

    ```csharp
    progressBar.AddBinding(model, (c, m) => c.Value == (int)m.SubModel.ProgressValue);
    ```

  - **Nested String Conversion Binding**:

    ```csharp
    (control, source) => control.propertyName == source.subMember.dataMember.ToString()
    ```

    Example:

    ```csharp
    comboBox.AddBinding(model, (c, m) => c.SelectedItem == m.SubModel.SelectedValue.ToString());
    ```

  - **Deeply Nested Property Binding**:

    ```csharp
    (control, source) => control.propertyName == source.....dataMember
    ```

    Example:

    ```csharp
    label.AddBinding(model, (c, m) => c.Text == m.Level1.Level2.Level3.Text);
    ```

  - **Deeply Nested Type Conversion Binding**:

    ```csharp
    (control, source) => control.propertyName == (type)source.....dataMember
    ```

    Example:

    ```csharp
    slider.AddBinding(model, (c, m) => c.Value == (double)m.Level1.Level2.Level3.SliderValue);
    ```

  - **Deeply Nested String Conversion Binding**:

    ```csharp
    (control, source) => control.propertyName == source.....dataMember.ToString()
    ```

    Example:

    ```csharp
    listBox.AddBinding(model, (c, m) => c.SelectedItem == m.Level1.Level2.Level3.ItemValue.ToString());
    ```

  **Directional Binding**:

  The `AddBinding` method supports directional binding using comparison operators to control data flow direction, similar to WPF:

  - **Two-Way Binding** (Default):
    ```csharp
    // Data flows both directions: source ↔ control
    // Both forms are equivalent and supported:
    textBox.AddBinding(model, (c, m) => c.Text == m.Name);       // control == source
    textBox.AddBinding(model, (c, m) => m.Name == c.Text);       // source == control
    
    // Works with complex expressions too:
    textBox.AddBinding(model, (c, m) => c.Text == m.Count.ToString());     // control == source.ToString()
    textBox.AddBinding(model, (c, m) => m.Count.ToString() == c.Text);     // source.ToString() == control
    
    numericUpDown.AddBinding(model, (c, m) => c.Value == (decimal)m.IntValue);  // control == (cast)source
    numericUpDown.AddBinding(model, (c, m) => (decimal)m.IntValue == c.Value);  // (cast)source == control
    ```

  - **One-Way Source-to-Control** (Read-Only):
    ```csharp
    // Data flows only from source to control: source → control
    // Control changes do not update the source
    
    // Both forms are equivalent and supported:
    label.AddBinding(model, (c, m) => c.Text < m.Status);      // control < source
    label.AddBinding(model, (c, m) => m.Status > c.Text);      // source > control
    
    numericUpDown.AddBinding(model, (c, m) => c.Value < m.Count);  // control < source
    numericUpDown.AddBinding(model, (c, m) => m.Count > c.Value);  // source > control
    ```

  - **One-Way Control-to-Source** (Write-Only):
    ```csharp
    // Data flows only from control to source: control → source
    // Source changes do not update the control
    
    // Both forms are equivalent and supported:
    textBox.AddBinding(model, (c, m) => c.Text > m.UserInput);     // control > source
    textBox.AddBinding(model, (c, m) => m.UserInput < c.Text);     // source < control
    
    numericUpDown.AddBinding(model, (c, m) => c.Value > m.UserNumber); // control > source
    numericUpDown.AddBinding(model, (c, m) => m.UserNumber < c.Value); // source < control
    ```

  This feature provides fine-grained control over binding behavior and improves performance in scenarios where you need unidirectional data flow. The syntax is intuitive and closely resembles the mathematical relationship between the values.

- **UI-Threading**: Determine whether to use a controls' UI thread to execute code or not. (`Control.SafelyInvoke`/`Control.Async`)

  ```cs
  form.SafelyInvoke(() => {
      // UI thread safe code
  });
  ```

## DataGridView Extensions

### Info on used datatypes

- **methodname**: The name (=string) of an instance or static method, utilize `nameof()`-operator
- **propertyname**: The name (=string) of an instance or static property, utilize `nameof()`-operator
- **colorstring**: A serialized (=string) color, must be one of:
  - Hex-Digits between 0-F:
    - `#AARRGGBB`, e.g. `#CCFF9900`
    - `#RRGGBB`, e.g. `#FF9900`
    - `#ARGB`, e.g. `#CF90`
    - `#RGB`, e.g. `#F90`
  - Decimal-Digits between 0-255:
    - `Alpha, Red, Green, Blue`
    - `Red, Green, Blue`
  - Floating-Point between 0.0-1.0:
    - `Alpha, Red, Green, Blue`
    - `Red, Green, Blue`
  - Known Color Name
    - e.g `Red`, `Lime`, `DodgerBlue`, etc.

### Column Types

These extensions provide additional DataGridView column types, enhancing the functionality and interactivity of your grids.

#### BoundComboBox

A ComboBox column that supports data binding within cells.

| Parameter               | Type   | Target Signature    | Description                                                                                                                                                                         |
| ----------------------- | ------ | ------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| DataSourcePropertyName  | string | object propertyname | The name of the property that provides the data source for the ComboBox. This should be the name of a property in the data-bound object that returns a collection or list of items. |
| EnabledWhenPropertyName | string | bool propertyname   | The name of the property that determines whether the ComboBox is enabled. This should be a boolean property.                                                                        |
| ValueMember             | string | string propertyname | The name of the property in the data source items that provides the value for the ComboBox.                                                                                         |
| DisplayMember           | string | string propertyname | The name of the property in the data source items that provides the display text for the ComboBox.                                                                                  |

#### DateTimePicker

A DateTimePicker column for date selection.

#### DisableButton

A button column with disable functionality.

#### ImageAndText

A column that can display both images and text.

| Parameter | Type  | Target Signature | Description                                    |
| --------- | ----- | ---------------- | ---------------------------------------------- |
| Image     | Image | Image value      | The (default) image to display in the cell.    |
| ImageSize | Size  | Size value       | The size to which the image should be resized. |

#### MultiImage

Supports multiple images in a single cell.

| Parameter                     | Type    | Target Signature                                 | Description                                              |
| ----------------------------- | ------- | ------------------------------------------------ | -------------------------------------------------------- |
| ImageSizeInPixels             | int     | int value                                        | The size in pixels to which all images should be scaled. |
| Padding                       | Padding | Padding value                                    | The padding around each image.                           |
| Margin                        | Padding | Padding value                                    | The margin between images.                               |
| OnClickMethodName             | string  | void methodname(object record, int imageIndex)   | The method to execute when an image is clicked.          |
| ToolTipTextProviderMethodName | string  | string methodname(object record, int imageIndex) | The method that returns the tooltip text for an image.   |

#### NumericUpDown

A column with NumericUpDown control for numeric input.

| Parameter             | Type    | Target Signature | Description                                                                             |
| --------------------- | ------- | ---------------- | --------------------------------------------------------------------------------------- |
| DecimalPlaces         | int     | int value        | The number of decimal places to display.                                                |
| Increment             | decimal | decimal value    | The amount to increment or decrement the value when the up or down buttons are clicked. |
| Minimum               | decimal | decimal value    | The minimum value allowed.                                                              |
| Maximum               | decimal | decimal value    | The maximum value allowed.                                                              |
| UseThousandsSeparator | bool    | bool value       | Whether to use a thousands separator.                                                   |

#### ProgressBar

A column with a progress bar to visualize progress.

| Parameter | Type   | Target Signature | Description                            |
| --------- | ------ | ---------------- | -------------------------------------- |
| Minimum   | double | double value     | The minimum value of the progress bar. |
| Maximum   | double | double value     | The maximum value of the progress bar. |

### Attributes

These can be used on the data records used as `DataSource` for the `DataGridView`.

**Complete Example -- Attribute-Driven DataGridView**

The following example demonstrates how to define a data model class with attributes that control column types, cell styles, row styles, tooltips, click handlers, and conditional formatting. Once `EnableExtendedAttributes()` is called, the `DataGridView` reads these attributes and automatically configures columns, cell rendering, and behavior -- no manual column setup required.

```csharp
// 1. Define the data model with attributes

[DataGridViewRowStyle(
  ForeColor = "Black",
  BackColor = "White"
)]
[DataGridViewRowStyle(
  ForeColor = "White",
  BackColor = "Firebrick",
  ConditionalPropertyName = nameof(IsOverdue)
)]
[DataGridViewRowHeight(HeightInPixel = 32)]
public class OrderRow {

  [DataGridViewColumnWidth(WidthInPixels = 80)]
  [DataGridViewCellStyle(Alignment = DataGridViewContentAlignment.MiddleRight)]
  public int OrderId { get; set; }

  [DataGridViewColumnWidth(WidthInPixels = 200)]
  [DataGridViewCellTooltip(ToolTipTextPropertyName = nameof(CustomerTooltip))]
  [DataGridViewClickable(OnDoubleClickMethodName = nameof(OpenCustomerDetail))]
  public string CustomerName { get; set; }

  [DataGridViewColumnWidth(WidthInPixels = 100)]
  [DataGridViewCellStyle(Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight)]
  [DataGridViewCellStyle(
    ForeColor = "Green",
    ConditionalPropertyName = nameof(IsHighValue)
  )]
  public decimal Total { get; set; }

  [DataGridViewProgressBarColumn(Minimum = 0, Maximum = 100)]
  [DataGridViewColumnWidth(WidthInPixels = 120)]
  public double CompletionPercent { get; set; }

  [DataGridViewButtonColumn(
    OnClickMethodName = nameof(MarkAsShipped),
    IsEnabledWhenPropertyName = nameof(CanShip)
  )]
  public string ShipAction => "Ship";

  // Supporting properties (not displayed as columns themselves)
  [Browsable(false)] public bool IsOverdue => DueDate < DateTime.Today && CompletionPercent < 100;
  [Browsable(false)] public bool IsHighValue => Total > 1000m;
  [Browsable(false)] public bool CanShip => CompletionPercent >= 100;
  [Browsable(false)] public string CustomerTooltip => $"Double-click to view {CustomerName}'s details";
  [Browsable(false)] public DateTime DueDate { get; set; }

  public void OpenCustomerDetail() { /* navigate to customer */ }
  public void MarkAsShipped() { /* update shipping status */ }
}

// 2. Wire it up in your Form

dataGridView.EnableExtendedAttributes();  // must be called once
dataGridView.DataSource = new BindingList<OrderRow>(orders);
// That's it -- columns, styles, buttons, progress bars, and tooltips are all automatic
```

#### Record-Based (Applies to Full Row)

##### DataGridViewConditionalRowHidden

Conditionally hides rows based on specified criteria.

| Parameter    | Type   | Target Signature  | Description                                                                        |
| ------------ | ------ | ----------------- | ---------------------------------------------------------------------------------- |
| IsHiddenWhen | string | bool propertyname | The name of the boolean property that determines whether the row should be hidden. |

##### DataGridViewFullMergedRow

Merges multiple cells into a single cell spanning multiple columns.

| Parameter               | Type   | Target Signature    | Description                                                                                    |
| ----------------------- | ------ | ------------------- | ---------------------------------------------------------------------------------------------- |
| HeadingTextPropertyName | string | string propertyname | The name of the property that provides the heading text for the merged row.                    |
| ForeColor               | string | colorstring         | The foreground color for the merged row, specified as a color string (e.g., "Red", "#FF0000"). |
| TextSize                | float  | float value         | The size of the text in the merged row.                                                        |

##### DataGridViewRowHeight

Sets the height of the rows.

| Parameter                | Type   | Target Signature  | Description                                                                            |
| ------------------------ | ------ | ----------------- | -------------------------------------------------------------------------------------- |
| HeightInPixel            | int    | int value         | The height of the row in pixels.                                                       |
| RowHeightEnabledProperty | string | bool propertyname | The name of the boolean property that determines whether custom row height is enabled. |
| RowHeightProperty        | string | int propertyname  | The name of the property that provides the custom row height.                          |

##### DataGridViewRowSelectable

Specifies if the row can be selected.

| Parameter         | Type   | Target Signature  | Description                                                                       |
| ----------------- | ------ | ----------------- | --------------------------------------------------------------------------------- |
| ConditionProperty | string | bool propertyname | The name of the boolean property that determines whether the row can be selected. |

##### DataGridViewRowStyle

Applies a specific style to the entire row.

| Parameter               | Type   | Target Signature    | Description                                                                                  |
| ----------------------- | ------ | ------------------- | -------------------------------------------------------------------------------------------- |
| ForeColor               | string | colorstring         | The foreground color of the row, specified as a color string (e.g., "Blue", "#0000FF").      |
| BackColor               | string | colorstring         | The background color of the row, specified as a color string (e.g., "LightGray", "#D3D3D3"). |
| Format                  | string | string value        | The format string applied to the row's content (e.g., "C2" for currency).                    |
| ConditionalPropertyName | string | bool propertyname   | The name of the boolean property that determines whether the style should be applied.        |
| ForeColorPropertyName   | string | Color? propertyname | The name of the property that provides the foreground color for the row.                     |
| BackColorPropertyName   | string | Color? propertyname | The name of the property that provides the background color for the row.                     |
| IsBold                  | bool   | bool value          | Whether the row's text should be bold.                                                       |
| IsItalic                | bool   | bool value          | Whether the row's text should be italic.                                                     |
| IsStrikeout             | bool   | bool value          | Whether the row's text should be struck out.                                                 |
| IsUnderline             | bool   | bool value          | Whether the row's text should be underlined.                                                 |

#### Property-Based (Applies to Cell)

##### DataGridViewConditionalReadOnly

Makes cells read-only based on specified conditions.

| Parameter      | Type   | Target Signature  | Description                                                                     |
| -------------- | ------ | ----------------- | ------------------------------------------------------------------------------- |
| IsReadOnlyWhen | string | bool propertyname | The name of the boolean property that determines whether the cell is read-only. |

##### DataGridViewSupportsConditionalImage

Shows an image next to a cell's text based on specified conditions.

| Parameter               | Type   | Target Signature   | Description                                                                             |
| ----------------------- | ------ | ------------------ | --------------------------------------------------------------------------------------- |
| ImagePropertyName       | string | Image propertyname | The name of the property that provides the image for the cell.                          |
| ConditionalPropertyName | string | bool propertyname  | The name of the boolean property that determines whether the image should be displayed. |

##### DataGridViewCellDisplayText

Sets the display text of a cell.

| Parameter    | Type   | Target Signature    | Description                                                           |
| ------------ | ------ | ------------------- | --------------------------------------------------------------------- |
| PropertyName | string | string propertyname | The name of the property that provides the display text for the cell. |

##### DataGridViewCellStyle

Applies a specific style to a cell.

| Parameter               | Type                         | Target Signature                   | Description                                                                               |
| ----------------------- | ---------------------------- | ---------------------------------- | ----------------------------------------------------------------------------------------- |
| ForeColor               | string                       | colorstring                        | The foreground color of the cell, specified as a color string (e.g., "Black", "#000000"). |
| BackColor               | string                       | colorstring                        | The background color of the cell, specified as a color string (e.g., "White", "#FFFFFF"). |
| Format                  | string                       | string value                       | The format string applied to the cell's content (e.g., "C2" for currency).                |
| Alignment               | DataGridViewContentAlignment | DataGridViewContentAlignment value | The alignment of the cell's content.                                                      |
| WrapMode                | DataGridViewTriState         | DataGridViewTriState value         | Whether the cell's text should wrap.                                                      |
| ConditionalPropertyName | string                       | bool propertyname                  | The name of the boolean property that determines whether the style should be applied.     |
| ForeColorPropertyName   | string                       | Color? propertyname                | The name of the property that provides the foreground color for the cell.                 |
| BackColorPropertyName   | string                       | Color? propertyname                | The name of the property that provides the background color for the cell.                 |
| WrapModePropertyName    | string                       | DataGridViewTriState propertyname  | The name of the property that provides the wrap mode for the cell.                        |

##### DataGridViewCellTooltip

Sets a tooltip for the cell.

| Parameter               | Type   | Target Signature    | Description                                                                               |
| ----------------------- | ------ | ------------------- | ----------------------------------------------------------------------------------------- |
| ToolTipText             | string | string value        | The tooltip text for the cell.                                                            |
| ToolTipTextPropertyName | string | string propertyname | The name of the property that provides the tooltip text.                                  |
| ConditionalPropertyName | string | bool propertyname   | The name of the boolean property that determines whether the tooltip should be displayed. |
| Format                  | string | string value        | The format string applied to the tooltip text.                                            |

##### DataGridViewClickable

Makes the cell clickable and defines click behavior.

| Parameter               | Type   | Target Signature  | Description                                                          |
| ----------------------- | ------ | ----------------- | -------------------------------------------------------------------- |
| OnClickMethodName       | string | void methodname() | The name of the method to be called when the cell is clicked.        |
| OnDoubleClickMethodName | string | void methodname() | The name of the method to be called when the cell is double-clicked. |

##### DataGridViewColumnSortMode

Sets the sort mode for the column.

| Parameter | Type                       | Target Signature                 | Description                   |
| --------- | -------------------------- | -------------------------------- | ----------------------------- |
| SortMode  | DataGridViewColumnSortMode | DataGridViewColumnSortMode value | The sort mode for the column. |

##### DataGridViewColumnWidth

Sets the width of the column.

| Parameter      | Type                           | Target Signature                     | Description                                             |
| -------------- | ------------------------------ | ------------------------------------ | ------------------------------------------------------- |
| CharacterCount | char                           | char value                           | The number of characters to determine the column width. |
| Characters     | string                         | string value                         | The string used to determine the column width.          |
| WidthInPixels  | int                            | int value                            | The width of the column in pixels.                      |
| Mode           | DataGridViewAutoSizeColumnMode | DataGridViewAutoSizeColumnMode value | The auto-size mode for the column.                      |

#### Property-Based (Applies to Column Type)

##### DataGridViewButtonColumn

Generates a button column for the property.

| Parameter                 | Type   | Target Signature  | Description                                                                     |
| ------------------------- | ------ | ----------------- | ------------------------------------------------------------------------------- |
| OnClickMethodName         | string | void methodname() | The name of the method to be called when the button is clicked.                 |
| IsEnabledWhenPropertyName | string | bool propertyname | The name of the boolean property that determines whether the button is enabled. |

##### DataGridViewCheckboxColumn

Generates a checkbox column for the property.

No additional parameters required.

##### DataGridViewComboboxColumn

Generates a combobox column for the property.

| Parameter               | Type   | Target Signature    | Description                                                                                        |
| ----------------------- | ------ | ------------------- | -------------------------------------------------------------------------------------------------- |
| EnabledWhenPropertyName | string | bool propertyname   | The name of the boolean property that determines whether the combobox is enabled.                  |
| DataSourcePropertyName  | string | object propertyname | The name of the property that provides the data source for the combobox.                           |
| ValueMember             | string | string propertyname | The name of the property in the data source items that provides the value for the combobox.        |
| DisplayMember           | string | string propertyname | The name of the property in the data source items that provides the display text for the combobox. |

##### DataGridViewImageColumn

Generates an image column for the property.

| Parameter               | Type   | Target Signature       | Description                                                             |
| ----------------------- | ------ | ---------------------- | ----------------------------------------------------------------------- |
| ImageListPropertyName   | string | ImageList propertyname | The name of the property that provides the image list for the column.   |
| ToolTipTextPropertyName | string | string propertyname    | The name of the property that provides the tooltip text for the images. |
| OnClickMethodName       | string | void methodname()      | The name of the method to be called when the image is clicked.          |
| OnDoubleClickMethodName | string | void methodname()      | The name of the method to be called when the image is double-clicked.   |

##### DataGridViewMultiImageColumn

Generates a multi-image column for the property.

| Parameter                 | Type   | Target Signature               | Description                                                         |
| ------------------------- | ------ | ------------------------------ | ------------------------------------------------------------------- |
| OnClickMethodName         | string | void methodname(object, int)   | The name of the method to be called when an image is clicked.       |
| ToolTipProviderMethodName | string | string methodname(object, int) | The name of the method that provides the tooltip text for an image. |
| MaximumImageSize          | int    | int value                      | The maximum size of each image.                                     |
| PaddingLeft               | int    | int value                      | The left padding around the images.                                 |
| PaddingTop                | int    | int value                      | The top padding around the images.                                  |
| PaddingRight              | int    | int value                      | The right padding around the images.                                |
| PaddingBottom             | int    | int value                      | The bottom padding around the images.                               |
| MarginLeft                | int    | int value                      | The left margin between the images.                                 |
| MarginTop                 | int    | int value                      | The top margin between the images.                                  |
| MarginRight               | int    | int value                      | The right margin between the images.                                |
| MarginBottom              | int    | int value                      | The bottom margin between the images.                               |

##### DataGridViewNumericUpDownColumn

Generates a numericupdown column for the property.

| Parameter     | Type   | Target Signature | Description                                                                             |
| ------------- | ------ | ---------------- | --------------------------------------------------------------------------------------- |
| Minimum       | double | double value     | The minimum value for the numeric up-down control.                                      |
| Maximum       | double | double value     | The maximum value for the numeric up-down control.                                      |
| Increment     | double | double value     | The amount to increment or decrement the value when the up or down buttons are clicked. |
| DecimalPlaces | int    | int value        | The number of decimal places to display.                                                |

##### DataGridViewProgressBarColumn

Generates a progressbar column for the property.

| Parameter | Type   | Target Signature | Description                            |
| --------- | ------ | ---------------- | -------------------------------------- |
| Minimum   | double | double value     | The minimum value of the progress bar. |
| Maximum   | double | double value     | The maximum value of the progress bar. |

##### DataGridViewImageAndTextColumn

Generates an image and text column for the property.

| Parameter             | Type              | Target Signature        | Description                                                                     |
| --------------------- | ----------------- | ----------------------- | ------------------------------------------------------------------------------- |
| ImageListPropertyName | string            | ImageList propertyname  | The name of the property that provides the image list for the column.           |
| ImageKeyPropertyName  | string            | int/object propertyname | The name of the property that provides the key for the image in the image list. |
| ImagePropertyName     | string            | Image propertyname      | The name of the property that provides the image for the cell.                  |
| TextImageRelation     | TextImageRelation | TextImageRelation value | The relationship between the image and the text.                                |
| FixedImageWidth       | uint              | uint value              | The fixed width of the image.                                                   |
| FixedImageHeight      | uint              | uint value              | The fixed height of the image.                                                  |
| KeepAspectRatio       | bool              | bool value              | Whether to keep the aspect ratio of the image.                                  |

## List Control Extensions (ListView, ListBox, ComboBox)

Attribute-based data binding and styling for list controls, similar to DataGridView's declarative model.

### Quick Start

```csharp
// Shared attributes work with ListView, ListBox, and ComboBox
[ListItemStyle(foreColor: "Red", conditionalPropertyName: nameof(IsOnSale))]
[ListItemImage(imageListPropertyName: nameof(Icons), imageKeyPropertyName: nameof(StatusIcon))]
public class Product {
  [ListViewColumn("Name", Width = 200)]  // ListView only
  public string Name { get; set; }

  [ListViewColumn("Price", Width = 80, Alignment = HorizontalAlignment.Right)]  // ListView only
  [ListViewColumnColor(foreColor: "Green", conditionalPropertyName: nameof(IsDiscounted))]  // ListView only
  public decimal Price { get; set; }

  [ListViewColumn("Rating")]  // ListView only
  [ListViewRepeatedImage(nameof(StarImages), "star", 5)]  // ListView only
  public int Rating { get; set; }

  public bool IsOnSale { get; set; }
  public bool IsDiscounted { get; set; }
  public ImageList Icons { get; set; }
  public string StatusIcon { get; set; }
  public ImageList StarImages { get; set; }
}

// ListView
listView.EnableExtendedAttributes();
listView.SetDataSource(products);

// ListBox
listBox.EnableExtendedAttributes();
listBox.DataSource = products;

// ComboBox
comboBox.EnableExtendedAttributes();
comboBox.DataSource = products;
```

### Shared Attributes (All Controls)

#### ListItemStyleAttribute (Class-Level)

Applied to the data class to style the entire item row. Multiple attributes can be used with conditions.

| Parameter               | Type   | Target Signature   | Description                                                 |
| ----------------------- | ------ | ------------------ | ----------------------------------------------------------- |
| foreColor               | string | colorstring        | The foreground color for the item (e.g., "Red", "#FF0000"). |
| backColor               | string | colorstring        | The background color for the item.                          |
| foreColorPropertyName   | string | Color propertyname | Property that provides dynamic foreground color.            |
| backColorPropertyName   | string | Color propertyname | Property that provides dynamic background color.            |
| conditionalPropertyName | string | bool propertyname  | Only apply style when this boolean property is true.        |

```csharp
[ListItemStyle(foreColor: "Red", conditionalPropertyName: nameof(IsOverdue))]
[ListItemStyle(foreColor: "Green", conditionalPropertyName: nameof(IsComplete))]
public class Task { ... }
```

#### ListItemImageAttribute (Class-Level)

Applied to the data class to display an image next to each item.

| Parameter              | Type   | Target Signature       | Description                                         |
| ---------------------- | ------ | ---------------------- | --------------------------------------------------- |
| imageListPropertyName  | string | ImageList propertyname | Property providing the ImageList containing images. |
| imageKeyPropertyName   | string | string propertyname    | Property providing the image key.                   |
| imageIndexPropertyName | string | int propertyname       | Alternative: property providing the image index.    |

```csharp
[ListItemImage(imageListPropertyName: nameof(Icons), imageKeyPropertyName: nameof(StatusIcon))]
public class Task {
  public ImageList Icons { get; set; }
  public string StatusIcon { get; set; }
}
```

### ListView-Specific Attributes

#### ListViewColumnAttribute (Property-Level)

Defines column configuration for ListView in Details view.

| Parameter    | Type                | Description                                     |
| ------------ | ------------------- | ----------------------------------------------- |
| HeaderText   | string              | Column header text (defaults to property name). |
| Width        | int                 | Column width in pixels (-1 for auto).           |
| DisplayIndex | int                 | Column display order (-1 for natural order).    |
| Alignment    | HorizontalAlignment | Text alignment (Left, Center, Right).           |
| Visible      | bool                | Whether the column is visible.                  |
| Format       | string              | Format string for IFormattable values.          |

```csharp
[ListViewColumn("Price", Width = 80, Alignment = HorizontalAlignment.Right, Format = "C2")]
public decimal Price { get; set; }
```

#### ListViewColumnColorAttribute (Property-Level)

Applied to properties to override row colors for specific columns. Multiple attributes allowed.

| Parameter               | Type   | Target Signature   | Description                            |
| ----------------------- | ------ | ------------------ | -------------------------------------- |
| foreColor               | string | colorstring        | The foreground color for this column.  |
| backColor               | string | colorstring        | The background color for this column.  |
| foreColorPropertyName   | string | Color propertyname | Property for dynamic foreground color. |
| backColorPropertyName   | string | Color propertyname | Property for dynamic background color. |
| conditionalPropertyName | string | bool propertyname  | Only apply when this property is true. |

```csharp
// Row is red when overdue, but Status column can be orange when pending
[ListItemStyle(foreColor: "Red", conditionalPropertyName: nameof(IsOverdue))]
public class Task {
  [ListViewColumn("Status")]
  [ListViewColumnColor(foreColor: "Orange", conditionalPropertyName: nameof(IsPending))]
  public string Status { get; set; }

  public bool IsOverdue { get; set; }
  public bool IsPending { get; set; }
}
```

#### ListViewRepeatedImageAttribute (Property-Level)

Displays repeated images based on a numeric property value (e.g., star ratings).

| Parameter             | Type   | Description                                  |
| --------------------- | ------ | -------------------------------------------- |
| imageListPropertyName | string | Property name providing the ImageList.       |
| imageKey              | string | Key of the image to repeat in the ImageList. |
| maxCount              | int    | Maximum number of repetitions (default: 5).  |

**Special Features:**

- **Fractional values**: When the property value is a floating-point number (float, double, decimal), partial images are displayed. For example, a value of 3.5 shows 3 full stars and a half star (only the left portion of the fourth star is drawn).
- **Negative values**: When the property value is negative, images are rendered in grayscale. The absolute value determines the count. For example, -3 shows 3 grayscale stars, and -2.5 shows 2 full grayscale stars and a partial grayscale star.

```csharp
[ListViewColumn("Rating")]
[ListViewRepeatedImage(nameof(StarImages), "star", 5)]  // maxCount = 5
public double Rating { get; set; }  // Shows 0-5 stars based on value

// Examples:
// Rating = 4     -> 4 full stars
// Rating = 3.5   -> 3 full stars + half star
// Rating = -2    -> 2 grayscale stars (indicates negative/bad rating)
// Rating = -1.5  -> 1 full grayscale star + half grayscale star

public ImageList StarImages { get; set; }
```

### Extension Methods

#### All Controls

| Method                       | Description                                                                             |
| ---------------------------- | --------------------------------------------------------------------------------------- |
| `EnableExtendedAttributes()` | Enable attribute-based rendering (required for custom styling).                         |
| `PauseUpdates()`             | Returns `ISuspendedUpdateToken` to suspend updates during batch operations (see below). |
| `SelectAll()`                | Select all items (multi-select modes).                                                  |
| `SelectNone()`               | Deselect all items.                                                                     |
| `EnableDoubleBuffering()`    | Reduce flicker during rendering.                                                        |
| `GetBoundData<T>()`          | Get typed data objects from all items.                                                  |

**ISuspendedUpdateToken / PauseUpdates()**

`ListView`, `ListBox`, and `ComboBox` all support `BeginUpdate()` / `EndUpdate()` to batch visual updates. However, forgetting to call `EndUpdate()` -- especially when an exception interrupts the code -- leaves the control in a permanently frozen state. `PauseUpdates()` wraps this pair in a RAII token: `BeginUpdate()` is called on creation, and `EndUpdate()` is guaranteed on disposal.

```csharp
// ListView: bulk-loading items without per-item repainting
using (listView.PauseUpdates()) {
  listView.Items.Clear();
  foreach (var product in products)
    listView.Items.Add(new ListViewItem(new[] { product.Name, product.Price.ToString("C") }));
} // EndUpdate() called here -- the ListView repaints once with all items

// ListBox: replacing the entire item list efficiently
using (listBox.PauseUpdates()) {
  listBox.Items.Clear();
  foreach (var name in filteredNames)
    listBox.Items.Add(name);
} // single repaint with all items visible

// ComboBox: populating dropdown options
using (comboBox.PauseUpdates()) {
  comboBox.Items.Clear();
  foreach (var option in availableOptions)
    comboBox.Items.Add(option);
} // dropdown renders once with full list
```

#### ListView-Specific

| Method                           | Description                                                 |
| -------------------------------- | ----------------------------------------------------------- |
| `SetDataSource(items)`           | Set data source and auto-configure columns from attributes. |
| `GetDataSource()`                | Get the current data source.                                |
| `ConfigureColumnsFromType<T>()`  | Configure columns from `ListViewColumnAttribute`.           |
| `AddItem(text)`                  | Add item with text.                                         |
| `AddItem(text, subItems)`        | Add item with sub-items.                                    |
| `AddItem<T>(data)`               | Add item from data object using attributes.                 |
| `AddItems<T>(dataItems)`         | Add multiple items from data objects.                       |
| `RemoveWhere(predicate)`         | Remove items matching predicate.                            |
| `Filter(predicate)`              | Show only matching items.                                   |
| `FilterByText(searchText)`       | Filter by text (case-insensitive).                          |
| `ClearFilter()`                  | Restore all items after filtering.                          |
| `SortByColumn(index, ascending)` | Sort by column index.                                       |
| `GetSelectedItems<T>()`          | Get typed data from selected items.                         |
| `GetCheckedItems<T>()`           | Get typed data from checked items.                          |
| `CheckAll()` / `UncheckAll()`    | Check/uncheck all items.                                    |
| `InvertSelection()`              | Invert current selection.                                   |
| `SelectWhere(predicate)`         | Select items matching predicate.                            |
| `ScrollToItem(item)`             | Scroll to make item visible.                                |
| `ScrollToEnd()`                  | Scroll to last item.                                        |

#### ListBox-Specific

| Method                   | Description                               |
| ------------------------ | ----------------------------------------- |
| `GetSelectedItems<T>()`  | Get typed data from selected items.       |
| `GetSelectedItem<T>()`   | Get typed data from single selected item. |
| `SelectWhere(predicate)` | Select items matching predicate.          |
| `Filter(predicate)`      | Show only matching items.                 |
| `ClearFilter()`          | Restore all items after filtering.        |
| `ScrollToItem(item)`     | Scroll to make item visible.              |

#### ComboBox-Specific

| Method                   | Description                           |
| ------------------------ | ------------------------------------- |
| `GetSelectedItem<T>()`   | Get typed data from selected item.    |
| `SelectWhere(predicate)` | Select first item matching predicate. |
| `AutoAdjustWidth()`      | Adjust width to fit longest item.     |

## TextBox Extensions

- **ICaretPositionToken**: A RAII token that saves and restores the caret (cursor) position in a text field. (`TextBoxExtensions.SaveCaretPosition`)

  When programmatically modifying the text content of a `TextBox` -- for example, applying formatting, auto-completing, or transforming input -- the caret position is typically lost because setting `Text` resets it to position 0. This forces the user's cursor to jump unexpectedly, which is disruptive during active editing. `SaveCaretPosition()` captures the current caret coordinates on creation and restores them when the token is disposed, ensuring seamless text manipulation.

  ```csharp
  // Save caret position, modify text, restore caret position automatically
  using (TextBoxExtensions.SaveCaretPosition()) {
    textBox.Text = TransformText(textBox.Text);
  } // caret position restored here -- user sees no cursor jump

  // Useful during live formatting (e.g., auto-capitalizing input)
  using (TextBoxExtensions.SaveCaretPosition()) {
    textBox.Text = textBox.Text.ToUpperInvariant();
  }
  ```

- **MoveCursorToEnd**: Moves the cursor to the end of the text. Useful after programmatically appending text. (`TextBox.MoveCursorToEnd`)

  ```csharp
  textBox.Text += " appended text";
  textBox.MoveCursorToEnd(); // cursor is now after "appended text"
  ```

- **TryParseInt**: Safely parses the text content as an integer without throwing exceptions. (`TextBox.TryParseInt`)

  ```csharp
  var value = 0;
  if (textBox.TryParseInt(ref value))
    ProcessNumber(value);
  ```

## RichTextBox Extensions

- **Syntax Highlighting**: Adds syntax highlighting capabilities to RichTextBox controls, making it easier to implement features for code editors or similar applications.

## TabControl Extensions

- **Tab Headers**: Adds coloring and images to tab page headers.

## Custom Controls

Modern UI controls for Windows Forms applications.

### Input Controls

#### PlaceholderTextBox

A TextBox with watermark/placeholder text that disappears when the user types.

```csharp
var textBox = new PlaceholderTextBox {
  PlaceholderText = "Enter your email...",
  PlaceholderColor = SystemColors.GrayText
};
```

| Property         | Type   | Description                              |
| ---------------- | ------ | ---------------------------------------- |
| PlaceholderText  | string | The watermark text displayed when empty. |
| PlaceholderColor | Color  | The color of the placeholder text.       |

#### ToggleSwitch

iOS/Android-style on/off switch control.

```csharp
var toggle = new ToggleSwitch {
  Checked = true,
  OnColor = Color.DodgerBlue,
  OnText = "ON",
  OffText = "OFF"
};
toggle.CheckedChanged += (s, e) => Console.WriteLine($"Checked: {toggle.Checked}");
```

| Property          | Type   | Description                            |
| ----------------- | ------ | -------------------------------------- |
| Checked           | bool   | Gets or sets whether the switch is on. |
| OnColor           | Color  | The color when the switch is on.       |
| OffColor          | Color  | The color when the switch is off.      |
| ThumbColor        | Color  | The color of the sliding thumb.        |
| OnText            | string | Text displayed when on.                |
| OffText           | string | Text displayed when off.               |
| ShowText          | bool   | Whether to show on/off text.           |
| AnimateTransition | bool   | Whether to animate the toggle.         |

#### RatingControl

Star rating input/display control.

```csharp
var rating = new RatingControl {
  MaxRating = 5,
  Value = 3,
  AllowHalfStars = true
};
rating.ValueChanged += (s, e) => Console.WriteLine($"Rating: {rating.Value}");
```

| Property       | Type  | Description                           |
| -------------- | ----- | ------------------------------------- |
| Value          | int   | Current rating value.                 |
| MaxRating      | int   | Maximum number of stars (default: 5). |
| AllowHalfStars | bool  | Enable half-star ratings.             |
| ReadOnly       | bool  | Prevent user input.                   |
| FilledImage    | Image | Custom filled star image.             |
| EmptyImage     | Image | Custom empty star image.              |
| HalfImage      | Image | Custom half-star image.               |
| ImageSize      | int   | Size of each star in pixels.          |
| Spacing        | int   | Space between stars.                  |

#### RangeSlider

Dual-thumb slider for selecting a range of values.

```csharp
var slider = new RangeSlider {
  Minimum = 0,
  Maximum = 100,
  LowerValue = 25,
  UpperValue = 75
};
slider.RangeChanged += (s, e) => Console.WriteLine($"Range: {slider.LowerValue}-{slider.UpperValue}");
```

| Property      | Type        | Description                      |
| ------------- | ----------- | -------------------------------- |
| Minimum       | double      | Minimum value of the range.      |
| Maximum       | double      | Maximum value of the range.      |
| LowerValue    | double      | Current lower value.             |
| UpperValue    | double      | Current upper value.             |
| SmallChange   | double      | Increment for small adjustments. |
| LargeChange   | double      | Increment for large adjustments. |
| SnapToTicks   | bool        | Snap values to tick marks.       |
| TickFrequency | double      | Interval between tick marks.     |
| Orientation   | Orientation | Horizontal or Vertical.          |

#### SearchTextBox

TextBox with search icon, clear button, and debounced search events.

```csharp
var search = new SearchTextBox {
  PlaceholderText = "Search...",
  SearchDelay = 300
};
search.SearchTriggered += (s, e) => PerformSearch(search.Text);
```

| Property        | Type   | Description                                |
| --------------- | ------ | ------------------------------------------ |
| Text            | string | The current search text.                   |
| PlaceholderText | string | Watermark text when empty.                 |
| SearchIcon      | Image  | Custom search icon.                        |
| ShowClearButton | bool   | Show the clear (X) button.                 |
| SearchDelay     | int    | Milliseconds before SearchTriggered fires. |

#### ColorPickerButton

Button that opens a color picker dropdown.

```csharp
var picker = new ColorPickerButton {
  SelectedColor = Color.Blue,
  AllowCustomColor = true
};
picker.SelectedColorChanged += (s, e) => ApplyColor(picker.SelectedColor);
```

| Property         | Type    | Description                              |
| ---------------- | ------- | ---------------------------------------- |
| SelectedColor    | Color   | The currently selected color.            |
| StandardColors   | Color[] | Palette of available colors.             |
| RecentColors     | Color[] | Recently selected colors (auto-tracked). |
| MaxRecentColors  | int     | Maximum recent colors to track.          |
| AllowCustomColor | bool    | Show "More Colors..." option.            |
| ShowColorName    | bool    | Display color name on button.            |

### Display Controls

#### CircularProgressBar

Ring/donut-style progress indicator.

```csharp
var progress = new CircularProgressBar {
  Value = 75,
  Thickness = 10,
  ShowText = true,
  TextFormat = "{0:0}%"
};
```

| Property        | Type   | Description                              |
| --------------- | ------ | ---------------------------------------- |
| Value           | double | Current progress value.                  |
| Minimum         | double | Minimum value (default: 0).              |
| Maximum         | double | Maximum value (default: 100).            |
| Thickness       | int    | Width of the progress ring.              |
| ProgressColor   | Color  | Color of the progress arc.               |
| TrackColor      | Color  | Color of the background track.           |
| ShowText        | bool   | Display progress text in center.         |
| TextFormat      | string | Format string for text (e.g., "{0:0}%"). |
| IsIndeterminate | bool   | Animate without specific progress.       |

#### BadgeLabel

Label or icon with notification badge overlay.

```csharp
var badge = new BadgeLabel {
  Icon = myIcon,
  BadgeValue = 5,
  BadgeColor = Color.Red
};
```

| Property       | Type             | Description                            |
| -------------- | ---------------- | -------------------------------------- |
| Icon           | Image            | The main icon to display.              |
| Text           | string           | Text to display (alternative to icon). |
| BadgeValue     | int              | Number to show in badge.               |
| BadgeColor     | Color            | Background color of badge.             |
| BadgeTextColor | Color            | Text color of badge.                   |
| BadgePosition  | ContentAlignment | Position of badge (default: TopRight). |
| MaxBadgeValue  | int              | Shows "99+" above this value.          |
| HideWhenZero   | bool             | Hide badge when value is 0.            |

#### LoadingSpinner

Animated loading indicator with multiple styles.

```csharp
var spinner = new LoadingSpinner {
  Style = SpinnerStyle.Circle,
  SpinnerColor = Color.DodgerBlue,
  LoadingText = "Loading..."
};
spinner.Start();
```

| Property     | Type         | Description                       |
| ------------ | ------------ | --------------------------------- |
| IsSpinning   | bool         | Whether the spinner is active.    |
| Style        | SpinnerStyle | Circle, Dots, or Bars.            |
| SpinnerColor | Color        | Color of the spinner.             |
| Speed        | int          | Milliseconds per animation frame. |
| LoadingText  | string       | Optional text below spinner.      |

#### Gauge

Speedometer/dial gauge for displaying values.

```csharp
var gauge = new Gauge {
  Minimum = 0,
  Maximum = 100,
  Value = 65,
  Zones = new[] {
    new GaugeZone { Start = 0, End = 50, Color = Color.Green },
    new GaugeZone { Start = 50, End = 80, Color = Color.Yellow },
    new GaugeZone { Start = 80, End = 100, Color = Color.Red }
  }
};
```

| Property       | Type        | Description                 |
| -------------- | ----------- | --------------------------- |
| Value          | double      | Current value displayed.    |
| Minimum        | double      | Minimum scale value.        |
| Maximum        | double      | Maximum scale value.        |
| StartAngle     | double      | Starting angle in degrees.  |
| SweepAngle     | double      | Arc sweep in degrees.       |
| Zones          | GaugeZone[] | Colored zones on the dial.  |
| ShowTicks      | bool        | Display tick marks.         |
| MajorTickCount | int         | Number of major ticks.      |
| MinorTickCount | int         | Minor ticks between majors. |
| ShowValue      | bool        | Display value text.         |
| ValueFormat    | string      | Format string for value.    |
| Unit           | string      | Unit label (e.g., "km/h").  |

### Container Controls

#### CardControl

Material Design-style card container with shadow.

```csharp
var card = new CardControl {
  Title = "User Profile",
  ShowShadow = true,
  CornerRadius = 8
};
card.ContentPanel.Controls.Add(new Label { Text = "Content here" });
```

| Property     | Type   | Description                   |
| ------------ | ------ | ----------------------------- |
| Title        | string | Card title text.              |
| TitleIcon    | Image  | Icon next to title.           |
| ShowShadow   | bool   | Display drop shadow.          |
| ShadowDepth  | int    | Shadow offset in pixels.      |
| CornerRadius | int    | Corner rounding radius.       |
| CardColor    | Color  | Background color of card.     |
| ContentPanel | Panel  | Container for main content.   |
| ActionPanel  | Panel  | Container for action buttons. |

#### ExpanderControl

Collapsible panel with animated expand/collapse.

```csharp
var expander = new ExpanderControl {
  HeaderText = "Advanced Options",
  IsExpanded = false,
  AnimateExpansion = true
};
expander.ContentPanel.Controls.Add(optionsPanel);
expander.Expanded += (s, e) => Console.WriteLine("Expanded!");
```

| Property         | Type   | Description              |
| ---------------- | ------ | ------------------------ |
| HeaderText       | string | Text in the header bar.  |
| HeaderIcon       | Image  | Icon in the header.      |
| IsExpanded       | bool   | Current expansion state. |
| CollapsedHeight  | int    | Height when collapsed.   |
| ExpandedHeight   | int    | Height when expanded.    |
| AnimateExpansion | bool   | Animate height changes.  |
| ContentPanel     | Panel  | Container for content.   |

| Event      | Description                           |
| ---------- | ------------------------------------- |
| Expanded   | Fired after expanding.                |
| Collapsed  | Fired after collapsing.               |
| Expanding  | Fired before expanding (cancelable).  |
| Collapsing | Fired before collapsing (cancelable). |

#### AccordionPanel

Multiple collapsible sections (uses ExpanderControl internally).

```csharp
var accordion = new AccordionPanel {
  AllowMultipleExpanded = false
};
var section1 = accordion.AddSection("General");
section1.ContentPanel.Controls.Add(generalSettings);
var section2 = accordion.AddSection("Advanced");
section2.ContentPanel.Controls.Add(advancedSettings);
```

| Property              | Type               | Description                    |
| --------------------- | ------------------ | ------------------------------ |
| Sections              | AccordionSection[] | All sections in the accordion. |
| AllowMultipleExpanded | bool               | Allow multiple open sections.  |
| AnimateExpansion      | bool               | Animate section transitions.   |

#### WizardControl

Multi-step wizard with navigation and step indicator.

```csharp
var wizard = new WizardControl();
var page1 = wizard.AddPage("Welcome", "Get started with setup");
page1.ContentPanel.Controls.Add(welcomePanel);
var page2 = wizard.AddPage("Configuration", "Configure your settings");
page2.ContentPanel.Controls.Add(configPanel);
wizard.Finished += (s, e) => CompleteSetup();
```

| Property              | Type         | Description                    |
| --------------------- | ------------ | ------------------------------ |
| Pages                 | WizardPage[] | All wizard pages.              |
| CurrentPageIndex      | int          | Index of current page.         |
| CurrentPage           | WizardPage   | Current page object.           |
| ShowStepIndicator     | bool         | Show step progress at top.     |
| ShowNavigationButtons | bool         | Show Back/Next/Finish buttons. |
| NextButtonText        | string       | Text for Next button.          |
| BackButtonText        | string       | Text for Back button.          |
| FinishButtonText      | string       | Text for Finish button.        |

### Navigation Controls

#### BreadcrumbControl

Navigation breadcrumb trail with clickable items.

```csharp
var breadcrumb = new BreadcrumbControl();
breadcrumb.Push("Home");
breadcrumb.Push("Documents");
breadcrumb.Push("Reports");
breadcrumb.ItemClicked += (s, e) => NavigateTo(e.Index);
```

| Property       | Type                   | Description                          |
| -------------- | ---------------------- | ------------------------------------ |
| Items          | BreadcrumbItem[]       | Current breadcrumb items.            |
| Separator      | string                 | Text between items (default: " > "). |
| ClickableItems | bool                   | Allow clicking to navigate.          |
| OverflowMode   | BreadcrumbOverflowMode | Ellipsis, Scroll, or Wrap.           |

| Method            | Description               |
| ----------------- | ------------------------- |
| Push(text, tag)   | Add item to the end.      |
| Pop()             | Remove the last item.     |
| NavigateTo(index) | Remove items after index. |
| Clear()           | Remove all items.         |

#### ChipControl

Tag/chip collection with add, remove, and selection support.

```csharp
var chips = new ChipControl {
  AllowAdd = true,
  AllowRemove = true,
  SelectionMode = SelectionMode.MultiSimple
};
chips.AddChip("C#", Color.Purple);
chips.AddChip("WinForms", Color.Blue);
chips.ChipRemoved += (s, e) => Console.WriteLine($"Removed: {e.Chip.Text}");
```

| Property       | Type          | Description                |
| -------------- | ------------- | -------------------------- |
| Chips          | Chip[]        | All chips in the control.  |
| AllowAdd       | bool          | Allow adding new chips.    |
| AllowRemove    | bool          | Allow removing chips.      |
| AllowSelection | bool          | Enable chip selection.     |
| SelectionMode  | SelectionMode | None, One, or MultiSimple. |
| ChipSpacing    | int           | Space between chips.       |

| Method               | Description             |
| -------------------- | ----------------------- |
| AddChip(text, color) | Add a new chip.         |
| RemoveChip(chip)     | Remove a specific chip. |
| ClearChips()         | Remove all chips.       |
| GetSelectedChips()   | Get selected chips.     |

### Notification Controls

#### ToastNotification

Non-modal popup notifications with auto-dismiss.

```csharp
// Simple usage via ToastManager
ToastManager.Show("File saved successfully!", ToastType.Success);
ToastManager.Show("Connection lost", ToastType.Error, duration: 5000);

// Advanced options
ToastManager.Show(new ToastOptions {
  Title = "Update Available",
  Message = "Version 2.0 is ready to install",
  Type = ToastType.Info,
  Duration = 0,  // Persistent until clicked
  Position = ToastPosition.TopRight
});
```

| ToastManager Property | Type          | Description                  |
| --------------------- | ------------- | ---------------------------- |
| DefaultPosition       | ToastPosition | Default position for toasts. |
| MaxVisible            | int           | Maximum simultaneous toasts. |

| ToastType | Description                   |
| --------- | ----------------------------- |
| Info      | Informational message (blue). |
| Success   | Success message (green).      |
| Warning   | Warning message (yellow).     |
| Error     | Error message (red).          |

| ToastPosition | Description                    |
| ------------- | ------------------------------ |
| TopLeft       | Top-left corner.               |
| TopRight      | Top-right corner.              |
| TopCenter     | Top center.                    |
| BottomLeft    | Bottom-left corner.            |
| BottomRight   | Bottom-right corner (default). |
| BottomCenter  | Bottom center.                 |

#### TimelineControl

Vertical timeline for displaying events chronologically.

```csharp
var timeline = new TimelineControl {
  Layout = TimelineLayout.Alternating,
  NodeSize = 16
};
timeline.AddItem(DateTime.Now, "Project Started", "Initial commit");
timeline.AddItem(DateTime.Now.AddDays(7), "First Release", "v1.0.0");
timeline.ItemClicked += (s, e) => ShowDetails(e.Item);
```

| Property  | Type           | Description                  |
| --------- | -------------- | ---------------------------- |
| Items     | TimelineItem[] | All timeline items.          |
| Layout    | TimelineLayout | Left, Right, or Alternating. |
| LineColor | Color          | Color of the vertical line.  |
| NodeSize  | int            | Size of timeline nodes.      |

| TimelineItem Property | Type     | Description         |
| --------------------- | -------- | ------------------- |
| Date                  | DateTime | Event date/time.    |
| Title                 | string   | Event title.        |
| Description           | string   | Event description.  |
| Icon                  | Image    | Optional icon.      |
| NodeColor             | Color    | Color of this node. |
| Tag                   | object   | Custom data.        |

### Scheduling & Calendar Controls

#### SchedulerControl

Full-featured day/week/month appointment scheduler (Outlook-style).

```csharp
var scheduler = new SchedulerControl {
  ViewType = SchedulerViewType.Week,
  CurrentDate = DateTime.Today
};
scheduler.Items.Add(new SchedulerItem {
  Subject = "Team Meeting",
  Start = DateTime.Today.AddHours(10),
  End = DateTime.Today.AddHours(11),
  Color = Color.Blue
});
scheduler.ItemClicked += (s, e) => ShowAppointment(e.Item);
```

| Property     | Type              | Description                      |
| ------------ | ----------------- | -------------------------------- |
| ViewType     | SchedulerViewType | Day, Week, or Month view.        |
| CurrentDate  | DateTime          | The date currently being viewed. |
| Items        | SchedulerItem[]   | All appointments/events.         |
| WorkDayStart | TimeSpan          | Start of working hours.          |
| WorkDayEnd   | TimeSpan          | End of working hours.            |

#### MonthCalendarEx

Enhanced calendar with appointment markers and custom rendering.

```csharp
var calendar = new MonthCalendarEx {
  ShowWeekNumbers = true,
  ShowTodayCircle = true
};
calendar.DateMarkers.Add(new CalendarDateMarker {
  Date = DateTime.Today.AddDays(3),
  Color = Color.Red,
  Tooltip = "Meeting"
});
calendar.DateSelected += (s, e) => ShowDate(e.Date);
```

| Property        | Type                 | Description               |
| --------------- | -------------------- | ------------------------- |
| SelectedDate    | DateTime             | Currently selected date.  |
| ShowWeekNumbers | bool                 | Display ISO week numbers. |
| ShowTodayCircle | bool                 | Highlight today's date.   |
| DateMarkers     | CalendarDateMarker[] | Colored markers on dates. |

### Rich Navigation Controls

#### NavigationPane

Outlook-style collapsible navigation sidebar with groups and items.

```csharp
var navPane = new NavigationPane {
  ExpandedWidth = 200,
  CollapsedWidth = 48
};
var group = navPane.AddGroup("Mail", mailIcon);
group.AddItem("Inbox", inboxIcon);
group.AddItem("Sent", sentIcon);
navPane.ItemSelected += (s, e) => ShowContent(e.Item);
```

| Property        | Type                      | Description                    |
| --------------- | ------------------------- | ------------------------------ |
| DisplayMode     | NavigationPaneDisplayMode | Expanded, Collapsed, or Auto.  |
| ExpandedWidth   | int                       | Width when expanded.           |
| CollapsedWidth  | int                       | Width when collapsed.          |
| AllowCollapse   | bool                      | Allow user to collapse/expand. |
| AnimateCollapse | bool                      | Animate collapse transitions.  |
| Groups          | NavigationGroup[]         | Navigation groups.             |

#### RibbonControl

Office-style ribbon interface with tabs, groups, and button types.

```csharp
var ribbon = new RibbonControl();
var homeTab = ribbon.AddTab("Home");
var clipboardGroup = homeTab.AddGroup("Clipboard");
clipboardGroup.AddButton("Paste", pasteIcon, RibbonButtonStyle.Large);
clipboardGroup.AddButton("Cut", cutIcon, RibbonButtonStyle.Small);
ribbon.ItemClicked += (s, e) => HandleAction(e.Item);
```

| Property              | Type                     | Description                      |
| --------------------- | ------------------------ | -------------------------------- |
| Tabs                  | RibbonTab[]              | Ribbon tabs.                     |
| SelectedTabIndex      | int                      | Active tab index.                |
| ShowApplicationButton | bool                     | Show the "File" button.          |
| ApplicationButtonText | string                   | Text for application button.     |
| ApplicationMenu       | ContextMenuStrip         | Menu for application button.     |
| Minimized             | bool                     | Whether the ribbon is collapsed. |
| QuickAccessToolbar    | RibbonQuickAccessToolbar | Quick access items.              |

#### CarouselControl

Image/content carousel with transitions.

```csharp
var carousel = new CarouselControl {
  AutoPlay = true,
  TransitionDuration = 500,
  TransitionType = CarouselTransition.SlideLeft
};
carousel.Items.Add(new CarouselItem { Image = image1, Text = "Slide 1" });
carousel.Items.Add(new CarouselItem { Image = image2, Text = "Slide 2" });
```

| Property              | Type               | Description                   |
| --------------------- | ------------------ | ----------------------------- |
| Items                 | CarouselItem[]     | All carousel slides.          |
| CurrentIndex          | int                | Active slide index.           |
| AutoPlay              | bool               | Auto-advance slides.          |
| AutoPlayInterval      | int                | Milliseconds between slides.  |
| TransitionType        | CarouselTransition | Slide, Fade, Flip, Zoom, etc. |
| TransitionDuration    | int                | Animation duration in ms.     |
| ShowIndicators        | bool               | Show dot indicators.          |
| ShowNavigationButtons | bool               | Show prev/next arrows.        |

### ToolStrip Controls

#### ToolStripSpringTextBox

A `ToolStripTextBox` that automatically fills the remaining width of its parent `ToolStrip`.

```csharp
var toolStrip = new ToolStrip();
toolStrip.Items.Add(new ToolStripButton("File"));
toolStrip.Items.Add(new ToolStripSpringTextBox()); // fills remaining space
```

#### BindableToolStripButton

A `ToolStripButton` that implements `IBindableComponent` for WinForms data binding.

```csharp
var button = new BindableToolStripButton();
button.DataBindings.Add("Checked", viewModel, "IsFeatureEnabled");
```

#### BindableToolStripSpringTextBox

Combines `ToolStripSpringTextBox` behavior with `IBindableComponent` support.

## Charting Framework

### SimpleChart Control

A lightweight charting control supporting common chart types.

```csharp
var chart = new SimpleChart {
  Title = "Monthly Sales",
  ChartType = ChartType.Line,
  ShowLegend = true
};
var series = chart.AddSeries("2024");
series.AddPoint(1, 100);
series.AddPoint(2, 150);
series.AddPoint(3, 120);
```

**Supported SimpleChart Types:** Line, Bar, Pie, Area, Scatter, Radar

### AdvancedChart Control

A full-featured charting control in `System.Windows.Forms.Charting` supporting 100+ chart types:

```csharp
var chart = new AdvancedChart {
  ChartType = AdvancedChartType.Candlestick,
  Title = "Stock Prices"
};
chart.OHLCData.Add(new OHLCDataPoint {
  Date = DateTime.Today,
  Open = 100, High = 110, Low = 95, Close = 105
});
```

**Chart Type Categories:**

| Category            | Chart Types                                                                                                                                                                           |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Comparison          | Bar, Column, GroupedBar, GroupedColumn, StackedBar, StackedColumn, DivergingStackedBar, Lollipop, DotPlot, Dumbbell, Bullet, Radar, PolarArea, Nightingale, RangePlot, SmallMultiples |
| Trend / Time Series | Line, MultiLine, Spline, Area, StackedArea, Step, StepArea, StreamGraph, Sparkline, RangeArea, BumpArea, Barcode                                                                      |
| Part-to-Whole       | Pie, Donut, SemiCircleDonut, NestedDonut, Treemap, CircularTreemap, ConvexTreemap, Sunburst, Waffle, Icicle, Mosaic, Marimekko, Parliament, Unit                                      |
| Distribution        | Histogram, RadialHistogram, BoxPlot, Violin, Density, Beeswarm, StripPlot, JitterPlot, Ridgeline, Horizon, Cumulative, PopulationPyramid, OneDimensionalHeatmap                       |
| Correlation         | Scatter, CategoricalScatter, Bubble, ConnectedScatter, Heatmap, Correlogram, ScatterMatrix, Hexbin, Contour, QuadrantChart, MatrixChart                                               |
| Ranking             | OrderedBar, Slope, Bump, ParallelCoordinates, RadialBar, TableHeatmap, TableChart                                                                                                     |
| Geospatial          | Choropleth, GeographicHeatmap, TileMap, BubbleMap, ConnectionMap, DotMap                                                                                                              |
| Temporal            | Timeline, Gantt, CalendarHeatmap, Seasonal, Spiral                                                                                                                                    |
| Financial           | Candlestick, OHLC, Kagi, Renko, Waterfall, PointFigure                                                                                                                                |
| Specialized         | Funnel, Pyramid, Gauge, CircularGauge, WordCloud, Pictogram, Venn, EulerDiagram, IconArray                                                                                            |

### DiagramControl

A structural and relational diagram control in `System.Windows.Forms.Charting.Diagrams`:

```csharp
var diagram = new DiagramControl {
  DiagramType = DiagramType.Sankey,
  Title = "Energy Flow"
};
diagram.Nodes.Add(new DiagramNode("A", "Source A"));
diagram.Nodes.Add(new DiagramNode("B", "Target B"));
diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "B", 100));
```

**Supported Diagram Types:** Sankey, Chord, Network/Force-directed, Tree/Hierarchy, Flowcharts, Class diagrams, Sequence diagrams, Use Case diagrams, Activity/Swimlane diagrams, Component diagrams, Deployment diagrams, ER diagrams, Data Flow diagrams, Venn diagrams, Quadrant charts, Matrix diagrams, Journey maps, BPMN diagrams, Kanban boards, Git branch graphs, Requirement diagrams, Rack diagrams, Packet diagrams

## Additional Extension Methods

### Image Extensions (`System.Drawing`)

| Method                                         | Description                                                             |
| ---------------------------------------------- | ----------------------------------------------------------------------- |
| `PrintImageWithDialog(documentName?, dialog?)` | Prints the image using a print dialog and returns the `PrinterSettings` |
| `PrintImage(documentName?, settings?)`         | Prints the image directly with optional printer settings                |

### ProgressBar Extensions

| Method                        | Description                                                       |
| ----------------------------- | ----------------------------------------------------------------- |
| `SetPercent(percentage)`      | Sets value as a percentage (0-100)                                |
| `SetNormalizedValue(value)`   | Sets value as a normalized ratio (0.0-1.0)                        |
| `SetValue(current, max)`      | Sets value relative to a maximum without changing control min/max |
| `SetValue(current, min, max)` | Sets value relative to a custom range                             |

### NumericUpDown Extensions

| Method                          | Description                                                                          |
| ------------------------------- | ------------------------------------------------------------------------------------ |
| `SetMinMaxStep(min, max, step)` | Sets minimum, maximum, and increment (decimal/double/int overloads)                  |
| `SetSaveValue(value)`           | Sets value clamped to the valid range (decimal/double/int/uint/long/ulong overloads) |

### ErrorProvider Extensions

| Method           | Description                           |
| ---------------- | ------------------------------------- |
| `Clear(control)` | Clears the error message on a control |

### ImageList Extensions

| Method                           | Description                                          |
| -------------------------------- | ---------------------------------------------------- |
| `SaveToDirectory(directoryName)` | Saves all images in the list to a directory as files |
| `SaveToDirectory(directory)`     | Overload accepting `DirectoryInfo`                   |

### TableLayoutPanel Extensions

| Method                                                      | Description                                                                                   |
| ----------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `RemoveRow(row)`                                            | Removes a row and shifts remaining controls up                                                |
| `CopyLastRow(controlCallback?, allowSuspendingLayout?)`     | Duplicates the last row with all its controls (3 overloads for different callback signatures) |
| `GetColumnValue<TControl, TType>(row, columnIndex, reader)` | Reads a value from a control at a grid position                                               |
| `SetColumnValue<TControl>(row, columnIndex, writer)`        | Writes a value to a control at a grid position                                                |
| `GetControlFromPositionFixed<TControl>(column, row)`        | Gets a typed control at a position (fixes framework bugs with span)                           |
| `GetControlFromPositionFixed(column, row)`                  | Gets a control at a position (fixes framework bugs with span)                                 |
| `UseAsTable(cellControls)`                                  | Populates the panel with a 2D array of controls arranged as a table                           |

### Binding Extensions

| Method                               | Description                                                   |
| ------------------------------------ | ------------------------------------------------------------- |
| `AddTypeConverter<TType>(converter)` | Adds a format/parse type conversion handler to a data binding |

### BindingsCollection Extensions

| Method                                    | Description                                      |
| ----------------------------------------- | ------------------------------------------------ |
| `GetBindingByPropertyName(propertyName)`  | Gets the first binding for a given property name |
| `GetBindingsByPropertyName(propertyName)` | Gets all bindings for a given property name      |

### ToolStripItem Extensions

| Method                 | Description                                                            |
| ---------------------- | ---------------------------------------------------------------------- |
| `SetTextAndShow(text)` | Sets text and makes the item visible; hides if text is null/whitespace |
| `ClearTextAndHide()`   | Clears text and hides the item                                         |

### ToolStripComboBox Extensions

| Method                                                   | Description                                          |
| -------------------------------------------------------- | ---------------------------------------------------- |
| `SetSelectedItemAndSuppressEvent(selectedItem, handler)` | Sets selection without firing `SelectedIndexChanged` |

### ToolStripProgressBar Extensions

| Method                        | Description                                                   |
| ----------------------------- | ------------------------------------------------------------- |
| `SetPercent(percentage)`      | Sets value as a percentage (0-100)                            |
| `SetNormalizedValue(value)`   | Sets value as a normalized ratio (float and double overloads) |
| `SetValue(current, max)`      | Sets value relative to a maximum                              |
| `SetValue(current, min, max)` | Sets value relative to a custom range                         |

### ToolTip Extensions

| Method                                  | Description                                                   |
| --------------------------------------- | ------------------------------------------------------------- |
| `SetToolTips(baseControl, toolTipText)` | Sets the same tooltip on a control and all its child controls |

### TreeView Extensions

| Method                                                                     | Description                                                         |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| `EnabledDragAndDrop(folderSelector?, allowRootNodeDragging?, onNodeMove?)` | Enables full drag-and-drop with folder detection and move callbacks |

### TreeNode Extensions

| Method              | Description                                                           |
| ------------------- | --------------------------------------------------------------------- |
| `IsChildOf(parent)` | Checks if the node is a descendant of a given parent at any depth     |
| `GetId()`           | Gets a unique hierarchical ID string for the node                     |
| `GetImage()`        | Gets the image associated with the node from its TreeView's ImageList |

### TreeNodeCollection Extensions

| Method       | Description                                                        |
| ------------ | ------------------------------------------------------------------ |
| `AllNodes()` | Flattens the hierarchy into a depth-first enumeration of all nodes |

### Event Extensions (`System.Windows.Threading`)

Thread-safe event invocation that marshals calls to subscriber threads via `ISynchronizeInvoke`.

| Method                                  | Description                                               |
| --------------------------------------- | --------------------------------------------------------- |
| `SafeInvoke<T>(sender, eventArgs)`      | Invokes an `EventHandler<T>` on each subscriber's thread  |
| `SafeInvoke(arguments)`                 | Invokes a `MulticastDelegate` on each subscriber's thread |
| `AsyncSafeInvoke<T>(sender, eventArgs)` | Asynchronous version with retry logic                     |
| `AsyncSafeInvoke(arguments)`            | Asynchronous multicast delegate version with retry logic  |

```csharp
public event EventHandler<MyEventArgs> MyEvent;

public void RaiseEvent(string message) {
    MyEvent.SafeInvoke(this, new MyEventArgs { Message = message });
}
```

## Planned Features

_None currently planned._

## Known Bugs and Limitations

- **Windows-only**: Requires WinForms / GDI+; not cross-platform.
- `EnableExtendedAttributes()` uses `ConditionalWeakTable` for state management; requires .NET Framework 4.0+.
- `AdvancedChart` and `DiagramControl` are GDI+-based; may have performance limitations with very large datasets.
- `ToolStripSpringTextBox` width calculation depends on the owning `ToolStrip` being properly sized before layout.
- Some controls use owner-draw mode which may conflict with existing custom drawing code.
- `Duplicate()` for `Control` uses reflection and may not copy all properties on custom control subclasses.
