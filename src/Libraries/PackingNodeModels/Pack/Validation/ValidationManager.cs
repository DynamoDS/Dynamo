using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingNodeModels.Pack.Validation
{
    internal interface IValidationManager
    {
        void HandleValidation(Dictionary<int, object> valuesByIndex);
        ReadOnlyCollection<Validation> Warnings { get; }
    }

    internal class ValidationManager : IValidationManager
    {
        private List<Validation> warnings;

        public ReadOnlyCollection<Validation> Warnings { get { return warnings.AsReadOnly(); } }

        public Pack Node { get; private set; }

        public ValidationManager(Pack node)
        {
            Node = node;
            warnings = new List<Validation>();
        }

        public void HandleValidation(Dictionary<int, object> valuesByIndex)
        {
            var properties = Node.TypeDefinition.Properties.ToList();
            foreach (var pair in valuesByIndex)
            {
                ClearWarningForPortIndex(pair.Key);

                if (!Node.InPorts[pair.Key].Connectors.Any())
                    continue;

                var validation = PortValidator.Validate(properties[pair.Key - 1], pair.Value, Node.InPorts[pair.Key]); //i - 1 because we're skipping the Type port.

                if (!string.IsNullOrEmpty(validation))
                    warnings.Add(new Validation { Message = validation, PortIndex = pair.Key });
            }

            ComputeWarnings();
        }

        private void ClearWarningForPortIndex(int index)
        {
            warnings.RemoveAll(w => w.PortIndex == index);
        }

        private void ComputeWarnings()
        {
            Node.ClearErrorsAndWarnings();
            if (warnings.Any())
                Node.Warning(String.Join(String.Empty, warnings.Select(w => w.Message).ToArray()), true);
        }
    }

    internal class Validation
    {
        public string Message { get; set; }
        public int PortIndex { get; set; }
    }
}
