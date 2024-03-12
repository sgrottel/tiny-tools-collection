using SGrottel;
using System.Net.Sockets;

namespace LocalHtmlInterop.Handler
{
	internal sealed class AppTcpClient : Server.Client
	{
		internal class Data
		{
			public string? Command { get; set; }
			public string? CallbackId { get; set; }
			public Dictionary<string, string>? CommandParameters { get; set; }
		}

		private ISimpleLog? log;
		private TcpClient client;
		private Thread worker;
		internal AppTcpClient(TcpClient client, ISimpleLog? log)
			: base()
		{
			this.client = client;
			this.log = log;
			worker = new Thread(work);
			worker.Start();
		}
		public override void Close()
		{
			client.Close();
		}
		public override void SendData(ReadOnlySpan<byte> data)
		{
			client.GetStream().Write(data);
		}
		public override void SendText(string text)
		{
			throw new NotImplementedException();
		}

		private void work(object? obj)
		{
			byte retval = 0;
			try
			{
				var stream = client.GetStream();

				byte[] bytes = new byte[4];
				stream.ReadExactly(bytes);

				uint len = BitConverter.ToUInt32(bytes);

				bytes = new byte[len];
				stream.ReadExactly(bytes);

				FireOnDataMessageReceived(bytes);

				retval = 1;
			}
			catch (Exception ex)
			{
				log?.Write(ISimpleLog.FlagError, $"EXCEPTION handling AppTcpClient: {ex}");
			}
			try
			{
				client.GetStream().WriteByte(retval);
				client.Close();
			}
			catch (Exception ex)
			{
				log?.Write(ISimpleLog.FlagError, $"EXCEPTION closing AppTcpClient: {ex}");
			}
			try
			{
				FireOnClosed();
			}
			catch (Exception ex)
			{
				log?.Write(ISimpleLog.FlagError, $"EXCEPTION FireOnClosed AppTcpClient: {ex}");
			}
		}
	}
}
