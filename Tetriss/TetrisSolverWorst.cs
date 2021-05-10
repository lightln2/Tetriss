using System;
using System.Diagnostics;
using System.Linq;

public class TetrisSolverWorst
{
    static ulong processedMoves;
    static int maxStepFound = 0;
    static int maxScoreFound = 0;
    static readonly Stopwatch timer = new Stopwatch();
    static readonly int[] moves = new int[30000];
    static int movesLength = 0;
    static int MAX_STEP = 0;

    public static void Solve(int maxDepth)
    {
        MAX_STEP = maxDepth;
        timer.Start();
        Solve(Tetris.board, 0, 0);
        Console.WriteLine($"Finished with max score={maxScoreFound} and max step={maxStepFound}; processed {processedMoves:N0} moves in {timer.Elapsed}");
    }

    public static int GetWorstPiece(V256 board)
    {
        int worstPiece = -1;
        int worstDepth = -1;

        for (int pieceIndex = 0; pieceIndex < Tetris.PIECES.Length; pieceIndex++)
        {
            int height = GetMinHeight(board, pieceIndex);
            if (height > worstDepth)
            {
                worstDepth = height;
                worstPiece = pieceIndex;
            }
        }
        return worstPiece;        
    }

    static int GetMinHeight(V256 board, int pieceIndex)
    {
        int minHeight = 100;
        V256[] currentPiece = Tetris.PIECES[pieceIndex];
        for (int rotationIndex = 0; rotationIndex < currentPiece.Length; rotationIndex++)
        {
            int height = GetMinHeight(board, currentPiece[rotationIndex]);
            if (height < minHeight) minHeight = height;
        }
        return minHeight;
    }
    
    static int GetMinHeight(V256 board, V256 piece)
    {
        int minHeight = GetHeight(board, piece);
        for (int i = 1; i <= 5; i++)
        {
            if (((piece << i) & board) != 0) break;
            int height = GetHeight(board, piece << i);
            if (height < minHeight) minHeight = height;
        }
        for (int i = 1; i <= 5; i++)
        {
            if (((piece >> i) & board) != 0) break;
            int height = GetHeight(board, piece >> i);
            if (height < minHeight) minHeight = height;
        }
        return minHeight;
    }

    static int GetHeight(V256 board, V256 piece)
    {
        while ((board & (piece << Tetris.width)) == 0) piece <<= Tetris.width;
        board |= piece;
        int score = 0;
        return Tetris.RemoveFullLinesAndCountDepth(ref board, ref score);
    }

    static void Solve(V256 board, int step, int score)
    {
        processedMoves++;
        if (processedMoves % 1_000_000_000 == 0)
        {
            double millionMovesPerSecond = processedMoves / timer.Elapsed.TotalSeconds / 1000000;
            Console.WriteLine($"Processed {processedMoves:N0} moves at {timer.Elapsed} ({millionMovesPerSecond:0.00} M moves/second)");
        }
        if (step > maxStepFound)
        {
            maxStepFound = step;
            Console.WriteLine($"new max depth={maxStepFound} after processing {processedMoves:N0} moves at {timer.Elapsed}");
        }
        if (step >= MAX_STEP) return;
        int curScore = 0;
        V256 curBoard = board;
        int height = Tetris.RemoveFullLinesAndCountDepth(ref curBoard, ref curScore);
        if (height > 7) return;

        int pieceIndex = GetWorstPiece(board);
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
