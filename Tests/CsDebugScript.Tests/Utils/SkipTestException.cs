using System;

namespace CsDebugScript.Tests.Utils
{
    public class SkipTestException : Exception
    {
        public SkipTestException(string reason)
            : base(reason)
        {
        }
    }
}
