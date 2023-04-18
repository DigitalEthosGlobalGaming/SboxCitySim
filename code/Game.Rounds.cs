using Sandbox;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace CitySim
{

	public partial class GameOptions : BaseNetworkable
	{
		[Net]
		public int XSize { get; set; }
		[Net]
		public int YSize { get; set; }

		[Net]
		public MyGame.GameModes Mode { get; set; }
	}

	public partial class MyGame : GameManager
	{

		[Net]
		public float StartGameTimer { get; set; } = 0;

		[Net]
		public GameOptions CurrentGameOptions { get; set; }

		public static GameStateEnum GameState { get; set; }


		public enum GameModes
		{
			Normal = 0,
			Chaos = 1,
			Sandbox = 2,
		}

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

		[ConCmd.Server( "cs.game.start" )]
		public static void StartGameCmd( GameModes? mode = GameModes.Normal )
		{
			// Delete all Cars; if we are resetting.
			CleanupEntitiesCmd();

			var options = new GameOptions();
			options.Mode = mode ?? GameModes.Normal;
			var minAmount = 10;
			var maxAmount = 15;
			if ( mode == GameModes.Chaos )
			{
				minAmount = (int)(minAmount * 1.8f);
				maxAmount = (int)(maxAmount * 1.8f);
			}
			else if ( mode == GameModes.Sandbox )
			{
				minAmount = (int)(minAmount * 2f);
				maxAmount = (int)(maxAmount * 2f);
			}
			options.XSize = Game.Random.Int( minAmount, maxAmount );
			options.YSize = Game.Random.Int( minAmount, maxAmount );
			GameObject.StartGame( options );
		}
		[ConCmd.Server( "cs.game.restart" )]
		public static void TestRestartCmd()
		{
			GameObject.SetGameState( GameStateEnum.End );
			GameObject.SetGameState( GameStateEnum.Start );
		}

		[ConCmd.Server( "cs.game.restart_same_settings" )]
		public static void TestRestartSameSettingsCmd()
		{
			var options = GameObject.CurrentGameOptions;
			GameObject.EndGame();
			StartGameCmd( options.Mode );
		}

		[ConCmd.Server( "cs.game.end" )]
		public static void EndGameCmd()
		{
			GameObject.EndGame();
		}

		public void EndGame()
		{
			if ( GameState == GameStateEnum.Playing )
			{
				SetGameState( GameStateEnum.End );
			}
		}


		public void SetGameState( GameStateEnum state )
		{
			if ( Game.IsServer )
			{
				StartGameTimer = Time.Now + 10;
				GameState = state;
				Event.Run( "citysim.gamestate" );
				UpdateClientGameState( state );
			}
		}

		[ClientRpc]
		public void UpdateClientGameState( GameStateEnum state )
		{
			GameState = state;
			Event.Run( "citysim.gamestate" );
		}

		[ConCmd.Server]
		public static void VoteToStart( MyGame.GameModes mode = MyGame.GameModes.Normal )
		{
			StartGameCmd( mode );
		}


		public void StartGame( GameOptions options )
		{
			if ( Game.IsServer )
			{
				if ( GameState == GameStateEnum.Start || GameState == GameStateEnum.End || true )
				{

					SetGameState( GameStateEnum.WarmingUp );
					CurrentGameOptions = options;
					if ( CityAmbianceSystem != null )
					{
						CityAmbianceSystem = null;
					}

					if ( Map != null )
					{
						Map.Delete();
						Map = null;
					}


					if ( Map == null )
					{
						Map = new RoadMap();
						Map.Init( options.XSize, options.YSize );

						CityAmbianceSystem = new CityAmbianceSystem( Map );

						if ( options.Mode == GameModes.Chaos )
						{
							Map.TimeBetweenPieces = 1f;
							Map.TimeBetweenPiecesModifier = 0f;
						}
						else if ( options.Mode == GameModes.Sandbox )
						{
							Map.TimeBetweenPieces = -1f;
						}
					}

					foreach ( var client in Game.Clients )
					{
						var player = client.Pawn;
						if ( player is Pawn p )
						{

							p.Score = 0;
							p.PivotPoint = Map.Position;
						}
					}
				}
			}
		}
	}
}
