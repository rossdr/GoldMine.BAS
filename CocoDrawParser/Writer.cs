using System.Drawing;

namespace CocoDrawParser
{
    /// <summary>
    /// Render a number into a bitmap display.
    /// The font is sent to the constructor.
    /// </summary>
    /// <remarks>
    /// David Ross 8/31/2025.
    /// </remarks>
    public class Writer
    {
        private Bitmap[] _numberFont;
        private byte _letterSeparator;
        public Writer(Bitmap[] numberFont, byte letterSeparator)
        {
            _numberFont = numberFont;
            _letterSeparator = letterSeparator;
        }

        public Bitmap RenderVerticalNumber(uint number)
        {
            var word = number.ToString();
            int bmHeight = word.Length * (_numberFont[0].Height + _letterSeparator) - _letterSeparator;
            var bm = new Bitmap(_numberFont[0].Width, bmHeight);
            var g = Graphics.FromImage(bm);
            var place = new Point(1, 0);
            for (int L = 0; L < word.Length; L++)
            {
                if (word[L] >= '0' && word[L] <= '9')
                    g.DrawImage(_numberFont[(int)word[L] - 48], place);
                else place.Y += _numberFont[0].Height;//like space
                place.Y += _letterSeparator;
            }
            return bm;
        }
    }
}
