using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
	public class MatrixRoomMessageEmote : MatrixRoomMessageBase, ITextMessage
	{
		public MatrixRoomMessageEmote()
			: base()
		{
			MessageType = "m.emote";
		}

		[DataMember(Name = "body")]
		public string Body { get; set; }
	}
}