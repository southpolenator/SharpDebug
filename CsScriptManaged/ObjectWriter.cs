using System;

namespace CsScriptManaged
{
    /// <summary>
    /// Helper interface for writing objects during interactive scripting. Set Context.ObjectWriter to change the effects of interactive scripting.
    /// </summary>
    public interface IObjectWriter
    {
        /// <summary>
        /// Outputs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        void Output(object obj);
    }

    /// <summary>
    /// Helper class for writing objects on console during interactive scripting. Set Context.ObjectWriter to change the effects of interactive scripting.
    /// </summary>
    public class ConsoleObjectWriter : IObjectWriter
    {
        /// <summary>
        /// Outputs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Output(object obj)
        {
            if (obj != null)
            {
                Console.WriteLine(obj);
            }
        }
    }
}
