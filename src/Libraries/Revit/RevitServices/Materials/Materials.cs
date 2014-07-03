using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace RevitServices.Materials
{
    public class MaterialsManager
    {
        private static MaterialsManager instance;

        public ElementId DynamoMaterialId { get; internal set; }
        public ElementId DynamoGStyleId { get; internal set; }

        public static MaterialsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MaterialsManager();
                }
                return instance;
            }
        }

        private MaterialsManager()
        {
            FindorCreateDynamoMaterial();
            FindDynamoGraphicsStyle();
        }

        public static void Reset()
        {
            instance = null;
            instance = new MaterialsManager();
        }

        private void FindorCreateDynamoMaterial()
        {
            Dictionary<string, Material> materials
            = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument)
              .OfClass(typeof(Material))
              .Cast<Material>()
              .ToDictionary<Material, string>(
                e => e.Name);

            // First try to get or create the Dynamo material
            if (materials.ContainsKey("Dynamo"))
            {
                DynamoMaterialId = materials["Dynamo"].Id;
                return;
            }

            Threading.IdlePromise.ExecuteOnIdleAsync(
            () =>
            {
                TransactionManager.Instance.EnsureInTransaction(
                    DocumentManager.Instance.CurrentDBDocument);

                var glass = materials["Glass"];
                var dynamoMaterial = glass.Duplicate("Dynamo");
                dynamoMaterial.Color = new Color(255,128,0);
                DynamoMaterialId = dynamoMaterial.Id;

                TransactionManager.Instance.ForceCloseTransaction();
            });
        }

        private void FindDynamoGraphicsStyle()
        {
            var styles = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            styles.OfClass(typeof(GraphicsStyle));

            var gStyle = styles.ToElements().FirstOrDefault(x => x.Name == "Generic Models");

            if (gStyle != null)
            {
                DynamoGStyleId = gStyle.Id;
            }
            else
            {
                DynamoGStyleId = ElementId.InvalidElementId;
            }
        }
    }
}
