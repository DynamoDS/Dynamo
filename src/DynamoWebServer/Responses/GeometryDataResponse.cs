using DynamoWebServer.Messages;

namespace DynamoWebServer.Responses
{
    class GeometryDataResponse : Response
    {
        public GeometryData GeometryData { get; private set; }

        public GeometryDataResponse(GeometryData geometryData)
        {
            GeometryData = geometryData;
        }
    }
}
