using CsScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsScripts.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::wstring
    /// </summary>
    public class wstring
    {
        private UserMember<Variable> value;
        private UserMember<int> length;
        private UserMember<int> reserved;
        private UserMember<string> text;

        public wstring(Variable variable)
        {
            value = UserMember.Create(() => variable.GetField("_MyPair").GetField("_Myval2"));
            length = UserMember.Create(() => (int)Value.GetField("_Mysize"));
            reserved = UserMember.Create(() => (int)Value.GetField("_Myres"));
            text = UserMember.Create(GetText);
        }

        public int Length
        {
            get
            {
                return length.Value;
            }
        }

        public int Reserved
        {
            get
            {
                return reserved.Value;
            }
        }

        public string Text
        {
            get
            {
                return text.Value;
            }
        }

        private Variable Value
        {
            get
            {
                return value.Value;
            }
        }

        public override string ToString()
        {
            return Text;
        }

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
    }
}
