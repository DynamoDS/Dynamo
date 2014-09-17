#region

#endregion

namespace Dynamo.DSEngine
{
    /// <summary>
    ///     The type of a function.
    /// </summary>
    public enum FunctionType
    {
        GenericFunction,
        Constructor,
        StaticMethod,
        InstanceMethod,
        StaticProperty,
        InstanceProperty
    }
}
