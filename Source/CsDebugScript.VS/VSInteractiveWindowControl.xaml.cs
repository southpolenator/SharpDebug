using CsDebugScript.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Interaction logic for VSInteractiveWindowControl.
    /// </summary>
    public partial class VSInteractiveWindowControl : UserControl
    {
        private InteractiveWindowContent contentControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSInteractiveWindowControl"/> class.
        /// </summary>
        public VSInteractiveWindowControl()
        {
            this.InitializeComponent();

            Grid grid = new Grid();
            contentControl = CreateInteractiveWindowContent();
            grid.Children.Add(contentControl);
            this.Content = grid;

            MakeEnabled(VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgBreakMode);
            VSContext.DebuggerEnteredBreakMode += () => MakeEnabled(true);
            VSContext.DebuggerEnteredDesignMode += () => MakeEnabled(false);
            VSContext.DebuggerEnteredRunMode += () => MakeEnabled(false);
        }

        private static InteractiveWindowContent CreateInteractiveWindowContent()
        {
            var result = new List<HighlightingColor>();
            var properties = VSContext.DTE.Properties["FontsAndColors", "TextEditor"];
            var colors = properties.Item("FontsAndColorsItems").Object as EnvDTE.FontsAndColorsItems;
            string fontFamily = properties.Item("FontFamily").Value.ToString();
            double fontSize = double.Parse(properties.Item("FontSize").Value.ToString());
            int indentationSize = 4; // TODO:

            result.Add(CreateColor("#RegularText#", colors.Item("Plain Text")));
            result.Add(CreateColor("#CurrentLine#", colors.Item("CurrentLineActiveFormat")));
            result.Add(CreateColor("#TooltipText#", colors.Item("Plain Text"), colors.Item("Peek Highlighted Text Unfocused")));
            result.Add(CreateColor("#CompletionText#", colors.Item("Plain Text"), colors.Item("Peek Background Unfocused")));

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
                var color = CreateColor(kvp.Key, colors.Item(kvp.Value));

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

            return new InteractiveWindowContent(fontFamily, fontSize * 1.4, indentationSize, result.ToArray());
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
            contentControl.IsEnabled = enabled;
        }
    }
}
