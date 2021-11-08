using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
	public class MatrixRoomMessageNotice : MatrixRoomMessageBase, ITextMessage
	{
		public MatrixRoomMessageNotice()
			: base()
		{
			MessageType = "m.notice";
		}

		[DataMember(Name = "body")]
		public string Body { get; set; }
	}
}