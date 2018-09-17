using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Interface that represents the pen object.
    /// </summary>
    public interface IPen
    {
        /// <summary>
        /// Gets the brush used to draw the stroke.
        /// </summary>
        IBrush Brush { get; }

        /// <summary>
        /// Gets the stroke thickness.
        /// </summary>
        double Thickness { get; }

        // TODO: add more options: DashCap, DashStyle, EndLineCap, LineJoin, MiterLimit, StartLineCap
    }
}
