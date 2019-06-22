using System;

namespace CsDebugScript.Tests
{
    public class SkipTestException : Exception
    {
        public SkipTestException(string reason)
            : base(reason)
        {
        }
    }
}
