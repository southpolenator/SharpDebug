using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    class IndentedWriter
    {
        private TextWriter output;
        private int indentSpaces;

        public IndentedWriter(TextWriter output, int indentSpaces = 4)
        {
            this.output = output;
            this.indentSpaces = indentSpaces;
        }

        public void WriteLine(int indentation, string format, params object[] parameters)
        {
            output.Write(new string(' ', indentation * indentSpaces));
            output.WriteLine(format, parameters);
        }

        public void WriteLine()
        {
            output.WriteLine();
        }
    }
}
