namespace Dynamo.PackageManager
{
    public class PackageManagerResult
    {
        public PackageManagerResult(string error, bool success)
        {
            Error = error;
            Success = success;
        }

        public static PackageManagerResult Succeeded()
        {
            return new PackageManagerResult("", true);
        }

        public static PackageManagerResult Failed(string error)
        {
            return new PackageManagerResult(error, false);
        }

        public string Error { get; set; }
        public bool Success { get; set; }
    }
}