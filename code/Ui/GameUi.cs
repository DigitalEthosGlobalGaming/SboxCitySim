using CitySim;
using Sandbox;
using Sandbox.UI;


namespace GridSystem.Ui
{
	public partial class GameUi : HudEntity<RootPanel>
	{

		public Main MainElement { get; set; }
		public StartScreen StartScreenElement { get; set; }
		public EndScreen EndScreenElement { get; set; }

		public LoadingScreen LoadingScreenElement { get; set; }

		public GameUi()
		{
			OnGameStateChange();
			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<CustomScoreboard>();
			RootPanel.AddChild<GameMenu>();
		}


		[Event( "citysim.gamestate" )]
		public void OnGameStateChange()
		{
			var state = MyGame.GameState;

			StartScreenElement?.Delete();
			MainElement?.Delete();
			EndScreenElement?.Delete();
			LoadingScreenElement?.Delete();
			if ( state == MyGame.GameStateEnum.Start )
			{
				StartScreenElement = RootPanel.AddChild<StartScreen>();
			}
			else if ( state == MyGame.GameStateEnum.Playing )
			{
				MainElement = RootPanel.AddChild<Main>();
			}
			else if ( state == MyGame.GameStateEnum.End )
			{
				EndScreenElement = RootPanel.AddChild<EndScreen>();
			}
			else if ( state == MyGame.GameStateEnum.WarmingUp )
			{
				LoadingScreenElement = RootPanel.AddChild<LoadingScreen>();
			}
		}
	}
}
