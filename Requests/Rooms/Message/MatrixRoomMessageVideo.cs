using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
	public class MatrixRoomMessageVideo : MatrixRoomMessageBase, IFileMessage
	{
		public MatrixRoomMessageVideo()
			: base()
		{
			MessageType = "m.video";
		}

		[DataMember(Name = "body", IsRequired = true)]
		public string Description { get; set; }

		[DataMember(Name = "url", IsRequired = false)]
		public string MediaUrl { get; set; }
	}
}