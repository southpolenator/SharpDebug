using CsDebugScript.UI.CodeWindow;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Class that visualizes <see cref="VariableCollection"/> (usually <see cref="StackFrame.Locals"/> or <see cref="StackFrame.Arguments"/>).
    /// </summary>
    internal class VariableCollectionResultVisualizer : CustomObjectResultVisualizer
    {
        /// <summary>
        /// Variable collection to be visualized.
        /// </summary>
        private VariableCollection variableCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollectionResultVisualizer"/> class.
        /// </summary>
        /// <param name="variableCollection">Variable collection to be visualized.</param>
        /// <param name="variableCollectionType">Type of the resulting object that should be visualized.</param>
        /// <param name="name">Name of the variable / property.</param>
        /// <param name="image">Image that represents icon of the variable / property</param>
        /// <param name="interactiveResultVisualizer">Interactive result visualizer that can be used for creating UI elements.</param>
        public VariableCollectionResultVisualizer(VariableCollection variableCollection, Type variableCollectionType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
            : base(variableCollection, variableCollectionType, name, image, interactiveResultVisualizer)
        {
            this.variableCollection = variableCollection;
        }

        /// <summary>
        /// Gets child elements that should be expanded when this object is visualized.
        /// This group will not be shown as a group like other properties like <see cref="ResultVisualizer.DynamicChildren"/>.
        /// </summary>
        public override IEnumerable<IResultVisualizer> ExpandedChildren
        {
            get
            {
                yield return Create(variableCollection.Count, null, "Count", CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                for (int i = 0; i < variableCollection.Count; i++)
                {
                    yield return Create(GetValue(() => variableCollection[i]), typeof(Variable), variableCollection[i].GetName(), CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                }
            }
        }

        /// <summary>
        /// Gets the value of the property that will be visualized.
        /// </summary>
        protected override object GetValue()
        {
            return $"{{ Variables: {variableCollection.Count} }}";
        }
    }
}
