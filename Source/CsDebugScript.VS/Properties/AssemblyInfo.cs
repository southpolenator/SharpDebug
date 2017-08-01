using Microsoft.VisualStudio.Shell;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CsDebugScript.VS")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("CsDebugScript.VS")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]


// After updating Roslyn, these are needed to be up to date or VisualStudio won't allow assembly loading
[assembly: ProvideBindingRedirection(AssemblyName = "System.IO.FileSystem", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.2.0", NewVersion = "4.0.2.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.IO.FileSystem.Primitives", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.2.0", NewVersion = "4.0.2.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Collections.Immutable", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "1.2.1.0", NewVersion = "1.2.1.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Diagnostics.StackTrace", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.3.0", NewVersion = "4.0.3.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Security.Cryptography.Algorithms", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.1.0.0", NewVersion = "4.1.0.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Security.Cryptography.Primitives", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.1.0", NewVersion = "4.0.1.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Xml.XPath.XDocument", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.2.0", NewVersion = "4.0.2.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Diagnostics.FileVersionInfo", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.1.0", NewVersion = "4.0.1.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Threading.Thread", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.1.0", NewVersion = "4.0.1.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Reflection.Metadata", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "1.4.1.0", NewVersion = "1.4.1.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.Xml.ReaderWriter", PublicKeyToken = "b03f5f7f11d50a3a",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.1.0.0", NewVersion = "4.1.0.0")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.IO.Compression", PublicKeyToken = "b77a5c561934e089",
    OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.1.2.0", NewVersion = "4.1.2.0")]
