﻿using CsDebugScript.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using CsDebugScript.Engine.Utility;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Interaction logic for VSInteractiveWindowControl.
    /// </summary>
    public partial class VSInteractiveWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VSInteractiveWindowControl"/> class.
        /// </summary>
        /// <param name="interactiveExecutionBehavior">Customization of interactive execution.</param>
        public VSInteractiveWindowControl(InteractiveExecutionBehavior interactiveExecutionBehavior)
        {
            this.InitializeComponent();

            Grid grid = new Grid();
            ContentControl = CreateInteractiveWindowContent(interactiveExecutionBehavior);
            grid.Children.Add(ContentControl);
            this.Content = grid;

            MakeEnabled(VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgBreakMode);
            // TODO: Once we don't use this control, we should remove our actions from the events
            VSContext.DebuggerEnteredBreakMode += () => MakeEnabled(true);
            VSContext.DebuggerEnteredDesignMode += () => MakeEnabled(false);
            VSContext.DebuggerEnteredRunMode += () => MakeEnabled(false);
        }

        internal InteractiveWindowContent ContentControl { get; private set; }

        private static InteractiveWindowContent CreateInteractiveWindowContent(InteractiveExecutionBehavior interactiveExecutionBehavior)
        {
            try
            {
                var result = new List<HighlightingColor>();
                var properties = VSContext.DTE.Properties["FontsAndColors", "TextEditor"];
                var colors = properties.Item("FontsAndColorsItems").Object as EnvDTE.FontsAndColorsItems;
                string fontFamily = properties.Item("FontFamily").Value.ToString();
                double fontSize = double.Parse(properties.Item("FontSize").Value.ToString());
                DictionaryCache<string, EnvDTE.ColorableItems> colorsCache = new DictionaryCache<string, EnvDTE.ColorableItems>((name) => colors.Item(name));
                int indentationSize = 4; // TODO:

                result.Add(CreateColor("#RegularText#", colorsCache["Plain Text"]));
                result.Add(CreateColor("#CurrentLine#", colorsCache["CurrentLineActiveFormat"], colorsCache["Plain Text"]));
                result.Add(CreateColor("#TooltipText#", colorsCache["Plain Text"], colorsCache["Peek Highlighted Text Unfocused"]));
                result.Add(CreateColor("#CompletionText#", colorsCache["Plain Text"], colorsCache["Peek Background Unfocused"]));

                Dictionary<string, string> colorMap = new Dictionary<string, string>()
                {
                    { "Comment", "Comment" },
                    { "String", "String" },
                    { "StringInterpolation", "Plain Text" },
                    { "Char", "String" },
                    { "Preprocessor", "Preprocessor Keyword" },
                    { "Punctuation", "Punctuation" },
                    { "ValueTypeKeywords", "Keyword" },
                    { "ReferenceTypeKeywords", "Keyword" },
                    { "MethodCall", "Plain Text" },
                    { "NumberLiteral", "Number" },
                    { "ThisOrBaseReference", "Keyword" },
                    { "NullOrValueKeywords", "Keyword" },
                    { "Keywords", "Keyword" },
                    { "GotoKeywords", "Keyword" },
                    { "ContextKeywords", "Keyword" },
                    { "ExceptionKeywords", "Keyword" },
                    { "CheckedKeyword", "Keyword" },
                    { "UnsafeKeywords", "Keyword" },
                    { "OperatorKeywords", "Keyword" },
                    { "ParameterModifiers", "Keyword" },
                    { "Modifiers", "Keyword" },
                    { "Visibility", "Keyword" },
                    { "NamespaceKeywords", "Keyword" },
                    { "GetSetAddRemove", "Keyword" },
                    { "TrueFalse", "Keyword" },
                    { "TypeKeywords", "Keyword" },
                    { "SemanticKeywords", "Keyword" },
                };

                foreach (var kvp in colorMap)
                {
                    var color = CreateColor(kvp.Key, colorsCache[kvp.Value]);

                    color.Background = null;
                    result.Add(color);
                }

#if false
                System.Drawing.Color cc = System.Drawing.Color.FromArgb(37, 37, 38);

                IEnumerable<EnvDTE.ColorableItems> sorted = colors.Cast<EnvDTE.ColorableItems>()
                    .OrderBy(color =>
                    {
                        var c = System.Drawing.ColorTranslator.FromOle((int)color.Background);

                        return System.Math.Abs(c.B - cc.B) + System.Math.Abs(c.G - cc.G) + System.Math.Abs(c.R - cc.R);
                    });

                var sb = new System.Text.StringBuilder();

                foreach (var color in sorted.Take(10))
                {
                    var c = System.Drawing.ColorTranslator.FromOle((int)color.Background);
                    int diff = System.Math.Abs(c.B - cc.B) + System.Math.Abs(c.G - cc.G) + System.Math.Abs(c.R - cc.R);

                    sb.AppendLine($"({diff}) [{c.R}, {c.G}, {c.B}] '{color.Name}'");
                }

                MessageBox.Show(sb.ToString());
#endif

                    return new InteractiveWindowContent(interactiveExecutionBehavior, fontFamily, fontSize * 1.4, indentationSize, result.ToArray());
            }
            catch
            {
                return new InteractiveWindowContent(interactiveExecutionBehavior);
            }
        }

        private static HighlightingColor CreateColor(string name, EnvDTE.ColorableItems colorableItems)
        {
            return CreateColor(name, colorableItems, colorableItems);
        }

        private static HighlightingColor CreateColor(string name, EnvDTE.ColorableItems foreground, EnvDTE.ColorableItems background)
        {
            return new HighlightingColor()
            {
                Name = name,
                Background = ConvertDTEColor(background.Background),
                Foreground = ConvertDTEColor(foreground.Foreground),
                FontStyle = FontStyles.Normal,
                FontWeight = foreground.Bold ? FontWeights.Bold : FontWeights.Normal,
                Underline = false,
            };
        }

        private static SimpleHighlightingBrush ConvertDTEColor(uint oleColor)
        {
            var sdColor = System.Drawing.ColorTranslator.FromOle((int)oleColor);
            Color color = Color.FromArgb(sdColor.A, sdColor.R, sdColor.G, sdColor.B);

            return new SimpleHighlightingBrush(color);
        }

        private void MakeEnabled(bool enabled)
        {
            ContentControl.IsEnabled = enabled;
        }
    }
}
