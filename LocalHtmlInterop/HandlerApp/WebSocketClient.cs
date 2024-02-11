using SGrottel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalHtmlInterop.Handler
{

	/// <summary>
	/// https://datatracker.ietf.org/doc/html/rfc6455#section-5.2
	/// </summary>
	internal abstract class WebSocketClient : Server.Client
	{
		private Thread receiver;

		protected WebSocketClient(TcpClient client)
		{
			this.client = client;
			receiver = new Thread(RunReceiverWorker);
			receiver.Start();
		}

		protected TcpClient client;

		public override int? Port { get => ((IPEndPoint?)(client.Client?.LocalEndPoint))?.Port; }

		public override void Close()
		{
			silentExceptions = true;
			client.Close();
			FireOnClosed();
		}

		public override void SendText(string text)
		{
			SendImpl(1, Encoding.UTF8.GetBytes(text));
		}

		public override void SendData(ReadOnlySpan<byte> data)
		{
			SendImpl(2, data);
		}

		private void SendImpl(byte mode, ReadOnlySpan<byte> data)
		{
			int l = data.Length;
			int headerSizeBytesNeeded = (l < 126) ? 0 : (l <= ushort.MaxValue) ? 2 : 8;

			byte[] header = new byte[2 + headerSizeBytesNeeded];
			header[0] = (byte)((mode & 0x0f) | 0b_1000_0000);

			if (l < 126)
			{
				header[1] = (byte)l;
			}
			else if (l <= ushort.MaxValue)
			{
				header[1] = 126;
				byte[] lData = BitConverter.GetBytes((ushort)l);
				header[2] = lData[1];
				header[3] = lData[0];
			}
			else
			{
				header[1] = 127;
				byte[] lData = BitConverter.GetBytes((ulong)l);
				header[2] = lData[7];
				header[3] = lData[6];
				header[4] = lData[5];
				header[5] = lData[4];
				header[6] = lData[3];
				header[7] = lData[2];
				header[8] = lData[1];
				header[9] = lData[0];
			}

			var stream = client.GetStream();
			stream.Write(header);
			stream.Write(data);
		}

		private bool silentExceptions = false;

		private void RunReceiverWorker()
		{
			try
			{
				var stream = client.GetStream();
				int read = -1;

				int openMessage = 0;
				IEnumerable<byte>? messageData = null;

				while (true)
				{
					if (((read = stream.ReadByte()) < 0) || (read > 255)) break; // stream broken
					byte b0 = (byte)read;
					bool fin = (b0 & 0b10000000) != 0;
					int opcode = b0 & 0b00001111;

					if (((read = stream.ReadByte()) < 0) || (read > 255)) break; // stream broken
					byte b1 = (byte)read;
					bool mask = (b1 & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"

					ulong msglen = b1 & (ulong)0b01111111;
					if (msglen == 126)
					{
						byte[] ldata = new byte[2];
						for (int i = 1; i >= 0; i--)
						{
							if (((read = stream.ReadByte()) < 0) || (read > 255)) break; // stream broken
							ldata[i] = (byte)read;
						}
						msglen = BitConverter.ToUInt16(ldata, 0);
					}
					else if (msglen == 127)
					{
						byte[] ldata = new byte[8];
						for (int i = 7; i >= 0; i--)
						{
							if (((read = stream.ReadByte()) < 0) || (read > 255)) break; // stream broken
							ldata[i] = (byte)read;
						}
						msglen = BitConverter.ToUInt64(ldata, 0);
					}

					byte[]? masks = null;
					if (mask)
					{
						masks = new byte[4];
						stream.ReadExactly(masks);
					}

					byte[] data = Array.Empty<byte>();
					if (msglen > 0)
					{
						data = new byte[msglen];
						stream.ReadExactly(data);
						if (masks != null)
						{
							for (ulong i = 0; i < msglen; ++i)
								data[i] = (byte)(data[i] ^ masks[i % 4]);
						}
					}

					switch (opcode)
					{
						case 0: // continuation
							if (openMessage == 0)
							{
								Log?.Write(ISimpleLog.FlagWarning, $"Continuation message fragment without start ignored ({msglen} bytes)");
								break;
							}
							messageData = messageData!.Concat(data);
							if (fin)
							{
								FireOnMessageRecived(openMessage, messageData.ToArray());
								openMessage = 0;
								messageData = null;
							}
							break;
						case 1: // text
							// no break;
						case 2: // binary
							if (fin)
							{
								FireOnMessageRecived(opcode, data);
							}
							else
							{
								openMessage = opcode;
								messageData = data;
							}
							break;
						case 8: // close
							SendImpl(0x08, Array.Empty<byte>()); // ping back control frame, then close
							break;
						case 9: // ping
							SendImpl(0x0A, data); // echo data back as pong
							break;
						case 10: // pong
							// no response expected
							break;
						default:
							Log?.Write(ISimpleLog.FlagWarning, $"Message of unknown opcode {opcode} ignored ({msglen} bytes)");
							break;
					}
					if (opcode == 8) break;
				}
			}
			catch (Exception ex)
			{
				if (!silentExceptions)
				{
					Log?.Write(ISimpleLog.FlagError, $"EXCEPTION: Client {Port} closed; {ex}");
				}
	
			}

			if (client?.Connected ?? false)
			{
				try
				{
					Close();
				}
				catch
				{
				}
			}
		}

		private void FireOnMessageRecived(int opcode, byte[] data)
		{
			if (opcode == 1)
			{
				try
				{
					FireOnTextMessageReceived(Encoding.UTF8.GetString(data));
				}
				catch { }
			}
			else if (opcode == 2)
			{
				FireOnDataMessageReceived(data);
			}
		}
	}
}
