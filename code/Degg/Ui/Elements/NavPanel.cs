using Sandbox.UI;
using Sandbox;
using System.Collections.Generic;

namespace Degg.Ui.Elements
{
	public partial class NavPanel : Panel
	{
		public Panel Base { get; set; }
		public Panel NavBar { get; set; }
		public Panel CurrentPage { get; set; }

		public int Page { get; set; }

		public float LastTick { get; set; }

		public List<Panel> Pages {get;set;}
		public List<string> PageNames { get; set; }

		public NavPanel()
		{
			SetTemplate( "/Degg/Ui/Elements/NavPanel.html" );
			StyleSheet.Load( "/Degg/Ui/Elements/NavPanel.scss" );
			AddClass( "nav-panel" );

			Pages = new List<Panel>();
			PageNames = new List<string>();
			SetPage( 0 );
		}

		public void AddPage<T>(string name ) where T : Panel, new()
		{
			Pages.Add( AddChild<T>());
			PageNames.Add( name );
			if (NavBar == null)
			{
				NavBar = AddChild<Panel>();
			}

			var button = NavBar.AddChild<Button>();
			button.Text = name;
			button.AddClass( "button" );
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
						CurrentPage.RemoveClass( "hidden" );
					} else {
						CurrentPage.AddClass( "hidden" );
					}
				}
			}

		}
		public override void Tick()
		{
			if ( LastTick == Time.Tick ) {
				return;
			}
			LastTick = Time.Tick;

			if (Input.Pressed(InputButton.SlotNext))
			{
				NextPage();
			} else if ( Input.Pressed( InputButton.SlotPrev ) )
			{
				PreviousPage();
			}
			
		}

	}
}
