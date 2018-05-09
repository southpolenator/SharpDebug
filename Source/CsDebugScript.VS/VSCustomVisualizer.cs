using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.ComponentInterfaces;
using Microsoft.VisualStudio.Debugger.Evaluation;
using System;

namespace CsDebugScript.VS
{
    public class VSCustomVisualizer : IDkmCustomVisualizer
    {
        public void EvaluateVisualizedExpression(DkmVisualizedExpression visualizedExpression, out DkmEvaluationResult resultObject)
        {
            VSCustomVisualizerEvaluator evaluator = new VSCustomVisualizerEvaluator(visualizedExpression);

            resultObject = evaluator.EvaluationResult;
            visualizedExpression.SetDataItem(DkmDataCreationDisposition.CreateAlways, evaluator);
        }

        public void GetChildren(DkmVisualizedExpression visualizedExpression, int initialRequestSize, DkmInspectionContext inspectionContext, out DkmChildVisualizedExpression[] initialChildren, out DkmEvaluationResultEnumContext enumContext)
        {
            VSCustomVisualizerEvaluator evaluator = visualizedExpression.GetDataItem<VSCustomVisualizerEvaluator>();

            // TODO: It would be great to fill in initial children with real data
            initialChildren = new DkmChildVisualizedExpression[0];
            enumContext = DkmEvaluationResultEnumContext.Create(1, visualizedExpression.StackFrame, visualizedExpression.InspectionContext, evaluator);
        }

        public void GetItems(DkmVisualizedExpression visualizedExpression, DkmEvaluationResultEnumContext enumContext, int startIndex, int count, out DkmChildVisualizedExpression[] items)
        {
            VSCustomVisualizerEvaluator evaluator = visualizedExpression.GetDataItem<VSCustomVisualizerEvaluator>();

            // TODO: Do children evaluation
            items = new DkmChildVisualizedExpression[count];
            for (int i = 0; i < count; i++)
            {
                DkmSuccessEvaluationResult result = DkmSuccessEvaluationResult.Create(
                        visualizedExpression.InspectionContext,
                        visualizedExpression.StackFrame,
                        "[name]", // Name - Left column
                        "[fullname]", // FullName - What is being copied when "Add to watch"
                        DkmEvaluationResultFlags.ReadOnly,
                        "[value]", // Value - Middle column
                        "",
                        "[faketype]", // Type - Right column
                        DkmEvaluationResultCategory.Property,
                        DkmEvaluationResultAccessType.None,
                        DkmEvaluationResultStorageType.None,
                        DkmEvaluationResultTypeModifierFlags.None,
                        null,
                        null,
                        null,
                        null);

                items[i] = DkmChildVisualizedExpression.Create(
                    visualizedExpression.InspectionContext,
                    visualizedExpression.VisualizerId,
                    visualizedExpression.SourceId,
                    visualizedExpression.StackFrame,
                    visualizedExpression.ValueHome,
                    result,
                    visualizedExpression,
                    (uint)(startIndex + i),
                    null);
            }
        }

        public string GetUnderlyingString(DkmVisualizedExpression visualizedExpression)
        {
            return "";
            //throw new NotImplementedException();
        }

        public void SetValueAsString(DkmVisualizedExpression visualizedExpression, string value, int timeout, out string errorText)
        {
            throw new NotImplementedException();
        }

        public void UseDefaultEvaluationBehavior(DkmVisualizedExpression visualizedExpression, out bool useDefaultEvaluationBehavior, out DkmEvaluationResult defaultEvaluationResult)
        {
            // We always want to highjack evaluation, for now :)
            useDefaultEvaluationBehavior = false;
            defaultEvaluationResult = null;
        }
    }
}
