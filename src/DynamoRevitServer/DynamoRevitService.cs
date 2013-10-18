using System.Threading.Tasks;
using Dynamo.Utilities;
using RevitServices.Threading;

namespace Dynamo.Revit.Server
{

    /// <summary>
    /// A mixture of Revit and Dynamo commands for prototyping purposes.
    /// Note that this entire interface could be done async, these are all bools 
    /// that return true for now.
    /// </summary>
    public class DynamoRevitService : IDynamoRevitService
    {
        public DynamoController_Revit DynamoRevitApplication { get; set; }

        // a debugging method
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }
        
        public bool OpenFile(string path)
        {
            IdlePromise.ExecuteOnIdleAsync( () => dynRevitSettings.Revit.OpenAndActivateDocument(path) );
            return true;
        }
        
        public bool OpenDynamoWorkspace(string path)
        {
            Task.Run(() => dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(path));
            return true;
        }

        public bool RunDynamoExpression()
        {
            Task.Run(() => dynSettings.Controller.RunExpression());
            return true;
        }
    }
}
