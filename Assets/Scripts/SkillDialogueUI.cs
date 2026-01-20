using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Settings")]
    public float displayDuration = 2.5f;
    public float fadeSpeed = 6f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void ShowSkillDialogue(string text, Sprite portrait)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        gameObject.SetActive(true);
        dialogueText.text = text;
        portraitImage.sprite = portrait;

        currentRoutine = StartCoroutine(DialogueRoutine());
    }

    private IEnumerator DialogueRoutine()
    {
        //Fade in
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
        }
        yield return new WaitForSeconds(displayDuration);

        //Fade out
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
