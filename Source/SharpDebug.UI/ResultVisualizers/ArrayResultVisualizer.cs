using SharpDebug.UI.CodeWindow;
using System;
using System.Collections.Generic;

namespace SharpDebug.UI.ResultVisualizers
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
        /// <param name="dataType">Data type that will be used to generate icon of the variable / property</param>
        /// <param name="interactiveResultVisualizer">Interactive result visualizer that can be used for creating UI elements.</param>
        public ArrayResultVisualizer(Array array, Type arrayType, string name, CompletionDataType dataType, InteractiveResultVisualizer interactiveResultVisualizer)
            : base(array, arrayType, name, dataType, interactiveResultVisualizer)
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
                yield return Create(array.Length, null, "Length", CompletionDataType.Property, interactiveResultVisualizer);
                if (resultType == typeof(char[]))
                    yield return Create(ToString((char[])array), typeof(string), "Text", CompletionDataType.Property, interactiveResultVisualizer);
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
        public override IEnumerable<Tuple<string, IEnumerable<IResultVisualizer>>> ChildrenGroups
        {
            get
            {
                bool elementsReturned = array.Length <= ArrayElementsVisualized;

                foreach (Tuple<string, IEnumerable<IResultVisualizer>> children in base.ChildrenGroups)
                {
                    yield return children;
                    if (!elementsReturned && children.Item1 == ExpandedGroupName)
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
                yield return Create(GetValue(() => array.GetValue(i)), resultType.GetElementType(), $"[{i}]", CompletionDataType.Variable, interactiveResultVisualizer);
            }
        }

        /// <summary>
        /// Extracts C-style string from char array.
        /// </summary>
        /// <param name="array">The array of chars.</param>
        /// <returns>C-style string</returns>
        private static string ToString(char[] array)
        {
            int length = 0;

            while (length < array.Length && array[length] != 0)
                length++;
            return new string(array, 0, length);
        }
    }
}
