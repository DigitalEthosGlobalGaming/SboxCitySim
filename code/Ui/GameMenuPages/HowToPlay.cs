using Degg.UI;
using Degg.Util;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace CitySim.UI
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
			var button = this.AddChild<ButtonGlyph>();

			button.SetIcon( AdvInput.InputButton( InputButton.Flashlight, InputButton.Menu ), "Toggle Menu" );
		}


	}
}
