using UnityEngine;
using System;
using System.Collections.Generic;

public class SudokuGenerator : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }

    private int[,] solutionGrid;

    #region Public API
    public int[,] GeneratePuzzle(Difficulty difficulty)
    {
        solutionGrid = new int[9, 9];
        FillGrid(solutionGrid);

        int[,] puzzle = (int[,])solutionGrid.Clone();
        RemoveNumbers(puzzle, difficulty);

        return puzzle;
    }

    public int[,] GetSolution() => solutionGrid;

    public int GetSolutionNumber(int row, int col) => solutionGrid[row, col];
    #endregion

    #region Grid Generation
    private bool FillGrid(int[,] grid)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (grid[row, col] == 0)
                {
                    List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    Shuffle(numbers);

                    foreach (int num in numbers)
                    {
                        if (IsSafe(grid, row, col, num))
                        {
                            grid[row, col] = num;
                            if (FillGrid(grid)) return true;
                            grid[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    private bool IsSafe(int[,] grid, int row, int col, int num)
    {
        for (int x = 0; x < 9; x++)
            if (grid[row, x] == num || grid[x, col] == num)
                return false;

        int startRow = row - row % 3;
        int startCol = col - col % 3;

        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (grid[startRow + r, startCol + c] == num)
                    return false;

        return true;
    }
    #endregion

    #region Puzzle Creation
    private void RemoveNumbers(int[,] grid, Difficulty difficulty)
    {
        int removeTarget = difficulty == Difficulty.Easy ? 30 :
                           difficulty == Difficulty.Medium ? 45 : 55;

        System.Random rand = new System.Random();
        int removed = 0;

        while (removed < removeTarget)
        {
            int row = rand.Next(0, 9);
            int col = rand.Next(0, 9);

            if (grid[row, col] == 0) continue;

            int backup = grid[row, col];
            grid[row, col] = 0;

            if (!HasUniqueSolution((int[,])grid.Clone()))
                grid[row, col] = backup;
            else
                removed++;
        }
    }
    #endregion

    #region Solver
    private bool HasUniqueSolution(int[,] puzzle)
    {
        int solutions = 0;
        SolveSudoku(puzzle, ref solutions);
        return solutions == 1;
    }

    private bool SolveSudoku(int[,] grid, ref int solutions)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (grid[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (IsSafe(grid, row, col, num))
                        {
                            grid[row, col] = num;
                            if (SolveSudoku(grid, ref solutions)) return true;
                            grid[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }

        solutions++;
        if (solutions > 1) return true;
        return false;
    }
    #endregion
}
