using Degg.UI.Forms;
using Degg.Util;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Degg.UI.Forms
{
	public partial class Form: Panel
	{

		public FormElement FocusedElement { get; set; }

		public float LastTick { get; set; }

		public int FocusedIndex { get; set; }
		public Form()
		{
			StyleSheet.Load( "/Degg/Ui/Forms/Form.scss" );
			AddClass( "form" );
		}

		public void Navigate(int amount)
		{
			var children = new List<FormElement>( ChildrenOfType<FormElement>() );
			
			if ( children.Count <= 0)
			{
				return;
			}

			FocusedIndex = FocusedIndex + amount;
			if ( FocusedIndex >= children.Count)
			{
				FocusedIndex = 0;
			} else if ( FocusedIndex < 0)
			{
				FocusedIndex = children.Count - 1;
			}

			if ( FocusedElement != null)
			{
				FocusedElement.SetControllerFocus( false );
			}

			FocusedElement = children[FocusedIndex];

			FocusedElement.SetControllerFocus( true );
		}

		public void NavigateUp()
		{
			Navigate(  1 );
		}

		public void NavigateDown()
		{
			Navigate( -1 );
		}


		public override void Tick()
		{
			if ( LastTick == Time.Tick)
			{
				return;
			}
			LastTick = Time.Tick;
			if (AdvInput.Pressed(InputButton.Forward, InputButton.Slot1))
			{
				NavigateUp();
			} else if ( AdvInput.Pressed( InputButton.Back, InputButton.Slot3 ) ) {
				NavigateDown();
			}

			if (AdvInput.Pressed(InputButton.Jump, InputButton.Jump) ) {
				if (FocusedElement != null)
				{
					FocusedElement.CreateEvent( "onpress", null );
				}
			}
		}
	}



}
