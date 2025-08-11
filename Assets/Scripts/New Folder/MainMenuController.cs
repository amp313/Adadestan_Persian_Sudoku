using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject[] buttons;

    [Header("Sound")]
    public AudioClip clickSound;
    public AudioSource audioSource;

    private float delay = 0.5f;
    private float fadeDuration = 0.5f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        PrepareButtons();
        StartCoroutine(ShowButtonsSequentially());
    }

    private void PrepareButtons()
    {
        foreach (var btn in buttons)
        {
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
        foreach (var btn in buttons)
        {
            yield return new WaitForSeconds(delay);

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
        foreach (var btn in buttons)
        {
            var cg = btn.GetComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound);
    }

    private IEnumerator LoadSceneDelayed(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    public void StartGame()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneDelayed("level_game", 0.5f));
    }
    public void OpenSettings()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneDelayed("settings_menu",0.3f));
    }
    public void OpenHelp()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneDelayed("help_game", 0.3f));
    }
    public void QuitGame()
    {
        PlayClickSound();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
