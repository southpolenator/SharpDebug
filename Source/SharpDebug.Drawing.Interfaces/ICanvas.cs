using System.Collections.Generic;

namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Interface that represents canvas as drawing object. Canvas is container of other drawing objects.
    /// </summary>
    public interface ICanvas : IDrawing
    {
        /// <summary>
        /// Gets the read only list of drawing objects that are located on this canvas.
        /// </summary>
        IReadOnlyList<IDrawing> Drawings { get; }

        /// <summary>
        /// Clears list of drawing objects.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds drawing object to the list of drawing objects.
        /// </summary>
        /// <param name="drawing">Drawing object.</param>
        void AddDrawing(IDrawing drawing);

        /// <summary>
        /// Removes drawing object from the list of drawing objects.
        /// </summary>
        /// <param name="drawing">Drawing object.</param>
        void RemoveDrawing(IDrawing drawing);
    }
}
