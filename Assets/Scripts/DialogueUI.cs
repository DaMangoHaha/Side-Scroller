using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public CanvasGroup dialogueCanvasGroup;

    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    [Header("Continue Prompt")]
    public TextMeshProUGUI continueText;
    public float continueFadeSpeed = 2f;
    private bool showContinuePrompt = false;   // 👈 NEW FLAG


    private bool dialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string fullText;

    public bool IsDialogueActive => dialogueActive;
    public bool IsTyping => isTyping;
    public void SetContinuePromptVisible(bool visible)
    {
        showContinuePrompt = visible;
        if (!visible && continueText != null)
            continueText.alpha = 0; // hide when not used
    }


    void Start()
    {
        dialogueBox.SetActive(false);

        if (portraitImage != null)
            portraitImage.enabled = false;

        if (dialogueCanvasGroup != null)
            dialogueCanvasGroup.alpha = 0;

        if (continueText != null)
            continueText.alpha = 0; // hide at start
    }

    public void ShowDialogue(string text, Sprite portrait = null)
    {
        dialogueActive = true;
        dialogueBox.SetActive(true);

        if (portraitImage != null)
        {
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));

        if (dialogueCanvasGroup != null)
            StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, 0.3f));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;
        dialogueText.text = "";

        // Hide "Press ..." while typing
        if (continueText != null)
            continueText.alpha = 0;

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Only fade in if this dialogue is allowed to show the prompt
        if (showContinuePrompt && continueText != null)
            StartCoroutine(FadeInContinueText());
    }


    IEnumerator FadeInContinueText()
    {
        while (continueText.alpha < 1)
        {
            continueText.alpha += Time.deltaTime * continueFadeSpeed;
            yield return null;
        }
    }

    public void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        dialogueText.text = fullText;

        // Only show immediately if allowed
        if (showContinuePrompt && continueText != null)
            continueText.alpha = 1;
    }


    public void HideDialogue()
    {
        dialogueActive = false;

        if (dialogueCanvasGroup != null)
            StartCoroutine(FadeOutAndDisable());
        else
            dialogueBox.SetActive(false);

        if (portraitImage != null)
            portraitImage.enabled = false;

        if (continueText != null)
            continueText.alpha = 0; // Hide prompt
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, 0.3f);
        dialogueBox.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        cg.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        cg.alpha = end;
    }
}
