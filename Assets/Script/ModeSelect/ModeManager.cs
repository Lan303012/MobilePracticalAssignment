using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeManager : MonoBehaviour
{
    public SceneLoader sceneLoader;
    public GameObject modePanel;
    public GameObject difficultyPanel;

    public Animator modeAnimator;
    public Animator difficultyAnimator;

    private string selectedMode = "";
    private string selectedDifficulty = "";

    public static string SelectedMode { get; private set; }
    public static string SelectedDifficulty { get; private set; }

    public void OnModeSelected(string modeName)
    {
        selectedMode = modeName;
        modeAnimator.SetTrigger("FadeOut");
    }

    public void OnFadeOutFinished()
    {
        modePanel.SetActive(false);
        difficultyPanel.SetActive(true);
        CanvasGroup cg = difficultyPanel.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 0f;

        difficultyAnimator.SetTrigger("FadeIn");
    }

    public void OnDifficultySelected(string difficulty)
    {
        selectedDifficulty = difficulty;

        // Store globally
        SelectedMode = selectedMode;
        SelectedDifficulty = selectedDifficulty;

        // Load corresponding scene
        switch (selectedMode)
        {
            case "Counting":
                sceneLoader.LoadSceneByName("CountingScene");
                break;
            case "NumberRecognition":
                sceneLoader.LoadSceneByName("NumberRecognition");
                break;
            case "MissingNumber":
                sceneLoader.LoadSceneByName("MissingNumber");
                break;
            default:
                Debug.LogError("Unknown mode selected");
                break;
        }
    }

    public void OnBackToModeSelected()
    {
        difficultyAnimator.SetTrigger("FadeOutBack");
    }

    public void OnFadeOutBackFinished()
    {
        difficultyPanel.SetActive(false);
        modePanel.SetActive(true);
        modeAnimator.SetTrigger("FadeIn");
    }

}
