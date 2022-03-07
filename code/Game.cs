#define TEST

using CitySim.UI;
using GridSystem.Ui;
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

		public const bool IsDevelopment = true;
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
			GameObject = this;
			CreateUi();
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			// Create a pawn for this client to play with
			var pawn = new Pawn();
			client.Pawn = pawn;

			if (Map != null)
			{
				pawn.Position = Map.Position + (Vector3.Up * 500f);
			}
			UpdateClientGameState( GameState );
		}

#if DEBUG && !RELEASE
		[ServerCmd( "check_models" )]
		public static void TestServerCmd()
		{
			((MyGame)Current).RefreshMap();
		}
		[ClientCmd( "cs.test.worldUI" )]
		public static void TestWorldUICmd()
		{
			// Test the WorldTileStateUI here.
			WorldTileStatUI WorldUI = new WorldTileStatUI();
			Assert.NotNull( WorldUI );
			WorldUI.Name = "Test World UI";
			WorldUI.Points = 500;
			WorldUI.Position = Local.Pawn.Position;
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
				//this.RefreshMap();
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
