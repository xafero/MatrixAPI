﻿using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
	public class MatrixRoomMessageText : MatrixRoomMessageBase, ITextMessage
	{
		public MatrixRoomMessageText()
			: base()
		{
			MessageType = "m.text";
		}

		[DataMember(Name = "body")]
		public string Body { get; set; }
	}
}