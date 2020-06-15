using System;
using System.Text;
using System.Threading;

public class Tetris
{
    public static readonly int width = 12; // field + borders
    static int score = 0;
    static readonly Random rand = new Random(12345);

    // for drawing
    static readonly V256 tile = V256.FromUShort(' ' + ('*' << 8));
    static readonly V256 lineToStringShuffler = new V256(0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1);
    static readonly V256 lineToStringMask = new V256(1, 2, 4, 8, 16, 32, 64, 128, 1, 2, 4, 8);

    public static V256 board = new V256(
        1, 24, 128, 1, 24, 128, 1, 24,
        128, 1, 24, 128, 1, 24, 128, 1,
        24, 128, 1, 24, 128, 1, 24, 128,
        1, 24, 128, 1, 24, 128, 255, 15);

    public static readonly V256 fullLine = new V256(
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 255, 15);

    public static readonly V256 emptyLine = new V256(1, 8);

    public static readonly V256 checkLineMask = new V256(
        0, 4, 64, 0, 4, 64, 0, 4, 64, 0, 4, 64,
        0, 4, 64, 0, 4, 64, 0, 4, 64, 0, 4, 64,
        0, 4, 64, 0, 4, 64, 0, 0);

    // array of all rotated variants; rotation cyclically choses between them
    static readonly V256[] fallingPiece = new V256[10];
    static int rotationsCount;
    static int fallingPieceRotation;
    static V256 FallingPiece => fallingPiece[fallingPieceRotation % rotationsCount];

    // possible pieces with rotations
    public static readonly V256[][] PIECES = new V256[][]
    {
        new[] { // I-shape
            new V256(240, 0),
            new V256(64, 0, 4, 64, 0, 4),
        },
        new[] { // O-shape
            new V256(96, 0, 6),
        },
        new[] { // Z-shape
            new V256(96, 0, 12),
            new V256(64, 0, 6, 32),
        },
        new[] { // S-shape
            new V256(192, 0, 6),
            new V256(32, 0, 6, 64),
        },
        new[] { // L-shape
            new V256(112, 0, 2),
            new V256(64, 0, 6, 64),
            new V256(32, 0, 7),
            new V256(32, 0, 6, 32),
        },
        new[] { // J-shape
            new V256(112, 0, 4),
            new V256(64, 0, 4, 96),
            new V256(32, 0, 14),
            new V256(96, 0, 2, 32),
        },
        new[] { // T-shape
            new V256(224, 0, 2),
            new V256(96, 0, 4, 64),
            new V256(64, 0, 7),
            new V256(32, 0, 2, 96),
        },
        new[] { // surprise!
            new V256(56, 128, 2, 32, 0, 14, 160, 0, 14),
            new V256(24, 1, 17, 112, 1, 25, 112, 1),
            new V256(200, 129, 20, 120, 129, 20, 200, 1),
            new V256(72, 2, 21, 224, 0, 21, 72, 2),
        },
    };

    static void Main(string[] args)
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.Clear();
        if (args.Length == 0)
        {
            PlayGame();
            return;
        }
        else if (args[0] == "-solve")
        {
            TetrisSolver.Solve(int.Parse(args[1]));
            return;
        }
        else if (args[0] == "-replay")
        {
            Replay(args[1]);
            return;
        }
        else
        {
            Console.WriteLine("Usage: \ntetris\ntetris -solve max-depth\ntetris -replay \"script\"");
        }
    }

    static void PlayGame()
    {
        SelectNextPiece();

        while (!Intersects(FallingPiece))
        {
            DisplayField();

            for (int i = 0; i < 10; i++)
            {
                Control();
                Thread.Sleep(100);
            }

            if (!MoveDown())
            {
                AddPieceToBoard();
                RemoveFullLines();
                SelectNextPiece();
                continue;
            }
        }
        DisplayField();
    }

    static void SelectNextPiece() => SelectPiece(rand.Next(0, PIECES.GetLength(0)));

    static void SelectPiece(int index)
    {
        fallingPieceRotation = 0;
        rotationsCount = PIECES[index].Length;
        Array.Copy(PIECES[index], fallingPiece, rotationsCount);
    }

    static void AddPieceToBoard() => board |= FallingPiece;
    static bool Intersects(V256 piece) => (piece & board) != 0;
    static bool MoveDown() => Shift(width);
    static bool MoveRight() => Shift(1);
    static bool MoveLeft() => Shift(-1);
    static void RotateLeft() => Rotate(1);
    static void RotateRight() => Rotate(-1);
    static void RemoveFullLines() => RemoveFullLines(ref board, ref score);

    static bool Shift(int count)
    {
        if (count > 0)
        {
            if (Intersects(FallingPiece << count)) return false;
            for (int i = 0; i < rotationsCount; i++) fallingPiece[i] <<= count;
        }
        else
        {
            if (Intersects(FallingPiece >> -count)) return false;
            for (int i = 0; i < rotationsCount; i++) fallingPiece[i] >>= -count;
        }
        return true;
    }

    static bool Rotate(int count)
    {
        while (count < 0) count += rotationsCount;
        fallingPieceRotation += count;
        if (!Intersects(FallingPiece)) return true;
        fallingPieceRotation -= count;
        return false;
    }

    static void Control()
    {
        if (Console.KeyAvailable)
        {
            ConsoleKeyInfo key = Console.ReadKey(false);
            if (key.Key == ConsoleKey.UpArrow) RotateLeft();
            else if (key.Key == ConsoleKey.DownArrow) RotateRight();
            else if (key.Key == ConsoleKey.LeftArrow) MoveLeft();
            else if (key.Key == ConsoleKey.RightArrow) MoveRight();
            else if (key.Key == ConsoleKey.Spacebar) while (MoveDown()) ;
            while (Console.KeyAvailable) Console.ReadKey();
            DisplayField();
        }
    }

    public static void RemoveFullLines(ref V256 board, ref int score)
    {
        // every 12 bits of test:
        V256 test = board;   // * a1 a2 a3 a4 a5 a6 a7 a8 a9 a10 * //
        test &= (test << 5); // ? ? ? ? ? a1&a6 a2&a7 a3&a8 a4&a9 a5*a10 ? //
        test &= (test << 2); // ? ? ? ? ? ? ? a1&a6&a3&a8 a2&a7&a4&a9 a5*a10 ? //
        test &= (test << 1); // ? ? ? ? ? ? ? ? a1&a6&a3&a8&a2&a7&a4&a9 a5*a10 ? //
        test &= (test << 1); // ? ? ? ? ? ? ? ? ? a1&...&a10 ? 
        // each eleventh bit out of twelve is set iff line is full
        if ((test & checkLineMask) == 0) return; 

        int scoreBoost = 1;
        V256 lineMask = fullLine >> width;
        V256 underLineMask = fullLine;
        while (lineMask != 0)
        {
            while ((board & lineMask) == lineMask)
            {
                score += scoreBoost;
                scoreBoost <<= 1;
                board = (board & underLineMask) | ((board & ~underLineMask & ~lineMask) << width) | emptyLine;
            }
            underLineMask |= lineMask;
            lineMask >>= width;
        }
    }

    static void DisplayField()
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Gray;
        DisplaySystemInfo();
        Console.SetCursorPosition(0, 2);
        Console.ForegroundColor = ConsoleColor.White;

        V256 field = board | FallingPiece;
        while (field != 0)
        {
            Console.WriteLine("   " + GetLine(field));
            field >>= width;
        }
        Console.WriteLine($"\n\n   Score: {score}\n");
    }

    static string GetLine(V256 field)
    {
        V256 line = field.Shuffle(lineToStringShuffler) & lineToStringMask;
        line = line.Min(V256.ONE);
        line = tile.Shuffle(line);
        return Encoding.ASCII.GetString(line.ToArray<byte>(), 0, width);
    }

    static void DisplaySystemInfo()
    {
        Console.SetCursorPosition(16, 2);
        Console.WriteLine($"Left/Right: move piece; Up/Down: rotate; Space: drop piece");
        Console.SetCursorPosition(16, 5);
        Console.WriteLine($"Board: {board}");
        Console.SetCursorPosition(16, 7);
        Console.WriteLine($"Piece: {FallingPiece}");
        Console.SetCursorPosition(16, 10);
        Console.WriteLine($"p:000  {fallingPiece[0]}");
        Console.SetCursorPosition(16, 11);
        Console.WriteLine($"p:090  {fallingPiece[1]}");
        Console.SetCursorPosition(16, 12);
        Console.WriteLine($"p:180  {fallingPiece[2]}");
        Console.SetCursorPosition(16, 13);
        Console.WriteLine($"p:270  {fallingPiece[3]}");
    }

    static void Replay(string script)
    {
        foreach (string next in script.Split(' '))
        {
            SelectNextPiece();
            DisplayField();
            Thread.Sleep(100);
            int rotation = int.Parse(next.Split(':')[0]);
            int move = int.Parse(next.Split(':')[1]);
            for (int i = 0; i < rotation; i++)
            {
                RotateLeft();
                DisplayField();
                Thread.Sleep(100);
            }
            DisplayField();
            Thread.Sleep(200);
            int moveDelta = Math.Sign(move);
            for (int i = 0; i < Math.Abs(move); i++)
            {
                Shift(moveDelta);
                DisplayField();
                Thread.Sleep(100);
            }
            DisplayField();
            Thread.Sleep(200);

            while (MoveDown())
            {
                DisplayField();
                Thread.Sleep(40);
            }

            AddPieceToBoard();
            RemoveFullLines();
            DisplayField();
            Thread.Sleep(100);
        }
    }

}
