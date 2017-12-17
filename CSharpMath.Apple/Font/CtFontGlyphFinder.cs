﻿
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using CoreText;

namespace CSharpMath.Apple {
  public class CtFontGlyphFinder : IGlyphFinder<TGlyph> {
    private readonly CTFont _ctFont;

    public CtFontGlyphFinder(CTFont ctFontPointSizeIrrelevant) {
      _ctFont = ctFontPointSizeIrrelevant;
    }
    private IEnumerable<TGlyph> ToUintEnumerable(byte[] bytes) {
      for (int i = 0; i < bytes.Length; i += 2) {
        if (i == bytes.Length - 1) {
          yield return bytes[i];
        } else {
          yield return BitConverter.ToUInt16(bytes, i);
        }
      }
    }

    public TGlyph[] ToUintArray(byte[] bytes) {
      return ToUintEnumerable(bytes).ToArray();
    }


    public byte[] ToByteArray(TGlyph[] glyphs) {
      byte[] r = new byte[glyphs.Length * 2];
      for (int i = 0; i < glyphs.Length; i++) {
        byte[] localBytes = BitConverter.GetBytes(glyphs[i]);
        r[2 * i] = localBytes[0];
        r[1 + 2 * i] = localBytes[1];
      }
      return r;
    }

    private IEnumerable<ushort> FindGlyphsInternal(string str) {
      // not completely sure this is correct. Need an actual
      // example of a composed character sequence coming from LaTeX.
      var unicodeIndexes = StringInfo.ParseCombiningCharacters(str);
      foreach (var index in unicodeIndexes) {
        yield return FindGlyphForCharacterAtIndex(index, str);
      }
    }

    public ushort FindGlyphForCharacterAtIndex(int index, string str) {

      var unicodeIndexes = StringInfo.ParseCombiningCharacters(str);
      int start = 0;
      int end = str.Length;
      foreach (var unicodeIndex in unicodeIndexes) {
        if (unicodeIndex <= index) {
          start = unicodeIndex;
        } else {
          end = unicodeIndex;
          break;
        }
      }
      var encoding = new UnicodeEncoding();
      var substring = str.Substring(start, end - start);
      var encodeSubstring = encoding.GetBytes(substring);
      byte enc0 = encodeSubstring[0];
      byte enc1 = (encodeSubstring.Length <= 1) ? (byte)0 : encodeSubstring[1];
      var bytes = new byte[] { enc0, enc1 };
      var r = BitConverter.ToUInt16(bytes, 0);
      return r;
    }

    public TGlyph[] FindGlyphs(string str)
      => FindGlyphsInternal(str).ToArray();

    public string FindStringDebugPurposesOnly(TGlyph[] glyphs) {
      byte[] bytes = ToByteArray(glyphs);
      var encoding = new UnicodeEncoding();
      var decoder = encoding.GetDecoder();
      var r = encoding.GetString(bytes, 0, bytes.Length);
      return r;
    }
  }
}
