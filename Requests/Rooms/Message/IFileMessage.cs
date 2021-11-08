namespace libMatrix.Requests.Rooms.Message
{
	public interface IFileMessage
	{
		string Description { get; set; }

		string MediaUrl { get; set; }
	}
}