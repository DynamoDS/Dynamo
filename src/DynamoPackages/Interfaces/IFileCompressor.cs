namespace Dynamo.PackageManager.Interfaces
{
    /// <summary>
    ///     An abstract IFileCompressor for mocking purposes
    /// </summary>
    public interface IFileCompressor
    {
        IFileInfo Zip(IDirectoryInfo directory);
    }
}