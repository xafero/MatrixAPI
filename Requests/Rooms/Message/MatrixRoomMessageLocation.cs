using System.Runtime.Serialization;
using libMatrix.APITypes;

namespace libMatrix.Requests.Rooms.Message
{
    [DataContract]
    public class MatrixRoomMessageLocation : MatrixRoomMessageBase
    {
        public MatrixRoomMessageLocation()
            : base()
        {
            MessageType = "m.location";
        }

        [DataMember(Name = "body", IsRequired = true)]
        public string Description { get; set; }

        [DataMember(Name = "geo_uri", IsRequired = true)]
        public string GeoUri { get; set; }

        [DataMember(Name = "info", IsRequired = false)]
        public MatrixThumbnailInfo ThumbnailInfo { get; set; }
    }
}