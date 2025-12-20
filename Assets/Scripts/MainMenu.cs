using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider musicSlider;
    public Slider sfxSlider;
    private void Start()
    {
        LoadVolume();
        MusicManager.Instance.PlayMusic("Main Menu");
    }
    // Called by Play Button
    public void PlayGame()
    {
        // Load the next scene (Level Select)
        SceneManager.LoadScene("LevelSelect");
    }

    // Called by Settings Button
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    // Called by Shop Button
    public void OpenCredits()
    {
        SceneManager.LoadScene("Credits");
        // Credits scene for appropriate acknowledgments for assests used in game
    }

    // Called by Quit Button (if added later)
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }


    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}

