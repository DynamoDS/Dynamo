﻿using Dynamo.Models;

namespace Dynamo.Interfaces
{
    public interface IVisualizationManager
    {
        /// <summary>
        /// Flag allows us to pause visualization updates.
        /// </summary>
        bool UpdatingPaused { get; }

        /// <summary>
        /// Is another context available for drawing?
        /// This property can be queried indirectly by the view to enable or disable
        /// UI functionality based on whether an alternate drawing context is available.
        /// </summary>
        bool AlternateDrawingContextAvailable { get; set; }

        /// <summary>
        /// Should we draw to the alternate context if it is available?
        /// </summary>
        bool DrawToAlternateContext { get; set; }

        /// <summary>
        /// Can be used to expose a name of the alternate context for use in the UI.
        /// </summary>
        string AlternateContextName { get; set; }

        /// <summary>
        /// The maximum number of divisions in any direction of an analytical surface 
        /// tesselation
        /// </summary>
        int MaxTesselationDivisions { get; set; }

        /// <summary>
        /// An event triggered on the completion of visualization update.
        /// </summary>
        event RenderCompleteEventHandler RenderComplete;

        /// <summary>
        /// Display a label for one or several render packages 
        /// based on the paths of those render packages.
        /// </summary>
        /// <param name="path"></param>
        void TagRenderPackageForPath(string path);

        /// <summary>
        /// An event triggered when there are results to visualize
        /// </summary>
        event ResultsReadyHandler ResultsReadyToVisualize;

        /// <summary>
        /// Pause the visualization manager.
        /// </summary>
        void Pause();

        /// <summary>
        /// Unpause the visualization manager.
        /// </summary>
        void UnPause();

        void RequestBranchUpdate(NodeModel node);
    }
}
