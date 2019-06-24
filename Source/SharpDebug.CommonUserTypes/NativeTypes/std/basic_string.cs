using SharpDebug.Exceptions;
using System;

namespace SharpDebug.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::basic_string
    /// </summary>
    [UserType(TypeName = "std::basic_string<>", CodeTypeVerification = nameof(basic_string.VerifyCodeType))]
    [UserType(TypeName = "std::__1::basic_string<>", CodeTypeVerification = nameof(basic_string.VerifyCodeType))]
    public class basic_string : UserType
    {
        private interface IBasicString
        {
            /// <summary>
            /// Gets the string length.
            /// </summary>
            int Length { get; }

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            int Reserved { get; }

            /// <summary>
            /// Gets the string text.
            /// </summary>
            string Text { get; }
        }

        /// <summary>
        /// Common code for Microsoft Visual Studio implementations of std::basic_string
        /// </summary>
        internal class VisualStudio : IBasicString
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Function that reads size field.
                /// </summary>
                public Func<ulong, int> ReadSize;

                /// <summary>
                /// Function that reads reserved field.
                /// </summary>
                public Func<ulong, int> ReadReserved;

                /// <summary>
                /// Offset of buffer field.
                /// </summary>
                public int BufferOffset;

                /// <summary>
                /// Array length of buffer field.
                /// </summary>
                public int BufferLength;

                /// <summary>
                /// Offset of pointer field.
                /// </summary>
                public int PointerOffset;

                /// <summary>
                /// Number of bytes needed to store a character.
                /// </summary>
                public int CharSize;

                /// <summary>
                /// Code type of std::basic_string.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// The length of the string
            /// </summary>
            private int length;

            /// <summary>
            /// The reserved number of characters in the string buffer
            /// </summary>
            private int reserved;

            /// <summary>
            /// The text inside the string buffer
            /// </summary>
            private UserMember<string> text;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                length = data.ReadSize(address);
                reserved = data.ReadReserved(address);
                text = UserMember.Create(() => GetText());
            }

            /// <summary>
            /// Gets the string length.
            /// </summary>
            public int Length => length;

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            public int Reserved => reserved;

            /// <summary>
            /// Gets the string text.
            /// </summary>
            public string Text => text.Value;

            /// <summary>
            /// Gets the string text.
            /// </summary>
            private string GetText()
            {
                ulong stringAddress;

                if (length >= data.BufferLength)
                    stringAddress = data.Process.ReadPointer(address + (uint)data.PointerOffset);
                else
                    stringAddress = address + (uint)data.BufferOffset;
                return data.Process.ReadString(stringAddress, data.CharSize, length);
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::basic_string
        /// </summary>
        internal class VisualStudio2013 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2013"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio2013(Variable variable, object savedData)
                : base(variable, savedData)
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _Bx
                // | _Buf
                // | _Ptr
                // _Mysize
                // _Myres
                CodeType _Bx, _Buf, _Ptr, _Mysize, _Myres;
                var fields = codeType.GetFieldTypes();

                if (!fields.TryGetValue("_Bx", out _Bx) || !fields.TryGetValue("_Mysize", out _Mysize) || !fields.TryGetValue("_Myres", out _Myres))
                    return null;

                var _BxFields = _Bx.GetFieldTypes();

                if (!_BxFields.TryGetValue("_Buf", out _Buf) || !_BxFields.TryGetValue("_Ptr", out _Ptr))
                    return null;

                return new ExtractedData
                {
                    ReadSize = codeType.Module.Process.GetReadInt(codeType, "_Mysize"),
                    ReadReserved = codeType.Module.Process.GetReadInt(codeType, "_Myres"),
                    CharSize = (int)_Buf.ElementType.Size,
                    BufferLength = (int)(_Buf.Size / _Buf.ElementType.Size),
                    BufferOffset = codeType.GetFieldOffset("_Bx") + _Bx.GetFieldOffset("_Buf"),
                    PointerOffset = codeType.GetFieldOffset("_Bx") + _Bx.GetFieldOffset("_Ptr"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::basic_string
        /// </summary>
        internal class VisualStudio2015 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2015"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio2015(Variable variable, object savedData)
                : base(variable, savedData)
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _Mypair
                // | _Myval2
                //   | _Bx
                //     | _Buf
                //     | _Ptr
                //   | _Mysize
                //   | _Myres
                CodeType _Mypair, _Myval2, _Bx, _Buf, _Ptr, _Mysize, _Myres;

                if (!codeType.GetFieldTypes().TryGetValue("_Mypair", out _Mypair))
                    return null;
                if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                    return null;

                var _Myval2Fields = _Myval2.GetFieldTypes();

                if (!_Myval2Fields.TryGetValue("_Bx", out _Bx) || !_Myval2Fields.TryGetValue("_Mysize", out _Mysize) || !_Myval2Fields.TryGetValue("_Myres", out _Myres))
                    return null;

                var _BxFields = _Bx.GetFieldTypes();

                if (!_BxFields.TryGetValue("_Buf", out _Buf) || !_BxFields.TryGetValue("_Ptr", out _Ptr))
                    return null;

                int baseOffset = codeType.GetFieldOffset("_Mypair") + _Mypair.GetFieldOffset("_Myval2");

                return new ExtractedData
                {
                    ReadSize = codeType.Module.Process.GetReadInt(codeType, "_Mypair._Myval2._Mysize"),
                    ReadReserved = codeType.Module.Process.GetReadInt(codeType, "_Mypair._Myval2._Myres"),
                    CharSize = (int)_Buf.ElementType.Size,
                    BufferLength = (int)(_Buf.Size / _Buf.ElementType.Size),
                    BufferOffset = baseOffset + _Myval2.GetFieldOffset("_Bx") + _Bx.GetFieldOffset("_Buf"),
                    PointerOffset = baseOffset + _Myval2.GetFieldOffset("_Bx") + _Bx.GetFieldOffset("_Ptr"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string
        /// </summary>
        internal class LibStdCpp6 : IBasicString
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            private class ExtractedData
            {
                /// <summary>
                /// Function that reads length field.
                /// </summary>
                public Func<ulong, int> ReadLength;

                /// <summary>
                /// Function that reads capacity field.
                /// </summary>
                public Func<ulong, int> ReadCapacity;

                /// <summary>
                /// Offset of buffer field.
                /// </summary>
                public int BufferOffset;

                /// <summary>
                /// Array length of buffer field.
                /// </summary>
                public int BufferLength;

                /// <summary>
                /// Offset of pointer field.
                /// </summary>
                public int PointerOffset;

                /// <summary>
                /// Number of bytes needed to store a character.
                /// </summary>
                public int CharSize;

                /// <summary>
                /// Code type of std::basic_string.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// The length of the string
            /// </summary>
            private int length;

            /// <summary>
            /// The reserved number of characters in the string buffer
            /// </summary>
            private int reserved;

            /// <summary>
            /// The text inside the string buffer
            /// </summary>
            private UserMember<string> text;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public LibStdCpp6(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                length = data.ReadLength(address);
                ulong stringAddress = data.Process.ReadPointer(address + (uint)data.PointerOffset);
                bool localData = stringAddress == address + (uint)data.BufferOffset;
                reserved = localData ? data.BufferLength : data.ReadCapacity(address);
                text = UserMember.Create(() => data.Process.ReadString(stringAddress, data.CharSize, length));
            }

            /// <summary>
            /// Gets the string length.
            /// </summary>
            public int Length => length;

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            public int Reserved => reserved;

            /// <summary>
            /// Gets the string text.
            /// </summary>
            public string Text => text.Value;

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _M_dataplus
                // | _M_p
                // _M_string_length
                // @unnamed_union
                // | _M_local_buf
                // | _M_allocated_capacity
                CodeType _M_dataplus, _M_p, _M_string_length, _M_local_buf, _M_allocated_capacity;

                if (!codeType.GetFieldTypes().TryGetValue("_M_dataplus", out _M_dataplus))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("_M_string_length", out _M_string_length))
                    return null;
                if (!_M_dataplus.GetFieldTypes().TryGetValue("_M_p", out _M_p))
                    return null;

                // These should be part of unnamed union
                if (!codeType.GetFieldTypes().TryGetValue("_M_local_buf", out _M_local_buf))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("_M_allocated_capacity", out _M_allocated_capacity))
                    return null;
                return new ExtractedData
                {
                    ReadLength = codeType.Module.Process.GetReadInt(codeType, "_M_string_length"),
                    ReadCapacity = codeType.Module.Process.GetReadInt(codeType, "_M_allocated_capacity"),
                    BufferOffset = codeType.GetFieldOffset("_M_local_buf"),
                    PointerOffset = codeType.GetFieldOffset("_M_dataplus") + _M_dataplus.GetFieldOffset("_M_p"),
                    CharSize = (int)_M_local_buf.ElementType.Size,
                    BufferLength = (int)(15 / _M_local_buf.ElementType.Size),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string with _GLIBCXX_USE_CXX11_ABI not defined.
        /// </summary>
        internal class LibStdCpp6_NoAbi : IBasicString
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            private class ExtractedData
            {
                /// <summary>
                /// Function that reads length field.
                /// </summary>
                public Func<ulong, int> ReadLength;

                /// <summary>
                /// Function that reads capacity field.
                /// </summary>
                public Func<ulong, int> ReadCapacity;

                /// <summary>
                /// Offset of pointer field.
                /// </summary>
                public int PointerOffset;

                /// <summary>
                /// Number of bytes needed to store a character.
                /// </summary>
                public int CharSize;

                /// <summary>
                /// Code type of std::basic_string.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// The length of the string
            /// </summary>
            private int length;

            /// <summary>
            /// The reserved number of characters in the string buffer
            /// </summary>
            private int reserved;

            /// <summary>
            /// The text inside the string buffer
            /// </summary>
            private UserMember<string> text;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6_NoAbi"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public LibStdCpp6_NoAbi(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                ulong stringAddress = data.Process.ReadPointer(address + (uint)data.PointerOffset);
                length = data.ReadLength(stringAddress);
                reserved = data.ReadCapacity(stringAddress);
                text = UserMember.Create(() => data.Process.ReadString(stringAddress, data.CharSize, length));
            }

            /// <summary>
            /// Gets the string length.
            /// </summary>
            public int Length => length;

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            public int Reserved => reserved;

            /// <summary>
            /// Gets the string text.
            /// </summary>
            public string Text => text.Value;

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _M_dataplus
                // | _M_p
                // basic_string<>::_Rep type
                // _M_length
                // _M_capacity
                // _M_refcount
                CodeType _M_dataplus, _M_p, _Rep, _M_length, _M_capacity, _M_refcount;

                try
                {
                    _Rep = CodeType.Create($"{codeType.Name}::_Rep", codeType.Module);
                }
                catch
                {
                    return null;
                }

                if (!codeType.GetFieldTypes().TryGetValue("_M_dataplus", out _M_dataplus))
                    return null;
                if (!_M_dataplus.GetFieldTypes().TryGetValue("_M_p", out _M_p))
                    return null;
                if (!_Rep.GetFieldTypes().TryGetValue("_M_length", out _M_length))
                    return null;
                if (!_Rep.GetFieldTypes().TryGetValue("_M_capacity", out _M_capacity))
                    return null;
                if (!_Rep.GetFieldTypes().TryGetValue("_M_refcount", out _M_refcount))
                    return null;
                int repOffset = -(int)_Rep.Size;
                return new ExtractedData
                {
                    ReadLength = codeType.Module.Process.GetReadInt(_Rep, "_M_length", repOffset),
                    ReadCapacity = codeType.Module.Process.GetReadInt(_Rep, "_M_capacity", repOffset),
                    CharSize = (int)_M_p.ElementType.Size,
                    PointerOffset = codeType.GetFieldOffset("_M_dataplus") + _M_dataplus.GetFieldOffset("_M_p"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::basic_string
        /// </summary>
        internal class ClangLibCpp : IBasicString
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            private class ExtractedData
            {
                /// <summary>
                /// Function that reads short data length field.
                /// </summary>
                public Func<ulong, int> ReadShortDataLength;

                /// <summary>
                /// Function that reads long data length field.
                /// </summary>
                public Func<ulong, int> ReadLongDataLength;

                /// <summary>
                /// Function that reads long data capacity field.
                /// </summary>
                public Func<ulong, int> ReadCapacity;

                /// <summary>
                /// Offset of buffer field.
                /// </summary>
                public int BufferOffset;

                /// <summary>
                /// Array length of buffer field.
                /// </summary>
                public int BufferLength;

                /// <summary>
                /// Offset of pointer field.
                /// </summary>
                public int PointerOffset;

                /// <summary>
                /// Number of bytes needed to store a character.
                /// </summary>
                public int CharSize;

                /// <summary>
                /// Code type of std::basic_string.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// The length of the string
            /// </summary>
            private int length;

            /// <summary>
            /// The reserved number of characters in the string buffer
            /// </summary>
            private int reserved;

            /// <summary>
            /// The text inside the string buffer
            /// </summary>
            private UserMember<string> text;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClangLibCpp"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public ClangLibCpp(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                ulong stringAddress = data.Process.ReadPointer(address + (uint)data.PointerOffset);
                bool localData = stringAddress == 0;
                if (localData)
                    stringAddress = address + (uint)data.BufferOffset;
                bool bigEndian = true; // TODO:
                if (localData)
                {
                    length = data.ReadShortDataLength(address);
                    if (bigEndian)
                        length = length >> 1;
                }
                else
                    length = data.ReadLongDataLength(address);
                if (localData)
                    reserved = data.BufferLength;
                else
                    reserved = data.ReadCapacity(address);
                text = UserMember.Create(() => data.Process.ReadString(stringAddress, data.CharSize, length));
            }

            /// <summary>
            /// Gets the string length.
            /// </summary>
            public int Length => length;

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            public int Reserved => reserved;

            /// <summary>
            /// Gets the string text.
            /// </summary>
            public string Text => text.Value;

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // __r_
                // | __value_
                //   | __l
                //     | __cap_
                //     | __data_
                //     | __size_
                //   | __r
                //     | __words
                //   | __s
                //     | __data_
                //     | __lx
                //     | __size_
                CodeType __r_, __value_, __l, __cap_, __data_, __size_, __r, __words, __s, __data_2, __lx, __size_2;

                if (!codeType.GetFieldTypes().TryGetValue("__r_", out __r_))
                    return null;
                if (!__r_.GetFieldTypes().TryGetValue("__value_", out __value_))
                    return null;
                if (!__value_.GetFieldTypes().TryGetValue("__l", out __l))
                    return null;
                if (!__l.GetFieldTypes().TryGetValue("__cap_", out __cap_) || !__l.GetFieldTypes().TryGetValue("__data_", out __data_) || !__l.GetFieldTypes().TryGetValue("__size_", out __size_))
                    return null;
                if (!__value_.GetFieldTypes().TryGetValue("__r", out __r))
                    return null;
                if (!__r.GetFieldTypes().TryGetValue("__words", out __words))
                    return null;
                if (!__value_.GetFieldTypes().TryGetValue("__s", out __s))
                    return null;
                if (!__s.GetFieldTypes().TryGetValue("__data_", out __data_2) || !__s.GetFieldTypes().TryGetValue("__lx", out __lx) || !__s.GetFieldTypes().TryGetValue("__size_", out __size_2))
                    return null;
                return new ExtractedData
                {
                    BufferLength = (int)(__data_2.Size / __data_2.ElementType.Size),
                    BufferOffset = codeType.GetFieldOffset("__r_") + __r_.GetFieldOffset("__value_") + __value_.GetFieldOffset("__s") + __s.GetFieldOffset("__data_"),
                    CharSize = (int)__data_2.ElementType.Size,
                    PointerOffset = codeType.GetFieldOffset("__r_") + __r_.GetFieldOffset("__value_") + __value_.GetFieldOffset("__l") + __l.GetFieldOffset("__data_"),
                    ReadCapacity = codeType.Module.Process.GetReadInt(codeType, "__r_.__value_.__l.__cap_"),
                    ReadLongDataLength = codeType.Module.Process.GetReadInt(codeType, "__r_.__value_.__l.__size_"),
                    ReadShortDataLength = codeType.Module.Process.GetReadInt(codeType, "__r_.__value_.__s.__size_"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IBasicString> typeSelector = new TypeSelector<IBasicString>(new[]
        {
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp6), LibStdCpp6.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp6_NoAbi), LibStdCpp6_NoAbi.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(ClangLibCpp), ClangLibCpp.VerifyCodeType),
        });

        /// <summary>
        /// Verifies that type user type can work with the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if user type can work with the specified code type; <c>false</c> otherwise</returns>
        public static bool VerifyCodeType(CodeType codeType)
        {
            return typeSelector.VerifyCodeType(codeType);
        }

        /// <summary>
        /// The instance used to read variable data
        /// </summary>
        private IBasicString instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="basic_string"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::basic_string</exception>
        public basic_string(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::basic_string");
            }
        }

        /// <summary>
        /// Gets the string length.
        /// </summary>
        public int Length
        {
            get
            {
                return instance.Length;
            }
        }

        /// <summary>
        /// Gets the reserved space in buffer (number of characters).
        /// </summary>
        public int Reserved
        {
            get
            {
                return instance.Reserved;
            }
        }

        /// <summary>
        /// Gets the string text.
        /// </summary>
        public string Text
        {
            get
            {
                return instance.Text;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as basic_string;

            return other != null && Text == other.Text;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
}
