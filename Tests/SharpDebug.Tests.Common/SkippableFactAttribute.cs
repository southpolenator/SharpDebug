using Xunit;
using Xunit.Sdk;

namespace SharpDebug.Tests
{
    [XunitTestCaseDiscoverer("SharpDebug.Tests.SkippableFactDiscoverer", "SharpDebug.Tests")]
    public class SkippableFactAttribute : FactAttribute
    {
        public string SkipOnFailurePropertyName { get; set; }
    }
}
