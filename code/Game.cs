using CitySim.UI;
using CitySim.Utils;
using Degg.Analytics;
using Degg.Util;
using Sandbox;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace CitySim
{
	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// </summary>
	public partial class MyGame : GameManager
	{
		public static MyGame GameObject { get; set; }
		public static GameUi Ui { get; set; } = null;

		[Net]
		public RoadMap Map { get; set; }

		public CityAmbianceSystem CityAmbianceSystem { get; set; }

		public MyGame()
		{
			SetupAnalytics();

			GameObject = this;
			if ( Game.IsServer )
			{
				GameAnalytics.TriggerEvent( null, "game_start" );
			}
			CreateUi();
		}

		public static RoadMap GetMap()
		{
			return GameObject.Map;
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( IClient client )
		{
			base.ClientJoined( client );

			// Create a pawn for this client to play with
			var pawn = new Pawn();
			client.Pawn = pawn;

			if ( Game.IsServer )
			{
				GameAnalytics.TriggerEvent( client.SteamId.ToString(), "game_join" );
			}

			if ( Map != null )
			{
				pawn.Position = Map.Position;
				pawn.PivotPoint = Map.Position;
			}
			UpdateClientGameState( GameState );
		}

		public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );
			GameAnalytics.TriggerEvent( cl.SteamId.ToString(), "game_disconnect", (int)reason );
		}

		[ConCmd.Server( "cs.debug.cleanupEntities" )]
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
		[ConCmd.Client( "cs.debug.cleanupWorldUI" )]
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


		public void CreateUi()
		{
			if ( Game.IsClient )
			{
				if ( Ui != null )
				{
					Ui.Delete();
				}

				Ui = new GameUi();
			}

		}

		[Event.Hotload]
		public void OnLoad()
		{
			if ( Game.IsServer )
			{
				SetupAnalytics();
			}

			CreateUi();
		}


		public void RefreshMap()
		{
			if ( Map != null )
			{
				foreach ( var item in Map.Grid )
				{
					// ((GenericTile)item).CheckModel();
				}
			}
			else
			{
				AdvLog.Info( "NO MAP" );
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
			if ( TickableCollection.Global != null )
			{
				TickableCollection.Global.ServerTick();
			}

			if ( CityAmbianceSystem != null )
			{
				CityAmbianceSystem.Update();
			}

			if ( Map != null )
			{
				Map.ServerTick();
			}
			if ( GameState == GameStateEnum.End && StartGameTimer < Time.Now )
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
