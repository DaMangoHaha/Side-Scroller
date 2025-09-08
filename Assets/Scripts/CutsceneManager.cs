using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI cutsceneText;
    public string[] lines;
    public float textSpeed = 0.05f;

    private int index;

    void Start()
    {
        cutsceneText.text = "";
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            cutsceneText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            cutsceneText.text = "";
            StartCoroutine(TypeLine());
        }
        else
        {
            // Finished all lines, go to level
            SceneManager.LoadScene("Level1"); // Your actual game scene
        }
    }
}

