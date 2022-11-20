using System.Collections.Generic;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class describes a folder (usually containing Dynamo graphs) added to the Dynamo Player or Generative Design
    /// </summary>
    public class DynamoPlayerFolder
    {
        /// <summary>
        /// The full path of the folder
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The display name of the folder
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The ID of the folder
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Is this folder removable from the settings (Built-in folders are non-removable)
        /// </summary>
        public bool IsRemovable { get; set; } = true;

        /// <summary>
        /// The order of the folder
        /// </summary>
        public int Order { get; set; } = -1;

        /// <summary>
        /// Is the folder path a valid path
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Is the folder path the default directory of the list
        /// </summary>
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// This class defines a group of folders associated with a Dynamo Player or Generative Design entry point.
    /// </summary>
    public class DynamoPlayerFolderGroup
    {
        /// <summary>
        /// The name of the Player entry point
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// The List of Folder Items for this group
        /// </summary>
        public List<DynamoPlayerFolder> Folders { get; set; } = new List<DynamoPlayerFolder>();
    }
}
