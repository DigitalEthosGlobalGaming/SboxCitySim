using Sandbox;
using System.Text.Json.Serialization;



namespace Degg.Backend
{
	public partial class DeggBackend
	{
		public const string SocketUrl  =  "wss://localhost:8080";

		public static WebSocket Socket { get; set; }
		public static void Initialise(string key, string secret)
		{
			var socket = new WebSocket();
			socket.Connect( SocketUrl );
			Socket = socket;
		}

		public void SendEvent(string name, JsonSerialisable data)
		{
			Seriali
		}




	}
}
