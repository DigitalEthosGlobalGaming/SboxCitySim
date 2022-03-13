using Sandbox;
using System.Text.Json;



namespace Degg.Backend
{

	class EventPayload
	{
		public string Name { get; set; }
		public object? Type { get; set; }
		public string CallbackId { get; set; }

		public EventPayload( string name, object? data )
		{
			Name = name;
			Type = data;
			CallbackId = System.Guid.NewGuid().ToString();
		}


	}

	public partial class DeggBackend
	{
		public const string SocketUrl  =  "wss://localhost:8080";

		public static WebSocket Socket { get; set; }
		public static void Initialise(string key, string secret)
		{
			var socket = new WebSocket();
			socket.Connect( SocketUrl );
			Socket = socket;
			SendEvent( "test", null );
		}

		public static void SendEvent(string name, object? raw)
		{
			EventPayload ePayload = new EventPayload( name, raw);
			var data = JsonSerializer.Serialize( ePayload );
			Socket.Send( data );
		}




	}
}
