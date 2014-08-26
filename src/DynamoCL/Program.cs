using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Models;

namespace DynamoCL
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = DynamoModel.Start();
            Console.WriteLine(model.Version);
        }
    }
}
