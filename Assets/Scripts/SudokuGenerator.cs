using UnityEngine;
using System;
using System.Collections.Generic;

public class SudokuGenerator : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }

    private int[,] solutionGrid;

    public int[,] GeneratePuzzle(Difficulty difficulty)
    {
        solutionGrid = new int[9, 9];
        FillGrid(solutionGrid);

        int[,] puzzle = (int[,])solutionGrid.Clone();
        RemoveNumbers(puzzle, difficulty);
        return puzzle;
    }

    public int[,] GetSolution()
    {
        return solutionGrid;
    }

    private bool FillGrid(int[,] grid)
    {
        List<int> numbers = new List<int>() { 1,2,3,4,5,6,7,8,9 };
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (grid[row, col] == 0)
                {
                    Shuffle(numbers);
                    foreach (int num in numbers)
                    {
                        if (IsSafe(grid, row, col, num))
                        {
                            grid[row, col] = num;
                            if (FillGrid(grid))
                                return true;
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
            int temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
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

    private void RemoveNumbers(int[,] grid, Difficulty difficulty)
    {
        int removeCount = difficulty == Difficulty.Easy ? 40 :
                          difficulty == Difficulty.Medium ? 50 : 60;

        while (removeCount > 0)
        {
            int row = UnityEngine.Random.Range(0, 9);
            int col = UnityEngine.Random.Range(0, 9);

            if (grid[row, col] != 0)
            {
                grid[row, col] = 0;
                removeCount--;
            }
        }
    }
}
