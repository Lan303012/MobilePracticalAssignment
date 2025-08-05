using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource audioSource;
    public AudioClip defaultMusic;
    public string[] scenesWithContinuousMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource.clip = defaultMusic;
            audioSource.loop = true;
            audioSource.Play();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldKeepMusic = false;

        foreach (string sceneName in scenesWithContinuousMusic)
        {
            if (scene.name == sceneName)
            {
                shouldKeepMusic = true;
                break;
            }
        }

        if (!shouldKeepMusic)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject); 
        }
    }
}
