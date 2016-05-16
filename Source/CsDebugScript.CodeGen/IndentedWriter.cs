using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.CodeGen
{
    class IndentedWriter
    {
        private TextWriter output;
        private int indentSpaces;
        private bool compressed;

        public IndentedWriter(TextWriter output, int indentSpaces = 4)
        {
            this.output = output;
            this.indentSpaces = indentSpaces;
        }

        public IndentedWriter(TextWriter output, bool compressed)
            : this(output, compressed ? 1 : 4)
        {
            this.compressed = compressed;
        }

        public void WriteLine(int indentation, string format, params object[] parameters)
        {
            output.Write(new string(' ', indentation * indentSpaces));
            output.WriteLine(format, parameters);
        }

        public void WriteLine()
        {
            if (!compressed)
            {
                output.WriteLine();
            }
        }
    }
}
