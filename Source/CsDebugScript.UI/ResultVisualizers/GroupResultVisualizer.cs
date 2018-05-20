using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using CsDebugScript.UI.CodeWindow;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Simple wrapper around child elements that can appear ([Dynamic] group for example).
    /// </summary>
    internal class GroupResultVisualizer : IResultVisualizer
    {
        /// <summary>
        /// Gets the name of the variable / property.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the property that will be visualized.
        /// If it is not <see cref="UIElement"/>, it will be added as a string (<see cref="ValueString"/>).
        /// </summary>
        public object Value => null;

        /// <summary>
        /// Gets the string that represents type of the variable / property.
        /// </summary>
        public string Type => null;

        /// <summary>
        /// Gets the image that represents icon of the variable / property.
        /// </summary>
        public ImageSource Image => throw new NotImplementedException();

        /// <summary>
        /// Checks if this item has child elements and should be expandable.
        /// </summary>
        public bool IsExpandable => true;

        /// <summary>
        /// Gets the child elements in groups (Tuple of group name and elements).
        /// </summary>
        public IEnumerable<Tuple<string, IEnumerable<IResultVisualizer>>> ChildrenGroups => throw new NotImplementedException();

        /// <summary>
        /// Get child elements and groups.
        /// </summary>
        public IEnumerable<IResultVisualizer> Children { get; private set; }

        /// <summary>
        /// Gets the string that describes value of the variable / property.
        /// </summary>
        public string ValueString => "";

        /// <summary>
        /// Data type that will be used to generate icon of the variable / property.
        /// </summary>
        public CompletionDataType DataType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupResultVisualizer"/> class.
        /// </summary>
        /// <param name="name">Name of the variable / property.</param>
        /// <param name="children">Child elements and groups.</param>
        /// <param name="dataType">Data type that will be used to generate icon of the variable / property</param>
        public GroupResultVisualizer(string name, IEnumerable<IResultVisualizer> children, CompletionDataType dataType = CompletionDataType.Namespace)
        {
            Name = name;
            Children = children;
            DataType = dataType;
        }

        /// <summary>
        /// Initializes caches so that properties can be safely queried in the UI (STA) thread.
        /// </summary>
        public void Initialize()
        {
        }
    }
}
