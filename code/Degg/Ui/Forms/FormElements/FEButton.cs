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

		public bool IsCentre { get; set; } = false;
		public Label Label { get; set; }
		public FEButton(): base()
		{
			StyleSheet.Load( "/Degg/Ui/Styles/button.scss" );
			AddClass( "button" );
			Label = AddChild<Label>();
			AddEventListener( "onclick", () =>
			 {
				 CreateEvent( "onpress", null );
			 });
		}


		public void SetCenter(bool isCentre)
		{
			IsCentre = isCentre;
			SetClass("button-centre", isCentre );
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
