namespace DynamoPackagesAnalyzer.Models
{
    /// <summary>
    /// dynamopackages.com response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Response<T>
    {
        public double Timestamp;
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Content { get; set; }

        public Response()
        {
            Timestamp = 0;
        }
    }

    internal class Response : Response<object>
    {
    }
}
