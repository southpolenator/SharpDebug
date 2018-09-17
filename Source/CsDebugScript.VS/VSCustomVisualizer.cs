using CsDebugScript.UI.CodeWindow;
using CsDebugScript.UI.ResultVisualizers;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.ComponentInterfaces;
using Microsoft.VisualStudio.Debugger.Evaluation;
using System;
using System.Linq;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Class that communicates with VS debugger and visualizes expressions (watch/locals window).
    /// </summary>
    public class VSCustomVisualizer : IDkmCustomVisualizer
    {
        /// <summary>
        /// Evaluates the specified visualized expression using our custom evaluator.
        /// </summary>
        public void EvaluateVisualizedExpression(DkmVisualizedExpression visualizedExpression, out DkmEvaluationResult resultObject)
        {
            VSCustomVisualizerEvaluator evaluator = new VSCustomVisualizerEvaluator(visualizedExpression);

            resultObject = evaluator.EvaluationResult;
            visualizedExpression.SetDataItem(DkmDataCreationDisposition.CreateAlways, evaluator);
        }

        /// <summary>
        /// Returns number of child elements in previous evaluation.
        /// </summary>
        public void GetChildren(DkmVisualizedExpression visualizedExpression, int initialRequestSize, DkmInspectionContext inspectionContext, out DkmChildVisualizedExpression[] initialChildren, out DkmEvaluationResultEnumContext enumContext)
        {
            // Check if we want to use passthrough visualizer
            PassThroughVisualizer passThroughVisualizer = visualizedExpression.GetDataItem<PassThroughVisualizer>();

            if (passThroughVisualizer != null)
            {
                passThroughVisualizer.GetChildren(visualizedExpression, initialRequestSize, inspectionContext, out initialChildren, out enumContext);
                return;
            }

            // Execute our regular visualizer
            VSCustomVisualizerEvaluator evaluator = visualizedExpression.GetDataItem<VSCustomVisualizerEvaluator>();

            initialChildren = new DkmChildVisualizedExpression[0];
            enumContext = DkmEvaluationResultEnumContext.Create(evaluator.ResultVisualizer?.Children.Count() ?? 0, visualizedExpression.StackFrame, visualizedExpression.InspectionContext, evaluator);
        }

        /// <summary>
        /// Returns child elements of previous evaluation.
        /// </summary>
        public void GetItems(DkmVisualizedExpression visualizedExpression, DkmEvaluationResultEnumContext enumContext, int startIndex, int count, out DkmChildVisualizedExpression[] items)
        {
            // Check if we want to use passthrough visualizer
            PassThroughVisualizer passThroughVisualizer = enumContext.GetDataItem<PassThroughVisualizer>();

            if (passThroughVisualizer != null)
            {
                passThroughVisualizer.GetItems(visualizedExpression, enumContext, startIndex, count, out items);
                return;
            }

            // Execute our regular visualizer
            VSCustomVisualizerEvaluator evaluator = visualizedExpression.GetDataItem<VSCustomVisualizerEvaluator>();
            IResultVisualizer[] itemsAsResults = evaluator.ResultVisualizer.Children.Skip(startIndex).Take(count).ToArray();

            items = new DkmChildVisualizedExpression[itemsAsResults.Length];
            for (int i = 0; i < items.Length; i++)
            {
                IResultVisualizer item = itemsAsResults[i];
                DkmEvaluationResultCategory category;

                switch (item.DataType)
                {
                    case CompletionDataType.Class:
                        category = DkmEvaluationResultCategory.Class;
                        break;
                    case CompletionDataType.Property:
                    case CompletionDataType.StaticProperty:
                        category = DkmEvaluationResultCategory.Property;
                        break;
                    case CompletionDataType.Event:
                        category = DkmEvaluationResultCategory.Event;
                        break;
                    case CompletionDataType.Method:
                        category = DkmEvaluationResultCategory.Method;
                        break;
                    case CompletionDataType.Enum:
                    case CompletionDataType.EnumValue:
                    case CompletionDataType.Keyword:
                    case CompletionDataType.Namespace:
                    case CompletionDataType.StaticClass:
                    case CompletionDataType.StaticEvent:
                    case CompletionDataType.StaticMethod:
                    case CompletionDataType.StaticVariable:
                    case CompletionDataType.Unknown:
                    case CompletionDataType.Variable:
                    default:
                        category = DkmEvaluationResultCategory.Data;
                        break;
                }

                DkmExpressionValueHome valueHome = visualizedExpression.ValueHome;
                ulong address = 0;
                string fullName = string.Empty;
                string typeName = null;

                try
                {
                    if (item.Value is Variable variable)
                    {
                        address = variable.GetPointerAddress();
                        typeName = variable.GetCodeType().Name;
                        fullName = $"*(({typeName}*)0x{address:X})";
                        valueHome = DkmPointerValueHome.Create(address);
                    }
                }
                catch
                {
                }

                DkmEvaluationResult result;
                DkmDataItem dataItem = null;

                if (item.ShouldForceDefaultVisualizer && !string.IsNullOrEmpty(fullName))
                {
                    using (DkmLanguageExpression languageExpression = DkmLanguageExpression.Create(visualizedExpression.InspectionContext.Language, DkmEvaluationFlags.TreatAsExpression, fullName, null))
                    {
                        visualizedExpression.EvaluateExpressionCallback(visualizedExpression.InspectionContext, languageExpression, visualizedExpression.StackFrame, out result);
                    }

                    if (result is DkmSuccessEvaluationResult successResult)
                    {
                        dataItem = new PassThroughVisualizer(successResult);
                        result = DkmSuccessEvaluationResult.Create(
                            successResult.InspectionContext,
                            successResult.StackFrame,
                            item.Name, // Name - Left column
                            successResult.FullName,
                            successResult.Flags,
                            successResult.Value, // Value - Middle column
                            successResult.EditableValue,
                            successResult.Type, // Type - Right column
                            category,
                            successResult.Access,
                            successResult.StorageType,
                            successResult.TypeModifierFlags,
                            successResult.Address,
                            successResult.CustomUIVisualizers,
                            successResult.ExternalModules,
                            successResult.RefreshButtonText,
                            dataItem);
                    }
                }
                else
                {
                    result = DkmSuccessEvaluationResult.Create(
                        visualizedExpression.InspectionContext,
                        visualizedExpression.StackFrame,
                        item.Name, // Name - Left column
                        fullName, // FullName - What is being copied when "Add to watch"
                        DkmEvaluationResultFlags.ReadOnly | (item.IsExpandable ? DkmEvaluationResultFlags.Expandable : DkmEvaluationResultFlags.None),
                        item.ValueString, // Value - Middle column
                        "",
                        item.Type ?? "", // Type - Right column
                        category,
                        DkmEvaluationResultAccessType.None,
                        DkmEvaluationResultStorageType.None,
                        DkmEvaluationResultTypeModifierFlags.None,
                        null,
                        VSUIVisualizerService.GetUIVisualizers(item),
                        null,
                        null);
                    dataItem = new VSCustomVisualizerEvaluator(result, item);
                }
                items[i] = DkmChildVisualizedExpression.Create(
                    visualizedExpression.InspectionContext,
                    visualizedExpression.VisualizerId,
                    visualizedExpression.SourceId,
                    visualizedExpression.StackFrame,
                    valueHome,
                    result,
                    visualizedExpression,
                    (uint)(startIndex + i),
                    dataItem);
            }
        }

        public string GetUnderlyingString(DkmVisualizedExpression visualizedExpression)
        {
            //throw new NotImplementedException();
            return "";
        }

        /// <summary>
        /// Updating values is not supported.
        /// </summary>
        public void SetValueAsString(DkmVisualizedExpression visualizedExpression, string value, int timeout, out string errorText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// We always want to use our evaluation behavior.
        /// </summary>
        public void UseDefaultEvaluationBehavior(DkmVisualizedExpression visualizedExpression, out bool useDefaultEvaluationBehavior, out DkmEvaluationResult defaultEvaluationResult)
        {
            // We always want to highjack evaluation, for now :)
            useDefaultEvaluationBehavior = false;
            defaultEvaluationResult = null;
        }

        /// <summary>
        /// Helper class to execute passthrough visualization of data (proviced option to fall back to NatVis visualizations).
        /// </summary>
        private class PassThroughVisualizer : DkmDataItem
        {
            public PassThroughVisualizer(DkmSuccessEvaluationResult successResult)
            {
                EvaluationResult = successResult;
            }

            public DkmSuccessEvaluationResult EvaluationResult { get; private set; }

            public void GetChildren(DkmVisualizedExpression visualizedExpression, int initialRequestSize, DkmInspectionContext inspectionContext, out DkmChildVisualizedExpression[] initialChildren, out DkmEvaluationResultEnumContext enumContext)
            {
                DkmEvaluationResult[] initialChildrenAsResult;

                visualizedExpression.GetChildrenCallback(EvaluationResult, 0, inspectionContext, out initialChildrenAsResult, out enumContext);
                initialChildren = Convert(visualizedExpression, initialChildrenAsResult);
                enumContext.SetDataItem(DkmDataCreationDisposition.CreateAlways, this);
            }

            public void GetItems(DkmVisualizedExpression visualizedExpression, DkmEvaluationResultEnumContext enumContext, int startIndex, int count, out DkmChildVisualizedExpression[] items)
            {
                DkmEvaluationResult[] itemsAsResult;

                visualizedExpression.GetItemsCallback(enumContext, startIndex, count, out itemsAsResult);
                items = Convert(visualizedExpression, itemsAsResult, startIndex);
            }

            private DkmChildVisualizedExpression[] Convert(DkmVisualizedExpression visualizedExpression, DkmEvaluationResult[] itemsAsResult, int startIndex = 0)
            {
                DkmChildVisualizedExpression[] items = new DkmChildVisualizedExpression[itemsAsResult.Length];

                for (int i = 0; i < items.Length; i++)
                {
                    DkmEvaluationResult result = itemsAsResult[i];
                    PassThroughVisualizer defaultEvaluator = null;

                    if (result is DkmSuccessEvaluationResult successResult)
                    {
                        defaultEvaluator = new PassThroughVisualizer(successResult);
                        result = DkmSuccessEvaluationResult.Create(
                            successResult.InspectionContext,
                            successResult.StackFrame,
                            successResult.Name, // Name - Left column
                            successResult.FullName,
                            successResult.Flags,
                            successResult.Value, // Value - Middle column
                            successResult.EditableValue,
                            successResult.Type, // Type - Right column
                            successResult.Category,
                            successResult.Access,
                            successResult.StorageType,
                            successResult.TypeModifierFlags,
                            successResult.Address,
                            successResult.CustomUIVisualizers,
                            successResult.ExternalModules,
                            successResult.RefreshButtonText,
                            defaultEvaluator);
                    }

                    items[i] = DkmChildVisualizedExpression.Create(
                        visualizedExpression.InspectionContext,
                        visualizedExpression.VisualizerId,
                        visualizedExpression.SourceId,
                        visualizedExpression.StackFrame,
                        null,
                        result,
                        visualizedExpression,
                        (uint)(startIndex + i),
                        defaultEvaluator);
                }
                return items;
            }
        }
    }
}
