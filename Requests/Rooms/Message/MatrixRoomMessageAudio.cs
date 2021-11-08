using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
	public class MatrixRoomMessageAudio : MatrixRoomMessageBase, IFileMessage
	{
		public MatrixRoomMessageAudio()
			: base()
		{
			MessageType = "m.audio";
		}

		[DataMember(Name = "body", IsRequired = true)]
		public string Description { get; set; }

		[DataMember(Name = "url", IsRequired = false)]
		public string MediaUrl { get; set; }
	}
}