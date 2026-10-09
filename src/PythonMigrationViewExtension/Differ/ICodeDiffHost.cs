namespace Dynamo.PythonMigration.Differ
{
    /// <summary>
    /// What BaseDiffViewer needs to show a before/after and to run Accept / Reject.
    /// The migration assistant and the assistant Python review both implement this.
    /// The window does not know which one it is.
    /// </summary>
    internal interface ICodeDiffHost
    {
        IDiffViewViewModel CurrentViewModel { get; }
        string WindowTitle { get; }

        /// <summary>
        /// True only for the Python 2-to-3 migrator. The assistant review must not
        /// be counted as a migration in telemetry.
        /// </summary>
        bool TrackAsMigration { get; }

        void ChangeViewModel(ViewMode viewMode);

        /// <summary>
        /// Accept button. Returns false to keep the window open; the host has told the user why.
        /// </summary>
        bool ChangeCode();

        /// <summary>
        /// Reject button. Returns false to keep the window open; the host has told the user why.
        /// </summary>
        bool RejectCode();
    }
}
