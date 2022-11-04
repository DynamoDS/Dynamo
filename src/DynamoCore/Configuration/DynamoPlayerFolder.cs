using System.Collections.Generic;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class describes a folder (usually containing Dynamo graphs) added to the Dynamo Player or Generative Design
    /// </summary>
    public class DynamoPlayerFolder
    {
        /// <summary>
        /// The full path if the folder
        /// </summary>
        public string Path;

        /// <summary>
        /// The display name of the folder
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// The ID of the folder
        /// </summary>
        public string Id;

        /// <summary>
        /// Whether this folder is removable from the settings (Built-in folders are non-removable)
        /// </summary>
        public bool IsRemovable = true;

        /// <summary>
        /// The order of the folder
        /// </summary>
        public int Order = -1;

        /// <summary>
        /// Initialize a DynamoPlayerFolderItem
        /// </summary>
        /// <param name="path">The full path of the folder</param>
        /// <param name="displayName">The display name of the folder</param>
        /// <param name="id">The id of the folder</param>
        /// <param name="isRemovable">The removalbe state of the folder</param>
        /// <param name="order">The order of the folder in the UI</param>
        public DynamoPlayerFolder(string path, string displayName, string id, bool isRemovable, int order)
        {
            Path = path;
            DisplayName = displayName;
            Id = id;
            IsRemovable = isRemovable;
            Order = order;
        }

        /// <summary>
        /// Create an empty DynamoPlayerFolderItem
        /// </summary>
        public DynamoPlayerFolder()
        {

        }
    }

    /// <summary>
    /// This Class defines a group of folders assciated with a Dynamo Player or Generative Design entry point.
    /// </summary>
    public class DynamoPlayerFolderGroup
    {
        /// <summary>
        /// The name of the Player entry point
        /// </summary>
        public string EntryPoint;

        /// <summary>
        /// The List of Folder Items for this group
        /// </summary>
        public List<DynamoPlayerFolder> Folders;

        /// <summary>
        /// Creates an empty DynamoPlayerFolderGroup
        /// </summary>
        public DynamoPlayerFolderGroup()
        {
            Folders = new List<DynamoPlayerFolder>();
        }

        /// <summary>
        /// Initialize a DynamoPlayerFolderItem
        /// </summary>
        /// <param name="entryPoint">The name of the Player entry point</param>
        /// <param name="folders">The list of Folder data</param>
        public DynamoPlayerFolderGroup(string entryPoint, List<DynamoPlayerFolder> folders)
        {
            EntryPoint = entryPoint;
            Folders = folders;
        }
    }
}
