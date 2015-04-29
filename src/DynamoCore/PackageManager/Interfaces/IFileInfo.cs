namespace Dynamo.PackageManager
{
    interface IFileInfo
    {
        string Name { get; }
        long Length { get; }
    }
}