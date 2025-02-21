using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;
using TFonts = CSharpMath.Rendering.Fonts;
using Typography.OpenFont;
using Typography.TextLayout;

namespace CSharpMath.Rendering {
  public class GraphicsContext : IGraphicsContext<TFonts, Glyph> {
    public (Color glyph, Color textRun)? GlyphBoxColor { get; set; }
    public ICanvas Canvas { get; set; }

#warning HIGH PRIORITY: Remove (Must have a Mac to test)
    void IGraphicsContext<TFonts, Glyph>.SetTextPosition(PointF position) => Translate(position);

    public void DrawGlyphsAtPoints(ForEach<Glyph> glyphs, TFonts font, ForEach<PointF> points, Color? color) {
      foreach(var (glyph, point) in glyphs.Zip(points)) {
        if (GlyphBoxColor != null) {
          var rentedArray = new RentedArray<Glyph>(glyph);
          var rect = GlyphBoundsProvider.Instance.GetBoundingRectsForGlyphs(font, rentedArray.Result, 1).Single();
          Canvas.CurrentColor = GlyphBoxColor?.glyph;
          Canvas.StrokeRect(point.X + rect.X, point.Y + rect.Y, rect.Width, rect.Height);
          rentedArray.Return();
        }
        var typeface = glyph.Typeface;
        var scale = typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
        var pathBuilder = new GlyphPathBuilder(typeface);
        pathBuilder.BuildFromGlyph(glyph.Info, font.PointSize);
        Canvas.Save();
        Canvas.CurrentColor = color;
        Canvas.Translate(point.X, point.Y);
        pathBuilder.ReadShapes(Canvas.GetPath());
        Canvas.Restore();
      }
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness, Color? color) {
      Canvas.CurrentColor = color;
      Canvas.DrawLine(x1, y1, x2, y2, lineThickness);
    }

    public void DrawGlyphRunWithOffset(Display.Text.AttributedGlyphRun<TFonts, Glyph> run, PointF offset, Color? color) {
      var textPosition = offset;
      if (GlyphBoxColor != null) {
        Bounds bounds;
        float advance, scale, width = 0, ascent = 0, descent = 0;
        foreach (var (glyph, kernAfter, _) in run.GlyphInfos) {
          bounds = glyph.Info.Bounds;
          advance = glyph.Typeface.GetHAdvanceWidthFromGlyphIndex(glyph.Info.GlyphIndex);
          scale = glyph.Typeface.CalculateScaleToPixelFromPointSize(run.Font.PointSize);
          width += advance * scale + kernAfter;
          ascent = System.Math.Max(ascent, bounds.YMax * scale);
          descent = System.Math.Min(descent, bounds.YMin * scale);
        }
        Canvas.CurrentColor = GlyphBoxColor?.textRun;
        Canvas.StrokeRect(textPosition.X, textPosition.Y + descent, width, ascent - descent);
      }
      var pointSize = run.Font.PointSize;
      Canvas.Save();
      Canvas.Translate(textPosition.X, textPosition.Y);
      Canvas.CurrentColor = color;
      foreach(var (glyph, kernAfter, foreground) in run.GlyphInfos) {
        var typeface = glyph.Typeface;
        var pathBuilder = new GlyphPathBuilder(typeface);
        var scale = typeface.CalculateScaleToPixelFromPointSize(pointSize);
        var index = glyph.Info.GlyphIndex;
        pathBuilder.BuildFromGlyph(glyph.Info, pointSize);
        Canvas.CurrentColor = foreground ?? color;
        pathBuilder.ReadShapes(Canvas.GetPath());
        Canvas.Translate(typeface.GetHAdvanceWidthFromGlyphIndex(index) * scale + kernAfter, 0);
      }
      Canvas.Restore();
    }

    public void RestoreState() => Canvas.Restore();

    public void SaveState() => Canvas.Save();

    public void Translate(PointF dxy) => Canvas.Translate(dxy.X, dxy.Y);
  }
}
