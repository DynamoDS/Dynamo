namespace DynamoWebServer.Responses
{
    public class ContentResponse : Response
    {
        public string Message { get; private set; }

        public ContentResponse(string message)
        {
            Message = message;
        }
    }
}
