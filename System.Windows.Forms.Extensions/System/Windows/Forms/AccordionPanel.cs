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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Event arguments for accordion section events.
/// </summary>
public class AccordionSectionEventArgs : EventArgs {
  /// <summary>
  /// Gets the section that raised the event.
  /// </summary>
  public AccordionSection Section { get; }

  /// <summary>
  /// Gets the index of the section.
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="AccordionSectionEventArgs"/> class.
  /// </summary>
  public AccordionSectionEventArgs(AccordionSection section, int index) {
    this.Section = section;
    this.Index = index;
  }
}

/// <summary>
/// Represents a section in an accordion panel.
/// </summary>
public class AccordionSection {
  private readonly ExpanderControl _expander;

  /// <summary>
  /// Gets or sets the header text.
  /// </summary>
  public string HeaderText {
    get => this._expander.HeaderText;
    set => this._expander.HeaderText = value;
  }

  /// <summary>
  /// Gets or sets the header icon.
  /// </summary>
  public Image HeaderIcon {
    get => this._expander.HeaderIcon;
    set => this._expander.HeaderIcon = value;
  }

  /// <summary>
  /// Gets or sets whether this section is expanded.
  /// </summary>
  public bool IsExpanded {
    get => this._expander.IsExpanded;
    set => this._expander.IsExpanded = value;
  }

  /// <summary>
  /// Gets the content panel.
  /// </summary>
  public Panel ContentPanel => this._expander.ContentPanel;

  internal ExpanderControl Expander => this._expander;

  internal AccordionSection(ExpanderControl expander) {
    this._expander = expander;
  }
}

/// <summary>
/// A panel with multiple collapsible sections.
/// </summary>
/// <example>
/// <code>
/// var accordion = new AccordionPanel {
///   AllowMultipleExpanded = false
/// };
/// var section1 = accordion.AddSection("Section 1");
/// section1.ContentPanel.Controls.Add(new Label { Text = "Content 1" });
/// var section2 = accordion.AddSection("Section 2");
/// section2.ContentPanel.Controls.Add(new Label { Text = "Content 2" });
/// </code>
/// </example>
public class AccordionPanel : ContainerControl {
  private readonly List<AccordionSection> _sections = new();
  private bool _allowMultipleExpanded;
  private bool _animateExpansion = true;
  private int _sectionHeight = 150;

  /// <summary>
  /// Occurs when a section is expanded.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a section is expanded.")]
  public event EventHandler<AccordionSectionEventArgs> SectionExpanded;

  /// <summary>
  /// Occurs when a section is collapsed.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a section is collapsed.")]
  public event EventHandler<AccordionSectionEventArgs> SectionCollapsed;

  /// <summary>
  /// Initializes a new instance of the <see cref="AccordionPanel"/> class.
  /// </summary>
  public AccordionPanel() {
    this.AutoScroll = true;
    this.Size = new Size(200, 300);
  }

  /// <summary>
  /// Gets the sections.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public AccordionSection[] Sections => this._sections.ToArray();

  /// <summary>
  /// Gets or sets whether multiple sections can be expanded simultaneously.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether multiple sections can be expanded simultaneously.")]
  [DefaultValue(false)]
  public bool AllowMultipleExpanded {
    get => this._allowMultipleExpanded;
    set => this._allowMultipleExpanded = value;
  }

  /// <summary>
  /// Gets or sets whether to animate expansion.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to animate the expansion/collapse.")]
  [DefaultValue(true)]
  public bool AnimateExpansion {
    get => this._animateExpansion;
    set {
      this._animateExpansion = value;
      foreach (var section in this._sections)
        section.Expander.AnimateExpansion = value;
    }
  }

  /// <summary>
  /// Gets or sets the default section height when expanded.
  /// </summary>
  [Category("Layout")]
  [Description("The default section height when expanded.")]
  [DefaultValue(150)]
  public int SectionHeight {
    get => this._sectionHeight;
    set {
      this._sectionHeight = Math.Max(50, value);
      foreach (var section in this._sections)
        section.Expander.ExpandedHeight = value;
    }
  }

  /// <summary>
  /// Adds a new section.
  /// </summary>
  public AccordionSection AddSection(string header) {
    var expander = new ExpanderControl {
      HeaderText = header,
      Dock = DockStyle.Top,
      ExpandedHeight = this._sectionHeight,
      AnimateExpansion = this._animateExpansion,
      IsExpanded = false
    };

    var section = new AccordionSection(expander);

    expander.Expanding += (s, e) => {
      if (this._allowMultipleExpanded)
        return;

      foreach (var other in this._sections)
        if (other.Expander != expander && other.IsExpanded)
          other.IsExpanded = false;
    };

    expander.Expanded += (s, e) => {
      var index = this._sections.IndexOf(section);
      this.OnSectionExpanded(new AccordionSectionEventArgs(section, index));
    };

    expander.Collapsed += (s, e) => {
      var index = this._sections.IndexOf(section);
      this.OnSectionCollapsed(new AccordionSectionEventArgs(section, index));
    };

    this._sections.Add(section);
    this.Controls.Add(expander);
    expander.BringToFront();

    return section;
  }

  /// <summary>
  /// Removes a section.
  /// </summary>
  public void RemoveSection(AccordionSection section) {
    if (!this._sections.Contains(section))
      return;

    this._sections.Remove(section);
    this.Controls.Remove(section.Expander);
    section.Expander.Dispose();
  }

  /// <summary>
  /// Removes all sections.
  /// </summary>
  public void ClearSections() {
    foreach (var section in this._sections.ToArray())
      this.RemoveSection(section);
  }

  /// <summary>
  /// Expands all sections.
  /// </summary>
  public void ExpandAll() {
    foreach (var section in this._sections)
      section.IsExpanded = true;
  }

  /// <summary>
  /// Collapses all sections.
  /// </summary>
  public void CollapseAll() {
    foreach (var section in this._sections)
      section.IsExpanded = false;
  }

  /// <summary>
  /// Raises the <see cref="SectionExpanded"/> event.
  /// </summary>
  protected virtual void OnSectionExpanded(AccordionSectionEventArgs e) => this.SectionExpanded?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="SectionCollapsed"/> event.
  /// </summary>
  protected virtual void OnSectionCollapsed(AccordionSectionEventArgs e) => this.SectionCollapsed?.Invoke(this, e);
}
