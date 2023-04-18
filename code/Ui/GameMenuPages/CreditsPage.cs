using Sandbox.UI;
using System.Collections.Generic;

namespace CitySim.UI
{
	public partial class CreditsPage : Panel
	{


		public bool Opened { get; set; }

		public bool IsOpening { get; set; }

		public int Page { get; set; }

		public List<Panel> Pages {get;set;}


		public CreditsPage()
		{
			SetTemplate( "Ui/GameMenuPages/CreditsPage.html" );
			StyleSheet.Load( "Ui/GameMenuPages/CreditsPage.scss" );
			AddClass( "main" );
		}



	}
}
