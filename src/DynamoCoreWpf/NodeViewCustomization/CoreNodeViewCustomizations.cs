using System.Reflection;

namespace Dynamo.Wpf
{
    public class CoreNodeViewCustomizations : AssemblyNodeViewCustomizations
    {
        public CoreNodeViewCustomizations() : base(Assembly.GetExecutingAssembly())
        {
        }
    }
}