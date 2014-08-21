using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace DynamoWebServer
{
    public sealed class DynamoViewModelPool
    {
        private readonly int size;
        private readonly object locker;
        private readonly Queue<DynamoViewModel> queue;

        /// <summary>
        /// Initializes a new instance of the DynamoViewModelPool class.
        /// </summary>
        /// <param name="size">The size of the pool.</param>
        public DynamoViewModelPool(int size)
        {
            if (size <= 0)
            {
                const string message = "The size of the pool must be greater than zero.";
                throw new ArgumentOutOfRangeException("size", size, message);
            }

            this.size = size;
            locker = new object();
            queue = new Queue<DynamoViewModel>();

            for (int i = 0; i < size; i++)
            {
                Put(NewModel());
            }
        }


        /// <summary>
        /// Retrieves an item from the pool. 
        /// </summary>
        /// <returns>The item retrieved from the pool.</returns>
        public DynamoViewModel Get()
        {
            lock (locker)
            {
                return queue.Count > 0 ? queue.Dequeue() : NewModel();
            }
        }

        /// <summary>
        /// Places an item in the pool.
        /// </summary>
        /// <param name="item">The item to place to the pool.</param>
        public void Put(DynamoViewModel item)
        {
            lock (locker)
            {
                if (queue.Count < size)
                {
                    ClearWorkspace(item);
                    queue.Enqueue(item);
                }
            }
        }

        /// <summary>
        /// Creates new ViewModel
        /// </summary>
        /// <returns>new ViewModel</returns>
        public static DynamoViewModel NewModel()
        {
            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration
                {
                    Preferences = PreferenceSettings.Load()
                });

            var viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration
                {
                    CommandFilePath = null,
                    DynamoModel = model
                });

            return viewModel;
        }

        /// <summary>
        /// Cleanup workspace
        /// </summary>
        /// <param name="item">Model for cleaning</param>
        private static void ClearWorkspace(DynamoViewModel item)
        {
            foreach (var guid in item.model.CustomNodeManager.NodeInfos.Keys)
            {
                item.SearchViewModel.Model.RemoveNodeAndEmptyParentCategory(guid);

                var name = item.Model.CustomNodeManager.NodeInfos[guid].Name;
                var workspace = item.Workspaces.First(elem => elem.Name == name).Model;
                item.Model.Workspaces.Remove(workspace);

                item.Model.CustomNodeManager.LoadedCustomNodes.Remove(guid);
            }
            item.Model.CustomNodeManager.NodeInfos.Clear();

            item.ShowStartPage = false;
            item.Model.Clear(null);
        }
    }
}
