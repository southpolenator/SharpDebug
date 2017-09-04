using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Describes the properties of a symbol instance.
    /// <note>Symbols will return meaningful data for only some of these methods, depending on the type of symbol.</note>
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CB787B2F-BD6C-4635-BA52-933126BD2DCD")]
    public interface IDiaSymbol
    {
        /// <summary>
        /// Retrieves the unique symbol identifier.
        /// </summary>
        [DispId(0)]
        uint symIndexId { get; }

        /// <summary>
        /// Retrieves the symbol type classifier.
        /// </summary>
        [DispId(1)]
        SymTagEnum symTag { get; }

        /// <summary>
        /// Retrieves the name of the symbol.
        /// </summary>
        [DispId(2)]
        string name
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves a reference to the lexical parent of the symbol.
        /// </summary>
        [DispId(3)]
        IDiaSymbol lexicalParent
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves a reference to the class parent of the symbol.
        /// </summary>
        [DispId(4)]
        IDiaSymbol classParent
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves a reference to the function signature.
        /// </summary>
        [DispId(5)]
        IDiaSymbol type
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the variable classification of a data symbol.
        /// </summary>
        [DispId(6)]
        DataKind dataKind { get; }

        /// <summary>
        /// Retrieves the location type of a data symbol.
        /// </summary>
        [DispId(7)]
        LocationType locationType { get; }

        /// <summary>
        /// Retrieves the section part of an address location.
        /// </summary>
        [DispId(8)]
        uint addressSection { get; }

        /// <summary>
        /// Retrieves the offset part of an address location.
        /// </summary>
        [DispId(9)]
        uint addressOffset { get; }

        /// <summary>
        /// Retrieves the relative virtual address (RVA) of the location.
        /// </summary>
        [DispId(10)]
        uint relativeVirtualAddress { get; }

        /// <summary>
        /// Retrieves the virtual address (VA) of the location.
        /// </summary>
        [DispId(11)]
        ulong virtualAddress { get; }

        /// <summary>
        /// Retrieves the register designator of the location.
        /// </summary>
        [DispId(12)]
        CV_HREG_e registerId { get; }

        /// <summary>
        /// Retrieves the offset of the symbol location.
        /// </summary>
        [DispId(13)]
        int offset { get; }

        /// <summary>
        /// Retrieves the number of bytes of memory used by the object represented by this symbol.
        /// </summary>
        [DispId(14)]
        ulong length { get; }

        /// <summary>
        /// Retrieves the slot number of the location.
        /// </summary>
        [DispId(15)]
        uint slot { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is volatile.
        /// </summary>
        [DispId(16)]
        bool volatileType
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is constant.
        /// </summary>
        [DispId(17)]
        bool constType
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is unaligned.
        /// </summary>
        [DispId(18)]
        bool unalignedType
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the access modifier of a class member.
        /// </summary>
        [DispId(19)]
        uint access { get; }

        /// <summary>
        /// Retrieves the file name of the library or object file from which the object was loaded.
        /// </summary>
        [DispId(20)]
        string libraryName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the platform type for which the program or compiland was compiled.
        /// </summary>
        [DispId(21)]
        CV_CPU_TYPE_e platform { get; }

        /// <summary>
        /// Retrieves the language of the source.
        /// </summary>
        [DispId(22)]
        CV_CFL_LANG language { get; }

        /// <summary>
        /// Retrieves the flag describing the Edit and Continue features of the compiled program or unit.
        /// </summary>
        [DispId(23)]
        bool editAndContinueEnabled
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the front-end major version number.
        /// </summary>
        [DispId(24)]
        uint frontEndMajor { get; }

        /// <summary>
        /// Retrieves the front-end minor version number.
        /// </summary>
        [DispId(25)]
        uint frontEndMinor { get; }

        /// <summary>
        /// Retrieves the front-end build number.
        /// </summary>
        [DispId(26)]
        uint frontEndBuild { get; }

        /// <summary>
        /// Retrieves the back-end major version number.
        /// </summary>
        [DispId(27)]
        uint backEndMajor { get; }

        /// <summary>
        /// Retrieves the back-end minor version number.
        /// </summary>
        [DispId(28)]
        uint backEndMinor { get; }

        /// <summary>
        /// Retrieves the back-end build number.
        /// </summary>
        [DispId(29)]
        uint backEndBuild { get; }

        /// <summary>
        /// Retrieves the file name of the source file.
        /// </summary>
        [DispId(30)]
        string sourceFileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// !!! Not used any more. Not even documented.
        /// </summary>
        [DispId(31)]
        string unused
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the thunk type of a function.
        /// </summary>
        [DispId(32)]
        uint thunkOrdinal { get; }

        /// <summary>
        /// Retrieves the logical <c>this</c> adjustor for the method.
        /// </summary>
        [DispId(33)]
        int thisAdjust { get; }

        /// <summary>
        /// Retrieves the offset in the virtual function table of a virtual function.
        /// </summary>
        [DispId(34)]
        uint virtualBaseOffset { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the function is virtual.
        /// </summary>
        [DispId(35)]
        bool @virtual
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function is the base class virtual function.
        /// </summary>
        [DispId(36)]
        bool intro
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that indicating whether the function is pure virtual.
        /// </summary>
        [DispId(37)]
        bool pure
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Returns an indicator of a method's calling convention.
        /// </summary>
        [DispId(38)]
        uint callingConvention { get; }

        /// <summary>
        /// Retrieves the value of a constant.
        /// </summary>
        [DispId(39)]
        object value
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            get;
        }

        /// <summary>
        /// Retrieves the type tag of a simple type.
        /// </summary>
        [DispId(40)]
        BasicType baseType { get; }

        /// <summary>
        /// Retrieves the metadata token of a managed function or variable.
        /// </summary>
        [DispId(41)]
        uint token { get; }

        /// <summary>
        /// Retrieves the timestamp of the underlying executable file.
        /// </summary>
        [DispId(42)]
        uint timeStamp { get; }

        /// <summary>
        /// Retrieves the symbol's GUID.
        /// </summary>
        [DispId(43)]
        Guid guid { get; }

        /// <summary>
        /// Retrieves the name of the file from which the symbols were loaded.
        /// </summary>
        [DispId(44)]
        string symbolsFileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether a pointer type is a reference.
        /// </summary>
        [DispId(46)]
        bool reference
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the number of items in a list or array.
        /// </summary>
        [DispId(47)]
        uint count { get; }

        /// <summary>
        /// Retrieves the bit position of a location.
        /// </summary>
        [DispId(49)]
        uint bitPosition { get; }

        /// <summary>
        /// Retrieves the symbol identifier of the array index type.
        /// </summary>
        [DispId(50)]
        IDiaSymbol arrayIndexType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is packed.
        /// </summary>
        [DispId(51)]
        bool packed
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has a constructor.
        /// </summary>
        [DispId(52)]
        bool constructor
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has overloaded operators.
        /// </summary>
        [DispId(53)]
        bool overloadedOperator
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is nested.
        /// </summary>
        [DispId(54)]
        bool nested
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has nested type definitions.
        /// </summary>
        [DispId(55)]
        bool hasNestedTypes
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has any assignment operators defined.
        /// </summary>
        [DispId(56)]
        bool hasAssignmentOperator
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has any cast operators defined.
        /// </summary>
        [DispId(57)]
        bool hasCastOperator
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type appears in a nonglobal lexical scope.
        /// </summary>
        [DispId(58)]
        bool scoped
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is a virtual base class.
        /// </summary>
        [DispId(59)]
        bool virtualBaseClass
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is an indirect virtual base class.
        /// </summary>
        [DispId(60)]
        bool indirectVirtualBaseClass
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the offset of the virtual base pointer.
        /// </summary>
        [DispId(61)]
        int virtualBasePointerOffset { get; }

        /// <summary>
        /// Retrieves the symbol interface of the type of the virtual table for a user-defined type.
        /// </summary>
        [DispId(62)]
        IDiaSymbol virtualTableShape
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the lexical parent identifier of the symbol.
        /// </summary>
        [DispId(64)]
        uint lexicalParentId { get; }

        /// <summary>
        /// Retrieves the class parent identifier of the symbol.
        /// </summary>
        [DispId(65)]
        uint classParentId { get; }

        /// <summary>
        /// Retrieves the type identifier of the symbol.
        /// </summary>
        [DispId(66)]
        uint typeId { get; }

        /// <summary>
        /// Retrieves the array index type identifier of the symbol.
        /// </summary>
        [DispId(67)]
        uint arrayIndexTypeId { get; }

        /// <summary>
        /// Retrieves the virtual table shape identifier of the symbol.
        /// </summary>
        [DispId(68)]
        uint virtualTableShapeId { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the symbol refers to a code address.
        /// </summary>
        [DispId(69)]
        bool code
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the public symbol refers to a function.
        /// </summary>
        [DispId(70)]
        bool function
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that indicating whether the symbol refers to managed code.
        /// </summary>
        [DispId(71)]
        bool managed
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the symbol refers to Microsoft Intermediate Language (MSIL) code.
        /// </summary>
        [DispId(72)]
        bool msil
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the index to the virtual base displacement table.
        /// </summary>
        [DispId(73)]
        uint virtualBaseDispIndex { get; }

        /// <summary>
        /// Retrieves the undecorated name for a C++ decorated, or linkage, name.
        /// </summary>
        [DispId(74)]
        string undecoratedName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the age value of a program database.
        /// </summary>
        [DispId(75)]
        uint age { get; }

        /// <summary>
        /// Retrieves the symbol's signature value.
        /// </summary>
        [DispId(76)]
        uint signature { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the symbol was compiler-generated.
        /// </summary>
        [DispId(77)]
        bool compilerGenerated
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether another symbol references this address.
        /// </summary>
        [DispId(78)]
        bool addressTaken
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the rank of a FORTRAN multidimensional array.
        /// </summary>
        [DispId(79)]
        uint rank { get; }

        /// <summary>
        /// Retrieves the lower bound of a FORTRAN array dimension.
        /// </summary>
        [DispId(80)]
        IDiaSymbol lowerBound
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the upper bound of a FORTRAN array dimension.
        /// </summary>
        [DispId(81)]
        IDiaSymbol upperBound
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the symbol identifier of the lower bound of a FORTRAN array dimension.
        /// </summary>
        [DispId(82)]
        uint lowerBoundId { get; }

        /// <summary>
        /// Retrieves the symbol identifier of the upper bound of a FORTRAN array dimension.
        /// </summary>
        [DispId(83)]
        uint upperBoundId { get; }

        /// <summary>
        /// Retrieves the data bytes of an OEM symbol.
        /// </summary>
        /// <param name="cbData">Size of the buffer to hold the data.</param>
        /// <param name="pcbData">Returns the number of bytes written, or, if the data parameter is null, returns the number of bytes available.</param>
        /// <param name="pbData">A buffer that is filled in with the data bytes.</param>
        void get_dataBytes(
            [In] uint cbData,
            [Out] out uint pcbData,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ref byte[] pbData);

        /// <summary>
        /// Retrieves the children of the symbol.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum" /> Enumeration. Set to <see cref="SymTagEnum.Null" /> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions" /> Enumeration enumeration can be used alone or in combination.</param>
        /// <returns>Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findChildren(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] NameSearchOptions compareFlags);

        /// <summary>
        /// Retrieves the children of the symbol. The local symbols that are returned include live range information, if the program is compiled with optimization on.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <returns>Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findChildrenEx(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] NameSearchOptions compareFlags);

        /// <summary>
        /// Retrieves the children of the symbol that are valid at a specified address.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="isect">Specifies the section component of the address.</param>
        /// <param name="offset">Specifies the offset component of the address.</param>
        /// <returns>Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findChildrenExByAddr(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] NameSearchOptions compareFlags,
            [In] uint isect,
            [In] uint offset);

        /// <summary>
        /// Retrieves the children of the symbol that are valid at a specified virtual address.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="va">Specifies the virtual address. </param>
        /// <returns>Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findChildrenExByVA(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] NameSearchOptions compareFlags,
            [In] ulong va);

        /// <summary>
        /// Retrieves the children of the symbol that are valid at a specified relative virtual address (RVA).
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="rva">Specifies the RVA.</param>
        /// <returns>Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findChildrenExByRVA(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] NameSearchOptions compareFlags,
            [In] uint rva);

        /// <summary>
        /// Retrieves the address section of a thunk target.
        /// </summary>
        [DispId(84)]
        uint targetSection { get; }

        /// <summary>
        /// Retrieves the offset section of a thunk target.
        /// </summary>
        [DispId(85)]
        uint targetOffset { get; }

        /// <summary>
        /// Retrieves the relative virtual address (RVA) of a thunk target.
        /// </summary>
        [DispId(86)]
        uint targetRelativeVirtualAddress { get; }

        /// <summary>
        /// Retrieves the virtual address (VA) of a thunk target.
        /// </summary>
        [DispId(87)]
        ulong targetVirtualAddress { get; }

        /// <summary>
        /// Retrieves the type of the target CPU.
        /// </summary>
        [DispId(88)]
        uint machineType { get; }

        /// <summary>
        /// Retrieves the symbol's oemId value.
        /// </summary>
        [DispId(89)]
        uint oemId { get; }

        /// <summary>
        /// Retrieves the symbol's oemSymbolId value.
        /// </summary>
        [DispId(90)]
        uint oemSymbolId { get; }

        /// <summary>
        /// Retrieves an array of compiler-specific types for this symbol.
        /// </summary>
        /// <param name="cTypes">Retrieves an array of compiler-specific types for this symbol.</param>
        /// <param name="pcTypes">Returns the number of types written, or, if the types parameter is null, then the total number of types available.</param>
        /// <param name="pTypes">An array that is to be filled in with the <see cref="IDiaSymbol"/> objects that represent all the types for this symbol.</param>
        void get_types(
            [In] uint cTypes,
            [Out] out uint pcTypes,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaSymbol[] pTypes);

        /// <summary>
        /// Retrieves an array of compiler-specific type identifier values for this symbol.
        /// </summary>
        /// <param name="cTypeIds">Size of the buffer to hold the data.</param>
        /// <param name="pcTypeIds">Returns the number of typeIds written, or, if typeIds is null, then the total number of type identifiers available.</param>
        /// <param name="pdwTypeIds">An array that is to be filled in with the type identifiers.</param>
        void get_typeIds(
            [In] uint cTypeIds,
            [Out] out uint pcTypeIds,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ref uint[] pdwTypeIds);

        /// <summary>
        /// Retrieves the type of the object pointer for a class method.
        /// </summary>
        [DispId(91)]
        IDiaSymbol objectPointerType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the variety of a user-defined type (UDT).
        /// </summary>
        [DispId(92)]
        uint udtKind { get; }

        /// <summary>
        /// Retrieves part or all of an undecorated name for a C++ decorated (linkage) name.
        /// </summary>
        /// <param name="undecorateOptions">Specifies a combination of flags that control what is returned.</param>
        /// <returns>Returns the undecorated name for a C++ decorated name.</returns>
        [return: MarshalAs(UnmanagedType.BStr)]
        string get_undecoratedNameEx(
            [In] UndecoratedNameOptions undecorateOptions);

        /// <summary>
        /// Retrieves a flag indicating whether the function has been declared with the noreturn attribute.
        /// </summary>
        [DispId(93)]
        bool noReturn
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function uses a custom calling convention.
        /// </summary>
        [DispId(94)]
        bool customCallingConvention
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function is marked with the noinline attribute.
        /// </summary>
        [DispId(95)]
        bool noInline
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function or label contains optimized code as well as debug information.
        /// </summary>
        [DispId(96)]
        bool optimizedCodeDebugInfo
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function or label is never reached.
        /// </summary>
        [DispId(97)]
        bool notReached
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function has a return from interrupt instruction.
        /// </summary>
        [DispId(98)]
        bool interruptReturn
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function uses a far return.
        /// </summary>
        [DispId(99)]
        bool farReturn
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether a function or thunk layer is static.
        /// </summary>
        [DispId(100)]
        bool isStatic
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the compiland contains any debugging information.
        /// </summary>
        [DispId(101)]
        bool hasDebugInfo
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the managed compiland was linked with the linker's LTCG.
        /// </summary>
        [DispId(102)]
        bool isLTCG
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the elements of a user-defined data type are aligned to a specific boundary.
        /// </summary>
        [DispId(103)]
        bool isDataAligned
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function or compiland has security checks compiled in (via the /GS (Buffer Security Check) compiler switch).
        /// </summary>
        [DispId(104)]
        bool hasSecurityChecks
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the name of the compiler used to create the Compiland.
        /// </summary>
        [DispId(105)]
        string compilerName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function contains a call to <c>alloca</c>.
        /// </summary>
        [DispId(106)]
        bool hasAlloca
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function contains a setjmp command.
        /// </summary>
        [DispId(107)]
        bool hasSetJump
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function contains a longjmp command (part of C-style exception handling).
        /// </summary>
        [DispId(108)]
        bool hasLongJump
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function has inline assembly.
        /// </summary>
        [DispId(109)]
        bool hasInlAsm
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function has a C++-style exception handler.
        /// </summary>
        [DispId(110)]
        bool hasEH
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function has Win32-style Structured Exception Handling.
        /// </summary>
        [DispId(111)]
        bool hasSEH
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function has an asynchronous exception handler.
        /// </summary>
        [DispId(112)]
        bool hasEHa
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function has the naked attribute.
        /// </summary>
        [DispId(113)]
        bool isNaked
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the data is part of an aggregate of many symbols.
        /// </summary>
        [DispId(114)]
        bool isAggregated
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the data has been split into an aggregate of separate symbols.
        /// </summary>
        [DispId(115)]
        bool isSplitted
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the containing symbol of this symbol.
        /// </summary>
        [DispId(116)]
        IDiaSymbol container
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the function has been marked with the inline attribute.
        /// </summary>
        [DispId(117)]
        bool inlSpec
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether no stack ordering could be done as part of stack buffer checking.
        /// </summary>
        [DispId(118)]
        bool noStackOrdering
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the type of a virtual base table pointer.
        /// </summary>
        [DispId(119)]
        IDiaSymbol virtualBaseTableType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the module contains managed code.
        /// </summary>
        [DispId(120)]
        bool hasManagedCode
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the module was compiled with the /hotpatch (Create Hotpatchable Image) compiler switch.
        /// </summary>
        [DispId(121)]
        bool isHotpatchable
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the module was converted from Common Intermediate Language (CIL) to native code.
        /// </summary>
        [DispId(122)]
        bool isCVTCIL
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the managed compiland is a .netmodule (containing only metadata).
        /// </summary>
        [DispId(123)]
        bool isMSILNetmodule
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether the symbol file contains C types.
        /// </summary>
        [DispId(124)]
        bool isCTypes
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating whether private symbols have been stripped from the symbol file.
        /// </summary>
        [DispId(125)]
        bool isStripped
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(126)]
        uint frontEndQFE { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(127)]
        uint backEndQFE { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(128)]
        bool wasInlined
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(129)]
        bool strictGSCheck
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(130)]
        bool isCxxReturnUdt
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(131)]
        bool isConstructorVirtualBase
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(132)]
        bool RValueReference
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a reference to the original (unmodified) type.
        /// </summary>
        [DispId(133)]
        IDiaSymbol unmodifiedType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(134)]
        bool framePointerPresent
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(135)]
        bool isSafeBuffers
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that specifies whether a class is an intrinsic type.
        /// </summary>
        [DispId(136)]
        bool intrinsic
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that specifies whether the class or method is sealed.
        /// </summary>
        [DispId(137)]
        bool @sealed
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(138)]
        bool hfaFloat
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(139)]
        bool hfaDouble
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Returns the section part of the starting address range in which the local symbol is valid.
        /// </summary>
        [DispId(140)]
        uint liveRangeStartAddressSection { get; }

        /// <summary>
        /// Returns the offset part of the starting address range in which the local symbol is valid.
        /// </summary>
        [DispId(141)]
        uint liveRangeStartAddressOffset { get; }

        /// <summary>
        /// Returns the start of the address range in which the local symbol is valid.
        /// </summary>
        [DispId(142)]
        uint liveRangeStartRelativeVirtualAddress { get; }

        /// <summary>
        /// Retrieves the number of valid address ranges associated with the local symbol.
        /// </summary>
        [DispId(143)]
        uint countLiveRanges { get; }

        /// <summary>
        /// Returns the length of the address range in which the local symbol is valid.
        /// </summary>
        [DispId(144)]
        ulong liveRangeLength { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(145)]
        uint offsetInUdt { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(146)]
        uint paramBasePointerRegisterId { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(147)]
        uint localBasePointerRegisterId { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(148)]
        bool isLocationControlFlowDependent
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the stride of the matrix or strided array.
        /// </summary>
        [DispId(149)]
        uint stride { get; }

        /// <summary>
        /// Retrieves the number of rows in the matrix.
        /// </summary>
        [DispId(150)]
        uint numberOfRows { get; }

        /// <summary>
        /// Retrieves the number of columns in the matrix.
        /// </summary>
        [DispId(151)]
        uint numberOfColumns { get; }

        /// <summary>
        /// Specifies whether the matrix is row major.
        /// </summary>
        [DispId(152)]
        bool isMatrixRowMajor
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        /// <param name="cnt"></param>
        /// <param name="pcnt"></param>
        /// <param name="pProperties"></param>
        void get_numericProperties(
            [In] uint cnt,
            [Out] out uint pcnt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ref uint[] pProperties);

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        /// <param name="cnt"></param>
        /// <param name="pcnt"></param>
        /// <param name="pModifiers"></param>
        void get_modifierValues(
            [In] uint cnt,
            [Out] out uint pcnt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ref ushort[] pModifiers);

        /// <summary>
        /// Specifies whether the variable carries a return value.
        /// </summary>
        [DispId(153)]
        bool isReturnValue
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether the variable is optimized away.
        /// </summary>
        [DispId(154)]
        bool isOptimizedAway
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a built-in kind of the HLSL type.
        /// </summary>
        [DispId(155)]
        uint builtInKind { get; }

        /// <summary>
        /// Retrieves the register type.
        /// </summary>
        [DispId(156)]
        uint registerType { get; }

        /// <summary>
        /// Retrieves the base data slot.
        /// </summary>
        [DispId(157)]
        uint baseDataSlot { get; }

        /// <summary>
        /// Retrieves the base data offset.
        /// </summary>
        [DispId(158)]
        uint baseDataOffset { get; }

        /// <summary>
        /// Retrieves the texture slot.
        /// </summary>
        [DispId(159)]
        uint textureSlot { get; }

        /// <summary>
        /// Retrieves the sampler slot.
        /// </summary>
        [DispId(160)]
        uint samplerSlot { get; }

        /// <summary>
        /// Retrieves the uav slot.
        /// </summary>
        [DispId(161)]
        uint uavSlot { get; }

        /// <summary>
        /// Retrieves the size of a member of a user-defined type.
        /// </summary>
        [DispId(162)]
        uint sizeInUdt { get; }

        /// <summary>
        /// Retrieves the memory space kind.
        /// </summary>
        [DispId(163)]
        uint memorySpaceKind { get; }

        /// <summary>
        /// Retrieves the ID of the original (unmodified) type.
        /// </summary>
        [DispId(164)]
        uint unmodifiedTypeId { get; }

        /// <summary>
        /// Retrieves the sub type ID.
        /// </summary>
        [DispId(165)]
        uint subTypeId { get; }

        /// <summary>
        /// Retrieves the sub type.
        /// </summary>
        [DispId(166)]
        IDiaSymbol subType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the number of modifiers that are applied to the original type.
        /// </summary>
        [DispId(167)]
        uint numberOfModifiers { get; }

        /// <summary>
        /// Retrieves the number of register indices.
        /// </summary>
        [DispId(168)]
        uint numberOfRegisterIndices { get; }

        /// <summary>
        /// Specifies whether this symbol represents High Level Shader Language (HLSL) data.
        /// </summary>
        [DispId(169)]
        bool isHLSLData
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether this symbol is a pointer to a data member.
        /// </summary>
        [DispId(170)]
        bool isPointerToDataMember
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether this symbol is a pointer to a member function.
        /// </summary>
        [DispId(171)]
        bool isPointerToMemberFunction
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether the <c>this</c> pointer points to a data member with single inheritance.
        /// </summary>
        [DispId(172)]
        bool isSingleInheritance
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether the <c>this</c> pointer points to a data member with multiple inheritance.
        /// </summary>
        [DispId(173)]
        bool isMultipleInheritance
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether the <c>this</c> pointer points to a data member with virtual inheritance.
        /// </summary>
        [DispId(174)]
        bool isVirtualInheritance
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether the <c>this</c> pointer is flagged as restricted.
        /// </summary>
        [DispId(175)]
        bool restrictedType
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Specifies whether the <c>this</c> pointer is based on a symbol value.
        /// </summary>
        [DispId(176)]
        bool isPointerBasedOnSymbolValue
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the symbol from which the pointer is based.
        /// </summary>
        [DispId(177)]
        IDiaSymbol baseSymbol
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Gets the base symbol identifier.
        /// </summary>
        [DispId(178)]
        uint baseSymbolId { get; }

        /// <summary>
        /// Retrieves the symbol ID from which the pointer is based.
        /// </summary>
        [DispId(179)]
        string objectFileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Specifies whether the module is compiled with the /SDL option.
        /// </summary>
        [DispId(184)]
        bool isSdl
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(185)]
        bool isWinRTPointer
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(186)]
        bool isRefUdt
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(187)]
        bool isValueUdt
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(188)]
        bool isInterfaceUdt
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves an enumeration that allows a client to iterate through all of the inline frames on a given address.
        /// </summary>
        /// <param name="isect">Specifies the section component of the address.</param>
        /// <param name="offset">Specifies the offset component of the address.</param>
        /// <returns>Holds an <see cref="IDiaEnumSymbols"/> object that contains the list of frames that are retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findInlineFramesByAddr(
            [In] uint isect,
            [In] uint offset);

        /// <summary>
        /// Retrieves an enumeration that allows a client to iterate through all of the inline frames on a specified relative virtual address (RVA).
        /// </summary>
        /// <param name="rva">Specifies the address as an RVA.</param>
        /// <returns>Holds an <see cref="IDiaEnumSymbols"/> object that contains the list of frames that are retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findInlineFramesByRVA(
            [In] uint rva);

        /// <summary>
        /// Retrieves an enumeration that allows a client to iterate through all of the inline frames on a specified virtual address (VA).
        /// </summary>
        /// <param name="va">Specifies the address as a VA.</param>
        /// <returns>Holds an <see cref="IDiaEnumSymbols"/> object that contains the list of frames that are retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols findInlineFramesByVA(
            [In] ulong va);

        /// <summary>
        /// Retrieves an enumeration that allows a client to iterate through the line number information of all functions that are inlined, directly or indirectly, in this symbol.
        /// </summary>
        /// <returns>Holds an <see cref="IDiaEnumLineNumbers"/> object that contains the list of line numbers that are retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumLineNumbers findInlineeLines();

        /// <summary>
        /// Retrieves an enumeration that allows a client to iterate through the line number information of all functions that are inlined, directly or indirectly, in this symbol within the specified address range.
        /// </summary>
        /// <param name="isect">Specifies the section component of the address.</param>
        /// <param name="offset">Specifies the offset component of the address.</param>
        /// <param name="length">Specifies the address range, in number of bytes, to cover with this query.</param>
        /// <returns>Holds an <see cref="IDiaEnumLineNumbers"/> object that contains the list of line numbers that are retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumLineNumbers findInlineeLinesByAddr(
            [In] uint isect,
            [In] uint offset,
            [In] uint length);

        /// <summary>
        /// Retrieves an enumeration that allows a client to iterate through the line number information of all functions that are inlined, directly or indirectly, in this symbol within the specified relative virtual address (RVA).
        /// </summary>
        /// <param name="rva">Specifies the address as an RVA.</param>
        /// <param name="length">Specifies the address range, in number of bytes, to cover with this query.</param>
        /// <returns>Holds an <see cref="IDiaEnumLineNumbers"/> object that contains the list of line numbers that are retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumLineNumbers findInlineeLinesByRVA(
            [In] uint rva,
            [In] uint length);

        /// <summary>
        /// Retrieves an enumeration that allows a client to iterate through the line number information of all functions that are inlined, directly or indirectly, in this symbol within the specified virtual address (VA).
        /// </summary>
        /// <param name="va">Specifies the address as a VA.</param>
        /// <param name="length">Specifies the address range, in number of bytes, to cover with this query.</param>
        /// <returns>Holds an <see cref="IDiaEnumLineNumbers"/> object that contains the list of line numbers that are retrieved.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumLineNumbers findInlineeLinesByVA(
            [In] ulong va,
            [In] uint length);

        /// <summary>
        /// Retrieves the source file and line number that indicate where a specified user-defined type is defined.
        /// </summary>
        /// <returns>A <see cref="IDiaLineNumber"/> object that contains the source file and line number where the user-defined.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaLineNumber getSrcLineOnTypeDefn();

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(189)]
        bool isPGO
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(190)]
        bool hasValidPGOCounts
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(191)]
        bool isOptimizedForSpeed
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(192)]
        uint PGOEntryCount { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(193)]
        uint PGOEdgeCount { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(194)]
        ulong PGODynamicInstructionCount { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(195)]
        uint staticSize { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(196)]
        uint finalLiveStaticSize { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(197)]
        string phaseName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(198)]
        bool hasControlFlowCheck
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(199)]
        bool constantExport
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(200)]
        bool dataExport
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(201)]
        bool privateExport
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(202)]
        bool noNameExport
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(203)]
        bool exportHasExplicitlyAssignedOrdinal
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(204)]
        bool exportIsForwarder
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(205)]
        uint ordinal { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(206)]
        uint frameSize { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(207)]
        uint exceptionHandlerAddressSection { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(208)]
        uint exceptionHandlerAddressOffset { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(209)]
        uint exceptionHandlerRelativeVirtualAddress { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(210)]
        ulong exceptionHandlerVirtualAddress { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaInputAssemblyFile findInputAssemblyFile();

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(211)]
        uint characteristics { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(212)]
        IDiaSymbol coffGroup
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(213)]
        uint bindID { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(214)]
        uint bindSpace { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(215)]
        uint bindSlot { get; }
    }
}
