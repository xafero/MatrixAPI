﻿using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
	public class MatrixRoomMessageImage : MatrixRoomMessageBase, IFileMessage
	{
		public MatrixRoomMessageImage()
			: base()
		{
			MessageType = "m.image";
		}

		[DataMember(Name = "body", IsRequired = true)]
		public string Description { get; set; }

		[DataMember(Name = "url", IsRequired = false)]
		public string MediaUrl { get; set; }
	}
}