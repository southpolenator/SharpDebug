using CsDebugScript.Exceptions;
using System;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::basic_string
    /// </summary>
    [UserType(TypeName = "std::basic_string<>", CodeTypeVerification = nameof(basic_string.VerifyCodeType))]
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
        public class VisualStudio : IBasicString
        {
            /// <summary>
            /// The internal value field inside the std::basic_string
            /// </summary>
            private UserMember<Variable> value;

            /// <summary>
            /// The length of the string
            /// </summary>
            private UserMember<int> length;

            /// <summary>
            /// The reserved number of characters in the string buffer
            /// </summary>
            private UserMember<int> reserved;

            /// <summary>
            /// The text inside the string buffer
            /// </summary>
            private UserMember<string> text;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="value">The value variable.</param>
            public VisualStudio(UserMember<Variable> value)
            {
                this.value = value;
                length = UserMember.Create(() => (int)Value.GetField("_Mysize"));
                reserved = UserMember.Create(() => (int)Value.GetField("_Myres"));
                text = UserMember.Create(() => GetText());
            }

            /// <summary>
            /// Gets the string length.
            /// </summary>
            public int Length
            {
                get
                {
                    return length.Value;
                }
            }

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            public int Reserved
            {
                get
                {
                    return reserved.Value;
                }
            }

            /// <summary>
            /// Gets the string text.
            /// </summary>
            public string Text
            {
                get
                {
                    return text.Value;
                }
            }

            /// <summary>
            /// Gets the internal value field inside the std::basic_string
            /// </summary>
            private Variable Value
            {
                get
                {
                    return value.Value;
                }
            }

            /// <summary>
            /// Gets the string text.
            /// </summary>
            private string GetText()
            {
                var bx = Value.GetField("_Bx");
                var buf = bx.GetField("_Buf");

                if (Length >= buf.GetArrayLength())
                {
                    return bx.GetField("_Ptr").ToString();
                }
                else
                {
                    return buf.ToString();
                }
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::basic_string
        /// </summary>
        public class VisualStudio2013 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2013"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public VisualStudio2013(Variable variable)
                : base(UserMember.Create(() => variable))
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
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
                    return false;

                var _BxFields = _Bx.GetFieldTypes();

                if (!_BxFields.TryGetValue("_Buf", out _Buf) || !_BxFields.TryGetValue("_Ptr", out _Ptr))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::basic_string
        /// </summary>
        public class VisualStudio2015 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2015"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public VisualStudio2015(Variable variable)
                : base(UserMember.Create(() => variable.GetField("_Mypair").GetField("_Myval2")))
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
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
                    return false;
                if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                    return false;

                var _Myval2Fields = _Myval2.GetFieldTypes();

                if (!_Myval2Fields.TryGetValue("_Bx", out _Bx) || !_Myval2Fields.TryGetValue("_Mysize", out _Mysize) || !_Myval2Fields.TryGetValue("_Myres", out _Myres))
                    return false;

                var _BxFields = _Bx.GetFieldTypes();

                if (!_BxFields.TryGetValue("_Buf", out _Buf) || !_BxFields.TryGetValue("_Ptr", out _Ptr))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string
        /// </summary>
        public class LibStdCpp6 : IBasicString
        {
            /// <summary>
            /// The text inside the string buffer
            /// </summary>
            private UserMember<Variable> text;

            /// <summary>
            /// The length of the string
            /// </summary>
            private UserMember<int> length;

            /// <summary>
            /// The local buffer that can be used for the string
            /// </summary>
            private UserMember<Variable> localBuffer;

            /// <summary>
            /// The string buffer capacity
            /// </summary>
            private UserMember<int> capacity;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public LibStdCpp6(Variable variable)
            {
                length = UserMember.Create(() => (int)variable.GetField("_M_string_length"));
                text = UserMember.Create(() => variable.GetField("_M_dataplus").GetField("_M_p"));
                localBuffer = UserMember.Create(() => variable.GetField("_M_local_buf"));
                capacity = UserMember.Create(() => (int)variable.GetField("_M_allocated_capacity"));
            }

            /// <summary>
            /// Gets the string length.
            /// </summary>
            public int Length
            {
                get
                {
                    return length.Value;
                }
            }

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            public int Reserved
            {
                get
                {
                    if (IsLocalData)
                    {
                        return (int)(15 / text.Value.GetCodeType().ElementType.Size);
                    }
                    else
                    {
                        return capacity.Value;
                    }
                }
            }

            /// <summary>
            /// Gets the string text.
            /// </summary>
            public string Text
            {
                get
                {
                    return text.Value.ToString();
                }
            }

            /// <summary>
            /// Gets a value indicating whether basic_string is using local data.
            /// </summary>
            /// <value>
            /// <c>true</c> if basic_string is using local data; otherwise, <c>false</c>.
            /// </value>
            private bool IsLocalData
            {
                get
                {
                    return localBuffer.Value.GetPointerAddress() == text.Value.GetPointerAddress();
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
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
                    return false;
                if (!codeType.GetFieldTypes().TryGetValue("_M_string_length", out _M_string_length))
                    return false;
                if (!_M_dataplus.GetFieldTypes().TryGetValue("_M_p", out _M_p))
                    return false;

                // These should be part of unnamed union
                if (!codeType.GetFieldTypes().TryGetValue("_M_local_buf", out _M_local_buf))
                    return false;
                if (!codeType.GetFieldTypes().TryGetValue("_M_allocated_capacity", out _M_allocated_capacity))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string with _GLIBCXX_USE_CXX11_ABI not defined.
        /// </summary>
        public class LibStdCpp6_NoAbi : IBasicString
        {
            /// <summary>
            /// The text inside the string buffer
            /// </summary>
            private UserMember<Variable> text;

            /// <summary>
            /// The length of the string
            /// </summary>
            private UserMember<int> length;

            /// <summary>
            /// The string buffer capacity
            /// </summary>
            private UserMember<int> capacity;

            /// <summary>
            /// Header that holds extra info about string
            /// </summary>
            private UserMember<Variable> header;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6_NoAbi"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public LibStdCpp6_NoAbi(Variable variable)
            {
                CodeType codeType = variable.GetCodeType();
                CodeType _Rep = CodeType.Create($"{codeType.Name}::_Rep", codeType.Module);
                header = UserMember.Create(() => text.Value.AdjustPointer(-(int)_Rep.Size).CastAs(_Rep));
                length = UserMember.Create(() => (int)header.Value.GetField("_M_length"));
                text = UserMember.Create(() => variable.GetField("_M_dataplus").GetField("_M_p"));
                capacity = UserMember.Create(() => (int)header.Value.GetField("_M_capacity"));
            }

            /// <summary>
            /// Gets the string length.
            /// </summary>
            public int Length
            {
                get
                {
                    return length.Value;
                }
            }

            /// <summary>
            /// Gets the reserved space in buffer (number of characters).
            /// </summary>
            public int Reserved
            {
                get
                {
                    return capacity.Value;
                }
            }

            /// <summary>
            /// Gets the string text.
            /// </summary>
            public string Text
            {
                get
                {
                    return text.Value.ToString();
                }
            }

            /// <summary>
            /// Gets a value indicating whether basic_string is using local data.
            /// </summary>
            /// <value>
            /// <c>true</c> if basic_string is using local data; otherwise, <c>false</c>.
            /// </value>
            private bool IsLocalData
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
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
                    return false;
                }

                if (!codeType.GetFieldTypes().TryGetValue("_M_dataplus", out _M_dataplus))
                    return false;
                if (!_M_dataplus.GetFieldTypes().TryGetValue("_M_p", out _M_p))
                    return false;
                if (!_Rep.GetFieldTypes().TryGetValue("_M_length", out _M_length))
                    return false;
                if (!_Rep.GetFieldTypes().TryGetValue("_M_capacity", out _M_capacity))
                    return false;
                if (!_Rep.GetFieldTypes().TryGetValue("_M_refcount", out _M_refcount))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IBasicString> typeSelector = new TypeSelector<IBasicString>(new[]
        {
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(LibStdCpp6), LibStdCpp6.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(LibStdCpp6_NoAbi), LibStdCpp6_NoAbi.VerifyCodeType),
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
