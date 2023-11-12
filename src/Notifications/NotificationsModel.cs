using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Notifications
{
    [DataContract]
    internal class NotificationsModel
    {
        [DataMember(Name = "version")]
        internal string Version { get; set; }

        [DataMember(Name = "last_update_timestamp")]
        internal DateTime LastUpdate { get; set; }

        [DataMember(Name = "notifications")]
        internal List<NotificationItemModel> Notifications { get; set; }
    }

    [DataContract]
    internal class NotificationItemModel
    {
        [DataMember(Name = "id")]
        internal string Id { get; set; }

        [DataMember(Name = "title")]
        internal string Title { get; set; }

        [DataMember(Name = "link")]
        internal string Link { get; set; }

        [DataMember(Name = "linkTitle")]
        internal string LinkTitle { get; set; }

        [DataMember(Name = "longDescription")]
        internal string LongDescription { get; set; }

        [DataMember(Name = "created")]
        internal DateTime Created { get; set; }

        [DataMember(Name = "thumbnail")]
        internal string Thumbnail { get; set; }

        [DataMember(Name = "priority")]
        internal string Priority { get; set; }

        [DataMember(Name = "type")]
        internal string Type { get; set; }

        [DataMember(Name = "scope")]
        internal string Scope { get; set; }

        [DataMember(Name = "source")]
        internal string Source { get; set; }

        [DataMember(Name = "status")]
        internal string Status { get; set; }

        [DataMember(Name = "isPinned")]
        internal bool IsPinned { get; set; }

        [DataMember(Name = "isRead")]
        internal bool IsRead { get; set; } = true;
    }
}
