using Sandbox.UI;
using Sandbox;
using CitySim;
using System;
using System.Collections.Generic;
using Degg.Ui.Elements;

namespace GridSystem.Ui
{
	public partial class GameMenu : Panel
	{
		public Panel Base { get; set; }

		public bool Opened { get; set; }
		public int Page { get; set; }

		public float LastTick { get; set; }

		public List<Panel> Pages {get;set;}
		public Panel NavBar { get; set; }
		public FormattableString IsClient { get; private set; }

		public GameMenu()
		{
			SetTemplate( "Ui/GameMenu.html" );
			StyleSheet.Load( "Ui/GameMenu.scss" );
			AddClass( "game-menu" );
			Pages = new List<Panel>();

			Opened = true;

			var nav = NavBar.AddChild<NavPanel>();
			nav.AddPage<HowToPlay>("Welcome");
			nav.AddPage<TestPage>("Test");
		}



		public void OpenMenu()
		{
			Pawn.SetControlsDisabledCmd( true );
		}

		public void CloseMenu()
		{
			Pawn.SetControlsDisabledCmd(false);
		}
		public override void Tick()
		{
			if ( LastTick == Time.Tick ) {
				return;
			}
			LastTick = Time.Tick;

			if ( Input.Pressed( (InputButton.Menu) ) )
			{
				Opened = !Opened;
				if ( Opened )
				{
					OpenMenu();
				}
				else
				{
					CloseMenu();
				}
			}

			SetClass( "open", Opened );

			var values = Enum.GetValues( typeof(InputButton) );
			foreach(var i in values)
			{
				if (Input.Pressed( (InputButton)i) )
				{
					Log.Info( i );
				}
			}
			
		}

	}
}
