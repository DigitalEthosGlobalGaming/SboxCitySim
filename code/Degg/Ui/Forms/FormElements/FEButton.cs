using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Degg.UI.Forms.Elements
{
	public partial class FEButton : FormElement
	{

		public Label Label { get; set; }
		public FEButton(): base()
		{
			AddClass( "button" );
			Label = AddChild<Label>();
			AddEventListener( "onclick", () =>
			 {
				 CreateEvent( "onpress", null );
			 });
		}

		public override void OnControllerFocus()
		{
			base.OnControllerFocus();
			AddClass( "focus" );
		}

		public override void OnControllerUnFocus()
		{
			base.OnControllerUnFocus();
			RemoveClass( "focus" );
		}
	}



}
