﻿using System.Linq;

namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Useful <see cref="IGraphics"/> extensions for creating objects with different parameters.
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Creates canvas object that contains the specified list of drawings.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="drawings">Drawing objects.</param>
        /// <returns>Canvas object.</returns>
        public static ICanvas CreateCanvas(this IGraphics graphics, params IDrawing[] drawings)
        {
            return graphics.CreateCanvas(drawings);
        }

        /// <summary>
        /// Creates canvas object that contains the specified list of drawings.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="drawingObjects">Drawing visualizer objects.</param>
        /// <returns>Canvas object.</returns>
        public static ICanvas CreateCanvas(this IGraphics graphics, params IDrawingVisualizerObject[] drawingObjects)
        {
            IDrawing[] drawings = drawingObjects.Where(d => d.CanVisualize()).Select(d => d.CreateDrawing(graphics)).ToArray();

            return graphics.CreateCanvas(drawings);
        }

        /// <summary>
        /// Adds drawing objects to the list of drawing objects.
        /// </summary>
        /// <param name="canvas"><see cref="ICanvas"/> object.</param>
        /// <param name="drawings">Drawing objects.</param>
        public static void Add(this ICanvas canvas, params IDrawing[] drawings)
        {
            foreach (IDrawing drawing in drawings)
                canvas.AddDrawing(drawing);
        }

        /// <summary>
        /// Combines two drawing objects into single canvas object.
        /// If both drawing objects are canvas, first canvas will get all second canvas drawing objects and second canvas will be cleared.
        /// If only one of the objects is canvas, second object will be added to the canvas.
        /// If none of the objects is canvas, new canvas will be created with both drawings.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="drawing1">First drawing object.</param>
        /// <param name="drawing2">Second drawing object.</param>
        /// <returns>Canvas object containing both drawing objects.</returns>
        public static ICanvas Combine(this IGraphics graphics, IDrawing drawing1, IDrawing drawing2)
        {
            ICanvas canvas1 = drawing1 as ICanvas;
            ICanvas canvas2 = drawing2 as ICanvas;

            if (canvas1 != null && canvas2 != null)
            {
                IDrawing[] drawings = canvas2.Drawings.ToArray();

                canvas2.Clear();
                foreach (IDrawing drawing in drawings)
                {
                    canvas1.AddDrawing(drawing);
                }
                return canvas1;
            }
            else if (canvas1 != null)
            {
                canvas1.AddDrawing(drawing2);
                return canvas1;
            }
            else if (canvas2 != null)
            {
                canvas2.AddDrawing(drawing1);
                return canvas2;
            }
            else
            {
                return graphics.CreateCanvas(drawing1, drawing2);
            }
        }

        /// <summary>
        /// Creates pen object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="color">Pen color.</param>
        /// <param name="thickness">Pen thickness.</param>
        /// <returns>Pen object.</returns>
        public static IPen CreatePen(this IGraphics graphics, Color color, double thickness = 1)
        {
            IBrush brush = graphics.CreateSolidColorBrush(color);

            return graphics.CreatePen(brush, thickness);
        }

        /// <summary>
        /// Creates line as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="penColor">Pen color.</param>
        /// <param name="x1">First point X coordinate.</param>
        /// <param name="y1">First point Y coordinate.</param>
        /// <param name="x2">Second point X coordinate.</param>
        /// <param name="y2">Second point Y coordinate.</param>
        /// <param name="penThickness">Pen thickness.</param>
        /// <returns>Line as drawing object.</returns>
        public static ILine CreateLine(this IGraphics graphics, Color penColor, double x1, double y1, double x2, double y2, double penThickness = 1)
        {
            IPen pen = CreatePen(graphics, penColor, penThickness);

            return graphics.CreateLine(pen, x1, y1, x2, y2);
        }

        /// <summary>
        /// Creates rectangle as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="penColor">Pen color.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Rectangle width.</param>
        /// <param name="height">Rectangle height.</param>
        /// <param name="penThickness">Pen thickness.</param>
        /// <returns>Rectangle as drawing object.</returns>
        public static IRectangle CreateRectangle(this IGraphics graphics, Color penColor, double left, double top, double width, double height, double penThickness = 1)
        {
            IPen pen = CreatePen(graphics, penColor, penThickness);

            return graphics.CreateRectangle(pen, left, top, width, height);
        }

        /// <summary>
        /// Creates rectangle as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="penColor">Pen color.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Rectangle width.</param>
        /// <param name="height">Rectangle height.</param>
        /// <param name="fillBrushColor">Color of the brush used to fill the content.</param>
        /// <param name="penThickness">Pen thickness.</param>
        /// <returns>Rectangle as drawing object.</returns>
        public static IRectangle CreateRectangle(this IGraphics graphics, Color penColor, double left, double top, double width, double height, Color fillBrushColor, double penThickness = 1)
        {
            IPen pen = CreatePen(graphics, penColor, penThickness);
            IBrush brush = graphics.CreateSolidColorBrush(fillBrushColor);

            return graphics.CreateRectangle(pen, left, top, width, height, brush);
        }

        /// <summary>
        /// Creates ellipse as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="penColor">Pen color.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Ellipse rectangle width.</param>
        /// <param name="height">Ellipse rectangle height.</param>
        /// <param name="rotation">Ellipse clockwise rotation in radians.</param>
        /// <param name="penThickness">Pen thickness.</param>
        /// <returns>Ellipse as drawing object.</returns>
        public static IEllipse CreateEllipse(this IGraphics graphics, Color penColor, double left, double top, double width, double height, double rotation = 0, double penThickness = 1)
        {
            IPen pen = CreatePen(graphics, penColor, penThickness);

            return graphics.CreateEllipse(pen, left, top, width, height, rotation);
        }

        /// <summary>
        /// Creates ellipse as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="penColor">Pen color.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Ellipse rectangle width.</param>
        /// <param name="height">Ellipse rectangle height.</param>
        /// <param name="rotation">Ellipse clockwise rotation in radians.</param>
        /// <param name="fillBrushColor">Color of the brush used to fill the content.</param>
        /// <param name="penThickness">Pen thickness.</param>
        /// <returns>Ellipse as drawing object.</returns>
        public static IEllipse CreateEllipse(this IGraphics graphics, Color penColor, double left, double top, double width, double height, double rotation, Color fillBrushColor, double penThickness = 1)
        {
            IPen pen = CreatePen(graphics, penColor, penThickness);
            IBrush brush = graphics.CreateSolidColorBrush(fillBrushColor);

            return graphics.CreateEllipse(pen, left, top, width, height, rotation, brush);
        }

        /// <summary>
        /// Creates circle as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="penColor">Pen color.</param>
        /// <param name="centerX">Circle center X coordinate.</param>
        /// <param name="centerY">Circle center Y coordinate.</param>
        /// <param name="radius">Circle radius.</param>
        /// <param name="penThickness">Pen thickness.</param>
        /// <returns>Circle as drawing object.</returns>
        public static ICircle CreateCircle(this IGraphics graphics, Color penColor, double centerX, double centerY, double radius, double penThickness = 1)
        {
            IPen pen = CreatePen(graphics, penColor, penThickness);

            return graphics.CreateCircle(pen, centerX, centerY, radius);
        }

        /// <summary>
        /// Creates circle as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="penColor">Pen color.</param>
        /// <param name="centerX">Circle center X coordinate.</param>
        /// <param name="centerY">Circle center Y coordinate.</param>
        /// <param name="radius">Circle radius.</param>
        /// <param name="fillBrushColor">Color of the brush used to fill the content.</param>
        /// <param name="penThickness">Pen thickness.</param>
        /// <returns>Circle as drawing object.</returns>
        public static ICircle CreateCircle(this IGraphics graphics, Color penColor, double centerX, double centerY, double radius, Color fillBrushColor, double penThickness = 1)
        {
            IPen pen = CreatePen(graphics, penColor, penThickness);
            IBrush brush = graphics.CreateSolidColorBrush(fillBrushColor);

            return graphics.CreateCircle(pen, centerX, centerY, radius, brush);
        }

        /// <summary>
        /// Creates text as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="text">Text that will be drawn.</param>
        /// <param name="font">Font that will be used to draw the text.</param>
        /// <param name="foregroundColor">Color of the brush that will be used to paint the text.</param>
        /// <param name="left">Left position of the text.</param>
        /// <param name="top">Top position of the text.</param>
        /// <param name="width">Virtual text box width. If value is less than 0, it will be automatically computed.</param>
        /// <param name="height">Virtual text box height. If value is less than 0, it will be automatically computed.</param>
        /// <param name="horizontalAlignment">Text horizontal alignment.</param>
        /// <param name="verticalAlignment">Text vertical alignment.</param>
        /// <param name="wrapping">Text wrapping.</param>
        /// <param name="rotation">Text clockwise rotation in radians.</param>
        /// <returns>Text as drawing object.</returns>
        public static IText CreateText(this IGraphics graphics, string text, IFont font, Color foregroundColor, double left = 0, double top = 0, double width = -1, double height = -1, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left, TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top, TextWrapping wrapping = TextWrapping.Wrap, double rotation = 0)
        {
            IBrush foreground = graphics.CreateSolidColorBrush(foregroundColor);

            return graphics.CreateText(text, font, foreground, left, top, width, height, horizontalAlignment, verticalAlignment, wrapping, rotation);
        }

        /// <summary>
        /// Creates text as drawing object.
        /// </summary>
        /// <param name="graphics"><see cref="IGraphics"/> object.</param>
        /// <param name="text">Text that will be drawn.</param>
        /// <param name="font">Font that will be used to draw the text.</param>
        /// <param name="foregroundColor">Color of the brush that will be used to paint the text.</param>
        /// <param name="backgroundColor">Color of the brush that will be used to paint the text's background.</param>
        /// <param name="left">Left position of the text.</param>
        /// <param name="top">Top position of the text.</param>
        /// <param name="width">Virtual text box width. If value is less than 0, it will be automatically computed.</param>
        /// <param name="height">Virtual text box height. If value is less than 0, it will be automatically computed.</param>
        /// <param name="horizontalAlignment">Text horizontal alignment.</param>
        /// <param name="verticalAlignment">Text vertical alignment.</param>
        /// <param name="wrapping">Text wrapping.</param>
        /// <param name="rotation">Text clockwise rotation in radians.</param>
        /// <returns>Text as drawing object.</returns>
        public static IText CreateText(this IGraphics graphics, string text, IFont font, Color foregroundColor, Color backgroundColor, double left = 0, double top = 0, double width = -1, double height = -1, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left, TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top, TextWrapping wrapping = TextWrapping.Wrap, double rotation = 0)
        {
            IBrush foreground = graphics.CreateSolidColorBrush(foregroundColor);
            IBrush background = graphics.CreateSolidColorBrush(backgroundColor);

            return graphics.CreateText(text, font, foreground, background, left, top, width, height, horizontalAlignment, verticalAlignment, wrapping, rotation);
        }
    }
}
