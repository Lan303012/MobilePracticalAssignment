using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        SceneTransitionManager.Instance.StartSceneTransition(sceneName);
    }
}
