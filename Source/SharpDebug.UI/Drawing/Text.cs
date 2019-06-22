using SharpDebug.Drawing.Interfaces;
using System;

namespace SharpDebug.UI.Drawing
{
    /// <summary>
    /// Implements the text object.
    /// </summary>
    internal class Text : Drawing, IText
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text" /> class.
        /// </summary>
        /// <param name="text">Text that will be drawn.</param>
        /// <param name="font">Font that will be used to draw the text.</param>
        /// <param name="foreground">Brush that will be used to paint the text.</param>
        /// <param name="background">Brush that will be used to paint the text's background.</param>
        /// <param name="left">Left position of the text.</param>
        /// <param name="top">Top position of the text.</param>
        /// <param name="width">Virtual text box width. If value is less than 0, it will be automatically computed.</param>
        /// <param name="height">Virtual text box height. If value is less than 0, it will be automatically computed.</param>
        /// <param name="horizontalAlignment">Text horizontal alignment.</param>
        /// <param name="verticalAlignment">Text vertical alignment.</param>
        /// <param name="wrapping">Text wrapping.</param>
        /// <param name="rotation">Text clockwise rotation in radians.</param>
        public Text(string text, IFont font, IBrush foreground, IBrush background, double left, double top, double width, double height, TextHorizontalAlignment horizontalAlignment, TextVerticalAlignment verticalAlignment, TextWrapping wrapping, double rotation)
        {
            Label = text;
            Font = font;
            Background = background;
            Foreground = foreground;
            Width = width;
            Height = height;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            Wrapping = wrapping;
            Rotation = rotation;

            System.Windows.FontStyle fontStyle;

            switch (font.Style)
            {
                default:
                case FontStyle.Normal:
                    fontStyle = System.Windows.FontStyles.Normal;
                    break;
                case FontStyle.Italic:
                    fontStyle = System.Windows.FontStyles.Italic;
                    break;
                case FontStyle.Oblique:
                    fontStyle = System.Windows.FontStyles.Oblique;
                    break;
            }
            UIText = new System.Windows.Controls.TextBlock()
            {
                Text = text,
                HorizontalAlignment = (System.Windows.HorizontalAlignment)horizontalAlignment,
                VerticalAlignment = (System.Windows.VerticalAlignment)((int)verticalAlignment - 1),
                Foreground = (foreground as Brush)?.UIBrush,
                Background = (background as Brush)?.UIBrush,
                FontFamily = new System.Windows.Media.FontFamily(font.Family),
                FontSize = font.Size,
                FontStyle = fontStyle,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight((int)font.Weight),
                TextWrapping = (System.Windows.TextWrapping)((int)wrapping + 1),
            };
            if (width >= 0)
                UIText.Width = width;
            if (height >= 0)
                UIText.Height = height;
            if (rotation != 0)
            {
                UIText.RenderTransform = new System.Windows.Media.RotateTransform(rotation * 180 / Math.PI);
                UIText.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }
            System.Windows.Controls.Canvas.SetLeft(UIText, left);
            System.Windows.Controls.Canvas.SetTop(UIText, top);
        }

        /// <summary>
        /// Gets the font used to draw the text.
        /// </summary>
        public IFont Font { get; private set; }

        /// <summary>
        /// Gets a brush used to paint the text's background.
        /// </summary>
        public IBrush Background { get; private set; }

        /// <summary>
        /// Gets a brush used to paint the text.
        /// </summary>
        public IBrush Foreground { get; private set; }

        /// <summary>
        /// Gets the label.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Gets the text horizontal alignment.
        /// </summary>
        public TextHorizontalAlignment HorizontalAlignment { get; private set; }

        /// <summary>
        /// Gets the text vertical alignment.
        /// </summary>
        public TextVerticalAlignment VerticalAlignment { get; private set; }

        /// <summary>
        /// Gets the text wrapping.
        /// </summary>
        public TextWrapping Wrapping { get; private set; }

        /// <summary>
        /// Gets the left position of the text.
        /// </summary>
        public double Left { get; private set; }

        /// <summary>
        /// Gets the top position of the text.
        /// </summary>
        public double Top { get; private set; }

        /// <summary>
        /// Gets the virtual text box width.
        /// If text is smaller than the box width, <see cref="HorizontalAlignment"/> will be used; if it is bigger, <see cref="Wrapping"/> will be used.
        /// If value is less than 0, it will be automatically computed.
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Gets the virtual text box height.
        /// If text is smaller that the box height, <see cref="VerticalAlignment"/> will be used.
        /// If value is less than 0, it will be automatically computed.
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Gets the text clockwise rotation in radians.
        /// </summary>
        public double Rotation { get; private set; }

        /// <summary>
        /// UI object used to represent text.
        /// </summary>
        public System.Windows.Controls.TextBlock UIText { get; private set; }

        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        public override object UIObject => UIText;
    }
}
