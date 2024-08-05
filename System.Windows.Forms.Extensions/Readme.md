# Extensions to WindowsForms

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/System.Windows.Forms.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.System.Windows.Forms)](https://www.nuget.org/packages/FrameworkExtensions.System.Windows.Forms/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

This repository contains various C# extension methods and utilities for enhancing the functionality of existing .NET types, within the `System.Windows.Forms` namespace. These extensions aim to simplify common tasks, improve performance, and enhance the user experience of Windows Forms applications.

## Control Extensions

- **IsDesignMode**: Detects if a control is in design mode, useful for conditionally executing code only at runtime.
- **ISuspendedLayoutToken**: Facilitates the suspension and resumption of layout logic on controls, improving performance during batch updates.
- **ISuspendedRedrawToken**: Similar to `ISuspendedLayoutToken`, but suspends and resumes redraw operations to avoid flickering during updates.

## DataGridView Extensions

### Column Types

These extensions provide additional DataGridView column types, enhancing the functionality and interactivity of your grids:

- **BoundComboBox**: A ComboBox column that supports data binding within cells.
- **Button**: Adds a button to each cell in the column.
- **Checkbox**: A column of checkboxes.
- **DateTimePicker**: A DateTimePicker column for date selection.
- **DisableButton**: A button column with disable functionality.
- **ImageAndText**: A column that can display both images and text.
- **MultiImage**: Supports multiple images in a single cell.
- **NumericUpDown**: A column with NumericUpDown control for numeric input.
- **ProgressBar**: A column with a progress bar to visualize progress.

### Attributes

#### Record-Based (Applies to Full Row)

- **ConditionalRowHidden**: Conditionally hides rows based on specified criteria.
- **FullMergedRow**: Merges multiple cells into a single cell spanning multiple columns.
- **RowHeight**: Sets the height of the rows.
- **RowSelectable**: Specifies if the row can be selected.
- **RowStyle**: Applies a specific style to the entire row.

#### Property-Based (Applies to Cell)

- **ConditionalReadOnly**: Makes cells read-only based on specified conditions.
- **CellDisplayText**: Sets the display text of a cell.
- **CellStyle**: Applies a specific style to a cell.
- **CellToolTip**: Sets a tooltip for the cell.
- **Clickable**: Makes the cell clickable and defines click behavior.
- **ColumnSortMode**: Sets the sort mode for the column.
- **ColumnWidth**: Sets the width of the column.

#### Property-based (applies to column type)

- **ComboboxColumn**: Generates a combobox column for the property.
- **ImageColumn**: Generates an image column for the property.
- **MultiImageColumn**: Generates a multi-image column for the property.
- **NumericUpDownColumn**: Generates a numericupdown column for the property.
- **ProgressBarColumn**: Generates a progressbar column for the property.
- **ImageAndTextColumn**: Generates a imageandtext column for the property.

## RichTextBox Extensions

- **Syntax Highlighting**: Adds syntax highlighting capabilities to RichTextBox controls, making it easier to implement features for code editors or similar applications.
