using System;

namespace CsDebugScript.CodeGen
{
    /// <summary>
    /// Options used when generating user type code.
    /// </summary>
    [Flags]
    internal enum UserTypeGenerationFlags
    {
        /// <summary>
        /// The empty set of options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Generated properties should be formatted in single line.
        /// </summary>
        SingleLineProperty = 1,

        /// <summary>
        /// Generate comment with field type information.
        /// </summary>
        GenerateFieldTypeInfoComment = 2,

        /// <summary>
        /// Generated code will use DIA symbol provider
        /// </summary>
        UseClassFieldsFromDiaSymbolProvider = 4,

        /// <summary>
        /// Generated code should use operator new for creating user types instead of casting.
        /// </summary>
        ForceUserTypesToNewInsteadOfCasting = 8,

        /// <summary>
        /// Generated user type should cache value of fields.
        /// </summary>
        CacheUserTypeFields = 16,

        /// <summary>
        /// Generated user type should cache value of static fields.
        /// </summary>
        CacheStaticUserTypeFields = 32,

        /// <summary>
        /// Generated user type should lazily cache value of fields.
        /// </summary>
        LazyCacheUserTypeFields = 64,

        /// <summary>
        /// Generated code will use physical mapping instead of symbolic to access fields.
        /// </summary>
        GeneratePhysicalMappingOfUserTypes = 128,

        /// <summary>
        /// Generated code should be saved in single file.
        /// </summary>
        SingleFileExport = 256,

        /// <summary>
        /// Generator should try to match use of Hungarian notation.
        /// </summary>
        UseHungarianNotation = 512,

        /// <summary>
        /// Generated code should be compressed.
        /// </summary>
        CompressedOutput = 1024,

        /// <summary>
        /// The namespace should be generated as static class
        /// </summary>
        GenerateNamespaceAsStaticClass = 2048,

        /// <summary>
        /// Don't save generated code files if compiling with Roslyn.
        /// </summary>
        DontSaveGeneratedCodeFiles = 4096,
    }
}
