using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NumberRecognitionManager : MonoBehaviour
{
    public Button[] optionButtons;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI numberText;

    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip tickSound;
    public AudioClip startSound;
    public AudioClip warningSound;

    public AudioSource audioSource;

    public GameObject titlePanel;
    public CanvasGroup titleCanvasGroup;
    public TextMeshProUGUI countdownText;
    private int currentLevel = 1;
    private int totalLevels = 10;
    private float remainingTime;
    private int displayTimeSnapshot;
    private int wrongAttempts = 0;
    private bool gameActive = false;
    public GameObject gameOverPanel;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI statsText;
    private bool canAnswer = true;
    public RectTransform shakeTarget;
    private Vector3 originalShakePos;
    private float initialTime;
    private bool warningPlayed = false;


    private int correctNumber;
    private Dictionary<int, string> numberWords = new Dictionary<int, string>()
    {
        {1, "one"}, {2, "two"}, {3, "three"}, {4, "four"}, {5, "five"},
        {6, "six"}, {7, "seven"}, {8, "eight"}, {9, "nine"}, {10, "ten"},
        {11, "eleven"}, {12, "twelve"}, {13, "thirteen"}, {14, "fourteen"},
        {15, "fifteen"}, {16, "sixteen"}, {17, "seventeen"}, {18, "eighteen"},
        {19, "nineteen"}, {20, "twenty"}
    };

    void Start()
    {
        currentLevel = 1;
        UpdateLevelUI();
        wrongAttempts = 0;
        canAnswer = true;
        gameActive = false;

        audioSource = GetComponent<AudioSource>();
        countdownText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);

        initialTime = ModeManager.SelectedDifficulty == "Easy" ? 60f : 120f;
        remainingTime = initialTime;
        UpdateTimerUI();

        originalShakePos = shakeTarget.localPosition;

        StartCoroutine(ShowTitleThenCountdown());
    }

    void Update()
    {
        if (gameActive)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                EndGame(false);
            }

            if (remainingTime <= 30f && !warningPlayed)
            {
                timerText.color = Color.red;
                audioSource.PlayOneShot(warningSound);
                warningPlayed = true;
            }
        }
    }


    void WinGame()
    {
        EndGame(true);
    }

    void EndGame(bool win)
    {
        gameActive = false;
        canAnswer = false;
        gameOverPanel.SetActive(true);

        int uiRemainingTime = displayTimeSnapshot;
        float timeUsed = initialTime - uiRemainingTime;

        if (win)
        {
            statusText.text = "You Win!";
            statusText.color = Color.green;
        }
        else
        {
            statusText.text = "Time's Up!";
            statusText.color = Color.red;
        }

        statsText.text = "Time Used: " + Mathf.CeilToInt(timeUsed) + "s" +
                        "\n\nWrong Attempts: " + wrongAttempts;
    }


    void UpdateTimerUI()
    {
        int displayTime;

        if (gameActive)
        {
            displayTime = Mathf.CeilToInt(remainingTime);
            displayTimeSnapshot = displayTime; 
        }
        else
        {
            displayTime = displayTimeSnapshot;
        }

        timerText.text = displayTime + "s";

        if (displayTime > 30)
        {
            timerText.color = Color.green;
        }
        else
        {
            timerText.color = Color.red;
        }
    }


    void UpdateLevelUI()
    {
        if (currentLevel > totalLevels)
            levelText.text = totalLevels + "/" + totalLevels;
        else
            levelText.text = currentLevel + "/" + totalLevels;
    }

    IEnumerator ShowTitleThenCountdown()
    {
        titlePanel.SetActive(true);
        titleCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(2f);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            titleCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        titlePanel.SetActive(false);

        countdownText.gameObject.SetActive(true);

        yield return StartCoroutine(CountdownSequence());

        countdownText.gameObject.SetActive(false);
        gameActive = true;
        GenerateQuestion();
    }

    IEnumerator CountdownSequence()
    {
        string[] countdown = { "3", "2", "1", "Start!" };

        foreach (string text in countdown)
        {
            countdownText.text = text;

            if (text == "Start!")
            {
                countdownText.color = Color.yellow;
                audioSource.PlayOneShot(startSound);
                yield return StartCoroutine(ScaleText(countdownText, 1.8f, 0.5f));
                yield return StartCoroutine(FlashText(countdownText));
            }
            else
            {
                countdownText.color = Color.white;
                audioSource.PlayOneShot(tickSound);
                yield return StartCoroutine(ScaleText(countdownText, 1.5f, 0.5f));
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void GenerateQuestion()
    {
        int maxNumber = ModeManager.SelectedDifficulty == "Easy" ? 10 : 20;
        correctNumber = Random.Range(1, maxNumber + 1);
        numberText.text = correctNumber.ToString();

        GenerateOptions(maxNumber);
        feedbackText.text = "";
        canAnswer = true;
    }

    void GenerateOptions(int maxNumber)
    {
        List<string> options = new List<string> { numberWords[correctNumber] };
        while (options.Count < 3)
        {
            int wrongNum = Random.Range(1, maxNumber + 1);
            string wrongWord = numberWords[wrongNum];
            if (!options.Contains(wrongWord))
                options.Add(wrongWord);
        }

        options.Sort((a, b) => Random.Range(-1, 2));

        for (int i = 0; i < optionButtons.Length; i++)
        {
            string word = options[i];
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = word;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(word));
        }
    }

    void CheckAnswer(string selectedWord)
    {
        if (!gameActive || !canAnswer) return;

        canAnswer = false;

        if (selectedWord == numberWords[correctNumber])
        {
            feedbackText.text = "Correct!";
            feedbackText.color = Color.green;
            audioSource.PlayOneShot(correctSound);

            if (currentLevel == totalLevels)
            {
                UpdateLevelUI();
                Invoke("WinGame", 1.5f);
            }
            else
            {
                currentLevel++;
                UpdateLevelUI();
                Invoke("GenerateQuestion", 1.5f);
            }
        }
        else
        {
            feedbackText.text = "Try Again!";
            feedbackText.color = Color.red;
            audioSource.PlayOneShot(wrongSound);
            wrongAttempts++;
            StartCoroutine(ShakeObject());
            StartCoroutine(EnableAnswerAfterDelay(0.5f));
        }
    }

    IEnumerator EnableAnswerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canAnswer = true;
    }

    IEnumerator ShakeObject()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float magnitude = 10f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            shakeTarget.localPosition = originalShakePos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeTarget.localPosition = originalShakePos;
    }

    IEnumerator ScaleText(TextMeshProUGUI textObj, float maxScale, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * maxScale;

        while (elapsed < duration / 2f)
        {
            float t = elapsed / (duration / 2f);
            textObj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textObj.transform.localScale = targetScale;
        elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            float t = elapsed / (duration / 2f);
            textObj.transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textObj.transform.localScale = startScale;
    }

    IEnumerator FlashText(TextMeshProUGUI textObj)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Color startColor = textObj.color;
        Color flashColor = Color.white;

        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 3f, 1f);
            textObj.color = Color.Lerp(startColor, flashColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textObj.color = startColor;
    }
}
