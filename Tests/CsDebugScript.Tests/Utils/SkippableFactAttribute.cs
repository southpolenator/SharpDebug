using Xunit;
using Xunit.Sdk;

namespace CsDebugScript.Tests.Utils
{
    [XunitTestCaseDiscoverer("CsDebugScript.Tests.Utils.SkippableFactDiscoverer", "CsDebugScript.Tests")]
    public class SkippableFactAttribute : FactAttribute
    {
        public string SkipOnFailurePropertyName { get; set; }
    }
}
