using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public CanvasGroup dialogueCanvasGroup; // For fade effect (optional)


    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    private string fullText;

    public bool IsDialogueActive => dialogueActive;
    public bool IsTyping => isTyping;

    void Start()
    {
        dialogueBox.SetActive(false);
        if (portraitImage != null)
            portraitImage.enabled = false;

        // If using fade effect, make sure the group starts invisible
        if (dialogueCanvasGroup != null)
            dialogueCanvasGroup.alpha = 0;
    }

    void Update()
    {
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            HideDialogue();
        }
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
            StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, 0.3f)); // fade in
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;           // store the full line
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
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
    public void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        dialogueText.text = fullText; // show entire line immediately
    }

}
