
using GridSystem.Ui;
using Sandbox;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace CitySim
{

	public class GameOptions
	{
		public int XSize { get; set; }
		public int YSize { get; set; }
	}
	public partial class MyGame : Sandbox.Game
	{

		public static GameOptions CurrentGameOptions { get; set; }

		public static GameStateEnum GameState { get; set; }

		public enum GameStateEnum
		{
			Start = 0,
			WarmingUp = 1,
			Playing = 2,
			End = 3,
		}

		[Event( "citysim.map_setup" )]
		public void OnMapSetup( RoadMap map )
		{
			SetGameState( GameStateEnum.Playing );
		}

		[ServerCmd( "cs.game.start" )]
		public static void StartGameCmd()
		{
			// Delete all Cars; if we are resetting.
			CleanupEntitiesCmd();

			var options = new GameOptions();
			options.XSize = Rand.Int(10, 20);
			options.YSize = Rand.Int(10, 20);
			GameObject.StartGame( options );
		}
		[ServerCmd( "cs.game.restart" )]
		public static void TestRestartCmd()
		{
			MyGame.EndGameCmd();
			MyGame.StartGameCmd();
		}

		[ServerCmd( "cs.game.end" )]
		public static void EndGameCmd()
		{
			GameObject.EndGame();
		}

		public void EndGame()
		{
			if (GameState == GameStateEnum.Playing)
			{
				SetGameState( GameStateEnum.End );				
			}
		}


		public void SetGameState( GameStateEnum state)
		{
			if (IsServer)
			{
				GameState = state;
				Event.Run( "citysim.gamestate" );
				UpdateClientGameState(state);
			}
		}

		[ClientRpc]
		public void UpdateClientGameState(GameStateEnum state)
		{
			GameState = state;
			Event.Run( "citysim.gamestate" );
		}

		[ServerCmd]
		public static void VoteToStart()
		{
			StartGameCmd();
		}

		public void StartGame(GameOptions options)
		{
			if ( IsServer )
			{
				if ( GameState == GameStateEnum.Start || GameState == GameStateEnum.End || true )
				{

					SetGameState( GameStateEnum.WarmingUp );
					CurrentGameOptions = options;
					if ( Map != null )
					{
						Map.Delete();
						Map = null;
					}


					if ( Map == null )
					{
						Map = new RoadMap();
						Map.Init( options.XSize, options.YSize );
					}

					foreach ( var client in Client.All )
					{
						var player = client.Pawn;
						if ( player is Pawn )
						{
							player.Position = Map.Position + (Vector3.Up * 1000f);
						}
					}
				}
			}
		}		
	}
}
