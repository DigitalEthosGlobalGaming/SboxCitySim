using Sandbox.UI;

namespace Degg.UI.Elements
{
	public partial class Center : Panel
	{		
		public Center()
		{
			StyleSheet.Load( "Degg/Ui/Elements/Center.scss" );
			AddClass( "center" );		
		}
	}
}
