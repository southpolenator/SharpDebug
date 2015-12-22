using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    class Program
    {
        private static void OpenPdb(string path, out IDiaDataSource dia, out IDiaSession session)
        {
            dia = new DiaSource();
            dia.loadDataFromPdb(path);
            dia.openSession(out session);
        }

        private static void DumpSymbol(IDiaSymbol symbol)
        {
            Type type = typeof(IDiaSymbol);

            foreach (var property in type.GetProperties())
            {
                Console.WriteLine("{0} = {1}", property.Name, property.GetValue(symbol));
            }
        }

        static void Main(string[] args)
        {
            var error = Console.Error;

            if (args.Length < 2)
            {
                error.WriteLine("Not enough arguments, expected 2:");
                error.WriteLine("  [pdbPath]          Path to PDB file");
                error.WriteLine("  type1,type2,...    Types to be exported");
                return;
            }

            string pdbPath = args[0];
            string[] typeNames = args[1].Split(",".ToCharArray());
            UserTypeGenerationFlags options = UserTypeGenerationFlags.GenerateFieldComment | UserTypeGenerationFlags.SingleLineProperty;

            string moduleName = Path.GetFileNameWithoutExtension(pdbPath).ToLower();
            Dictionary<string, UserType> symbols = new Dictionary<string, UserType>();
            IDiaDataSource dia;
            IDiaSession session;

            OpenPdb(pdbPath, out dia, out session);
            foreach (var typeName in typeNames)
            {
                IDiaSymbol symbol = session.globalScope.GetChild(typeName, SymTagEnum.SymTagUDT);

                if (symbol == null)
                {
                    error.WriteLine("Symbol not found: {0}", typeName);
                }
                else
                {
                    symbols.Add(typeName, new UserType(symbol, moduleName));
                }
            }

            foreach (var symbolEntry in symbols)
            {
                var userType = symbolEntry.Value;

                if (userType.Symbol.name.Contains("::"))
                {
                    string[] names = userType.Symbol.name.Split(new string[] { "::" }, StringSplitOptions.None);

                    string parentTypeName = string.Join("::", names.Take(names.Length - 1));
                    if (symbols.ContainsKey(parentTypeName))
                    {
                        userType.SetDeclaredInType(symbols[parentTypeName]);
                    }
                    else
                    {
                        throw new Exception("Unsupported namespace of class " + userType.Symbol.name);
                    }
                }
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\output\\";
            Directory.CreateDirectory(outputDirectory);

            string[] allUDTs = session.globalScope.GetChildren(SymTagEnum.SymTagUDT).Select(s => s.name).Distinct().OrderBy(s => s).ToArray();

            File.WriteAllLines(outputDirectory + "symbols.txt", allUDTs);

            foreach (var symbolEntry in symbols)
            {
                var userType = symbolEntry.Value;
                var symbol = userType.Symbol;

                Console.WriteLine(symbolEntry.Key);
                if (userType.DeclaredInType != null)
                {
                    continue;
                }

                using (TextWriter output = new StreamWriter(string.Format("{0}{1}.exported.cs", outputDirectory, symbol.name)))
                {
                    userType.WriteCode(new IndentedWriter(output), error, symbols, options);
                }
            }
        }
    }
}
