using System.Runtime.Serialization;

namespace libMatrix.Responses.Rooms
{
	[DataContract]
    public class RoomAlias
    {
        [DataMember(Name = "room_id")]
        public string RoomID { get; set; }

        [DataMember(Name = "servers")]
        public string[] Servers { get; set; }
    }
}