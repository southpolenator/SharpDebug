using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace CsDebugScript.PdbSymbolProvider.Utility
{
    /// <summary>
    /// Simple wrapper class for memory mapped file.
    /// </summary>
    internal unsafe class MMFile : IDisposable
    {
        /// <summary>
        /// File stream for underlying file that will be opened for reading.
        /// </summary>
        private FileStream fileStream;

        /// <summary>
        /// Memory mapped file.
        /// </summary>
        private MemoryMappedFile memoryMappedFile;

        /// <summary>
        /// Memory mapped view stream.
        /// </summary>
        private MemoryMappedViewStream stream;

        /// <summary>
        /// Memory pointer to the file start.
        /// </summary>
        private byte* basePointer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MMFile"/> class.
        /// </summary>
        /// <param name="filePath">Path of the file to be opened.</param>
        public MMFile(string filePath)
        {
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                Length = fileStream.Length;
                memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, Guid.NewGuid().ToString(), Length, MemoryMappedFileAccess.Read, HandleInheritability.Inheritable, false);
                stream = memoryMappedFile.CreateViewStream(0, Length, MemoryMappedFileAccess.Read);
                stream.SafeMemoryMappedViewHandle.AcquirePointer(ref basePointer);
            }
            catch
            {
                stream?.Dispose();
                memoryMappedFile?.Dispose();
                fileStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            stream.SafeMemoryMappedViewHandle.ReleasePointer();
            stream.Dispose();
            memoryMappedFile.Dispose();
            fileStream.Dispose();
        }

        /// <summary>
        /// Gets the pointer to the file start.
        /// </summary>
        public byte* BasePointer => basePointer;

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        public long Length { get; private set; }
    }
}
