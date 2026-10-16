using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dynamo.Notifications
{
    [DataContract]
    internal class NotificationsModel
    {
        [DataMember(Name = "version")]
        [JsonPropertyName("version")]
        internal string Version { get; set; }

        [DataMember(Name = "last_update_timestamp")]
        [JsonPropertyName("last_update_timestamp")]
        internal DateTime LastUpdate { get; set; }

        [DataMember(Name = "notifications")]
        [JsonPropertyName("notifications")]
        internal List<NotificationItemModel> Notifications { get; set; }
    }

    [DataContract]
    internal class NotificationItemModel
    {
        [DataMember(Name = "id")]
        [JsonPropertyName("id")]
        internal string Id { get; set; }

        [DataMember(Name = "title")]
        [JsonPropertyName("title")]
        internal string Title { get; set; }

        [DataMember(Name = "link")]
        [JsonPropertyName("link")]
        internal string Link { get; set; }

        [DataMember(Name = "linkTitle")]
        [JsonPropertyName("linkTitle")]
        internal string LinkTitle { get; set; }

        [DataMember(Name = "longDescription")]
        [JsonPropertyName("longDescription")]
        internal string LongDescription { get; set; }

        [DataMember(Name = "created")]
        [JsonPropertyName("created")]
        internal DateTime Created { get; set; }

        [DataMember(Name = "thumbnail")]
        [JsonPropertyName("thumbnail")]
        internal string Thumbnail { get; set; }

        [DataMember(Name = "priority")]
        [JsonPropertyName("priority")]
        internal string Priority { get; set; }

        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        internal string Type { get; set; }

        [DataMember(Name = "scope")]
        [JsonPropertyName("scope")]
        internal string Scope { get; set; }

        [DataMember(Name = "source")]
        [JsonPropertyName("source")]
        internal string Source { get; set; }

        [DataMember(Name = "status")]
        [JsonPropertyName("status")]
        internal string Status { get; set; }

        [DataMember(Name = "isPinned")]
        [JsonPropertyName("isPinned")]
        internal bool IsPinned { get; set; }

        [DataMember(Name = "isRead")]
        [JsonPropertyName("isRead")]
        internal bool IsRead { get; set; } = true;
    }
}
