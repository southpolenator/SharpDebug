using Xunit;
using Xunit.Sdk;

namespace CsDebugScript.Tests
{
    [XunitTestCaseDiscoverer("CsDebugScript.Tests.SkippableTheoryDiscoverer", "CsDebugScript.Tests")]
    public class SkippableTheoryAttribute : TheoryAttribute
    {
    }
}
