using Sandbox.UI;
using Sandbox;
using CitySim;
using System;

namespace GridSystem.Ui
{
	public partial class StartScreen: Panel
	{

		public Panel Base { get; set; }

		public Button StartButton { get; set; }

		public StartScreen()
		{
			SetTemplate( "Ui/StartScreen.html" );
			StyleSheet.Load( "Ui/StartScreen.scss" );
			AddClass("start-screen");
		}

		public override void Tick()
		{

			SetClass( "open", Input.Down( InputButton.Score ) );
			if ( Input.Down( InputButton.Jump ) )
			{
				VoteToStart();
			}
		}

		public void VoteToStart()
		{
			MyGame.VoteToStart();
		}

	}
}
