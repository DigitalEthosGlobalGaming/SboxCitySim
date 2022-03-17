

using Sandbox;

namespace Degg.Util
{
	public partial class AdvLog
	{
		public static void ClientInfo(object message)
		{
			if (Game.Current.IsClient)
			{
				Log.Info( $"[CLIENT] {message}" );
			}
		}

		public static void ServerInfo( object message )
		{
			if ( Game.Current.IsClient )
			{
				Log.Info( $"[SERVER] {message}" );
			}
		}

		public static void Info( object message )
		{
			ClientInfo( message.ToString() );
			ServerInfo( message.ToString() );
		}
	}
}
