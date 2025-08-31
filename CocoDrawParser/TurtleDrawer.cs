using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace CocoDrawParser
{
    /// <summary>
    /// This handles the shorthand of the Extended Color Basic DRAW.
    /// </summary>
    /// <remarks>
    /// This bit is David R Ross, 12/9/2012.
    /// </remarks>
    public class TurtleDrawer
    {
        //for "full" documentation, read Color Computer Magazine March 1983 pp. 20f.
        //Also there is undocumented functionality, that you may run into in code, noted in CCM July 1984 p.70.

        //I remember this "buff" colour, but it always looked white to me
        public static readonly Color[] _color = new Color[9]
        {
            Color.Black,Color.Green,Color.Yellow,Color.Blue,Color.Red,Color.White,Color.Cyan,Color.Magenta,Color.Orange
        };

        //per obj
        private Graphics _g;
        private Pen _forepen;
        private int _maxParseHeight;
        private int _maxParseWidth;
        public int MilliDelay { get; set; }
        private delegate void DrawFunc(string s);
        private Dictionary<char, DrawFunc> _parser;

        //per incoming string
        private Pen _pen;
        private Point _p;
        private double _scale;
        private bool _liftpen;
        private bool _noUpdateTurtle;

        public TurtleDrawer(Graphics g, Pen forepen, int milliDelay = 0)
        {
            _g = g;
            _forepen = forepen;
            _parser = new Dictionary<char, DrawFunc>()
            {
                {'M', new DrawFunc(Move)},

                {'R', new DrawFunc(GoRight)},
                {'L', new DrawFunc(GoLeft)},
                {'U', new DrawFunc(GoUp)},
                {'D', new DrawFunc(GoDown)},
                
                {'E', new DrawFunc(GoRightUp)},
                {'F', new DrawFunc(GoRightDown)},
                {'G', new DrawFunc(GoLeftDown)},
                {'H', new DrawFunc(GoLeftUp)},

                {'S', new DrawFunc(Scale)},
                {'C', new DrawFunc(Colour)}
            };
            _maxParseHeight = _g.VisibleClipBounds.Height.ToString().Length;
            _maxParseWidth = _g.VisibleClipBounds.Width.ToString().Length;
            MilliDelay = milliDelay;
            Reset();
        }

        private void Reset()
        {
            _pen = _forepen;
            _liftpen = false;
            _noUpdateTurtle = false;
            _p = new Point(0, 0);
            _scale = 1;//s4
        }

        public void Draw(string tobeParsed)
        {
            int i = 0;
            bool eol = tobeParsed.Length == 0;
            while (!eol)
            {
                char command = tobeParsed[i];
                if (command == 'B')
                {
                    SetNextNoDraw();
                    i++;
                }
                else if (command == 'N')
                {
                    SetNextNoUpdateTurtle();
                    i++;
                }
                else if (command == ';')//the spacer
                { i++; }
                else
                {
                    int j = i + 1;
                    char next = 'O';//dummy, a non-command
                    char[] arg = new char[tobeParsed.Length - j]; //to the end of the string

                    int t = 0;
                    while (!_parser.ContainsKey(next) && next != 'B' && next != 'N' && next != ';' && j < tobeParsed.Length)
                    {
                        next = tobeParsed[j];
                        arg[t] = next;
                        j++;
                        t++;
                    }

                    string strarg = (new string(arg)).TrimEnd('\0');
                    char lastchar = strarg[strarg.Length - 1];
                    if (_parser.ContainsKey(lastchar) || lastchar == 'B' || lastchar == 'N' || lastchar == ';')
                    {
                        strarg = strarg.Remove(strarg.Length - 1);
                    }

                    if (strarg.Length == 0)
                        strarg = "1";//per the manual.

                    _parser[command](strarg);

                    if (j < tobeParsed.Length)
                        i = j - 1;
                    else
                        eol = true;
                }
            }
            Thread.Sleep(MilliDelay); 
        }

        //b.
        private void SetNextNoDraw()
        {
            _liftpen = true;
        }

        //n.
        private void SetNextNoUpdateTurtle()
        {
            _noUpdateTurtle = true;
        }

        private int ParseVal(string value)
        {
            return (int)(_scale * double.Parse(value));
        }

        #region "functions"
        private void Move(string value)
        {
            string[] parsed = value.Split(',');

            bool isXRelative = (parsed[0][0] == '+') || (parsed[0][0] == '-');
            bool isYRelative = (parsed[1][0] == '+') || (parsed[1][0] == '-');

            int newX;
            int newY;

            if (isXRelative)
                newX = _p.X + ParseVal(parsed[0]);
            else
                newX = int.Parse(parsed[0]);
            if (isXRelative || isYRelative)
                newY = _p.Y + ParseVal(parsed[1]);
            else
                newY = int.Parse(parsed[1]);

            Point newPoint = new Point(newX, newY);

            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }

        private void SetNewpoint(Point newPoint)
        {
            if (!_noUpdateTurtle)
                _p = newPoint;

            _liftpen = false;
            _noUpdateTurtle = false;
        }

        private void GoRight(string value)
        {
            Point newPoint = new Point(_p.X + ParseVal(value), _p.Y);
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }


        private void GoLeft(string value)
        {
            Point newPoint = new Point(_p.X - ParseVal(value), _p.Y);
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }

        private void GoUp(string value)
        {
            Point newPoint = new Point(_p.X, _p.Y - ParseVal(value));
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }

        private void GoDown(string value)
        {
            Point newPoint = new Point(_p.X, _p.Y + ParseVal(value));
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }

        //e, 45
        private void GoRightUp(string value)
        {
            Point newPoint = new Point(_p.X + ParseVal(value), _p.Y - ParseVal(value));
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }
        //f, 135
        private void GoRightDown(string value)
        {
            Point newPoint = new Point(_p.X + ParseVal(value), _p.Y + ParseVal(value));
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }
        //g, 225
        private void GoLeftDown(string value)
        {
            Point newPoint = new Point(_p.X - ParseVal(value), _p.Y + ParseVal(value));
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }
        //h, 315
        private void GoLeftUp(string value)
        {
            Point newPoint = new Point(_p.X - ParseVal(value), _p.Y - ParseVal(value));
            if (!_liftpen)
                _g.DrawLine(_pen, _p, newPoint);

            SetNewpoint(newPoint);
        }

        //s. 1 is .25, 4 is full, 8 is double etc.
        private void Scale(string value)
        {
            _scale = double.Parse(value) * .25;
        }

        //c.
        private void Colour(string value)
        {
            _pen.Color = _color[int.Parse(value)];
        }

        //thankfully we don't need A. (Didn't really need M+ or C, but, meh.)

        #endregion
    }
}
