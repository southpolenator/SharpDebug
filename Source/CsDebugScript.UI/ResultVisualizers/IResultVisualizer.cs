using CsDebugScript.UI.CodeWindow;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Interface for abstracting result visualizers.
    /// </summary>
    internal interface IResultVisualizer
    {
        /// <summary>
        /// Gets the name of the variable / property.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value of the property that will be visualized.
        /// If it is not <see cref="UIElement"/>, it will be added as a string (<see cref="ValueString"/>).
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets the string that represents type of the variable / property.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the image that represents icon of the variable / property.
        /// </summary>
        ImageSource Image { get; }

        /// <summary>
        /// Data type that will be used to generate icon of the variable / property.
        /// </summary>
        CompletionDataType DataType { get; }

        /// <summary>
        /// Checks if this item has child elements and should be expandable.
        /// </summary>
        bool IsExpandable { get; }

        /// <summary>
        /// Gets the child elements in groups (Tuple of group name and elements).
        /// </summary>
        IEnumerable<Tuple<string, IEnumerable<IResultVisualizer>>> ChildrenGroups { get; }

        /// <summary>
        /// Get child elements and groups.
        /// </summary>
        IEnumerable<IResultVisualizer> Children { get; }

        /// <summary>
        /// Gets the string that describes value of the variable / property.
        /// </summary>
        string ValueString { get; }

        /// <summary>
        /// Initializes caches so that properties can be safely queried in the UI (STA) thread.
        /// </summary>
        void Initialize();
    }
}
