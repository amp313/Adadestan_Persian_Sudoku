using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpUI : MonoBehaviour
{
    #region Variables
    [Header("Sounds")]
    public AudioClip clickSound;
    #endregion

    #region Private Methods
    private void PlayClickSound()
    {
        if (clickSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(clickSound);
    }

    private IEnumerator LoadSceneDelayed(string sceneName, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        SceneManager.LoadScene(sceneName);
    }
    #endregion

    #region Public Methods
    public void BackToMainMenu()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneDelayed("main_menu", 0.5f));
    }
    #endregion
}
