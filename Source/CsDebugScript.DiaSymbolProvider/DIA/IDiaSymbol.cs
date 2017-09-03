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
        int volatileType { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is constant.
        /// </summary>
        [DispId(17)]
        int constType { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is unaligned.
        /// </summary>
        [DispId(18)]
        int unalignedType { get; }

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
        uint platform { get; }

        /// <summary>
        /// Retrieves the language of the source.
        /// </summary>
        [DispId(22)]
        CV_CFL_LANG language { get; }

        /// <summary>
        /// Retrieves the flag describing the Edit and Continue features of the compiled program or unit.
        /// </summary>
        [DispId(23)]
        int editAndContinueEnabled { get; }

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
        int @virtual { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the function is the base class virtual function.
        /// </summary>
        [DispId(36)]
        int intro { get; }

        /// <summary>
        /// Retrieves a flag that indicating whether the function is pure virtual.
        /// </summary>
        [DispId(37)]
        int pure { get; }

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
        int reference { get; }

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
        int packed { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has a constructor.
        /// </summary>
        [DispId(52)]
        int constructor { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has overloaded operators.
        /// </summary>
        [DispId(53)]
        int overloadedOperator { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is nested.
        /// </summary>
        [DispId(54)]
        int nested { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has nested type definitions.
        /// </summary>
        [DispId(55)]
        int hasNestedTypes { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has any assignment operators defined.
        /// </summary>
        [DispId(56)]
        int hasAssignmentOperator { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type has any cast operators defined.
        /// </summary>
        [DispId(57)]
        int hasCastOperator { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type appears in a nonglobal lexical scope.
        /// </summary>
        [DispId(58)]
        int scoped { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is a virtual base class.
        /// </summary>
        [DispId(59)]
        int virtualBaseClass { get; }

        /// <summary>
        /// Retrieves a flag indicating whether the user-defined data type is an indirect virtual base class.
        /// </summary>
        [DispId(60)]
        int indirectVirtualBaseClass { get; }

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
        int msil { get; }

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
        int compilerGenerated { get; }

        /// <summary>
        /// Retrieves a flag indicating whether another symbol references this address.
        /// </summary>
        [DispId(78)]
        int addressTaken { get; }

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
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out byte[] pbData);

        /// <summary>
        /// Retrieves the children of the symbol.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="ppResult">Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</param>
        void findChildren(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] NameSearchOptions compareFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        /// <summary>
        /// Retrieves the children of the symbol. The local symbols that are returned include live range information, if the program is compiled with optimization on.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="ppResult">Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</param>
        void findChildrenEx(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        /// <summary>
        /// Retrieves the children of the symbol that are valid at a specified address.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="isect">Specifies the section component of the address.</param>
        /// <param name="offset">Specifies the offset component of the address.</param>
        /// <param name="ppResult">Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</param>
        void findChildrenExByAddr(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [In] uint isect,
            [In] uint offset,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        /// <summary>
        /// Retrieves the children of the symbol that are valid at a specified virtual address.
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="va">Specifies the virtual address. </param>
        /// <param name="ppResult">Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</param>
        void findChildrenExByVA(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [In] ulong va,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        /// <summary>
        /// Retrieves the children of the symbol that are valid at a specified relative virtual address (RVA).
        /// </summary>
        /// <param name="symTag">Specifies the symbol tags of the children to be retrieved, as defined in the <see cref="SymTagEnum"/> Enumeration. Set to <see cref="SymTagEnum.Null"/> for all children to be retrieved.</param>
        /// <param name="name">Specifies the name of the children to be retrieved. Set to null for all children to be retrieved.</param>
        /// <param name="compareFlags">Specifies the comparison options applied to name matching. Values from the <see cref="NameSearchOptions"/> Enumeration enumeration can be used alone or in combination.</param>
        /// <param name="rva">Specifies the RVA. </param>
        /// <param name="ppResult">Returns an <see cref="IDiaEnumSymbols"/> object that contains a list of the child symbols retrieved.</param>
        void findChildrenExByRVA(
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [In] uint rva,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

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
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] out IDiaSymbol pTypes);

        /// <summary>
        /// Retrieves an array of compiler-specific type identifier values for this symbol.
        /// </summary>
        /// <param name="cTypeIds">Size of the buffer to hold the data.</param>
        /// <param name="pcTypeIds">Returns the number of typeIds written, or, if typeIds is null, then the total number of type identifiers available.</param>
        /// <param name="pdwTypeIds">An array that is to be filled in with the type identifiers.</param>
        void get_typeIds(
            [In] uint cTypeIds,
            [Out] out uint pcTypeIds,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out uint[] pdwTypeIds);

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
        /// <param name="name">Returns the undecorated name for a C++ decorated name. </param>
        void get_undecoratedNameEx(
            [In] UndecoratedNameOptions undecorateOptions,
            [Out, MarshalAs(UnmanagedType.BStr)] out string name);

        [DispId(93)]
        int noReturn { get; }

        [DispId(94)]
        int customCallingConvention { get; }

        [DispId(95)]
        int noInline { get; }

        [DispId(96)]
        int optimizedCodeDebugInfo { get; }

        [DispId(97)]
        int notReached { get; }

        [DispId(98)]
        int interruptReturn { get; }

        [DispId(99)]
        int farReturn { get; }

        [DispId(100)]
        int isStatic { get; }

        [DispId(101)]
        int hasDebugInfo { get; }

        [DispId(102)]
        int isLTCG { get; }

        [DispId(103)]
        int isDataAligned { get; }

        [DispId(104)]
        int hasSecurityChecks { get; }

        [DispId(105)]
        string compilerName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        [DispId(106)]
        int hasAlloca { get; }

        [DispId(107)]
        int hasSetJump { get; }

        [DispId(108)]
        int hasLongJump { get; }

        [DispId(109)]
        int hasInlAsm { get; }

        [DispId(110)]
        int hasEH { get; }

        [DispId(111)]
        int hasSEH { get; }

        [DispId(112)]
        int hasEHa { get; }

        [DispId(113)]
        int isNaked { get; }

        [DispId(114)]
        int isAggregated { get; }

        [DispId(115)]
        int isSplitted { get; }

        [DispId(116)]
        IDiaSymbol container
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(117)]
        int inlSpec { get; }

        [DispId(118)]
        int noStackOrdering { get; }

        [DispId(119)]
        IDiaSymbol virtualBaseTableType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(120)]
        int hasManagedCode { get; }

        [DispId(121)]
        int isHotpatchable { get; }

        [DispId(122)]
        int isCVTCIL { get; }

        [DispId(123)]
        int isMSILNetmodule { get; }

        [DispId(124)]
        int isCTypes { get; }

        [DispId(125)]
        int isStripped { get; }

        [DispId(126)]
        uint frontEndQFE { get; }

        [DispId(127)]
        uint backEndQFE { get; }

        [DispId(128)]
        int wasInlined { get; }

        [DispId(129)]
        int strictGSCheck { get; }

        [DispId(130)]
        int isCxxReturnUdt { get; }

        [DispId(131)]
        int isConstructorVirtualBase { get; }

        [DispId(132)]
        int RValueReference { get; }

        [DispId(133)]
        IDiaSymbol unmodifiedType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(134)]
        int framePointerPresent { get; }

        [DispId(135)]
        int isSafeBuffers { get; }

        [DispId(136)]
        int intrinsic { get; }

        [DispId(137)]
        int @sealed { get; }

        [DispId(138)]
        int hfaFloat { get; }

        [DispId(139)]
        int hfaDouble { get; }

        [DispId(140)]
        uint liveRangeStartAddressSection { get; }

        [DispId(141)]
        uint liveRangeStartAddressOffset { get; }

        [DispId(142)]
        uint liveRangeStartRelativeVirtualAddress { get; }

        [DispId(143)]
        uint countLiveRanges { get; }

        [DispId(144)]
        ulong liveRangeLength { get; }

        [DispId(145)]
        uint offsetInUdt { get; }

        [DispId(146)]
        uint paramBasePointerRegisterId { get; }

        [DispId(147)]
        uint localBasePointerRegisterId { get; }

        [DispId(148)]
        int isLocationControlFlowDependent { get; }

        [DispId(149)]
        uint stride { get; }

        [DispId(150)]
        uint numberOfRows { get; }

        [DispId(151)]
        uint numberOfColumns { get; }

        [DispId(152)]
        int isMatrixRowMajor { get; }

        void get_numericProperties(
            [In] uint cnt,
            [Out] out uint pcnt,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out uint[] pProperties);

        void get_modifierValues(
            [In] uint cnt,
            [Out] out uint pcnt,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out ushort[] pModifiers);

        [DispId(153)]
        int isReturnValue { get; }

        [DispId(154)]
        int isOptimizedAway { get; }

        [DispId(155)]
        uint builtInKind { get; }

        [DispId(156)]
        uint registerType { get; }

        [DispId(157)]
        uint baseDataSlot { get; }

        [DispId(158)]
        uint baseDataOffset { get; }

        [DispId(159)]
        uint textureSlot { get; }

        [DispId(160)]
        uint samplerSlot { get; }

        [DispId(161)]
        uint uavSlot { get; }

        [DispId(162)]
        uint sizeInUdt { get; }

        [DispId(163)]
        uint memorySpaceKind { get; }

        [DispId(164)]
        uint unmodifiedTypeId { get; }

        [DispId(165)]
        uint subTypeId { get; }

        [DispId(166)]
        IDiaSymbol subType
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(167)]
        uint numberOfModifiers { get; }

        [DispId(168)]
        uint numberOfRegisterIndices { get; }

        [DispId(169)]
        int isHLSLData { get; }

        [DispId(170)]
        int isPointerToDataMember { get; }

        [DispId(171)]
        int isPointerToMemberFunction { get; }

        [DispId(172)]
        int isSingleInheritance { get; }

        [DispId(173)]
        int isMultipleInheritance { get; }

        [DispId(174)]
        int isVirtualInheritance { get; }

        [DispId(175)]
        int restrictedType { get; }

        [DispId(176)]
        int isPointerBasedOnSymbolValue { get; }

        [DispId(177)]
        IDiaSymbol baseSymbol
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(178)]
        uint baseSymbolId { get; }

        [DispId(179)]
        string objectFileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        [DispId(184)]
        int isSdl { get; }

        [DispId(185)]
        int isWinRTPointer { get; }

        [DispId(186)]
        int isRefUdt { get; }

        [DispId(187)]
        int isValueUdt { get; }

        [DispId(188)]
        int isInterfaceUdt { get; }

        void findInlineFramesByAddr(
            [In] uint isect,
            [In] uint offset,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findInlineFramesByRVA(
            [In] uint rva,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findInlineFramesByVA(
            [In] ulong va,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findInlineeLines(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineeLinesByAddr(
            [In] uint isect,
            [In] uint offset,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineeLinesByRVA(
            [In] uint rva,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineeLinesByVA(
            [In] ulong va,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void getSrcLineOnTypeDefn(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaLineNumber ppResult);

        [DispId(189)]
        int isPGO { get; }

        [DispId(190)]
        int hasValidPGOCounts { get; }

        [DispId(191)]
        int isOptimizedForSpeed { get; }

        [DispId(192)]
        uint PGOEntryCount { get; }

        [DispId(193)]
        uint PGOEdgeCount { get; }

        [DispId(194)]
        ulong PGODynamicInstructionCount { get; }

        [DispId(195)]
        uint staticSize { get; }

        [DispId(196)]
        uint finalLiveStaticSize { get; }

        [DispId(197)]
        string phaseName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        [DispId(198)]
        int hasControlFlowCheck { get; }

        [DispId(199)]
        int constantExport { get; }

        [DispId(200)]
        int dataExport { get; }

        [DispId(201)]
        int privateExport { get; }

        [DispId(202)]
        int noNameExport { get; }

        [DispId(203)]
        int exportHasExplicitlyAssignedOrdinal { get; }

        [DispId(204)]
        int exportIsForwarder { get; }

        [DispId(205)]
        uint ordinal { get; }

        [DispId(206)]
        uint frameSize { get; }

        [DispId(207)]
        uint exceptionHandlerAddressSection { get; }

        [DispId(208)]
        uint exceptionHandlerAddressOffset { get; }

        [DispId(209)]
        uint exceptionHandlerRelativeVirtualAddress { get; }

        [DispId(210)]
        ulong exceptionHandlerVirtualAddress { get; }

        void findInputAssemblyFile(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaInputAssemblyFile ppResult);

        [DispId(211)]
        uint characteristics { get; }

        [DispId(212)]
        IDiaSymbol coffGroup
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(213)]
        uint bindID { get; }

        [DispId(214)]
        uint bindSpace { get; }

        [DispId(215)]
        uint bindSlot { get; }
    }
}
