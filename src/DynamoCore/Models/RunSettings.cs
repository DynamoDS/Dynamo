using System.Diagnostics;

using Dynamo.Core;

namespace Dynamo.Models
{
    /// <summary>
    /// The RunType enumeration provides values for
    /// specifying the type of run that will be conducted.
    /// </summary>
    public enum RunType { Manual, Automatic, Periodic }

    /// <summary>
    /// The RunSettings object contains properties which control
    /// how execution is carried out.
    /// </summary>
    public class RunSettings : NotificationObject
    {
        #region Class Data Members and Properties

        private int runPeriod;
        private RunType runType;
        private bool runEnabled;

        /// <summary>
        /// Defalut milliseconds number for the period in periodic run.
        /// </summary>
        public const int DefaultRunPeriod = 1000;

        /// <summary>
        /// The length, in milliseconds, of the period
        /// between requests to execute.
        /// </summary>
        public int RunPeriod
        {
            get { return runPeriod; }
            set
            {
                if (runPeriod == value) return;

                runPeriod = value;
                RaisePropertyChangeWithDebug("RunPeriod");
            }
        }

        /// <summary>
        /// The current RunType.
        /// </summary>
        public RunType RunType
        {
            get { return runType; }
            set
            {
                if (runType == value) return;

                runType = value;
                RaisePropertyChangeWithDebug("RunType");
            }
        }

        /// <summary>
        /// A flag which indicates whether running is possible. This 
        /// flag is set to false during execution and is set to true
        /// when execution is completed.
        /// </summary>
        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                if (runEnabled == value) return;

                runEnabled = value;
                RaisePropertyChangeWithDebug("RunEnabled");
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// This method creates default RunSettings with DefaultRunPeriod, Manual run type and run is anabled.
        /// </summary>
        public RunSettings()
        {
            RunPeriod = DefaultRunPeriod;
            RunType = RunType.Manual;
            RunEnabled = true;
        }

        /// <summary>
        /// This function creates RunSettings with specified run type and run period.
        /// </summary>
        /// <param name="runType">RunType</param>
        /// <param name="period">milliseconds</param>
        public RunSettings(RunType runType, int period)
        {
            RunPeriod = period;
            RunType = runType;
            RunEnabled = true;
        }

        #endregion

        #region Public Operational Methods

        internal void Reset()
        {
            RunEnabled = true;
            RunType = RunType.Automatic;
            RunPeriod = DefaultRunPeriod;
        }

        #endregion

        #region Private Class Methods

        private void RaisePropertyChangeWithDebug(string propertyName)
        {
            RaisePropertyChanged(propertyName);
        }

        #endregion

    }
}
