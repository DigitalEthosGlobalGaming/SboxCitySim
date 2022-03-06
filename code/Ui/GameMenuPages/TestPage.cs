using Sandbox.UI;
using Sandbox;
using CitySim;
using System;
using System.Collections.Generic;

namespace GridSystem.Ui
{
	public partial class TestPage : Panel
	{

		public Panel Base { get; set; }

		public bool Opened { get; set; }

		public bool IsOpening { get; set; }

		public int Page { get; set; }

		public List<Panel> Pages {get;set;}


		public TestPage()
		{
			SetTemplate( "Ui/GameMenuPages/TestPage.html" );
			StyleSheet.Load( "Ui/GameMenuPages/TestPage.scss" );
			AddClass( "how-to-play" );
		}



	}
}
