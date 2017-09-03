using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Accesses information that describes the process of mapping from a block of bytes of image text to a source file line number.
    /// </summary>
    [ComImport, Guid("B388EB14-BE4D-421D-A8A1-6CF7AB057086"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaLineNumber
    {
        /// <summary>
        /// Retrieves a reference to the symbol for the compiland that contributed the bytes of image text.
        /// </summary>
        [DispId(1)]
        IDiaSymbol compiland
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves a reference to the source file object.
        /// </summary>
        [DispId(2)]
        IDiaSourceFile sourceFile
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the line number in the source file.
        /// </summary>
        [DispId(3)]
        uint lineNumber { get; }

        /// <summary>
        /// Retrieves the one-based source line number where the statement or expression ends.
        /// </summary>
        [DispId(4)]
        uint lineNumberEnd { get; }

        /// <summary>
        /// Retrieves the column number where the expression or statement begins.
        /// </summary>
        [DispId(5)]
        uint columnNumber { get; }

        /// <summary>
        /// Retrieves the column number where the expression or statement ends.
        /// </summary>
        [DispId(6)]
        uint columnNumberEnd { get; }

        /// <summary>
        /// Retrieves the section part of the memory address where a block begins.
        /// </summary>
        [DispId(7)]
        uint addressSection { get; }

        /// <summary>
        /// Retrieves the offset part of the memory address where a block begins.
        /// </summary>
        [DispId(8)]
        uint addressOffset { get; }

        /// <summary>
        /// Retrieves the image relative virtual address (RVA) of a block.
        /// </summary>
        [DispId(9)]
        uint relativeVirtualAddress { get; }

        /// <summary>
        /// Retrieves the virtual address (VA) of a block.
        /// </summary>
        [DispId(10)]
        ulong virtualAddress { get; }

        /// <summary>
        /// Retrieves the number of bytes in a block.
        /// </summary>
        [DispId(11)]
        uint length { get; }

        /// <summary>
        /// Retrieves a unique source file identifier for the source file that contributed this line.
        /// </summary>
        [DispId(12)]
        uint sourceFileId { get; }

        /// <summary>
        /// Retrieves a flag indicating that this line information describes the beginning of a statement in the program source.
        /// </summary>
        [DispId(13)]
        bool statement
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the unique identifier for the compiland that contributed this line.
        /// </summary>
        [DispId(14)]
        uint compilandId { get; }
    }
}
