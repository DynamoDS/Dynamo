namespace Dynamo.PackageManager
{
    interface ICompressor
    {
        IFileInfo Zip(string directoryPath);
    }
}