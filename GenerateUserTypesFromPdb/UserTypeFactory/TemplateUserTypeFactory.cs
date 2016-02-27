using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb.UserTypes
{
    class TemplateUserTypeFactory : UserTypeFactory
    {
        public TemplateUserTypeFactory(UserTypeFactory factory, TemplateUserType templateType)
            : base(factory)
        {
            TemplateType = templateType;
            OriginalFactory = factory;
        }

        public TemplateUserType TemplateType { get; private set; }

        public UserTypeFactory OriginalFactory { get; private set; }

        internal override bool GetUserType(IDiaSymbol type, out UserType userType)
        {
            string argumentName;
            string typeString = TypeToString.GetTypeString(type);

            if (typeString.Contains("Auto"))
            {
            }

            if (TryGetArgument(typeString, out argumentName))
            {
                //#fixme invesitage this
                userType = new PrimitiveUserType(argumentName, type);
                return true;
            }

            return base.GetUserType(type, out userType);
        }

        internal override bool TryGetUserType(string typeString, out UserType userType)
        {
            string argumentName;

            if (TryGetArgument(typeString, out argumentName))
            {
                //#fixme invesitage this
                userType = new PrimitiveUserType(argumentName, null);
                return true;
            }

            return base.TryGetUserType(typeString, out userType);
        }

        private bool TryGetArgument(string typeString, out string argumentName)
        {
            if (TemplateType.TryGetArgument(typeString, out argumentName))
            {
                return true;
            }

            if (typeString == "wchar_t")
            {
                if (TemplateType.TryGetArgument("unsigned short", out argumentName))
                    return true;
            }
            else if (typeString == "unsigned short")
            {
                if (TemplateType.TryGetArgument("whcar_t", out argumentName))
                    return true;
            }
            else if (typeString == "unsigned long long")
            {
                if (TemplateType.TryGetArgument("unsigned __int64", out argumentName))
                    return true;
            }
            else if (typeString == "unsigned __int64")
            {
                if (TemplateType.TryGetArgument("unsigned long long", out argumentName))
                    return true;
            }
            else if (typeString == "long long")
            {
                if (TemplateType.TryGetArgument("__int64", out argumentName))
                    return true;
            }
            else if (typeString == "__int64")
            {
                if (TemplateType.TryGetArgument("long long", out argumentName))
                    return true;
            }

            return false;
        }
    }
}
