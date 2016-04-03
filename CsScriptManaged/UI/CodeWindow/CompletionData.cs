using System;
using System.Collections.Generic;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.NRefactory.Completion;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Globalization;

namespace CsScriptManaged.UI.CodeWindow
{
    internal class CompletionData : ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData, ICSharpCode.NRefactory.Completion.ICompletionData
    {
        private static Dictionary<CompletionDataType, ImageSource> Images = CreateImages();

        public CompletionData(CompletionDataType completionDataType, object content, double priority = 1, object description = null, string text = null)
        {
            CompletionDataType = completionDataType;
            Content = content;
            Priority = priority;
            Description = description ?? content;
            Text = text ?? content.ToString();
            CompletionText = Content.ToString();
            DisplayText = Text;
            Image = GetImage();
        }

        public CompletionDataType CompletionDataType { get; private set; }

        public CompletionCategory CompletionCategory { get; set; }

        public string CompletionText { get; set; }

        public object Content { get; private set; }

        public object Description { get; private set; }

        public DisplayFlags DisplayFlags { get; set; }

        public string DisplayText { get; set; }

        public bool HasOverloads { get; private set; } = false;

        public ImageSource Image { get; private set; }

        public IEnumerable<ICompletionData> OverloadedData { get; private set; }

        public double Priority { get; private set; }

        public string Text { get; private set; }

        string ICompletionData.Description { get; set; }

        public void AddOverload(ICompletionData data)
        {
            // Do nothing
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        internal static ImageSource GetImage(CompletionDataType completionDataType)
        {
            ImageSource image;

            Images.TryGetValue(completionDataType, out image);
            return image;
        }

        private ImageSource GetImage()
        {
            return GetImage(CompletionDataType);
        }

        private static Dictionary<CompletionDataType, ImageSource> CreateImages()
        {
            var images = new Dictionary<CompletionDataType, ImageSource>();
            foreach (CompletionDataType completionDataType in Enum.GetValues(typeof(CompletionDataType)))
                images[completionDataType] = CreateImage(completionDataType);
            return images;
        }

        private static ImageSource CreateImage(CompletionDataType completionDataType)
        {
            string text;
            Brush textColor;

            switch (completionDataType)
            {
                default:
                case CompletionDataType.Unknown:
                    return null;
                //text = "🔒";
                //textColor = Brushes.Black;
                //break;
                case CompletionDataType.Keyword:
                    text = "";
                    textColor = Brushes.Black;
                    break;
                case CompletionDataType.StaticProperty:
                    text = "🔧";
                    textColor = Brushes.Black;
                    break;
                case CompletionDataType.Property:
                    text = "🔧";
                    textColor = Brushes.Gray;
                    break;
                case CompletionDataType.Method:
                    text = "🔶";
                    textColor = Brushes.DarkViolet;
                    break;
                case CompletionDataType.StaticMethod:
                    text = "🔶";
                    textColor = Brushes.Black;
                    break;
                case CompletionDataType.Namespace:
                    text = "⧛";
                    textColor = Brushes.Gray;
                    break;
                case CompletionDataType.Variable:
                    text = "🔲";
                    textColor = Brushes.Blue;
                    break;
                case CompletionDataType.StaticVariable:
                    text = "🔲";
                    textColor = Brushes.Black;
                    break;
                case CompletionDataType.Class:
                    text = "🎓";
                    textColor = Brushes.RosyBrown;
                    break;
                case CompletionDataType.StaticClass:
                    text = "🎓";
                    textColor = Brushes.Black;
                    break;
                case CompletionDataType.Enum:
                    text = ""; //  
                    textColor = Brushes.Yellow;
                    break;
                case CompletionDataType.EnumValue:
                    text = ""; //  
                    textColor = Brushes.Blue;
                    break;
                case CompletionDataType.Event:
                    text = "🔔"; // 🌠
                    textColor = Brushes.Blue;
                    break;
                case CompletionDataType.StaticEvent:
                    text = "🔔"; // 🌠
                    textColor = Brushes.Black;
                    break;
            }

            const int ImageWidth = 256;
            const int ImageHeight = 256;
            const int FontHeight = 200;

            var pixels = new byte[ImageWidth * ImageHeight * 4];
            BitmapSource bitmapSource = BitmapSource.Create(ImageWidth, ImageHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, ImageWidth * 4);
            var visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, ImageWidth, ImageHeight));
                drawingContext.DrawText(
                    new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("Segoe UI Symbol"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                        FontHeight, textColor), new Point(0, 0));
            }

            return new DrawingImage(visual.Drawing);
        }
    }
}
