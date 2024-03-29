﻿using Sandbox.UI;
using Sandbox;
using System;
using Degg.UI.Elements;
using Degg.Util;

namespace CitySim.UI
{
	public partial class GameMenu : Panel
	{
		public Panel Base { get; set; }
		public bool Opened { get; set; }
		public float LastTick { get; set; }

		public Panel NavBar { get; set; }
		public FormattableString IsClient { get; private set; }

		public GameMenu()
		{
			SetTemplate( "Ui/GameMenu.html" );
			StyleSheet.Load( "Ui/GameMenu.scss" );
			AddClass( "game-menu" );
			Opened = false;			

			var nav = NavBar.AddChild<NavPanel>();
			nav.AddPage<HowToPlay>("Welcome");
			nav.AddPage<CreditsPage>("Credits");
		}



		public void OpenMenu()
		{
			Pawn.SetControlsDisabledCmd( true );
			Opened = true;
		}

		public void CloseMenu()
		{
			Pawn.GetClientPawn().HasReadWelcome = true;
			Pawn.SetControlsDisabledCmd(false);
			Opened = false;
		}
		public override void Tick()
		{
			if ( LastTick == Time.Tick ) {
				return;
			}
			LastTick = Time.Tick;

			if ( AdvInput.Pressed(InputButton.Flashlight, InputButton.Menu) )
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
		}

	}
}
