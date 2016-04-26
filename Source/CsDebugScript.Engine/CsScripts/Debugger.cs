using CsDebugScript;
using CsDebugScript.Utility;
using DbgEngManaged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CsScripts
{
    /// <summary>
    /// Helper class that controls the debugger
    /// </summary>
    public static class Debugger
    {
#region Executing native commands
        /// <summary>
        /// Executes the specified command and captures its output.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Captured text</returns>
        public static string ExecuteAndCapture(string command, params object[] parameters)
        {
            DebugOutput captureFlags = DebugOutput.Normal | DebugOutput.Error | DebugOutput.Warning | DebugOutput.Verbose
                | DebugOutput.Prompt | DebugOutput.PromptRegisters | DebugOutput.ExtensionWarning | DebugOutput.Debuggee
                | DebugOutput.DebuggeePrompt | DebugOutput.Symbols | DebugOutput.Status;
            return ExecuteAndCapture(captureFlags, command, parameters);
        }

        /// <summary>
        /// Executes the specified command and captures its output.
        /// </summary>
        /// <param name="captureFlags">The capture flags.</param>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Captured text</returns>
        public static string ExecuteAndCapture(DebugOutput captureFlags, string command, params object[] parameters)
        {
            using (StringWriter writer = new StringWriter())
            {
                var callbacks = DebuggerOutputToTextWriter.Create(writer, captureFlags);
                using (OutputCallbacksSwitcher switcher = OutputCallbacksSwitcher.Create(callbacks))
                {
                    Execute(command, parameters);
                    writer.Flush();
                    return writer.GetStringBuilder().ToString();
                }
            }
        }

        /// <summary>
        /// Executes the specified command, but leaves its output visible to the user.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Execute(string command, params object[] parameters)
        {
            Context.Debugger.Execute(command, parameters);
        }
#endregion

#region Searching memory for a pattern
#region Structure types
        /// <summary>
        /// Finds the pattern in memory of the current process.
        /// </summary>
        /// <example><code>
        ///     int pattern = 1212121212;
        ///     ulong address = Debugger.FindPatternInMemory(pattern);
        ///     if (address != 0)
        ///         Console.WriteLine("Found occurrence: {0}", address);
        /// </code></example>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindPatternInMemory<T>(T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            return FindPatternInMemory(0, structure, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the pattern in memory of the current process.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindPatternInMemory<T>(ulong memoryStart, T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            return FindPatternInMemory(memoryStart, ulong.MaxValue, structure, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the pattern in memory of the current process.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindPatternInMemory<T>(ulong memoryStart, ulong memoryEnd, T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            return FindPatternInMemory(Process.Current, memoryStart, memoryEnd, structure, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the pattern in memory of the specified process.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="process">The process.</param>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindPatternInMemory<T>(Process process, ulong memoryStart, ulong memoryEnd, T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            byte[] bytes = Convert(structure);

            return FindBytePatternInMemory(process, memoryStart, memoryEnd, bytes, 0, bytes.Length, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <example><code>
        ///     int pattern = 1212121212;
        ///     IEnumerable&lt;ulong&gt; addresses = Debugger.FindAllPatternInMemory(pattern);
        /// </code></example>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllPatternInMemory<T>(T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            return FindAllPatternInMemory(0, structure, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllPatternInMemory<T>(ulong memoryStart, T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            return FindAllPatternInMemory(memoryStart, ulong.MaxValue, structure, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllPatternInMemory<T>(ulong memoryStart, ulong memoryEnd, T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            return FindAllPatternInMemory(Process.Current, memoryStart, memoryEnd, structure, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the specified process and returns all of its occurrences.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllPatternInMemory<T>(Process process, ulong memoryStart, ulong memoryEnd, T structure, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
            where T : struct
        {
            return FindAllPatternInMemory(memoryStart, memoryEnd, searchAlignment,
                (newMemoryStart) => FindPatternInMemory(process, newMemoryStart, memoryEnd, structure, searchAlignment, searchWritableMemoryOnly));
        }
#endregion

#region Text
        /// <summary>
        /// Finds the specified text in memory of the current process. Unicode encoding will be used.
        /// </summary>
        /// <example><code>
        ///     ulong address = Debugger.FindTextPatternInMemory("qwerty");
        ///     if (address != 0)
        ///         Console.WriteLine("Found occurrence: {0}", address);
        /// </code></example>
        /// <param name="text">The text.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindTextPatternInMemory(string text, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindTextPatternInMemory(text, Encoding.Unicode, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the specified text in memory of the current process.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindTextPatternInMemory(string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindTextPatternInMemory(0, text, textEncoding, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the specified text in memory of the current process.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindTextPatternInMemory(ulong memoryStart, string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindTextPatternInMemory(memoryStart, ulong.MaxValue, text, textEncoding, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the specified text in memory of the current process.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindTextPatternInMemory(ulong memoryStart, ulong memoryEnd, string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindTextPatternInMemory(Process.Current, memoryStart, memoryEnd, text, textEncoding, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the specified text in memory of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindTextPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            byte[] bytes = textEncoding.GetBytes(text);

            return FindBytePatternInMemory(process, memoryStart, memoryEnd, bytes, 0, bytes.Length, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences. Unicode encoding will be used.
        /// </summary>
        /// <example><code>
        ///     IEnumerable&lt;ulong&gt; addresses = Debugger.FindAllTextPatternInMemory("qwerty");
        /// </code></example>
        /// <param name="text">The text.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllTextPatternInMemory(string text, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllTextPatternInMemory(text, Encoding.Unicode, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllTextPatternInMemory(string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllTextPatternInMemory(0, text, textEncoding, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllTextPatternInMemory(ulong memoryStart, string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllTextPatternInMemory(memoryStart, ulong.MaxValue, text, textEncoding, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllTextPatternInMemory(ulong memoryStart, ulong memoryEnd, string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllTextPatternInMemory(Process.Current, memoryStart, memoryEnd, text, textEncoding, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the specified process and returns all of its occurrences.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="text">The text.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllTextPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, string text, Encoding textEncoding, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllPatternInMemory(memoryStart, memoryEnd, searchAlignment,
                (newMemoryStart) => FindTextPatternInMemory(process, newMemoryStart, memoryEnd, text, textEncoding, searchAlignment, searchWritableMemoryOnly));
        }
#endregion

#region Bytes
        /// <summary>
        /// Finds the pattern in memory of the current process.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindBytePatternInMemory(byte[] pattern, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindBytePatternInMemory(0, pattern, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the pattern in memory of the current process.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindBytePatternInMemory(ulong memoryStart, byte[] pattern, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindBytePatternInMemory(memoryStart, ulong.MaxValue, pattern, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the pattern in memory of the current process.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindBytePatternInMemory(ulong memoryStart, ulong memoryEnd, byte[] pattern, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindBytePatternInMemory(memoryStart, memoryEnd, pattern, 0, pattern.Length, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the pattern in memory of the current process.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="patternStart">The pattern start.</param>
        /// <param name="patternEnd">The pattern end.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindBytePatternInMemory(ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindBytePatternInMemory(Process.Current, memoryStart, memoryEnd, pattern, patternStart, patternEnd, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds the pattern in memory of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="patternStart">The pattern start.</param>
        /// <param name="patternEnd">The pattern end.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public static ulong FindBytePatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            if (patternStart < 0)
            {
                throw new ArgumentOutOfRangeException("patternStart", "less than 0");
            }

            if (patternEnd <= patternStart)
            {
                throw new ArgumentOutOfRangeException("patternEnd", "less than patternStart");
            }

            return Context.Debugger.FindPatternInMemory(process, memoryStart, memoryEnd, pattern, patternStart, patternEnd, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllBytePatternInMemory(byte[] pattern, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllBytePatternInMemory(0, pattern, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllBytePatternInMemory(ulong memoryStart, byte[] pattern, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllBytePatternInMemory(memoryStart, ulong.MaxValue, pattern, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllBytePatternInMemory(ulong memoryStart, ulong memoryEnd, byte[] pattern, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllBytePatternInMemory(memoryStart, memoryEnd, pattern, 0, pattern.Length, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the current process and returns all of its occurrences.
        /// </summary>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="patternStart">The pattern start.</param>
        /// <param name="patternEnd">The pattern end.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllBytePatternInMemory(ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllBytePatternInMemory(Process.Current, memoryStart, memoryEnd, pattern, patternStart, patternEnd, searchAlignment, searchWritableMemoryOnly);
        }

        /// <summary>
        /// Finds pattern in memory of the specified process and returns all of its occurrences.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="patternStart">The pattern start.</param>
        /// <param name="patternEnd">The pattern end.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>Enumeration of address of the successful match.</returns>
        public static IEnumerable<ulong> FindAllBytePatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            return FindAllPatternInMemory(memoryStart, memoryEnd, searchAlignment,
                (newMemoryStart) => FindBytePatternInMemory(process, newMemoryStart, memoryEnd, pattern, patternStart, patternEnd, searchAlignment, searchWritableMemoryOnly));
        }

#endregion

        private static IEnumerable<ulong> FindAllPatternInMemory(ulong memoryStart, ulong memoryEnd, uint searchAlignment, Func<ulong, ulong> patternSearch)
        {
            do
            {
                ulong address = patternSearch(memoryStart);

                if (address == 0)
                    break;

                yield return address;
                memoryStart = address + searchAlignment;
            }
            while (memoryStart < memoryEnd);
        }
#endregion

        /// <summary>
        /// Reads the memory from the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The memory address.</param>
        /// <param name="size">The buffer size.</param>
        /// <returns>Buffer containing read memory</returns>
        public static MemoryBuffer ReadMemory(Process process, ulong address, uint size)
        {
            var dumpReader = process.DumpFileMemoryReader;

            if (dumpReader != null)
            {
                return dumpReader.ReadMemory(address, (int)size);
            }
            else
            {
                return Context.Debugger.ReadMemory(process, address, size);
            }
        }

        /// <summary>
        /// Converts the specified structure to the bytes array.
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="structure">The structure.</param>
        private static byte[] Convert<T>(T structure)
            where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] bytes = new byte[size];
            IntPtr pointer = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(structure, pointer, true);
                Marshal.Copy(pointer, bytes, 0, bytes.Length);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }
}
