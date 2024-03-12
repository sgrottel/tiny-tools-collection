using SGrottel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LocalHtmlInterop.Handler.Server;

namespace LocalHtmlInterop.Handler
{
	internal class Server
	{
		private object listenerLock = new object();
		private TcpListener? listener = null;

		public ISimpleLog? Log { get; set; }

		public bool Running {
			get
			{
				lock (listenerLock)
				{
					return listener != null;
				}
			}
			set
			{
				lock (listenerLock)
				{
					if (value)
					{
						if (listener != null) return;
						listener = new TcpListener(System.Net.IPAddress.Loopback, port);
						Log?.Write($"Start TCP listening on {port}");
						listener.Start();
						listener.BeginAcceptTcpClient(OnNewClientHandshake, null);
					}
					else
					{
						if (listener == null) return;
						Log?.Write("Stopping TCP listening");
						listener.Stop();
						listener = null;
					}
				}
			}
		}

		private ushort port = ServerPortConfig.DefaultPort;
		public ushort Port {
			get => port;
			set
			{
				if (port == value) return;
				if (Running) throw new InvalidOperationException("You cannot change the port while the server is running");
				port = value;
			}
		}

		public abstract class Client {
			public abstract void Close();
			public event EventHandler? OnClosed;

			public int Port { get; protected set; } = 0;
			public ISimpleLog? Log { get; set; }

			public event EventHandler<byte[]>? OnDataMessageReceived;

			public event EventHandler<string>? OnTextMessageReceived;

			public abstract void SendText(string text);

			public abstract void SendData(ReadOnlySpan<byte> data);

			protected void FireOnClosed()
			{
				OnClosed?.Invoke(this, EventArgs.Empty);
			}

			protected void FireOnDataMessageReceived(byte[] data)
			{
				OnDataMessageReceived?.Invoke(this, data);
			}

			protected void FireOnTextMessageReceived(string msg)
			{
				OnTextMessageReceived?.Invoke(this, msg);
			}
		}

		private class PreHandshakeClient : Client
		{
			protected TcpClient client;
			private byte[] handshakeData = new byte[3];
			public event EventHandler<Func<Client>>? OnHandshakeComplete;

			public int ListeningPort { get; set; }

			public PreHandshakeClient(TcpClient client, int listeningPort, ISimpleLog? log = null, int readTimeoutMs = 3000)
			{
				Log = log;
				this.client = client;
				Port = ((IPEndPoint?)(client.Client?.RemoteEndPoint))?.Port ?? 0;
				ListeningPort = listeningPort;
				Log?.Write($"Incoming connection {Port}");
				var stream = client.GetStream();
				stream.ReadTimeout = readTimeoutMs;
				Task readTask = stream.ReadExactlyAsync(handshakeData).AsTask();
				readTask.ContinueWith(HandshakeStartReceived);
			}

			public override void Close()
			{
				client.Close();
				FireOnClosed();
			}

			private void HandshakeStartReceived(Task readTask)
			{
				if (!readTask.IsCompletedSuccessfully)
				{
					Log?.Write($"Failed to receive handshake for {Port}");
					Close();
					return;
				}

				string handshakeMarker = Encoding.UTF8.GetString(handshakeData, 0, 3);
				if (handshakeMarker.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
				{
					// try WebSocket handshake
					// https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
					// reading until reaching "\r\n\r\n"
					Task.Run(() =>
					{
						int termState = 0;
						List<byte> data = new(handshakeData);
						while (termState < 4)
						{
							int b = client.GetStream().ReadByte();
							if (b < 0 || b > 255) return Task.FromException(new EndOfStreamException());
							data.Add((byte)b);
							if (((termState == 0 || termState == 2) && b == '\r')
								|| ((termState == 1 || termState == 3) && b == '\n'))
							{
								termState++;
							}
							else
							{
								termState = 0;
							}
						}

						handshakeData = data.ToArray();
						return Task.CompletedTask;
					}).ContinueWith(HttpHeaderReceived);

				}
				else if (handshakeMarker.Equals("SGR"))
				{
					// custom app TCP handshake
					if (OnHandshakeComplete != null)
					{
						OnHandshakeComplete(this, () => new AppTcpClient(client, Log));
					}
					else
					{
						Log?.Write("Cannot complete handshake, as now acceptor callback is set");
						Close();
					}
				}
				else
				{
					// unknown data
					Log?.Write(ISimpleLog.FlagError, "First data did not match a known handshake");
					Close();
				}

			}

			private void HttpHeaderReceived(Task readTask)
			{
				if (!readTask.IsCompletedSuccessfully)
				{
					Log?.Write($"Failed to receive http handshake for {Port}");
					// here, we cannot be sure if it was a valid http request, so we don't send a http error response
					Close();
					return;
				}

				var SendErrorAndClose = (string code = "400 Bad Request") =>
				{
					var stream = client.GetStream();
					stream.ReadTimeout = Timeout.Infinite;
					stream.WriteTimeout = Timeout.Infinite;
					stream.Write(Encoding.UTF8.GetBytes($"HTTP/1.1 {code}\r\n\r\n"));
					Close();
				};

				string httpRequest = Encoding.UTF8.GetString(handshakeData);
				//	Edge:
				//
				//	GET / HTTP/1.1
				//	Host: 127.0.0.1:18245
				//	Connection: Upgrade
				//	Pragma: no-cache
				//	Cache-Control: no-cache
				//	User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0
				//	Upgrade: websocket
				//	Origin: null
				//	Sec-WebSocket-Version: 13
				//	Accept-Encoding: gzip, deflate, br
				//	Accept-Language: de,de-DE;q=0.9,en;q=0.8,en-US;q=0.7,en-GB;q=0.6
				//	Sec-WebSocket-Key: RYW37tzTtSM3oj38soRrsw==
				//	Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits
				//
				//	Firefox:
				//
				//	GET / HTTP/1.1
				//	Host: 127.0.0.1:18245
				//	User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:122.0) Gecko/20100101 Firefox/122.0
				//	Accept: */*
				//	Accept-Language: de,en-US;q=0.7,en;q=0.3
				//	Accept-Encoding: gzip, deflate, br
				//	Sec-WebSocket-Version: 13
				//	Origin: null
				//	Sec-WebSocket-Extensions: permessage-deflate
				//	Sec-WebSocket-Key: 6PwSdgvoAjNuzQuHkz+O9A==
				//	DNT: 1
				//	Connection: keep-alive, Upgrade
				//	Sec-Fetch-Dest: empty
				//	Sec-Fetch-Mode: websocket
				//	Sec-Fetch-Site: cross-site
				//	Pragma: no-cache
				//	Cache-Control: no-cache
				//	Upgrade: websocket

				// Check request fields
				var match = Regex.Match(httpRequest, @"GET\s+/\s+HTTP"); // GET root resource
				if (!match.Success)
				{
					Log?.Write($"Http handshake for {Port} failed: Requested resource is not root");
					SendErrorAndClose();
					return;
				}
				match = Regex.Match(httpRequest, $@"Host:\s+((127\.0\.0\.1)|localhost)\:{ListeningPort}", RegexOptions.IgnoreCase); // HOST'r'us
				if (!match.Success)
				{
					Log?.Write($"Http handshake for {Port} failed: Host mismatch");
					SendErrorAndClose();
					return;
				}
				match = Regex.Match(httpRequest, @"Upgrade:.+websocket", RegexOptions.IgnoreCase); // Upgrade to websocket
				if (!match.Success)
				{
					Log?.Write($"Http handshake for {Port} failed: Upgrade to websocket not found");
					SendErrorAndClose();
					return;
				}
				match = Regex.Match(httpRequest, @"Connection:.+Upgrade", RegexOptions.IgnoreCase); // Connection allowed to upgrade
				if (!match.Success)
				{
					Log?.Write($"Http handshake for {Port} failed: Connection upgrade not found");
					SendErrorAndClose();
					return;
				}
				match = Regex.Match(httpRequest, @"Sec-WebSocket-Version:\s+13"); // Websocket version 13
				if (!match.Success)
				{
					Log?.Write($"Http handshake for {Port} failed: Websocket version 13 not found");
					SendErrorAndClose();
					return;
				}

				// Prepare handshake response
				// 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
				var swkMatch = Regex.Match(httpRequest, "Sec-WebSocket-Key: (.*)");
				if (!swkMatch.Success)
				{
					Log?.Write($"Http handshake for {Port} failed: Sec-WebSocket-Key not found");
					SendErrorAndClose();
					return;
				}

				// 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
				// 3. Compute SHA-1 and Base64 hash of the new value
				// 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
				string swk = swkMatch.Groups[1].Value.Trim();
				string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
				byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
				string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

				// Build response to accept the websocket connection, without subprotocol, nor extensions
				// HTTP/1.1 defines the sequence CR LF as the end-of-line marker
				byte[] response = Encoding.UTF8.GetBytes(
					"HTTP/1.1 101 Switching Protocols\r\n" +
					"Connection: Upgrade\r\n" +
					"Upgrade: websocket\r\n" +
					"Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

				var stream = client.GetStream();
				stream.ReadTimeout = Timeout.Infinite;
				stream.Write(response);

				if (OnHandshakeComplete != null)
				{
					OnHandshakeComplete(this, ConvertToWebSocketClient);
				}
				else
				{
					Log?.Write("Cannot complete handshake, as now acceptor callback is set");
					SendErrorAndClose("500 Internal Server Error");
					Close();
				}
			}

			private class WebSocketClientImpl : WebSocketClient
			{
				public WebSocketClientImpl(TcpClient c) : base(c) { }
			}

			private Client ConvertToWebSocketClient()
			{
				return new WebSocketClientImpl(client) { Log = Log };
			}

			public override void SendText(string text)
			{
				throw new InvalidOperationException();
			}

			public override void SendData(ReadOnlySpan<byte> data)
			{
				throw new InvalidOperationException();
			}
		}

		public event EventHandler<Client>? OnNewClient;

		public event EventHandler<Client>? OnClientClosed;

		private List<Client> clients = new();

		public void CloseAllClients()
		{
			Client[] cs;
			lock (listenerLock)
			{
				cs = clients.ToArray();
				clients.Clear();
			}
			foreach (Client c in cs)
			{
				try
				{
					c.OnClosed -= Client_OnClosed;
					OnClientClosed?.Invoke(this, c);
					Log?.Write($"Unlisting client {c.Port}");
					c.Close();
				}
				catch { }
			}
			Log?.Write($"All clients closed.");
		}

		private void OnNewClientHandshake(IAsyncResult ar)
		{
			TcpClient? client = null;
			lock (listenerLock)
			{
				if (listener != null)
				{
					client = listener.EndAcceptTcpClient(ar);
					listener.BeginAcceptTcpClient(OnNewClientHandshake, null);
				}
			}
			if (client != null)
			{
				lock (listenerLock)
				{
					PreHandshakeClient c = new PreHandshakeClient(client, Port, Log);
					c.OnClosed += Client_OnClosed;
					c.OnHandshakeComplete += Client_OnHandshakeComplete;
					clients.Add(c);
					Log?.Write($"Listing client {c.Port}\tNow {clients.Count} clients listed.");
				}
			}
		}

		private void Client_OnClosed(object? sender, EventArgs e)
		{
			Client? clc = sender as Client;
			if (clc == null) return;

			lock (listenerLock)
			{
				clients.Remove(clc);
				Log?.Write($"Unlisting client {clc.Port}\tNow {clients.Count} clients listed.");
			}

			if (clc.GetType() != typeof(PreHandshakeClient))
			{
				OnClientClosed?.Invoke(this, clc);
			}
		}

		private void Client_OnHandshakeComplete(object? sender, Func<Client> e)
		{
			Client? clc = sender as Client;
			if (clc == null) return;

			Client nclc;
			lock (listenerLock)
			{
				clients.Remove(clc);
				clc.OnClosed -= Client_OnClosed;

				nclc = e();
				nclc.OnClosed += Client_OnClosed;

				clients.Add(nclc);
				Log?.Write($"Accepted client {nclc.Port} after handshake\tNow {clients.Count} clients listed.");
			}

			OnNewClient?.Invoke(this, nclc);
		}

	}
}
