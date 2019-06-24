using Xunit;
using Xunit.Sdk;

namespace SharpDebug.Tests
{
    [XunitTestCaseDiscoverer("SharpDebug.Tests.SkippableTheoryDiscoverer", "SharpDebug.Tests")]
    public class SkippableTheoryAttribute : TheoryAttribute
    {
    }
}
