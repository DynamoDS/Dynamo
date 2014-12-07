using System;
using System.Diagnostics;
using System.Windows.Forms;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics.Sample.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            AutoMeasurement.DebugWriter = d => Debug.WriteLine(d);
            AutoMeasurement.Start(new MeasurementConfiguration("UA-319000-8"));

            AllowUsageDataCollectionCheckBox.Checked = AutoMeasurement.VisitorStatus == VisitorStatus.Active;
        }

        private void TrackScreenButtonClick(object sender, EventArgs e)
        {
            AutoMeasurement.Client.TrackScreenView("My Shiny Screen");
        }

        private void TrackEventButtonClick(object sender, EventArgs e)
        {
            AutoMeasurement.Client.TrackEvent("My Action", "My Custom Category", "My Label");
        }

        private void AllowUsageDataCollectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var isAlreadyActive = AutoMeasurement.VisitorStatus == VisitorStatus.Active;
            var value = AllowUsageDataCollectionCheckBox.Checked;

            if ((value && !isAlreadyActive) || (!value && isAlreadyActive))
            {
                AutoMeasurement.SetOptOut(!value);
            }
        }
    }
}