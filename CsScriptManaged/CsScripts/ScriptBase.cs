using System;

namespace CsScripts
{
    public class ScriptBase
    {
        public static void write(object obj)
        {
            Console.Write(obj);
        }

        public static void write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        public static void Write(object obj)
        {
            Console.Write(obj);
        }

        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        public static void writeln(object obj)
        {
            Console.WriteLine(obj);
        }

        public static void writeln(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static void WriteLine(object obj)
        {
            Console.WriteLine(obj);
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
