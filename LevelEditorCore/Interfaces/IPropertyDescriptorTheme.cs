//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace LevelEditorCore
{
    /// <summary>
    /// Theme interface used for drawing properties.
    /// This interface can be implemented property descriptors</summary>
    public interface IPropertyDescriptorTheme
    {
        /// <summary>
        /// Gets the property background brush.
        /// return null to use default value.</summary>
        Brush PropertyBackgroundBrush { get; }

        /// <summary>
        /// Gets the selected property background brush.
        /// return null to use default value.</summary>
        Brush PropertyBackgroundHighlightBrush { get; }

        /// <summary>
        /// Gets the property text brush.
        /// return null to use default value.</summary>
        Brush PropertyTextBrush { get; }

        /// <summary>
        /// Gets the property read only text brush.
        /// return null to use default value.</summary>
        Brush PropertyReadOnlyTextBrush { get; }

        /// <summary>
        /// Gets the selected property text brush.
        /// return null to use default value.</summary>
        Brush PropertyTextHighlightBrush { get; }

        /// <summary>
        /// Gets the property expander pen.
        /// return null to use default value.</summary>
        Pen PropertyExpanderPen {get;}

        /// <summary>
        /// Gets the property line pen.
        /// return null to use default value.</summary>
        Pen PropertyLinePen { get; }
    }
}
