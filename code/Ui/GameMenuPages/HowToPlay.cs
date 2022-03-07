using Sandbox.UI;
using Sandbox;
using CitySim;
using System;
using System.Collections.Generic;

namespace GridSystem.Ui
{
	public partial class HowToPlay : Panel
	{
		public bool Opened { get; set; }

		public bool IsOpening { get; set; }

		public int Page { get; set; }

		public List<Panel> Pages {get;set;}


		public HowToPlay()
		{
			SetTemplate( "Ui/GameMenuPages/HowToPlay.html" );
			StyleSheet.Load( "Ui/GameMenuPages/HowToPlay.scss" );
			AddClass( "how-to-play" );
		}


	}
}
