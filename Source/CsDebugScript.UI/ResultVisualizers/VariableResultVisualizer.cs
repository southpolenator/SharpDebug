using CsDebugScript.Engine;
using CsDebugScript.UI.CodeWindow;
using System;
using System.Collections.Generic;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Class that visualizes <see cref="Variable"/> from process that is being debugged.
    /// </summary>
    internal class VariableResultVisualizer : CustomObjectResultVisualizer
    {
        /// <summary>
        /// Variable to be visualized.
        /// </summary>
        private Variable variable;

        /// <summary>
        /// If <see cref="ISymbolProvider"/> supports accessing class inheritance, we should use it for visualization.
        /// </summary>
        private bool extractUsingClasses;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableResultVisualizer"/> class.
        /// </summary>
        /// <param name="variable">Variable to be visualized.</param>
        /// <param name="variableType">Type of the resulting object that should be visualized.</param>
        /// <param name="name">Name of the variable / property.</param>
        /// <param name="dataType">Data type that will be used to generate icon of the variable / property</param>
        /// <param name="interactiveResultVisualizer">Interactive result visualizer that can be used for creating UI elements.</param>
        public VariableResultVisualizer(Variable variable, Type variableType, string name, CompletionDataType dataType, InteractiveResultVisualizer interactiveResultVisualizer)
            : base(variable, variableType, name, dataType, interactiveResultVisualizer)
        {
            this.variable = variable;
            try
            {
                extractUsingClasses = variable.GetCodeType().ClassFieldNames != null;
            }
            catch
            {
                extractUsingClasses = false;
            }
        }

        /// <summary>
        /// Checks if this item has child elements and should be expandable.
        /// </summary>
        public override bool IsExpandable
        {
            get
            {
                CodeType codeType = variable.GetCodeType();

                return !codeType.IsSimple && !codeType.IsEnum && variable != null && !variable.IsNullPointer();
            }
        }

        /// <summary>
        /// Gets child elements that should be expanded when this object is visualized.
        /// This is specialized for <see cref="Variable"/>.
        /// </summary>
        public override IEnumerable<IResultVisualizer> ExpandedChildren
        {
            get
            {
                CodeType codeType = variable.GetCodeType();

                if (codeType.IsPointer && (codeType.ElementType.IsSimple || codeType.ElementType.IsPointer || codeType.ElementType.IsArray || codeType.ElementType.IsEnum))
                {
                    yield return Create(GetValue(() => variable.DereferencePointer()), typeof(Variable), "*", CompletionDataType.Variable, interactiveResultVisualizer);
                }
                else if (codeType.IsArray)
                {
                    yield return Create(variable.GetArrayLength(), typeof(int), "Length", CompletionDataType.Property, interactiveResultVisualizer);
                    if (variable.GetArrayLength() <= ArrayElementsVisualized)
                    {
                        foreach (IResultVisualizer element in GetArrayElements(0, variable.GetArrayLength()))
                        {
                            yield return element;
                        }
                    }
                }
                // TODO: Continue specializing on variable types
                else
                {
                    foreach (IResultVisualizer item in OrderItems(ExtractFields()))
                    {
                        yield return item;
                    }
                    yield return Create(codeType, codeType.GetType(), "CodeType", CompletionDataType.Property, interactiveResultVisualizer);
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
                bool elementsReturned = !variable.GetCodeType().IsArray || variable.GetArrayLength() <= ArrayElementsVisualized;

                foreach (Tuple<string, IEnumerable<IResultVisualizer>> children in base.ChildrenGroups)
                {
                    yield return children;
                    if (!elementsReturned && children.Item1 == ExpandedGroupName)
                    {
                        elementsReturned = true;
                        for (int j = 0; j < variable.GetArrayLength(); j += ArrayElementsVisualized)
                        {
                            int end = Math.Min(j + ArrayElementsVisualized, variable.GetArrayLength());

                            yield return Tuple.Create($"[{j}-{end - 1}]", GetArrayElements(j, end));
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
            CodeType codeType = variable.GetCodeType();

            if (codeType.IsEnum)
            {
                return $"{variable} ({variable.Data:X}) [0x{variable.Data:X}]";
            }
            else if (codeType.IsArray)
            {
                return $"{{ Length: {variable.GetArrayLength()} }}";
            }

            return variable;
        }

        /// <summary>
        /// Gets the string that describes value of the variable / property.
        /// </summary>
        protected override string GetTypeString()
        {
            return variable.GetCodeType().ToString();
        }

        /// <summary>
        /// Gets group of array elements to be visualized.
        /// </summary>
        /// <param name="start">Array index of the group start.</param>
        /// <param name="end">Array index of the group end (not included).</param>
        /// <returns>Group of array elements to be visualized.</returns>
        private IEnumerable<IResultVisualizer> GetArrayElements(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                yield return Create(GetValue(() => variable.GetArrayElement(i)), typeof(Variable), $"[{i}]", CompletionDataType.Variable, interactiveResultVisualizer);
            }
        }

        /// <summary>
        /// Extracts fields of the object that will be visualized in [Expanded] group.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IResultVisualizer> ExtractFields()
        {
            CodeType codeType = variable.GetCodeType();

            if (extractUsingClasses)
            {
                foreach (string baseClass in codeType.InheritedClasses.Keys)
                {
                    object baseClassValue = GetValue(() => variable.GetBaseClass(baseClass));
                    Variable baseClassVariable = baseClassValue as Variable;

                    if (baseClassVariable != null)
                    {
                        yield return new VariableResultVisualizer(baseClassVariable, typeof(Variable), $"[{baseClass}]", CompletionDataType.Class, interactiveResultVisualizer);
                    }
                    else
                    {
                        yield return Create(baseClassValue, typeof(Variable), $"[{baseClass}]", CompletionDataType.Class, interactiveResultVisualizer);
                    }
                }

                foreach (string fieldName in codeType.ClassFieldNames)
                {
                    yield return Create(GetValue(() => variable.GetClassField(fieldName)), typeof(Variable), fieldName, CompletionDataType.Variable, interactiveResultVisualizer);
                }
            }
            else
            {
                foreach (string fieldName in codeType.FieldNames)
                {
                    yield return Create(GetValue(() => variable.GetField(fieldName)), typeof(Variable), fieldName, CompletionDataType.Variable, interactiveResultVisualizer);
                }
            }
        }
    }
}
