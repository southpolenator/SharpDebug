using System;

namespace SharpDebug.Tests
{
    public class SkipTestException : Exception
    {
        public SkipTestException(string reason)
            : base(reason)
        {
        }
    }
}
