using UnityEngine;

public class BackButton : MonoBehaviour
{
    public void MainMenu()
    {
        SceneTransition.Instance.LoadScene("MainMenu");
    }
}
