#pragma warning disable 1591

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    public enum TypeLeafKind : ushort
    {
        #region 16 bit type records.
        LF_MODIFIER_16t = 0x0001,
        LF_POINTER_16t = 0x0002,
        LF_ARRAY_16t = 0x0003,
        LF_CLASS_16t = 0x0004,
        LF_STRUCTURE_16t = 0x0005,
        LF_UNION_16t = 0x0006,
        LF_ENUM_16t = 0x0007,
        LF_PROCEDURE_16t = 0x0008,
        LF_MFUNCTION_16t = 0x0009,
        LF_COBOL0_16t = 0x000b,
        LF_COBOL1 = 0x000c,
        LF_BARRAY_16t = 0x000d,
        LF_NULL = 0x000f,
        LF_NOTTRAN = 0x0010,
        LF_DIMARRAY_16t = 0x0011,
        LF_VFTPATH_16t = 0x0012,
        LF_PRECOMP_16t = 0x0013,
        LF_ENDPRECOMP = 0x0014,
        LF_OEM_16t = 0x0015,
        LF_TYPESERVER_ST = 0x0016,

        LF_SKIP_16t = 0x0200,
        LF_ARGLIST_16t = 0x0201,
        LF_DEFARG_16t = 0x0202,
        LF_LIST = 0x0203,
        LF_FIELDLIST_16t = 0x0204,
        LF_DERIVED_16t = 0x0205,
        LF_BITFIELD_16t = 0x0206,
        LF_METHODLIST_16t = 0x0207,
        LF_DIMCONU_16t = 0x0208,
        LF_DIMCONLU_16t = 0x0209,
        LF_DIMVARU_16t = 0x020a,
        LF_DIMVARLU_16t = 0x020b,
        LF_REFSYM = 0x020c,
        #endregion

        #region 16 bit member types. Generally not length prefixed.
        LF_BCLASS_16t = 0x0400,
        LF_VBCLASS_16t = 0x0401,
        LF_IVBCLASS_16t = 0x0402,
        LF_ENUMERATE_ST = 0x0403,
        LF_FRIENDFCN_16t = 0x0404,
        LF_INDEX_16t = 0x0405,
        LF_MEMBER_16t = 0x0406,
        LF_STMEMBER_16t = 0x0407,
        LF_METHOD_16t = 0x0408,
        LF_NESTTYPE_16t = 0x0409,
        LF_VFUNCTAB_16t = 0x040a,
        LF_FRIENDCLS_16t = 0x040b,
        LF_ONEMETHOD_16t = 0x040c,
        LF_VFUNCOFF_16t = 0x040d,

        LF_TI16_MAX = 0x1000,

        LF_ARRAY_ST = 0x1003,
        LF_CLASS_ST = 0x1004,
        LF_STRUCTURE_ST = 0x1005,
        LF_UNION_ST = 0x1006,
        LF_ENUM_ST = 0x1007,
        LF_COBOL0 = 0x100a,
        LF_BARRAY = 0x100b,
        LF_DIMARRAY_ST = 0x100c,
        LF_VFTPATH = 0x100d,
        LF_PRECOMP_ST = 0x100e,
        LF_OEM = 0x100f,
        LF_ALIAS_ST = 0x1010,
        LF_OEM2 = 0x1011,

        LF_SKIP = 0x1200,
        LF_DEFARG_ST = 0x1202,
        LF_DERIVED = 0x1204,
        LF_DIMCONU = 0x1207,
        LF_DIMCONLU = 0x1208,
        LF_DIMVARU = 0x1209,
        LF_DIMVARLU = 0x120a,
        #endregion

        #region Member type records. These are generally not length prefixed, and appear inside of a field list record.
        LF_FRIENDFCN_ST = 0x1403,
        LF_MEMBER_ST = 0x1405,
        LF_STMEMBER_ST = 0x1406,
        LF_METHOD_ST = 0x1407,
        LF_NESTTYPE_ST = 0x1408,
        LF_FRIENDCLS = 0x140a,
        LF_ONEMETHOD_ST = 0x140b,
        LF_VFUNCOFF = 0x140c,
        LF_NESTTYPEEX_ST = 0x140d,
        LF_MEMBERMODIFY_ST = 0x140e,
        LF_MANAGED_ST = 0x140f,

        LF_ST_MAX = 0x1500,
        LF_TYPESERVER = 0x1501,
        LF_DIMARRAY = 0x1508,
        LF_PRECOMP = 0x1509,
        LF_ALIAS = 0x150a,
        LF_DEFARG = 0x150b,
        LF_FRIENDFCN = 0x150c,
        LF_NESTTYPEEX = 0x1512,
        LF_MEMBERMODIFY = 0x1513,
        LF_MANAGED = 0x1514,
        LF_STRIDED_ARRAY = 0x1516,
        LF_HLSL = 0x1517,
        LF_MODIFIER_EX = 0x1518,
        LF_VECTOR = 0x151b,
        LF_MATRIX = 0x151c,
        #endregion

        #region ID leaf records. Subsequent leaf types may be referenced from .debug$S.

        #region Numeric leaf types. These are generally contained in other records, and not encountered in the main type stream.
        LF_NUMERIC = 0x8000,
        LF_CHAR = 0x8000,
        LF_SHORT = 0x8001,
        LF_USHORT = 0x8002,
        LF_LONG = 0x8003,
        LF_ULONG = 0x8004,
        LF_REAL32 = 0x8005,
        LF_REAL64 = 0x8006,
        LF_REAL80 = 0x8007,
        LF_REAL128 = 0x8008,
        LF_QUADWORD = 0x8009,
        LF_UQUADWORD = 0x800a,
        LF_REAL48 = 0x800b,
        LF_COMPLEX32 = 0x800c,
        LF_COMPLEX64 = 0x800d,
        LF_COMPLEX80 = 0x800e,
        LF_COMPLEX128 = 0x800f,
        LF_VARSTRING = 0x8010,
        LF_OCTWORD = 0x8017,
        LF_UOCTWORD = 0x8018,
        LF_DECIMAL = 0x8019,
        LF_DATE = 0x801a,
        LF_UTF8STRING = 0x801b,
        LF_REAL16 = 0x801c,
        #endregion

        #region Padding bytes. These are emitted into alignment bytes in the type stream.
        LF_PAD0 = 0xf0,
        LF_PAD1 = 0xf1,
        LF_PAD2 = 0xf2,
        LF_PAD3 = 0xf3,
        LF_PAD4 = 0xf4,
        LF_PAD5 = 0xf5,
        LF_PAD6 = 0xf6,
        LF_PAD7 = 0xf7,
        LF_PAD8 = 0xf8,
        LF_PAD9 = 0xf9,
        LF_PAD10 = 0xfa,
        LF_PAD11 = 0xfb,
        LF_PAD12 = 0xfc,
        LF_PAD13 = 0xfd,
        LF_PAD14 = 0xfe,
        LF_PAD15 = 0xff,
        #endregion
        #endregion

        LF_POINTER = 0x1002,
        LF_MODIFIER = 0x1001,
        LF_PROCEDURE = 0x1008,
        LF_MFUNCTION = 0x1009,
        LF_LABEL = 0x000e,
        LF_ARGLIST = 0x1201,

        LF_FIELDLIST = 0x1203,

        LF_ARRAY = 0x1503,
        LF_CLASS = 0x1504,
        LF_STRUCTURE = 0x1505,
        LF_INTERFACE = 0x1519,
        LF_UNION = 0x1506,
        LF_ENUM = 0x1507,
        LF_TYPESERVER2 = 0x1515,
        LF_VFTABLE = 0x151d,
        LF_VTSHAPE = 0x000a,

        LF_BITFIELD = 0x1205,

        // Member type records. These are generally not length prefixed, and appear
        // inside of a field list record.
        LF_BCLASS = 0x1400,
        LF_BINTERFACE = 0x151a,

        LF_VBCLASS = 0x1401,
        LF_IVBCLASS = 0x1402,

        LF_VFUNCTAB = 0x1409,
        LF_STMEMBER = 0x150e,
        LF_METHOD = 0x150f,
        LF_MEMBER = 0x150d,
        LF_NESTTYPE = 0x1510,
        LF_ONEMETHOD = 0x1511,
        LF_ENUMERATE = 0x1502,
        LF_INDEX = 0x1404,

        // ID leaf records. Subsequent leaf types may be referenced from .debug$S.
        LF_FUNC_ID = 0x1601,
        LF_MFUNC_ID = 0x1602,
        LF_BUILDINFO = 0x1603,
        LF_SUBSTR_LIST = 0x1604,
        LF_STRING_ID = 0x1605,
        LF_UDT_SRC_LINE = 0x1606,
        LF_UDT_MOD_SRC_LINE = 0x1607,

        LF_METHODLIST = 0x1206,
    }
}
