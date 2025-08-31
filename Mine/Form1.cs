using CocoDrawParser;
using System.Media; //which doesn't play .flac, what the hell?

namespace Mine
{
    /// <summary>
    /// GOLDMINE.BAS
    /// Mark Barnes
    /// CoCoNews July 1982.
    /// Converted by David Reid Ross 11-14 August 2025.
    /// </summary>
    /// <remarks>
    /// CocoDrawParser from TIMPIST 2012.
    /// Painter from Karim Oumghar; the maze-constructor from 1981 Byte mag.
    /// </remarks>
    public partial class Form1 : Form
    {
        private const byte _xMax = 10, _yMax = 9;
        private const byte _factor = 52; //20 for TRS-80 mode (minimum). A multiple of four (because we're going to byteshift it).
        private const byte _maxTurns = 140;
        private readonly Keys[] directions = { Keys.Up, Keys.Right, Keys.Down, Keys.Left };

        private readonly SoundPlayer _playerABCDEFG;
        private readonly SoundPlayer _playerECECEC;

        private readonly Bitmap _bmpRightBasis;
        private readonly Bitmap _bmpFlame;
        private readonly Writer _scoreWriter;
        private Bitmap _bmpRight;
        private Bitmap _bmpMaze; //when you light the match.
        private Bitmap _bmpLeft;

        private Maze _maze;
        private ushort _score = 0, _seed = 1;
        private byte _matches = 2;
        private byte _x, _y;
        private byte _turn = 0;

        public Form1()
        {
            InitializeComponent();
            _maze = new Maze(_xMax, _yMax);
            _playerABCDEFG = new SoundPlayer(@".\SoundEffects\ABCDEFG.wav");
            _playerECECEC = new SoundPlayer(@".\SoundEffects\ECECEC.wav");

            //setting up the TNT fuse.
            _bmpRightBasis = new Bitmap(42, 192); //from y 201-242 that is. score will run down over on 243-255.
            var gright = Graphics.FromImage(_bmpRightBasis);
            //CIRCLE(22,150),10,3,.75 - radius 10, color 3, h/w ratio 3:4
            gright.DrawEllipse(new Pen(Color.White, 2), 12, _maxTurns - 3, 21, 15);
            gright.FillEllipse(new SolidBrush(Color.Cyan), 13, _maxTurns - 2, 19, 13);//PAINT<220,150>,2,3:
            //We are on Screen 1,1 / PMode 1. so what's color 3 here?
            var td2 = new TurtleDrawer(gright, new Pen(Color.White, 2)); //forget "C3".
            td2.Draw($"S4;BM33,{_maxTurns + 10};D40;L20;U40;C4"); //shifted the scalefactor from l. 55
            //PAINT<222,180>,3,3 - so yeah, border color is going to be "3"
            _bmpRightBasis.FloodFill(new Point(22, _maxTurns + 40), Color.Black, Color.Magenta);

            Pen p = new Pen(Color.Orange, 2); //lo-res PMODE 1. (we don't seem to own height-2, width-1 of the PMODE 2-3 worlds.)
            gright.DrawLine(p, 22, 5, 22, _maxTurns + 10); //started from 0 but the flame is going to wipe the top 10 immediately.

            //l. 510 gosubbing ll. 970f.
            _bmpFlame = new Bitmap(10, 10);
            var gflame = Graphics.FromImage(_bmpFlame);
            gflame.FillEllipse(new SolidBrush(Color.HotPink), 1, 2, 6, 8);

            _scoreWriter = new Writer(Numerics.GetNumerals(Color.Black, Color.White), 14);

            _bmpLeft = GetPlayScreen();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            //form1_load won't run this stuff
            //ll. 10-100 is the splash.
            //so, threadsleeps.
            pictureBox1.Width = Width;//for splash purpose
            pictureBox1.Height = 192; //max for Coco PMODE 1 w/ multicolor
            pictureBox2.Visible = false;
            pictureBox1.BackColor = Color.Black;
            pictureBox2.BackColor = Color.Black;
            var g = pictureBox1.CreateGraphics();

            var td = new TurtleDrawer(g, new Pen(Color.Orange, 2)); //in lieu of "C8".
            int pitch = 1000, delay = 50;
            for (byte s = 4; s <= 9; s++)
            {
                Console.Beep(pitch, delay);
                pictureBox1.Refresh();
                td.Draw($"S{s}BM0,10;R20D5L15D20R10U5L5U5R10D15L20U30BR30R20D30L20U30BF5R10D20L10U20BU5BR25R5D25R15D5L20U30BR30R15F5D20G5L15U30BF5R8D20L8U20");
                td.Draw($"BM0,180;U30R5F5E5R5D30L5U25G5H5D25L5BR31U5R7U20L7U5R19D5L7D20R7D5L19BR29U30R5D5F10U15R5D30L5U5H10D15L5BR30U30R20D5L15D7R7D6L7D7R15D5L20");

                Thread.Sleep(delay + 150);
                pitch += 100;
            }
            //ll. 60-110 halt the program to play music and bleet between screen 1,1 and 1,0
            //so meanwhile...
            for (byte i = 1; i < 10; i++)
            {
                _playerABCDEFG.Play();
                Thread.Sleep(500);
            }
            //this can be async.
            var player = new SoundPlayer(@"SoundEffects\MusicStart.wav");
            player.Play();

            //unsplash all this, set up the real board.
            pictureBox1.Height = Math.Max(192, _bmpMaze.Height);
            pictureBox1.Width = _bmpMaze.Width;
            pictureBox2.Left = _bmpMaze.Width;
            pictureBox2.Height = pictureBox1.Height;
            Width = pictureBox1.Width + pictureBox2.Width + 22;
            Height = pictureBox1.Height + 50;

            //now let's never be here again
            Activated -= Form1_Activated;
            pictureBox1.Paint += pictureBox1_Paint;
            this.KeyDown += Form1_KeyDown;
            pictureBox2.Visible = true;
            pictureBox1.Refresh();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var g = Graphics.FromImage(_bmpRight);
            //PUT<218,T>-<229,T+10>,FL,PSET
            g.FillRectangle(new SolidBrush(Color.Black), 18, _turn, 10, 10);
            g.DrawImage(_bmpFlame, 18, _turn - 2);

            _turn++;
            pictureBox2.Refresh();
            if (_turn > _maxTurns)
            { //ll.1360f
                this.KeyDown -= Form1_KeyDown;
                timer1.Stop();
                for (byte i = 1; i < 10; i++)
                {
                    //flash screen
                    _playerECECEC.Play(); //on octave 1 in l. 1380
                    Thread.Sleep(500);
                }
                var player = new SoundPlayer(@"SoundEffects\Dirge.wav");
                player.Play();
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show($"You gathered ${_score} worth of gold...\r\n but gold is worthless to a DEAD MAN!\r\n\r\nY O U  L O S E\r\n\r\nAnother game?", "Lose", buttons);
                if (result == DialogResult.No) Close();

                _score = 0;
                _seed = 0;
                _bmpLeft = GetPlayScreen();
                pictureBox1.Refresh();
                pictureBox2.Refresh();
                this.KeyDown += Form1_KeyDown;
                timer1.Start();
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //_KeyPress didn't work, boo.
            //checkmap can waste time, so remove further keyhits.
            this.KeyDown -= Form1_KeyDown;
            switch (e.KeyCode)
            {
                case Keys.Space:
                    e.Handled = true;
                    if (_matches == 0) { _bmpMaze = null; break; }
                    var g = pictureBox1.CreateGraphics();
                    g.Clear(Color.White);
                    g.DrawImage(_bmpMaze, 0, 0);
                    Thread.Sleep(2000);
                    pictureBox1.Refresh();
                    _matches--;
                    break;
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    e.Handled = CheckMap(e.KeyCode);
                    break;
                default:
                    e.Handled = false;
                    break;
            }
            this.KeyDown += Form1_KeyDown;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            //from now on we play in PMODE 1,3. PMODE 1,1 is the sketchboard.

            //l. 445 screen setup.
            g.DrawImage(_bmpLeft, 0, 0);
            if (_x < byte.MaxValue)
            {
                g.FillRectangle(new SolidBrush(Color.White),
                    new Rectangle(
                        _x * _factor + (_factor >>> 2), _y * _factor + (_factor >>> 2),
                        _factor >>> 1, _factor >>> 1));//rightshift is like a divide-by-two. unsigned.
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            //these are from x>200 so, picturebox2. ll. 470f.
            var g2 = e.Graphics;
            g2.DrawImage(_bmpRight, 0, 0);

            var imgScore = _scoreWriter.RenderVerticalNumber(_score);
            g2.FillRectangle(new SolidBrush(pictureBox2.BackColor), new Rectangle(42, 2, imgScore.Width, imgScore.Height));
            g2.DrawImage(imgScore, new Point(42, 2));
        }

        private Bitmap GetPlayScreen()
        {
            _maze.Generate(_seed);
            _bmpMaze = _maze.Render(_factor, 2, Color.White, Color.Orange);

            byte trapCount = 5;
            var bmp = new Bitmap(_bmpMaze.Width, _bmpMaze.Height);
            var g = Graphics.FromImage(bmp);
            //Screen 1,0: Green/Yellow/Blue/Red
            g.Clear(Color.LightGreen); //PCLS, with the default background being green (1).
            var pen2 = new Pen(Color.Yellow);
            var pen3 = new Pen(Color.Blue, 2);
            if (_bmpMaze.Width >= 250) pen3.Width = 4;
            var brush2 = new SolidBrush(pen2.Color);
            var brush3 = new SolidBrush(pen3.Color);
            byte halfStep = _factor >> 1;
            int radius = _bmpMaze.Width < 250 ? 3 : 6;//CIRCLE(X,Y),3,2: radius,color (and it's filled)
            for (int x = halfStep + _factor; x <= _bmpMaze.Width - halfStep; x += _factor * 2) //X=30 TO ·190 STEP 40
                for (int y = halfStep; y <= _bmpMaze.Height - halfStep; y += _factor * 2) //Y=10 TO 170 STEP 40
                    g.FillEllipse(brush2, x - radius, y - radius, radius * 2, radius * 2);
            for (int x = halfStep; x <= _bmpMaze.Width - halfStep - _factor; x += _factor * 2) //X=10 TO 170 STEP 40
                for (int y = halfStep + _factor; y <= _bmpMaze.Height - halfStep - _factor; y += _factor * 2) //Y=30 TO 170 STEP 40; typo for 150
                    g.FillEllipse(brush2, x - radius, y - radius, radius * 2, radius * 2);

            //gosub 890: place the X teleport traps. Unlike the maze this will be stochastic.
            //also Barnes allows overlap resulting in fewer than the count.
            var r = new Random();
            var td = new TurtleDrawer(g, pen3);
            if (_bmpMaze.Width < 250) td.Draw("S4"); //line 55
            else td.Draw("S6");
            for (byte j = 1; j <= trapCount; j++)
            {
                Color ppxy;
                int x, y;
                do
                {
                    x = r.Next(1, _xMax - 1) * _factor + halfStep; //*20+10
                    y = r.Next(0, _yMax - 1) * _factor + halfStep;
                    ppxy = bmp.GetPixel(x, y);
                    if (ppxy == pen2.Color) x -= _factor;
                } while (ppxy == pen3.Color || (x < _factor && y < _factor));
                td.Draw($"BM{x},{y}NE4NF4NG4NH4");
            }

            //l. 500 opens the egress, seen line 440.

            //now the player's admin stuff
            _x = 0; _y = 0;
            _turn = 0;
            _matches = 2;
            _bmpRight = _bmpRightBasis.Clone() as Bitmap;
            return bmp;
        }
        private bool CheckMap(Keys keychar)
        {
            //ll. 720 et al., for that wall over in PMODE 1,1.
            var direction = (byte)(Array.IndexOf(directions, keychar) + 1);
            if (!_maze.CanGo(_x, _y, direction)) return false;

            var xOffset = keychar == Keys.Left ? -1 : keychar == Keys.Right ? 1 : 0;
            _x = (byte)(_x + xOffset);
            if (_x >= _xMax)
            {
                //l.1500f
                timer1.Stop();
                _score += _maxTurns;
                _score -= _turn;
                var player = new SoundPlayer(@"SoundEffects\Winner.wav");
                player.Play();
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show($"You gathered ${_score} worth of gold\r\nand got out of the mine alive!\r\n\r\nC O N G R A T U L A T O N S\r\n\r\nContinue?", "Win", buttons);
                if (result == DialogResult.No) Close(); //win.

                _seed++;
                _bmpLeft = GetPlayScreen();
                pictureBox1.Refresh();
                pictureBox2.Refresh();
                timer1.Start();
                return true;
            }

            //don't have to preset, picturebox1_paint redraws the player
            var yOffset = keychar == Keys.Up ? -1 : keychar == Keys.Down ? 1 : 0;
            _y = (byte)(_y + yOffset);

            //gosub 800
            //here we PPOINT the visible grid
            int xPos = _x * _factor + (_factor >>> 2), yPos = _y * _factor + (_factor >>> 2);

            // pictureBox1.Image as Bitmap isn't of help
            var ppxy = _bmpLeft.GetPixel(xPos + (_factor >>> 2), yPos + (_factor >>> 2)); // divide by four. And have to be explicit this division happens first!
            if (ppxy.ToArgb() == Color.Yellow.ToArgb())
            {
                _score += 20;
                _playerABCDEFG.Play();
                var g = Graphics.FromImage(_bmpLeft);
                g.FillRectangle(new SolidBrush(Color.LightGreen), xPos, yPos, _factor >> 1, _factor >> 1);
                pictureBox2.Refresh();
            }
            else if (ppxy.ToArgb() == Color.Blue.ToArgb())
            {
                var g = Graphics.FromImage(_bmpLeft);
                g.FillRectangle(new SolidBrush(Color.LightGreen), xPos, yPos, _factor >> 1, _factor >> 1);
                //play music, flash player... waste a precious second
                var xDummy = _x;
                for (int i = 0; i < 10; i++)
                {
                    _playerECECEC.Play();
                    _x = byte.MaxValue;
                    pictureBox1.Refresh();
                    Thread.Sleep(200);
                    _x = xDummy;
                    pictureBox1.Refresh();
                    Thread.Sleep(200);
                }

                var r = new Random();
                //DRR: I didn't like how Barnes jumped onto gold or X's, so this jump will refuse those
                do
                {
                    //l 870 is X = (RND(10) -1) * 20 + 5 : Y = RND(8) * 20 + 5
                    _x = (byte)r.Next(0, _xMax - 1); xPos = _x * _factor + (_factor >>> 2);
                    _y = (byte)r.Next(1, _yMax - 1); yPos = _y * _factor + (_factor >>> 2);
                    ppxy = _bmpLeft.GetPixel(xPos + (_factor >>> 2), yPos + (_factor >>> 2));
                } while (ppxy.ToArgb() != Color.LightGreen.ToArgb());
            }
            pictureBox1.Refresh();
            return true;
        }
    }
}