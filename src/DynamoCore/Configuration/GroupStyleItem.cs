using Dynamo.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dynamo.Configuration
{
    /// <summary>
    /// Group specific style item
    /// Note: This class does not contain special property yet comparing to base class
    /// </summary>
    public class GroupStyleItem: StyleItem
    {
        // These fields must stay above DefaultGroupStyleItems: static initializers run in textual order.
        private static readonly Guid ActionsStyleId = new Guid("4d68be4a-a04d-4945-9dd5-cdf61079d790");
        private static readonly Guid InputsStyleId = new Guid("883066aa-1fe2-44a4-9bd1-c3df86bfe9f6");
        private static readonly Guid OutputsStyleId = new Guid("07655dc1-2d65-4fed-8d6a-37235d3e3a8d");
        private static readonly Guid ReviewStyleId = new Guid("bc688959-ce34-4bf5-90f8-6ddd23f80989");

        /// <summary>
        /// Static set of default group styles defined by Dynamo Team
        /// </summary>
        public static ObservableCollection<GroupStyleItem> DefaultGroupStyleItems =
            new ObservableCollection<GroupStyleItem>() {
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultActions, HexColorString = Resources.GroupStyleDefaultActionsColor, FontSize = 36, IsDefault = true, GroupStyleId = ActionsStyleId },
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultInputs, HexColorString = Resources.GroupStyleDefaultInputsColor, FontSize = 36, IsDefault = true, GroupStyleId = InputsStyleId },
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultOutputs, HexColorString = Resources.GroupStyleDefaultOutputsColor, FontSize = 36, IsDefault = true, GroupStyleId = OutputsStyleId },
                new GroupStyleItem() { Name = Resources.GroupStyleDefaultReview, HexColorString = Resources.GroupStyleDefaultReviewColor, FontSize = 36 ,IsDefault = true, GroupStyleId = ReviewStyleId }
            };

        /// <summary>
        /// Returns the name of a default group style in the current UI culture, or null if the id
        /// does not belong to a default group style. Reads the resources on every call, so it
        /// reflects the culture at the time of the call and not the one active when
        /// <see cref="DefaultGroupStyleItems"/> was first initialised.
        /// </summary>
        /// <param name="groupStyleId">Id of the group style</param>
        internal static string GetLocalizedDefaultName(Guid groupStyleId)
        {
            if (groupStyleId == ActionsStyleId) return Resources.GroupStyleDefaultActions;
            if (groupStyleId == InputsStyleId) return Resources.GroupStyleDefaultInputs;
            if (groupStyleId == OutputsStyleId) return Resources.GroupStyleDefaultOutputs;
            if (groupStyleId == ReviewStyleId) return Resources.GroupStyleDefaultReview;
            return null;
        }

        /// <summary>
        /// Creates a new list with clones of the default group styles.
        /// </summary>
        internal static List<GroupStyleItem> CloneDefaultGroupStyleItems()
        {
            return DefaultGroupStyleItems
                .Select(defaultStyle => new GroupStyleItem
                {
                    Name = defaultStyle.Name,
                    HexColorString = defaultStyle.HexColorString,
                    FontSize = defaultStyle.FontSize,
                    GroupStyleId = defaultStyle.GroupStyleId,
                    IsDefault = true
                })
                .ToList();
        }
    }
}
