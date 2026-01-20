using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpeningCutscene : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public CanvasGroup dialogueCanvasGroup;
    public Image portraitImage;

    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    [Header("Continue Prompt")]
    public TextMeshProUGUI continueText;
    public TextMeshProUGUI skipText;
    public float continueFadeSpeed = 2f;
    public float skipFadeSpeed = 2f;
    private bool showContinuePrompt = false;
    private bool showSkipPrompt = false;

    [Header("Start Settings")]
    public bool showOnStart = true;
    [TextArea] public string startingText;
    public Sprite startingPortrait;

    private bool dialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string fullText;

    // Prevent repeatedly attempting to load the main menu every frame
    private bool mainMenuLoaded = false;

    public bool IsDialogueActive => dialogueActive;
    public bool IsTyping => isTyping;
    public void SetContinuePromptVisible(bool visible)
    {
        showContinuePrompt = visible;
        if (!visible && continueText != null)
            continueText.alpha = 0; // hide when not used
    }

    public void SetSkipPromptVisible(bool visible)
    {
        showSkipPrompt = visible;
        if (!visible && skipText != null)
            skipText.alpha = 0;
    }


    void Start()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (portraitImage != null)
            portraitImage.enabled = false;

        if (dialogueCanvasGroup != null)
            dialogueCanvasGroup.alpha = 0;

        if (continueText != null)
            continueText.alpha = 0; // hide at start

        if (skipText != null)
            skipText.alpha = 0;

        // Show dialogue on start if requested. Use startingText if provided,
        // otherwise use the existing dialogueText.text (if any).
        if (showOnStart)
        {
            string textToShow = !string.IsNullOrEmpty(startingText)
                ? startingText
                : (dialogueText != null && !string.IsNullOrEmpty(dialogueText.text) ? dialogueText.text : null);

            if (!string.IsNullOrEmpty(textToShow))
                ShowDialogue(textToShow, startingPortrait);
        }
    }

    private void Update()
    {
        // If the dialogue box object exists and is not active in the scene,
        // load the MainMenu scene once (guarded by mainMenuLoaded).
        if (!mainMenuLoaded && dialogueBox != null && !dialogueBox.activeInHierarchy)
        {
            mainMenuLoaded = true;
            // Make sure "MainMenu" is added to Build Settings -> Scenes In Build
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ShowDialogue(string text, Sprite portrait = null)
    {
        dialogueActive = true;
        if (dialogueBox != null)
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
        if (dialogueText != null)
            dialogueText.text = "";

        // Hide "Press ..." while typing
        if (continueText != null)
            continueText.alpha = 0;

        if (skipText != null)
            skipText.alpha = 0;

        if (dialogueText != null)
        {
            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        else
        {
            // fallback: wait the approximate time so timing remains consistent
            yield return new WaitForSeconds(text.Length * typingSpeed);
        }

        isTyping = false;

        // Only fade in if this dialogue is allowed to show the prompt
        if (showContinuePrompt && continueText != null)
            StartCoroutine(FadeInContinueText());

        if (showSkipPrompt && skipText != null)
            StartCoroutine(FadeInSkipText());
    }


    IEnumerator FadeInContinueText()
    {
        while (continueText != null && continueText.alpha < 1)
        {
            continueText.alpha += Time.deltaTime * continueFadeSpeed;
            yield return null;
        }
    }

    IEnumerator FadeInSkipText()
    {
        while (skipText != null && skipText.alpha < 1)
        {
            skipText.alpha += Time.deltaTime * continueFadeSpeed;
            yield return null;
        }
    }

    public void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        if (dialogueText != null)
            dialogueText.text = fullText;

        // Only show immediately if allowed
        if (showContinuePrompt && continueText != null)
            continueText.alpha = 1;

        if (showSkipPrompt && skipText != null)
            skipText.alpha = 1;
    }


    public void HideDialogue()
    {
        dialogueActive = false;

        if (dialogueCanvasGroup != null)
            StartCoroutine(FadeOutAndDisable());
        else if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (portraitImage != null)
            portraitImage.enabled = false;

        if (continueText != null)
            continueText.alpha = 0; // Hide prompt

        if (skipText != null)
            skipText.alpha = 0;
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, 0.3f);
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null)
            yield break;

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
