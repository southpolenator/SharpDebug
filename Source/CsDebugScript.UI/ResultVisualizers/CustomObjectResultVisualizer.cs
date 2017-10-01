using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Helper (base) class for visualizing custom resulting objects.
    /// Usually one would inherit <see cref="CustomObjectResultVisualizer"/> class
    /// and override <see cref="ResultVisualizer.ExpandedChildren"/> and <see cref="ResultVisualizer.GetValue"/>
    /// to implement custom object visualizers.
    /// </summary>
    internal class CustomObjectResultVisualizer : ObjectResultVisualizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomObjectResultVisualizer"/> class.
        /// </summary>
        /// <param name="result">Resulting object that should be visualized.</param>
        /// <param name="resultType">Type of the resulting object that should be visualized.</param>
        /// <param name="name">Name of the variable / property.</param>
        /// <param name="image">Image that represents icon of the variable / property</param>
        /// <param name="interactiveResultVisualizer">Interactive result visualizer that can be used for creating UI elements.</param>
        public CustomObjectResultVisualizer(object result, Type resultType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
            : base(result, resultType, name, image, interactiveResultVisualizer)
        {
        }

        /// <summary>
        /// Checks if this item has child elements and should be expandable.
        /// </summary>
        public override bool IsExpandable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the child elements in groups.
        /// Since inherited classes would override ExpandedChildren, we need to return them into [Public] group.
        /// </summary>
        public override IEnumerable<Tuple<string, IEnumerable<IResultVisualizer>>> Children
        {
            get
            {
                bool publicsReturned = false;

                foreach (Tuple<string, IEnumerable<IResultVisualizer>> children in base.Children)
                {
                    yield return children;
                    if (!publicsReturned && children.Item1 == "[Expanded]")
                    {
                        publicsReturned = true;
                        yield return Tuple.Create("[Public]", base.ExpandedChildren);
                    }
                }
            }
        }
    }
}
