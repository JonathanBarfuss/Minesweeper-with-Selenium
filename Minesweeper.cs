using System;
using System.Collections.Generic;
using System.Text;

namespace Minesweeper_with_Selenium
{
    /// <summary>
    /// Class to play minesweeper. Methods here analyze patterns to discover where mines are and aren't
    /// </summary>
    class Minesweeper
    {
        private Square[][] grid;
        private bool lost;
        private bool won;
        private MinesweeperPage page;
        private int rows;
        private int cols;

        public Minesweeper(int rows, int cols, MinesweeperPage page)
        {
            this.page = page;
            this.rows = rows;
            this.cols = cols;
            
            grid = new Square[rows][];
            for(int i = 0; i < rows; i++)
            {
                grid[i] = new Square[cols];
                for(int j = 0; j < cols; j++)
                {
                    grid[i][j] = new Square();
                }
            }
        }

        /// <summary>
        /// This method will both select the square on the page, but also in the class data model.
        /// </summary>
        /// <param name="row">The row of the square (starting at 0)</param>
        /// <param name="col">The column of the square (starting at 0)</param>
        public void selectSquare(int row, int col)
        {
            if (row < 0 || col < 0 || row > grid.Length - 1 || col > grid[0].Length - 1 || grid[row][col].getRevealed() || grid[row][col].isFlagged())
                return;
            
            page.selectSquare(row + 1, col + 1);
            lost = page.gameLost();

            if (lost)
                return;

            grid[row][col].setValue(page.readSquare(row + 1, col + 1));

            if(grid[row][col].getValue() == 0)
            {
                abstractSelect(row - 1, col - 1);
                abstractSelect(row, col - 1);
                abstractSelect(row + 1, col - 1);
                abstractSelect(row - 1, col);
                abstractSelect(row + 1, col);
                abstractSelect(row - 1, col + 1);
                abstractSelect(row, col + 1);
                abstractSelect(row + 1, col + 1);
            }
        }

        private void abstractSelect(int row, int col)
        {
            if (row < 0 || col < 0 || row > grid.Length - 1 || col > grid[0].Length - 1 || grid[row][col].getRevealed() || grid[row][col].isFlagged())
                return;

            grid[row][col].setValue(page.readSquare(row + 1, col + 1));

            if (grid[row][col].getValue() == 0)
            {
                abstractSelect(row - 1, col - 1);
                abstractSelect(row, col - 1);
                abstractSelect(row + 1, col - 1);
                abstractSelect(row - 1, col);
                abstractSelect(row + 1, col);
                abstractSelect(row - 1, col + 1);
                abstractSelect(row, col + 1);
                abstractSelect(row + 1, col + 1);
            }
        }

        public void flagSquare(int row, int col)
        {
            if (grid[row][col].isFlagged() || grid[row][col].getRevealed())
            {
                return;
            }
            page.flagSquare(row + 1, col + 1);
            grid[row][col].flag();
        }

        public void searchMines()
        {
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    if(!grid[i][j].getRevealed() && !grid[i][j].isFlagged())
                    {
                        if (page.isBomb(i + 1, j + 1))
                        {
                            grid[i][j].boom();
                        }
                        if(page.isDeath(i + 1, j + 1))
                        {
                            grid[i][j].ouch();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method that compares each square value to number of unrevealed squares around it. If the numbers match, everything is flagged
        /// </summary>
        public void analyzeForFlags()
        {
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    Square current = grid[i][j];
                    if(!current.getRevealed() || current.getValue() == 0 || current.isDone())
                    {
                        continue;
                    }

                    int numBlank = 0;
                    int numFlagged = 0;
                    for(int a = -1; a < 2; a++)
                    {
                        if ((i == 0 && a == -1) || (i == rows - 1 && a == 1))
                        {
                            continue;
                        }
                        for (int b = -1; b < 2; b++)
                        {
                            if ((j == 0 && b == -1) || (j == cols - 1 && b == 1))
                            {
                                continue;
                            }
                            if (!grid[i + a][j + b].getRevealed())
                            {                                
                                numBlank++;
                            }
                            if (grid[i + a][j + b].isFlagged())
                            {
                                numFlagged++;
                            }
                        }
                    }

                    if(numBlank == numFlagged)
                    {
                        current.done();
                        continue;
                    }

                    if(current.getValue() == numBlank)
                    {
                        for (int a = -1; a < 2; a++)
                        {
                            if((i == 0 && a == -1) || (i == rows-1 && a == 1))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if((j == 0 && b == -1) || (j == cols-1 && b == 1))
                                {
                                    continue;
                                }
                                flagSquare(i + a, j + b);
                            }
                        }
                        current.done();
                    }
                }
            }
        }

        /// <summary>
        /// Method to see if the number of neighboring flags match the value of the square, if so it will click the remaining neighbors
        /// </summary>
        public void analyzeClicks()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Square current = grid[i][j];
                    if (!current.getRevealed() || current.getValue() == 0 || current.isDone())
                    {
                        continue;
                    }

                    int numFlagged = 0;
                    for (int a = -1; a < 2; a++)
                    {
                        if ((i == 0 && a == -1) || (i == rows - 1 && a == 1))
                        {
                            continue;
                        }
                        for (int b = -1; b < 2; b++)
                        {
                            if ((j == 0 && b == -1) || (j == cols - 1 && b == 1))
                            {
                                continue;
                            }
                            if (grid[i+a][j+b].isFlagged())
                            {
                                numFlagged++;
                            }
                        }
                    }

                    if (current.getValue() == numFlagged)
                    {
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i == 0 && a == -1) || (i == rows - 1 && a == 1))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j == 0 && b == -1) || (j == cols - 1 && b == 1))
                                {
                                    continue;
                                }
                                selectSquare(i + a, j + b);
                            }
                        }
                        current.done();
                    }
                }
            }
        }

        /// <summary>
        /// Method that searches for a specific pattern. When there is a consecutive 1 2 on the grid, there is often a flag on the corner of 2, and no flag to the side of 1. This method searches for that pattern.
        /// </summary>
        public void analyzeDoubles()
        {
            //Horizontal check
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols - 2; j++)
                {
                    //Checking for 3 consecutive, unflagged && unrevealed squares
                    if (grid[i][j].getRevealed() || grid[i][j + 1].getRevealed() || grid[i][j + 2].getRevealed() ||
                        grid[i][j].isFlagged() || grid[i][j + 1].isFlagged() || grid[i][j + 2].isFlagged())
                        continue;

                    //Checking square beneath common three
                    if (i < rows - 1)
                    {
                        //Checking to see the 3 squares are the only unmarked squares
                        int numFlagged = 0;
                        int numRevealed = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + 1 + a < 0) || (i + 1 + a >= rows))
                            {
                                goto Above;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + 1 + b < 0) || (j + 1 + b >= cols))
                                {
                                    goto Above;
                                }
                                if (grid[i + 1 + a][j + 1 + b].isFlagged())
                                    numFlagged++;
                                if (grid[i + 1 + a][j + 1 + b].getRevealed())
                                    numRevealed++;
                            }
                        }

                        if ((numRevealed + numFlagged) != 6)
                            goto Above;

                        //Checking that square has 2 mines left
                        if (grid[i + 1][j + 1].getValue() - numFlagged != 2)
                            goto Above;

                        Square left = grid[i + 1][j];
                        Square right = grid[i + 1][j + 2];

                        //Checking how many flags have been placed for left square
                        int leftFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + 1 + a < 0) || (i + 1 + a >= rows))
                            {
                                goto Above;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + b < 0) || (j + b >= cols))
                                {
                                    goto Above;
                                }
                                if (grid[i + 1 + a][j + b].isFlagged())
                                    leftFlag++;
                            }
                        }

                        //Checking how many squares have been placed for right square
                        int rightFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + 1 + a < 0) || (i + 1 + a >= rows))
                            {
                                goto Above;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + 2 + b < 0) || (j + 2 + b >= cols))
                                {
                                    goto Above;
                                }
                                if (grid[i + 1 + a][j + 2 + b].isFlagged())
                                    rightFlag++;
                            }
                        }

                        //If there is only one mine left on left square, the following applies
                        if (left.getValue() - leftFlag == 1)
                        {
                            flagSquare(i, j + 2);
                            selectSquare(i, j - 1);
                            selectSquare(i + 1, j - 1);
                            selectSquare(i + 2, j - 1);
                        }

                        //If there is only one mine left on right square, the following applies
                        if (right.getValue() - rightFlag == 1)
                        {
                            flagSquare(i, j);
                            selectSquare(i, j + 3);
                            selectSquare(i + 1, j + 3);
                            selectSquare(i + 2, j + 3);
                        }

                    }

                    Above:
                    //Checking square above the common 3
                    if(i > 0)
                    {
                        //Checking to see the 3 squares are the only unmarked squares
                        int numFlagged = 0;
                        int numRevealed = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i - 1 + a < 0) || (i - 1 + a >= rows))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + 1 + b < 0) || (j + 1 + b >= cols))
                                {
                                    continue;
                                }
                                if (grid[i - 1 + a][j + 1 + b].isFlagged())
                                    numFlagged++;
                                if (grid[i - 1 + a][j + 1 + b].getRevealed())
                                    numRevealed++;
                            }
                        }

                        if ((numRevealed + numFlagged) != 6)
                            continue;

                        //Checking that square has 2 mines left
                        if (grid[i - 1][j + 1].getValue() - numFlagged != 2)
                            continue;

                        Square left = grid[i - 1][j];
                        Square right = grid[i - 1][j + 2];

                        //Checking how many flags have been placed for left square
                        int leftFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i - 1 + a < 0) || (i - 1 + a >= rows))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + b < 0) || (j + b >= cols))
                                {
                                    continue;
                                }
                                if (grid[i - 1 + a][j + b].isFlagged())
                                    leftFlag++;
                            }
                        }

                        //Checking how many squares have been placed for right square
                        int rightFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i - 1 + a < 0) || (i - 1 + a >= rows))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + 2 + b < 0) || (j + 2 + b >= cols))
                                {
                                    continue;
                                }
                                if (grid[i - 1 + a][j + 2 + b].isFlagged())
                                    rightFlag++;
                            }
                        }

                        //If there is only one mine left on left square, the following applies
                        if (left.getValue() - leftFlag == 1)
                        {
                            flagSquare(i, j + 2);
                            selectSquare(i, j - 1);
                            selectSquare(i - 1, j - 1);
                            selectSquare(i - 2, j - 1);
                        }

                        //If there is only one mine left on right square, the following applies
                        if (right.getValue() - rightFlag == 1)
                        {
                            flagSquare(i, j);
                            selectSquare(i, j + 3);
                            selectSquare(i - 1, j + 3);
                            selectSquare(i - 2, j + 3);
                        }
                    }
                }
            }

            //Checking vertical
            for (int i = 0; i < rows - 2; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //Checking for 3 consecutive, unflagged && unrevealed squares
                    if (grid[i][j].getRevealed() || grid[i + 1][j].getRevealed() || grid[i + 2][j].getRevealed() ||
                        grid[i][j].isFlagged() || grid[i + 1][j].isFlagged() || grid[i + 2][j].isFlagged())
                        continue;

                    //Checking square right of common three
                    if (j < cols - 1)
                    {
                        //Checking to see the 3 squares are the only unmarked squares
                        int numFlagged = 0;
                        int numRevealed = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + 1 + a < 0) || (i + 1 + a >= rows))
                            {
                                goto Left;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + 1 + b < 0) || (j + 1 + b >= cols))
                                {
                                    goto Left;
                                }
                                if (grid[i + 1 + a][j + 1 + b].isFlagged())
                                    numFlagged++;
                                if (grid[i + 1 + a][j + 1 + b].getRevealed())
                                    numRevealed++;
                            }
                        }

                        if ((numRevealed + numFlagged) != 6)
                            goto Left;

                        //Checking that square has 2 mines left
                        if (grid[i + 1][j + 1].getValue() - numFlagged != 2)
                            goto Left;

                        Square up = grid[i][j + 1];
                        Square down = grid[i + 2][j + 1];

                        //Checking how many flags have been placed for up square
                        int upFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + a < 0) || (i + a >= rows))
                            {
                                goto Left;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + 1 + b < 0) || (j + 1 + b >= cols))
                                {
                                    goto Left;
                                }
                                if (grid[i + a][j + 1 + b].isFlagged())
                                    upFlag++;
                            }
                        }

                        //Checking how many squares have been placed for down square
                        int downFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + 2 + a < 0) || (i + 2 + a >= rows))
                            {
                                goto Left;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + 1 + b < 0) || (j + 1 + b >= cols))
                                {
                                    goto Left;
                                }
                                if (grid[i + 2 + a][j + 1 + b].isFlagged())
                                    downFlag++;
                            }
                        }

                        //If there is only one mine left on up square, the following applies
                        if (up.getValue() - upFlag == 1)
                        {
                            flagSquare(i + 2, j);
                            selectSquare(i - 1, j);
                            selectSquare(i - 1, j + 1);
                            selectSquare(i - 1, j + 2);
                        }

                        //If there is only one mine left on down square, the following applies
                        if (down.getValue() - downFlag == 1)
                        {
                            flagSquare(i, j);
                            selectSquare(i + 3, j);
                            selectSquare(i + 3, j + 1);
                            selectSquare(i + 3, j + 2);
                        }

                    }

                    Left: 
                    //Checking square left of the common 3
                    if (j > 0)
                    {
                        //Checking to see the 3 squares are the only unmarked squares
                        int numFlagged = 0;
                        int numRevealed = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + 1 + a < 0) || (i + 1 + a >= rows))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + b < 1) || (j - 1 + b >= cols))
                                {
                                    continue;
                                }
                                if (grid[i + 1 + a][j - 1 + b].isFlagged())
                                    numFlagged++;
                                if (grid[i + 1 + a][j - 1 + b].getRevealed())
                                    numRevealed++;
                            }
                        }

                        if ((numRevealed + numFlagged) != 6)
                            continue;

                        //Checking that square has 2 mines left
                        if (grid[i + 1][j - 1].getValue() - numFlagged != 2)
                            continue;

                        Square up = grid[i][j - 1];
                        Square down = grid[i + 2][j - 1];

                        //Checking how many flags have been placed for up square
                        int upFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + a < 0) || (i + a >= rows))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j + b < 1) || (j - 1 + b >= cols))
                                {
                                    continue;
                                }
                                if (grid[i + a][j - 1 + b].isFlagged())
                                    upFlag++;
                            }
                        }

                        //Checking how many squares have been placed for down square
                        int downFlag = 0;
                        for (int a = -1; a < 2; a++)
                        {
                            if ((i + 2 + a < 0) || (i + 2 + a >= rows))
                            {
                                continue;
                            }
                            for (int b = -1; b < 2; b++)
                            {
                                if ((j - 1 + b < 0) || (j - 1 + b >= cols))
                                {
                                    continue;
                                }
                                if (grid[i + 2 + a][j - 1 + b].isFlagged())
                                    downFlag++;
                            }
                        }

                        //If there is only one mine left on up square, the following applies
                        if (up.getValue() - upFlag == 1)
                        {
                            flagSquare(i + 2, j);
                            selectSquare(i - 1, j);
                            selectSquare(i - 1, j - 1);
                            selectSquare(i - 1, j - 2);
                        }

                        //If there is only one mine left on down square, the following applies
                        if (down.getValue() - downFlag == 1)
                        {
                            flagSquare(i, j);
                            selectSquare(i + 3, j);
                            selectSquare(i + 3, j - 1);
                            selectSquare(i + 3, j - 2);
                        }
                    }
                }
            }
        }

        class Square
        {
            private int value;
            private bool revealed;
            private bool flagged;
            private bool death;     //Indicates this square was a mine and selected
            private bool mine;      //Indicates this square is a mine
            private bool d;

            public Square()
            {
                revealed = false;
                flagged = false;
                death = false;
                mine = false;
                d = false;
            }

            public void setValue(int v)
            {
                value = v;
                revealed = true;
            }

            public void flag()
            {
                if (revealed)
                {
                    return;
                }
                flagged = true;
            }

            public void boom()
            {
                mine = true;
            }

            public void ouch()
            {
                death = true;
            }

            public void done()
            {
                d = true;
            }

            public int getValue()
            {
                return value;
            }

            public bool getRevealed()
            {
                return revealed;
            }

            public bool isFlagged()
            {
                return flagged;
            }

            public bool isDeath()
            {
                return death;
            }

            public bool isMine()
            {
                return mine;
            }

            public bool isDone()
            {
                return d;
            }
        }

        public void display()
        {
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    if (grid[i][j].getRevealed())
                    {
                        if (grid[i][j].getValue() == 0)
                        {
                            Console.Write("   ");
                        }
                        else
                        {
                            Console.Write(" " + grid[i][j].getValue() + " ");
                        }
                    }
                    else if (grid[i][j].isFlagged())
                    {
                        Console.Write("[!]");
                    } else if (grid[i][j].isMine())
                    {
                        Console.Write("[x]");
                    } else if (grid[i][j].isDeath())
                    {
                        Console.Write("END");
                    } else
                    {
                        Console.Write("[ ]");
                    }
                }
                Console.WriteLine();
            }
        }

    }
}
