using CsDebugScript.UI;
using CsDebugScript.UI.ResultVisualizers;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Interface that represents UI visualizer service. It is used only for external exposure using natvis.
    /// </summary>
    [Guid("2CD61523-0C74-4611-8A47-1E0B403F5DEE")]
    public interface IVSUIVisualizerService
    {
    }

    /// <summary>
    /// List of internal visualizer ids exposed using <see cref="IVSUIVisualizerService"/>.
    /// </summary>
    public enum VSUIVisualizerType : uint
    {
        ModalWindow = 1,
        InteractiveWindow = 2,
    }

    /// <summary>
    /// Implementation of <see cref="IVSUIVisualizerService"/>.
    /// </summary>
    public class VSUIVisualizerService : IVSUIVisualizerService, IVsCppDebugUIVisualizer
    {
        /// <summary>
        /// GUID used for custom UI visualizer in natvis files.
        /// </summary>
        private static readonly string GuidString = $"{{{typeof(IVSUIVisualizerService).GUID}}}";

        /// <summary>
        /// Collection of all custom UI visualizers.
        /// </summary>
        private static ReadOnlyCollection<DkmCustomUIVisualizerInfo> AllCustomUiVisualizers;

        /// <summary>
        /// Initializes the <see cref="VSUIVisualizerService"/> class.
        /// </summary>
        static VSUIVisualizerService()
        {
            List<DkmCustomUIVisualizerInfo> allVisualizers = new List<DkmCustomUIVisualizerInfo>();
            allVisualizers.Add(DkmCustomUIVisualizerInfo.Create((uint)VSUIVisualizerType.ModalWindow, "CsDebugScript visualization", "Visualizes expression using CsDebugScript UI Visualizer", GuidString));
            allVisualizers.Add(DkmCustomUIVisualizerInfo.Create((uint)VSUIVisualizerType.InteractiveWindow, $"Add to '{VSInteractiveWindow.CaptionText}' interactive window", $"Adds expression to '{VSInteractiveWindow.CaptionText}' interactive window", GuidString));
            AllCustomUiVisualizers = new ReadOnlyCollection<DkmCustomUIVisualizerInfo>(allVisualizers);
        }

        /// <summary>
        /// Function that is being called when user wants custom UI visualizer to visualize evaluated expression.
        /// </summary>
        /// <param name="ownerHwnd">Owner HWND. It looks like it is always 0.</param>
        /// <param name="visualizerId">Visualizer type. <see cref="VSUIVisualizerType"/></param>
        /// <param name="debugProperty">Evaluated expression.</param>
        /// <returns>S_OK if succeeds, or HRESULT error otherwise.</returns>
        public int DisplayValue(uint ownerHwnd, uint visualizerId, IDebugProperty3 debugProperty)
        {
            VSUIVisualizerType visualizerType = (VSUIVisualizerType)visualizerId;

            try
            {
                // Extract data
                DkmSuccessEvaluationResult evaluationResult = DkmSuccessEvaluationResult.ExtractFromProperty(debugProperty);
                int processId = evaluationResult.InspectionSession?.Process?.LivePart?.Id ?? 0;
                string moduleName = evaluationResult.ExternalModules?.FirstOrDefault()?.Module?.Name ?? evaluationResult.StackFrame?.ModuleInstance?.Module?.Name;
                string typeString = evaluationResult.Type;
                ulong? address = evaluationResult.Address?.Value;

                if (!string.IsNullOrEmpty(typeString) && !string.IsNullOrEmpty(moduleName) && address.HasValue)
                {
                    // Create variable
                    Process process = Process.All.First(p => p.SystemId == processId);
                    Module module = process.ModulesByName[System.IO.Path.GetFileNameWithoutExtension(moduleName)];
                    CodeType codeType = VSCustomVisualizerEvaluator.ResolveCodeType(process, module, typeString);
                    Variable variable = codeType.IsPointer ? Variable.CreatePointer(codeType, address.Value) : Variable.Create(codeType, address.Value);

                    // Open visualizer window
                    switch (visualizerType)
                    {
                        case VSUIVisualizerType.InteractiveWindow:
                            // Add this variable to interactive window and make it visible.
                            VSInteractiveWindowCommand.Instance.ShowToolWindow();
                            AddVariable("temp", variable, VSInteractiveWindowCommand.Instance.InteractiveWindowTool.InteractiveControl.ContentControl);
                            break;
                        default:
                        case VSUIVisualizerType.ModalWindow:
                            // Open new modal window that will visualize this variable
                            UI.InteractiveWindow.ShowModalWindow((interactiveWindow) =>
                            {
                                AddVariable("result", variable, interactiveWindow.ContentControl);
                            });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            return 0;
        }

        private static void AddVariable(string name, Variable variable, InteractiveWindowContent contentControl)
        {
            CodeType codeType = variable.GetCodeType();
            CodeType baseCodeType = codeType;
            int pointer = 0;

            while (baseCodeType.IsPointer)
            {
                baseCodeType = baseCodeType.ElementType;
                pointer++;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("var ");
            sb.Append(name);
            sb.Append(" = ");
            sb.Append(pointer > 0 ? "Variable.CreatePointer(" : "Variable.Create(");
            sb.Append("CodeType.Create(");
            if (baseCodeType.Module.Process != Process.Current)
            {
                sb.Append($"Process.All.First(p => p.SystemId == {baseCodeType.Module.Process.SystemId}), ");
            }
            sb.Append('"');
            sb.Append(baseCodeType.Module.Name);
            sb.Append("!");
            sb.Append(baseCodeType.Name);
            sb.Append('"');
            sb.Append(")");
            for (int i = 0; i < pointer; i++)
            {
                sb.Append(".PointerToType");
            }
            sb.Append(", ");
            sb.Append(variable.GetPointerAddress());
            sb.Append(");");
            sb.AppendLine();
            sb.Append(name);

            contentControl.TextEditor.Text = sb.ToString();
            contentControl.TextEditor.TextArea_ExecuteCSharpScript(null, null);
        }

        /// <summary>
        /// Finds best collection of UI visualizers that will be used for result visualizer.
        /// </summary>
        /// <param name="resultVisualizer">Result visualizer.</param>
        /// <returns>Best collection of UI visualizers</returns>
        internal static ReadOnlyCollection<DkmCustomUIVisualizerInfo> GetUIVisualizers(IResultVisualizer resultVisualizer)
        {
            // Get resulting object that is being visualized
            object result = (resultVisualizer as ResultVisualizer)?.Result;

            // Variable can be visualized in all UI visualizers
            if (result is Variable)
            {
                return AllCustomUiVisualizers;
            }

            // TODO: There are many examples of different visualizers, for example CodeType can be visualized per byte usage
            return null;
        }
    }
}
