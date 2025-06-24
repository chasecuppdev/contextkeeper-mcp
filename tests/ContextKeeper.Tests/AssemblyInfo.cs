using Xunit;

// Disable parallel test execution at the assembly level to prevent conflicts
// between tests that modify Environment.CurrentDirectory
[assembly: CollectionBehavior(DisableTestParallelization = true)]