using UnityEngine;

public class ExitManager : MonoBehaviour
{
    public GameObject exitConfirmPanel;
    public SceneLoader sceneLoader;

    private bool isPaused = false;

    void Start()
    {
        exitConfirmPanel.SetActive(false);
    }

    public void OnExitButtonPressed()
    {
        isPaused = true;
        Time.timeScale = 0f;
        exitConfirmPanel.SetActive(true);
    }

    public void OnConfirmExit()
    {
        Time.timeScale = 1f;
        isPaused = false;
        sceneLoader.LoadSceneByName("GameSelect");
    }

    public void OnCancelExit()
    {
        exitConfirmPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
