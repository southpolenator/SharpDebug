using CsDebugScript.Drawing.Interfaces;
using CsDebugScript.UI.Drawing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CsDebugScript.UI
{
    /// <summary>
    /// User control that displays drawing and adds few more goodies that helps debugging images.
    /// </summary>
    internal class DrawingViewer : UserControl
    {
        /// <summary>
        /// Tooltip control that shows info at mouse position.
        /// </summary>
        private ToolTip tooltip;

        /// <summary>
        /// Max size of the tooltip control accumulated over time.
        /// </summary>
        private Size tooltipMaxSize;

        /// <summary>
        /// Array of bitmaps that are available in this control.
        /// </summary>
        private readonly Bitmap[] bitmaps;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingViewer"/> class.
        /// </summary>
        /// <param name="drawing">The drawing to be presented to the user.</param>
        public DrawingViewer(IDrawing drawing)
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            MouseMove += OnMouseMove;
            MouseLeave += OnMouseLeave;
            AddChild(drawing.UIObject);
            tooltip = new ToolTip
            {
                PlacementTarget = this,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
            };
            tooltipMaxSize = new Size(0, 0);
            ToolTip = tooltip;

            List<Bitmap> bmps = new List<Bitmap>();
            Queue<IDrawing> queue = new Queue<IDrawing>();

            queue.Enqueue(drawing);
            while (queue.Count > 0)
            {
                drawing = queue.Dequeue();
                if (drawing is Bitmap bitmap)
                    bmps.Add(bitmap);
                else if (drawing is ICanvas canvas)
                    foreach (var d in canvas.Drawings)
                        queue.Enqueue(d);
            }
            bitmaps = bmps.ToArray();
        }

        /// <summary>
        /// Function for handling event when mouse leaves this control.
        /// </summary>
        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            tooltip.IsOpen = false;
        }

        /// <summary>
        /// Function for handling event when mouse moves inside this control.
        /// </summary>
        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            double x = position.X - 1;
            double y = position.Y - 1;
            int xi = (int)Math.Round(x);
            int yi = (int)Math.Round(y);
            StringBuilder sb = new StringBuilder();

            sb.Append('(');
            sb.Append(xi);
            sb.Append(", ");
            sb.Append(yi);
            sb.Append(')');
            foreach (Bitmap bitmap in bitmaps)
                if (xi >= 0 && xi < bitmap.Width && yi >= 0 && yi < bitmap.Height)
                {
                    int stride = bitmap.Width * bitmap.ChannelsCount;
                    int index = yi * stride + xi * bitmap.ChannelsCount;

                    sb.AppendLine();
                    for (int i = 0; i < bitmap.Channels.Length; i++)
                    {
                        sb.Append(bitmap.Channels[i]);
                        sb.Append(": ");
                        sb.Append(bitmap.OriginalPixels.GetValue(index + i));
                    }
                }
            tooltip.Content = sb.ToString();
            tooltip.Width = double.NaN;
            tooltip.Height = double.NaN;
            tooltip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            tooltip.Width = tooltipMaxSize.Width = Math.Max(tooltipMaxSize.Width, tooltip.DesiredSize.Width);
            tooltip.Height = tooltipMaxSize.Height = Math.Max(tooltipMaxSize.Height, tooltip.DesiredSize.Height);
            tooltip.IsOpen = true;
            tooltip.HorizontalOffset = x + 20;
            tooltip.VerticalOffset = y + 20;
            e.Handled = false;
        }
    }
}
