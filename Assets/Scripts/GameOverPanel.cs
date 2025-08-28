using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TapsellPlusSDK;

public class GameOverPanel : MonoBehaviour
{
    #region Fields

    [Header("Buttons")]
    public Button restartButton;
    public Button continueButton;
    public Button newGameButton;

    [Header("Lives UI")]
    public Image[] lifeImages;
    public Sprite lifeNormalSprite;
    public Sprite lifeLostSprite;
    public int maxLives = 2;

    [Header("Ad (TapsellPlus)")]
    private string tapsellAppKey = "lsttmlmshrntqjlfoktgpqkejhbqancdmikqiglorhctnlnlhghqhfiodkidcmjagqmane";
    private string rewardedZoneId = "68a49a7ee6b8427db138b005";

    public SudokuUI sudokuUI;

    private string rewardedResponseId = "";
    private int retryCount = 0;
    private const int maxRetry = 3;
    private static bool tapsellInitialized = false;

    [SerializeField] private GameObject losePanel;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);

        if (!tapsellInitialized)
        {
            TapsellPlus.Initialize(
                tapsellAppKey,
                _ => { tapsellInitialized = true; },
                _ => { }
            );
        }
    }

    #endregion

    #region UI Control

    public void ShowGameOver()
    {
        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void Hide()
    {
        if (losePanel != null)
            losePanel.SetActive(false);
    }

    private void UpdateLivesUIFromSettings()
    {
        int remaining = GetRemainingLives();
        int lost = maxLives - remaining;

        if (lifeImages != null && lifeImages.Length > 0)
        {
            for (int i = 0; i < lifeImages.Length; i++)
            {
                if (lifeImages[i] == null) continue;
                lifeImages[i].sprite = (i < lost) ? lifeLostSprite : lifeNormalSprite;
            }
        }
    }

    private void UpdateRestartButtonInteractable()
    {
        if (restartButton != null)
            restartButton.interactable = GetRemainingLives() > 0;
    }

    #endregion

    #region Lives Handling

    private int GetRemainingLives()
    {
        return Mathf.Clamp(GameSettings.RemainingLives, 0, maxLives);
    }

    private void SetRemainingLives(int newVal)
    {
        GameSettings.RemainingLives = Mathf.Clamp(newVal, 0, maxLives);
    }

    #endregion

    #region Button Callbacks

    private void OnRestartClicked()
    {
        int remaining = GetRemainingLives();
        if (remaining <= 0) return;

        remaining -= 1;
        SetRemainingLives(remaining);
        UpdateLivesUIFromSettings();
        UpdateRestartButtonInteractable();

        if (sudokuUI != null)
        {
            sudokuUI.RestartGame();
            sudokuUI.ResumeTimer();
        }

        Hide();
    }

    private void OnContinueClicked()
    {
        if (continueButton != null)
            continueButton.interactable = false;

        RequestRewardedAd();
    }

    private void OnNewGameClicked()
    {
        SceneManager.LoadScene("level_game");
    }

    #endregion

    #region Rewarded Ads

    private void RequestRewardedAd()
    {
        retryCount = 0;
        rewardedResponseId = "";

        TapsellPlus.RequestRewardedVideoAd(
            rewardedZoneId,
            adModel =>
            {
                rewardedResponseId = adModel.responseId;
                retryCount = 0;
                ShowRewardedAd();
            },
            _ => { RetryRequestOrFail(); }
        );
    }

    private void RetryRequestOrFail()
    {
        retryCount++;
        if (retryCount <= maxRetry)
            RequestRewardedAd();
        else if (continueButton != null)
            continueButton.interactable = true;
    }

    private void ShowRewardedAd()
    {
        if (string.IsNullOrEmpty(rewardedResponseId))
        {
            if (continueButton != null)
                continueButton.interactable = true;

            return;
        }

        TapsellPlus.ShowRewardedVideoAd(
            rewardedResponseId,
            _ => { },
            _ =>
            {
                if (sudokuUI != null)
                {
                    sudokuUI.HighlightWrongCells();
                    sudokuUI.ResumeTimer();
                }
                Hide();
            },
            _ =>
            {
                if (continueButton != null)
                    continueButton.interactable = true;
            },
            _ =>
            {
                if (continueButton != null)
                    continueButton.interactable = true;
            }
        );
    }

    #endregion
}
