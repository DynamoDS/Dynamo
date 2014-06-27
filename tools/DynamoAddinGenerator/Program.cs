using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Autodesk.RevitAddIns;

namespace DynamoAddinGenerator
{
    class Program
    {
        private static string debugPath = string.Empty;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                // First argument should be the debug assembly path
                debugPath = args[0];
            }

            var allProducts = RevitProductUtility.GetAllInstalledRevitProducts();
            var prodColl = new RevitProductCollection(allProducts.Select(x=>new DynamoRevitProduct(x)));
            if (!prodColl.Products.Any())
            {
                Console.WriteLine("There were no Revit products found.");
                return;
            }

            var installs = DynamoInstallCollection.FindDynamoInstalls(debugPath);
            var dynamoColl = new DynamoInstallCollection(installs);
            if (!dynamoColl.Installs.Any())
            {
                Console.WriteLine("There were no Dynamo installations found.");
                return;
            }

            DeleteExistingAddins(prodColl);

            GenerateAddins(prodColl, dynamoColl);
        }

        /// <summary>
        /// Deletes all existing Dynamo addins.
        /// This method will delete addins like Dynamo.addin and 
        /// DynamoVersionSelector.addin
        /// </summary>
        internal static void DeleteExistingAddins(IRevitProductCollection products)
        {
            foreach (var product in products.Products)
            {
                try
                {
                    Console.WriteLine("Deleting addins in {0}", product.AddinsFolder);

                    var dynamoAddin = Path.Combine(product.AddinsFolder, "Dynamo.addin");
                    if (File.Exists(dynamoAddin))
                    {
                        Console.WriteLine("Deleting addin {0}", dynamoAddin);
                        File.Delete(dynamoAddin);
                    }

                    dynamoAddin = Path.Combine(product.AddinsFolder, "DynamoRevitVersionSelector.addin");
                    if (File.Exists(dynamoAddin))
                    {
                        Console.WriteLine("Deleting addin {0}", dynamoAddin);
                        File.Delete(dynamoAddin);
                    }

                    dynamoAddin = Path.Combine(product.AddinsFolder, "DynamoVersionSelector.addin");
                    if (File.Exists(dynamoAddin))
                    {
                        Console.WriteLine("Deleting addin {0}", dynamoAddin);
                        File.Delete(dynamoAddin);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There as an error deleting an addin {0}", product.AddinsFolder);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Generate new addin files for all applicable
        /// versions of Revit.
        /// </summary>
        /// <param name="products">A collection of revit installs.</param>
        /// <param name="dynamos">A collection of dynamo installs.</param>
        internal static void GenerateAddins(IRevitProductCollection products, IDynamoInstallCollection dynamos)
        {
            foreach (var prod in products.Products)
            {
                Console.WriteLine("Generating addins in {0}", prod.AddinsFolder);

                var addinData = new DynamoAddinData(prod, dynamos.GetLatest());
                GenerateDynamoAddin(addinData);
            }
        }

        /// <summary>
        /// Generate a Dynamo.addin file.
        /// </summary>
        /// <param name="data">An object containing data about the addin.</param>
        internal static void GenerateDynamoAddin(IDynamoAddinData data)
        {
            Console.WriteLine("Generating addin {0}", data.AddinPath);

            using (var tw = new StreamWriter(data.AddinPath, false))
            {
                var addin = String.Format(
                    "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>\n" +
                    "<RevitAddIns>\n" +
                    "<AddIn Type=\"Application\">\n" +
                    "<Name>Dynamo For Revit</Name>\n" +
                    "<Assembly>\"{0}\"</Assembly>\n" +
                    "<AddInId>{1}</AddInId>\n" +
                    "<FullClassName>{2}</FullClassName>\n" +
                    "<VendorId>Dynamo</VendorId>\n" +
                    "<VendorDescription>Dynamo</VendorDescription>\n" +
                    "</AddIn>\n" +
                    "</RevitAddIns>",
                    data.AssemblyPath, data.Id, data.ClassName
                    );

                tw.Write(addin);
                tw.Flush();
            }

            // Grant everyone permissions to delete this addin.
            //http://stackoverflow.com/questions/5298905/add-everyone-privilege-to-folder-using-c-net/5398398#5398398
            var sec = File.GetAccessControl(data.AddinPath);
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(data.AddinPath, sec);
        }
    }
}
