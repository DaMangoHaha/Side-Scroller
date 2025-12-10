using UnityEngine;

public class CharacterInfoUI : MonoBehaviour
{
    public GameObject infoPanel;   // The CharacterInfo Canvas or Panel

    private void Start()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);  // Hide at start
    }

    public void ShowInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    public void HideInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
}
