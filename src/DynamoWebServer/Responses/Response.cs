namespace DynamoWebServer.Responses
{
    public enum ResponseStatus
    {
        Success,
        Error
    }

    public abstract class Response
    {
        public ResponseStatus Status { get; set; }
    }
}
