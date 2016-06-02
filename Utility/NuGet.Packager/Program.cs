using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace NuGet.Packager
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> variables = ExtractCommonVariables();

            variables.Add("version", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion + "-beta");
            Console.WriteLine("Variables:");
            foreach (var t in variables)
                Console.WriteLine("  {0} = \"{1}\"", t.Key, t.Value);
            Console.WriteLine("Processing");
            foreach (string file in Directory.EnumerateFiles(Path.GetFullPath("nuspec")))
            {
                Console.WriteLine("  {0}", Path.GetFileNameWithoutExtension(file));
                CreateNuget(file, variables);
            }
        }

        private static Dictionary<string, string> ExtractCommonVariables()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NuGet.Packager.Common.xml"))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>();
                XmlDocument xml = new XmlDocument();

                xml.Load(stream);

                XmlNode metadata = xml.ChildNodes[1];

                foreach (XmlNode node in metadata.ChildNodes)
                    variables.Add(node.Name, node.InnerText);
                return variables;
            }
        }

        private static bool CreateNuget(string file, Dictionary<string, string> variables)
        {
            string nugetExePath = Path.GetFullPath("nuget.exe");
            Process p = new Process();
            StringBuilder arguments = new StringBuilder();
            string basePath = Directory.GetCurrentDirectory();
            string outputDirectory = Path.Combine(basePath, "nugets");

            Directory.CreateDirectory(outputDirectory);
            arguments.Append($"pack \"{file}\"");
            arguments.Append($" -BasePath \"{basePath}\"");
            arguments.Append($" -OutputDirectory \"{outputDirectory}\"");
            foreach (var t in variables)
                arguments.AppendFormat($" -prop {t.Key}=\"{t.Value}\"");
            p.StartInfo.FileName = nugetExePath;
            p.StartInfo.Arguments = arguments.ToString();
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.WaitForExit();
            return p.ExitCode == 0;
        }
    }
}
