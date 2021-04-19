using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;
using Dynamo.Models;

namespace Dynamo.Linting
{
    public class LinterManager : NotificationObject
    {
        #region Private fields
        private readonly IExtensionManager extensionManager;
        private LinterExtensionDescriptor activeLinter;
        #endregion

        #region Public properties

        /// <summary>
        /// Available linters
        /// </summary>
        public HashSet<LinterExtensionDescriptor> AvailableLinters { get; internal set; }

        /// <summary>
        /// Results from evaluated rules
        /// </summary>
        public ObservableCollection<IRuleEvaluationResult> RuleEvaluationResults { get; set; }

        /// <summary>
        /// The LinterDescripter that is currently set as active
        /// </summary>
        public LinterExtensionDescriptor ActiveLinter
        {
            get => activeLinter;
            set
            {
                if (activeLinter == value)
                    return;

                if (activeLinter != null)
                    GetLinterExtension(activeLinter).Deactivate();

                var linterExt = GetLinterExtension(value);
                if (linterExt is null)
                    return;

                linterExt.Activate();
                activeLinter = value;
            }
        }

        #endregion

        public LinterManager(IExtensionManager extensionManager)
        {
            this.extensionManager = extensionManager;
            AvailableLinters = new HashSet<LinterExtensionDescriptor>();
            RuleEvaluationResults = new ObservableCollection<IRuleEvaluationResult>();

            SubscribeLinterEvents();
        }

        /// <summary>
        /// Checks if the uniqueId equals the id of the Active linter descriptor
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        internal bool IsExtensionActive(string uniqueId)
        {
            return ActiveLinter?.Id == uniqueId;
        }

        #region Private methods
        private void SubscribeLinterEvents()
        {
            LinterExtensionBase.LinterExtensionReady += OnLinterExtensionReady;
            LinterRule.RuleEvaluated += OnRuleEvaluated;

        }

        private void OnLinterExtensionReady(LinterExtensionDescriptor extensionDescriptor)
        {
            if (AvailableLinters.Contains(extensionDescriptor))
                return;

            AvailableLinters.Add(extensionDescriptor);
        }

        private void OnRuleEvaluated(IRuleEvaluationResult result)
        {
            if (result is null)
                return;

            if (result.Status == RuleEvaluationStatusEnum.Passed)
            {
                if (!RuleEvaluationResults.Contains(result))
                    return;
                RuleEvaluationResults.Remove(result);
            }

            else
            {
                if (RuleEvaluationResults.Contains(result))
                    return;
                RuleEvaluationResults.Add(result);
            }
        }

        private LinterExtensionBase GetLinterExtension(LinterExtensionDescriptor activeLinter)
        {
            return this.extensionManager.
                Extensions.
                OfType<LinterExtensionBase>().
                Where(x => x.UniqueId == activeLinter.Id).
                FirstOrDefault();
        }

        #endregion
    }
}
