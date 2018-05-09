using System;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Evaluation;

namespace CsDebugScript.VS
{
    class VSCustomVisualizerEvaluator : DkmDataItem
    {
        public VSCustomVisualizerEvaluator(DkmVisualizedExpression visualizedExpression)
        {
            VisualizedExpression = visualizedExpression;
            Evaluate();
        }

        public DkmVisualizedExpression VisualizedExpression { get; private set; }

        public DkmEvaluationResult EvaluationResult { get; private set; }

        private void Evaluate()
        {
            if (VisualizedExpression.TagValue == DkmVisualizedExpression.Tag.RootVisualizedExpression)
            {
                DkmRootVisualizedExpression rootVisualizedExpression = VisualizedExpression as DkmRootVisualizedExpression;
                int processId = rootVisualizedExpression.InspectionSession?.Process?.LivePart?.Id ?? 0;
                string moduleName = rootVisualizedExpression.Module?.Name; // TODO: This might need to be trimmed of file extension
                string typeString = rootVisualizedExpression.Type;
                ulong address = 0;
                bool hasAddress = false;

                if (VisualizedExpression.ValueHome.TagValue == DkmExpressionValueHome.Tag.PointerValueHome)
                {
                    address = (VisualizedExpression.ValueHome as DkmPointerValueHome).Address;
                    hasAddress = true;
                }

                if (typeString.Length == 0 || moduleName.Length == 0 || !hasAddress)
                {
                    string displayString = "{...CsDebugScript failure...}";

                    EvaluationResult = DkmSuccessEvaluationResult.Create(
                        VisualizedExpression.InspectionContext,
                        VisualizedExpression.StackFrame,
                        rootVisualizedExpression.Name,
                        rootVisualizedExpression.FullName,
                        DkmEvaluationResultFlags.ReadOnly,
                        displayString,
                        "",
                        rootVisualizedExpression.Type,
                        DkmEvaluationResultCategory.Other,
                        DkmEvaluationResultAccessType.None,
                        DkmEvaluationResultStorageType.None,
                        DkmEvaluationResultTypeModifierFlags.None,
                        null,
                        null,
                        null,
                        null);
                    return;
                }

                DkmDataAddress dkmDataAddress = DkmDataAddress.Create(VisualizedExpression.RuntimeInstance, address, rootVisualizedExpression.StackFrame?.InstructionAddress);

                EvaluationResult = DkmSuccessEvaluationResult.Create(
                    VisualizedExpression.InspectionContext,
                    VisualizedExpression.StackFrame,
                    rootVisualizedExpression.Name,
                    rootVisualizedExpression.FullName,
                    DkmEvaluationResultFlags.ReadOnly | DkmEvaluationResultFlags.Expandable,
                    "{...CsDebugScript...}",
                    "",
                    rootVisualizedExpression.Type,
                    DkmEvaluationResultCategory.Other,
                    DkmEvaluationResultAccessType.None,
                    DkmEvaluationResultStorageType.None,
                    DkmEvaluationResultTypeModifierFlags.None,
                    dkmDataAddress,
                    null,
                    null,
                    null);
                return;
            }

            throw new NotImplementedException();

            EvaluationResult = DkmSuccessEvaluationResult.Create(
                VisualizedExpression.InspectionContext,
                VisualizedExpression.StackFrame,
                "Name",
                "FullName",
                DkmEvaluationResultFlags.ReadOnly,
                "{...CsDebugScript child...}",
                "",
                "", // Type
                DkmEvaluationResultCategory.Other,
                DkmEvaluationResultAccessType.None,
                DkmEvaluationResultStorageType.None,
                DkmEvaluationResultTypeModifierFlags.None,
                null,
                null,
                null,
                null);
        }
    }
}
