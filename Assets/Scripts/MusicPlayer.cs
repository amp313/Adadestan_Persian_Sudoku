using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    #region Fields
    private static MusicPlayer instance;
    private AudioSource audioSource;

    public AudioClip[] musicClips;
    private int currentTrackIndex = 0;
    #endregion

    #region Properties
    public static MusicPlayer Instance
    {
        get { return instance; }
    }
    #endregion

    #region Unity Methods
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        currentTrackIndex = PlayerPrefs.GetInt("SelectedTrack", 0);

        audioSource.volume = savedVolume;

        if (musicClips.Length > 0 && currentTrackIndex < musicClips.Length)
        {
            audioSource.clip = musicClips[currentTrackIndex];
        }

        if (!audioSource.isPlaying)
            audioSource.Play();
    }
    #endregion

    #region Public Methods
    public void SetVolume(float value)
    {
        audioSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }

    public void SetTrack(int index)
    {
        if (index >= 0 && index < musicClips.Length)
        {
            currentTrackIndex = index;
            audioSource.clip = musicClips[index];
            audioSource.Play();
            PlayerPrefs.SetInt("SelectedTrack", index);
        }
    }

    public int GetCurrentTrack()
    {
        return currentTrackIndex;
    }
    #endregion
}
