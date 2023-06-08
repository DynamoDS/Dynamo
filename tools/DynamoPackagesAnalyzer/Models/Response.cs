namespace DynamoPackagesAnalyzer.Models
{
    /// <summary>
    /// dynamopackages.com response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Response<T>
    {
        internal double Timestamp;
        internal bool Success { get; set; }
        internal string Message { get; set; }
        internal T Content { get; set; }

        internal Response()
        {
            Timestamp = 0;
        }
    }

    internal class Response : Response<object>
    {
    }
}
