using System;

namespace SharpDebug
{
    /// <summary>
    /// Helper interface for writing objects during interactive scripting. Set InteractiveScriptBase.ObjectWriter to change the effects of interactive scripting.
    /// </summary>
    public interface IObjectWriter
    {
        /// <summary>
        /// Outputs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        object Output(object obj);
    }

    /// <summary>
    /// Helper class for writing objects during interactive scripting. Set InteractiveScriptBase.ObjectWriter to change the effects of interactive scripting.
    /// </summary>
    public class DefaultObjectWriter : IObjectWriter
    {
        /// <summary>
        /// Outputs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public object Output(object obj)
        {
            return obj;
        }
    }

    /// <summary>
    /// Helper class for writing objects to console during interactive scripting.
    /// </summary>
    public class ConsoleObjectWriter : IObjectWriter
    {
        /// <summary>
        /// Outputs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public object Output(object obj)
        {
            if (obj != null)
            {
                Console.WriteLine(obj.ToString());
            }

            return null;
        }
    }
}
