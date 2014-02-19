using Dynamo.Units;

namespace DynamoUnits
{
    public interface IUnitsManager
    {
        DynamoLengthUnit HostApplicationInternalLengthUnit { get; set; }
        DynamoAreaUnit HostApplicationInternalAreaUnit { get; set; }
        DynamoVolumeUnit HostApplicationInternalVolumeUnit { get; set; }
        DynamoLengthUnit LengthUnit { get; set; }
        DynamoAreaUnit AreaUnit { get; set; }
        DynamoVolumeUnit VolumeUnit { get; set; }
        double UiLengthConversion { get; }
        double UiAreaConversion { get; }
        double UiVolumeConversion { get; }
        string NumberFormat { get; set; }
    }
}
