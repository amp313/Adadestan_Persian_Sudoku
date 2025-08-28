using System.Linq;
using UnityEngine;

public static class GameSettings
{
    #region Variables

    public static float RemainingTime = 0f;
    public static SudokuGenerator.Difficulty Difficulty = SudokuGenerator.Difficulty.Easy;
    public static int TotalScore = 0;
    public static int RemainingLives = 3;
    public static int HintUsedCount = 0;
    public static int[,] CurrentPuzzle;
    public static int[,] OriginalPuzzle;
    public static bool IsGameIncomplete = false;

    private const string TotalScoreKey = "TotalScore";
    private const string LivesKey = "RemainingLives";
    private const string RemainingTimeKey = "RemainingTime";
    private const string DifficultyKey = "Difficulty";
    private const string HintUsedKey = "HintUsed";
    private const string PuzzleKey = "Puzzle";
    private const string OriginalPuzzleKey = "OriginalPuzzle";
    private const string IncompleteKey = "IsGameIncomplete";

    public static bool EraserMode = false;
    public static bool PencilMode = false;
    public static bool HelpMode = false;
    public static bool[] NumberButtonActive = new bool[9];
    public static int HintButtonState = 0;

    #endregion

    #region Save / Load

    public static void SaveGame()
    {
        PlayerPrefs.SetFloat(RemainingTimeKey, RemainingTime);
        PlayerPrefs.SetInt(DifficultyKey, (int)Difficulty);
        PlayerPrefs.SetInt(LivesKey, RemainingLives);
        PlayerPrefs.SetInt(HintUsedKey, HintUsedCount);

        PlayerPrefs.SetString(PuzzleKey, SerializeGrid(CurrentPuzzle));
        PlayerPrefs.SetString(OriginalPuzzleKey, SerializeGrid(OriginalPuzzle));

        PlayerPrefs.SetInt("EraserMode", EraserMode ? 1 : 0);
        PlayerPrefs.SetInt("PencilMode", PencilMode ? 1 : 0);
        PlayerPrefs.SetInt("HelpMode", HelpMode ? 1 : 0);

        PlayerPrefs.SetInt(IncompleteKey, IsGameIncomplete ? 1 : 0);
        PlayerPrefs.SetString("NumberButtonActive", string.Join(",", NumberButtonActive.Select(b => b ? "1" : "0")));
        PlayerPrefs.Save();
    }

    public static void LoadGame()
    {
        RemainingTime = PlayerPrefs.GetFloat(RemainingTimeKey, 0f);
        Difficulty = (SudokuGenerator.Difficulty)PlayerPrefs.GetInt(DifficultyKey, 0);
        RemainingLives = PlayerPrefs.GetInt(LivesKey, 3);
        HintUsedCount = PlayerPrefs.GetInt(HintUsedKey, 0);

        CurrentPuzzle = DeserializeGrid(PlayerPrefs.GetString(PuzzleKey));
        OriginalPuzzle = DeserializeGrid(PlayerPrefs.GetString(OriginalPuzzleKey));

        EraserMode = PlayerPrefs.GetInt("EraserMode", 0) == 1;
        PencilMode = PlayerPrefs.GetInt("PencilMode", 0) == 1;
        HelpMode = PlayerPrefs.GetInt("HelpMode", 0) == 1;

        string str = PlayerPrefs.GetString("NumberButtonActive", "1,1,1,1,1,1,1,1,1");
        string[] parts = str.Split(',');
        for (int i = 0; i < 9; i++)
            NumberButtonActive[i] = parts[i] == "1";

        IsGameIncomplete = PlayerPrefs.GetInt(IncompleteKey, 0) == 1;
    }

    public static void SaveTotalScore()
    {
        PlayerPrefs.SetInt(TotalScoreKey, TotalScore);
        PlayerPrefs.Save();
    }

    public static void LoadTotalScore()
    {
        TotalScore = PlayerPrefs.GetInt(TotalScoreKey, 0);
    }

    #endregion

    #region Helper Methods

    private static string SerializeGrid(int[,] grid)
    {
        if (grid == null) return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(81 * 2);
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                sb.Append(grid[r, c]);
                if (!(r == 8 && c == 8)) sb.Append(',');
            }
        }
        return sb.ToString();
    }

    private static int[,] DeserializeGrid(string str)
    {
        if (string.IsNullOrEmpty(str)) return new int[9, 9];
        string[] parts = str.Split(',');
        int[,] grid = new int[9, 9];
        for (int i = 0; i < 81; i++)
        {
            int r = i / 9;
            int c = i % 9;
            grid[r, c] = int.Parse(parts[i]);
        }
        return grid;
    }

    #endregion

    #region Clear

    public static void ClearSavedGame()
    {
        PlayerPrefs.DeleteKey(RemainingTimeKey);
        PlayerPrefs.DeleteKey(DifficultyKey);
        PlayerPrefs.DeleteKey(LivesKey);
        PlayerPrefs.DeleteKey(HintUsedKey);
        PlayerPrefs.DeleteKey(PuzzleKey);
        PlayerPrefs.DeleteKey(OriginalPuzzleKey);
        PlayerPrefs.DeleteKey("EraserMode");
        PlayerPrefs.DeleteKey("PencilMode");
        PlayerPrefs.DeleteKey("HelpMode");
        PlayerPrefs.DeleteKey("NumberButtonActive");
        PlayerPrefs.DeleteKey(IncompleteKey);

        RemainingTime = 0f;
        Difficulty = SudokuGenerator.Difficulty.Easy;
        RemainingLives = 3;
        HintUsedCount = 0;
        CurrentPuzzle = new int[9, 9];
        OriginalPuzzle = new int[9, 9];
        EraserMode = false;
        PencilMode = false;
        HelpMode = false;
        NumberButtonActive = Enumerable.Repeat(true, 9).ToArray();
        HintButtonState = 0;
        IsGameIncomplete = false;

        PlayerPrefs.Save();
    }

    #endregion
}
