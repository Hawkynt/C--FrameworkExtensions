
# Extensions to WindowsForms

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/System.Windows.Forms.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.System.Windows.Forms)](https://www.nuget.org/packages/FrameworkExtensions.System.Windows.Forms/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods and utilities for the `System.Windows.Forms` namespace, providing additional functionality for Windows Forms applications.

# Features

Additional features are available via extension methods. Use IntelliSense to explore available methods.

## Control Extensions

- **IsDesignMode**: Detects if a control is in design mode (even within the constructor), useful for conditionally executing code only at runtime. (`Control.IsDesignMode`)

  ```cs
  if (this.IsDesignMode()) {
    // Execute design-time logic
  }
  ```

- **ISuspendedLayoutToken**: Facilitates the suspension and resumption of layout logic on controls, improving performance during batch updates. (`Control.PauseLayout`)

  ```cs
  using (this.PauseLayout()) {
      // Perform layout updates
  }
  ```

- **ISuspendedRedrawToken**: Similar to `ISuspendedLayoutToken`, but suspends and resumes redraw operations to avoid flickering during updates. (`Control.PauseRedraw`)

  ```cs
  using (this.PauseRedraw()) {
      // Perform redraw updates
  }
  ```

- **Bindings**: Allows using Lambdas to add bindings for easier Model-View-ViewModel architecture (MVVM). This feature facilitates the binding of control properties to model properties using lambda expressions, ensuring a cleaner and more maintainable codebase. Ensure to use the equality (`==`) operator as assignments (`=`) are not supported in expression trees. (`Control.AddBinding`)

  ```csharp
  // Bind the model's LabelText property to the label's Text property.
  label.AddBinding(model, (c, m) => c.Text == m.LabelText);
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

| Parameter | Type | Target Signature | Description |
|-----------|------|------------------|-------------|
| DataSourcePropertyName | string | object propertyname | The name of the property that provides the data source for the ComboBox. This should be the name of a property in the data-bound object that returns a collection or list of items. |
| EnabledWhenPropertyName | string | bool propertyname | The name of the property that determines whether the ComboBox is enabled. This should be a boolean property. |
| ValueMember | string | string propertyname | The name of the property in the data source items that provides the value for the ComboBox. |
| DisplayMember | string | string propertyname | The name of the property in the data source items that provides the display text for the ComboBox. |

#### DateTimePicker

A DateTimePicker column for date selection.

#### DisableButton

A button column with disable functionality.

#### ImageAndText

A column that can display both images and text.

| Parameter | Type | Target Signature | Description |
|-----------|------|------------------|-------------|
| Image | Image | Image value | The (default) image to display in the cell. |
| ImageSize | Size | Size value | The size to which the image should be resized. |

#### MultiImage

Supports multiple images in a single cell.

| Parameter | Type | Target Signature | Description |
|-----------|------|------------------|-------------|
| ImageSizeInPixels | int | int value | The size in pixels to which all images should be scaled. |
| Padding | Padding | Padding value | The padding around each image. |
| Margin | Padding | Padding value | The margin between images. |
| OnClickMethodName | string | void methodname(object record, int imageIndex) | The method to execute when an image is clicked. |
| ToolTipTextProviderMethodName | string | string methodname(object record, int imageIndex) | The method that returns the tooltip text for an image. |

#### NumericUpDown

A column with NumericUpDown control for numeric input.

| Parameter | Type | Target Signature | Description |
|-----------|------|------------------|-------------|
| DecimalPlaces | int | int value | The number of decimal places to display. |
| Increment | decimal | decimal value | The amount to increment or decrement the value when the up or down buttons are clicked. |
| Minimum | decimal | decimal value | The minimum value allowed. |
| Maximum | decimal | decimal value | The maximum value allowed. |
| UseThousandsSeparator | bool | bool value | Whether to use a thousands separator. |

#### ProgressBar

A column with a progress bar to visualize progress.

| Parameter | Type | Target Signature | Description |
|-----------|------|------------------|-------------|
| Minimum | double | double value | The minimum value of the progress bar. |
| Maximum | double | double value | The maximum value of the progress bar. |

### Attributes

These can be used on the data records used as `DataSource` for the `DataGridView`.

#### Record-Based (Applies to Full Row)

##### DataGridViewConditionalRowHidden

Conditionally hides rows based on specified criteria.

| Parameter          | Type    | Target Signature | Description |
|--------------------|---------|------------------|-------------|
| IsHiddenWhen       | string  | bool propertyname | The name of the boolean property that determines whether the row should be hidden. |

##### DataGridViewFullMergedRow

Merges multiple cells into a single cell spanning multiple columns.

| Parameter               | Type    | Target Signature | Description |
|-------------------------|---------|------------------|-------------|
| HeadingTextPropertyName | string  | string propertyname | The name of the property that provides the heading text for the merged row. |
| ForeColor               | string  | colorstring      | The foreground color for the merged row, specified as a color string (e.g., "Red", "#FF0000"). |
| TextSize                | float   | float value      | The size of the text in the merged row. |

##### DataGridViewRowHeight

Sets the height of the rows.

| Parameter               | Type    | Target Signature | Description |
|-------------------------|---------|------------------|-------------|
| HeightInPixel           | int     | int value        | The height of the row in pixels. |
| RowHeightEnabledProperty| string  | bool propertyname | The name of the boolean property that determines whether custom row height is enabled. |
| RowHeightProperty       | string  | int propertyname | The name of the property that provides the custom row height. |

##### DataGridViewRowSelectable

Specifies if the row can be selected.

| Parameter               | Type    | Target Signature | Description |
|-------------------------|---------|------------------|-------------|
| ConditionProperty       | string  | bool propertyname | The name of the boolean property that determines whether the row can be selected. |

##### DataGridViewRowStyle

Applies a specific style to the entire row.

| Parameter               | Type    | Target Signature | Description |
|-------------------------|---------|------------------|-------------|
| ForeColor               | string  | colorstring      | The foreground color of the row, specified as a color string (e.g., "Blue", "#0000FF"). |
| BackColor               | string  | colorstring      | The background color of the row, specified as a color string (e.g., "LightGray", "#D3D3D3"). |
| Format                  | string  | string value     | The format string applied to the row's content (e.g., "C2" for currency). |
| ConditionalPropertyName | string  | bool propertyname | The name of the boolean property that determines whether the style should be applied. |
| ForeColorPropertyName   | string  | Color? propertyname | The name of the property that provides the foreground color for the row. |
| BackColorPropertyName   | string  | Color? propertyname | The name of the property that provides the background color for the row. |
| IsBold                  | bool    | bool value       | Whether the row's text should be bold. |
| IsItalic                | bool    | bool value       | Whether the row's text should be italic. |
| IsStrikeout             | bool    | bool value       | Whether the row's text should be struck out. |
| IsUnderline             | bool    | bool value       | Whether the row's text should be underlined. |

#### Property-Based (Applies to Cell)

##### DataGridViewConditionalReadOnly

Makes cells read-only based on specified conditions.

| Parameter         | Type    | Target Signature | Description |
|-------------------|---------|------------------|-------------|
| IsReadOnlyWhen    | string  | bool propertyname | The name of the boolean property that determines whether the cell is read-only. |

##### DataGridViewSupportsConditionalImage

Shows an image next to a cell's text based on specified conditions.

| Parameter               | Type    | Target Signature | Description |
|-------------------------|---------|------------------|-------------|
| ImagePropertyName       | string  | Image propertyname | The name of the property that provides the image for the cell. |
| ConditionalPropertyName | string  | bool propertyname  | The name of the boolean property that determines whether the image should be displayed. |

##### DataGridViewCellDisplayText

Sets the display text of a cell.

| Parameter   | Type    | Target Signature    | Description |
|-------------|---------|---------------------|-------------|
| PropertyName| string  | string propertyname | The name of the property that provides the display text for the cell. |

##### DataGridViewCellStyle

Applies a specific style to a cell.

| Parameter               | Type                       | Target Signature           | Description |
|-------------------------|----------------------------|----------------------------|-------------|
| ForeColor               | string                     | colorstring                | The foreground color of the cell, specified as a color string (e.g., "Black", "#000000"). |
| BackColor               | string                     | colorstring                | The background color of the cell, specified as a color string (e.g., "White", "#FFFFFF"). |
| Format                  | string                     | string value               | The format string applied to the cell's content (e.g., "C2" for currency). |
| Alignment               | DataGridViewContentAlignment | DataGridViewContentAlignment value | The alignment of the cell's content. |
| WrapMode                | DataGridViewTriState       | DataGridViewTriState value | Whether the cell's text should wrap. |
| ConditionalPropertyName | string                     | bool propertyname          | The name of the boolean property that determines whether the style should be applied. |
| ForeColorPropertyName   | string                     | Color? propertyname        | The name of the property that provides the foreground color for the cell. |
| BackColorPropertyName   | string                     | Color? propertyname        | The name of the property that provides the background color for the cell. |
| WrapModePropertyName    | string                     | DataGridViewTriState propertyname | The name of the property that provides the wrap mode for the cell. |

##### DataGridViewCellTooltip

Sets a tooltip for the cell.

| Parameter               | Type    | Target Signature    | Description |
|-------------------------|---------|---------------------|-------------|
| ToolTipText             | string  | string value        | The tooltip text for the cell. |
| ToolTipTextPropertyName | string  | string propertyname | The name of the property that provides the tooltip text. |
| ConditionalPropertyName | string  | bool propertyname   | The name of the boolean property that determines whether the tooltip should be displayed. |
| Format                  | string  | string value        | The format string applied to the tooltip text. |

##### DataGridViewClickable

Makes the cell clickable and defines click behavior.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| OnClickMethodName       | string  | void methodname()    | The name of the method to be called when the cell is clicked. |
| OnDoubleClickMethodName | string  | void methodname()    | The name of the method to be called when the cell is double-clicked. |

##### DataGridViewColumnSortMode

Sets the sort mode for the column.

| Parameter | Type                        | Target Signature              | Description |
|-----------|-----------------------------|--------------------------------|-------------|
| SortMode  | DataGridViewColumnSortMode  | DataGridViewColumnSortMode value | The sort mode for the column. |

##### DataGridViewColumnWidth

Sets the width of the column.

| Parameter       | Type                           | Target Signature              | Description |
|-----------------|--------------------------------|--------------------------------|-------------|
| CharacterCount  | char                            | char value                     | The number of characters to determine the column width. |
| Characters      | string                         | string value                  | The string used to determine the column width. |
| WidthInPixels   | int                            | int value                     | The width of the column in pixels. |
| Mode            | DataGridViewAutoSizeColumnMode | DataGridViewAutoSizeColumnMode value | The auto-size mode for the column. |

#### Property-Based (Applies to Column Type)

##### DataGridViewButtonColumn

Generates a button column for the property.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| OnClickMethodName       | string  | void methodname()    | The name of the method to be called when the button is clicked. |
| IsEnabledWhenPropertyName | string | bool propertyname    | The name of the boolean property that determines whether the button is enabled. |

##### DataGridViewCheckboxColumn

Generates a checkbox column for the property.

No additional parameters required.

##### DataGridViewComboboxColumn

Generates a combobox column for the property.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| EnabledWhenPropertyName | string  | bool propertyname    | The name of the boolean property that determines whether the combobox is enabled. |
| DataSourcePropertyName  | string  | object propertyname  | The name of the property that provides the data source for the combobox. |
| ValueMember             | string  | string propertyname  | The name of the property in the data source items that provides the value for the combobox. |
| DisplayMember           | string  | string propertyname  | The name of the property in the data source items that provides the display text for the combobox. |

##### DataGridViewImageColumn

Generates an image column for the property.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| ImageListPropertyName   | string  | ImageList propertyname | The name of the property that provides the image list for the column. |
| ToolTipTextPropertyName | string  | string propertyname  | The name of the property that provides the tooltip text for the images. |
| OnClickMethodName       | string  | void methodname()    | The name of the method to be called when the image is clicked. |
| OnDoubleClickMethodName | string  | void methodname()    | The name of the method to be called when the image is double-clicked. |

##### DataGridViewMultiImageColumn

Generates a multi-image column for the property.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| OnClickMethodName       | string  | void methodname(object, int) | The name of the method to be called when an image is clicked. |
| ToolTipProviderMethodName | string | string methodname(object, int) | The name of the method that provides the tooltip text for an image. |
| MaximumImageSize        | int     | int value            | The maximum size of each image. |
| PaddingLeft             | int     | int value            | The left padding around the images. |
| PaddingTop              | int     | int value            | The top padding around the images. |
| PaddingRight            | int     | int value            | The right padding around the images. |
| PaddingBottom           | int     | int value            | The bottom padding around the images. |
| MarginLeft              | int     | int value            | The left margin between the images. |
| MarginTop               | int     | int value            | The top margin between the images. |
| MarginRight             | int     | int value            | The right margin between the images. |
| MarginBottom            | int     | int value            | The bottom margin between the images. |

##### DataGridViewNumericUpDownColumn

Generates a numericupdown column for the property.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| Minimum                 | double  | double value         | The minimum value for the numeric up-down control. |
| Maximum                 | double  | double value         | The maximum value for the numeric up-down control. |
| Increment               | double  | double value         | The amount to increment or decrement the value when the up or down buttons are clicked. |
| DecimalPlaces           | int     | int value            | The number of decimal places to display. |

##### DataGridViewProgressBarColumn

Generates a progressbar column for the property.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| Minimum                 | double  | double value         | The minimum value of the progress bar. |
| Maximum                 | double  | double value         | The maximum value of the progress bar. |

##### DataGridViewImageAndTextColumn

Generates an image and text column for the property.

| Parameter               | Type    | Target Signature     | Description |
|-------------------------|---------|----------------------|-------------|
| ImageListPropertyName   | string  | ImageList propertyname | The name of the property that provides the image list for the column. |
| ImageKeyPropertyName    | string  | int/object propertyname | The name of the property that provides the key for the image in the image list. |
| ImagePropertyName       | string  | Image propertyname   | The name of the property that provides the image for the cell. |
| TextImageRelation       | TextImageRelation | TextImageRelation value | The relationship between the image and the text. |
| FixedImageWidth         | uint    | uint value           | The fixed width of the image. |
| FixedImageHeight        | uint    | uint value           | The fixed height of the image. |
| KeepAspectRatio         | bool    | bool value           | Whether to keep the aspect ratio of the image. |

## RichTextBox Extensions

- **Syntax Highlighting**: Adds syntax highlighting capabilities to RichTextBox controls, making it easier to implement features for code editors or similar applications.

## TabControl Extensions

- **Tab Headers**: Adds coloring and images to tab page headers.
