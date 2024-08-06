# Extensions to WindowsForms

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/System.Windows.Forms.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.System.Windows.Forms)](https://www.nuget.org/packages/FrameworkExtensions.System.Windows.Forms/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

This repository contains various C# extension methods and utilities for enhancing the functionality of existing .NET types, within the `System.Windows.Forms` namespace. These extensions aim to simplify common tasks, improve performance, and enhance the user experience of Windows Forms applications.

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

- **UI-Threading**: Determine whether to use a controls' UI thread to execute code or not. (`Control.SafelyInvoke`/`Control.Async`)

  ```cs
  form.SafelyInvoke(() => {
      // UI thread safe code
  });
  ```

## DataGridView Extensions

### Info on used datatypes

- **methodname**: The name of an instance or static method, utilize `nameof()`-operator
- **propertyname**: The name of an instance or static property, utilize `nameof()`-operator
- **colorstring**:
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

- **BoundComboBox**: A ComboBox column that supports data binding within cells.
  - DataSourcePropertyName: object propertyname
  - EnabledWhenPropertyName: bool propertyname
  - ValueMember: string propertyname
  - DisplayMember: string propertyname
- **DateTimePicker**: A DateTimePicker column for date selection.
- **DisableButton**: A button column with disable functionality.
- **ImageAndText**: A column that can display both images and text.
  - Image: Image
  - ImageSize: Size
- **MultiImage**: Supports multiple images in a single cell.
  - ImageSizeInPixels: int
  - Padding: Padding
  - Margin: Padding
  - OnClickMethodName: void methodname(object, int)
  - ToolTipTextProviderMethodName: string methodname(object, int)
- **NumericUpDown**: A column with NumericUpDown control for numeric input.
  - DecimalPlaces: int
  - Increment: decimal
  - Minimum: decimal
  - Maximum: decimal
  - UseThousandsSeparator: bool
- **ProgressBar**: A column with a progress bar to visualize progress.
  - Minimum: double
  - Maximum: double

### Attributes

#### Record-Based (Applies to Full Row)

- **ConditionalRowHidden**: Conditionally hides rows based on specified criteria.
  - IsHiddenWhen: bool propertyname
- **FullMergedRow**: Merges multiple cells into a single cell spanning multiple columns.
  - HeadingTextPropertyName: string propertyname
  - ForeColor: colorstring
  - TextSize: float
- **RowHeight**: Sets the height of the rows.
  - HeightInPixel: int
  - RowHeightEnabledProperty: bool propertyname
  - RowHeightProperty: int propertyname
- **RowSelectable**: Specifies if the row can be selected.
  - ConditionProperty: bool propertyname
- **RowStyle**: Applies a specific style to the entire row.
  - ForeColor: colorstring
  - BackColor: colorstring
  - Format: string
  - ConditionalPropertyName: bool propertyname
  - ForeColorPropertyName: Color? propertyname
  - BackColorPropertyName: Color? propertyname
  - IsBold: bool
  - IsItalic: bool
  - IsStrikeout: bool
  - IsUnderline: bool

#### Property-Based (Applies to Cell)

- **ConditionalReadOnly**: Makes cells read-only based on specified conditions.
  - IsReadOnlyWhen: bool propertyname
- **SupportsConditionalImageAttribute**: Shows an image next to a cells' text based on specified conditions.
  - ImagePropertyName: Image Image
  - ConditionalPropertyName: bool propertyname
- **CellDisplayText**: Sets the display text of a cell.
  - PropertyName: string propertyname
- **CellStyle**: Applies a specific style to a cell.
  - ForeColor: colorstring
  - BackColor: colorstring
  - Format: string
  - Alignment: DataGridViewContentAlignment
  - WrapMode: DataGridViewTriState
  - ConditionalPropertyName: bool propertyname
  - ForeColorPropertyName: Color? propertyname
  - BackColorPropertyName: Color? propertyname
  - WrapModePropertyName: DataGridViewTriState propertyname
- **CellToolTip**: Sets a tooltip for the cell.
  - ToolTipText: string
  - ToolTipTextPropertyName: string propertyname
  - ConditionalPropertyName: bool propertyname
  - Format: string
- **Clickable**: Makes the cell clickable and defines click behavior.
  - OnClickMethodName: void methodname()
  - OnDoubleClickMethodName: void methodname()
- **ColumnSortMode**: Sets the sort mode for the column.
  - SortMode: DataGridViewColumnSortMode
- **ColumnWidth**: Sets the width of the column.
  - CharacterCount: char
  - Characters: string
  - WidthInPixelsInPixels: int
  - Mode: DataGridViewAutoSizeColumnMode

#### Property-based (applies to column type)

- **ButtonColumn**: Generates a button column for the property.
  - OnClickMethodName: void methodname()
  - IsEnabledWhenPropertyName: bool propertyname
- **CheckboxColumn**: Generates a checkbox column for the property.
- **ComboboxColumn**: Generates a combobox column for the property.
  - EnabledWhenPropertyName: bool propertyname
  - DataSourcePropertyName: object propertyname
  - ValueMember: string propertyname
  - DisplayMember: string propertyname
- **ImageColumn**: Generates an image column for the property.
  - ImageListPropertyName: ImageList propertyname
  - ToolTipTextPropertyName: string propertyname
  - OnClickMethodName: void methodname()
  - OnDoubleClickMethodName: void methodname()
- **MultiImageColumn**: Generates a multi-image column for the property.
  - OnClickMethodName: void methodname(object, int)
  - ToolTipProviderMethodName: string methodname(object, int)
  - MaximumImageSize: int
  - PaddingLeft: int
  - PaddingTop: int
  - PaddingRight: int
  - PaddingBottom: int
  - MarginLeft: int
  - MarginTop: int
  - MarginRight: int
  - MarginBottom: int
- **NumericUpDownColumn**: Generates a numericupdown column for the property.
  - Minimum: double
  - Maximum: double
  - Increment: double
  - DecimalPlaces: int
- **ProgressBarColumn**: Generates a progressbar column for the property.
  - Minimum: double
  - Maximum: double
- **ImageAndTextColumn**: Generates a imageandtext column for the property.
  - ImageListPropertyName: ImageList propertyname
  - ImageKeyPropertyName: int/object propertyname
  - ImagePropertyName: Image propertyname
  - TextImageRelation: TextImageRelation
  - FixedImageWidth: uint
  - FixedImageHeight: uint
  - KeepAspectRatio: bool

## RichTextBox Extensions

- **Syntax Highlighting**: Adds syntax highlighting capabilities to RichTextBox controls, making it easier to implement features for code editors or similar applications.

## TabControl Extensions

- **Tab Headers**: Adds coloring and images to tab page headers.
