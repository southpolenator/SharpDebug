using GenerateUserTypesFromPdb.UserTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    internal static class GlobalCache
    {
        internal static ConcurrentDictionary<string, Symbol> DiaSymbolsByName = new ConcurrentDictionary<string, Symbol>();
        internal static ConcurrentDictionary<string, UserType> UserTypesBySymbolName = new ConcurrentDictionary<string, UserType>();
        internal static ConcurrentDictionary<string, bool> InstantiableTemplateUserTypes = new ConcurrentDictionary<string, bool>();
    };
}
