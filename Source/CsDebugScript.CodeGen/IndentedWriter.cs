using System.IO;

namespace CsDebugScript.CodeGen
{
    /// <summary>
    /// Helper class that provides indented writing to the text writer.
    /// </summary>
    internal class IndentedWriter
    {
        /// <summary>
        /// The output text writer.
        /// </summary>
        private TextWriter output;

        /// <summary>
        /// The number of space characters in one indent unit.
        /// </summary>
        private int indentSpaces;

        /// <summary>
        /// Flag for compressing output.
        /// </summary>
        private bool compressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedWriter"/> class.
        /// </summary>
        /// <param name="output">The output text writer.</param>
        /// <param name="indentSpaces">The number of space characters in one indent unit.</param>
        public IndentedWriter(TextWriter output, int indentSpaces = 4)
        {
            this.output = output;
            this.indentSpaces = indentSpaces;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedWriter"/> class.
        /// </summary>
        /// <param name="output">The output text writer.</param>
        /// <param name="compressed">if set to <c>true</c> output should be compressed.</param>
        public IndentedWriter(TextWriter output, bool compressed)
            : this(output, compressed ? 1 : 4)
        {
            this.compressed = compressed;
        }

        /// <summary>
        /// Writes the formatted line to the output.
        /// </summary>
        /// <param name="indentation">The indentation.</param>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteLine(int indentation, string format, params object[] parameters)
        {
            output.Write(new string(' ', indentation * indentSpaces));
            output.WriteLine(format, parameters);
        }

        /// <summary>
        /// Writes the empty line to the output.
        /// </summary>
        public void WriteLine()
        {
            if (!compressed)
            {
                output.WriteLine();
            }
        }
    }
}
