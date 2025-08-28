using UnityEngine;
using UnityEngine.UI;

public class SFXSlider : MonoBehaviour
{
    #region Variables
    public Slider sfxSlider;
    public Image sfxIcon;
    public Sprite sfxOnIcon;
    public Sprite sfxOffIcon;
    #endregion

    #region Unity Methods
    void Start()
    {
        if (SoundManager.Instance != null)
        {
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
        }

        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        UpdateIcon(sfxSlider.value);
    }
    #endregion

    #region Private Methods
    void OnSFXChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }

        UpdateIcon(value);
    }

    void UpdateIcon(float value)
    {
        if (sfxIcon == null) return;

        if (value <= 0.001f)
            sfxIcon.sprite = sfxOffIcon;
        else
            sfxIcon.sprite = sfxOnIcon;
    }
    #endregion
}
