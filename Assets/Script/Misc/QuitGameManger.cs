using UnityEngine;

public class QuitGameManager : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit requested");
        Application.Quit();
    }
}
