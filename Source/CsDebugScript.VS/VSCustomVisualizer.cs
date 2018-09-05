using CsDebugScript.UI.CodeWindow;
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
            VSCustomVisualizerEvaluator evaluator = visualizedExpression.GetDataItem<VSCustomVisualizerEvaluator>();

            initialChildren = new DkmChildVisualizedExpression[0];
            enumContext = DkmEvaluationResultEnumContext.Create(evaluator.ResultVisualizer?.Children.Count() ?? 0, visualizedExpression.StackFrame, visualizedExpression.InspectionContext, evaluator);
        }

        /// <summary>
        /// Returns child elements of previous evaluation.
        /// </summary>
        public void GetItems(DkmVisualizedExpression visualizedExpression, DkmEvaluationResultEnumContext enumContext, int startIndex, int count, out DkmChildVisualizedExpression[] items)
        {
            VSCustomVisualizerEvaluator evaluator = visualizedExpression.GetDataItem<VSCustomVisualizerEvaluator>();
            int i = 0;

            items = evaluator.ResultVisualizer.Children.Skip(startIndex).Take(count).Select(item =>
            {
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

                DkmSuccessEvaluationResult result = DkmSuccessEvaluationResult.Create(
                    visualizedExpression.InspectionContext,
                    visualizedExpression.StackFrame,
                    item.Name, // Name - Left column
                    "[fullname]", // TODO: FullName - What is being copied when "Add to watch"
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

                var expression = DkmChildVisualizedExpression.Create(
                    visualizedExpression.InspectionContext,
                    visualizedExpression.VisualizerId,
                    visualizedExpression.SourceId,
                    visualizedExpression.StackFrame,
                    visualizedExpression.ValueHome,
                    result,
                    visualizedExpression,
                    (uint)(startIndex + (i++)),
                    null);
                expression.SetDataItem(DkmDataCreationDisposition.CreateAlways, new VSCustomVisualizerEvaluator(result, item));
                return expression;
            }).ToArray();
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
    }
}
