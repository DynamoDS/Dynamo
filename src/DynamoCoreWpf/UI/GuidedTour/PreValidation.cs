namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will store the information of the actions/methods that should be executed before showing a Popup
    /// </summary>
    internal class PreValidation
    {
        /// <summary>
        /// Default parameterless constructor used when deserializing
        /// </summary>
        public PreValidation() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="data"></param>
        public PreValidation(PreValidation data)
        {
            ReturnType = data.ReturnType;
            FuncName = data.FuncName;
            ExpectedValue = data.ExpectedValue;
            ControlType = data.ControlType;
        }
        /// <summary>
        /// string that will contain the return data type
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// string that will contain the function name that will be executed(the function might exist in the static class GuidesValidationMethods
        /// </summary>
        public string FuncName { get; set; }

        /// <summary>
        /// string that will contain the the expected value after executing the function (FuncName) if the function result and this value match the popup will be shown
        /// </summary>
        public string ExpectedValue { get; set; }

        /// <summary>
        /// string that will contain the Type of control or feature for example the if the Popup is open or not
        /// </summary>
        public string ControlType { get; set; }
    }
}