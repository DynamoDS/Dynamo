namespace Dynamo.PackageManager
{
    interface IFileCompressor
    {
        IFileInfo Zip(string directoryPath);
    }
}