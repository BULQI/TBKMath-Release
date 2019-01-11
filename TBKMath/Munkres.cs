using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    // adapted from R.A. Pilgrim http://csclab.murraystate.edu/bob.pilgrim/445/munkres.html
    // by TBKepler 8 November 2014 Boston University

    /*
    The MIT License (MIT)

    Copyright (c) 2000 Robert A. Pilgrim
                       Murray State University
                       Dept. of Computer Science & Information Systems
                       Murray,Kentucky

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

*/


    public class Munkres
    {
        enum mark { None, Star, Prime}

        private int[,] overlapMatrix;
        int[,] originalMatrix;
        private int nrow;
        private int ncol;

        private mark[,] marks;

        private bool[] rowCovered;
        private bool[] colCovered;
        private int step;

        public Munkres(int[,] overlapMatrix)
        {
            originalMatrix = overlapMatrix;
            nrow = overlapMatrix.GetLength(0);
            ncol = overlapMatrix.GetLength(1);
            this.overlapMatrix = new int[nrow, ncol];
            for (int i = 0; i < nrow; i++)
            {
                for (int j = 0; j < ncol; j++)
                {
                    this.overlapMatrix[i, j] = originalMatrix[i, j];
                }
            }
            marks = new mark[nrow, ncol];
            rowCovered = new bool[nrow];
            colCovered = new bool[ncol];
            step = 1;
        }

        /// <summary>
        ///  Runs the Pilgrim version of the Munkres algorithm solving the assignment problem.
        /// </summary>
        /// <returns>The first item in the tuple is the value of the optimal assignment. The second 
        /// item is the assignment vector. The indices of the assignment vector are the row indices 
        /// of the overlap matrix; the values are the indices of the columns in the overlap matrix assigned
        /// to the row index.</returns>
        public Tuple<int,int[]> Run()
        {
            bool done = false;
            int value = 0;
            subtractMaxes();
            star();
            int[] assignments = null;
            int[] path0 = new int[2];
            while (!done)
            {
                switch (step)
                {
                    case 3:
                        coverColumns();
                        break;
                    case 4:
                        path0 = primeZeros();
                        break;
                    case 5:
                        restar(path0);
                        break;
                    case 6:
                        transformByCovers();
                        break;
                    case 7:
                        assignments = getAssignments();
                        value = GetValue(assignments);
                        done = true;
                        break;
                }
            }
            return new Tuple<int, int[]>(value, assignments);
        }

        //For each row of the overlap matrix, find the largest element and subtract
        //it from every element in its row.  When finished, Go to Step 2.
        private void subtractMaxes()
        {
            int max_in_row;

            for (int r = 0; r < nrow; r++)
            {
                max_in_row = int.MinValue;
                for (int c = 0; c < ncol; c++)
                {
                    if (overlapMatrix[r, c] > max_in_row)
                    {
                        max_in_row = overlapMatrix[r, c];
                    }
                }
                for (int c = 0; c < ncol; c++)
                {
                    overlapMatrix[r, c] -= max_in_row;
                }
            }
            step = 2;
        }

        //Find a zero (Z) in the resulting matrix.  If there is no starred 
        //zero in its row or column, star Z. Repeat for each element in the 
        //matrix. Go to Step 3.
        // Starred means covered in both row and column coverings
        private void star()
        {
            for (int r = 0; r < nrow; r++)
            {
                for (int c = 0; c < ncol; c++)
                {
                    if (overlapMatrix[r, c] == 0 && !rowCovered[r] && !colCovered[c])
                    {
                        marks[r, c] = mark.Star;
                        rowCovered[r] = true;
                        colCovered[c] = true;
                    }
                }
            }
            clear_covers();
            step = 3;
        }

        //Cover each column containing a starred zero.  If K columns are covered, 
        //the starred zeros describe a complete set of unique assignments.  In this 
        //case, Go to DONE, otherwise, Go to Step 4.
        private void coverColumns()
        {
            int colcount;
            for (int r = 0; r < nrow; r++)
            {
                for (int c = 0; c < ncol; c++)
                {
                    if (marks[r, c] == mark.Star)
                    {
                        colCovered[c] = true;
                    }
                }
            }
            colcount = 0;
            for (int c = 0; c < ncol; c++)
            {
                if (colCovered[c])
                {
                    colcount += 1;
                }
            }
            if (colcount >= ncol || colcount >= nrow)
            {
                step = 7;
            }
            else
            {
                step = 4;
            }
        }

        //methods to support step 4
        private int[] findAZero()
        {
            int r = 0;
            int c;
            bool done;
            int row = -1;
            int col = -1;
            done = false;
            while (!done)
            {
                c = 0;
                while (true)
                {
                    if (overlapMatrix[r, c] == 0 && !rowCovered[r] && !colCovered[c])
                    {
                        row = r;
                        col = c;
                        done = true;
                    }
                    c += 1;
                    if (c >= ncol || done)
                        break;
                }
                r += 1;
                if (r >= nrow)
                    done = true;
            }
            return new int[] { row, col };
        }

        private bool thereIsStarInRow(int row)
        {
            bool tmp = false;
            for (int c = 0; c < ncol; c++)
            {
                if (marks[row, c] == mark.Star)
                {
                    tmp = true;
                    break;
                }
            }
            return tmp;
        }

        private int findStarInRow(int row)
        {
            for (int c = 0; c < ncol; c++)
            {
                if (marks[row, c] == mark.Star)
                {
                    return c;
                }
            }
            return -1;
        }

        //Find a noncovered zero and prime it.  If there is no starred zero 
        //in the row containing this primed zero, Go to Step 5.  Otherwise, 
        //cover this row and uncover the column containing the starred zero. 
        //Continue in this manner until there are no uncovered zeros left. 
        //Save the smallest uncovered value and Go to Step 6.
        private int[] primeZeros()
        {
            int row = -1;
            int col = -1;
            bool done;

            int[] path0 = new int[2];
            done = false;
            while (!done)
            {
                int[] zero = findAZero();
                row = zero[0];
                col = zero[1];
                if (row == -1)
                {
                    done = true;
                    step = 6;
                }
                else
                {
                    marks[row, col] = mark.Prime;
                    if (thereIsStarInRow(row))
                    {
                        col = findStarInRow(row);
                        rowCovered[row] = true;
                        colCovered[col] = false;
                    }
                    else
                    {
                        done = true;
                        step = 5;
                        path0[0] = row;
                        path0[1] = col;
                    }
                }
            }
            return path0;
        }

        // methods to support step 5
        private int find_star_in_col(int c)
        {
            int r = -1;
            for (int i = 0; i < nrow; i++)
            {
                if (marks[i, c] == mark.Star)
                {
                    r = i;
                }
            }
            return r;
        }

        private int find_prime_in_row(int r)
        {
            int c = -1;
            for (int j = 0; j < ncol; j++)
            {
                if (marks[r, j] == mark.Prime)
                {
                    c = j;
                }
            }
            return c;
        }

        private void augment_path(int pathCount, int[,] pathe)
        {
            for (int p = 0; p < pathCount; p++)
            {
                if (marks[pathe[p, 0], pathe[p, 1]] == mark.Star)
                {
                    marks[pathe[p, 0], pathe[p, 1]] = mark.None;
                }
                else
                {
                    marks[pathe[p, 0], pathe[p, 1]] = mark.Star;
                }
            }
        }

        private void clear_covers()
        {
            for (int r = 0; r < nrow; r++)
            {
                rowCovered[r] = false;
            }
            for (int c = 0; c < ncol; c++)
            {
                colCovered[c] = false;
            }
        }

        private void erase_primes()
        {
            for (int r = 0; r < nrow; r++)
            {
                for (int c = 0; c < ncol; c++)
                {
                    if (marks[r, c] == mark.Prime)
                    {
                        marks[r, c] = mark.None;
                    }
                }
            }
        }

        //Construct a series of alternating primed and starred zeros as follows.  
        //Let Z0 represent the uncovered primed zero found in Step 4.  Let Z1 denote 
        //the starred zero in the column of Z0 (if any). Let Z2 denote the primed zero 
        //in the row of Z1 (there will always be one).  Continue until the series 
        //terminates at a primed zero that has no starred zero in its column.  
        //Unstar each starred zero of the series, star each primed zero of the series, 
        //erase all primes and uncover every line in the matrix.  Return to Step 3.
        private  void restar(int[] path0)
        {
            bool done;
            int r = -1;
            int c = -1;

            int path_count = 1;
            int[,] path = new int[nrow + ncol + 1, 2];
            path[path_count - 1, 0] = path0[0];
            path[path_count - 1, 1] = path0[1];
            done = false;
            while (!done)
            {
                r = find_star_in_col(path[path_count - 1, 1]);
                if (r > -1)
                {
                    path_count += 1;
                    path[path_count - 1, 0] = r;
                    path[path_count - 1, 1] = path[path_count - 2, 1];
                }
                else
                {
                    done = true;
                }
                if (!done)
                {
                    c = find_prime_in_row(path[path_count - 1, 0]);
                    path_count += 1;
                    path[path_count - 1, 0] = path[path_count - 2, 0];
                    path[path_count - 1, 1] = c;
                }
            }
            augment_path(path_count, path);
            clear_covers();
            erase_primes();
            step = 3;
        }

        //methods to support step 6
        private int find_largest()
        {
            int maxval = int.MinValue;
            for (int r = 0; r < nrow; r++)
            {
                for (int c = 0; c < ncol; c++)
                {
                    if (!rowCovered[r] && !colCovered[c])
                    {
                        if (overlapMatrix[r, c] > maxval)
                        {
                            maxval = overlapMatrix[r, c];
                        }
                    }
                }
            }
            return maxval;
        }

        //Add the value found in Step 4 to every element of each covered row, and subtract 
        //it from every element of each uncovered column.  Return to Step 4 without 
        //altering any stars, primes, or covered lines.
        private void transformByCovers()
        {
            int maxval = find_largest();
            for (int r = 0; r < nrow; r++)
            {
                for (int c = 0; c < ncol; c++)
                {
                    if (rowCovered[r])
                    {
                        overlapMatrix[r, c] += maxval;
                    }
                    if (!colCovered[c])
                    {
                        overlapMatrix[r, c] -= maxval;
                    }
                }
            }
            step = 4;
        }

        /// <summary>
        /// Gets the column index in the optimal assignment for each row index
        /// </summary>
        /// <returns>The entries in the return vector are the column indices where the row indices are the indices of the return vector.</returns>
        private int[] getAssignments()
        {
            int[] assignments = new int[nrow];
            for (int i = 0; i < nrow; i++)
            {
                assignments[i] = findStarInRow(i);
            }
            return assignments;
        }

        /// <summary>
        /// Computes the value of the assignment.
        /// </summary>
        /// <param name="assignments">The assignment array giving the column index for each row index.</param>
        /// <returns>The value of the assignment on the original Matrix.</returns>
        private int GetValue(int[] assignments)
        {
            int value = 0;
            for (int i = 0; i < assignments.Length; i++)
            {
                if (assignments[i] > -1)
                {
                    value += originalMatrix[i, assignments[i]];
                }
            }
            return value;
        }

        private static int[,] transpose(int[,] array)
        {
            int nRows = array.GetLength(1);
            int nCols = array.GetLength(0);

            int[,] newArray = new int[nRows, nCols];
            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    newArray[i, j] = array[j, i];
                }
            }
            return newArray;
        }
    }

}
