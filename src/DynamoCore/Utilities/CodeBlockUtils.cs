using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Utilities
{
    public class CodeBlockUtils
    {
        /// <summary>
        /// Call this method to generate a list of PortData from given set of 
        /// unbound identifiers. This method ensures that the generated ports 
        /// are only having names that do not exceed a preconfigured length.
        /// </summary>
        /// <param name="unboundIdents">A list of unbound identifiers for which 
        /// input port data is to be generated. This list can be empty but it 
        /// cannot be null.</param>
        /// <returns>Returns a list of input port data generated based on the 
        /// input unbound identifier list.</returns>
        /// 
        public static IEnumerable<PortData> GenerateInputPortData(
            IEnumerable<string> unboundIdents)
        {
            if (unboundIdents == null)
                throw new ArgumentNullException("unboundIdents");

            int maxLength = Configurations.CBNMaxPortNameLength;
            List<PortData> inputPorts = new List<PortData>();

            foreach (string name in unboundIdents)
            {
                string portName = name;
                if (portName.Length > maxLength)
                    portName = portName.Remove(maxLength - 3) + "...";

                inputPorts.Add(new PortData(portName, name, typeof(object)));
            }

            return inputPorts;
        }

        /// <summary>
        /// Call this method to get a list of lists of variables defined in 
        /// the given set of Statement objects. This method is typically used 
        /// in conjunction with StatementRequiresOutputPort method.
        /// </summary>
        /// <param name="statements">A list of Statement objects whose defined 
        /// variables are to be retrieved. This list can be empty but it cannot 
        /// be null.</param>
        /// <param name="onlyTopLevel">Set this parameter to false to retrieve 
        /// all variables defined in nested Statement objects.</param>
        /// <returns>Returns a list of lists of variables defined by the given 
        /// set of Statement objects.</returns>
        /// 
        public static IEnumerable<IEnumerable<string>> GetStatementVariables(
            IEnumerable<Statement> statements, bool onlyTopLevel)
        {
            if (statements == null)
                throw new ArgumentNullException("statements");

            var definedVariables = new List<List<string>>();
            foreach (var statement in statements)
            {
                definedVariables.Add(Statement.GetDefinedVariableNames(
                    statement, onlyTopLevel));
            }

            return definedVariables;
        }

        /// <summary>
        /// Checks wheter an outport is required for a Statement with the given 
        /// index. An outport is not required if there are no defined variables 
        /// or if any of the defined variables have been declared again later on
        /// in the same code block.
        /// </summary>
        /// <param name="statementVariables">A list of lists, each of which 
        /// contains variables defined by a Statement at the index. This list 
        /// can be obtained from calling GetStatementVariables method.</param>
        /// <param name="index">The index of the Statement for which this call 
        /// is made.</param>
        /// <returns>Returns true if an output port is required, or false 
        /// otherwise.</returns>
        /// 
        public static bool StatementRequiresOutputPort(
            IEnumerable<IEnumerable<string>> statementVariables, int index)
        {
            if (statementVariables == null)
                throw new ArgumentNullException("statementVariables");

            int statementCount = statementVariables.Count();
            if (statementCount <= 0)
                return false;

            if (index < 0 || (index >= statementCount))
                throw new IndexOutOfRangeException("index");

            var currentVariables = statementVariables.ElementAt(index);
            for (int stmt = index + 1; stmt < statementCount; stmt++)
            {
                var variables = statementVariables.ElementAt(stmt);
                foreach (var cv in currentVariables)
                {
                    if (variables.Contains(cv))
                        return false;
                }
            }

            return true;
        }
    }
}
