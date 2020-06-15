using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

[TestClass]
public class TetrisTests
{
    [TestMethod]
    public void Tetris_NoLinesToRemoveOEmptyBoard()
    {
        V256 board = Tetris.board;
        int score = 0;
        Tetris.RemoveFullLines(ref board, ref score);
        Assert.AreEqual(0, score);
    }

    [TestMethod]
    public void Tetris_RemoveOneLine()
    {
        V256 board = Tetris.board;
        V256 piece = Tetris.PIECES[0][0];
        piece >>= 3;
        while ((board & (piece << Tetris.width)) == 0) piece <<= Tetris.width;
        board |= piece;
        piece = Tetris.PIECES[0][0];
        piece <<= 3;
        while ((board & (piece << Tetris.width)) == 0) piece <<= Tetris.width;
        board |= piece;
        piece = Tetris.PIECES[0][1];
        piece >>= 1;
        while ((board & (piece << Tetris.width)) == 0) piece <<= Tetris.width;
        board |= piece;
        piece = Tetris.PIECES[0][1];
        while ((board & (piece << Tetris.width)) == 0) piece <<= Tetris.width;
        board |= piece;

        int score = 0;
        Tetris.RemoveFullLines(ref board, ref score);
        Assert.AreEqual(1, score);
    }

}
