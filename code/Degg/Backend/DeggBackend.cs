using Sandbox;
using System.Collections.Generic;
using System.Text.Json;



namespace Degg.Backend
{

	public class EventPayload
	{
		public string Name { get; set; }
		public object Type { get; set; }
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
		public const string SocketUrl  =  "ws://localhost:8080";

		public static Queue<EventPayload> EventQueue { get; set; } = new Queue<EventPayload>();

		public static WebSocket Socket { get; set; }
		public static void Initialise(string key, string secret)
		{
			CheckConnection();
			SendEvent( "test", null );
		}

		public static void CheckConnection()
		{
			if ( Socket == null )
			{
				var socket = new WebSocket();
				Socket = socket;
			}
			if ( !Socket.IsConnected )
			{
				Socket.Connect( SocketUrl );
			}
		}

		public static void SendEvents()
		{
			CheckConnection();
			if ( Socket.IsConnected )
			{
				while ( EventQueue.Count > 0 )
				{
					var e = EventQueue.Dequeue();
					var data = JsonSerializer.Serialize( e );
					Socket.Send( data );
				}
			}
		}
		public static void SendEvent(string name, object raw)
		{
			EventPayload ePayload = new EventPayload( name, raw );

			EventQueue.Enqueue( ePayload );
			SendEvents();
		}




	}
}
