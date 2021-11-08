using libMatrix.Helpers;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace libMatrix.Responses.Events.Room
{
	[DataContract]
    public class Message : MatrixEvents
    {
        [DataMember(Name = "content")]
        [JsonConverter(typeof(MatrixEventsRoomMessageItemConverter))]
        public MessageContent Content { get; set; }
    }

    [DataContract]
    public class MessageContent
    {
        [DataMember(Name = "body")]
        public string Body { get; set; }
        [DataMember(Name = "msgtype")]
        public string MessageType { get; set; }
    }

    [DataContract]
    public class MessageImageContent : MessageContent
    {
        [DataMember(Name = "info")]
        public APITypes.MatrixContentImageInfo ImageInfo { get; set; }
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }

    [DataContract]
    public class MessageLocationContent : MessageContent
    {
        [DataMember(Name = "geo_uri")]
        public string GeoUri { get; set; }
        [DataMember(Name = "thumbnail_info")]
        public APITypes.MatrixContentImageInfo ThumbnailInfo { get; set; }
        [DataMember(Name = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }
}
