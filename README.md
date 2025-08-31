"Gold Mine" is a game Mark Barnes wrote in 1982 for the TRS-80 (16k) Color Computer. Color Computer News published it in July.

The core of this game is the procedurally-generated thinwall square mouse maze, which Barnes had adapted from David Matuszek in BYTE (Dec. 1981), 190f. This maze is why I am posting the adaptation here. Maze.cs holds its rendering into bitmap and an algorithm for the player to test its walls. As to why "Gold Mine": one imagines it CSAVE'd neatly upon tape, as GOLDMINE.BAS.

This project provides, for DRAW: the "Coco Draw Parser" I use elsewhere. It includes Karim Oumghar's flood-fill algo for PAINT. I have added a "Writer" class to render the score as a vertical number.

As to sound: most this I recorded from Xroar literally by hand (with a microphone, sigh) which became FLAC and then WAV (double sigh).

Besides all that, I change little from how Barnes was running this zoo. You navigate a 10x9 maze with invisible walls, by the four arrow keys. You pick up gold (or cheese) automatically. You need to get to the exit. There's a time limit, which you see on the right as a fuse burns down to dynamite. If you escape in time, you sell the leftover TNT with the gold, for point$. Then it is off to the next newly-generated maze. Repeat until bored.

<img src="/Images/Goldmine.png">

You can get a flash of the walls, hitting spacebar, in the main picturebox. This effects the striking of a phosphour match; Barnes allows two of these. After which: my code "destructs" the very bitmap of the map, just to be a jerk.

<img src="/Images/Maze.png">


The timer "tick" handles refresh of that status picturebox over on the right. The gameplay - keydown - happens on the main picturebox.

The map is still needed, in my spotty conversion, to deliver the gold - and the "X" signs, which are teleport traps. Said spotty conversion uses the bitmap also for collision-detection, which I do not recommend for map-based games like this (unlike, say... a lander game).

For the seed to this small maze, I use Microsoft's native Random object. I think the TRS-80 used the then-new Knuth algorithm, current in Microsoft product to this day. That algo remains fine until the 64th byte is hit, so we're safe. Likewise this maze doesn't rise to the size I'd fool with variable-pointers. I may get into all that for some later project.

For better support of our high-resolution screens, I have a line "private const byte \_factor = 52". For the original 256x192 experience, as seen in the opening splash, you'd make that 20. Whichever number you use should be a multiple of four.


David Ross
31 August 2025