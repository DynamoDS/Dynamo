using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamoRevitClient.ServiceReference1;

namespace Dynamo.Revit.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new DynamoRevitServiceClient();
            Console.WriteLine(client.OpenDynamoWorkspace(@"C:\Users\boyerp\Dropbox\Github\Autodesk\Dynamo\test\revit\ReferencePoint.dyn"));
            Console.WriteLine(client.RunDynamoExpression());
        }
    }
} 
