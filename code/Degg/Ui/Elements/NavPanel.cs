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
		public List<Button> PageNames { get; set; }

		public NavPanel()
		{
			SetTemplate( "/Degg/Ui/Elements/NavPanel.html" );
			StyleSheet.Load( "/Degg/Ui/Elements/NavPanel.scss" );
			AddClass( "nav-panel" );

			Pages = new List<Panel>();
			PageNames = new List<Button>();
			SetPage( 0 );
		}

		public void AddPage<T>(string name ) where T : Panel, new()
		{
			var currentCount = Pages.Count;
			var page = AddChild<T>();
			Pages.Add( page );
			page.AddClass( "hidden" );		

			if (NavBar == null)
			{
				NavBar = AddChild<Panel>();
			}

			var button = NavBar.AddChild<Button>();
			PageNames.Add( button );
			button.Text = name;

			button.AddEventListener( "onclick", () =>
			 {
				 SetPage( currentCount );
			 } );
			button.AddClass( "button" );

			SetPage( Page );
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
				var pageButton = PageNames[i];
				if (page != null)
				{
					if ( i == pageNumber )
					{
						pageButton.AddClass( "active" );
						page.RemoveClass( "hidden" );
					} else {
						pageButton.RemoveClass( "active" );
						page.AddClass( "hidden" );
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
