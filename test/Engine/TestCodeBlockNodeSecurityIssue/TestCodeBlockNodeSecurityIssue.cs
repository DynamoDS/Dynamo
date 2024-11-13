namespace TestCodeBlockNodeSecurityIssue
{
    /// <summary>
    /// This class is used in
    /// Dynamo.Tests.CodeBlockNodeTests.ImportStatementInCodeBlock_DoesNotLoadAssemblyIntoProcess
    /// to test a security issue in CodeBlockNodes where an import statement
    /// would unexpectedly load the assembly into the process.
    /// This should be prevented as import statements are not permitted in CBNs
    /// and therefore should not be loaded into the process in the first place.
    /// </summary>
    public class TestCodeBlockNodeSecurityIssue
    {

    }
}
