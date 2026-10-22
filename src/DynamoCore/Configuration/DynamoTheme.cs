namespace Dynamo.Configuration
{
    /// <summary>
    /// The set of visual themes Dynamo can render its interface with.
    /// </summary>
    /// <remarks>
    /// The selected theme is persisted in <see cref="PreferenceSettings.Theme"/> and is
    /// resolved to a resource dictionary folder once during startup. Changing the value at
    /// runtime has no effect until Dynamo is restarted.
    /// </remarks>
    public enum DynamoTheme
    {
        /// <summary>
        /// The default dark theme shipped since Dynamo 2.13.
        /// </summary>
        Dark,

        /// <summary>
        /// A light theme intended for bright working environments and for users who found
        /// the dark node palette hard to read.
        /// </summary>
        Light
    }
}
