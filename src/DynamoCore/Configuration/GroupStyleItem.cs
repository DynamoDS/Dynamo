using System;
using System.Collections.ObjectModel;
using Dynamo.Properties;

namespace Dynamo.Configuration
{
    /// <summary>
    /// Group specific style item
    /// Note: This class does not contain special property yet comparing to base class
    /// </summary>
    public class GroupStyleItem: StyleItem
    {
        /// <summary>
        /// Static set of default group styles defined by Dynamo Team
        /// </summary>
        public static ObservableCollection<GroupStyleItem> DefaultGroupStyleItems =
            new ObservableCollection<GroupStyleItem>() {
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultActions, HexColorString = Resources.GroupStyleDefaultActionsColor, FontSize = 36, IsDefault = true, GroupStyleId = new Guid("4d68be4a-a04d-4945-9dd5-cdf61079d790") },
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultInputs, HexColorString = Resources.GroupStyleDefaultInputsColor, FontSize = 36, IsDefault = true, GroupStyleId = new Guid("883066aa-1fe2-44a4-9bd1-c3df86bfe9f6") },
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultOutputs, HexColorString = Resources.GroupStyleDefaultOutputsColor, FontSize = 36, IsDefault = true, GroupStyleId = new Guid("07655dc1-2d65-4fed-8d6a-37235d3e3a8d") },
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultReview, HexColorString = Resources.GroupStyleDefaultReviewColor, FontSize = 36 ,IsDefault = true, GroupStyleId = new Guid("bc688959-ce34-4bf5-90f8-6ddd23f80989") }
            };
    }
}
