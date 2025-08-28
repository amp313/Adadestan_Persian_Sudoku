using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    #region UI References
    [Header("UI References")]
    public Slider volumeSlider;
    public Image volumeIcon;
    #endregion

    #region Icons
    [Header("Icons")]
    public Sprite volumeOnIcon;
    public Sprite volumeOffIcon;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        if (MusicPlayer.Instance != null)
        {
            volumeSlider.value = MusicPlayer.Instance.GetVolume();
        }

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        UpdateIcon(volumeSlider.value);
    }
    #endregion

    #region Volume Handling
    void OnVolumeChanged(float value)
    {
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetVolume(value);

        UpdateIcon(value);
    }

    void UpdateIcon(float value)
    {
        if (volumeIcon == null) return;

        volumeIcon.sprite = (value <= 0.001f) ? volumeOffIcon : volumeOnIcon;
    }
    #endregion
}
