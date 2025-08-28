using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class LevelMenuController : MonoBehaviour
{
    #region Variables
    [Header("Buttons")]
    public GameObject[] levelButtons;
    public GameObject continuePanel;
    public TMP_Text totalScoreText;

    [Header("Sound")]
    public AudioClip clickSound;

    private float delay = 0.3f;
    private float fadeDuration = 0.3f;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        PrepareButtons();
        StartCoroutine(ShowButtonsSequentially());

        GameSettings.LoadTotalScore();
        UpdateScoreText();

        if (HasSavedGame())
        {
            if (continuePanel != null) continuePanel.SetActive(true);
        }
        else
        {
            if (continuePanel != null) continuePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMainMenu();
        }
    }
    #endregion

    #region Button Setup
    private void PrepareButtons()
    {
        foreach (var btn in levelButtons)
        {
            if (btn == null) continue;
            var cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            btn.transform.localScale = Vector3.one * 0.9f;
        }
    }

    private IEnumerator ShowButtonsSequentially()
    {
        foreach (var btn in levelButtons)
        {
            yield return new WaitForSeconds(delay);
            if (btn == null) continue;
            var cg = btn.GetComponent<CanvasGroup>();
            yield return StartCoroutine(FadeInAndScaleUp(cg, btn, fadeDuration));
        }
        EnableAllButtons();
    }

    private IEnumerator FadeInAndScaleUp(CanvasGroup cg, GameObject btn, float duration)
    {
        float time = 0f;
        Vector3 startScale = btn.transform.localScale;
        Vector3 targetScale = Vector3.one;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            btn.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        cg.alpha = 1f;
        btn.transform.localScale = targetScale;
    }

    private void EnableAllButtons()
    {
        foreach (var btn in levelButtons)
        {
            if (btn == null) continue;
            var cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) continue;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
    #endregion

    #region Score
    public void UpdateScoreText()
    {
        if (totalScoreText != null)
        {
            string persianScore = ToPersianNumber(GameSettings.TotalScore.ToString());
            totalScoreText.text = persianScore;

            string englishScore = GameSettings.TotalScore.ToString();
            totalScoreText.fontSize = englishScore.Length > 6 ? 60 : 80;
        }
    }

    private string ToPersianNumber(string input)
    {
        return input
            .Replace("0", "۰").Replace("1", "۱").Replace("2", "۲")
            .Replace("3", "۳").Replace("4", "۴").Replace("5", "۵")
            .Replace("6", "۶").Replace("7", "۷").Replace("8", "۸")
            .Replace("9", "۹");
    }
    #endregion

    #region Scene Loading
    private IEnumerator LoadSceneDelayed(string sceneName, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        SceneManager.LoadScene(sceneName);
    }

    public void SelectLevel(int level)
    {
        GameSettings.Difficulty = (SudokuGenerator.Difficulty)level;
        PlayClickSound();
        StartCoroutine(LoadSceneDelayed("main_game", 0.3f));
    }

    public void ContinueSavedGame()
    {
        PlayClickSound();
        if (continuePanel != null) continuePanel.SetActive(false);
        StartCoroutine(LoadSceneDelayed("main_game", 0.5f));
    }

    public void StartNewGame()
    {
        PlayClickSound();
        GameSettings.ClearSavedGame();
        if (continuePanel != null) continuePanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        PlayClickSound();
        SceneManager.LoadScene("main_menu");
    }
    #endregion

    #region Utilities
    private bool HasSavedGame()
    {
        GameSettings.LoadGame();
        return GameSettings.IsGameIncomplete;
    }

    private void PlayClickSound()
    {
        if (clickSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(clickSound);
    }
    #endregion
}
