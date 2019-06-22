using CsDebugScript.Drawing.Interfaces;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Useful extension methods for converting drawing interfaces to WPF objects.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Sets pen properties to the stroke properties.
        /// </summary>
        /// <param name="shape">WPF shape object.</param>
        /// <param name="pen">Pen object.</param>
        internal static void SetPen(this System.Windows.Shapes.Shape shape, IPen pen)
        {
            var uiPen = (pen as Pen)?.UIPen;

            if (uiPen != null)
            {
                shape.Stroke = uiPen.Brush;
                shape.StrokeThickness = uiPen.Thickness;
                shape.StrokeDashCap = uiPen.DashCap;
                if (uiPen.DashStyle != null)
                {
                    shape.StrokeDashArray = uiPen.DashStyle.Dashes;
                    shape.StrokeDashOffset = uiPen.DashStyle.Offset;
                }
                shape.StrokeStartLineCap = uiPen.StartLineCap;
                shape.StrokeEndLineCap = uiPen.EndLineCap;
                shape.StrokeLineJoin = uiPen.LineJoin;
                shape.StrokeMiterLimit = uiPen.MiterLimit;
            }
        }

        /// <summary>
        /// Converts <see cref="Color"/> to WPF Color structure.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>Converted color.</returns>
        internal static System.Windows.Media.Color ToWpfColor(this Color color)
        {
            return new System.Windows.Media.Color()
            {
                R = ConvertColor(color.R),
                G = ConvertColor(color.G),
                B = ConvertColor(color.B),
                A = ConvertColor(color.A),
            };
        }

        /// <summary>
        /// Converts color channel value to WPF color channel value.
        /// </summary>
        /// <param name="color">Color channel value.</param>
        /// <returns>Converted color channel value.</returns>
        private static byte ConvertColor(double color)
        {
            if (color <= 0)
                return 0;
            if (color >= 1)
                return 255;
            return (byte)(color * 255 + 0.5);
        }
    }
}
