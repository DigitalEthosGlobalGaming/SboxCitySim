using Sandbox.UI;
namespace Degg.UI.Forms
{
	public partial class FormElement : Panel
	{
		public bool ControllerFocused { get; set; } = false;
		public FormElement()
		{

		}

		public virtual void SetControllerFocus(bool focus)
		{
			if ( focus && !ControllerFocused )
			{
				OnControllerFocus();
			} else if ( !focus && ControllerFocused )
			{
				OnControllerUnFocus();
			}

		}
		public virtual void OnControllerFocus()
		{
			ControllerFocused = true;
		}

		public virtual void OnControllerUnFocus()
		{
			ControllerFocused = false;
		}
	}



}
