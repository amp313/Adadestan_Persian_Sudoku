using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BoardPanel : MonoBehaviour
{
    #region UI References
    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text timeText;
    public Image poemImage;
    public Sprite[] poemSprites;
    #endregion

    #region Private Fields
    private int stageScore;
    private bool scoreAdded = false;
    #endregion

    #region Win Panel & Effects
    [Header("Win Panel & Effects")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private UIConfetti confetti;
    #endregion

    #region Public Methods
    public void ShowFinalScore(float remainingTime, SudokuGenerator.Difficulty difficulty)
    {
        int difficultyMultiplier = difficulty == SudokuGenerator.Difficulty.Easy ? 1 :
                                   difficulty == SudokuGenerator.Difficulty.Medium ? 2 : 3;

        int thirtySecondBlocks = Mathf.FloorToInt(remainingTime / 30f);
        stageScore = thirtySecondBlocks * 10 * difficultyMultiplier;

        if (!scoreAdded)
        {
            GameSettings.TotalScore += stageScore;
            GameSettings.SaveTotalScore();
            scoreAdded = true;
        }

        if (scoreText != null)
            scoreText.text = ToPersianNumber(stageScore.ToString());

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timeText != null)
            timeText.text = ToPersianNumber(timeString);

        int digitSum = SumOfDigits(minutes, seconds) - 1;

        if (poemSprites != null && poemSprites.Length > digitSum)
        {
            poemImage.sprite = poemSprites[digitSum];
            poemImage.gameObject.SetActive(true);
        }
        else
        {
            poemImage.gameObject.SetActive(false);
        }

        if (winPanel != null)
            winPanel.SetActive(true);

        if (confetti != null)
            confetti.Play(100);
    }

    public void ResetPanel()
    {
        stageScore = 0;
        scoreAdded = false;

        if (scoreText != null) 
            scoreText.text = ToPersianNumber("۰");

        if (timeText != null) 
            timeText.text = ToPersianNumber("۰۰:۰۰");

        if (poemImage != null) 
            poemImage.gameObject.SetActive(false);
    }

    public void OnNewGameButton()
    {
        SceneManager.LoadScene("level_game");
    }

    public void OnHomeButton()
    {
        SceneManager.LoadScene("main_menu");
    }
    #endregion

    #region Private Methods
    private int SumOfDigits(int minutes, int seconds)
    {
        int sum = 0;
        sum += (minutes / 10) + (minutes % 10);
        sum += (seconds / 10) + (seconds % 10);
        return sum;
    }

    private string ToPersianNumber(string input)
    {
        return input.Replace("0", "۰").Replace("1", "۱").Replace("2", "۲")
                    .Replace("3", "۳").Replace("4", "۴").Replace("5", "۵")
                    .Replace("6", "۶").Replace("7", "۷").Replace("8", "۸")
                    .Replace("9", "۹");
    }
    #endregion
}
