namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An abstract IFileCompressor for mocking purposes
    /// </summary>
    interface IFileCompressor
    {
        IFileInfo Zip(string directoryPath);
    }
}