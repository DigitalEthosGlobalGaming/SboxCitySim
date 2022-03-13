using CitySim.UI;
using CitySim.Utils;
using Degg.Analytics;
using Sandbox;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace CitySim
{
	[Library( "citysim" ), Hammer.Skip]
	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// </summary>
	public partial class MyGame : Sandbox.Game
	{
		public static MyGame GameObject { get; set; }
		public static GameUi Ui { get; set; } = null;

		public static RoadMap GetMap()
		{
			return GameObject.Map;
		}

		[Net]
		public RoadMap Map { get; set; }
		public MyGame()
		{
			DeggBackend.ConfigureBuild( "1.0.0" );
			SetupAnalytics();

			GameObject = this;
			CreateUi();
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			DeggBackend.InitialisePlayer( client.PlayerId.ToString() );

			// Create a pawn for this client to play with
			var pawn = new Pawn();
			client.Pawn = pawn;

			if (Map != null)
			{
				pawn.Position = Map.Position + (Vector3.Up * 100f);
			}
			UpdateClientGameState( GameState );
		}

		[ServerCmd( "cs.debug.cleanupEntities" )]
		public static void CleanupEntitiesCmd()
		{
			foreach ( var entity in Entity.All )
			{
				if ( entity is MovementEntity )
				{
					entity.DeleteAsync( 0 );
				}
			}

			CleanupWorldUIRpc();
		}
		[ClientCmd("cs.debug.cleanupWorldUI")]
		public static void CleanupWorldUICmd()
		{
			CleanupWorldUIRpc();
		}

		[ClientRpc]
		public static void CleanupWorldUIRpc()
		{
			foreach ( var ui in WorldTileStatUI.All )
			{
				ui.Delete();
			}
		}

#if DEBUG && !RELEASE
		[ServerCmd( "cs.test.updateMap" )]
		public static void TestServerCmd()
		{
			((MyGame)Current).RefreshMap();
		}
#endif


		public void CreateUi()
		{
			if ( IsClient )
			{
				if (Ui != null)
				{
					Ui.Delete();
				}

			
				Ui = new GameUi();
			}

		}

		[Event.Hotload]
		public void OnLoad()
		{
			if ( IsServer )
			{
				SetupAnalytics();
			}

			CreateUi();
		}


		public void RefreshMap()
		{
			if ( Map != null )
			{
				foreach(var item in Map.Grid)
				{
					((GenericTile)item).CheckModel();
				}
			}
			else
			{
				Log.Info( "NO MAP" );
			}
		}

		public void ClearMap()
		{

		}

		[Event.Tick.Client]
		public void ClientTick()
		{
			if ( TickableCollection.Global != null )
			{
				TickableCollection.Global.ClientTick();
			}

			if ( Map != null )
			{
				Map.ClientTick();
			}
		}
		[Event.Tick.Server]
		public void ServerTick()
		{
			if ( TickableCollection.Global != null)
			{
				TickableCollection.Global.ServerTick();
			}

			if ( Map != null )
			{
				Map.ServerTick();
			}
			if ( GameState == GameStateEnum.End && StartGameTimer < Time.Now)
			{
				SetGameState( GameStateEnum.Start );
			}
		}

		[Event.Tick]
		public void SharedTick()
		{
			if ( TickableCollection.Global != null )
			{
				TickableCollection.Global.SharedTick();
			}
		}

	}

}
