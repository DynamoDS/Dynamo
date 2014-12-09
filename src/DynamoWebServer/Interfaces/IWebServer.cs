namespace DynamoWebServer
{
    public enum ResponceStatuses
    {
        Success,
        Error
    }

    public abstract class Response
    {
        public ResponceStatuses Status { get; private set; }

        public Response(ResponceStatuses status)
        {
            Status = status;
        }

        public Response() 
        {
            Status = ResponceStatuses.Success;
        }
    }

    public interface IWebServer
    {
        void Start();
        void SendResponse(Response response, string sessionId);
        void ExecuteMessageFromSocket(string message, string sessionId, bool enqueue);
        void ExecuteFileFromSocket(byte[] file, string sessionId, bool enqueue);
    }
}
