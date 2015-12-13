using System;
using System.Text.RegularExpressions;

namespace CsScripts.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::wstring
    /// </summary>
    public class wstring
    {
        /// <summary>
        /// The code type regular expression validator
        /// </summary>
        private static Regex codeTypeRegexValidator = new Regex(@"std::basic_string\s*<\s*wchar_t\s*,\s*std::char_traits\s*<\s*wchar_t\s*>\s*,\s*.*>", RegexOptions.Compiled);

        /// <summary>
        /// The internal value field inside the std::wstring
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
        /// Initializes a new instance of the <see cref="wstring"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public wstring(Variable variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            // Initialize members
            value = UserMember.Create(() => variable.GetField("_MyPair").GetField("_Myval2"));
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
        /// Gets the internal value field inside the std::wstring
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
            string codeTypeName = codeType.Name;
            var matches = codeTypeRegexValidator.Matches(codeTypeName);

            if (matches.Count != 1)
            {
                return false;
            }

            var match = matches[0];

            return match.Length == codeTypeName.Length;
        }
    }
}
