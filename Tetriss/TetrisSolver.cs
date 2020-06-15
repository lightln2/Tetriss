using System;
using System.Diagnostics;
using System.Linq;

public class TetrisSolver
{
    static int[] pieceOrder;
    static ulong processedMoves;
    static int maxStepFound = 0;
    static int maxScoreFound = 0;
    static readonly Stopwatch timer = new Stopwatch();
    static readonly int[] moves = new int[3000];
    static int movesLength = 0;

    public static void Solve(int maxDepth)
    {
        timer.Start();
        Random rand = new Random(12345);
        pieceOrder = Enumerable.Range(0, maxDepth).Select(i => rand.Next(0, Tetris.PIECES.Length)).ToArray();
        Solve(Tetris.board, 0, 0);
        Console.WriteLine($"Finished with max score={maxScoreFound} and max step={maxStepFound}; processed {processedMoves:N0} moves in {timer.Elapsed}");
    }

    static void Solve(V256 board, int step, int score)
    {
        processedMoves++;
        if (processedMoves % 100_000_000 == 0)
        {
            double millionMovesPerSecond = processedMoves / timer.Elapsed.TotalSeconds / 1000000;
            Console.WriteLine($"Processed {processedMoves:N0} moves at {timer.Elapsed} ({millionMovesPerSecond:0.00} M moves/second)");
        }
        if (step > maxStepFound)
        {
            maxStepFound = step;
            Console.WriteLine($"new max depth={maxStepFound} after processing {processedMoves:N0} moves at {timer.Elapsed}");
        }
        if (step >= pieceOrder.Length) return;

        int pieceIndex = pieceOrder[step];
        V256[] currentPiece = Tetris.PIECES[pieceIndex];

        movesLength += 2;
        for (int rotationIndex = 0; rotationIndex < currentPiece.Length; rotationIndex++)
        {
            V256 piece = currentPiece[rotationIndex];
            moves[movesLength - 2] = rotationIndex;
            if ((piece & board) != 0) break; // can't rotate anymore
            moves[movesLength - 1] = 0;
            Solve(board, piece, step, score);

            for (int i = 1; i <= 5; i++)
            {
                moves[movesLength - 1] = i;
                if (((piece << i) & board) != 0) break; // can't move anymore
                Solve(board, piece << i, step, score);
            }
            for (int i = 1; i <= 5; i++)
            {
                moves[movesLength - 1] = -i;
                if (((piece >> i) & board) != 0) break; // can't move anymore
                Solve(board, piece >> i, step, score);
            }

        }
        movesLength -= 2;
    }

    static void Solve(V256 board, V256 piece, int step, int score)
    {
        while ((board & (piece << Tetris.width)) == 0) piece <<= Tetris.width;
        board |= piece;
        Tetris.RemoveFullLines(ref board, ref score);
        if (score > maxScoreFound)
        {
            maxScoreFound = score;
            Console.WriteLine($"new max score={maxScoreFound} at depth {maxStepFound} after processing {processedMoves:N0} moves at {timer.Elapsed}");
            Console.WriteLine($"  {GetMovesAsString()}");
        }
        Solve(board, step + 1, score);
    }

    static string GetMovesAsString()
    {
        return string.Join(" ",
            Enumerable.Range(0, movesLength / 2).Select(i => $"{moves[i * 2]}:{moves[i * 2 + 1]}"));
    }
}
