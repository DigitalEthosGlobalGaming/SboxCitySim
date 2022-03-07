using Sandbox.UI;
using Sandbox;
using CitySim;
using System;
using System.Collections.Generic;

namespace GridSystem.Ui
{
	public partial class GameMenu : Panel
	{

		public Panel Base { get; set; }

		public bool Opened { get; set; }
		public int Page { get; set; }

		public float LastTick { get; set; }

		public List<Panel> Pages {get;set;}
		public FormattableString IsClient { get; private set; }

		public GameMenu()
		{
			SetTemplate( "Ui/GameMenu.html" );
			StyleSheet.Load( "Ui/GameMenu.scss" );
			AddClass( "game-menu" );
			Pages = new List<Panel>();
			Pages.Add( AddChild<HowToPlay>() );
			Pages.Add( AddChild<TestPage>() );
		}


		public void OpenMenu()
		{
			Pawn.SetControlsDisabledCmd( true );
		}

		public void NextPage()
		{
			SetPage( Page + 1 );
		}
		public void PreviousPage()
		{
			SetPage( Page - 1 );
		}
		public void SetPage(int pageNumber)
		{
			if ( pageNumber >= Pages.Count)
			{
				pageNumber = 0;
			} else if ( pageNumber < 0)
			{
				pageNumber = Pages.Count - 1;
			}

			Page = pageNumber;


			for ( int i = 0; i < Pages.Count; i++ )
			{
				var page = Pages[i];
				if (page != null)
				{
					if ( i == pageNumber )
					{
						page.RemoveClass( "hidden" );
					} else {
						page.AddClass( "hidden" );
					}
				}
			}

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

			if (Input.Pressed(InputButton.SlotNext))
			{
				NextPage();
			} else if ( Input.Pressed( InputButton.SlotPrev ) )
			{
				PreviousPage();
			}

			SetClass( "open", Opened );

			/*
			var values = Enum.GetValues( typeof(InputButton) );
			foreach(var i in values)
			{
				if (Input.Pressed( (InputButton)i) )
				{
					Log.Info( i );
				}
			}
			*/
			
		}

	}
}
