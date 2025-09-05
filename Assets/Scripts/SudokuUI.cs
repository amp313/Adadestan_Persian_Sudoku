using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using TapsellPlusSDK;
using UnityEngine.SceneManagement;

public class SudokuUI : MonoBehaviour
{
    #region Ads
    private string tapsellAppKey = "lsttmlmshrntqjlfoktgpqkejhbqancdmikqiglorhctnlnlhghqhfiodkidcmjagqmane";
    private string rewardedZoneId = "68a49a7ee6b8427db138b005";
    private string rewardedResponseId = "";
    private static bool tapsellInitialized = false;
    #endregion

    #region Grid Settings
    public CellUI cellPrefab;
    public Transform[] blockParents;
    #endregion

    #region Backgrounds
    public Sprite background1Sprite;
    public Sprite background2Sprite;
    public Sprite selectedBackground1Sprite;
    public Sprite selectedBackground2Sprite;
    public Sprite highlightBackground1Sprite;
    public Sprite highlightBackground2Sprite;
    #endregion

    #region Game Logic
    public SudokuGenerator sudokuGenerator;
    public TMP_Text timerText;

    private CellUI[,] cells = new CellUI[9, 9];
    private int[,] puzzle;
    private int[,] originalPuzzle;
    private int selectedRow = -1;
    private int selectedCol = -1;

    public SudokuGenerator.Difficulty difficulty;
    private float remainingTime;
    private bool timerRunning = false;
    #endregion

    #region UI Buttons
    public Button[] numberButtons;
    public TMP_Text[] numberCountTexts;
    public Button eraserButton;
    public Button hintButton;
    public Button pencilButton;
    public Sprite pencilOnSprite;
    public Sprite pencilOffSprite;
    public Sprite hintNormalSprite;
    public Sprite hintAdSprite;
    #endregion

    #region Modes
    public bool eraserMode = false;
    public bool helpMode = false;
    public bool pencilMode = false;
    #endregion

    #region Hint Settings
    [SerializeField] private int maxHints = 2;
    private int hintUsedCount = 0;
    #endregion

    #region Panels
    public GameObject gameOverPanel;
    public GameObject successPanel;
    #endregion

    #region Sound Effect
    public AudioClip numberEnterClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (sudokuGenerator == null || timerText == null || cellPrefab == null || blockParents.Length != 9) return;

        difficulty = GameSettings.Difficulty;

        if (GameSettings.IsGameIncomplete && GameSettings.CurrentPuzzle != null)
        {
            puzzle = (int[,])GameSettings.CurrentPuzzle.Clone();
            originalPuzzle = (int[,])GameSettings.OriginalPuzzle.Clone();
            remainingTime = GameSettings.RemainingTime;
            hintUsedCount = GameSettings.HintUsedCount;
            eraserMode = GameSettings.EraserMode;
            pencilMode = GameSettings.PencilMode;
            helpMode = GameSettings.HelpMode;

            for (int i = 0; i < 9; i++)
                if (numberButtons[i] != null)
                    numberButtons[i].interactable = GameSettings.NumberButtonActive[i];

            if (hintButton != null)
                hintButton.GetComponent<Image>().sprite = GameSettings.HintButtonState == 0 ? hintNormalSprite : hintAdSprite;
        }
        else
        {
            puzzle = sudokuGenerator.GeneratePuzzle(difficulty);
            originalPuzzle = (int[,])puzzle.Clone();
            remainingTime = 0;
            hintUsedCount = 0;
        }

        BuildGrid();
        StartCoroutine(ShowNumbersWithFade());
        UpdateNumberButtonsState();

        if (!tapsellInitialized)
        {
            TapsellPlus.Initialize(
                tapsellAppKey,
                adNetworkName => { tapsellInitialized = true; },
                error => { }
            );
        }
    }

    private void Update()
    {
        if (!timerRunning) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime < 0) remainingTime = 0;

        GameSettings.RemainingTime = remainingTime;

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00}";

        if (remainingTime <= 0f && timerRunning)
        {
            PauseTimer();
            TriggerGameOver();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            BackToMenu();
    }
    #endregion

    #region Grid Setup
    private void BuildGrid()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int blockIndex = (r / 3) * 3 + (c / 3);
                var parent = blockParents[blockIndex];
                Sprite defaultBg = (blockIndex % 2 == 0) ? background1Sprite : background2Sprite;
                Sprite selectedBg = (blockIndex % 2 == 0) ? selectedBackground1Sprite : selectedBackground2Sprite;
                Sprite highlightBg = (blockIndex % 2 == 0) ? highlightBackground1Sprite : highlightBackground2Sprite;

                CellUI cell = Instantiate(cellPrefab, parent);
                cell.Init(r, c, this, defaultBg, selectedBg, highlightBg);
                cell.SetNumber(puzzle[r, c], puzzle[r, c] != 0);

                cells[r, c] = cell;
            }
        }
    }

    private IEnumerator ShowNumbersWithFade()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                CellUI cell = cells[r, c];
                if (cell != null && !string.IsNullOrEmpty(cell.numberText.text))
                    StartCoroutine(FadeInText(cell));
            }
            yield return new WaitForSeconds(0.05f);
        }
        StartTimerForDifficulty();
    }

    private IEnumerator FadeInText(CellUI cell)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2;
            cell.numberText.alpha = t;
            yield return null;
        }
        cell.numberText.alpha = 1;
    }
    #endregion

    #region Timer
    private void StartTimerForDifficulty()
    {
        switch (difficulty)
        {
            case SudokuGenerator.Difficulty.Easy: remainingTime = 300; break;
            case SudokuGenerator.Difficulty.Medium: remainingTime = 600; break;
            case SudokuGenerator.Difficulty.Hard: remainingTime = 900; break;
            default: remainingTime = 600; break;
        }
        timerRunning = true;
        GameSettings.RemainingTime = remainingTime;
    }

    public void AddExtraTime(float seconds)
    {
        remainingTime += seconds;
        GameSettings.RemainingTime = remainingTime;

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int sec = Mathf.FloorToInt(remainingTime % 60);
        if (timerText != null)
            timerText.text = $"{minutes:00}:{sec:00}";
    }


    public void PauseTimer() => timerRunning = false;
    public void ResumeTimer() { if (remainingTime > 0f) timerRunning = true; }
    public void ResetTimer() => StartTimerForDifficulty();
    public void BackToMenu()
    {
        SaveCurrentGame();
        SceneManager.LoadScene("main_menu");
    }

    private void SaveCurrentGame()
    {
        GameSettings.CurrentPuzzle = puzzle;
        GameSettings.OriginalPuzzle = originalPuzzle;
        GameSettings.RemainingTime = remainingTime;
        GameSettings.HintUsedCount = hintUsedCount;
        GameSettings.EraserMode = eraserMode;
        GameSettings.PencilMode = pencilMode;
        GameSettings.HelpMode = helpMode;
        for (int i = 0; i < 9; i++)
            GameSettings.NumberButtonActive[i] = numberButtons[i].interactable;
        GameSettings.HintButtonState = (hintUsedCount >= maxHints) ? 1 : 0;
        GameSettings.IsGameIncomplete = true;
        GameSettings.SaveGame();
    }
    #endregion

    #region Cell Selection
    public void SelectCell(int row, int col)
    {
        selectedRow = row;
        selectedCol = col;

        ResetAllHighlights();
        CellUI cell = cells[row, col];

        if (!string.IsNullOrEmpty(cell.numberText.text))
        {
            if (int.TryParse(cell.numberText.text, out int number))
                SetSameNumberColor(number, Color.blue);
        }
        else
        {
            HighlightCells(row, col);
            cell.SetSelected();
        }

        UpdateNumberButtonsState();
    }

    private void ResetAllHighlights()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                cells[r, c].ResetBackground();
                cells[r, c].UpdateTextColor();
            }
    }

    private void HighlightCells(int row, int col)
    {
        for (int i = 0; i < 9; i++)
        {
            cells[row, i].SetHighlight();
            cells[i, col].SetHighlight();
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int rr = 0; rr < 3; rr++)
            for (int cc = 0; cc < 3; cc++)
                cells[startRow + rr, startCol + cc].SetHighlight();
    }

    private void SetSameNumberColor(int number, Color color)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (!string.IsNullOrEmpty(cells[r, c].numberText.text) &&
                    int.Parse(cells[r, c].numberText.text) == number)
                    cells[r, c].SetNumberColor(color);
    }
    #endregion

    #region Number Entry
    public void OnNumberButtonClicked(int number)
    {
        if (selectedRow == -1 || selectedCol == -1) return;
        CellUI cell = cells[selectedRow, selectedCol];
        if (cell.isFixed) return;

        if (pencilMode)
            cell.SetPencilNumber(number);
        else
        {
            puzzle[selectedRow, selectedCol] = number;
            cell.SetNumber(number, false);
            cell.isUserFilled = true;
            cell.isSelectable = false;
            cell.ClearPencil();
            RemovePencilNumberFromPeers(selectedRow, selectedCol, number);

            if (SoundManager.Instance != null && numberEnterClip != null)
                SoundManager.Instance.PlaySFX(numberEnterClip);
        }

        ResetAllHighlights();
        SetSameNumberColor(number, Color.blue);
        UpdateNumberButtonsState();

        if (IsPuzzleFull())
            CheckPuzzleResult();
    }

    private void RemovePencilNumberFromPeers(int row, int col, int number)
    {
        for (int c = 0; c < 9; c++) if (c != col) cells[row, c].RemovePencilNumber(number);
        for (int r = 0; r < 9; r++) if (r != row) cells[r, col].RemovePencilNumber(number);

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int rr = 0; rr < 3; rr++)
            for (int cc = 0; cc < 3; cc++)
            {
                int r = startRow + rr;
                int c = startCol + cc;
                if (r != row || c != col)
                    cells[r, c].RemovePencilNumber(number);
            }
    }

    private bool IsPuzzleFull()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle[r, c] == 0)
                    return false;
        return true;
    }
    #endregion

    #region Eraser & Hint
    public void OnEraserCell(int row, int col)
    {
        CellUI cell = cells[row, col];
        if (!cell.isUserFilled) return;

        puzzle[row, col] = 0;
        cell.SetNumber(0, false);
        cell.isUserFilled = false;
        cell.isSelectable = true;

        eraserMode = false;
        UpdateNumberButtonsState();
        ResetAllHighlights();
    }

    public void OnHelpCell(int row, int col)
    {
        if (hintUsedCount >= maxHints) return;

        CellUI cell = cells[row, col];
        if (!string.IsNullOrEmpty(cell.numberText.text)) return;

        int solutionNumber = sudokuGenerator.GetSolution()[row, col];
        puzzle[row, col] = solutionNumber;
        cell.SetNumber(solutionNumber, false);
        cell.isUserFilled = true;
        cell.isSelectable = false;

        ResetAllHighlights();
        SetSameNumberColor(solutionNumber, Color.blue);

        helpMode = false;
        UpdateNumberButtonsState();

        hintUsedCount++;
        if (hintUsedCount >= maxHints && hintButton != null)
            hintButton.GetComponent<Image>().sprite = hintAdSprite;
    }

    public void OnEraserButtonClicked()
    {
        eraserMode = !eraserMode;
        helpMode = false;
        UpdateNumberButtonsState();
    }

    public void OnHintButtonClicked()
    {
        if (hintUsedCount < maxHints)
        {
            helpMode = !helpMode;
            eraserMode = false;
            UpdateNumberButtonsState();

            if (hintButton != null && hintNormalSprite != null)
                hintButton.GetComponent<Image>().sprite = hintNormalSprite;
        }
        else
        {
            if (hintButton != null && hintAdSprite != null)
                hintButton.GetComponent<Image>().sprite = hintAdSprite;
            GameSettings.HintButtonState = 1;
            RequestRewardedAd();
        }
    }
    #endregion

    #region Pencil
    public void OnPencilButtonClicked()
    {
        pencilMode = !pencilMode;
        eraserMode = false;
        helpMode = false;
        UpdateNumberButtonsState();

        if (selectedRow != -1 && selectedCol != -1)
            cells[selectedRow, selectedCol].SetPencilMode(pencilMode);

        if (pencilButton != null)
            pencilButton.GetComponent<Image>().sprite = pencilMode ? pencilOnSprite : pencilOffSprite;
    }
    #endregion

    #region Puzzle Result / Game Over
    private void CheckPuzzleResult()
    {
        int[,] solution = sudokuGenerator.GetSolution();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle[r, c] == 0 || puzzle[r, c] != solution[r, c])
                {
                    PauseTimer();
                    TriggerGameOver();
                    return;
                }

        ShowSuccess();
    }

    private void TriggerGameOver()
    {
        timerRunning = false;
        if (gameOverPanel == null) return;
        if (SoundManager.Instance != null && loseClip != null)
            SoundManager.Instance.PlaySFX(loseClip);

        gameOverPanel.SetActive(true);
        gameOverPanel.GetComponent<GameOverPanel>().ShowGameOver();
    }
    #endregion

    #region Number Buttons Update
    private void UpdateNumberButtonsState()
    {
        for (int num = 1; num <= 9; num++)
        {
            int count = CountNumberInPuzzle(num);
            int remaining = 9 - count;
            bool interactable = !(eraserMode || (helpMode && hintUsedCount < maxHints)) || (hintUsedCount >= maxHints);

            if (numberButtons != null && numberButtons.Length >= num && numberButtons[num - 1] != null)
                numberButtons[num - 1].interactable = interactable && remaining > 0;

            if (numberCountTexts != null && numberCountTexts.Length >= num && numberCountTexts[num - 1] != null)
                numberCountTexts[num - 1].text = remaining.ToString();
        }
    }

    private int CountNumberInPuzzle(int number)
    {
        int count = 0;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle[r, c] == number)
                    count++;
        return count;
    }
    #endregion

    #region Restart & Highlight
    public void RestartGame()
    {
        foreach (Transform parent in blockParents)
            foreach (Transform child in parent)
                Destroy(child.gameObject);

        puzzle = (int[,])originalPuzzle.Clone();
        BuildGrid();
        UpdateNumberButtonsState();

        hintUsedCount = 0;
        if (hintButton != null && hintNormalSprite != null)
            hintButton.GetComponent<Image>().sprite = hintNormalSprite;

        ResetTimer();
        if (successPanel != null) successPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        BoardPanel boardPanel = successPanel?.GetComponent<BoardPanel>() ?? successPanel?.GetComponentInChildren<BoardPanel>(true);
        if (boardPanel != null)
            boardPanel.ResetPanel();
    }

    public void HighlightWrongCells()
    {
        int[,] solution = sudokuGenerator.GetSolution();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                if (cells[r, c].isUserFilled)
                {
                    if (puzzle[r, c] != solution[r, c])
                        cells[r, c].SetNumberColor(Color.red);
                    else
                        cells[r, c].UpdateTextColor();
                }
            }
    }
    #endregion

    #region Success
    public void ShowSuccess()
    {
        timerRunning = false;
        if (successPanel == null) return;
        if (SoundManager.Instance != null && winClip != null)
            SoundManager.Instance.PlaySFX(winClip);

        successPanel.SetActive(true);
        BoardPanel boardPanel = successPanel.GetComponent<BoardPanel>() ?? successPanel.GetComponentInChildren<BoardPanel>(true);
        if (boardPanel != null)
            boardPanel.ShowFinalScore(remainingTime, difficulty);
    }
    #endregion

    #region Ads
    private void RequestRewardedAd()
    {
        rewardedResponseId = "";
        TapsellPlus.RequestRewardedVideoAd(
            rewardedZoneId,
            adModel =>
            {
                rewardedResponseId = adModel.responseId;
                ShowRewardedAd();
            },
            error => { }
        );
    }

    private void ShowRewardedAd()
    {
        if (string.IsNullOrEmpty(rewardedResponseId)) return;

        TapsellPlus.ShowRewardedVideoAd(
            rewardedResponseId,
            onOpen => { },
            onReward => { RestoreOneHint(); },
            onClose => { },
            error => { }
        );
    }

    private void RestoreOneHint()
    {
        hintUsedCount = maxHints - 1;
        if (hintButton != null && hintNormalSprite != null)
            hintButton.GetComponent<Image>().sprite = hintNormalSprite;
        UpdateNumberButtonsState();
    }
    #endregion
}
