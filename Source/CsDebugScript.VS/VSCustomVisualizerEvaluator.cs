using System;
using System.Linq;
using CsDebugScript.UI;
using CsDebugScript.UI.CodeWindow;
using CsDebugScript.UI.ResultVisualizers;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Evaluation;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Helper class that represents evaluator for VS custom visualizer.
    /// </summary>
    internal class VSCustomVisualizerEvaluator : DkmDataItem
    {
        /// <summary>
        /// Dummy interactive result visualizer used to create result visualizers
        /// </summary>
        private static InteractiveResultVisualizer dummyInteractiveResultVisualizer = new InteractiveResultVisualizer(null);

        /// <summary>
        /// Initializes the <see cref="VSCustomVisualizerEvaluator"/> class.
        /// </summary>
        static VSCustomVisualizerEvaluator()
        {
            VSContext.InitializeIfNeeded();
        }

        /// <summary>
        /// Initializes the <see cref="VSCustomVisualizerEvaluator"/> class.
        /// </summary>
        /// <param name="visualizedExpression"></param>
        public VSCustomVisualizerEvaluator(DkmVisualizedExpression visualizedExpression)
        {
            VisualizedExpression = visualizedExpression;
            Evaluate();
        }

        /// <summary>
        /// Initializes the <see cref="VSCustomVisualizerEvaluator"/> class.
        /// </summary>
        /// <param name="evaluationResult"></param>
        /// <param name="resultVisualizer"></param>
        public VSCustomVisualizerEvaluator(DkmEvaluationResult evaluationResult, IResultVisualizer resultVisualizer)
        {
            EvaluationResult = evaluationResult;
            ResultVisualizer = resultVisualizer;
        }

        /// <summary>
        /// Visualized expression that came from VS debugger.
        /// </summary>
        public DkmVisualizedExpression VisualizedExpression { get; private set; }

        /// <summary>
        /// Evaluation result that returns back to VS debugger.
        /// </summary>
        public DkmEvaluationResult EvaluationResult { get; private set; }

        /// <summary>
        /// Variable initialized from the visualized expression.
        /// </summary>
        public Variable Variable { get; private set; }

        /// <summary>
        /// Result visualizer that represents evaluated expression.
        /// </summary>
        public IResultVisualizer ResultVisualizer { get; private set; }

        /// <summary>
        /// Evaluates visual expression and converts it to result visualizer.
        /// </summary>
        private void Evaluate()
        {
            if (VisualizedExpression.TagValue == DkmVisualizedExpression.Tag.RootVisualizedExpression)
            {
                DkmRootVisualizedExpression rootVisualizedExpression = VisualizedExpression as DkmRootVisualizedExpression;
                int processId = rootVisualizedExpression.InspectionSession?.Process?.LivePart?.Id ?? 0;
                string moduleName = rootVisualizedExpression.Module?.Name;
                string typeString = rootVisualizedExpression.Type;
                ulong address = 0;
                bool hasAddress = false;

                if (VisualizedExpression.ValueHome.TagValue == DkmExpressionValueHome.Tag.PointerValueHome)
                {
                    address = (VisualizedExpression.ValueHome as DkmPointerValueHome).Address;
                    hasAddress = true;
                }

                if (string.IsNullOrEmpty(typeString) || string.IsNullOrEmpty(moduleName) || !hasAddress)
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

                string title;

                try
                {
                    Process process = Process.All.First(p => p.SystemId == processId);
                    Module module = process.ModulesByName[System.IO.Path.GetFileNameWithoutExtension(moduleName)];
                    CodeType codeType = ResolveCodeType(process, module, typeString);

                    Variable = codeType.IsPointer ? Variable.CreatePointer(codeType, address) : Variable.Create(codeType, address);
                    title = Variable.ToString();
                    ResultVisualizer = CsDebugScript.UI.ResultVisualizers.ResultVisualizer.Create(Variable, Variable.GetType(), "result", CompletionDataType.Unknown, dummyInteractiveResultVisualizer);
                }
                catch
                {
                    title = "{...CsDebugScript...}";
                }

                DkmDataAddress dkmDataAddress = DkmDataAddress.Create(VisualizedExpression.RuntimeInstance, address, rootVisualizedExpression.StackFrame?.InstructionAddress);

                EvaluationResult = DkmSuccessEvaluationResult.Create(
                    VisualizedExpression.InspectionContext,
                    VisualizedExpression.StackFrame,
                    rootVisualizedExpression.Name,
                    rootVisualizedExpression.FullName,
                    DkmEvaluationResultFlags.ReadOnly | DkmEvaluationResultFlags.Expandable,
                    title,
                    "",
                    rootVisualizedExpression.Type,
                    DkmEvaluationResultCategory.Other,
                    DkmEvaluationResultAccessType.None,
                    DkmEvaluationResultStorageType.None,
                    DkmEvaluationResultTypeModifierFlags.None,
                    dkmDataAddress,
                    VSUIVisualizerService.GetUIVisualizers(ResultVisualizer),
                    null,
                    null);
                return;
            }

            // This should never happen...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resolves code type by the specified name.
        /// </summary>
        /// <param name="process">The process where type is defined.</param>
        /// <param name="module">The module where type is defined.</param>
        /// <param name="codeTypeName">The code type name.</param>
        /// <returns>Resolved code type.</returns>
        internal static CodeType ResolveCodeType(Process process, Module module, string codeTypeName)
        {
            CodeType codeType;
            int pointer = 0;

            FixCodeTypeSearchName(ref codeTypeName);
            while (codeTypeName.EndsWith("*"))
            {
                pointer++;
                codeTypeName = codeTypeName.Substring(0, codeTypeName.Length - 1).TrimEnd();
                FixCodeTypeSearchName(ref codeTypeName);
            }

            FixCodeTypeSearchName(ref codeTypeName);
            codeType = CodeType.Create(process, codeTypeName, module);
            for (int i = 0; i < pointer; i++)
            {
                codeType = codeType.PointerToType;
            }
            return codeType;
        }

        /// <summary>
        /// Removes unused keywords from the type name.
        /// </summary>
        /// <param name="typeName">The code type name.</param>
        private static void FixCodeTypeSearchName(ref string typeName)
        {
            typeName = typeName.Trim();
            if (typeName.EndsWith(" const"))
                typeName = typeName.Substring(0, typeName.Length - 6);
            if (typeName.EndsWith(" volatile"))
                typeName = typeName.Substring(0, typeName.Length - 9);
            if (typeName.StartsWith("enum "))
                typeName = typeName.Substring(5);
        }
    }
}
