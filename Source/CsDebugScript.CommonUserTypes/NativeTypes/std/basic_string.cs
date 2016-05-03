using System;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::basic_string
    /// </summary>
    public class basic_string
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
        /// Initializes a new instance of the <see cref="basic_string"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public basic_string(Variable variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            // Initialize members
            value = UserMember.Create(() => variable.GetField("_Mypair").GetField("_Myval2"));
            length = UserMember.Create(() => (int)Value.GetField("_Mysize"));
            reserved = UserMember.Create(() => (int)Value.GetField("_Myres"));
            text = UserMember.Create(GetText);
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Gets the string text.
        /// </summary>
        private string GetText()
        {
            var bx = Value.GetField("_Bx");
            var buf = bx.GetField("_Buf");

            if (buf.GetArrayLength() >= Length)
            {
                return buf.ToString();
            }
            else
            {
                return bx.GetField("_Ptr").ToString();
            }
        }

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private static bool VerifyCodeType(CodeType codeType)
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
}
