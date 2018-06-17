using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CsDebugScript
{
    /// <summary>
    /// Helper class that controls the debugger
    /// </summary>
    public static class Debugger
    {
        /// <summary>
        /// If set to <c>true</c> don't use dump reader.
        /// </summary>
        internal static bool DontUseDumpReader = false;

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
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        internal static ulong ReadSimpleData(CodeType codeType, ulong address)
        {
            Process process = codeType.Module.Process;
            uint size = codeType.Size;

            if (codeType.IsPointer)
            {
                size = process.GetPointerSize();
            }

            byte[] buffer = ReadMemory(process, address, size).Bytes;

            // TODO: This doesn't work with bit fields
            switch (size)
            {
                case 1:
                    return buffer[0];
                case 2:
                    return BitConverter.ToUInt16(buffer, 0);
                case 4:
                    return BitConverter.ToUInt32(buffer, 0);
                case 8:
                    return BitConverter.ToUInt64(buffer, 0);
                default:
                    throw new Exception("Unexpected data size " + size);
            }
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
        /// Resolves the function address if the specified address points to function type public symbol
        /// or returns specified address otherwise.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <returns>Resolved function address.</returns>
        public static ulong ResolveFunctionAddress(Process process, ulong address)
        {
            bool rethrow = false;

            try
            {
                if (Context.SymbolProvider.IsFunctionAddressPublicSymbol(process, address))
                {
                    Module module = process.GetModuleByInnerAddress(address);

                    if (module != null && module.ClrModule == null)
                    {
                        const uint length = 5;
                        MemoryBuffer buffer = Debugger.ReadMemory(process, address, length);
                        byte jmpByte = UserType.ReadByte(buffer, 0);
                        uint relativeAddress = UserType.ReadUint(buffer, 1);

                        if (jmpByte != 0xe9)
                        {
                            rethrow = true;
                            throw new Exception("Unsupported jump instruction while resolving function address.");
                        }

                        return address + relativeAddress + length;
                    }
                }
            }
            catch
            {
                if (rethrow)
                {
                    throw;
                }
            }

            return address;
        }

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

            if (dumpReader != null && !DontUseDumpReader)
            {
                return dumpReader.ReadMemory(address, (int)size);
            }
            else
            {
                return Context.Debugger.ReadMemory(process, address, size);
            }
        }

        /// <summary>
        /// Reads the memory from the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        /// <returns>Buffer containing read memory</returns>
        public static MemoryBuffer ReadMemory(Process process, Variable pointer, uint size)
        {
            if (pointer.GetCodeType().IsPointer)
            {
                return ReadMemory(process, pointer.GetPointerAddress(), size);
            }
            else
            {
                return ReadMemory(process, (ulong)pointer, size);
            }
        }

        /// <summary>
        /// Reads the memory from current process.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        /// <returns>Buffer containing read memory</returns>
        public static MemoryBuffer ReadMemory(Variable pointer, uint size)
        {
            return ReadMemory(pointer.GetCodeType().Module.Process, pointer, size);
        }

        /// <summary>
        /// Reads the memory from the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        /// <returns>Buffer containing read memory</returns>
        public static MemoryBuffer ReadMemory(Process process, NakedPointer pointer, uint size)
        {
            return ReadMemory(process, pointer.GetPointerAddress(), size);
        }

        /// <summary>
        /// Reads the memory from current process.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        /// <returns>Buffer containing read memory</returns>
        public static MemoryBuffer ReadMemory(NakedPointer pointer, uint size)
        {
            return ReadMemory(Process.Current, pointer, size);
        }

        #region DebuggeeControl
        /// <summary>
        /// When doing live process debugging continues debugee execution of the current process.
        /// </summary>
        public static void ContinueExecution()
        {
            Context.Debugger.ContinueExecution(Process.Current);
        }

        /// <summary>
        /// When doing live process debugging breaks debugee execution of the current process.
        /// </summary>
        public static void BreakExecution()
        {
            Context.Debugger.BreakExecution(Process.Current);
        }

        /// <summary>
        /// When doing live process debugging continues debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to be continued.</param>
        public static void ContinueExecution(Process process)
        {
            Context.Debugger.ContinueExecution(process);
        }

        /// <summary>
        /// When doing live process debugging breaks debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to be stopped.</param>
        public static void BreakExecution(Process process)
        {
            Context.Debugger.BreakExecution(process);
        }

        /// <summary>
        /// Terminate process that is being debugged and ends debugging session.
        /// </summary>
        public static void Terminate()
        {
            Context.Debugger.Terminate(Process.Current);
        }

        /// <summary>
        /// Terminates given process.
        /// </summary>
        /// <param name="process">Process to be terminated.</param>
        public static void Terminate(Process process)
        {
            Context.Debugger.Terminate(process);
        }

        /// <summary>
        /// Adds breakpoint to current process.
        /// </summary>
        /// <param name="expression">Expression to be evaluated into breakpoint.</param>
        /// <param name="action">Action to be executed when breakpoint is hit.</param>
        /// <returns></returns>
        public static IBreakpoint AddBreakpoint(string expression, Action action)
        {
            return Context.Debugger.AddBreakpoint(Process.Current, expression, action);
        }
        #endregion

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
