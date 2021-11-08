﻿using System.Runtime.Serialization;

namespace libMatrix.Requests.Rooms.Message
{
	[DataContract]
    public class MatrixRoomMessageImage : MatrixRoomMessageBase
    {
        public MatrixRoomMessageImage()
            : base()
        {
            base.MessageType = "m.image";
        }

        [DataMember(Name = "body", IsRequired = true)]
        public string Description { get; set; }

        [DataMember(Name = "url", IsRequired = false)]
        public string ImageUrl { get; set; }
    }
}
