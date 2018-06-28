using System;
using System.Collections.Generic;
using CsDebugScript.Drawing.Interfaces;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Implements canvas as drawing object. Canvas is container of other drawing objects.
    /// </summary>
    internal class Canvas : Drawing, ICanvas
    {
        /// <summary>
        /// List of drawing objects that are contained in this canvas.
        /// </summary>
        private List<IDrawing> drawings = new List<IDrawing>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Brush" /> class.
        /// </summary>
        /// <param name="drawings">Drawing objects.</param>
        public Canvas(IEnumerable<IDrawing> drawings)
        {
            UICanvas = new System.Windows.Controls.Canvas();
            if (drawings != null)
            {
                foreach (IDrawing drawing in drawings)
                {
                    AddDrawing(drawing);
                }
            }
        }

        /// <summary>
        /// Gets the read only list of drawing objects that are located on this canvas.
        /// </summary>
        public IReadOnlyList<IDrawing> Drawings => drawings;

        /// <summary>
        /// UI object used to represent canvas.
        /// </summary>
        public System.Windows.Controls.Canvas UICanvas { get; private set; }

        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        public override object UIObject => UICanvas;

        /// <summary>
        /// Clears list of drawing objects.
        /// </summary>
        public void Clear()
        {
            foreach (IDrawing drawing in drawings)
            {
                Drawing d = (Drawing)drawing;

                d.Container = null;
            }
            UICanvas.Children.Clear();
        }

        /// <summary>
        /// Adds drawing object to the list of drawing objects.
        /// </summary>
        /// <param name="drawing">Drawing object.</param>
        public void AddDrawing(IDrawing drawing)
        {
            Drawing d = drawing as Drawing;

            if (d == null)
            {
                throw new ArgumentException("Drawing object doesn't implement internal class.", nameof(drawing));
            }

            int index = drawings.IndexOf(drawing);

            if (index >= 0)
            {
                throw new ArgumentException("Drawing object is already part of this canvas.", nameof(drawing));
            }

            if (d.Container != null)
            {
                d.Container.RemoveDrawing(drawing);
            }

            UICanvas.Children.Add((System.Windows.UIElement)drawing.UIObject);
            drawings.Add(drawing);
            d.Container = this;
        }

        /// <summary>
        /// Removes drawing object from the list of drawing objects.
        /// </summary>
        /// <param name="drawing">Drawing object.</param>
        public void RemoveDrawing(IDrawing drawing)
        {
            Drawing d = drawing as Drawing;

            if (d == null)
            {
                throw new ArgumentException("Drawing object doesn't implement internal class.", nameof(drawing));
            }

            int index = drawings.IndexOf(drawing);

            if (index < 0)
            {
                throw new ArgumentException("Drawing object is not part of this canvas.", nameof(drawing));
            }

            UICanvas.Children.Remove((System.Windows.UIElement)drawing.UIObject);
            ((Drawing)drawing).Container = null;
            drawings.RemoveAt(index);
        }
    }
}
