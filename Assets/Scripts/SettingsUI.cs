using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsUI : MonoBehaviour
{
    #region UI References
    [Header("UI References")]
    public Slider volumeSlider;
    public Toggle[] musicToggles;
    public Button mainMenuButton;
    #endregion

    #region Sounds
    [Header("Sounds")]
    public AudioClip clickSound;
    #endregion

    #region Unity Methods
    void Start()
    {
        if (MusicPlayer.Instance != null)
            volumeSlider.value = MusicPlayer.Instance.GetVolume();

        volumeSlider.onValueChanged.AddListener((value) =>
        {
            if (MusicPlayer.Instance != null)
                MusicPlayer.Instance.SetVolume(value);
        });

        if (MusicPlayer.Instance != null)
        {
            int currentTrack = MusicPlayer.Instance.GetCurrentTrack();

            if (currentTrack < musicToggles.Length)
                musicToggles[currentTrack].isOn = true;
        }

        for (int i = 0; i < musicToggles.Length; i++)
        {
            int index = i;
            musicToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn && MusicPlayer.Instance != null)
                    MusicPlayer.Instance.SetTrack(index);
            });
        }
    }
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

    public void BackToMainMenu()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneDelayed("main_menu", 0.5f));
    }
}
