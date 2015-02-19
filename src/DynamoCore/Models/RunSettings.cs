using System.Diagnostics;

using Dynamo.Core;

namespace Dynamo.Models
{
    /// <summary>
    /// The RunType enumeration provides values for
    /// specifying the type of run that will be conducted.
    /// </summary>
    public enum RunType { Manually, Automatically, Periodically }

    /// <summary>
    /// The RunSettings object contains properties which control
    /// how execution is carried out.
    /// </summary>
    public class RunSettings : NotificationObject
    {
        #region private members

        private int runPeriod;
        private RunType runType;
        private bool runEnabled;

        #endregion

        #region properties

        public int RunPeriod
        {
            get { return runPeriod; }
            set
            {
                runPeriod = value;
                RaisePropertyChangeWithDebug("RunPeriod");
            }
        }

        public RunType RunType
        {
            get { return runType; }
            set
            {
                runType = value;
                RaisePropertyChangeWithDebug("RunType");
            }
        }

        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                if (Equals(value, runEnabled)) return;
                runEnabled = value;
                RaisePropertyChangeWithDebug("RunEnabled");
            }
        }

        #endregion

        #region constructors

        public RunSettings()
        {
            RunPeriod = 100;
            RunType = RunType.Manually;
            RunEnabled = true;
        }

        public RunSettings(RunType runType, int period)
        {
            RunPeriod = period;
            RunType = runType;
            RunEnabled = true;
        }

        #endregion

        #region private methods

        private void RaisePropertyChangeWithDebug(string propertyName)
        {
#if DEBUG
            Debug.WriteLine(string.Format("{0} property change raised on the RunSettings object.", propertyName));
#endif
            RaisePropertyChanged(propertyName);
        }

        #endregion
    }
}
