namespace DynamoWebServer
{
    public enum ResponceStatuses
    {
        Success,
        Error
    }

    public abstract class Response
    {
        public ResponceStatuses Status { get; set; }
    }

    public interface IWebServer
    {
        void Start();
        void SendResponse(Response response, string sessionId);
        void ExecuteMessageFromSocket(string message, string sessionId);
    }
}
