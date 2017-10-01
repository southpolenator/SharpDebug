using CsDebugScript.UI.CodeWindow;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Class that visualizes .NET arrays.
    /// </summary>
    internal class ArrayResultVisualizer : CustomObjectResultVisualizer
    {
        /// <summary>
        /// Array to be visualized.
        /// </summary>
        private Array array;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayResultVisualizer"/> class.
        /// </summary>
        /// <param name="array">Array to be visualized.</param>
        /// <param name="arrayType">Type of the resulting object that should be visualized.</param>
        /// <param name="name">Name of the variable / property.</param>
        /// <param name="image">Image that represents icon of the variable / property</param>
        /// <param name="interactiveResultVisualizer">Interactive result visualizer that can be used for creating UI elements.</param>
        public ArrayResultVisualizer(Array array, Type arrayType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
            : base(array, arrayType, name, image, interactiveResultVisualizer)
        {
            this.array = array;
        }

        /// <summary>
        /// Gets child elements that should be expanded when this object is visualized.
        /// This group will not be shown as a group like other properties like <see cref="ResultVisualizer.DynamicChildren"/>.
        /// </summary>
        public override IEnumerable<IResultVisualizer> ExpandedChildren
        {
            get
            {
                yield return Create(array.Length, null, "Length", CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                if (array.Length <= ArrayElementsVisualized)
                {
                    foreach (IResultVisualizer element in GetElements(0, array.Length))
                    {
                        yield return element;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the child elements in groups.
        /// Since we can have too many array elements, we would like to "page" them into groups.
        /// </summary>
        public override IEnumerable<Tuple<string, IEnumerable<IResultVisualizer>>> Children
        {
            get
            {
                bool elementsReturned = array.Length <= ArrayElementsVisualized;

                foreach (Tuple<string, IEnumerable<IResultVisualizer>> children in base.Children)
                {
                    yield return children;
                    if (!elementsReturned && children.Item1 == "[Expanded]")
                    {
                        elementsReturned = true;
                        for (int j = 0; j < array.Length; j += ArrayElementsVisualized)
                        {
                            int end = Math.Min(j + ArrayElementsVisualized, array.Length) - 1;

                            yield return Tuple.Create($"[{j}-{end}]", GetElements(j, end));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the value of the property that will be visualized.
        /// </summary>
        protected override object GetValue()
        {
            return $"{{ Length: {array.Length} }}";
        }

        /// <summary>
        /// Gets group of elements to be visualized.
        /// </summary>
        /// <param name="start">Array index of the group start.</param>
        /// <param name="end">Array index of the group end (not included).</param>
        /// <returns>Group of elements to be visualized.</returns>
        private IEnumerable<IResultVisualizer> GetElements(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                yield return Create(GetValue(() => array.GetValue(i)), resultType.GetElementType(), $"[{i}]", CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
            }
        }
    }
}
