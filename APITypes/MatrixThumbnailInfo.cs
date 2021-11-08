using System.Runtime.Serialization;

namespace libMatrix.APITypes
{
	[DataContract]
	public class MatrixThumbnailInfo
	{
		[DataMember(Name = "thumbnail_url", IsRequired = false)]
		public string ThumbnailUrl { get; set; }

		[DataMember(Name = "thumbnail_info", IsRequired = false)]
		public MatrixContentImageInfo ThumbnailInfo { get; set; }
	}
}