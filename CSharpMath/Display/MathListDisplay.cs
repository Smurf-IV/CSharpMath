using CSharpMath.Atoms;
using Color = CSharpMath.Structures.Color;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;

namespace CSharpMath.Display {
  /// <summary>
  /// Corresponds to MTMathListDisplay in iosMath.
  /// </summary>
  public class ListDisplay<TFont, TGlyph>: IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    public IReadOnlyList<IDisplay<TFont, TGlyph>> Displays { get; }
    public Enumerations.LinePosition LinePosition { get; set; }
    public Color? TextColor { get; set; }
    public bool HasScript { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor = TextColor ?? textColor;
      foreach (var display in Displays)
        display.SetTextColorRecursive(textColor);
    }
    /// <summary>For a subscript or superscript, this is the index in the
    /// parent list. For a regular list, it is int.MinValue.</summary>
    public int IndexInParent { get; set; }

    public ListDisplay(IReadOnlyList<IDisplay<TFont, TGlyph>> displays) {
      Displays = displays;
      LinePosition = Enumerations.LinePosition.Regular;
      IndexInParent = int.MinValue;
    }

    public float Ascent => Displays.CollectionAscent();
    public float Descent => Displays.CollectionDescent();
    public PointF Position { get; set; }
    public RectangleF DisplayBounds => this.ComputeDisplayBounds();
    public Range Range => Range.Combine(Displays.Select(d => d.Range));
    public float Width => Displays.CollectionMaxX() - Displays.CollectionX();
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      context.Translate(this.Position);
      context.SetTextPosition(new PointF());
      foreach (var displayAtom in Displays)
        if(displayAtom != null)
          displayAtom.Draw(context);
      context.RestoreState();
    }
    /// <summary>The string returned is NOT real TeX! It's for debugging purposes only.</summary>
    public override string ToString() => string.Concat(Displays);
  }
}
