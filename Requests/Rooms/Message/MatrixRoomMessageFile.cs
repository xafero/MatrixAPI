using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
	public class MatrixRoomMessageFile : MatrixRoomMessageBase, IFileMessage
	{
		public MatrixRoomMessageFile()
			: base()
		{
			MessageType = "m.file";
		}

		[DataMember(Name = "body", IsRequired = true)]
		public string Description { get; set; }

		[DataMember(Name = "url", IsRequired = false)]
		public string MediaUrl { get; set; }
	}
}