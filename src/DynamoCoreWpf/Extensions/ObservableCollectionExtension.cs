using System.Collections.ObjectModel;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// A class of extensions for the ObservableCollection.
    /// </summary>
    public static class ObservableCollectionExtension
    {
        /// <summary>
        /// A method to insert an element to collection by index. If index exceeds 
        /// count of elements element will be pushed to the tail of collection.
        /// </summary>
        /// <param name="index">Index to insert element to.</param>
        /// <param name="entry">Element to insert.</param>
        public static void TryInsert(this ObservableCollection<ISearchEntryViewModel> items, int index, ISearchEntryViewModel entry)
        {
            if (index < items.Count)
                items.Insert(index, entry);
            else
                items.Add(entry);
        }
    }
}
