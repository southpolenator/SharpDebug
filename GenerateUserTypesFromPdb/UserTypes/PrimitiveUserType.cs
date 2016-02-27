using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb.UserTypes
{

    class PrimitiveUserType : UserType
    {
        private string typeName;

        public PrimitiveUserType(string typeName, IDiaSymbol diaSymbol)
            : base(diaSymbol, null, null)
        {
            this.typeName = typeName;
        }

        public override string ClassName
        {
            get
            {
                return typeName;
            }
        }

        public override string FullClassName
        {
            get
            {
                return typeName;
            }
        }
    }
}
