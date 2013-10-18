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
            Console.WriteLine(client.GetData(3));
            Console.Read();
            client.OpenFile(@"C:\Autodesk\Dynamo\Core\samples\Mass with 2 Curves.rfa");
            Console.WriteLine( client.OpenFile(@"C:\Autodesk\Dynamo\Core\samples\Mass with 2 Curves.rfa") );
            Console.WriteLine(client.GetData(3));
        }
    }
} 
