namespace Dynamo.PackageManager.Interfaces
{
    public interface IFileInfo
    {
        string Name { get; }
        long Length { get; }
    }
}