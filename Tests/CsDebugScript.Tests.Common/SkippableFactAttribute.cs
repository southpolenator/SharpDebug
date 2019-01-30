using Xunit;
using Xunit.Sdk;

namespace CsDebugScript.Tests
{
    [XunitTestCaseDiscoverer("CsDebugScript.Tests.SkippableFactDiscoverer", "CsDebugScript.Tests")]
    public class SkippableFactAttribute : FactAttribute
    {
        public string SkipOnFailurePropertyName { get; set; }
    }
}
