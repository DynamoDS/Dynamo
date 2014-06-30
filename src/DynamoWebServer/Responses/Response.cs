namespace DynamoWebServer.Responses
{
    public enum ResponceStatuses
    {
        Success,
        Error
    }

    public abstract class Response
    {
        public ResponceStatuses Status { get; set; }

        public abstract string GetResponse();
    }
}
