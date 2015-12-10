using CsScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsScriptManaged.SymbolProviders
{
    public interface ISymbolProvider
    {
        SymTag GetTypeTag(Module module, uint typeId);
        string[] GetTypeFieldNames(Module module, uint typeId);
        string GetTypeName(Module module, uint typeId);
        uint GetTypeElementTypeId(Module module, uint typeId);
        uint GetTypeSize(Module module, uint typeId);
    }
}
