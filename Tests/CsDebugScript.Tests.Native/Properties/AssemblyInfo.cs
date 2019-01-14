using Xunit;

#if DEBUG
[assembly: CollectionBehavior(DisableTestParallelization = false)] // This enables running tests in Visual Studio
#else
[assembly: CollectionBehavior(DisableTestParallelization = true)]
#endif
