﻿using System.Text;

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
        private StringBuilder output;

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
        public IndentedWriter(StringBuilder output, int indentSpaces = 4)
        {
            this.output = output;
            this.indentSpaces = indentSpaces;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedWriter"/> class.
        /// </summary>
        /// <param name="output">The output text writer.</param>
        /// <param name="compressed">if set to <c>true</c> output should be compressed.</param>
        public IndentedWriter(StringBuilder output, bool compressed)
            : this(output, compressed ? 1 : 4)
        {
            this.compressed = compressed;
        }

        /// <summary>
        /// Current indentation that will increase indentation specified to the <see cref="WriteLine(int, string, object[])"/> function.
        /// </summary>
        public int CurrentIndent { get; set; }

        /// <summary>
        /// Writes the formatted line to the output.
        /// </summary>
        /// <param name="indentation">The indentation.</param>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteLine(int indentation, string format, params object[] parameters)
        {
            WriteIndentation(indentation);
            output.AppendFormat(format, parameters);
            output.AppendLine();
        }

        /// <summary>
        /// Writes the formatted line to the output.
        /// </summary>
        /// <param name="indentation">The indentation.</param>
        /// <param name="text">The text to be written.</param>
        public void WriteLine(int indentation, string text)
        {
            WriteIndentation(indentation);
            output.AppendLine(text);
        }

        /// <summary>
        /// Writes the formatted line to the output.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteLine(string format, params object[] parameters)
        {
            WriteLine(0, format, parameters);
        }

        /// <summary>
        /// Writes the formatted line to the output.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        public void WriteLine(string text)
        {
            WriteIndentation(0);
            output.AppendLine(text);
        }

        /// <summary>
        /// Writes the empty line to the output.
        /// </summary>
        public void WriteLine()
        {
            if (!compressed)
            {
                output.AppendLine();
            }
        }

        /// <summary>
        /// Starts new line with the specified indentation. One should end line with <see cref="EndLine()"/> functions.
        /// </summary>
        /// <param name="indentation">The indentation.</param>
        public void StartLine(int indentation = 0)
        {
            WriteIndentation(indentation);
        }

        /// <summary>
        /// Starts new list with the specified indentation. One should end line with <see cref="EndLine()"/> functions.
        /// </summary>
        /// <param name="indentation">The indentation.</param>
        /// <param name="text">The text to be written.</param>
        public void StartLine(int indentation, string text)
        {
            WriteIndentation(indentation);
            output.Append(text);
        }

        /// <summary>
        /// Starts new list with the specified indentation. One should end line with <see cref="EndLine()"/> functions.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        public void StartLine(string text)
        {
            WriteIndentation(0);
            output.Append(text);
        }

        /// <summary>
        /// Writes text on the current line that was started with <see cref="StartLine(string)"/> functions.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        public void Write(string text)
        {
            output.Append(text);
        }

        /// <summary>
        /// Writes text on the current line that was started with <see cref="StartLine(string)"/> functions.
        /// </summary>
        /// <param name="value">Number to be written.</param>
        public void Write(int value)
        {
            output.Append(value);
        }

        /// <summary>
        /// Ends current line that was started with <see cref="StartLine(string)"/> functions.
        /// </summary>
        public void EndLine()
        {
            output.AppendLine();
        }

        /// <summary>
        /// Ends current line that was started with <see cref="StartLine(string)"/> functions.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        public void EndLine(string text)
        {
            output.AppendLine(text);
        }

        /// <summary>
        /// Writed indentation to the output.
        /// </summary>
        /// <param name="indentation">The indentation.</param>
        private void WriteIndentation(int indentation)
        {
            indentation += CurrentIndent;
            if (indentation > 0)
                for (int i = indentation * indentSpaces; i > 0; i--)
                    output.Append(' ');
        }
    }
}
