using CsDebugScript.UI.CodeWindow;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Class that visualizes .NET dictionaries.
    /// </summary>
    internal class DictionaryResultVisualizer : CustomObjectResultVisualizer
    {
        /// <summary>
        /// Dictionary to be visualized.
        /// </summary>
        private IDictionary dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryResultVisualizer"/> class.
        /// </summary>
        /// <param name="dictionary">Dictionary to be visualized.</param>
        /// <param name="dictionaryType">Type of the resulting object that should be visualized.</param>
        /// <param name="name">Name of the variable / property.</param>
        /// <param name="dataType">Data type that will be used to generate icon of the variable / property</param>
        /// <param name="interactiveResultVisualizer">Interactive result visualizer that can be used for creating UI elements.</param>
        public DictionaryResultVisualizer(IDictionary dictionary, Type dictionaryType, string name, CompletionDataType dataType, InteractiveResultVisualizer interactiveResultVisualizer)
            : base(dictionary, dictionaryType, name, dataType, interactiveResultVisualizer)
        {
            this.dictionary = dictionary;
        }

        /// <summary>
        /// Gets child elements that should be expanded when this object is visualized.
        /// This group will not be shown as a group like other properties like <see cref="ResultVisualizer.DynamicChildren"/>.
        /// </summary>
        public override IEnumerable<IResultVisualizer> ExpandedChildren
        {
            get
            {
                yield return Create(dictionary.Count, null, "Count", CompletionDataType.Property, interactiveResultVisualizer);
                foreach (IResultVisualizer item in OrderItems(ExtractItems()))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Gets the value of the property that will be visualized.
        /// </summary>
        protected override object GetValue()
        {
            return $"{{ Elements: {dictionary.Count} }}";
        }

        /// <summary>
        /// Extracts entries that should be visualized.
        /// </summary>
        /// <returns>Entries that should be visualized as a group.</returns>
        private IEnumerable<IResultVisualizer> ExtractItems()
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return Create(GetValue(() => entry.Value), resultType.GetElementType(), entry.Key.ToString(), CompletionDataType.Variable, interactiveResultVisualizer);
            }
        }
    }
}
