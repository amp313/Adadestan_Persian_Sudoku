using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region Singleton
    public static SoundManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
    #endregion

    #region Fields
    private float sfxVolume = 1f;
    #endregion

    #region Public Methods
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Vector3.zero, sfxVolume);
    }
    #endregion
}
