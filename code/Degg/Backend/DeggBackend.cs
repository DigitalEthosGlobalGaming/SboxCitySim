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

		public EventPayload( string name, object data )
		{
			Name = name;
			Type = data;
			CallbackId = System.Guid.NewGuid().ToString();
		}


	}

	public partial class DeggBackend
	{
		public const string SocketUrl  = "ws://degg-sbox-01.hmvqhod896k22.ap-southeast-2.cs.amazonlightsail.com:8080/";

		public static Queue<EventPayload> EventQueue { get; set; } = new Queue<EventPayload>();

		public static WebSocket Socket { get; set; }
		public static void Initialise(string key, string secret)
		{
			SendEvent( "test", null );
		}

		public static bool CheckConnection()
		{
			return false; // This is extremely broken for me.
			/*
			if ( Socket == null )
			{
				Socket = new WebSocket();
			}

			Assert.False( ThreadSafe.IsMainThread );

			if ( !Socket.IsConnected )
			{
				Socket.Connect( SocketUrl );
			}

			return Socket.IsConnected;
			*/
		}

		public static void SendEvents()
		{
			return; // This is extremely broken for me.
			/*
			if ( CheckConnection() )
			{
				Assert.False( ThreadSafe.IsMainThread );

				while ( EventQueue.Count > 0 )
				{
					var e = EventQueue.Dequeue();
					var data = JsonSerializer.Serialize( e );
					Socket.Send( data );
				}
			}
			*/
		}
		public static void SendEvent(string name, object raw)
		{
			return; // This is extremely broken for me.
			/*
			Assert.False( ThreadSafe.IsMainThread );

			EventPayload ePayload = new EventPayload( name, raw );

			EventQueue.Enqueue( ePayload );
			SendEvents();
			*/
		}
	}
}
