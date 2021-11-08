using System.Collections.Generic;
using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms
{
	[DataContract]
	public class MatrixRoomCreate
	{
		[DataMember(Name = "name", IsRequired = false)]
		public string Name { get; set; }

		[DataMember(Name = "invite", IsRequired = false)]
		public List<string> InviteList { get; set; }

		[DataMember(Name = "topic", IsRequired = false)]
		public string Topic { get; set; }

		[DataMember(Name = "is_direct", IsRequired = false)]
		public bool IsDirect { get; set; }

		[DataMember(Name = "room_alias_name", IsRequired = true)]
		public string RoomAliasName { get; set; }

		[DataMember(Name = "visibility", IsRequired = false)]
		public string Visibility { get; set; }
	}
}