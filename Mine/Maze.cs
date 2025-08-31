namespace Mine
{
    /// <summary>
    /// Procedurally-generated maze, with one egress to the right.
    /// It's a "maze" because it doesn't open any four-square rooms.
    /// </summary>
    /// <remarks>
    /// David Ross 8/14/2025 from Matuszek and Barnes.
    /// </remarks>
    internal class Maze
    {
        private byte[,] _maze; //each room has a one-way door, which we're bitmasking. 4 bits for square; 6 for hex if we do that.

        public Maze(byte x, byte y)
        {
            _maze = new byte[x, y];
        }

        /// <summary>
        /// Regenerate the maze from a seed.
        /// </summary>
        /// <param name="seed"></param>
        /// <remarks>
        /// Algo is David Matuszek
        /// Byte Dec 1981 pp. 190f.
        /// ...Barnes uses the Microsoft Random object also cooked up in 1981ish.
        /// </remarks>
        public void Generate(int seed)
        {
            var ran = new Random(seed);
            var hv = new List<Tuple<int, int>>(50);
            int xMax = _maze.GetLength(0), yMax = _maze.GetLength(1);
            var t = new Tuple<int, int>(ran.Next(0, xMax - 1), ran.Next(0, yMax - 1)); //-2 in line 230. up to but not including the edges...
            _maze = new byte[xMax, yMax];
            byte[,] s = new byte[xMax, yMax];
            s[t.Item1, t.Item2] = 2;

            do
            { //from Barnes ll. 250-420
                int X = t.Item1, Y = t.Item2;
                if (X < xMax - 1)
                {
                    if (s[X + 1, Y] == 0)
                    {
                        s[X + 1, Y] = 1;// Barnes, stuck with MS BASIC, had here: h[j] = X + 1: v[j] = Y: j = j + 1
                        hv.Add(new Tuple<int, int>(X + 1, Y));
                    }
                }
                if (X > 0)
                {
                    if (s[X - 1, Y] == 0)
                    {
                        s[X - 1, Y] = 1;
                        hv.Add(new Tuple<int, int>(X - 1, Y));
                    }
                }
                if (Y < yMax - 1)
                {
                    if (s[X, Y + 1] == 0)
                    {
                        s[X, Y + 1] = 1;
                        hv.Add(new Tuple<int, int>(X, Y + 1));
                    }
                }
                if (Y > 0)
                {
                    if (s[X, Y - 1] == 0)
                    {
                        s[X, Y - 1] = 1;
                        hv.Add(new Tuple<int, int>(X, Y - 1));
                    }
                }
                //pop-at-index
                t = hv[ran.Next(0, hv.Count() - 1)];
                hv.Remove(t);
                s[t.Item1, t.Item2] = 2;
                bool q = false;
                do
                {
                    var k = ran.Next(1, 5); //Barnes' mapping of 1d4.
                    if (t.Item2 != 0 && k == 1 && s[t.Item1, t.Item2 - 1] == 2) //above is open
                    {
                        _maze[t.Item1, t.Item2] += 4;// we're drawing it right and down. We store the other directions for traversibility.
                        _maze[t.Item1, t.Item2 - 1] += 1;
                        q = true;
                    }
                    if (t.Item1 != xMax - 1 && k == 2 && s[t.Item1 + 1, t.Item2] == 2) //right is open
                    {
                        _maze[t.Item1, t.Item2] += 2;
                        _maze[t.Item1 + 1, t.Item2] += 8;
                        q = true;
                    }
                    if (t.Item2 != yMax - 1 && k == 3 && s[t.Item1, t.Item2 + 1] == 2) // bottom is open
                    {
                        _maze[t.Item1, t.Item2] += 1;
                        _maze[t.Item1, t.Item2 + 1] += 4;
                        q = true;
                    }
                    if (t.Item1 != 0 && k == 4 && s[t.Item1 - 1, t.Item2] == 2) //left is open
                    {
                        _maze[t.Item1, t.Item2] += 8;
                        _maze[t.Item1 - 1, t.Item2] += 2;
                        q = true;
                    }
                } while (!q);
            } while (hv.Count() > 0); //l. 420 "J<>1"

            //Barnes: open one exit to the right.
            var r2 = ran.Next(1, yMax); //1-8.
            _maze[xMax - 1, r2] += 2;
        }

        //replaces Barnes ll. 720f.
        public bool CanGo(byte x, byte y, byte direction)
        {
            var doors = _maze[x, y];
            switch (direction){//Barnes' order of the "K" die: up,right,down,left.
                case 1:
                    doors >>= 2;
                    break;
                case 2:
                    doors >>= 1;
                    break;
                case 3:
                    break;
                default:
                    doors >>= 3;
                    break;
            }
            return doors % 2 == 1;
        }

        //Mark Barnes ll. 135f for CoCo hires graphics
        public Bitmap Render(byte factor, byte wallWidth, Color backColor, Color foreColor) //ll. 135f
        {
            int xMax = _maze.GetLength(0), yMax = _maze.GetLength(1);
            var bmp = new Bitmap(xMax * factor + wallWidth, yMax * factor);
            var g = Graphics.FromImage(bmp);
            var backColorPen = new Pen(backColor, wallWidth); // for PRESETs
            var backColorBrush = new SolidBrush(backColor); // and p will likely be brown (orange here)
            var p = new Pen(foreColor, wallWidth); // for PRESETs

            g.Clear(backColor); //pmode1,1; screen1,1 -> orange on buff.

            int x = 0, y = 0;
            for (int i = 0; i <= xMax; i++)
            {
                g.DrawLine(p, x, 0, x, yMax * factor);
                x += factor;
            }
            for (int i = 0; i <= yMax; i++)
            {
                g.DrawLine(p, 0, y, xMax * factor, y);
                y += factor;
            }

            for (x = 0; x < xMax; x++)
                for (y = 0; y < yMax; y++)
                {
                    int a = x * factor, d = y * factor;
                    var square = _maze[x, y];
                    if (square % 2 == 1) //down
                        g.DrawLine(backColorPen, a, d + factor, a + factor, d + factor); //PRESET
                    square >>= 1;
                    if (square % 2 == 1) //right
                        g.DrawLine(backColorPen, a + factor, d + 1, a + factor, d + factor - 1);
                }
            return bmp;
        }
    }
}
