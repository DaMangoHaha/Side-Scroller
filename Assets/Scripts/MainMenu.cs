using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider musicSlider;
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

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
    }
}

