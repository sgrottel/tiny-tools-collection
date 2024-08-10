namespace LocalHtmlInterop.Handler
{
	internal enum CommandStatus
	{
		Unknown,
		Pending, //< also used when a partial answer is returned
		Error,
		Completed
	}
}
