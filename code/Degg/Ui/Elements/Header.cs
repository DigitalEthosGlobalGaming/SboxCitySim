using Sandbox.UI;
using Sandbox;
using System.Collections.Generic;

namespace Degg.UI.Elements
{
	public partial class Header : Panel
	{
		public Label Text { get; set; }

		public int HeaderSize = 1;
				
		
		public Header()
		{
			StyleSheet.Load( "Degg/Ui/Elements/Header.scss" );
			AddClass( "header" );
			AddClass( "header-1" );
			Text = AddChild<Label>();
			Text.AddClass( "header-label" );
		}

		public void SetText(string text, int size = 1)
		{

			RemoveClass( $"header-{HeaderSize}" );
			size = (int) MathX.Clamp(size, 1, 5 );
			if (size > 5)
			{
				size = 5;
			}

			HeaderSize = size;

			Text.SetText( text );

			AddClass( $"header-{HeaderSize}" );
		}
	}
}
