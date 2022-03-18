using Sandbox;
using Degg.Util;
using Sandbox.UI;

namespace Degg.UI
{
	public class ButtonGlyph : Panel
	{
		private Image IconBtn;

		// Configuration for the Input Glyphs.
		private const InputGlyphSize inputGlyphSize = InputGlyphSize.Small;
		private GlyphStyle glyphStyle = GlyphStyle.Knockout;

		public ButtonGlyph()
		{
			StyleSheet.Load( "/Degg/Ui/Elements/ButtonGlyph.scss" );
			this.AddClass( "button-glyph" );
		}

		public void SetIcon(InputButton icon, string text)
		{
			CreateButtonGlyph( icon, ref IconBtn, text );
		}

		private void CreateButtonGlyph( InputButton _inputBtn, ref Image _element, string _label )
		{
			var texture = Input.GetGlyph( _inputBtn, inputGlyphSize, glyphStyle );
			this.DeleteChildren();

			var placeBtnPanel = this.AddChild<Panel>();
			placeBtnPanel.AddClass( "button-prompt" );

			_element = placeBtnPanel.AddChild<Image>();
			_element.AddClass( "button-prompt-image" );
			_element.Texture = texture;

			var placeBtnLabel = placeBtnPanel.AddChild<Label>();
			placeBtnLabel.AddClass( "button-prompt-label" );
			placeBtnLabel.Text = _label;
		}
	}
}
