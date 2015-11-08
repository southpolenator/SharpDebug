using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsScriptManagedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CsScriptManaged.Context.Execute(@"..\..\..\..\samples\script.cs", new string[] { });
        }
    }
}
