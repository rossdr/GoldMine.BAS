namespace Mine
{
    internal static class Numerics
    {
        public static Bitmap[] GetNumerals(Color back, Color fore)
        {
            Pen pBack = new Pen(back, 2), p = new Pen(fore, 2);
            var numerals = new Bitmap[10];
            numerals[1] = new Bitmap(10, 10);
            var gN = Graphics.FromImage(numerals[1]);
            gN.DrawLine(p, 7, 0, 7, 10);
            numerals[7] = numerals[1].Clone() as Bitmap;
            gN = Graphics.FromImage(numerals[7]);
            gN.DrawLine(p, 2, 0, 7, 0);
            numerals[3] = numerals[7].Clone() as Bitmap;
            gN = Graphics.FromImage(numerals[3]);
            gN.DrawLine(p, 2, 4, 7, 4);
            gN.DrawLine(p, 2, 10, 7, 10);
            numerals[9] = numerals[3].Clone() as Bitmap;
            gN = Graphics.FromImage(numerals[9]);
            gN.DrawLine(p, 2, 0, 2, 4);
            numerals[8] = numerals[9].Clone() as Bitmap;
            gN = Graphics.FromImage(numerals[8]);
            gN.DrawLine(p, 2, 4, 2, 10);

            numerals[6] = numerals[8].Clone() as Bitmap;
            gN = Graphics.FromImage(numerals[6]);
            gN.DrawLine(pBack, 7, 1, 7, 3); //on to the presets.
            numerals[5] = numerals[8].Clone() as Bitmap;
            gN = Graphics.FromImage(numerals[5]);
            gN.DrawLine(pBack, 2, 3, 2, 9);
            gN.DrawLine(pBack, 2, 0, 2, 4);

            numerals[0] = new Bitmap(10, 10);
            gN = Graphics.FromImage(numerals[0]);
            gN.DrawRectangle(p, 2, 0, 7, 10);

            numerals[2] = numerals[0].Clone() as Bitmap;
            gN = Graphics.FromImage(numerals[2]);
            gN.DrawLine(p, 2, 4, 7, 4);
            gN.DrawLine(pBack, 2, 1, 2, 3);
            gN.DrawLine(p, 2, 0, 7, 0);
            gN.DrawLine(pBack, 7, 5, 7, 9);

            numerals[4] = new Bitmap(10, 10);
            gN = Graphics.FromImage(numerals[4]);
            gN.DrawLine(p, 2, 4, 7, 4);
            gN.DrawLine(p, 2, 0, 2, 4);
            gN.DrawLine(p, 7, 0, 7, 10);
            gN.DrawLine(p, 2, 0, 2, 4);
            return numerals;
        }
    }
}
