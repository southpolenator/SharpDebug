using Xunit;
using Xunit.Sdk;

namespace CsDebugScript.Tests.Utils
{
    [XunitTestCaseDiscoverer("CsDebugScript.Tests.Utils.SkippableTheoryDiscoverer", "CsDebugScript.Tests")]
    public class SkippableTheoryAttribute : TheoryAttribute
    {
    }
}
