using System.Drawing;

namespace CocoDrawParser
{
    public class SpriteFactory
    {
        private Bitmap _r;
        private Pen _p;
        private string _offset;

        public SpriteFactory(int x, int y, byte s, Color back, Brush fore)
        {
            _r = new Bitmap(x * s / 4, y * s / 4);//, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            using (var g = Graphics.FromImage(_r))
            {
                g.Clear(back);
            }
            _p = new Pen(fore);
            _offset = $"BM0,+{2 * s + 2};S{s}"; //he drew them from the bottom left
        }
        public Image Empty => _r;

        public Image MakeSprite(string draw)
        {
            var r = _r.Clone() as Bitmap;
            using (var g = Graphics.FromImage(r))
            {
                var td = new TurtleDrawer(g, _p);
                td.Draw(_offset);
                td.Draw(draw);
            }

            return r;
        }
    }
}
